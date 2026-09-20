using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Mehrfachauswahl aus einer Dateiliste (Name und Änderungsdatum) – für „PDF-Datei anhängen“: die in anderen
/// PDFlight-Fenstern geöffneten Dateien oder die PDF-Dateien des Ordners. <see cref="SelectedFiles"/> liefert die markierten
/// Dateien in Listenreihenfolge, Doppelklick oder Enter bestätigt.</summary>
public partial class FileListForm : Form
{
    /// <summary>Die markierten Dateien in Listenreihenfolge (nach OK).</summary>
    public List<string> SelectedFiles { get; private set; } = [];

    public FileListForm(string title, string prompt, IReadOnlyList<string> files)
    {
        InitializeComponent();
        Lng.Apply(this);
        Text = title;
        labelPrompt.Text = prompt;
        listView.BeginUpdate();
        foreach (var file in files)
        {
            DateTime? modified = null;
            try { modified = File.GetLastWriteTime(file); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { } // dann ohne Datum
            ListViewItem item = new(Path.GetFileName(file)) { Tag = file, ToolTipText = file };
            item.SubItems.Add(modified?.ToString("g") ?? string.Empty);
            listView.Items.Add(item);
        }
        listView.EndUpdate();
        ListView_Resize(listView, EventArgs.Empty);
        UpdateButtons();
    }

    private void UpdateButtons() => btnOK.Enabled = listView.SelectedItems.Count > 0;

    private void Accept()
    {
        if (listView.SelectedItems.Count == 0) { return; }
        SelectedFiles = [.. listView.Items.Cast<ListViewItem>().Where(i => i.Selected).Select(i => (string)i.Tag!)]; // jeder Eintrag trägt seinen Pfad
        DialogResult = DialogResult.OK;
    }

    /// <summary>Die Namensspalte füllt die Breite, die Datumsspalte behält ihr Maß.</summary>
    private void ListView_Resize(object? sender, EventArgs e) => colName.Width = Math.Max(120, listView.ClientSize.Width - colDate.Width - 4);

    private void ListView_SelectedIndexChanged(object? sender, EventArgs e) => UpdateButtons();

    private void ListView_DoubleClick(object? sender, EventArgs e) => Accept();

    private void ListView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter) { e.Handled = e.SuppressKeyPress = true; Accept(); }
        else if (e.KeyData == (Keys.Control | Keys.A)) { foreach (ListViewItem item in listView.Items) { item.Selected = true; } e.Handled = true; }
    }

    private void BtnOK_Click(object? sender, EventArgs e) => Accept();
}
