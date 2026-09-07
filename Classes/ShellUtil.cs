using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PDFLight.Classes;

/// <summary>Zeigt den Windows-Eigenschaften-Dialog einer Datei (portiert aus PDFMover NativeMethods).</summary>
internal static partial class ShellUtil
{
    /// <summary>Registriert die PDF-Verknüpfung (ProgID mit pdffile.ico und Öffnen-Befehl) bei jedem
    /// Start unter HKCU — unabhängig vom Installer-Task. So gilt das neutrale Dateisymbol auch dann,
    /// wenn der Anwender PDFlight erst nachträglich zum Standardprogramm für PDFs macht. Die
    /// Standard-Wahl selbst bleibt unberührt (die trifft seit Windows 10 allein der Benutzer).</summary>
    public static void RegisterFileType()
    {
        try
        {
            // die PDFlight-EXE neben der Programm-Assembly — auch aus Test-Treibern heraus korrekt
            var exe = Path.ChangeExtension(typeof(ShellUtil).Assembly.Location, ".exe");
            if (!File.Exists(exe)) { exe = Application.ExecutablePath; }
            var icon = Path.Combine(Path.GetDirectoryName(exe)!, "pdffile.ico");
            using var progId = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\PDFlight.Document");
            progId.SetValue(null, Lng.T("PDF-Datei"));
            using (var iconKey = progId.CreateSubKey("DefaultIcon"))
            {
                iconKey.SetValue(null, File.Exists(icon) ? icon : exe + ",0"); // ohne ico-Datei (z.B. Debug-Lauf) das EXE-Icon
            }
            using (var command = progId.CreateSubKey(@"shell\open\command"))
            {
                command.SetValue(null, $"\"{exe}\" \"%1\"");
            }
            using (var print = progId.CreateSubKey(@"shell\print\command")) // Explorer-Kontextmenü „Drucken“: ohne Dialog auf den Standarddrucker
            {
                print.SetValue(null, $"\"{exe}\" /print \"%1\"");
            }
            using var openWith = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\.pdf\OpenWithProgids");
            openWith.SetValue("PDFlight.Document", Array.Empty<byte>(), Microsoft.Win32.RegistryValueKind.None);
        }
        catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or IOException)
        {
            // ohne Registrierung läuft das Programm normal weiter — es fehlt nur das Datei-Icon
        }
    }

    /// <summary>Zeigt die Datei im Dateimanager an — in Directory Opus, falls installiert, sonst im Explorer (wie in PDFMover).</summary>
    public static void ShowInFileManager(string filePath)
    {
        var dopus = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"GPSoftware\Directory Opus\dopusrt.exe");
        if (File.Exists(dopus))
        {
            Process.Start(new ProcessStartInfo(dopus, $"/cmd Go \"{filePath}\""));
            BringDopusListerToFront();
        }
        else { Process.Start(new ProcessStartInfo("explorer.exe", $"/e, /select,\"{filePath}\"")); }
    }

    /// <summary>dopusrt reicht den Befehl nur an den laufenden Opus-Prozess weiter — ein bereits offenes
    /// Lister-Fenster bliebe sonst im Hintergrund. Kurz warten, bis ein Lister existiert (bei Bedarf
    /// öffnet Opus erst einen), dann aktivieren; das darf PDFlight, solange es selbst den Fokus hat.</summary>
    private static void BringDopusListerToFront()
    {
        Task.Run(() =>
        {
            for (var i = 0; i < 20; i++)
            {
                var lister = FindWindow("dopus.lister", null);
                if (lister != IntPtr.Zero)
                {
                    if (NativeMethods.IsIconic(lister)) { NativeMethods.ShowWindow(lister, NativeMethods.SW_RESTORE); }
                    NativeMethods.SetForegroundWindow(lister);
                    return;
                }
                Thread.Sleep(100);
            }
        });
    }

    [LibraryImport("user32.dll", EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr FindWindow(string className, string? windowName);

    private const int SW_SHOW = 5;
    private const uint SEE_MASK_INVOKEIDLIST = 12;

    // Blittable Variante (Strings als Zeiger), damit der LibraryImport-Quellgenerator sie marshallen kann
    [StructLayout(LayoutKind.Sequential)]
    private struct SHELLEXECUTEINFO
    {
        public int cbSize;
        public uint fMask;
        public IntPtr hwnd;
        public IntPtr lpVerb;
        public IntPtr lpFile;
        public IntPtr lpParameters;
        public IntPtr lpDirectory;
        public int nShow;
        public IntPtr hInstApp;
        public IntPtr lpIDList;
        public IntPtr lpClass;
        public IntPtr hkeyClass;
        public uint dwHotKey;
        public IntPtr hIcon;
        public IntPtr hProcess;
    }

    [LibraryImport("shell32.dll", EntryPoint = "ShellExecuteExW")] // der Generator macht kein A/W-Probing
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

    // ------------------------------------------------------------------ Papierkorb (Rückgängig nach dem Löschen)

    /// <summary>Sucht direkt nach dem Löschen den Papierkorb-Eintrag der Datei und liefert dessen
    /// eindeutigen Ablagepfad (C:\$Recycle.Bin\…\$R…) — null, wenn keiner existiert (z.B. Netzlaufwerk).
    /// Bei mehreren Einträgen gleichen Namens gewinnt der zuletzt gelöschte (ModifyDate = Löschdatum).</summary>
    public static string? FindRecycledFile(string originalPath)
    {
        try
        {
            if (Type.GetTypeFromProgID("Shell.Application") is not { } shellType) { return null; }
            dynamic shell = Activator.CreateInstance(shellType)!;
            var bin = shell.NameSpace(10); // ssfBITBUCKET = Papierkorb
            var name = Path.GetFileName(originalPath);
            var stem = Path.GetFileNameWithoutExtension(originalPath);
            var folder = Path.GetDirectoryName(originalPath);
            string? best = null;
            var bestDate = DateTime.MinValue;
            foreach (dynamic item in bin.Items())
            {
                string shownName = bin.GetDetailsOf(item, 0); // Anzeigename — je nach Explorer-Einstellung ohne Erweiterung
                if (!string.Equals(shownName, name, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(shownName, stem, StringComparison.OrdinalIgnoreCase)) { continue; }
                if (!string.Equals((string)bin.GetDetailsOf(item, 1), folder, StringComparison.OrdinalIgnoreCase)) { continue; } // Ursprungsordner
                DateTime deleted = item.ModifyDate; // bei Papierkorb-Einträgen das Löschdatum
                if (deleted > bestDate) { bestDate = deleted; best = (string)item.Path; }
            }
            return best;
        }
        catch (Exception ex) when (IsShellComError(ex)) { return null; }
    }

    /// <summary>Stellt den Papierkorb-Eintrag (Pfad aus FindRecycledFile) am Originalpfad wieder her —
    /// primär über das kanonische Shell-Verb „undelete“, sonst über den lokalisierten
    /// Wiederherstellen-Eintrag des Kontextmenüs. True, sobald die Datei wieder existiert.</summary>
    public static bool RestoreRecycledFile(string recycledPath, string originalPath)
    {
        try
        {
            if (Type.GetTypeFromProgID("Shell.Application") is not { } shellType) { return false; }
            dynamic shell = Activator.CreateInstance(shellType)!;
            var bin = shell.NameSpace(10);
            foreach (dynamic item in bin.Items())
            {
                if (!string.Equals((string)item.Path, recycledPath, StringComparison.OrdinalIgnoreCase)) { continue; }
                item.InvokeVerb("undelete");
                if (WaitForFile(originalPath)) { return true; }
                foreach (dynamic verb in item.Verbs()) // Fallback: lokalisierter Menüeintrag (Programmsprachen des OS)
                {
                    var caption = ((string)verb.Name).Replace("&", string.Empty).Trim();
                    if (caption is "Wiederherstellen" or "Restore" or "Restaurer" or "Restaurar")
                    {
                        verb.DoIt();
                        return WaitForFile(originalPath);
                    }
                }
                return false;
            }
        }
        catch (Exception ex) when (IsShellComError(ex)) { }
        return false;
    }

    /// <summary>Die Shell stellt asynchron wieder her — kurz auf das Erscheinen der Datei warten.</summary>
    private static bool WaitForFile(string path)
    {
        for (var i = 0; i < 30 && !File.Exists(path); i++) { Thread.Sleep(100); }
        return File.Exists(path);
    }

    private static bool IsShellComError(Exception ex) =>
        ex is COMException or InvalidOperationException or ArgumentException or NotSupportedException
        or Microsoft.CSharp.RuntimeBinder.RuntimeBinderException;

    public static void ShowFileProperties(string fileName)
    {
        var verb = Marshal.StringToHGlobalUni("properties");
        var file = Marshal.StringToHGlobalUni(fileName);
        try
        {
            SHELLEXECUTEINFO info = new()
            {
                cbSize = Marshal.SizeOf<SHELLEXECUTEINFO>(),
                lpVerb = verb,
                lpFile = file,
                nShow = SW_SHOW,
                fMask = SEE_MASK_INVOKEIDLIST,
            };
            _ = ShellExecuteEx(ref info);
        }
        finally
        {
            Marshal.FreeHGlobal(verb);
            Marshal.FreeHGlobal(file);
        }
    }
}
