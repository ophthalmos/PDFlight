using System.Runtime.InteropServices;

namespace PDFLight.Host;

/// <summary>Die PDFium-Funktionen, die das Hilfsprogramm aufruft (fpdfview.h, fpdf_text.h, fpdf_doc.h, fpdf_formfill.h, fpdf_save.h,
/// fpdf_ppo.h, fpdf_flatten.h, fpdf_edit.h). Handles sind nint; Windows-„unsigned long“ ist 32 Bit (uint), FPDF_BOOL ein int,
/// FPDF_WIDESTRING ein nullterminiertes UTF-16 (ushort*).</summary>
internal static unsafe partial class Pdfium
{
    private const string Lib = "pdfium";

    // ================================================================== Dokument und Seiten (fpdfview.h)

    [LibraryImport(Lib, EntryPoint = "FPDF_InitLibrary")] public static partial void InitLibrary();
    [LibraryImport(Lib, EntryPoint = "FPDF_LoadMemDocument64")] public static partial nint LoadMemDocument(nint data, nuint size, nint password);
    [LibraryImport(Lib, EntryPoint = "FPDF_GetLastError")] public static partial uint GetLastError();
    [LibraryImport(Lib, EntryPoint = "FPDF_CloseDocument")] public static partial void CloseDocument(nint document);
    [LibraryImport(Lib, EntryPoint = "FPDF_GetPageCount")] public static partial int GetPageCount(nint document);
    [LibraryImport(Lib, EntryPoint = "FPDF_GetPageSizeByIndex")] public static partial int GetPageSizeByIndex(nint document, int index, out double width, out double height);
    [LibraryImport(Lib, EntryPoint = "FPDF_LoadPage")] public static partial nint LoadPage(nint document, int index);
    [LibraryImport(Lib, EntryPoint = "FPDF_ClosePage")] public static partial void ClosePage(nint page);
    [LibraryImport(Lib, EntryPoint = "FPDF_DeviceToPage")]
    public static partial int DeviceToPage(nint page, int startX, int startY, int sizeX, int sizeY, int rotate, int deviceX, int deviceY, out double pageX, out double pageY);
    [LibraryImport(Lib, EntryPoint = "FPDF_PageToDevice")]
    public static partial int PageToDevice(nint page, int startX, int startY, int sizeX, int sizeY, int rotate, double pageX, double pageY, out int deviceX, out int deviceY);

    // ================================================================== Rendern

    public const int BitmapBgra = 4;            // FPDFBitmap_BGRA – dieselbe Byte-Reihenfolge wie GDI+ Format32bppArgb
    public const int RenderAnnotations = 0x01;  // FPDF_ANNOT
    public const int RenderForPrinting = 0x800; // FPDF_PRINTING

    [LibraryImport(Lib, EntryPoint = "FPDFBitmap_CreateEx")] public static partial nint BitmapCreateEx(int width, int height, int format, nint buffer, int stride);
    [LibraryImport(Lib, EntryPoint = "FPDFBitmap_FillRect")] public static partial int BitmapFillRect(nint bitmap, int left, int top, int width, int height, uint color);
    [LibraryImport(Lib, EntryPoint = "FPDFBitmap_Destroy")] public static partial void BitmapDestroy(nint bitmap);
    [LibraryImport(Lib, EntryPoint = "FPDF_RenderPageBitmap")] public static partial void RenderPageBitmap(nint bitmap, nint page, int startX, int startY, int sizeX, int sizeY, int rotate, int flags);
    [LibraryImport(Lib, EntryPoint = "FPDF_RenderPage")] public static partial void RenderPage(nint dc, nint page, int startX, int startY, int sizeX, int sizeY, int rotate, int flags);

    // ================================================================== Text (fpdf_text.h)

    public const uint SearchMatchCase = 0x1; // FPDF_MATCHCASE
    public const uint SearchWholeWord = 0x2; // FPDF_MATCHWHOLEWORD

