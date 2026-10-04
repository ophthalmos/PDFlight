using System.Drawing.Drawing2D;

namespace PDFLight.Viewer;

/// <summary>Zeichnet Kopfzeile und senkrechte Umschaltleiste der Seitenleiste (Miniaturen / Dokumentstruktur) wie in Chrome und Edge:
/// Fläche in der Farbe der Seitenleiste, beim Überfahren ein abgerundetes Feld, die gewählte Ansicht mit einem Balken in der
/// Akzentfarbe am linken Rand (ihr Symbol färbt der Aufrufer, <see cref="Accent"/>), dünne Trennlinien wie bei Edge – unter der
/// Kopfzeile (Dock oben) und rechts neben der Umschaltleiste (Dock links).</summary>
internal sealed class SidebarRailRenderer(bool dark) : ToolStripRenderer
{
    private const int BarWidth = 4, BarInset = 6, HoverInset = 4, HoverRadius = 6; // logische px

    /// <summary>Akzentfarbe der gewählten Ansicht (gemessen in Chrome: dunkel 138/180/248, hell 11/87/208).</summary>
    public static Color Accent(bool dark) => dark ? Color.FromArgb(138, 180, 248) : Color.FromArgb(11, 87, 208);

    public static Color Back(bool dark) => dark ? Color.FromArgb(40, 40, 40) : SystemColors.Window;

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        using var brush = new SolidBrush(Back(dark));
        e.Graphics.FillRectangle(brush, e.AffectedBounds);
    }

    public static Color Line(bool dark) => dark ? Color.FromArgb(72, 72, 72) : Color.FromArgb(222, 222, 222);

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        using var pen = new Pen(Line(dark));
        var size = e.ToolStrip.Size;
        if (e.ToolStrip.Dock == DockStyle.Left) { e.Graphics.DrawLine(pen, size.Width - 1, 0, size.Width - 1, size.Height); }
        else { e.Graphics.DrawLine(pen, 0, size.Height - 1, size.Width, size.Height - 1); }
    }

    protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var size = e.Item.Size;
        int Scale(int value) => e.ToolStrip?.LogicalToDeviceUnits(value) ?? value;
        if (e.Item.Selected || e.Item.Pressed)
        {
            var inset = e.ToolStrip?.Dock == DockStyle.Left ? Scale(HoverInset) : 0; // in der Kopfzeile füllt das Feld die Schaltfläche
            var bar = e.ToolStrip?.Dock == DockStyle.Left ? Scale(BarWidth) : 0;
            var hover = dark ? Color.FromArgb(e.Item.Pressed ? 85 : 66, e.Item.Pressed ? 85 : 66, e.Item.Pressed ? 85 : 66)
                : Color.FromArgb(e.Item.Pressed ? 214 : 232, e.Item.Pressed ? 214 : 232, e.Item.Pressed ? 214 : 232);
            using var brush = new SolidBrush(hover);
            using var path = RoundedRectangle(new Rectangle(inset + bar, inset, size.Width - 2 * inset - bar, size.Height - 2 * inset), Scale(HoverRadius));
            g.FillPath(brush, path);
        }
        if (e.Item is ToolStripButton { Checked: true })
        {
            var width = Scale(BarWidth);
            using var brush = new SolidBrush(Accent(dark));
            using var path = RoundedRectangle(new Rectangle(-width, Scale(BarInset), 2 * width, size.Height - 2 * Scale(BarInset)), width); // rechts gerundet, links am Rand
            g.FillPath(brush, path);
        }
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        // nur ohne Symbolschrift: die Zeichen aus dem Designer, die gewählte Ansicht in der Akzentfarbe
        e.TextColor = e.Item is ToolStripButton { Checked: true } ? Accent(dark) : dark ? Color.FromArgb(232, 232, 232) : SystemColors.ControlText;
        base.OnRenderItemText(e);
    }

    private static GraphicsPath RoundedRectangle(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var d = Math.Max(1, Math.Min(2 * radius, Math.Min(rect.Width, rect.Height)));
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}