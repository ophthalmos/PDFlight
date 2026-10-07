using System.Runtime.InteropServices;

namespace PDFLight.Controls;

/// <summary>ListView (Details) mit festen Spaltenköpfen: grau hinterlegt, ohne Hover- und Druck-Effekt. Für Listen, die sich nicht per
/// Klick auf den Kopf sortieren lassen – der Windows-Kopf hellte beim Überfahren auf und versprach so eine Sortierung, die es nicht gibt
/// (Wunsch vom 06.10.2026: Anhänge, Berechtigungen, „Anmerkungen verwalten“). Die Köpfe zeichnet das Control selbst (OwnerDraw), die
/// Einträge Windows; den Rest des Kopfes rechts der letzten Spalte malt ein Unterfenster-Hook nach dem Zeichnen des Kopfes grau aus.</summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal partial class StaticHeaderListView : ListView
{
    private const int LVM_GETHEADER = 0x101F;
    private const int HDM_GETITEMCOUNT = 0x1200;
    private const int HDM_GETITEMRECT = 0x1207;
    private const int WM_PAINT = 0x000F;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial nint SendMessage(nint hwnd, int msg, nint wParam, nint lParam);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial nint SendMessage(nint hwnd, int msg, nint wParam, ref RECT lParam);

    private static readonly Color HeaderBack = SystemColors.Control;
    private static readonly Color HeaderLine = Color.FromArgb(213, 213, 213);

    private HeaderWindow? header;

    public StaticHeaderListView()
    {
        OwnerDraw = true;
        View = View.Details;
        HeaderStyle = ColumnHeaderStyle.Nonclickable; // auch ohne Zeichnen: kein Drücken, kein Klick-Ereignis
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        var handle = SendMessage(Handle, LVM_GETHEADER, 0, 0);
        if (handle != 0) { header = new HeaderWindow(handle); }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        header?.ReleaseHandle();
        header = null;
        base.OnHandleDestroyed(e);
    }

    protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
    {
        var g = e.Graphics;
        using (SolidBrush back = new(HeaderBack)) { g.FillRectangle(back, e.Bounds); }
        using (Pen line = new(HeaderLine))
        {
            g.DrawLine(line, e.Bounds.Right - 1, e.Bounds.Top + LogicalToDeviceUnits(3), e.Bounds.Right - 1, e.Bounds.Bottom - LogicalToDeviceUnits(4)); // Trenner
            g.DrawLine(line, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }
        var padding = LogicalToDeviceUnits(6);
        Rectangle text = new(e.Bounds.Left + padding, e.Bounds.Top, Math.Max(0, e.Bounds.Width - 2 * padding), e.Bounds.Height);
        var align = e.Header?.TextAlign switch
        {
            HorizontalAlignment.Right => TextFormatFlags.Right,
            HorizontalAlignment.Center => TextFormatFlags.HorizontalCenter,
            _ => TextFormatFlags.Left,
        };
        TextRenderer.DrawText(g, e.Header?.Text, Font, text, SystemColors.ControlText,
            align | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
    }

    protected override void OnDrawItem(DrawListViewItemEventArgs e) => e.DrawDefault = true;

    protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e) => e.DrawDefault = true;

    /// <summary>Kopf-Steuerelement: nach seinem eigenen Zeichnen die Fläche rechts der letzten Spalte grau ausmalen – die zeichnet Windows
    /// selbst (weiß), OwnerDraw erreicht sie nicht.</summary>
    private sealed class HeaderWindow : NativeWindow
    {
        public HeaderWindow(nint handle) => AssignHandle(handle);

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg != WM_PAINT) { return; }
            var count = (int)SendMessage(Handle, HDM_GETITEMCOUNT, 0, 0);
            var left = 0;
            if (count > 0)
            {
                RECT last = default;
                if (SendMessage(Handle, HDM_GETITEMRECT, count - 1, ref last) != 0) { left = last.Right; }
            }
            using var g = Graphics.FromHwnd(Handle);
            var client = Rectangle.Truncate(g.VisibleClipBounds);
            if (left >= client.Right) { return; }
            using (SolidBrush back = new(HeaderBack)) { g.FillRectangle(back, left, client.Top, client.Right - left, client.Height); }
            using Pen line = new(HeaderLine);
            g.DrawLine(line, left, client.Bottom - 1, client.Right, client.Bottom - 1);
        }
    }
}
