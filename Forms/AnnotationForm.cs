using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Fragt Text, Position und Schriftgröße für eine Textanmerkung ab (Maße in Millimetern von der linken oberen Ecke der
/// ungedrehten Seite). Die Stelle wählt man seit 1.1.0 direkt auf der angezeigten Seite (Fadenkreuz oder Kontextmenü) – der Dialog
/// kommt mit ihr vorbelegt und braucht keine Vorschau mehr (Wunsch vom 03.10.2026); die mm-Felder korrigieren sie bei Bedarf.</summary>
public partial class AnnotationForm : Form
{
    public string AnnotationText => textBoxText.Text.Trim();
    public double LeftMm => (double)numLeft.Value;
    public double TopMm => (double)numTop.Value;
    public double FontSize => (double)numSize.Value;
    internal AnnotationStyle Style => new(Borders[Math.Max(0, comboBorder.SelectedIndex)].Color, Backgrounds[Math.Max(0, comboBackground.SelectedIndex)].Color, TextColors[Math.Max(0, comboTextColor.SelectedIndex)].Color);

    // die Farbauswahlen teilt sich der Dialog mit den Stempeln (ColorPalette)
    private static (string Name, Color? Color)[] Borders => ColorPalette.Borders;
    private static (string Name, Color? Color)[] Backgrounds => ColorPalette.Backgrounds;
    private static (string Name, Color Color)[] TextColors => ColorPalette.TextColors;

    public AnnotationForm(string filePath, int pageCount, int page)
    {
        InitializeComponent();
        TextBoxMargins.Apply(this);
        Lng.Apply(this);
        page = Math.Clamp(page, 1, Math.Max(1, pageCount));
        labelFileValue.Text = Path.GetFileName(filePath);
        labelPage.Text = string.Format(Lng.T("Seite {0} von {1}"), page, pageCount);
        comboBorder.Items.AddRange([.. Borders.Select(b => (object)Lng.T(b.Name))]);
        comboBackground.Items.AddRange([.. Backgrounds.Select(bg => (object)Lng.T(bg.Name))]);
        comboTextColor.Items.AddRange([.. TextColors.Select(tc => (object)Lng.T(tc.Name))]);
        SetStyle(AnnotationStyle.Default);
    }

    /// <summary>Rahmen, Hintergrund und Schriftfarbe vorbelegen (zuletzt gewählte Werte bzw. die der bearbeiteten Anmerkung); eine
    /// fremde Farbe außerhalb der Auswahl fällt auf den ersten Eintrag zurück.</summary>
    internal void SetStyle(AnnotationStyle style)
    {
        comboBorder.SelectedIndex = ColorPalette.IndexOf(Borders, style.BorderColor);
        comboBackground.SelectedIndex = ColorPalette.IndexOf(Backgrounds, style.Background);
        comboTextColor.SelectedIndex = ColorPalette.IndexOf(TextColors, style.TextColor);
    }

    /// <summary>Die auf der Seite gewählte Stelle (mm von links/oben der ungedrehten Seite) vorbelegen.</summary>
    internal void SetPosition(double leftMm, double topMm)
    {
        numLeft.Value = Math.Clamp((decimal)Math.Round(leftMm, 1), numLeft.Minimum, numLeft.Maximum);
        numTop.Value = Math.Clamp((decimal)Math.Round(topMm, 1), numTop.Minimum, numTop.Maximum);
    }

    /// <summary>Bearbeiten einer vorhandenen Anmerkung: Werte vorbelegen und den Titel anpassen.</summary>
    internal void Preset(string text, double leftMm, double topMm, double fontSize, AnnotationStyle style)
    {
        Text = Lng.T("Textanmerkung bearbeiten");
        buttonOK.Text = Lng.T("Übernehmen"); // statt „Hinzufügen“
        SetStyle(style);
        textBoxText.Text = string.Join(Environment.NewLine, PdfEditService.SplitLines(text));
        SetPosition(leftMm, topMm);
        numSize.Value = Math.Clamp((decimal)fontSize, numSize.Minimum, numSize.Maximum);
    }

    private void ButtonOK_Click(object? sender, EventArgs e)
    {
        if (AnnotationText.Length == 0)
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Bitte gib einen Text ein."), null, TaskDialogIcon.Warning);
            textBoxText.Focus();
            return;
        }
        DialogResult = DialogResult.OK;
    }

    private void ComboBorder_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index >= 0 && e.Index < Borders.Length) { DrawColorItem(e, Borders[e.Index].Name, Borders[e.Index].Color); }
    }

    private void ComboBackground_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index >= 0 && e.Index < Backgrounds.Length) { DrawColorItem(e, Backgrounds[e.Index].Name, Backgrounds[e.Index].Color); }
    }

    private void ComboTextColor_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index >= 0 && e.Index < TextColors.Length) { DrawColorItem(e, TextColors[e.Index].Name, TextColors[e.Index].Color); }
    }

    private void DrawColorItem(DrawItemEventArgs e, string name, Color? color) => ColorPalette.DrawItem(e, name, color, Font);

    private void ColorCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        (sender as ComboBox)?.Invalidate(); // selbst gezeichnete Auswahlfelder zeigen nach Tastaturwahl sonst noch den alten Eintrag
    }
}