    [LibraryImport(Lib, EntryPoint = "FPDFText_LoadPage")] public static partial nint TextLoadPage(nint page);
    [LibraryImport(Lib, EntryPoint = "FPDFText_ClosePage")] public static partial void TextClosePage(nint text);
    [LibraryImport(Lib, EntryPoint = "FPDFText_CountChars")] public static partial int TextCountChars(nint text);
    [LibraryImport(Lib, EntryPoint = "FPDFText_GetUnicode")] public static partial uint TextGetUnicode(nint text, int index);
    [LibraryImport(Lib, EntryPoint = "FPDFText_GetCharBox")] public static partial int TextGetCharBox(nint text, int index, out double left, out double right, out double bottom, out double top);
    [LibraryImport(Lib, EntryPoint = "FPDFText_GetLooseCharBox")] public static partial int TextGetLooseCharBox(nint text, int index, out RectF rect);
    [LibraryImport(Lib, EntryPoint = "FPDFText_GetCharIndexAtPos")] public static partial int TextGetCharIndexAtPos(nint text, double x, double y, double xTolerance, double yTolerance);
    [LibraryImport(Lib, EntryPoint = "FPDFText_CountRects")] public static partial int TextCountRects(nint text, int start, int count);
    [LibraryImport(Lib, EntryPoint = "FPDFText_GetRect")] public static partial int TextGetRect(nint text, int index, out double left, out double top, out double right, out double bottom);
    [LibraryImport(Lib, EntryPoint = "FPDFText_GetText")] public static partial int TextGetText(nint text, int start, int count, ushort* result);
    [LibraryImport(Lib, EntryPoint = "FPDFText_FindStart")] public static partial nint TextFindStart(nint text, ushort* what, uint flags, int startIndex);
    [LibraryImport(Lib, EntryPoint = "FPDFText_FindNext")] public static partial int TextFindNext(nint search);
    [LibraryImport(Lib, EntryPoint = "FPDFText_GetSchResultIndex")] public static partial int TextGetSchResultIndex(nint search);
    [LibraryImport(Lib, EntryPoint = "FPDFText_GetSchCount")] public static partial int TextGetSchCount(nint search);
    [LibraryImport(Lib, EntryPoint = "FPDFText_FindClose")] public static partial void TextFindClose(nint search);

    // Webadressen, die nur als Text auf der Seite stehen
    [LibraryImport(Lib, EntryPoint = "FPDFLink_LoadWebLinks")] public static partial nint LinkLoadWebLinks(nint text);
    [LibraryImport(Lib, EntryPoint = "FPDFLink_CountWebLinks")] public static partial int LinkCountWebLinks(nint links);
    [LibraryImport(Lib, EntryPoint = "FPDFLink_CountRects")] public static partial int LinkCountRects(nint links, int index);
    [LibraryImport(Lib, EntryPoint = "FPDFLink_GetRect")] public static partial int LinkGetRect(nint links, int index, int rect, out double left, out double top, out double right, out double bottom);
    [LibraryImport(Lib, EntryPoint = "FPDFLink_GetURL")] public static partial int LinkGetUrl(nint links, int index, ushort* buffer, int count);
    [LibraryImport(Lib, EntryPoint = "FPDFLink_CloseWebLinks")] public static partial void LinkCloseWebLinks(nint links);

    // ================================================================== Links, Ziele, Lesezeichen (fpdf_doc.h)

    public const uint ActionGoTo = 1, ActionUri = 3; // PDFACTION_GOTO, PDFACTION_URI

