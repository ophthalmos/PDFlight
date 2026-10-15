using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace PDFLight.Controls;

/// <summary>Schmaler senkrechter Streifen (≈ 16 px, Acrobat 18) am Rand der Seitenleiste nach dem Vorbild von Acrobat (Wunsch vom 15.10.2026): ein Klick klappt die
/// Seitenleiste auf oder zu, der Pfeil in der Mitte zeigt, wohin – ▶ bei geschlossener, ◀ bei offener Seitenleiste. Ersetzt den Knopf
/// „Inhaltsverzeichnis“ der Viewer-Leiste; die Umschaltleiste mit den Ansichten bleibt dabei immer sichtbar.</summary>
public sealed class SidebarToggle : Control
{
    private readonly ToolTip toolTip = new();
    private bool hot;

    public SidebarToggle()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        SetStyle(ControlStyles.Selectable, false); // wie ein Splitter: kein Tastaturfokus, bedient wird mit F10
        Cursor = Cursors.Hand;
    }

    [Category("Verhalten"), DefaultValue(true), Description("Die Seitenleiste ist zu: Pfeil nach rechts.")]
    public bool Collapsed { get; set { field = value; Invalidate(); } } = true;

    [Category("Darstellung"), DefaultValue(typeof(Color), "200, 200, 200"), Description("Hintergrund unter der Maus.")]
    public Color HoverBackColor { get; set { field = value; Invalidate(); } } = Color.FromArgb(200, 200, 200);

    [Category("Darstellung"), DefaultValue(typeof(Color), "80, 80, 80"), Description("Farbe des Pfeils.")]
    public Color ArrowColor { get; set { field = value; Invalidate(); } } = Color.FromArgb(80, 80, 80);

    [Category("Verhalten"), DefaultValue(""), Description("Tooltip beim Überfahren.")]
    public string ToolTipText
    {
        get;
        set { field = value ?? string.Empty; toolTip.SetToolTip(this, field); }
    } = string.Empty;

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        using (SolidBrush back = new(hot ? HoverBackColor : BackColor)) { g.FillRectangle(back, ClientRectangle); }
        // kleines gefülltes Dreieck in der Mitte, 5 × 10 logische px wie bei Acrobat
        var (w, h) = (LogicalToDeviceUnits(5), LogicalToDeviceUnits(10));
        var x = (ClientSize.Width - w) / 2f;
        var y = (ClientSize.Height - h) / 2f;
        PointF[] arrow = Collapsed
            ? [new(x, y), new(x + w, y + h / 2f), new(x, y + h)]
            : [new(x + w, y), new(x, y + h / 2f), new(x + w, y + h)];
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using SolidBrush brush = new(ArrowColor);
        g.FillPolygon(brush, arrow);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        hot = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        hot = false;
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { toolTip.Dispose(); }
        base.Dispose(disposing);
    }
}
