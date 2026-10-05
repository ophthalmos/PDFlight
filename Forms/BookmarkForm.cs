using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Lesezeichen-Editor (Bearbeiten-Menü, Strg+F2): die Gliederung als Baum, rechts Titel
/// und Zielseite des markierten Eintrags, dazu Neu (Nachbar oder Unterpunkt), Löschen, Verschieben, Ebenenwechsel und das
/// Aufklappen bis zu einer Ebene. Der Baum zeichnet sich komplett selbst (OwnerDrawAll): Pfeil (Chevron) statt Plus/Minus, Titel links, Zielseite rechts in Grau,
/// die Vorfahren des markierten Eintrags über die ganze Breite hellgrau hinterlegt. Gearbeitet wird am Modell in den Knoten-Tags;
/// erst „Speichern“ liefert die neue Liste (<see cref="Bookmarks"/>), geschrieben wird sie vom Hauptfenster.
/// Seit 04.10.2026 nicht-modal und an die Anzeige gekoppelt (Wunsch Wilhelms): Markieren eines Eintrags oder Ändern der Zielseite
/// blättert die Anzeige dorthin (<see cref="PageRequested"/>), „Angezeigte Seite“ und neue Einträge nehmen die gerade angezeigte Seite;
/// „Speichern“ meldet <see cref="SaveRequested"/> – prüfen, schreiben und schließen (<see cref="CompleteSave"/>) übernimmt das Hauptfenster.</summary>
public partial class BookmarkForm : Form
{
    private readonly Func<int> currentPage; // die gerade angezeigte Seite (1-basiert) – sie ändert sich, während der Editor offen ist
    private bool loading; // beim Befüllen der Felder keine Änderungen zurückschreiben
    private bool saved;   // vom Hauptfenster gespeichert – Schließen ohne Rückfrage

    /// <summary>Die bearbeitete Datei.</summary>
    public string FilePath { get; }

    /// <summary>Zustand der Datei beim Lesen der Gliederung – gespeichert wird nur, wenn sie seitdem unverändert ist.</summary>
    public FileStamp Stamp { get; }

    /// <summary>Etwas wurde geändert und noch nicht gespeichert.</summary>
    public bool Dirty { get; private set; }

    /// <summary>Die bearbeitete Gliederung (bei <see cref="SaveRequested"/> frisch aus dem Baum gelesen).</summary>
    public List<Bookmark> Bookmarks { get; private set; } = [];

    /// <summary>Die Anzeige soll zu dieser Seite (1-basiert) blättern.</summary>
    public event EventHandler<int>? PageRequested;

    /// <summary>„Speichern“: alle Titel sind gesetzt, <see cref="Bookmarks"/> ist aktuell.</summary>
    public event EventHandler? SaveRequested;

    public BookmarkForm(string filePath, FileStamp stamp, IReadOnlyList<Bookmark> bookmarks, int pageCount, Func<int> currentPage)
    {
        InitializeComponent();
        FilePath = filePath;
        Stamp = stamp;
        Lng.Apply(this);
        TextBoxMargins.Apply(this);
        pageCount = Math.Max(1, pageCount);
        this.currentPage = () => Math.Clamp(currentPage(), 1, pageCount);
        numPage.Maximum = pageCount;
        buttonNew.Image = ToolbarIcons.ButtonIcon(ToolbarIcons.Add, this);
        buttonDelete.Image = ToolbarIcons.ButtonIcon(ToolbarIcons.Delete, this);
        treeView.BeginUpdate();
        Fill(treeView.Nodes, bookmarks);
        treeView.EndUpdate();
        if (treeView.Nodes.Count > 0) { treeView.SelectedNode = treeView.Nodes[0]; }
        ShowSelected();
    }

    private static void Fill(TreeNodeCollection nodes, IReadOnlyList<Bookmark> bookmarks)
    {
        foreach (var bookmark in bookmarks)
        {
            var node = nodes.Add(bookmark.Title);
            node.Tag = bookmark;
            node.ToolTipText = PageText(bookmark);
            Fill(node.Nodes, bookmark.Children);
            if (bookmark.Open) { node.Expand(); }
        }
    }

    private static string PageText(Bookmark bookmark) => bookmark.Page > 0 ? string.Format(Lng.T("Seite {0}"), bookmark.Page) : Lng.T("Ohne Ziel");

    private static Bookmark BookmarkOf(TreeNode node) => (Bookmark)node.Tag!; // jeder Knoten trägt sein Lesezeichen (Fill/AddNode)