    [LibraryImport(Lib, EntryPoint = "FPDFLink_GetLinkAtPoint")] public static partial nint LinkGetLinkAtPoint(nint page, double x, double y);
    [LibraryImport(Lib, EntryPoint = "FPDFLink_GetDest")] public static partial nint LinkGetDest(nint document, nint link);
    [LibraryImport(Lib, EntryPoint = "FPDFLink_GetAction")] public static partial nint LinkGetAction(nint link);
    [LibraryImport(Lib, EntryPoint = "FPDFDest_GetDestPageIndex")] public static partial int DestGetPageIndex(nint document, nint dest);
    [LibraryImport(Lib, EntryPoint = "FPDFDest_GetLocationInPage")]
    public static partial int DestGetLocationInPage(nint dest, out int hasX, out int hasY, out int hasZoom, out float x, out float y, out float zoom);
    [LibraryImport(Lib, EntryPoint = "FPDFAction_GetType")] public static partial uint ActionGetType(nint action);
    [LibraryImport(Lib, EntryPoint = "FPDFAction_GetDest")] public static partial nint ActionGetDest(nint document, nint action);
    [LibraryImport(Lib, EntryPoint = "FPDFAction_GetURIPath")] public static partial uint ActionGetUriPath(nint document, nint action, nint buffer, uint length);
    [LibraryImport(Lib, EntryPoint = "FPDFBookmark_GetFirstChild")] public static partial nint BookmarkGetFirstChild(nint document, nint bookmark);
    [LibraryImport(Lib, EntryPoint = "FPDFBookmark_GetNextSibling")] public static partial nint BookmarkGetNextSibling(nint document, nint bookmark);
    [LibraryImport(Lib, EntryPoint = "FPDFBookmark_GetTitle")] public static partial uint BookmarkGetTitle(nint bookmark, nint buffer, uint length);
    [LibraryImport(Lib, EntryPoint = "FPDFBookmark_GetDest")] public static partial nint BookmarkGetDest(nint document, nint bookmark);
    [LibraryImport(Lib, EntryPoint = "FPDFBookmark_GetAction")] public static partial nint BookmarkGetAction(nint bookmark);

    // ================================================================== Formulare (fpdf_formfill.h)

    public const int ModLeftButton = 1 << 6;                // FWL_EVENTFLAG_LeftButtonDown
    [LibraryImport(Lib, EntryPoint = "FPDF_GetFormType")] public static partial int GetFormType(nint document);
    [LibraryImport(Lib, EntryPoint = "FPDFDOC_InitFormFillEnvironment")] public static partial nint InitFormFillEnvironment(nint document, nint formInfo);
    [LibraryImport(Lib, EntryPoint = "FPDFDOC_ExitFormFillEnvironment")] public static partial void ExitFormFillEnvironment(nint form);
    [LibraryImport(Lib, EntryPoint = "FORM_OnAfterLoadPage")] public static partial void FormOnAfterLoadPage(nint page, nint form);
    [LibraryImport(Lib, EntryPoint = "FORM_OnBeforeClosePage")] public static partial void FormOnBeforeClosePage(nint page, nint form);
    [LibraryImport(Lib, EntryPoint = "FPDF_FFLDraw")] public static partial void FflDraw(nint form, nint bitmap, nint page, int startX, int startY, int sizeX, int sizeY, int rotate, int flags);
    [LibraryImport(Lib, EntryPoint = "FPDF_SetFormFieldHighlightColor")] public static partial void SetFormFieldHighlightColor(nint form, int fieldType, uint color);
    [LibraryImport(Lib, EntryPoint = "FPDF_SetFormFieldHighlightAlpha")] public static partial void SetFormFieldHighlightAlpha(nint form, byte alpha);
    [LibraryImport(Lib, EntryPoint = "FPDFPage_HasFormFieldAtPoint")] public static partial int HasFormFieldAtPoint(nint form, nint page, double x, double y);
    [LibraryImport(Lib, EntryPoint = "FORM_OnMouseMove")] public static partial int FormOnMouseMove(nint form, nint page, int modifier, double x, double y);
    [LibraryImport(Lib, EntryPoint = "FORM_OnLButtonDown")] public static partial int FormOnLButtonDown(nint form, nint page, int modifier, double x, double y);
    [LibraryImport(Lib, EntryPoint = "FORM_OnLButtonUp")] public static partial int FormOnLButtonUp(nint form, nint page, int modifier, double x, double y);
    [LibraryImport(Lib, EntryPoint = "FORM_OnKeyDown")] public static partial int FormOnKeyDown(nint form, nint page, int keyCode, int modifier);
    [LibraryImport(Lib, EntryPoint = "FORM_OnKeyUp")] public static partial int FormOnKeyUp(nint form, nint page, int keyCode, int modifier);
    [LibraryImport(Lib, EntryPoint = "FORM_OnChar")] public static partial int FormOnChar(nint form, nint page, int character, int modifier);
    [LibraryImport(Lib, EntryPoint = "FORM_ForceToKillFocus")] public static partial int FormForceToKillFocus(nint form);
    [LibraryImport(Lib, EntryPoint = "FORM_GetSelectedText")] public static partial uint FormGetSelectedText(nint form, nint page, nint buffer, uint length);
    [LibraryImport(Lib, EntryPoint = "FORM_ReplaceSelection")] public static partial void FormReplaceSelection(nint form, nint page, ushort* text);
    [LibraryImport(Lib, EntryPoint = "FORM_SelectAllText")] public static partial int FormSelectAllText(nint form, nint page);
    [LibraryImport(Lib, EntryPoint = "FORM_Undo")] public static partial int FormUndo(nint form, nint page);
    [LibraryImport(Lib, EntryPoint = "FORM_Redo")] public static partial int FormRedo(nint form, nint page);
    [LibraryImport(Lib, EntryPoint = "FORM_GetFocusedAnnot")] public static partial int FormGetFocusedAnnot(nint form, out int pageIndex, out nint annot);
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_GetRect")] public static partial int AnnotGetRect(nint annot, out RectF rect);
    [LibraryImport(Lib, EntryPoint = "FPDFPage_CloseAnnot")] public static partial void PageCloseAnnot(nint annot);

