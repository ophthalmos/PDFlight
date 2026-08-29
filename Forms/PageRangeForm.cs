using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Fragt eine Seitenauswahl ab (z.B. "3" oder "2-5, 8"); optional mit Drehrichtung.</summary>
public partial class PageRangeForm : Form
{
    private readonly int pageCount;
    private readonly bool emptyMeansAll;

    /// <summary>Die gewählten Seiten, 1-basiert und sortiert.</summary>
    public List<int> SelectedPages { get; private set; } = [];

    /// <summary>+90 (rechts), -90 (links) oder 180 Grad — nur relevant, wenn der Dialog mit Drehoptionen angezeigt wird.</summary>
    public int RotationDelta => radioLeft.Checked ? -90 : radioTurn.Checked ? 180 : 90;

    public PageRangeForm(string title, int pageCount, bool emptyMeansAll, bool showRotation, string defaultRange = null, string infoText = null)
    {
        InitializeComponent();
        Lng.Apply(this);
        this.pageCount = pageCount;
        this.emptyMeansAll = emptyMeansAll;
        Text = title; // kommt bereits übersetzt vom Aufrufer
        labelPrompt.Text = string.Format(Lng.T("&Seiten (1–{0}):"), pageCount);
        labelHint.Text = Lng.T("z.B.  3   oder   2-5, 8") + (emptyMeansAll ? "   —   " + Lng.T("leer = alle Seiten") : string.Empty);
        if (!string.IsNullOrEmpty(infoText)) // optionaler Erklärtext oberhalb der Eingabe (kommt bereits übersetzt)
        {
            labelInfo.Text = infoText;
            labelInfo.Visible = true;
            var shift = labelInfo.Height + 12;
            labelPrompt.Top += shift;
            textBoxPages.Top += shift;
            labelHint.Top += shift;
            groupRotation.Top += shift;
            Height += shift;
        }
        if (!string.IsNullOrEmpty(defaultRange))
        {
            textBoxPages.Text = defaultRange; // z.B. die aktuelle Seite des Viewers
            textBoxPages.SelectAll(); // Tippen ersetzt den Vorschlag sofort
        }
        if (!showRotation)
        {
            groupRotation.Visible = false;
            Height -= groupRotation.Height + 8;
        }
    }

    private void ButtonOK_Click(object sender, EventArgs e)
    {
        var input = textBoxPages.Text.Trim();
        if (input.Length == 0 && emptyMeansAll)
        {
            SelectedPages = [.. Enumerable.Range(1, pageCount)];
            return;
        }
        var pages = PdfEditService.ParsePageRange(input, pageCount);
        if (pages == null)
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Ungültige Seitenangabe"), string.Format(Lng.T("Bitte geben Sie gültige Seiten zwischen 1 und {0} an,"), pageCount) + Environment.NewLine + Lng.T("z.B.  3  oder  2-5, 8"), TaskDialogIcon.Warning);
            DialogResult = DialogResult.None; // Dialog offen halten
            textBoxPages.SelectAll();
            textBoxPages.Focus();
            return;
        }
        SelectedPages = pages;
    }
}
