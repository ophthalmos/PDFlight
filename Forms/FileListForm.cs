using PDFLight.Classes;
using PDFLight.Controls; // ShellInfo.CompareNatural

namespace PDFLight.Forms;

/// <summary>Mehrfachauswahl aus einer Dateiliste (Name und Änderungsdatum) – für „PDF-Datei anhängen“: die in anderen
/// PDFlight-Fenstern geöffneten Dateien oder die PDF-Dateien des Ordners. Ausgewählt wird per Häkchen (Klick oder Leertaste auf dem
/// fokussierten Eintrag, Strg+A alle) – die Häkchen bleiben, egal wohin Fokus und Markierung wandern (Wunsch vom 20.09.2026). Die Markierung
/// dient nur dem Verschieben: ↑/↓, Alt+Pfeil und Ziehen ändern die Reihenfolge (sie ist die Anhängereihenfolge). <see cref="SelectedFiles"/>
/// liefert die angehakten Dateien in Listenreihenfolge, Enter bestätigt.</summary>
public partial class FileListForm : Form
{
    /// <summary>Was ein Listeneintrag im Tag trägt: Pfad und Änderungsdatum (fürs Sortieren).</summary>
    private sealed record FileEntry(string Path, DateTime? Modified);

    private int sortColumn = -1; // zuletzt angeklickte Spalte; erneuter Klick kehrt die Richtung um
    private bool sortAscending = true;

    /// <summary>Die angehakten Dateien in Listenreihenfolge (nach OK).</summary>
    public List<string> SelectedFiles { get; private set; } = [];

