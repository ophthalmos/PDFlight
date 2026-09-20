using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Lesezeichen-Editor (nur mit „ExperimentalFeatures“ in settings.json erreichbar): die Gliederung als
/// Baum – Umbenennen (F2), Zielseite, neue Einträge (Nachbar oder Unterpunkt), Löschen, Verschieben und Ebenenwechsel. Der Baum
/// zeichnet rechts die Zielseite in Grau (Owner-Draw), damit die Beschriftungsbearbeitung nur den Titel enthält. Gearbeitet wird am
/// Modell in den Knoten-Tags; erst „Speichern“ liefert die neue Liste (<see cref="Bookmarks"/>), geschrieben wird sie vom
/// Hauptfenster.</summary>
public partial class BookmarkForm : Form
{
    private readonly int currentPage;
    private bool loading; // beim Befüllen des Seitenfelds keine Änderungen zurückschreiben

    /// <summary>Die bearbeitete Gliederung (nach „Speichern“).</summary>
    public List<Bookmark> Bookmarks { get; private set; } = [];

    public BookmarkForm(IReadOnlyList<Bookmark> bookmarks, int pageCount, int currentPage)
    {
        InitializeComponent();
        Lng.Apply(this);
        TextBoxMargins.Apply(this);
        pageCount = Math.Max(1, pageCount);
        this.currentPage = Math.Clamp(currentPage, 1, pageCount);
        numPage.Maximum = pageCount;
        buttonNew.Image = ButtonIcon(ToolbarIcons.Add);
        buttonDelete.Image = ButtonIcon(ToolbarIcons.Delete);
        buttonRename.Image = ButtonIcon(ToolbarIcons.Rename);
        treeView.BeginUpdate();
        Fill(treeView.Nodes, bookmarks);
        treeView.EndUpdate();
        if (treeView.Nodes.Count > 0) { treeView.SelectedNode = treeView.Nodes[0]; }
        UpdateButtons();
    }

    /// <summary>Menüsymbol für einen Textbutton, ein paar Pixel nach unten gerückt (wie in der Anmerkungsliste).</summary>
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

    private void UpdateButtons()
    {
        var node = treeView.SelectedNode;
        var siblings = node?.Parent?.Nodes ?? treeView.Nodes;
        buttonNewChild.Enabled = buttonRename.Enabled = buttonDelete.Enabled = numPage.Enabled = node != null;
        buttonUp.Enabled = buttonIndent.Enabled = node != null && node.Index > 0;
        buttonDown.Enabled = node != null && node.Index < siblings.Count - 1;
        buttonOutdent.Enabled = node?.Parent != null;
        loading = true;
        numPage.Value = node != null ? Math.Clamp(BookmarkOf(node).Page, (int)numPage.Minimum, (int)numPage.Maximum) : numPage.Minimum;
        loading = false;
    }

    /// <summary>Neuer Eintrag mit der angezeigten Seite als Ziel – als Nachbar hinter dem markierten oder als sein letzter Unterpunkt;
    /// die Beschriftung geht sofort in die Bearbeitung.</summary>
    private void AddNode(bool asChild)
    {
        Bookmark bookmark = new() { Title = Lng.T("Neues Lesezeichen"), Page = currentPage };
        TreeNode node = new(bookmark.Title) { Tag = bookmark, ToolTipText = PageText(bookmark) };
        var selected = treeView.SelectedNode;
        if (selected == null) { treeView.Nodes.Add(node); }
        else if (asChild) { selected.Nodes.Add(node); selected.Expand(); }
        else { (selected.Parent?.Nodes ?? treeView.Nodes).Insert(selected.Index + 1, node); }
        treeView.SelectedNode = node;
        treeView.Focus();
        UpdateButtons();
        node.BeginEdit();
    }

    /// <summary>Den markierten Knoten samt Unterpunkten an eine andere Stelle hängen; Auswahl und Aufklappzustand bleiben.</summary>
    private void MoveNode(TreeNodeCollection target, int index)
    {
        if (treeView.SelectedNode is not { } node) { return; }
        var expanded = node.IsExpanded;
        treeView.BeginUpdate();
        node.Remove();
        target.Insert(index, node);
        if (expanded) { node.Expand(); }
        node.EnsureVisible();
        treeView.SelectedNode = node;
        treeView.EndUpdate();
        treeView.Focus();
        UpdateButtons();
    }

