using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Zeigt und bearbeitet die Dokumenteigenschaften (Metadaten) der aktuellen PDF-Datei.</summary>
internal partial class PropertiesForm : Form
{
    private readonly PdfInfo original;

    /// <summary>True, wenn „Metadaten entfernen“ gedrückt wurde: Beim Speichern werden alle Metadaten entfernt,
    /// auch die unsichtbaren (Anwendung, Produzent, Datumsangaben, XMP).</summary>
    public bool RemoveRequested { get; private set; }

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

    public PropertiesForm(PdfInfo info, FileInfo file, bool readOnly = false)
    {
        InitializeComponent();
        Lng.Apply(this);
        TextBoxMargins.Apply(this);
        original = info;
        // Bei schreibgeschützten PDF/A-Dateien nur anzeigen, nicht bearbeiten
        textBoxTitle.ReadOnly = textBoxAuthor.ReadOnly = textBoxSubject.ReadOnly = textBoxKeywords.ReadOnly = readOnly;
        buttonRemove.Enabled = !readOnly;
        textBoxTitle.Text = info.Title;
        textBoxAuthor.Text = info.Author;
        textBoxSubject.Text = info.Subject;
        textBoxKeywords.Text = info.Keywords;
        labelFileValue.Text = file.Name;
        labelInfoValue.Text = $"{info.PageCount} {Lng.T("Seiten")}   ·   PDF {info.Version}   ·   {file.Length / 1024.0:N0} KB   ·   {Lng.T("geändert")} {file.LastWriteTime:g}";
        var origin = new[]
        {
            string.IsNullOrWhiteSpace(info.Creator) ? null : Lng.T("Anwendung:") + " " + info.Creator.Trim(),
            string.IsNullOrWhiteSpace(info.Producer) ? null : Lng.T("PDF-Produzent:") + " " + info.Producer.Trim(),
        };
        labelProducerValue.Text = string.Join("   ·   ", origin.Where(s => s != null));
    }

    /// <summary>Leert die Felder und merkt sich, dass auch die unsichtbaren Metadaten weg sollen; gespeichert wird erst mit „Speichern“.</summary>
    private void ButtonRemove_Click(object? sender, EventArgs e)
    {
        RemoveRequested = true;
        textBoxTitle.Text = textBoxAuthor.Text = textBoxSubject.Text = textBoxKeywords.Text = string.Empty;
        labelProducerValue.Text = Lng.T("Anwendung, Produzent, Daten und XMP werden mit entfernt."); // kurz genug für das Label
        buttonRemove.Enabled = false;
        buttonOK.Focus();
    }
}
