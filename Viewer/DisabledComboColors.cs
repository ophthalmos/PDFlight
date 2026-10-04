using System.Runtime.InteropServices;

namespace PDFLight.Viewer;

/// <summary>Farben für das Textfeld einer gesperrten ComboBox (Zoomfeld im dunklen Anzeigehintergrund): Gesperrt fragt das Edit seine
/// Farben mit WM_CTLCOLORSTATIC ab statt mit WM_CTLCOLOREDIT – das beantwortet die WinForms-ComboBox nicht, und Windows nahm Weiß
/// (Fehlerbericht 04.10.2026: nach „Dokument schließen“ war das Zoomfeld weiß). Hängt sich per NativeWindow an die ComboBox.</summary>
internal sealed partial class DisabledComboColors : NativeWindow
{
    private const int WM_CTLCOLORSTATIC = 0x0138;
    private static readonly Dictionary<int, nint> brushes = []; // je Farbe ein Pinsel, lebt bis zum Programmende (eine Handvoll GDI-Objekte)
    private (Color Back, Color Fore)? colors;

    /// <summary>An die ComboBox hängen (erneut nach einem neuen Fensterhandle) und die Farben setzen; null = Windows entscheidet.</summary>
    public void Apply(ComboBox combo, Color? back, Color fore)
    {
        if (Handle != combo.Handle)
        {
            if (Handle != 0) { ReleaseHandle(); }
            AssignHandle(combo.Handle);
        }
        colors = back is { } b ? (b, fore) : null;
        combo.Invalidate(true);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_CTLCOLORSTATIC && colors is { } c)
        {
            _ = SetBkColor(m.WParam, ColorTranslator.ToWin32(c.Back));
            _ = SetTextColor(m.WParam, ColorTranslator.ToWin32(c.Fore));
            m.Result = Brush(c.Back);
            return;
        }
        base.WndProc(ref m);
    }

    private static nint Brush(Color color)
    {
        var key = ColorTranslator.ToWin32(color);
        if (!brushes.TryGetValue(key, out var brush)) { brushes[key] = brush = CreateSolidBrush(key); }
        return brush;
    }

    [LibraryImport("gdi32.dll")] private static partial int SetBkColor(nint hdc, int color);
    [LibraryImport("gdi32.dll")] private static partial int SetTextColor(nint hdc, int color);
    [LibraryImport("gdi32.dll")] private static partial nint CreateSolidBrush(int color);
}