    /// <summary>Die Gliederung aus dem Baum zurücklesen: Reihenfolge, Verschachtelung und Aufklappzustand kommen aus den Knoten.</summary>
    private static List<Bookmark> Collect(TreeNodeCollection nodes)
    {
        List<Bookmark> list = [];
        foreach (TreeNode node in nodes)
        {
            var bookmark = BookmarkOf(node);
            bookmark.Open = node.IsExpanded;
            bookmark.Children.Clear();
            bookmark.Children.AddRange(Collect(node.Nodes));
            list.Add(bookmark);
        }
        return list;
    }

    private static TreeNode? FirstWithEmptyTitle(TreeNodeCollection nodes)
    {
        foreach (TreeNode node in nodes)
        {
            if (BookmarkOf(node).Title.Length == 0) { return node; }
            if (FirstWithEmptyTitle(node.Nodes) is { } inner) { return inner; }
        }
        return null;
    }

    /// <summary>Tiefe des Baums (1 = nur oberste Ebene, 0 = leer).</summary>
    private static int Depth(TreeNodeCollection nodes)
    {
        var depth = 0;
        foreach (TreeNode node in nodes) { depth = Math.Max(depth, 1 + Depth(node.Nodes)); }
        return depth;
    }

    /// <summary>Felder und Schaltflächen auf den markierten Knoten einstellen; die Ebenen-Schaltflächen nach der Tiefe des Baums.</summary>
    private void ShowSelected()
    {
        var depth = Depth(treeView.Nodes);
        buttonLevel1.Enabled = buttonLevel2.Enabled = buttonLevelAll.Enabled = depth >= 2; // mit einer Ebene gibt es nichts auf- oder zuzuklappen
        buttonLevel3.Enabled = depth >= 3;
        var node = treeView.SelectedNode;
        var siblings = node?.Parent?.Nodes ?? treeView.Nodes;
        textBoxTitle.Enabled = numPage.Enabled = buttonNewChild.Enabled = buttonDelete.Enabled = node != null;
        buttonUp.Enabled = buttonIndent.Enabled = node != null && node.Index > 0;
        buttonDown.Enabled = node != null && node.Index < siblings.Count - 1;
        buttonOutdent.Enabled = node?.Parent != null;
        loading = true;
        textBoxTitle.Text = node != null ? BookmarkOf(node).Title : string.Empty;
        numPage.Value = node != null ? Math.Clamp(BookmarkOf(node).Page, (int)numPage.Minimum, (int)numPage.Maximum) : numPage.Minimum;
        loading = false;
    }

    /// <summary>Neuer Eintrag mit der angezeigten Seite als Ziel – als Nachbar hinter dem markierten oder als sein letzter Unterpunkt;
    /// der Titel steht danach markiert im Textfeld.</summary>
    private void AddNode(bool asChild)
    {
        Bookmark bookmark = new() { Title = Lng.T("Neues Lesezeichen"), Page = currentPage() };
        Dirty = true;
        TreeNode node = new(bookmark.Title) { Tag = bookmark, ToolTipText = PageText(bookmark) };
        var selected = treeView.SelectedNode;
        if (selected == null) { treeView.Nodes.Add(node); }
        else if (asChild) { selected.Nodes.Add(node); selected.Expand(); }
        else { (selected.Parent?.Nodes ?? treeView.Nodes).Insert(selected.Index + 1, node); }
        treeView.SelectedNode = node;
        ShowSelected();
        textBoxTitle.Focus();
        textBoxTitle.SelectAll();
    }

    /// <summary>Den markierten Knoten samt Unterpunkten an eine andere Stelle hängen; Auswahl und Aufklappzustand bleiben.</summary>
    private void MoveNode(TreeNodeCollection target, int index)
    {
        if (treeView.SelectedNode is not { } node) { return; }
        Dirty = true;
        var expanded = node.IsExpanded;
        treeView.BeginUpdate();
        node.Remove();
        target.Insert(index, node);
        if (expanded) { node.Expand(); }
        node.EnsureVisible();
        treeView.SelectedNode = node;
        treeView.EndUpdate();
        treeView.Focus();
        ShowSelected();
    }

    private void DeleteSelected()
    {
        if (treeView.SelectedNode is not { } node) { return; }
        var siblings = node.Parent?.Nodes ?? treeView.Nodes;
        Dirty = true;
        var next = node.NextNode ?? node.PrevNode ?? node.Parent;
        siblings.Remove(node);
        treeView.SelectedNode = next;
        treeView.Focus();
        ShowSelected();
    }

