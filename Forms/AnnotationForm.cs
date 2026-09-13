using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Fragt Text, Seite, Position und Schriftgröße für eine Textanmerkung ab (Maße in Millimetern
/// von der linken oberen Ecke der Seite).</summary>
public partial class AnnotationForm : Form
{
    public string AnnotationText => textBoxText.Text.Trim();
    public int Page => (int)numPage.Value;
    public double LeftMm => (double)numLeft.Value;
    public double TopMm => (double)numTop.Value;
    public double FontSize => (double)numSize.Value;

    public AnnotationForm(string fileName, int pageCount, int currentPage)
    {
        InitializeComponent();
        TextBoxMargins.Apply(this);
        Lng.Apply(this);
        labelFileValue.Text = fileName;
        numPage.Maximum = Math.Max(1, pageCount);
        numPage.Value = Math.Clamp(currentPage, 1, (int)numPage.Maximum); // meist soll die angezeigte Seite die Anmerkung bekommen
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
}
