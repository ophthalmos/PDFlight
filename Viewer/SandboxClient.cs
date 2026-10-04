using System.Collections.Concurrent;
using System.Diagnostics;

namespace PDFLight.Viewer;

/// <summary>Die Verbindung zum Hilfsprogramm (PDFium im AppContainer, s. SandboxProcess). Ein Thread mit Warteschlange schickt die
/// Aufträge nacheinander über die Pipe – wie früher der PDFium-Thread, nur dass PDFium jetzt in einem anderen Prozess läuft. Das
/// Hilfsprogramm startet beim ersten Öffnen eines Dokuments. Stürzt es ab, sind alle seine Dokumente verloren: Sie liefern danach nur
/// noch leere Ergebnisse (<see cref="PdfiumDocument"/>), <see cref="Crashed"/> meldet es, und das nächste Öffnen startet ein neues.</summary>
internal static class SandboxClient
{
    private static readonly BlockingCollection<Action> Queue = [];
    private static readonly Thread Thread = new(Run) { IsBackground = true, Name = "pdfhost" };
    private static readonly Dictionary<int, PdfiumDocument> Documents = []; // nur auf dem Sandbox-Thread
    private static SandboxProcess? process;

    /// <summary>Zählt die Starts des Hilfsprogramms – ein Dokument gehört zu genau einem davon.</summary>
    private static int generation;

    /// <summary>Das Hilfsprogramm ist abgestürzt oder hängen geblieben und wurde beendet (Sandbox-Thread).</summary>
    public static event EventHandler<SandboxCrashedException>? Crashed;

    /// <summary>pdfhost.exe im Programmordner (daneben pdfium.dll).</summary>
    public static string HelperPath => Path.Combine(AppContext.BaseDirectory, "pdfhost.exe");

    static SandboxClient()
    {
        Thread.Start();
    }

    private static void Run()
    {
        foreach (var work in Queue.GetConsumingEnumerable()) { work(); }
    }

    /// <summary>Startet das Hilfsprogramm schon jetzt im Hintergrund (spart beim ersten Öffnen ~0,1 s).</summary>
    public static void WarmUp() => _ = RunAsync(_ => true);

    /// <summary>Führt einen Auftrag auf dem Sandbox-Thread aus und startet das Hilfsprogramm, falls nötig (Öffnen eines Dokuments).</summary>
    public static Task<T> RunAsync<T>(Func<SandboxProcess, T> work) => Enqueue(() => work(EnsureProcess()));

    /// <summary>Auftrag für ein Dokument der Startnummer <paramref name="documentGeneration"/>: Läuft das Hilfsprogramm nicht mehr
    /// (abgestürzt) oder ist es ein neueres, gibt es das Dokument dort nicht – dann Ausnahme, ohne neu zu starten.</summary>
    public static Task<T> RunAsync<T>(int documentGeneration, Func<SandboxProcess, T> work) => Enqueue(() =>
    {
        if (process == null || documentGeneration != generation) { throw new InvalidOperationException("Die PDF-Darstellung ist abgestürzt – bitte die Datei neu öffnen."); }
        return work(process);
    });

    /// <summary>Wie <see cref="RunAsync{T}(int, Func{SandboxProcess, T})"/>, aber jeder Fehler ergibt <paramref name="fallback"/> –
    /// für Abfragen der Ansicht (Kacheln, Text, Links …), die nach einem Absturz einfach leer bleiben.</summary>
    public static async Task<T> QueryAsync<T>(int documentGeneration, T fallback, Func<SandboxProcess, T> work)
    {
        try { return await RunAsync(documentGeneration, work); }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine("Sandbox: " + ex.Message);
            return fallback;
        }
    }

    private static Task<T> Enqueue<T>(Func<T> work)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        Queue.Add(() =>
        {
            try { completion.SetResult(work()); }
            catch (SandboxCrashedException ex)
            {
                OnCrashed(ex);
                completion.SetException(ex);
            }
            catch (Exception ex) { completion.SetException(ex); }
        });
        return completion.Task;
    }

    // ================================================================== Prozess

    /// <summary>Startnummer des laufenden Hilfsprogramms (nur auf dem Sandbox-Thread gültig).</summary>
    public static int CurrentGeneration => generation;

    private static SandboxProcess EnsureProcess()
    {
        if (process != null) { return process; }
        try { process = SandboxProcess.Start(HelperPath); }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Das Hilfsprogramm für die PDF-Darstellung konnte nicht gestartet werden ({HelperPath}): {ex.Message}", ex);
        }
        process.EventReceived = OnEvent;
        generation++;
        return process;
    }

    private static void OnCrashed(SandboxCrashedException ex)
    {
        process?.Dispose();
        process = null;
        Documents.Clear();
        try { Crashed?.Invoke(null, ex); }
        catch (Exception handlerError) { Debug.WriteLine(handlerError); }
    }

    // ================================================================== Dokumente und Ereignisse

    /// <summary>Meldet ein Dokument an (Sandbox-Thread) – an es gehen die Formular-Ereignisse mit seiner Nummer.</summary>
    public static void Register(int id, PdfiumDocument document) => Documents[id] = document;

    public static void Unregister(int id) => Documents.Remove(id);

    private static void OnEvent(byte kind, BinaryReader reader)
    {
        switch (kind)
        {
            case Protocol.EventInvalidate:
                var (id, page) = (reader.ReadInt32(), reader.ReadInt32());
                var area = RectangleF.FromLTRB(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                if (Documents.TryGetValue(id, out var document)) { document.RaiseFormInvalidated(page, area); }
                break;
            case Protocol.EventChanged:
                if (Documents.TryGetValue(reader.ReadInt32(), out var changed)) { changed.RaiseChanged(); }
                break;
            default:
                throw new InvalidDataException($"Unbekanntes Ereignis {kind} vom Hilfsprogramm.");
        }
    }
}
