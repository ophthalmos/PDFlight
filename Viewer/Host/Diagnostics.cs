using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace PDFLight.Host;

/// <summary>Für das Prüfprogramm (SandboxCheck): Selbstbericht, Angriffsproben und simulierte Abstürze. Harmlos im Betrieb – was die
/// Proben versuchen, verbietet die Sandbox ohnehin.</summary>
internal static unsafe class Diagnostics
{
    public static string Report()
    {
        var appContainer = false;
        var integrity = "?";
        if (Native.OpenProcessToken(Native.GetCurrentProcess(), Native.TokenQuery, out var token))
        {
            try
            {
                var buffer = stackalloc byte[256];
                if (Native.GetTokenInformation(token, Native.TokenIsAppContainer, (nint)buffer, 256, out _)) { appContainer = *(int*)buffer != 0; }
                if (Native.GetTokenInformation(token, Native.TokenIntegrityLevel, (nint)buffer, 256, out _))
                {
                    var sid = *(nint*)buffer; // TOKEN_MANDATORY_LABEL: SID_AND_ATTRIBUTES, zuerst der SID-Zeiger
                    var count = *(byte*)Native.GetSidSubAuthorityCount(sid);
                    var rid = *(uint*)Native.GetSidSubAuthority(sid, (uint)(count - 1));
                    integrity = rid switch { 0 => "nicht vertrauenswürdig", 0x1000 => "niedrig", 0x2000 => "mittel", 0x3000 => "hoch", _ => $"0x{rid:X}" };
                }
            }
            finally { _ = Native.CloseHandle(token); }
        }
        return $"Prozess {Environment.ProcessId}, AppContainer: {(appContainer ? "ja" : "nein")}, Integritätsstufe: {integrity}, " +
            $"Benutzer: {Environment.UserName}, .NET {Environment.Version}";
    }

    /// <summary>Versucht etwas, das der Sandbox verboten sein soll. Ergebnis beginnt mit „verweigert“ oder „MÖGLICH“.</summary>
    public static string Probe(string kind, string target)
    {
        try
        {
            switch (kind)
            {
                case "read":
                    return $"{Protocol.ProbeAllowed}: {File.ReadAllBytes(target).Length} Byte gelesen";
                case "list":
                    return $"{Protocol.ProbeAllowed}: {Directory.GetFileSystemEntries(target).Length} Einträge";
                case "write":
                    File.WriteAllText(target, "Sandbox-Probe");
                    File.Delete(target);
                    return $"{Protocol.ProbeAllowed}: Datei angelegt und wieder gelöscht";
                case "dns":
                    return $"{Protocol.ProbeAllowed}: {string.Join(", ", Dns.GetHostAddresses(target).Select(a => a.ToString()))}";
                case "net":
                    var parts = target.Split(':');
                    using (var client = new TcpClient())
                    {
                        var connect = client.ConnectAsync(parts[0], int.Parse(parts[1]));
                        return connect.Wait(3000) ? $"{Protocol.ProbeAllowed}: verbunden" : $"{Protocol.ProbeDenied}: keine Verbindung binnen 3 s";
                    }
                case "process":
                    using (var process = Process.Start(new ProcessStartInfo(target) { UseShellExecute = false, CreateNoWindow = true }))
                    {
                        process?.WaitForExit(3000);
                        return $"{Protocol.ProbeAllowed}: Prozess gestartet";
                    }
                case "registry":
                    using (var key = Registry.CurrentUser.CreateSubKey(target, writable: true))
                    {
                        Registry.CurrentUser.DeleteSubKey(target, throwOnMissingSubKey: false);
                        return $"{Protocol.ProbeAllowed}: Schlüssel angelegt und wieder gelöscht";
                    }
                case "nullref": // muss eine normale .NET-Ausnahme bleiben – der Absturz-Handler darf hier nicht zuschlagen
                    var nothing = target.Length > 1000 ? target : null;
                    return $"{Protocol.ProbeAllowed}: {nothing!.Length}";
                case "execmem": // was ein Angreifer bräuchte, um eigenen Maschinencode nachzuladen
                    var memory = Native.VirtualAlloc(0, 4096, Native.MemCommitReserve, Native.PageExecuteReadWrite);
                    if (memory == 0) { return $"{Protocol.ProbeDenied}: VirtualAlloc mit Ausführungsrecht abgewiesen (Fehler {Marshal.GetLastPInvokeError()})"; }
                    _ = Native.VirtualFree(memory, 0, Native.MemRelease);
                    return $"{Protocol.ProbeAllowed}: ausführbarer Speicher angelegt";
                default:
                    return $"unbekannte Probe {kind}";
            }
        }
        catch (Exception ex)
        {
            var inner = ex is AggregateException { InnerException: { } first } ? first : ex;
            return $"{Protocol.ProbeDenied}: {inner.GetType().Name}: {inner.Message}";
        }
    }

    /// <summary>Simuliert einen Speicherfehler – in PDFium (ein ungültiges Dokument-Handle: PDFium liest über die Adresse) oder in
    /// einer Bibliothek, die PDFium aufruft (memset der C-Laufzeit mit kaputtem Zeiger – so enden Pufferüberläufe meist).</summary>
    public static void Crash(byte where)
    {
        var invalid = unchecked((nint)0x0000_7FFF_DEAD_0000);
        if (where == Protocol.CrashInPdfium) { _ = Pdfium.GetPageCount(invalid); }
        else { _ = Native.MemSet(invalid, 0, 16); }
    }
}
