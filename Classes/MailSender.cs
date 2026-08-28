using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace PDFLight.Classes;

/// <summary>OLE-DropTarget (nur die benötigte VTable; Daten werden als roher IDataObject-Zeiger durchgereicht).
/// Der POINTL-Parameter (8 Bytes by value) wird ABI-gleich als long übergeben — hier stets (0,0).</summary>
[GeneratedComInterface]
[Guid("00000122-0000-0000-C000-000000000046")]
internal partial interface IDropTarget
{
    void DragEnter(IntPtr dataObject, uint keyState, long pt, ref uint effect);
    void DragOver(uint keyState, long pt, ref uint effect);
    void DragLeave();
    void Drop(IntPtr dataObject, uint keyState, long pt, ref uint effect);
}

/// <summary>IShellItem — nur BindToHandler wird benötigt; es steht in der VTable direkt nach IUnknown.</summary>
[GeneratedComInterface]
[Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
internal partial interface IShellItem
{
    void BindToHandler(IntPtr pbc, in Guid bhid, in Guid riid, out IntPtr dataObject);
}

/// <summary>
/// Erstellt eine neue E-Mail mit Dateianhang über das SendMail-DropTarget der Windows-Shell —
/// dasselbe Ziel wie "Senden an → E-Mail-Empfänger". Dieser Weg (aus SumatraPDF bekannt, in
/// PDFMover bewährt) respektiert die ".mapimail"-Zuordnung der Windows-Einstellungen und
/// funktioniert damit auch mit eM Client, Thunderbird &amp; Co. Muss auf dem UI-Thread (STA) laufen.
/// </summary>
internal static partial class MailSender
{
    private const uint MK_LBUTTON = 0x0001;
    private const uint CLSCTX_INPROC_SERVER = 0x1;
    private static readonly Guid CLSID_SendMail = new("9E56BE60-C50F-11CF-9A2C-00A0C90A90CE");
    private static readonly Guid IID_IDropTarget = new("00000122-0000-0000-C000-000000000046");
    private static readonly Guid IID_IShellItem = new("43826d1e-e718-42ee-bc55-a1e261c37bfe");
    private static readonly Guid BHID_DataObject = new("B8C0BD9F-ED24-455C-83E6-D5390C4FE8C4");
    private static readonly Guid IID_IDataObject = new("0000010e-0000-0000-C000-000000000046");

    [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int SHCreateItemFromParsingName(string pszPath, IntPtr pbc, in Guid riid, out IShellItem shellItem);

    [LibraryImport("ole32.dll")]
    private static partial int CoCreateInstance(in Guid rclsid, IntPtr pUnkOuter, uint dwClsContext, in Guid riid, out IDropTarget dropTarget);

    /// <summary>Öffnet das "Neue E-Mail"-Fenster des .mapimail-Handlers mit der Datei als Anhang; wirft bei Fehlern.</summary>
    public static void SendViaDropTarget(string filePath)
    {
        Marshal.ThrowExceptionForHR(SHCreateItemFromParsingName(filePath, IntPtr.Zero, IID_IShellItem, out var shellItem));
        var dataObject = IntPtr.Zero;
        try
        {
            shellItem.BindToHandler(IntPtr.Zero, BHID_DataObject, IID_IDataObject, out dataObject);
            Marshal.ThrowExceptionForHR(CoCreateInstance(CLSID_SendMail, IntPtr.Zero, CLSCTX_INPROC_SERVER, IID_IDropTarget, out var dropTarget));
            uint effect = 1; // DROPEFFECT_COPY
            dropTarget.DragEnter(dataObject, MK_LBUTTON, 0L, ref effect);
            dropTarget.Drop(dataObject, MK_LBUTTON, 0L, ref effect);
        }
        finally
        {
            if (dataObject != IntPtr.Zero) { Marshal.Release(dataObject); }
        }
    }

    /// <summary>Alle Ausnahmen, die der Shell-Weg realistisch wirft — dann lohnt der MAPI-Fallback.</summary>
    public static bool IsComFailure(Exception ex)
    {
        return ex is COMException or InvalidCastException or FileNotFoundException
            or ArgumentException or NotSupportedException or MarshalDirectiveException;
    }
}
