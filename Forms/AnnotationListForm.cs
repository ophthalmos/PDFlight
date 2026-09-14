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

    /// <summary>Die Anmerkung, die der Benutzer bearbeiten will – „Bearbeiten“ schließt die Liste sofort, den Dialog
    /// öffnet danach das Hauptfenster (so bleibt das Verhalten unabhängig davon, ob der Dialog bestätigt oder abgebrochen wird).</summary>
    internal AnnotationInfo? EditRequested { get; private set; }

    private readonly string filePath;
    private readonly Func<Action, string, bool> runEdit;

    public AnnotationListForm(string filePath, Func<Action, string, bool> runEdit)
    {
        InitializeComponent();
        Lng.Apply(this);
        this.filePath = filePath;
        this.runEdit = runEdit;
        labelFileValue.Text = Path.GetFileName(filePath);
        btnEdit.Image = ButtonIcon(ToolbarIcons.Edit);
        btnDelete.Image = ButtonIcon(ToolbarIcons.Delete);
        Lng.Apply(contextMenuList); // Kontextmenüs hängen nicht im Control-Baum
        editMenuItem.Image = ToolbarIcons.MenuIcon(ToolbarIcons.Edit, this);
        deleteMenuItem.Image = ToolbarIcons.MenuIcon(ToolbarIcons.Delete, this);
        Reload();
    }

    /// <summary>Menüsymbol für einen Textbutton: die Glyphe sitzt zentriert im Zeilenkasten und wirkt neben dem Text zu hoch –
    /// ein paar Pixel Luft oben rücken sie optisch auf die Textmitte.</summary>
    private Image? ButtonIcon(char glyph)
    {
        var icon = ToolbarIcons.MenuIcon(glyph, this);
        if (icon == null) { return null; }
        var shift = LogicalToDeviceUnits(3);
        Bitmap padded = new(icon.Width, icon.Height + shift);
        using var g = Graphics.FromImage(padded);
        g.DrawImageUnscaled(icon, 0, shift);
        return padded;
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
                item.SubItems.Add(string.Format(Lng.T("links {0:0.#} mm, oben {1:0.#} mm"), annotation.LeftMm, annotation.TopMm));
                // andere Arten (Haftnotiz, Markierung …) lassen sich nur löschen – sie tragen ihre Art vor dem Text
                item.SubItems.Add(annotation.Subtype == "FreeText" ? FirstLine(annotation.Contents) : TypeName(annotation.Subtype) + ": " + FirstLine(annotation.Contents));
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
        "Stamp" => Lng.T("Stempel"),
        _ => subtype, // andere Arten sprachneutral mit ihrem PDF-Namen (Highlight, Square, …)
    };

    /// <summary>Der Text einzeilig für die Liste: Zeilenumbrüche (auch die reinen CR aus Acrobat) als „ | “, lange Texte gekürzt.</summary>
    private static string FirstLine(string contents)
    {
        var line = string.Join(" | ", PdfEditService.SplitLines(contents).Select(l => l.Trim()).Where(l => l.Length > 0));
        return line.Length > 80 ? line[..79] + "…" : line;
    }

    private void UpdateButtons()
    {
        var selected = Selected;
        btnEdit.Enabled = selected?.Subtype is "FreeText" or "Stamp"; // nur eigene Arten lassen sich neu zeichnen (Stempel: anderen Palettenstempel wählen)
        btnDelete.Enabled = selected != null;
    }

    private void ListView_SelectedIndexChanged(object? sender, EventArgs e) => UpdateButtons();

    /// <summary>Kontextmenü: dieselben Befehle wie die Buttons, mit deren Freigabe (Rechtsklick markiert die Zeile bereits).</summary>
    private void ContextMenuList_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        editMenuItem.Enabled = btnEdit.Enabled;
        deleteMenuItem.Enabled = btnDelete.Enabled;
        e.Cancel = Selected == null;
    }

    private void ListView_DoubleClick(object? sender, EventArgs e) { if (btnEdit.Enabled) { EditSelected(); } }

    private void ListView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && btnEdit.Enabled) { e.Handled = e.SuppressKeyPress = true; EditSelected(); } // Enter in der Liste = Bearbeiten (wie Doppelklick)
        if (e.KeyCode == Keys.Delete && btnDelete.Enabled) { e.Handled = e.SuppressKeyPress = true; BtnDelete_Click(sender, e); } // Entf = Löschen
    }

    private void BtnEdit_Click(object? sender, EventArgs e) => EditSelected();

    private void EditSelected()
    {
        if (Selected is not { Subtype: "FreeText" or "Stamp" } annotation) { return; }
        EditRequested = annotation;
        Close();
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (Selected is not { } annotation) { return; }
        if (runEdit(() => PdfEditService.DeleteAnnotation(filePath, annotation.Page, annotation.Index, annotation.ObjectNumber), Lng.T("Anmerkung löschen")))
        {
            Changed = true;
            LastPage = annotation.Page;
            Reload();
        }
    }
}
