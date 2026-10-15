using PDFLight.Classes;
using PDFLight.Viewer;

namespace PDFLight.Forms;

/// <summary>Filter der Anmerkungsliste (Wunsch vom 15.10.2026 – das Filtermenü mit Untermenüs und Haken war mühsam): kleines Fenster ohne
/// Rahmen unter dem Filter-Knopf nach dem Vorbild von Acrobat – je Autor, Art und (falls bekannt) Farbe eine Schaltfläche zum
/// Umschalten. Nichts gewählt = alles sichtbar; gewählte Schaltflächen schränken je Abschnitt ein. „Anwenden“ (Enter) übernimmt,
/// „Alle anzeigen“ hebt den Filter auf, ein Klick daneben oder Esc schließt ohne Änderung – wie das Drehen-Fenster.</summary>
public partial class AnnotationFilterForm : Form
{
    private readonly Point anchor;
    private readonly bool dark;
    private readonly Color borderColor, chipBorder, chipChecked, chipHover, chipCheckedBorder;
    private bool closing;

    /// <summary>Der gewählte Filter nach „Anwenden“ bzw. „Alle anzeigen“ (DialogResult OK).</summary>
    public AnnotationFilter Result { get; private set; } = new();

    /// <param name="authors">Autoren der Anmerkungen (leer = ohne Autor).</param>
    /// <param name="types">Vorkommende Arten (FPDF_ANNOT_*) mit Namen.</param>
    /// <param name="colors">Bekannte Farben (ARGB); leer blendet den Abschnitt aus.</param>
    /// <param name="anchorPoint">Rechte untere Ecke des Filter-Knopfs (Bildschirm) – das Fenster hängt rechtsbündig darunter.</param>
    public AnnotationFilterForm(IEnumerable<string> authors, IEnumerable<(int Subtype, string Name)> types, IEnumerable<int> colors, AnnotationFilter current, Point anchorPoint, bool dark)
    {
        InitializeComponent();
        Lng.Apply(this);
        anchor = anchorPoint;
        this.dark = dark;
        BackColor = dark ? Color.FromArgb(44, 44, 44) : SystemColors.Window;
        ForeColor = dark ? ViewerStripRenderer.Foreground : SystemColors.ControlText;
        borderColor = dark ? Color.FromArgb(80, 80, 80) : Color.FromArgb(160, 160, 160);
        chipBorder = dark ? Color.FromArgb(110, 110, 110) : Color.FromArgb(175, 175, 175);
        chipChecked = dark ? Color.FromArgb(32, 72, 112) : Color.FromArgb(204, 228, 247);
        chipHover = dark ? Color.FromArgb(62, 62, 62) : Color.FromArgb(229, 243, 255);
        chipCheckedBorder = dark ? Color.FromArgb(138, 180, 248) : Color.FromArgb(0, 120, 212);
        linkShowAll.LinkColor = linkShowAll.ActiveLinkColor = dark ? Color.FromArgb(138, 180, 248) : Color.FromArgb(0, 102, 204);
        if (dark)
        {
            buttonApply.FlatStyle = FlatStyle.Flat;
            buttonApply.BackColor = ViewerStripRenderer.Background;
            buttonApply.FlatAppearance.BorderColor = Color.FromArgb(110, 110, 110);
        }
        foreach (var author in authors) { flowAuthors.Controls.Add(Chip(author.Length > 0 ? author : Lng.T("Ohne Autor"), author, current.Authors.Contains(author))); }
        foreach (var (subtype, name) in types) { flowTypes.Controls.Add(Chip(name, subtype, current.Types.Contains(subtype))); }
        foreach (var color in colors) { flowColors.Controls.Add(Chip(string.Empty, color, current.Colors.Contains(color), Color.FromArgb(color))); }
        labelColor.Visible = flowColors.Visible = flowColors.Controls.Count > 0; // Farben verrät PDFium nur selten (FPDFAnnot_GetColor)
    }

