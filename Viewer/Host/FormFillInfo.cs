using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PDFLight.Host;

/// <summary>Die Rückruf-Struktur FPDF_FORMFILLINFO (Version 1) für die Formularumgebung eines Dokuments. Die Rückrufe ruft PDFium
/// während der FORM_-Aufrufe und beim Rendern auf; sie finden ihr Dokument über die Adresse der Struktur. Timer gibt es bewusst nicht:
/// das Hilfsprogramm antwortet nur auf Befehle, ein blinkender Textcursor bräuchte Meldungen von sich aus (der Cursor steht dann
/// einfach still). Nicht benötigte Rückrufe bleiben null – PDFium prüft jeden vor dem Aufruf.</summary>
internal static unsafe class FormFillInfo
{
    // x64: version (int + 4 Byte Ausrichtung), dann 15 Funktionszeiger und m_pJsPlatform. Großzügig angelegt und genullt.
    private const int InfoSize = 512;
    private const int OffInvalidate = 16, OffOnChange = 64, OffGetPage = 72, OffGetCurrentPage = 80, OffGetRotation = 88;

    private static readonly ConcurrentDictionary<nint, PdfDocument> Owners = new();

    public static nint Create(PdfDocument owner)
    {
        var info = (nint)NativeMemory.AllocZeroed(InfoSize);
        *(int*)info = 1; // version
        *(nint*)(info + OffInvalidate) = (nint)(delegate* unmanaged[Cdecl]<nint, nint, double, double, double, double, void>)&Invalidate;
        *(nint*)(info + OffOnChange) = (nint)(delegate* unmanaged[Cdecl]<nint, void>)&OnChange;
        *(nint*)(info + OffGetPage) = (nint)(delegate* unmanaged[Cdecl]<nint, nint, int, nint>)&GetPage;
        *(nint*)(info + OffGetCurrentPage) = (nint)(delegate* unmanaged[Cdecl]<nint, nint, nint>)&GetCurrentPage;
        *(nint*)(info + OffGetRotation) = (nint)(delegate* unmanaged[Cdecl]<nint, nint, int>)&GetRotation;
        Owners[info] = owner;
        return info;
    }

    public static void Free(nint info)
    {
        Owners.TryRemove(info, out _);
        NativeMemory.Free((void*)info);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void Invalidate(nint info, nint page, double left, double top, double right, double bottom)
    {
        if (Owners.TryGetValue(info, out var owner)) { owner.OnFormInvalidate(page, left, top, right, bottom); }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnChange(nint info)
    {
        if (Owners.TryGetValue(info, out var owner)) { Events.Changed(owner.Id); }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetPage(nint info, nint document, int index) => Owners.TryGetValue(info, out var owner) ? owner.LoadedPageHandle(index) : 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nint GetCurrentPage(nint info, nint document) => Owners.TryGetValue(info, out var owner) ? owner.LoadedPageHandle(owner.CurrentPage) : 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int GetRotation(nint info, nint page) => 0;

    // ================================================================== Speichern (FPDF_FILEWRITE)

    // FPDF_FILEWRITE: version (int + Ausrichtung), WriteBlock. Es läuft immer nur ein Speichervorgang (ein Thread) – ein statischer
    // Zielstrom genügt.
    private static MemoryStream? saveTarget;

    /// <summary>Speichert ein Dokument vollständig neu in ein Byte-Feld.</summary>
    public static byte[] SaveToBytes(nint document, uint flags = Pdfium.SaveNoIncremental)
    {
        var writer = (nint)NativeMemory.AllocZeroed(16);
        try
        {
            *(int*)writer = 1;
            *(nint*)(writer + 8) = (nint)(delegate* unmanaged[Cdecl]<nint, nint, uint, int>)&WriteBlock;
            saveTarget = new MemoryStream();
            if (Pdfium.SaveAsCopy(document, writer, flags) == 0) { throw new InvalidOperationException("PDFium konnte das Dokument nicht speichern."); }
            return saveTarget.ToArray();
        }
        finally
        {
            saveTarget = null;
            NativeMemory.Free((void*)writer);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int WriteBlock(nint self, nint data, uint size)
    {
        saveTarget?.Write(new ReadOnlySpan<byte>((void*)data, (int)size));
        return 1;
    }
}
