using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Die Stempelpalette: Doppelklick (oder Enter) setzt den markierten Stempel auf die angezeigte Seite.</summary>
public partial class StampPaletteForm : Form
{
    private readonly IReadOnlyList<Stamp> stamps;

    /// <summary>True, wenn „Stempel bearbeiten“ gedrückt wurde: Der Aufrufer öffnet die Verwaltung; der Einfügevorgang ist damit beendet.</summary>
    public bool ManageRequested { get; private set; }

    /// <summary>Der gewählte Stempel, nach OK gesetzt.</summary>
    public Stamp? SelectedStamp => listStamps.SelectedIndex >= 0 && listStamps.SelectedIndex < stamps.Count ? stamps[listStamps.SelectedIndex] : null;

    public StampPaletteForm(IReadOnlyList<Stamp> stamps, int page)
    {
        InitializeComponent();
        Lng.Apply(this);
        this.stamps = stamps;
        labelPage.Text = string.Format(Lng.T("Der Stempel kommt auf Seite {0}."), page);
        listStamps.Items.AddRange([.. stamps.Select(s => (object)s.Text)]);
        if (listStamps.Items.Count > 0) { listStamps.SelectedIndex = 0; }
        buttonOK.Enabled = listStamps.Items.Count > 0;
    }

    private void ListStamps_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= stamps.Count) { return; }
        StampListPainter.Draw(e, stamps[e.Index], Font);
    }

    private void ButtonEdit_Click(object? sender, EventArgs e)
    {
        ManageRequested = true;
        Close(); // Abbrechen als Ergebnis – der Aufrufer wertet ManageRequested aus
    }

    private void ListStamps_DoubleClick(object? sender, EventArgs e)
    {
        if (SelectedStamp != null) { DialogResult = DialogResult.OK; }
    }
}

/// <summary>Zeichnet einen Listeneintrag der Stempellisten: links die Stempelvorschau, rechts Größe und Position.</summary>
internal static class StampListPainter
{
    public static void Draw(DrawItemEventArgs e, Stamp stamp, Font font)
    {
        // zarte Markierung statt des kräftigen Systemblaus, damit die Stempelvorschau lesbar bleibt
        var selected = (e.State & DrawItemState.Selected) != 0;
        using (SolidBrush background = new(selected ? Color.White : UnselectedColor)) { e.Graphics.FillRectangle(background, e.Bounds); }
        var previewHeight = e.Bounds.Height - 10;
        Rectangle preview = new(e.Bounds.X + 6, e.Bounds.Y + 5, e.Bounds.Width / 2, previewHeight);
        stamp.DrawPreview(e.Graphics, preview);
        var textColor = SystemColors.GrayText;
        var info = $"{stamp.FontSize:0.#} pt · {Lng.T(Stamp.PositionNames[(int)stamp.Position])}";
        TextRenderer.DrawText(e.Graphics, info, font, new Rectangle(preview.Right + 8, e.Bounds.Y, e.Bounds.Right - preview.Right - 10, e.Bounds.Height), textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        if (selected && (e.State & DrawItemState.Focus) != 0) { ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds); }
    }

    /// <summary>Hellgrau für die nicht markierten Einträge – der markierte hebt sich weiß davon ab (Versuch statt Hellblau).</summary>
    private static readonly Color UnselectedColor = Color.FromArgb(228, 228, 228); // etwas dunkler als die Dialogfläche; gleich der BackColor der Listen im Designer, damit auch der leere Rest der Liste so aussieht
}
