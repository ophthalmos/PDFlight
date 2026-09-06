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
        g.Clear(Parent?.BackColor ?? SystemColors.Window); // Ecken außerhalb des Kreises: der Treppenrand der Region verschwindet im Hintergrund
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var fill = !Enabled ? BackColor
            : pressed ? Blend(BackColor, Color.Black, 0.18f)
            : hovered ? Blend(BackColor, Color.Black, 0.09f)
            : BackColor;
        using SolidBrush brush = new(fill);
        RectangleF disc = new(1f, 1f, Width - 2f, Height - 2f); // ein Pixel innerhalb der Region, damit die weiche Kante sichtbar bleibt
        g.FillEllipse(brush, disc);
        using Pen outline = new(Blend(BackColor, Color.Black, 0.22f));
        g.DrawEllipse(outline, disc);

        if (Glyph != '\0' && ToolbarIcons.FontAvailable)
        {
            var edge = Math.Max(8, Height / 2);
            var image = ToolbarIcons.Get(Glyph, new Size(edge, edge));
            var rect = new Rectangle((Width - edge) / 2, (Height - edge) / 2, edge, edge);
            if (Enabled) { g.DrawImage(image, rect); }
            else { ControlPaint.DrawImageDisabled(g, image, rect.X, rect.Y, Color.Transparent); }
        }
        else
        {
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, Enabled ? ForeColor : SystemColors.GrayText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }

    private static Color Blend(Color color, Color with, float amount) => Color.FromArgb(
        (int)(color.R + (with.R - color.R) * amount),
        (int)(color.G + (with.G - color.G) * amount),
        (int)(color.B + (with.B - color.B) * amount));
}
