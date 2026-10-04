using System.Runtime.InteropServices;

namespace PDFLight.HostCheck;

/// <summary>Fensterfunktionen des Prüfprogramms (den abgeschotteten Start erledigt PDFiumSandbox.Client.SandboxProcess).</summary>
internal static unsafe partial class Native
{
    // ================================================================== Fenster (Prüfung auf Fehlermeldungen des Systems)

    [LibraryImport("user32.dll", EntryPoint = "EnumWindows")] [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnumWindows(delegate* unmanaged<nint, nint, int> callback, nint parameter);
    [LibraryImport("user32.dll", EntryPoint = "IsWindowVisible")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool IsWindowVisible(nint window);
    [LibraryImport("user32.dll", EntryPoint = "GetWindowTextW")] private static partial int GetWindowText(nint window, char* text, int size);

    [ThreadStatic] private static List<(nint Window, string Title)>? windows;

    /// <summary>Sichtbare Fenster, deren Titel <paramref name="part"/> enthält (etwa „pdfhost.exe – Systemfehler“). Verglichen
    /// wird über die Fenster-Handles – schließt jemand zwischendurch eine alte Meldung, zählt das nicht als „weniger“.</summary>
    public static HashSet<nint> VisibleWindows(string part)
    {
        windows = [];
        _ = EnumWindows(&CollectWindow, 0);
        return [.. windows.Where(w => w.Title.Contains(part, StringComparison.OrdinalIgnoreCase)).Select(w => w.Window)];
    }

    [UnmanagedCallersOnly]
    private static int CollectWindow(nint window, nint parameter)
    {
        if (!IsWindowVisible(window)) { return 1; }
        var text = stackalloc char[256];
        var length = GetWindowText(window, text, 256);
        if (length > 0) { windows?.Add((window, new string(text, 0, length))); }
        return 1;
    }
}
