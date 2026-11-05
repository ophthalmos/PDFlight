using System.ComponentModel;
using System.Drawing.Drawing2D;
using PDFLight.Classes;

namespace PDFLight.Controls;

/// <summary>Feld zum Unterschreiben mit Maus, Stift oder Finger (Wunsch vom 03.11.2026): Striche werden als Punktfolgen gesammelt und
/// geglättet gezeichnet, darunter eine Unterschriftslinie mit „×“ wie auf Papier. Alternativ zeigt es eine aus einem Bild geladene
/// Unterschrift (<see cref="ShowImage"/>). <see cref="CreateSignature"/> liefert das Ergebnis.</summary>
internal sealed class SignaturePad : Control
{
    private readonly List<List<PointF>> strokes = [];
    private List<PointF>? current;
    private Signature? image; // geladenes Bild statt gezeichneter Striche

    public SignaturePad()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        BackColor = Color.White;
        Cursor = Cursors.Cross;
    }

    /// <summary>Text, solange das Feld leer ist.</summary>
    [Category("Appearance"), DefaultValue("")]
    public string EmptyText { get; set; } = string.Empty;

    /// <summary>Gezeichnet oder ein Bild geladen.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool HasContent => image != null || strokes.Count > 0;

    /// <summary>Inhalt geändert (gezeichnet, geleert, Bild geladen).</summary>
    public event EventHandler? ContentChanged;

    private float PenWidth => LogicalToDeviceUnits(3);

    public void Clear()
    {
        strokes.Clear();
        current = null;
        image = null;
        Invalidate();
        ContentChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Eine aus einem Bild gewonnene Unterschrift zeigen; die Striche entfallen.</summary>
    public void ShowImage(Signature signature)
    {
        strokes.Clear();
        current = null;
        image = signature;
        Invalidate();
        ContentChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Die Unterschrift aus Bild oder Strichen; null, wenn das Feld leer ist.</summary>
    public Signature? CreateSignature() => image ?? Signature.FromStrokes(strokes, PenWidth, DeviceDpi);

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        if (e.Button != MouseButtons.Left) { return; }
        if (image != null) { image = null; } // Zeichnen ersetzt ein geladenes Bild
        current = [e.Location];
        strokes.Add(current);
        Capture = true;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (current == null) { return; }
        var last = current[^1];
        if (Math.Abs(e.X - last.X) + Math.Abs(e.Y - last.Y) < 2) { return; } // jitterfreie Punktfolge
        current.Add(e.Location);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (current == null) { return; }
        current = null;
        Capture = false;
        Invalidate();
        ContentChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        // Unterschriftslinie mit „×“ wie auf einem Formular
        var margin = LogicalToDeviceUnits(24);
        var lineY = Height - LogicalToDeviceUnits(36);
        using (Pen line = new(Color.FromArgb(190, 190, 190), 1))
        {
            g.DrawLine(line, margin, lineY, Width - margin, lineY);
            var cross = LogicalToDeviceUnits(5);
            var crossX = margin + LogicalToDeviceUnits(4);
            var crossY = lineY - LogicalToDeviceUnits(10);
            g.DrawLine(line, crossX - cross, crossY - cross, crossX + cross, crossY + cross);
            g.DrawLine(line, crossX - cross, crossY + cross, crossX + cross, crossY - cross);
        }
        if (image != null)
        {
            var pad = LogicalToDeviceUnits(16);
            image.Draw(g, new RectangleF(pad, pad, Width - 2 * pad, lineY - pad), Color.Black);
            return;
        }
        if (strokes.Count == 0)
        {
            TextRenderer.DrawText(g, EmptyText, Font, new Rectangle(0, 0, Width, lineY - LogicalToDeviceUnits(16)), SystemColors.GrayText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            return;
        }
        using Pen pen = new(Color.Black, PenWidth) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        foreach (var stroke in strokes)
        {
            if (stroke.Count == 1) { g.DrawLine(pen, stroke[0], new PointF(stroke[0].X + 0.1f, stroke[0].Y)); }
            else { g.DrawCurve(pen, [.. stroke], 0.5f); }
        }
    }
}