    [StructLayout(LayoutKind.Sequential)]
    public struct RectF
    {
        public float Left, Top, Right, Bottom;
    }

    // ================================================================== Anmerkungen (fpdf_annot.h)

    public const int AnnotHighlight = 9, AnnotUnderline = 10, AnnotSquiggly = 11, AnnotStrikeOut = 12; // FPDF_ANNOT_*: Textmarkierungen
    public const int AnnotColor = 0;       // FPDFANNOT_COLORTYPE_Color
    public const int AnnotFlagPrint = 4;   // FPDF_ANNOT_FLAG_PRINT: wird mitgedruckt

    [LibraryImport(Lib, EntryPoint = "FPDFPage_CreateAnnot")] public static partial nint PageCreateAnnot(nint page, int subtype);
    [LibraryImport(Lib, EntryPoint = "FPDFPage_GetAnnotCount")] public static partial int PageGetAnnotCount(nint page);
    [LibraryImport(Lib, EntryPoint = "FPDFPage_GetAnnot")] public static partial nint PageGetAnnot(nint page, int index);
    [LibraryImport(Lib, EntryPoint = "FPDFPage_RemoveAnnot")] public static partial int PageRemoveAnnot(nint page, int index);
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_GetSubtype")] public static partial int AnnotGetSubtype(nint annot);
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_SetColor")] public static partial int AnnotSetColor(nint annot, int type, uint r, uint g, uint b, uint a);
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_SetFlags")] public static partial int AnnotSetFlags(nint annot, int flags);
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_GetFlags")] public static partial int AnnotGetFlags(nint annot);
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_AddInkStroke")] public static partial int AnnotAddInkStroke(nint annot, PointF* points, nuint count);
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_SetBorder")] public static partial int AnnotSetBorder(nint annot, float horizontalRadius, float verticalRadius, float width);
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_GetBorder")] public static partial int AnnotGetBorder(nint annot, out float horizontalRadius, out float verticalRadius, out float width);
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_GetInkListCount")] public static partial uint AnnotGetInkListCount(nint annot);
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_GetInkListPath")] public static partial uint AnnotGetInkListPath(nint annot, uint pathIndex, PointF* buffer, uint length);
    [LibraryImport(Lib, EntryPoint = "FPDF_GetPageWidthF")] public static partial float GetPageWidthF(nint page);
    [LibraryImport(Lib, EntryPoint = "FPDF_GetPageHeightF")] public static partial float GetPageHeightF(nint page);
    public const int AnnotInk = 15;        // FPDF_ANNOT_INK
    public const int AnnotFreeText = 3;    // FPDF_ANNOT_FREETEXT
    public const int AnnotStamp = 13;      // FPDF_ANNOT_STAMP
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_GetLinkedAnnot", StringMarshalling = StringMarshalling.Utf8)] public static partial nint AnnotGetLinkedAnnot(nint annot, string key);
    [LibraryImport(Lib, EntryPoint = "FPDFPage_GetAnnotIndex")] public static partial int PageGetAnnotIndex(nint page, nint annot);

