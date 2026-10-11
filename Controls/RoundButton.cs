using System.Drawing.Drawing2D;
using PDFLight.Classes;

namespace PDFLight.Controls;

/// <summary>Runder Schaltknopf, der über einem anderen Control schwebt (z.B. „Neuer Ordner“ über dem
/// Ordnerbaum). Zeichnet sich vollständig selbst: Der Standard-Button malt im Druckzustand ein Rechteck,
/// das die kreisförmige Region nur beschneidet (Strichreste am Rand), und eine Region kennt kein
/// Antialiasing (Treppenrand). Deshalb: Kreis mit Antialiasing einen Pixel innerhalb der Region, die Ecken
/// in der Hintergrundfarbe des Elternfensters, Zustände Normal/Hover/Gedrückt in eigenen Tönen, Symbol als
/// MDL2-Glyphe über ToolbarIcons (Rückfall: Text). Die Region dient nur noch der Trefferfläche.</summary>
internal class RoundButton : Button
{
    private bool hovered;
    private bool pressed;

    /// <summary>MDL2-Glyphe (ToolbarIcons-Konstante); '\0' zeichnet stattdessen den Text.</summary>
    [System.ComponentModel.DefaultValue((char)0), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public char Glyph { get; set; }

    // Optionale Farben (leer = wie bisher: Hover/Gedrückt aus BackColor abgedunkelt, Symbol dunkelgrau, Ecken in der Farbe des
    // Elternfensters). Für das Häkchen der Notizkarte (Wunsch vom 11.10.2026): zurückhaltend, kräftig erst beim Überfahren.

    /// <summary>Füllung beim Überfahren (gedrückt etwas dunkler); leer = BackColor leicht abgedunkelt.</summary>
    [System.ComponentModel.DefaultValue(typeof(Color), "")]
    public Color HoverBackColor { get; set; } = Color.Empty;

    /// <summary>Farbe des Symbols; leer = Dunkelgrau aus ToolbarIcons.</summary>
    [System.ComponentModel.DefaultValue(typeof(Color), "")]
    public Color GlyphColor { get; set; } = Color.Empty;

    /// <summary>Farbe des Symbols beim Überfahren und Drücken; leer = <see cref="GlyphColor"/>.</summary>
    [System.ComponentModel.DefaultValue(typeof(Color), "")]
    public Color HoverGlyphColor { get; set; } = Color.Empty;

    /// <summary>Farbe der Ecken außerhalb des Kreises; leer = Hintergrund des Elternfensters (bei durchsichtigem Elternfenster nötig).</summary>
    [System.ComponentModel.DefaultValue(typeof(Color), "")]
    public Color SurroundColor { get; set; } = Color.Empty;

    public RoundButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        TabStop = false; // per Maus bzw. Tastenkürzel bedient — kein Fokusrahmen im Kreis
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        using GraphicsPath path = new();
        path.AddEllipse(0, 0, Width, Height);
        Region = new Region(path); // nur die Trefferfläche — gezeichnet wird innerhalb davon
    }

    protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hovered = false; pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { pressed = true; Invalidate(); } base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(!SurroundColor.IsEmpty ? SurroundColor : Parent?.BackColor ?? SystemColors.Window); // Ecken außerhalb des Kreises: der Treppenrand der Region verschwindet im Hintergrund
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var hot = Enabled && (hovered || pressed);
        var hoverFill = HoverBackColor.IsEmpty ? null : (Color?)HoverBackColor;
        var fill = !Enabled ? BackColor
            : pressed ? Blend(hoverFill ?? BackColor, Color.Black, 0.18f)
            : hovered ? hoverFill ?? Blend(BackColor, Color.Black, 0.09f)
            : BackColor;
        using SolidBrush brush = new(fill);
        RectangleF disc = new(1f, 1f, Width - 2f, Height - 2f); // ein Pixel innerhalb der Region, damit die weiche Kante sichtbar bleibt
        g.FillEllipse(brush, disc);
        using Pen outline = new(Blend(hot && hoverFill != null ? fill : BackColor, Color.Black, 0.22f));
        g.DrawEllipse(outline, disc);

        if (Glyph != '\0' && ToolbarIcons.FontAvailable)
        {
            var edge = Math.Max(8, Height / 2);
            var glyphColor = hot && !HoverGlyphColor.IsEmpty ? HoverGlyphColor : GlyphColor;
            var image = glyphColor.IsEmpty ? ToolbarIcons.Get(Glyph, new Size(edge, edge)) : ToolbarIcons.Get(Glyph, new Size(edge, edge), glyphColor);
            // nach der gezeichneten Fläche mittig, nicht nach dem Schriftkästchen: Glyphen wie das Häkchen sitzen darin nicht in der
            // Mitte (stand sichtbar links oben, Fehlerbericht 11.10.2026); ganze Pixel, damit das Bild nicht verwischt
            var ink = InkBounds(image);
            var rect = new Rectangle((int)Math.Round((Width - ink.Width) / 2f) - ink.X, (int)Math.Round((Height - ink.Height) / 2f) - ink.Y, edge, edge);
            if (Enabled) { g.DrawImage(image, rect); }
            else { ControlPaint.DrawImageDisabled(g, image, rect.X, rect.Y, Color.Transparent); }
        }
        else
        {
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, Enabled ? ForeColor : SystemColors.GrayText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }

    /// <summary>Rechteck der sichtbaren Pixel eines Symbolbilds (Deckkraft über einem Viertel); das ganze Bild, wenn es keins gibt.</summary>
    private static Rectangle InkBounds(Image image)
    {
        if (image is not Bitmap bitmap) { return new Rectangle(Point.Empty, image.Size); }
        int left = bitmap.Width, top = bitmap.Height, right = -1, bottom = -1;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).A <= 64) { continue; }
                left = Math.Min(left, x); right = Math.Max(right, x);
                top = Math.Min(top, y); bottom = Math.Max(bottom, y);
            }
        }
        return right < 0 ? new Rectangle(Point.Empty, image.Size) : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
    }

    private static Color Blend(Color color, Color with, float amount) => Color.FromArgb(
        (int)(color.R + (with.R - color.R) * amount),
        (int)(color.G + (with.G - color.G) * amount),
        (int)(color.B + (with.B - color.B) * amount));
}
