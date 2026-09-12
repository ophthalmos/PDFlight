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
    /// <summary>Callback eines WinEvent-Hooks (SetWinEventHook); der Empfänger muss die Delegate-Referenz halten.</summary>
    public delegate void WinEventProc(nint hWinEventHook, uint eventType, nint hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime);

    [LibraryImport("user32.dll")]
    public static partial nint SetWinEventHook(uint eventMin, uint eventMax, nint hmodWinEventProc, WinEventProc callback, uint idProcess, uint idThread, uint flags);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnhookWinEvent(nint hWinEventHook);

    [LibraryImport("user32.dll")]
    public static partial uint GetWindowThreadProcessId(nint hWnd, out uint processId);
}
