using System.Drawing.Drawing2D;
using PDFLight.Classes;

namespace PDFLight.Controls;

/// <summary>Dropdown „Unterschreiben“ der Viewer-Leiste wie bei Acrobat (Wunsch vom 03.11.2026): die gespeicherten Unterschriften als
/// Vorschau, rechts je ein Minus zum Entfernen, darunter „Unterschrift hinzufügen…“. Die Zeilen zeichnet die Palette selbst
/// (Laufzeitdaten); eingebunden als ToolStripControlHost in MainForm.</summary>
public partial class SignaturePalette : UserControl
{
    private const int RowHeight = 56, AddRowHeight = 36, Width0 = 260; // logische px
    private List<Signature> signatures = [];
    private int hover = -1;          // Zeile unter der Maus (Signaturen 0…n-1, n = „hinzufügen“)
    private bool hoverRemove;        // … und dort das Minus
    private bool dark;

    /// <summary>Eine Unterschrift zum Einfügen gewählt.</summary>
    internal event EventHandler<Signature>? SignatureChosen;

    /// <summary>„Unterschrift hinzufügen…“ geklickt.</summary>
    public event EventHandler? AddRequested;

    /// <summary>Minus an einer Unterschrift geklickt.</summary>
    internal event EventHandler<Signature>? RemoveRequested;

    public SignaturePalette()
    {
        InitializeComponent();
        typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(panelRows, true);
    }

    /// <summary>Gespeicherte Unterschriften zeigen; die Höhe richtet sich nach ihrer Zahl.</summary>
    internal void SetSignatures(List<Signature> list)
    {
        signatures = list;
        hover = -1;
        Size = new Size(LogicalToDeviceUnits(Width0), Padding.Vertical + LogicalToDeviceUnits(RowHeight) * signatures.Count + LogicalToDeviceUnits(AddRowHeight));
        panelRows.Invalidate();
    }

    public void ApplyTheme(bool dark)
    {
        this.dark = dark;
        BackColor = dark ? Color.FromArgb(44, 44, 44) : SystemColors.Window;
        ForeColor = dark ? Color.FromArgb(232, 232, 232) : SystemColors.ControlText;
        panelRows.Invalidate();
    }

    private bool CanAdd => signatures.Count < SignatureStore.Limit;

    private Rectangle RowBounds(int index)
    {
        var height = index < signatures.Count ? LogicalToDeviceUnits(RowHeight) : LogicalToDeviceUnits(AddRowHeight);
        return new Rectangle(0, index * LogicalToDeviceUnits(RowHeight), panelRows.Width, height);
    }

    private Rectangle RemoveBounds(int index)
    {
        var row = RowBounds(index);
        var size = LogicalToDeviceUnits(18);
        return new Rectangle(row.Right - size - LogicalToDeviceUnits(12), row.Y + (row.Height - size) / 2, size, size);
    }

    private int IndexAt(Point point)
    {
        for (var i = 0; i <= signatures.Count; i++) { if (RowBounds(i).Contains(point)) { return i; } }
        return -1;
    }

    private void PanelRows_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        var hoverFill = dark ? Color.FromArgb(70, 70, 70) : Color.FromArgb(229, 241, 251);
        var ink = dark ? Color.FromArgb(240, 240, 240) : Color.Black;
        var gray = dark ? Color.FromArgb(160, 160, 160) : Color.FromArgb(110, 110, 110);
        for (var i = 0; i < signatures.Count; i++)
        {
            var row = RowBounds(i);
            if (i == hover) { using SolidBrush fill = new(hoverFill); g.FillRectangle(fill, row); }
            var remove = RemoveBounds(i);
            var preview = Rectangle.FromLTRB(row.X + LogicalToDeviceUnits(12), row.Y + LogicalToDeviceUnits(6), remove.Left - LogicalToDeviceUnits(10), row.Bottom - LogicalToDeviceUnits(6));
            signatures[i].Draw(g, preview, ink, recolor: dark); // im Dunkelmodus auch geladene Bilder hell, sonst dunkle Tinte auf dunklem Grund
            // Minus im Kreis (wie Acrobat): grau, unter der Maus kräftiger
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush circle = new(i == hover && hoverRemove ? (dark ? Color.FromArgb(200, 200, 200) : Color.FromArgb(90, 90, 90)) : gray)) { g.FillEllipse(circle, remove); }
            using (Pen bar = new(BackColor, LogicalToDeviceUnits(2))) { g.DrawLine(bar, remove.X + remove.Width * 0.27f, remove.Y + remove.Height / 2f, remove.Right - remove.Width * 0.27f, remove.Y + remove.Height / 2f); }
            g.SmoothingMode = SmoothingMode.None;
            using Pen separator = new(dark ? Color.FromArgb(70, 70, 70) : Color.FromArgb(225, 225, 225));
            g.DrawLine(separator, row.X + LogicalToDeviceUnits(8), row.Bottom - 1, row.Right - LogicalToDeviceUnits(8), row.Bottom - 1);
        }
        var add = RowBounds(signatures.Count);
        if (signatures.Count == hover && CanAdd) { using SolidBrush fill = new(hoverFill); g.FillRectangle(fill, add); }
        var text = CanAdd ? Lng.T("Unterschrift hinzufügen…") : string.Format(Lng.T("Höchstens {0} Unterschriften"), SignatureStore.Limit);
        TextRenderer.DrawText(g, text, Font, Rectangle.FromLTRB(add.X + LogicalToDeviceUnits(14), add.Y, add.Right, add.Bottom), CanAdd ? (dark ? Color.FromArgb(138, 180, 248) : Color.FromArgb(0, 95, 184)) : gray,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }

    private void PanelRows_MouseMove(object? sender, MouseEventArgs e)
    {
        var index = IndexAt(e.Location);
        var onRemove = index >= 0 && index < signatures.Count && RemoveBounds(index).Contains(e.Location);
        if (index == hover && onRemove == hoverRemove) { return; }
        (hover, hoverRemove) = (index, onRemove);
        panelRows.Cursor = index >= 0 && (index < signatures.Count || CanAdd) ? Cursors.Hand : Cursors.Default;
        toolTip.SetToolTip(panelRows, onRemove ? Lng.T("Unterschrift entfernen") : index >= 0 && index < signatures.Count ? Lng.T("Klicke danach auf die Stelle im Dokument") : null);
        panelRows.Invalidate();
    }

    private void PanelRows_MouseLeave(object? sender, EventArgs e)
    {
        hover = -1;
        panelRows.Invalidate();
    }

    private void PanelRows_MouseClick(object? sender, MouseEventArgs e)
    {
        var index = IndexAt(e.Location);
        if (e.Button != MouseButtons.Left || index < 0) { return; }
        if (index == signatures.Count) { if (CanAdd) { AddRequested?.Invoke(this, EventArgs.Empty); } return; }
        if (RemoveBounds(index).Contains(e.Location)) { RemoveRequested?.Invoke(this, signatures[index]); }
        else { SignatureChosen?.Invoke(this, signatures[index]); }
    }
}
