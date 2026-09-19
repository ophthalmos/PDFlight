using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Fragt ab, ob die leere Seite vor oder nach der angezeigten Seite eingefügt wird, wahlweise mit einem Bild darauf.</summary>
public partial class InsertPageForm : Form
{
    /// <summary>True = nach der aktuellen Seite (Vorgabe), sonst davor.</summary>
    public bool After => radioAfter.Checked;

    /// <summary>Pfad der Bilddatei für die neue Seite; null = leere Seite.</summary>
    public string? ImagePath => textBoxImage.Text.Trim().Length > 0 ? textBoxImage.Text.Trim() : null;

    public InsertPageForm(int page)
    {
        InitializeComponent();
        Lng.Apply(this);
        TextBoxMargins.Apply(this);
        labelPrompt.Text = string.Format(labelPrompt.Text, page); // der Designer-Text ist der (bereits übersetzte) Formatstring
    }

    private void ButtonBrowse_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new()
        {
            Filter = Lng.T("Bilddateien") + " (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff",
            Title = Lng.T("Bild für die neue Seite wählen"),
        };
        if (dialog.ShowDialog(this) == DialogResult.OK) { textBoxImage.Text = dialog.FileName; }
    }

    private void ButtonOK_Click(object? sender, EventArgs e)
    {
        if (ImagePath is { } image && !File.Exists(image))
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Die Bilddatei wurde nicht gefunden."), image, TaskDialogIcon.Warning);
            textBoxImage.SelectAll();
            textBoxImage.Focus();
            return;
        }
        DialogResult = DialogResult.OK;
    }
}