    public FileListForm(string title, IReadOnlyList<string> files)
    {
        InitializeComponent();
        Lng.Apply(this); // der Hinweis über der Liste (Strg+Leertaste, Reihenfolge) kommt aus dem Designer
        Text = title;
        listView.BeginUpdate();
        foreach (var file in files)
        {
            DateTime? modified = null;
            try { modified = File.GetLastWriteTime(file); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { } // dann ohne Datum
            ListViewItem item = new(Path.GetFileName(file)) { Tag = new FileEntry(file, modified), ToolTipText = file };
            item.SubItems.Add(modified?.ToString("g") ?? string.Empty);
            listView.Items.Add(item);
        }
        listView.EndUpdate();
        ListView_Resize(listView, EventArgs.Empty);
        UpdateButtons();
    }

    private List<ListViewItem> Selected => [.. listView.Items.OfType<ListViewItem>().Where(i => i.Selected)]; // in Listenreihenfolge; beim Anlegen des Handles feuert ItemChecked, während die Sammlung noch Lücken (null) hat

    private void UpdateButtons()
    {
        var selected = Selected;
        btnOK.Enabled = listView.CheckedItems.Count > 0;
        btnUp.Enabled = selected.Count > 0 && selected[0].Index > 0;
        btnDown.Enabled = selected.Count > 0 && selected[^1].Index < listView.Items.Count - 1;
    }

    private void Accept()
    {
        if (listView.CheckedItems.Count == 0) { return; }
        SelectedFiles = [.. listView.Items.OfType<ListViewItem>().Where(i => i.Checked).Select(i => ((FileEntry)i.Tag!).Path)]; // jeder Eintrag trägt seinen Pfad, Listenreihenfolge
        DialogResult = DialogResult.OK;
    }

    /// <summary>Die markierten Einträge um eine Position verschieben; ein zusammenhängender Block bleibt zusammen.</summary>
    private void MoveSelected(int delta)
    {
        var selected = Selected;
        if (selected.Count == 0) { return; }
        if (delta < 0 && selected[0].Index == 0) { return; }
        if (delta > 0 && selected[^1].Index == listView.Items.Count - 1) { return; }
        listView.BeginUpdate();
        foreach (var item in delta < 0 ? selected : Enumerable.Reverse(selected))
        {
            var index = item.Index;
            listView.Items.RemoveAt(index);
            listView.Items.Insert(index + delta, item);
        }
        listView.EndUpdate();
        foreach (var item in selected) { item.Selected = true; }
        (delta < 0 ? selected[0] : selected[^1]).EnsureVisible();
        listView.Focus();
        UpdateButtons();
    }

    /// <summary>Gezogene Einträge vor dem Eintrag an der Zielposition einfügen (hinter dem letzten, wenn darunter losgelassen).</summary>
    private void MoveTo(List<ListViewItem> items, int targetIndex)
    {
        listView.BeginUpdate();
        foreach (var item in items) { if (item.Index < targetIndex) { targetIndex--; } listView.Items.Remove(item); }
        foreach (var item in items) { listView.Items.Insert(targetIndex++, item); }
        listView.EndUpdate();
        listView.SelectedItems.Clear();
        foreach (var item in items) { item.Selected = true; }
        items[0].EnsureVisible();
        UpdateButtons();
    }

    /// <summary>Nach Spalte sortieren (Name natürlich wie im Explorer, Datum zeitlich); Häkchen bleiben, die Einträge werden nur
    /// umgeordnet – kein ListViewItemSorter, der würde jedes spätere Verschieben sofort wieder einsortieren.</summary>
    private void SortBy(int column)
    {
        sortAscending = column == sortColumn ? !sortAscending : true;
        sortColumn = column;
        var items = listView.Items.OfType<ListViewItem>().ToList();
        Comparison<ListViewItem> compare = column == 1 // Tag ist bei jedem Eintrag ein FileEntry (Konstruktor)
            ? (a, b) => Nullable.Compare(((FileEntry)a.Tag!).Modified, ((FileEntry)b.Tag!).Modified)
            : (a, b) => ShellInfo.CompareNatural(a.Text, b.Text);
        items.Sort(sortAscending ? compare : (a, b) => compare(b, a));
        listView.BeginUpdate();
        listView.Items.Clear();
        listView.Items.AddRange([.. items]);
        listView.EndUpdate();
        UpdateButtons();
    }

    // ==== Ereignisse (verdrahtet in der Designer-Datei)

    private void ListView_ColumnClick(object? sender, ColumnClickEventArgs e) => SortBy(e.Column);

    /// <summary>Die Namensspalte füllt die Breite, die Datumsspalte behält ihr Maß.</summary>
    private void ListView_Resize(object? sender, EventArgs e) => colName.Width = Math.Max(120, listView.ClientSize.Width - colDate.Width - 4);

    private void ListView_SelectedIndexChanged(object? sender, EventArgs e) => UpdateButtons();

    private void ListView_ItemChecked(object? sender, ItemCheckedEventArgs e) => UpdateButtons();

    private void ListView_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyData)
        {
            case Keys.Enter: Accept(); break;
            case Keys.Control | Keys.A: foreach (ListViewItem item in listView.Items) { item.Checked = true; } break; // die Leertaste schaltet das Häkchen des fokussierten Eintrags (ListView-Standard)
            case Keys.Alt | Keys.Up: MoveSelected(-1); break;   // Strg+Pfeil bleibt dem Fokuswechsel ohne Auswahländerung vorbehalten
            case Keys.Alt | Keys.Down: MoveSelected(1); break;
            default: return;
        }
        e.Handled = e.SuppressKeyPress = true;
    }

    private void ListView_ItemDrag(object? sender, ItemDragEventArgs e)
    {
        if (listView.SelectedItems.Count > 0) { listView.DoDragDrop(Selected, DragDropEffects.Move); }
    }

    private void ListView_DragEnter(object? sender, DragEventArgs e)
    {
        e.Effect = e.Data?.GetDataPresent(typeof(List<ListViewItem>)) == true ? DragDropEffects.Move : DragDropEffects.None;
    }

    /// <summary>Einfügemarke mitführen: vor dem Eintrag unter der Maus, hinter dem letzten unterhalb der Liste.</summary>
    private void ListView_DragOver(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(typeof(List<ListViewItem>)) != true) { e.Effect = DragDropEffects.None; return; }
        e.Effect = DragDropEffects.Move;
        var point = listView.PointToClient(new Point(e.X, e.Y));
        var target = listView.GetItemAt(point.X, point.Y);
        if (target == null) { listView.InsertionMark.AppearsAfterItem = true; listView.InsertionMark.Index = listView.Items.Count - 1; }
        else { listView.InsertionMark.AppearsAfterItem = false; listView.InsertionMark.Index = target.Index; }
    }

    private void ListView_DragLeave(object? sender, EventArgs e) => listView.InsertionMark.Index = -1;

    private void ListView_DragDrop(object? sender, DragEventArgs e)
    {
        var index = listView.InsertionMark.Index;
        var after = listView.InsertionMark.AppearsAfterItem;
        listView.InsertionMark.Index = -1;
        if (e.Data?.GetData(typeof(List<ListViewItem>)) is not List<ListViewItem> items || items.Count == 0 || index < 0) { return; }
        MoveTo(items, after ? index + 1 : index);
    }

    private void BtnUp_Click(object? sender, EventArgs e) => MoveSelected(-1);

    private void BtnDown_Click(object? sender, EventArgs e) => MoveSelected(1);

    private void BtnOK_Click(object? sender, EventArgs e) => Accept();
}
