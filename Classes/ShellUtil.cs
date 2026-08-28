using System.Runtime.InteropServices;

namespace PDFLight.Classes;

/// <summary>Zeigt den Windows-Eigenschaften-Dialog einer Datei (portiert aus PDFMover NativeMethods).</summary>
internal static partial class ShellUtil
{
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
