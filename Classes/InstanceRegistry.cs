using System.Diagnostics;

namespace PDFLight.Classes;

/// <summary>Macht sichtbar, welche Datei die anderen laufenden PDFlight-Instanzen gerade anzeigen, damit
/// dieselbe Datei nicht ungefragt in zwei Fenstern landet (Nachrücken nach dem Löschen, Blättern).
/// Jede Instanz schreibt ihren aktuellen Dateipfad nach %LOCALAPPDATA%\PDFlight\shown-&lt;PID&gt;.txt
/// (leer = keine Datei), löscht die Datei beim Beenden; Reste abgestürzter Instanzen räumt Cleanup beim
/// Start ab — dasselbe Muster wie die Undo-Sicherungen (undo-&lt;PID&gt;.pdf).</summary>
internal static class InstanceRegistry
{
    private static string Folder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PDFlight");
    private static string OwnFile => Path.Combine(Folder, $"shown-{Environment.ProcessId}.txt");

    /// <summary>Meldet die aktuell angezeigte Datei (null = keine) für die anderen Instanzen.</summary>
    public static void Publish(string path)
    {
        try
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(OwnFile, path ?? string.Empty);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { } // dann sehen die anderen nur einen veralteten Stand
    }

    /// <summary>Beim Beenden: die eigene Meldung entfernen.</summary>
    public static void Clear()
    {
        try { File.Delete(OwnFile); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { } // sonst räumt sie der nächste Start ab
    }

    /// <summary>Beim Start: Meldungen beendeter oder abgestürzter Instanzen löschen (erkennbar an der Prozess-ID).</summary>
    public static void Cleanup()
    {
        foreach (var (file, pid) in EnumerateOthers())
        {
            if (ProcessExists(pid)) { continue; }
            try { File.Delete(file); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
    }

    /// <summary>Liefert die Prozess-ID einer anderen laufenden Instanz, die die Datei anzeigt — sonst null.</summary>
    public static int? FindInstanceShowing(string path)
    {
        if (string.IsNullOrEmpty(path)) { return null; }
        foreach (var (file, pid) in EnumerateOthers())
        {
            if (!ProcessExists(pid)) { continue; }
            string shown;
            try { shown = File.ReadAllText(file).Trim(); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { continue; } // gerade im Schreiben — dann gilt sie als frei
            if (string.Equals(shown, path, StringComparison.OrdinalIgnoreCase)) { return pid; }
        }
        return null;
    }

    public static bool IsShownElsewhere(string path) => FindInstanceShowing(path) != null;

    /// <summary>Holt das Hauptfenster der Instanz nach vorn (das darf PDFlight, solange es selbst den Fokus hat).</summary>
    public static bool Activate(int pid)
    {
        try
        {
            using var process = Process.GetProcessById(pid);
            var hwnd = process.MainWindowHandle;
            if (hwnd == IntPtr.Zero) { return false; }
            if (NativeMethods.IsIconic(hwnd)) { NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE); }
            return NativeMethods.SetForegroundWindow(hwnd);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException) { return false; } // Prozess inzwischen beendet
    }

    public static bool ProcessExists(int pid)
    {
        try { using var process = Process.GetProcessById(pid); return true; }
        catch (ArgumentException) { return false; }
    }

    /// <summary>Alle shown-&lt;PID&gt;.txt außer der eigenen.</summary>
    private static IEnumerable<(string File, int Pid)> EnumerateOthers()
    {
        string[] files;
        try { files = Directory.Exists(Folder) ? Directory.GetFiles(Folder, "shown-*.txt") : []; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { yield break; }
        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (int.TryParse(name["shown-".Length..], out var pid) && pid != Environment.ProcessId) { yield return (file, pid); }
        }
    }
}
