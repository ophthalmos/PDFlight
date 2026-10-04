using System.ComponentModel;
using System.Runtime.InteropServices;

namespace PDFLight.Viewer;

/// <summary>GDI-Funktionen für den Druck: Das Hilfsprogramm liefert jede Seite als EMF (Vektor), das Hauptprogramm spielt sie im
/// Gerätekontext des Druckers ab.</summary>
internal static unsafe partial class NativePdfium
{
    public const int HorzRes = 8, VertRes = 10; // bedruckbarer Bereich in Gerätepixeln
    public const int LogPixelsX = 88;           // Auflösung (Pixel je Zoll)

    [LibraryImport("gdi32.dll", EntryPoint = "GetDeviceCaps")]
    public static partial int GetDeviceCaps(nint dc, int index);

    [LibraryImport("gdi32.dll", EntryPoint = "SetEnhMetaFileBits")] private static partial nint SetEnhMetaFileBits(uint size, byte* data);
    [LibraryImport("gdi32.dll", EntryPoint = "PlayEnhMetaFile")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool PlayEnhMetaFile(nint dc, nint metafile, Rect* bounds);
    [LibraryImport("gdi32.dll", EntryPoint = "DeleteEnhMetaFile")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool DeleteEnhMetaFile(nint metafile);

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left, Top, Right, Bottom;
    }

    /// <summary>Spielt eine EMF in einem Gerätekontext ab, eingepasst in <paramref name="target"/> (Gerätepixel).</summary>
    public static void PlayMetafile(nint dc, byte[] emf, Rectangle target)
    {
        nint metafile;
        fixed (byte* data = emf) { metafile = SetEnhMetaFileBits((uint)emf.Length, data); }
        if (metafile == 0) { throw new Win32Exception(); }
        try
        {
            var bounds = new Rect { Left = target.Left, Top = target.Top, Right = target.Right, Bottom = target.Bottom };
            if (!PlayEnhMetaFile(dc, metafile, &bounds)) { throw new Win32Exception(); }
        }
        finally { _ = DeleteEnhMetaFile(metafile); }
    }
}
