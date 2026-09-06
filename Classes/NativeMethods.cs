using System.Runtime.InteropServices;

namespace PDFLight.Classes;

internal static partial class NativeMethods
{
    // Für EM_SETMARGINS (Innenabstand der TextBoxen, s. TextBoxMargins.Apply)
    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    public static partial nint SendMessage(nint hWnd, uint msg, nint wParam, nint lParam);

    // Gibt per Icon-Handle geladene Icons frei (ShellInfo.FreeIcon, ShortcutsPdf)
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyIcon(nint hIcon);

    // Fremde Fenster nach vorn holen: andere PDFlight-Instanz (InstanceRegistry.Activate), Directory-Opus-Lister (ShellUtil)
    public const int SW_RESTORE = 9;

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetForegroundWindow(nint hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool IsIconic(nint hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShowWindow(nint hWnd, int nCmdShow);
}