    /// <summary>Alle Knoten zuklappen und bis zur gewünschten Ebene wieder öffnen (1 = nur die oberste Ebene sichtbar).</summary>
    private void ExpandToLevel(int level)
    {
        treeView.BeginUpdate();
        treeView.CollapseAll();
        Expand(treeView.Nodes, 1);
        treeView.SelectedNode?.EnsureVisible();
        treeView.EndUpdate();
        treeView.Focus();
        ShowSelected(); // das Zuklappen kann die Auswahl auf einen sichtbaren Vorfahren verschieben – Felder nachziehen

        void Expand(TreeNodeCollection nodes, int depth)
        {
            if (depth >= level) { return; }
            foreach (TreeNode node in nodes) { node.Expand(); Expand(node.Nodes, depth + 1); }
        }
    }

    private bool IsAncestorOfSelection(TreeNode node)
    {
        for (var current = treeView.SelectedNode?.Parent; current != null; current = current.Parent)
        {
            if (current == node) { return true; }
        }
        return false;
    }

    // ==== Ereignisse (verdrahtet in der Designer-Datei)

    private void TreeView_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        ShowSelected();
        treeView.Invalidate(); // die grau hinterlegten Vorfahren wechseln mit der Auswahl
        if (e.Node?.Tag is Bookmark { Page: > 0 } bookmark) { PageRequested?.Invoke(this, bookmark.Page); } // die Anzeige blättert mit
    }

    /// <summary>Zeile komplett selbst zeichnen: Hintergrund über die ganze Breite (Auswahl, Vorfahren der Auswahl in Hellgrau),
    /// Aufklappdreieck wie bei Edge, Titel und rechts die Zielseite.</summary>
    private void TreeView_DrawNode(object? sender, DrawTreeNodeEventArgs e)
    {
        if (e.Node?.Tag is not Bookmark bookmark || e.Bounds.Height <= 0) { e.DrawDefault = true; return; }
        var g = e.Graphics;
        var selected = e.Node == treeView.SelectedNode;
        var active = selected && treeView.Focused;
        Rectangle row = new(0, e.Bounds.Y, treeView.ClientSize.Width, e.Bounds.Height);
        var back = selected ? (active ? SystemColors.Highlight : SystemColors.ControlLight) : IsAncestorOfSelection(e.Node) ? Color.FromArgb(232, 232, 232) : treeView.BackColor;
        using (SolidBrush brush = new(back)) { g.FillRectangle(brush, row); }
        if (e.Node.Nodes.Count > 0) // Dreieck wie in der Dokumentstruktur, in der Textfarbe (Wunsch vom 05.10.2026)
        {
            ToolbarIcons.DrawExpander(g, new Rectangle(e.Node.Bounds.X - treeView.Indent, e.Bounds.Y, treeView.Indent, e.Bounds.Height), e.Node.IsExpanded,
                active ? SystemColors.HighlightText : treeView.ForeColor, DeviceDpi / 96f);
        }
        var pageWidth = LogicalToDeviceUnits(44);
        Rectangle pageBounds = new(row.Right - pageWidth - LogicalToDeviceUnits(4), e.Bounds.Y, pageWidth, e.Bounds.Height);
        Rectangle textBounds = new(e.Node.Bounds.X, e.Bounds.Y, Math.Max(0, pageBounds.Left - e.Node.Bounds.X), e.Bounds.Height);
        TextRenderer.DrawText(g, e.Node.Text, treeView.Font, textBounds, active ? SystemColors.HighlightText : treeView.ForeColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        TextRenderer.DrawText(g, bookmark.Page > 0 ? bookmark.Page.ToString() : "–", treeView.Font, pageBounds, active ? SystemColors.HighlightText : SystemColors.GrayText,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.NoPrefix);
    }

    private void TreeView_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.F2: textBoxTitle.Focus(); textBoxTitle.SelectAll(); break;
            case Keys.Delete: DeleteSelected(); break;
            case Keys.Insert: AddNode(asChild: e.Shift); break;
            default: return;
        }
        e.Handled = e.SuppressKeyPress = true;
    }

    /// <summary>Jeder Tastendruck im Titelfeld landet sofort im Knoten; gespeichert wird der Titel ohne Randleerzeichen.</summary>
    private void TextBoxTitle_TextChanged(object? sender, EventArgs e)
    {
        if (loading || treeView.SelectedNode is not { } node) { return; }
        BookmarkOf(node).Title = textBoxTitle.Text.Trim();
        Dirty = true;
        node.Text = textBoxTitle.Text;
    }

    private void NumPage_ValueChanged(object? sender, EventArgs e)
    {
        if (loading || treeView.SelectedNode is not { } node) { return; }
        var bookmark = BookmarkOf(node);
        bookmark.Page = (int)numPage.Value;
        Dirty = true;
        node.ToolTipText = PageText(bookmark);
        treeView.Invalidate(); // die Seitenspalte neu zeichnen
        PageRequested?.Invoke(this, bookmark.Page); // die neue Zielseite gleich zeigen
    }

    /// <summary>„Angezeigte Seite“: die Seite, die das Hauptfenster gerade zeigt, wird Ziel des markierten Eintrags.</summary>
    private void ButtonCurrentPage_Click(object? sender, EventArgs e)
    {
        if (treeView.SelectedNode == null) { return; }
        numPage.Value = Math.Clamp(currentPage(), (int)numPage.Minimum, (int)numPage.Maximum);
    }

    private void ButtonNew_Click(object? sender, EventArgs e) => AddNode(asChild: false);

    private void ButtonNewChild_Click(object? sender, EventArgs e) => AddNode(asChild: true);

    private void ButtonDelete_Click(object? sender, EventArgs e) => DeleteSelected();

    private void ButtonUp_Click(object? sender, EventArgs e)
    {
        if (treeView.SelectedNode is { Index: > 0 } node) { MoveNode(node.Parent?.Nodes ?? treeView.Nodes, node.Index - 1); }
    }

    private void ButtonDown_Click(object? sender, EventArgs e)
    {
        if (treeView.SelectedNode is { } node && node.Index < (node.Parent?.Nodes ?? treeView.Nodes).Count - 1) { MoveNode(node.Parent?.Nodes ?? treeView.Nodes, node.Index + 1); }
    }

    /// <summary>Ebene tiefer: der Eintrag wird letzter Unterpunkt seines Vorgängers.</summary>
    private void ButtonIndent_Click(object? sender, EventArgs e)
    {
        if (treeView.SelectedNode is { PrevNode: { } previous }) { MoveNode(previous.Nodes, previous.Nodes.Count); }
    }

    /// <summary>Ebene höher: der Eintrag rückt hinter seinen bisherigen Elternknoten.</summary>
    private void ButtonOutdent_Click(object? sender, EventArgs e)
    {
        if (treeView.SelectedNode is { Parent: { } parent }) { MoveNode(parent.Parent?.Nodes ?? treeView.Nodes, parent.Index + 1); }
    }

    private void ButtonLevel1_Click(object? sender, EventArgs e) => ExpandToLevel(1);

    private void ButtonLevel2_Click(object? sender, EventArgs e) => ExpandToLevel(2);

    private void ButtonLevel3_Click(object? sender, EventArgs e) => ExpandToLevel(3);

    private void ButtonLevelAll_Click(object? sender, EventArgs e)
    {
        treeView.ExpandAll();
        treeView.SelectedNode?.EnsureVisible();
        treeView.Focus();
        ShowSelected();
    }

    private void ButtonOK_Click(object? sender, EventArgs e)
    {
        if (FirstWithEmptyTitle(treeView.Nodes) is { } empty)
        {
            treeView.SelectedNode = empty;
            empty.EnsureVisible();
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Bitte gib für jedes Lesezeichen einen Titel ein."), null, TaskDialogIcon.Warning);
            textBoxTitle.Focus();
            return;
        }
        Bookmarks = Collect(treeView.Nodes);
        SaveRequested?.Invoke(this, EventArgs.Empty); // Dateiprüfung, Schreiben und Schließen übernimmt das Hauptfenster
    }

    /// <summary>Vom Hauptfenster nach erfolgreichem Speichern: ohne Rückfrage schließen.</summary>
    public void CompleteSave()
    {
        saved = true;
        DialogResult = DialogResult.OK;
        Close(); // nicht-modal: DialogResult allein schließt nicht
    }

    private void ButtonCancel_Click(object? sender, EventArgs e) => Close(); // nicht-modal: der DialogResult-Knopf allein schließt nicht

    /// <summary>Ungespeicherte Änderungen nicht stillschweigend verwerfen – auch nicht, wenn das Hauptfenster geschlossen wird.</summary>
    private void BookmarkForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!Dirty || saved || e.CloseReason is not (CloseReason.UserClosing or CloseReason.FormOwnerClosing)) { return; }
        if (!TaskDlg.ConfirmTaskDlg(Handle, Lng.T("Ungespeicherte Änderungen an den Lesezeichen verwerfen?"), null, TaskDialogIcon.Warning, defaultNo: true)) { e.Cancel = true; }
    }
}
