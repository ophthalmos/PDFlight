using System.Diagnostics;
using PDFLight.Classes;

namespace PDFLight.Viewer;

/// <summary>Ein Formularfeld mit seinen Skripten (leer = keins): Tastenanschlag, Formatieren, Prüfen, Berechnen.</summary>
internal sealed record FormField(string Name, string Label, string Keystroke, string Format, string Validate, string Calculate)
{
    public bool HasScript => Keystroke.Length + Format.Length + Validate.Length + Calculate.Length > 0;
}

/// <summary>Zustand eines Felds für die Skripte. Type: FPDF_FORMFIELD_*; Flags: /Ff; Display: 0 sichtbar, 1 verborgen, 2 nicht drucken, 3 nicht
/// anzeigen; Shared: mehrere Widgets, s. FormScriptRunner.UseRawValuesAsync; Options: Einträge einer Liste bzw. Ausfuhrwerte von
/// Kontrollkästchen und Optionsfeldern.</summary>
internal sealed record FormFieldState(string Name, string Value, int Type, int Flags, int Display, bool Shared, IReadOnlyList<string> Options);

/// <summary>Eine Eingabe wurde abgelehnt (<see cref="RejectedLabel"/>, das Feld hat wieder seinen alten Wert), ein Skript scheiterte
/// (<see cref="FailedLabel"/>), oder ein Skript hat etwas zu sagen (<see cref="Alert"/>, app.alert).</summary>
internal readonly record struct FormScriptOutcome(string? RejectedLabel = null, string? RejectedValue = null, string? FailedLabel = null, string? Alert = null);

/// <summary>Führt die Skripte eines Dokuments aus, wenn PDFium einen Feldwert übernommen hat, in Acrobats Reihenfolge: das geänderte Feld
/// prüfen (Tastenanschlag mit willCommit, Prüfen), dann alle Berechnungen, zuletzt das Formatieren. Die Skripte laufen in pdfscript.exe
/// (<see cref="ScriptClient"/>), die Ergebnisse schreibt pdfhost in einem Befehl ins Formular. Grenzen dieser Fassung: Die Berechnungen laufen in
/// Feldreihenfolge statt nach /CO (dafür bis zu drei Durchgänge), Tastenanschläge werden nicht schon beim Tippen geprüft.
/// Was pdfscript antwortet, gilt als nicht vertrauenswürdig: Es rechnet mit Skripten aus der Datei.</summary>
internal sealed class FormScriptRunner(PdfiumDocument document)
{
    private const int CalculatePasses = 3, AlertLength = 1000;
    private const int FlagsScriptsMayChange = 1 | 2; // /Ff: schreibgeschützt, Pflichtfeld – andere Bits ändern die Art des Felds

    /// <summary>Gesamtzeit je Eingabe bzw. fürs Formatieren eines Stands: Die Grenze je Skript (2 s) allein ließe eine Datei mit vielen
    /// Endlosschleifen das Speichern minutenlang aufhalten – es wartet auf die Skripte. Normale Formulare brauchen Millisekunden je Skript.</summary>
    private static readonly TimeSpan Budget = TimeSpan.FromSeconds(10);

    private List<FormField>? fields;
    private List<string> documentScripts = [];
    private readonly Dictionary<(string Name, string Shown), string> rawValues = [];
    private readonly Stopwatch formatTime = new();

    /// <summary>Formatiert Felder, die ihren Rohwert zeigen, ohne die Datei als geändert zu markieren. Das Hilfsprogramm lässt Felder aus, die
    /// inzwischen einen anderen Wert oder den Fokus haben. Was ein Formatier-Skript dabei an anderen Feldern ändert, gilt hier nicht: Es hat
    /// niemand etwas eingegeben. Ein leeres Ergebnis auch nicht – die Anzeige soll einen Wert nicht verschwinden lassen.</summary>
    public async Task FormatAsync(IReadOnlyList<(string Name, string Value, string Format)> found)
    {
        if (formatTime.Elapsed > Budget) { return; }
        formatTime.Start();
        List<(string Name, string Value, string Display)> output = [];
        try
        {
            if (fields == null) { (documentScripts, fields) = await document.FormScriptsAsync(); }
            var states = await document.FormValuesAsync();
            foreach (var (name, value, format) in found)
            {
                if (formatTime.Elapsed > Budget) { break; }
                var result = await ScriptClient.RunAsync(new(documentScripts, states.Values, name, format, value, false, Lng.CultureCode));
                if (!result.Failed && result.Value.Length > 0 && result.Value != value) { output.Add((name, value, result.Value)); }
            }
        }
        finally { formatTime.Stop(); }
        if (output.Count > 0) { await document.FormSetValuesAsync(output, displayOnly: true); }
    }

    /// <summary>Felder mit mehreren Widgets (ein Betrag auf zwei Seiten): Ihr Wert steht am Feld, dort kommt PDFiums Schnittstelle nur über das
    /// Formularmodul hin – Wert und Anzeige lassen sich nicht trennen, das Feld enthält „1.234,50 €“. Für die Skripte wird daraus wieder der
    /// Rohwert, über das Tastenanschlag-Skript des Felds, das seine Trennzeichen und seine Währung kennt.</summary>
    private async Task UseRawValuesAsync(Dictionary<string, FormFieldState> states, Func<FormField, string, string, bool, Task<ScriptResult>> run)
    {
        foreach (var state in states.Values.Where(s => s.Shared && s.Value.Length > 0).ToList())
        {
            if (fields?.Find(f => f.Name == state.Name) is not { Format.Length: > 0, Keystroke.Length: > 0 } field) { continue; }
            if (!rawValues.TryGetValue((state.Name, state.Value), out var raw))
            {
                var result = await run(field, field.Keystroke, state.Value, true);
                raw = result is { Failed: false, Rc: true } ? result.Value : state.Value;
                rawValues[(state.Name, state.Value)] = raw;
            }
            states[state.Name] = state with { Value = raw };
        }
    }