    /// <summary>Eine Schaltfläche zum Umschalten (CheckBox im Knopf-Aussehen); Farben als runde Fläche ohne Text.</summary>
    private CheckBox Chip(string text, object key, bool selected, Color? swatch = null)
    {
        CheckBox chip = new()
        {
            Appearance = Appearance.Button,
            FlatStyle = FlatStyle.Flat,
            // Breite selbst gemessen: AutoSize schnitt flache Knopf-CheckBoxen ab („wha“ statt „whapp“)
            Size = new Size(swatch == null ? TextRenderer.MeasureText(text, Font).Width + LogicalToDeviceUnits(24) : LogicalToDeviceUnits(32), LogicalToDeviceUnits(26)),
            Margin = new Padding(0, 0, LogicalToDeviceUnits(6), LogicalToDeviceUnits(6)),
            TextAlign = ContentAlignment.MiddleCenter,
            Text = text,
            Tag = key,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false,
            BackColor = BackColor,
            ForeColor = ForeColor,
            Checked = selected,
        };
        chip.FlatAppearance.CheckedBackColor = chipChecked;
        chip.FlatAppearance.MouseOverBackColor = chipHover;
        chip.FlatAppearance.MouseDownBackColor = chipChecked;
        chip.FlatAppearance.BorderColor = selected ? chipCheckedBorder : chipBorder;
        if (swatch is { } color) { chip.Image = Swatch(color); chip.ImageAlign = ContentAlignment.MiddleCenter; }
        chip.CheckedChanged += Chip_CheckedChanged;
        return chip;
    }

    private Bitmap Swatch(Color color)
    {
        var size = LogicalToDeviceUnits(16);
        Bitmap bitmap = new(size, size);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using (SolidBrush brush = new(color)) { g.FillEllipse(brush, 1, 1, size - 3, size - 3); }
        using (Pen pen = new(dark ? Color.FromArgb(150, 150, 150) : Color.FromArgb(160, 160, 160))) { g.DrawEllipse(pen, 1, 1, size - 3, size - 3); }
        return bitmap;
    }

    private void Chip_CheckedChanged(object? sender, EventArgs e)
    {
        if (sender is CheckBox chip) { chip.FlatAppearance.BorderColor = chip.Checked ? chipCheckedBorder : chipBorder; }
    }

    /// <summary>Rechtsbündig unter dem Filter-Knopf, ganz auf dem Bildschirm.</summary>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        var area = Screen.FromPoint(anchor).WorkingArea;
        Location = new Point(Math.Clamp(anchor.X - Width, area.Left, Math.Max(area.Left, area.Right - Width)), Math.Clamp(anchor.Y, area.Top, Math.Max(area.Top, area.Bottom - Height)));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using Pen pen = new(borderColor);
        e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
    }

    /// <summary>Wie ein Dropdown: Ein Klick daneben (oder ein Fensterwechsel) schließt ohne Änderung.</summary>
    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);
        if (!closing && IsHandleCreated) { closing = true; BeginInvoke(Close); }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        closing = true;
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        foreach (var chip in flowColors.Controls.OfType<CheckBox>()) { chip.Image?.Dispose(); }
        base.OnFormClosed(e);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape) { Close(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void ButtonApply_Click(object? sender, EventArgs e)
    {
        AnnotationFilter result = new();
        foreach (var chip in flowAuthors.Controls.OfType<CheckBox>().Where(c => c.Checked)) { result.Authors.Add((string)chip.Tag!); } // Tag: in Chip gesetzt
        foreach (var chip in flowTypes.Controls.OfType<CheckBox>().Where(c => c.Checked)) { result.Types.Add((int)chip.Tag!); }
        foreach (var chip in flowColors.Controls.OfType<CheckBox>().Where(c => c.Checked)) { result.Colors.Add((int)chip.Tag!); }
        Result = result;
        DialogResult = DialogResult.OK; // nicht modal: DialogResult schließt nicht von selbst
        Close();
    }

    private void LinkShowAll_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        Result = new AnnotationFilter();
        DialogResult = DialogResult.OK;
        Close();
    }
}

/// <summary>Filter der Anmerkungsliste: je Abschnitt die gewählten Werte; ein leerer Abschnitt schränkt nicht ein.</summary>
public sealed class AnnotationFilter
{
    public HashSet<string> Authors { get; } = [];
    public HashSet<int> Types { get; } = [];
    public HashSet<int> Colors { get; } = []; // ARGB

    public bool IsEmpty => Authors.Count + Types.Count + Colors.Count == 0;

    public bool Matches(AnnotationEntry entry) =>
        (Authors.Count == 0 || Authors.Contains(entry.Author))
        && (Types.Count == 0 || Types.Contains(entry.Subtype))
        && (Colors.Count == 0 || (entry.Color is { } color && Colors.Contains(color.ToArgb())));
}