    private void DeleteSelected()
    {
        if (treeView.SelectedNode is not { } node) { return; }
        var siblings = node.Parent?.Nodes ?? treeView.Nodes;
        var next = node.NextNode ?? node.PrevNode ?? node.Parent;
        siblings.Remove(node);
        treeView.SelectedNode = next;
        treeView.Focus();
        UpdateButtons();
    }

    // ==== Ereignisse (verdrahtet in der Designer-Datei)

    private void TreeView_AfterSelect(object? sender, TreeViewEventArgs e) => UpdateButtons();

    private void TreeView_BeforeLabelEdit(object? sender, NodeLabelEditEventArgs e)
    {
        LabelEditGuard.Pin(treeView); // WinForms-Bug: sonst droht nach der Bearbeitung ein FailFast-Absturz
    }

    private void TreeView_AfterLabelEdit(object? sender, NodeLabelEditEventArgs e)
    {
        if (e.Label == null || e.Node == null) { return; } // abgebrochen (Esc) – der Text bleibt
        var title = e.Label.Trim();
        if (title.Length == 0) { e.CancelEdit = true; return; } // leere Titel gibt es nicht
        BookmarkOf(e.Node).Title = title;
        if (title != e.Label) { e.CancelEdit = true; e.Node.Text = title; } // Randleerzeichen entfernt: den Knoten selbst beschriften
    }

    /// <summary>Titel links, Zielseite rechts in Grau; die Beschriftungsbearbeitung enthält so nur den Titel.</summary>
    private void TreeView_DrawNode(object? sender, DrawTreeNodeEventArgs e)
    {
        if (e.Node?.Tag is not Bookmark bookmark || e.Bounds.Height <= 0) { e.DrawDefault = true; return; }
        var selected = (e.State & TreeNodeStates.Selected) != 0;
        var active = selected && treeView.Focused;
        var pageWidth = LogicalToDeviceUnits(44);
        Rectangle pageBounds = new(treeView.ClientSize.Width - pageWidth - LogicalToDeviceUnits(4), e.Bounds.Y, pageWidth, e.Bounds.Height);
        Rectangle textBounds = new(e.Bounds.X, e.Bounds.Y, Math.Max(0, Math.Min(e.Bounds.Width, pageBounds.Left - e.Bounds.X)), e.Bounds.Height);
        if (selected)
        {
            using SolidBrush brush = new(active ? SystemColors.Highlight : SystemColors.ControlLight);
            e.Graphics.FillRectangle(brush, textBounds);
        }
        TextRenderer.DrawText(e.Graphics, e.Node.Text, treeView.Font, textBounds, active ? SystemColors.HighlightText : treeView.ForeColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        TextRenderer.DrawText(e.Graphics, bookmark.Page > 0 ? bookmark.Page.ToString() : "–", treeView.Font, pageBounds, SystemColors.GrayText,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.NoPrefix);
    }

    private void TreeView_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.F2: treeView.SelectedNode?.BeginEdit(); break;
            case Keys.Delete: DeleteSelected(); break;
            case Keys.Insert: AddNode(asChild: e.Shift); break;
            default: return;
        }
        e.Handled = e.SuppressKeyPress = true;
    }

    private void NumPage_ValueChanged(object? sender, EventArgs e)
    {
        if (loading || treeView.SelectedNode is not { } node) { return; }
        var bookmark = BookmarkOf(node);
        bookmark.Page = (int)numPage.Value;
        node.ToolTipText = PageText(bookmark);
        treeView.Invalidate(); // die Seitenspalte neu zeichnen
    }

    private void ButtonNew_Click(object? sender, EventArgs e) => AddNode(asChild: false);

    private void ButtonNewChild_Click(object? sender, EventArgs e) => AddNode(asChild: true);

    private void ButtonRename_Click(object? sender, EventArgs e)
    {
        treeView.Focus();
        treeView.SelectedNode?.BeginEdit();
    }

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

    private void ButtonOK_Click(object? sender, EventArgs e)
    {
        if (treeView.SelectedNode is { IsEditing: true } editing) { editing.EndEdit(cancel: false); }
        Bookmarks = Collect(treeView.Nodes);
        DialogResult = DialogResult.OK;
    }
}