    [StructLayout(LayoutKind.Sequential)]
    public struct PointF { public float X, Y; } // FS_POINTF
    public const int AnnotFlagHidden = 2;  // FPDF_ANNOT_FLAG_HIDDEN: nicht anzeigen, nicht drucken
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_SetRect")] public static partial int AnnotSetRect(nint annot, RectF* rect);
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_AppendAttachmentPoints")] public static partial int AnnotAppendAttachmentPoints(nint annot, QuadPointsF* points);
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_CountAttachmentPoints")] public static partial nuint AnnotCountAttachmentPoints(nint annot);
    [LibraryImport(Lib, EntryPoint = "FPDFAnnot_GetAttachmentPoints")] public static partial int AnnotGetAttachmentPoints(nint annot, nuint index, QuadPointsF* points);

    /// <summary>FS_QUADPOINTSF in der Reihenfolge, die Acrobat und PDFium erwarten: oben links, oben rechts, unten links, unten rechts.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct QuadPointsF
    {
        public float X1, Y1, X2, Y2, X3, Y3, X4, Y4;
    }

    // ================================================================== Speichern, Druckkopie (fpdf_save.h, fpdf_ppo.h, fpdf_flatten.h, fpdf_edit.h)

    public const uint SaveNoIncremental = 1 << 1; // FPDF_NO_INCREMENTAL
    public const uint SaveRemoveSecurity = 3;     // FPDF_REMOVE_SECURITY (ein Wert, keine Bitmaske): vollständig und unverschlüsselt
    [LibraryImport(Lib, EntryPoint = "FPDF_GetFileVersion")] public static partial int GetFileVersion(nint document, out int version);
    [LibraryImport(Lib, EntryPoint = "FPDF_GetSecurityHandlerRevision")] public static partial int GetSecurityHandlerRevision(nint document);
    [LibraryImport(Lib, EntryPoint = "FPDF_GetDocUserPermissions")] public static partial uint GetDocUserPermissions(nint document);
    [LibraryImport(Lib, EntryPoint = "FPDFDoc_GetAttachmentCount")] public static partial int DocGetAttachmentCount(nint document);
    [LibraryImport(Lib, EntryPoint = "FPDFDoc_GetAttachment")] public static partial nint DocGetAttachment(nint document, int index);
    [LibraryImport(Lib, EntryPoint = "FPDFAttachment_GetName")] public static partial uint AttachmentGetName(nint attachment, byte* buffer, uint length);
    [LibraryImport(Lib, EntryPoint = "FPDFAttachment_GetFile")] public static partial int AttachmentGetFile(nint attachment, byte* buffer, uint length, out uint written);
    public const int FlattenForPrint = 1;         // FLAT_PRINT
    public const int FlattenFailed = 0;           // FLATTEN_FAIL (1 = erledigt, 2 = nichts einzubrennen)

    [LibraryImport(Lib, EntryPoint = "FPDF_SaveAsCopy")] public static partial int SaveAsCopy(nint document, nint fileWrite, uint flags);
    [LibraryImport(Lib, EntryPoint = "FPDF_CreateNewDocument")] public static partial nint CreateNewDocument();
    [LibraryImport(Lib, EntryPoint = "FPDF_ImportPages")] public static partial int ImportPages(nint destination, nint source, nint pageRange, int index);
    [LibraryImport(Lib, EntryPoint = "FPDFPage_Flatten")] public static partial int FlattenPage(nint page, int flag);
}
