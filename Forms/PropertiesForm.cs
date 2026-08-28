using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Zeigt und bearbeitet die Dokumenteigenschaften (Metadaten) der aktuellen PDF-Datei.</summary>
internal partial class PropertiesForm : Form
{
    private readonly PdfInfo original;

    public string DocTitle => textBoxTitle.Text.Trim();
    public string DocAuthor => textBoxAuthor.Text.Trim();
    public string DocSubject => textBoxSubject.Text.Trim();
    public string DocKeywords => textBoxKeywords.Text.Trim();

    /// <summary>True, wenn der Benutzer mindestens ein Metadatum geändert hat.</summary>
    public bool InfoChanged =>
        DocTitle != (original.Title ?? string.Empty).Trim() ||
        DocAuthor != (original.Author ?? string.Empty).Trim() ||
        DocSubject != (original.Subject ?? string.Empty).Trim() ||
        DocKeywords != (original.Keywords ?? string.Empty).Trim();

    public PropertiesForm(PdfInfo info, FileInfo file)
    {
        InitializeComponent();
        original = info;
        textBoxTitle.Text = info.Title;
        textBoxAuthor.Text = info.Author;
        textBoxSubject.Text = info.Subject;
        textBoxKeywords.Text = info.Keywords;
        labelFileValue.Text = file.Name;
        labelInfoValue.Text = $"{info.PageCount} Seiten   ·   PDF {info.Version}   ·   {file.Length / 1024.0:N0} KB   ·   geändert {file.LastWriteTime:g}";
    }
}
