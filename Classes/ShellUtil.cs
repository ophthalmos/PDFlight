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
                    if (IsIconic(lister)) { ShowWindow(lister, SW_RESTORE); }
                    SetForegroundWindow(lister);
                    return;
                }
                Thread.Sleep(100);
            }
        });
    }

    private const int SW_RESTORE = 9;

    [LibraryImport("user32.dll", EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr FindWindow(string className, string windowName);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsIconic(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

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
