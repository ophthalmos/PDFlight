using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Verwaltet die Anmerkungen des Dokuments: listet sie mit Seite, Art, Position und Textanfang, lässt
/// Textanmerkungen bearbeiten (über den Einfügen-Dialog mit vorbelegten Werten) und beliebige Anmerkungen löschen.
/// Jede Änderung läuft über den Bearbeitungslauf des Hauptfensters (Sicherung für Rückgängig, Fehlerdialog) und
/// wird sofort gespeichert; die Liste wird danach neu gelesen.</summary>
public partial class AnnotationListForm : Form
{
    /// <summary>True, sobald mindestens eine Änderung gespeichert wurde – das Hauptfenster lädt dann neu.</summary>
    public bool Changed { get; private set; }

    /// <summary>Seite der zuletzt geänderten Anmerkung (fürs Neuladen).</summary>
    public int LastPage { get; private set; } = 1;

    private readonly string filePath;
    private readonly int pageCount;
    private readonly Func<Action, string, bool> runEdit;

    public AnnotationListForm(string filePath, int pageCount, Func<Action, string, bool> runEdit)
    {
        InitializeComponent();
        Lng.Apply(this);
        this.filePath = filePath;
        this.pageCount = pageCount;
        this.runEdit = runEdit;
        labelFileValue.Text = Path.GetFileName(filePath);
        btnEdit.Image = ToolbarIcons.MenuIcon(ToolbarIcons.Edit, this);
        btnDelete.Image = ToolbarIcons.MenuIcon(ToolbarIcons.Delete, this);
        Reload();
    }

    private AnnotationInfo? Selected => listView.SelectedItems.Count > 0 ? listView.SelectedItems[0].Tag as AnnotationInfo : null;

    private void Reload()
    {
        listView.BeginUpdate();
        listView.Items.Clear();
        try
        {
            foreach (var annotation in PdfEditService.ListAnnotations(filePath))
            {
                ListViewItem item = new(annotation.Page.ToString()) { Tag = annotation };
                item.SubItems.Add(TypeName(annotation.Subtype));
                item.SubItems.Add(string.Format(Lng.T("links {0:0.#} mm, oben {1:0.#} mm"), annotation.LeftMm, annotation.TopMm));
                item.SubItems.Add(FirstLine(annotation.Contents));
                listView.Items.Add(item);
            }
        }
        catch (Exception ex) when (PdfEditService.IsPdfReadError(ex))
        {
            TaskDlg.ErrTaskDlg(Handle, Lng.T("Die Datei kann nicht bearbeitet werden."), ex);
        }
        if (listView.Items.Count == 0) { listView.Items.Add(new ListViewItem(Lng.T("(keine Anmerkungen)")) { ForeColor = SystemColors.GrayText }); }
        else { listView.Items[0].Selected = true; }
        listView.EndUpdate();
        UpdateButtons();
    }

    private static string TypeName(string subtype) => subtype switch
    {
        "FreeText" => Lng.T("Textanmerkung"),
        "Text" => Lng.T("Haftnotiz"),
        _ => subtype, // andere Arten sprachneutral mit ihrem PDF-Namen (Highlight, Square, …)
    };

    private static string FirstLine(string contents)
    {
        var line = contents.Replace("\r", string.Empty).Split('\n')[0].Trim();
        return line.Length > 80 ? line[..79] + "…" : line;
    }

    private void UpdateButtons()
    {
        var selected = Selected;
        btnEdit.Enabled = selected?.Subtype == "FreeText"; // nur Textanmerkungen bekommen PDFlights Kasten neu gezeichnet
        btnDelete.Enabled = selected != null;
    }

    private void ListView_SelectedIndexChanged(object? sender, EventArgs e) => UpdateButtons();

    private void ListView_DoubleClick(object? sender, EventArgs e) { if (btnEdit.Enabled) { EditSelected(); } }

    private void BtnEdit_Click(object? sender, EventArgs e) => EditSelected();

    private void EditSelected()
    {
        if (Selected is not { Subtype: "FreeText" } annotation) { return; }
        using AnnotationForm dialog = new(filePath, pageCount, annotation.Page);
        dialog.Preset(annotation.Contents, annotation.LeftMm, annotation.TopMm, annotation.FontSize);
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        if (runEdit(() => PdfEditService.UpdateFreeTextAnnotation(filePath, annotation.Page, annotation.Index, dialog.AnnotationText, dialog.LeftMm, dialog.TopMm, dialog.FontSize), Lng.T("Textanmerkung")))
        {
            Changed = true;
            LastPage = annotation.Page;
            Reload();
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (Selected is not { } annotation) { return; }
        if (!TaskDlg.ConfirmTaskDlg(Handle, Lng.T("Diese Anmerkung löschen?"), TypeName(annotation.Subtype) + ", " + string.Format(Lng.T("Seite {0}"), annotation.Page) + "\n" + FirstLine(annotation.Contents))) { return; }
        if (runEdit(() => PdfEditService.DeleteAnnotation(filePath, annotation.Page, annotation.Index), Lng.T("Anmerkung löschen")))
        {
            Changed = true;
            LastPage = annotation.Page;
            Reload();
        }
    }
}
