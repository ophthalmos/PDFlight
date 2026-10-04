using System.Drawing.Drawing2D;

namespace PDFLight.Controls;

/// <summary>Farbe fürs Hervorheben, nach dem Vorbild der Adobe PDF Embed API (Wunsch vom 04.10.2026): 21 Farbfelder in 3 Reihen, die
/// gewählte mit Häkchen. Die Viewer-Leiste zeigt es als Dropdown neben „Hervorheben“ (ToolStripControlHost in MainForm).</summary>
public partial class HighlightPalette : UserControl
{
    private const int Columns = 7, SwatchSize = 28, CellSize = 33; // logische px

    /// <summary>Die Farben der Adobe-Palette (aus deren Optionenfeld abgelesen): kräftig, hell, Graustufen. FFC100 ist Acrobats Gelb.</summary>
    public static readonly Color[] Colors =
    [
        Color.FromArgb(0x13, 0x73, 0xEB), Color.FromArgb(0x06, 0x8A, 0x1C), Color.FromArgb(0xFF, 0xC1, 0x00), Color.FromArgb(0xFF, 0x62, 0x00),
        Color.FromArgb(0xDB, 0x34, 0x25), Color.FromArgb(0xC0, 0x37, 0xC4), Color.FromArgb(0x96, 0x43, 0xFC),
        Color.FromArgb(0x38, 0xE5, 0xFF), Color.FromArgb(0xC5, 0xFB, 0x72), Color.FromArgb(0xFC, 0xF4, 0x85), Color.FromArgb(0xFF, 0xA9, 0x7B),
        Color.FromArgb(0xF8, 0x64, 0x64), Color.FromArgb(0xFB, 0x88, 0xFF), Color.FromArgb(0xDC, 0xAA, 0xFF),
        Color.FromArgb(0xFF, 0xFF, 0xFF), Color.FromArgb(0xCC, 0xCC, 0xCC), Color.FromArgb(0xAA, 0xAA, 0xAA), Color.FromArgb(0x76, 0x76, 0x76),
        Color.FromArgb(0x44, 0x44, 0x44), Color.FromArgb(0x33, 0x33, 0x33), Color.FromArgb(0x00, 0x00, 0x00),
    ];

    /// <summary>Vorgabe: Acrobats Gelb.</summary>
    public static readonly Color DefaultColor = Color.FromArgb(0xFF, 0xC1, 0x00);

    private int hover = -1;
    private bool dark;

    /// <summary>Eine Farbe wurde angeklickt.</summary>
    public event EventHandler? ColorChanged;

    public HighlightPalette()
    {
        InitializeComponent();
        // das Panel zeichnet ohne Doppelpufferung beim Überfahren sichtbar nach; die Eigenschaft ist geschützt
        typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(panelColors, true);
    }

    /// <summary>Gewählte Farbe.</summary>
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden), System.ComponentModel.Browsable(false)]
    public Color SelectedColor
    {
        get;
        set { field = value; panelColors.Invalidate(); }
    } = DefaultColor;

    /// <summary>Dunkle Darstellung passend zum Anzeigehintergrund.</summary>
    public void ApplyTheme(bool dark)
    {
        this.dark = dark;
        BackColor = dark ? Color.FromArgb(44, 44, 44) : SystemColors.Window;
        ForeColor = dark ? Color.FromArgb(232, 232, 232) : SystemColors.ControlText;
        Invalidate(true);
    }

    private Rectangle SwatchBounds(int index)
    {
        int Scale(int value) => LogicalToDeviceUnits(value);
        var offset = (Scale(CellSize) - Scale(SwatchSize)) / 2;
        return new Rectangle(index % Columns * Scale(CellSize) + offset, index / Columns * Scale(CellSize) + offset, Scale(SwatchSize), Scale(SwatchSize));
    }

    private int IndexAt(Point point)
    {
        for (var i = 0; i < Colors.Length; i++)
        {
            var bounds = SwatchBounds(i);
            bounds.Inflate(2, 2);
            if (bounds.Contains(point)) { return i; }
        }
        return -1;
    }

    private void PanelColors_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        for (var i = 0; i < Colors.Length; i++)
        {
            var bounds = SwatchBounds(i);
            using (var brush = new SolidBrush(Colors[i])) { g.FillRectangle(brush, bounds); }
            // dünner Rand, damit Weiß und helle Farben auf hellem Grund zu sehen sind; überfahren: dunkler Rahmen
            using (var border = new Pen(i == hover ? (dark ? Color.FromArgb(232, 232, 232) : Color.FromArgb(64, 64, 64)) : Color.FromArgb(dark ? 90 : 60, 128, 128, 128), i == hover ? LogicalToDeviceUnits(2) : 1))
            {
                g.DrawRectangle(border, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            }
            if (Colors[i].ToArgb() != SelectedColor.ToArgb()) { continue; }
            // Häkchen wie bei Adobe: auf hellen Farben schwarz, auf dunklen weiß
            var light = Colors[i].R * 0.299 + Colors[i].G * 0.587 + Colors[i].B * 0.114 > 150;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(light ? Color.Black : Color.White, LogicalToDeviceUnits(2)) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            float x = bounds.X, y = bounds.Y, s = bounds.Width;
            g.DrawLines(pen, [new PointF(x + s * 0.25f, y + s * 0.52f), new PointF(x + s * 0.43f, y + s * 0.70f), new PointF(x + s * 0.76f, y + s * 0.32f)]);
            g.SmoothingMode = SmoothingMode.None;
        }
    }

    private void PanelColors_MouseMove(object? sender, MouseEventArgs e)
    {
        var index = IndexAt(e.Location);
        if (index == hover) { return; }
        hover = index;
        panelColors.Cursor = index >= 0 ? Cursors.Hand : Cursors.Default;
        // Weiß zeichnet PDFlight nicht multiplizierend (wäre unsichtbar), sondern kehrt die Farben darunter um
        toolTip.SetToolTip(panelColors, index >= 0 && Colors[index].ToArgb() == Color.White.ToArgb() ? Classes.Lng.T("Umkehren (für hellen Text auf dunklem Grund)") : null);
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
        ColorChanged?.Invoke(this, EventArgs.Empty);
    }
}
