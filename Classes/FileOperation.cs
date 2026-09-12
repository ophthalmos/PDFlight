using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace PDFLight.Classes;

/// <summary>IFileOperation der Windows-Shell (vollständige VTable in Originalreihenfolge; nicht benötigte
/// Schnittstellen-Parameter als rohe Zeiger). PDFlight nutzt nur DeleteItem für den Papierkorb.</summary>
[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("947aab5f-0a5c-4c13-b4d6-4bf7836fc9f8")]
internal partial interface IFileOperation
{
    void Advise(IntPtr sink, out uint cookie);
    void Unadvise(uint cookie);
    void SetOperationFlags(uint flags);
    void SetProgressMessage(string message);
    void SetProgressDialog(IntPtr dialog);
    void SetProperties(IntPtr properties);
    void SetOwnerWindow(IntPtr owner);
    void ApplyPropertiesToItem(IShellItem item);
    void ApplyPropertiesToItems(IntPtr items);
    void RenameItem(IShellItem item, string newName, IntPtr sink);
    void RenameItems(IntPtr items, string newName);
    void MoveItem(IShellItem item, IShellItem destination, string? newName, IntPtr sink);
    void MoveItems(IntPtr items, IShellItem destination);
    void CopyItem(IShellItem item, IShellItem destination, string? newName, IntPtr sink);
    void CopyItems(IntPtr items, IShellItem destination);
    void DeleteItem(IShellItem item, IntPtr sink);
    void DeleteItems(IntPtr items);
    void NewItem(IShellItem destination, uint attributes, string name, string? templateName, IntPtr sink);
    void PerformOperations();
    [return: MarshalAs(UnmanagedType.Bool)]
    bool GetAnyOperationsAborted();
}

/// <summary>Papierkorb über IFileOperation – dieselbe Shell-Operation wie im Explorer, ohne den Umweg über
/// Microsoft.VisualBasic.FileIO.</summary>
internal static partial class ShellUtil
{
    private const uint CLSCTX_INPROC_SERVER = 0x1;
    private const uint FOF_SILENT = 0x0004;            // kein Fortschrittsdialog
    private const uint FOF_NOCONFIRMATION = 0x0010;    // keine Rückfrage (die stellt PDFlight selbst, wenn gewünscht)
    private const uint FOF_ALLOWUNDO = 0x0040;         // in den Papierkorb statt endgültig
    private const uint FOF_WANTNUKEWARNING = 0x4000;   // warnen, wenn es doch endgültig wäre (z.B. Netzlaufwerk)
    private const uint FOFX_RECYCLEONDELETE = 0x00080000;
    private const int COPYENGINE_E_USER_CANCELLED = unchecked((int)0x80270000);
    private static readonly Guid CLSID_FileOperation = new("3ad05575-8857-4850-9277-11b85bdb8e09");
    private static readonly Guid IID_IFileOperation = new("947aab5f-0a5c-4c13-b4d6-4bf7836fc9f8");
    private static readonly Guid IID_IShellItem = new("43826d1e-e718-42ee-bc55-a1e261c37bfe");

    [LibraryImport("ole32.dll")]
    private static partial int CoCreateInstance(in Guid rclsid, IntPtr pUnkOuter, uint dwClsContext, in Guid riid, out IFileOperation fileOperation);

    [LibraryImport("shell32.dll", EntryPoint = "SHCreateItemFromParsingName", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int SHCreateItemFromParsingName(string pszPath, IntPtr pbc, in Guid riid, out IShellItem shellItem);

    /// <summary>Verschiebt eine Datei oder einen Ordner in den Papierkorb: ohne Rückfrage und Fortschrittsdialog,
    /// Fehler zeigt die Shell selbst an, und wo kein Papierkorb existiert, warnt sie vor dem endgültigen Löschen.
    /// Wirft OperationCanceledException bei Abbruch durch den Benutzer und IOException bei Fehlern.</summary>
    public static void MoveToRecycleBin(string path, IntPtr ownerWindow)
    {
        try
        {
            Marshal.ThrowExceptionForHR(CoCreateInstance(CLSID_FileOperation, IntPtr.Zero, CLSCTX_INPROC_SERVER, IID_IFileOperation, out var operation));
            Marshal.ThrowExceptionForHR(SHCreateItemFromParsingName(path, IntPtr.Zero, IID_IShellItem, out var item));
            operation.SetOperationFlags(FOF_ALLOWUNDO | FOFX_RECYCLEONDELETE | FOF_NOCONFIRMATION | FOF_SILENT | FOF_WANTNUKEWARNING);
            operation.SetOwnerWindow(ownerWindow);
            operation.DeleteItem(item, IntPtr.Zero);
            operation.PerformOperations();
            if (operation.GetAnyOperationsAborted()) { throw new OperationCanceledException(); }
        }
        catch (COMException ex)
        {
            if (ex.HResult == COPYENGINE_E_USER_CANCELLED) { throw new OperationCanceledException(ex.Message, ex); }
            throw new IOException(ex.Message, ex); // die Shell hat den Fehler bereits angezeigt
        }
    }
}