    public async Task<FormScriptOutcome> HandleCommitAsync(FieldCommittedEventArgs commit)
    {
        if (fields == null) { (documentScripts, fields) = await document.FormScriptsAsync(); }
        if (!fields.Exists(f => f.HasScript)) { return default; } // der Normalfall: Danach kostet eine Eingabe keinen weiteren Auftrag
        if (commit.Name.Length > 0 && commit.OldValue == commit.NewValue) { return default; }

        var states = await document.FormValuesAsync();
        var writes = new Dictionary<string, string>();
        var properties = new Dictionary<string, (int Flags, int Display)>();
        var changed = new List<string>();
        var elapsed = Stopwatch.StartNew();
        string? failed = null, alert = null;
        string Current(string name) => states.TryGetValue(name, out var state) ? state.Value : string.Empty;
        void Apply(string name, string value)
        {
            if (!states.TryGetValue(name, out var state) || state.Value == value) { return; }
            states[name] = state with { Value = value };
            writes[name] = value;
            if (!changed.Contains(name)) { changed.Add(name); }
        }
        async Task<ScriptResult> RunAsync(FormField field, string script, string value, bool willCommit)
        {
            if (elapsed.Elapsed > Budget) { failed ??= field.Label; return ScriptResult.Failure; }
            var result = await ScriptClient.RunAsync(new(documentScripts, states.Values, field.Name, script, value, willCommit, Lng.CultureCode));
            if (result.Failed) { failed ??= field.Label; return result; }
            foreach (var (name, written) in result.Writes) { Apply(name, written); }
            foreach (var (name, flags, display) in result.Properties)
            {
                if (!states.TryGetValue(name, out var state)) { continue; }
                var allowed = (state.Flags & ~FlagsScriptsMayChange) | (flags & FlagsScriptsMayChange);
                properties[name] = (allowed, display);
                states[name] = state with { Flags = allowed, Display = display };
            }
            if (alert == null && result.Alerts.Count > 0) { alert = result.Alerts[0].Length > AlertLength ? result.Alerts[0][..AlertLength] + "…" : result.Alerts[0]; }
            return result;
        }
        async Task SetPropertiesAsync()
        {
            if (properties.Count > 0) { await document.FormSetPropertiesAsync([.. properties.Select(p => (p.Key, p.Value.Flags, p.Value.Display))]); }
        }

        await UseRawValuesAsync(states, RunAsync);
        if (fields.Find(f => f.Name == commit.Name) is { } committed)
        {
            changed.Add(committed.Name);
            foreach (var (script, willCommit) in new[] { (committed.Keystroke, true), (committed.Validate, false) })
            {
                if (script.Length == 0) { continue; }
                var result = await RunAsync(committed, script, states.TryGetValue(committed.Name, out var state) ? state.Value : commit.NewValue, willCommit);
                if (result.Failed) { continue; }
                if (!result.Rc)
                {
                    var old = rawValues.GetValueOrDefault((committed.Name, commit.OldValue), commit.OldValue);
                    var shown = old;
                    if (committed.Format.Length > 0 && await RunAsync(committed, committed.Format, old, false) is { Failed: false } formatted) { shown = formatted.Value; }
                    await document.FormSetValuesAsync([(committed.Name, old, shown)]);
                    await SetPropertiesAsync();
                    return new(committed.Label, commit.NewValue, Alert: alert);
                }
                Apply(committed.Name, result.Value);
            }
        }

        for (var pass = 0; pass < CalculatePasses; pass++) // ein berechnetes Feld kann von einem anderen abhängen
        {
            var before = changed.Count + writes.Count;
            var again = false;
            foreach (var field in fields.Where(f => f.Calculate.Length > 0))
            {
                var current = Current(field.Name);
                var result = await RunAsync(field, field.Calculate, current, false);
                if (result.Failed || !result.Rc || result.Value == current) { continue; }
                Apply(field.Name, result.Value);
                again = true;
            }
            if (!again && changed.Count + writes.Count == before) { break; }
        }

        // Formatieren ändert wie bei Acrobat nur die Anzeige: Im Feld bleibt der Rohwert (1234.5), damit andere Skripte und Programme damit rechnen können
        var displays = new Dictionary<string, string>();
        foreach (var name in changed.ToArray())
        {
            if (fields.Find(f => f.Name == name) is not { Format.Length: > 0 } field) { continue; }
            var result = await RunAsync(field, field.Format, Current(name), false);
            if (!result.Failed) { displays[name] = result.Value; }
        }

        List<(string Name, string Value, string Display)> output = [];
        foreach (var name in writes.Keys.Union(displays.Keys))
        {
            var value = Current(name);
            var display = displays.GetValueOrDefault(name, value);
            if (writes.ContainsKey(name) || display != value)
            {
                output.Add((name, value, display));
                if (states.TryGetValue(name, out var state) && state.Shared) { rawValues[(name, display)] = value; } // das Feld enthält danach die Anzeige
            }
        }
        if (output.Count > 0) { await document.FormSetValuesAsync(output); }
        await SetPropertiesAsync();
        return new(FailedLabel: failed, Alert: alert);
    }
}
