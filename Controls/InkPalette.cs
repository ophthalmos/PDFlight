using System.Drawing.Drawing2D;

namespace PDFLight.Controls;

/// <summary>Farbe und Strichstärke fürs Zeichnen, nach dem Vorbild von Edge (Wunsch vom 03.10.2026): 30 Farbkreise in 5 Reihen, darunter
/// eine Vorschau-Linie in Farbe und Stärke und der Regler „Stärke“ (0,5–12 pt in halben Punkten). Die Viewer-Leiste zeigt es als
/// Dropdown neben „Zeichnen“ (ToolStripControlHost in MainForm).</summary>
public partial class InkPalette : UserControl
{
    private const int Columns = 6, CircleSize = 28, CellSize = 37; // logische px

    /// <summary>Die Farben in Edges Reihenfolge (Graustufen, kräftige Farben, Brauntöne, Pastell).</summary>
    public static readonly Color[] Colors =
    [
        Color.FromArgb(0, 0, 0), Color.FromArgb(89, 89, 89), Color.FromArgb(140, 140, 140), Color.FromArgb(171, 171, 171), Color.FromArgb(214, 214, 214), Color.FromArgb(255, 255, 255),
        Color.FromArgb(180, 19, 91), Color.FromArgb(232, 17, 35), Color.FromArgb(255, 90, 0), Color.FromArgb(255, 165, 0), Color.FromArgb(255, 204, 0), Color.FromArgb(255, 230, 0),
        Color.FromArgb(164, 224, 29), Color.FromArgb(44, 224, 15), Color.FromArgb(0, 128, 85), Color.FromArgb(0, 163, 204), Color.FromArgb(0, 80, 230), Color.FromArgb(58, 0, 179),
        Color.FromArgb(108, 0, 217), Color.FromArgb(92, 0, 128), Color.FromArgb(247, 215, 196), Color.FromArgb(184, 138, 96), Color.FromArgb(140, 80, 34), Color.FromArgb(92, 58, 40),
        Color.FromArgb(255, 128, 240), Color.FromArgb(255, 192, 128), Color.FromArgb(255, 255, 128), Color.FromArgb(128, 255, 159), Color.FromArgb(128, 212, 255), Color.FromArgb(196, 181, 255),
    ];

    private int hover = -1;
    private bool dark;

    /// <summary>Farbe oder Stärke wurde geändert.</summary>
    public event EventHandler? InkChanged;

    public InkPalette()
    {
        InitializeComponent();
        DoubleBufferPanels();
    }

    /// <summary>Gewählte Farbe.</summary>
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden), System.ComponentModel.Browsable(false)]
    public Color SelectedColor
    {
        get;
        set { field = value; panelColors.Invalidate(); panelPreview.Invalidate(); }
    } = Color.FromArgb(0, 80, 230);

    /// <summary>Strichstärke in Punkt (0,5–12).</summary>
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden), System.ComponentModel.Browsable(false)]
    public float InkWidth
    {
        get => trackWidth.Value / 2f;
        set => trackWidth.Value = Math.Clamp((int)Math.Round(value * 2), trackWidth.Minimum, trackWidth.Maximum);
    }

    /// <summary>Dunkle Darstellung passend zum Anzeigehintergrund.</summary>
    public void ApplyTheme(bool dark)
    {
        this.dark = dark;
        BackColor = trackWidth.BackColor = dark ? Color.FromArgb(44, 44, 44) : SystemColors.Window;
        ForeColor = dark ? Color.FromArgb(232, 232, 232) : SystemColors.ControlText;
        labelThin.ForeColor = labelWide.ForeColor = dark ? Color.FromArgb(170, 170, 170) : SystemColors.GrayText;
        Invalidate(true);
    }

    private void DoubleBufferPanels()
    {
        // Panels zeichnen ohne Doppelpufferung beim Überfahren sichtbar nach; die Eigenschaft ist geschützt
        foreach (var panel in new[] { panelColors, panelPreview })
        {
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(panel, true);
        }
    }

    private Rectangle CircleBounds(int index)
    {
        int Scale(int value) => LogicalToDeviceUnits(value);
        var offset = (Scale(CellSize) - Scale(CircleSize)) / 2;
        return new Rectangle(index % Columns * Scale(CellSize) + offset, index / Columns * Scale(CellSize) + offset, Scale(CircleSize), Scale(CircleSize));
    }

    private int IndexAt(Point point)
    {
        for (var i = 0; i < Colors.Length; i++)
        {
            var bounds = CircleBounds(i);
            bounds.Inflate(2, 2);
            if (bounds.Contains(point)) { return i; }
        }
        return -1;
    }

    private void PanelColors_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var ring = dark ? Color.FromArgb(232, 232, 232) : Color.FromArgb(0, 95, 184);
        for (var i = 0; i < Colors.Length; i++)
        {
            var bounds = CircleBounds(i);
            var selected = Colors[i].ToArgb() == SelectedColor.ToArgb();
            if (selected || i == hover)
            {
                // gewählt: Ring mit Abstand wie bei Edge; überfahren: dünner grauer Ring
                var outer = bounds;
                outer.Inflate(LogicalToDeviceUnits(3), LogicalToDeviceUnits(3));
                using var pen = new Pen(selected ? ring : Color.Gray, LogicalToDeviceUnits(selected ? 2 : 1));
                g.DrawEllipse(pen, outer);
            }
            using (var brush = new SolidBrush(Colors[i])) { g.FillEllipse(brush, bounds); }
            using (var border = new Pen(Color.FromArgb(dark ? 90 : 60, 128, 128, 128))) { g.DrawEllipse(border, bounds); } // helle Farben auf hellem Grund
        }
    }

    private void PanelColors_MouseMove(object? sender, MouseEventArgs e)
    {
        var index = IndexAt(e.Location);
        if (index == hover) { return; }
        hover = index;
        panelColors.Cursor = index >= 0 ? Cursors.Hand : Cursors.Default;
        panelColors.Invalidate();
    }

    private void PanelColors_MouseLeave(object? sender, EventArgs e)
    {
        hover = -1;
        panelColors.Invalidate();
    }

    private void PanelColors_MouseClick(object? sender, MouseEventArgs e)
    {
        var index = IndexAt(e.Location);
        if (e.Button != MouseButtons.Left || index < 0) { return; }
        SelectedColor = Colors[index];
        InkChanged?.Invoke(this, EventArgs.Empty);
    }

    private void PanelPreview_Paint(object? sender, PaintEventArgs e)
    {
        // Wellenlinie wie bei Edge, so breit wie der Strich bei 100 % Zoom
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var size = panelPreview.ClientSize;
        var width = Math.Max(1f, InkWidth * DeviceDpi / 72f);
        var margin = width / 2 + LogicalToDeviceUnits(6);
        var points = Enumerable.Range(0, 41).Select(i =>
        {
            var t = i / 40f;
            return new PointF(margin + t * (size.Width - 2 * margin), size.Height / 2f - (float)Math.Sin(t * 2 * Math.PI) * (size.Height / 2f - margin));
        }).ToArray();
        using var pen = new Pen(SelectedColor, width) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        g.DrawCurve(pen, points);
    }

    private void TrackWidth_ValueChanged(object? sender, EventArgs e)
    {
        panelPreview.Invalidate();
        InkChanged?.Invoke(this, EventArgs.Empty);
    }
}