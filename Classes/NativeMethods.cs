using System.Runtime.InteropServices;

namespace PDFLight.Classes;

internal static partial class NativeMethods
{
    // Für EM_SETMARGINS (Innenabstand der TextBoxen, s. TextBoxMargins.Apply)
    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    public static partial nint SendMessage(nint hWnd, uint msg, nint wParam, nint lParam);
}
