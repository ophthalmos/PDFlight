using System.ComponentModel;
using System.Diagnostics;

namespace PDFLight.Viewer;

/// <summary>Ein Lauf eines Formularskripts: Dokumentskripte, alle Felder im jetzigen Zustand, das Zielfeld und der Wert von event.value.</summary>
internal sealed record ScriptRequest(IReadOnlyList<string> DocumentScripts, IReadOnlyCollection<FormFieldState> Fields, string Target, string Script, string EventValue, bool WillCommit, string Language);

/// <summary>Ergebnis: event.rc, event.value und was das Skript an Feldern geändert hat; <see cref="Failed"/> bei Fehler oder Absturz.</summary>
internal sealed record ScriptResult(bool Rc, string Value, IReadOnlyList<(string Name, string Value)> Writes,
    IReadOnlyList<(string Name, int Flags, int Display)> Properties, IReadOnlyList<string> Alerts, bool Failed = false)
{
    public static readonly ScriptResult Failure = new(true, string.Empty, [], [], [], true);
}

/// <summary>Die Verbindung zu pdfscript.exe, dem Hilfsprogramm für Formularskripte. Es läuft getrennt von pdfhost und startet erst, wenn
/// eine Datei Skripte hat: Stürzt es ab, ist nur diese eine Berechnung verloren, der nächste Lauf startet ein neues. Immer ein normaler
/// AppContainer, auch im Debug-Build – in LPAC scheitert Jint schon beim Start an der Zeitzone (die Registry ist dort gesperrt) –, und ein
/// eigener, nicht der von pdfhost (<see cref="SandboxProcess.ScriptContainerName"/>).</summary>
internal static class ScriptClient
{
    private const int MaxCount = 100_000; // Anzahl in einer Antwort: pdfscript rechnet mit fremden Skripten, seine Antworten gelten nicht als sicher
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static SandboxProcess? process;

    private static string HelperPath => Path.Combine(AppContext.BaseDirectory, "pdfscript.exe");

    public static async Task<ScriptResult> RunAsync(ScriptRequest request)
    {
        await Gate.WaitAsync().ConfigureAwait(false);
        try { return await Task.Run(() => Run(request)).ConfigureAwait(false); }
        finally { Gate.Release(); }
    }

    private static int ReadCount(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        return count is >= 0 and <= MaxCount ? count : throw new InvalidDataException($"Unsinnige Anzahl {count} in der Antwort.");
    }

    private static ScriptResult Run(ScriptRequest request)
    {
        try
        {
            process ??= new Func<SandboxProcess>(() =>
            {
                var started = SandboxProcess.Start(HelperPath, lessPrivileged: false, SandboxProcess.ScriptContainerName);
                started.Timeout = TimeSpan.FromSeconds(5); // Jint bricht selbst nach 2 s ab, dies ist die Reserve für einen Hänger im Interpreter
                return started;
            })();
            return process.Call(Protocol.RunScript, w =>
            {
                w.Write(request.DocumentScripts.Count);
                foreach (var script in request.DocumentScripts) { w.Write(script); }
                w.Write(request.Fields.Count);
                foreach (var field in request.Fields)
                {
                    w.Write(field.Name);
                    w.Write(field.Value);
                    w.Write(field.Type);
                    w.Write(field.Flags);
                    w.Write(field.Display);
                    w.Write(field.Options.Count);
                    foreach (var option in field.Options) { w.Write(option); }
                }
                w.Write(request.Target);
                w.Write(request.Script);
                w.Write(request.EventValue);
                w.Write(request.WillCommit);
                w.Write(request.Language);
            }, r =>
            {
                var (rc, value) = (r.ReadBoolean(), r.ReadString());
                var writes = new (string, string)[ReadCount(r)];
                for (var i = 0; i < writes.Length; i++) { writes[i] = (r.ReadString(), r.ReadString()); }
                var properties = new (string, int, int)[ReadCount(r)];
                for (var i = 0; i < properties.Length; i++) { properties[i] = (r.ReadString(), r.ReadInt32(), r.ReadInt32()); }
                var alerts = new string[ReadCount(r)];
                for (var i = 0; i < alerts.Length; i++) { alerts[i] = r.ReadString(); }
                return new ScriptResult(rc, value, writes, properties, alerts);
            });
        }
        catch (SandboxErrorException ex) // Fehler im Skript: pdfscript läuft weiter
        {
            Debug.WriteLine("Formularskript: " + ex.Message);
            return ScriptResult.Failure;
        }
        catch (Exception ex) // abgestürzt, nicht startbar oder eine unsinnige Antwort (dann stimmt die Pipe nicht mehr): beim nächsten Lauf ein neues
        {
            Debug.WriteLine("pdfscript: " + ex.Message);
            process?.Dispose();
            process = null;
            return ScriptResult.Failure;
        }
    }
}
