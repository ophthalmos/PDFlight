using System.ComponentModel;

namespace PDFLight.Controls;

/// <summary>
/// Navigationsleiste (Zurück/Vor/Hoch/Verlauf) für einen FolderTreeView.
/// Ersatz für Jam.Shell.ShellHistoryToolBar (ShellBrowser.NET).
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal class FolderHistoryToolBar : ToolStrip
{
    private const int MaxHistoryEntries = 50;
    private readonly List<string> history = [];
    private int historyIndex = -1;
    private bool navigating; // verhindert, dass programmgesteuerte Navigation erneut im Verlauf landet
    private readonly ToolStripButton btnBack;
    private readonly ToolStripButton btnForward;
    private readonly ToolStripButton btnUp;
    private readonly ContextMenuStrip historyMenu = new();
    private Control dropDownAnchor;
    private FolderTreeView folderTree;

    public FolderHistoryToolBar()
    {
        GripStyle = ToolStripGripStyle.Hidden;
        LayoutStyle = ToolStripLayoutStyle.Flow;
        btnBack = new ToolStripButton("◀") { ToolTipText = "Strg+←", Enabled = false };
        btnForward = new ToolStripButton("▶") { ToolTipText = "Strg+→", Enabled = false };
        btnUp = new ToolStripButton("▲") { ToolTipText = "Strg+↑", Enabled = false };
        btnBack.Click += (sender, e) => MoveBackward();
        btnForward.Click += (sender, e) => MoveForward();
        btnUp.Click += (sender, e) => MoveUpward();
        Items.AddRange([btnBack, btnForward, btnUp]);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { historyMenu.Dispose(); }
        base.Dispose(disposing);
    }

    /// <summary>Steuerelement, unter dem das Verlaufsmenü aufklappt (z.B. der ▼-Button der PathEditBox); es wird je nach Verlauf aktiviert/deaktiviert.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Control DropDownAnchor
    {
        get => dropDownAnchor;
        set { dropDownAnchor = value; UpdateButtons(); }
    }

    /// <summary>Der Ordnerbaum, dessen Auswahl aufgezeichnet und gesteuert wird.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public FolderTreeView Tree
    {
        get => folderTree;
        set
        {
            if (folderTree == value) { return; }
            folderTree?.AfterSelect -= Tree_AfterSelect;
            folderTree = value;
            folderTree?.AfterSelect += Tree_AfterSelect;
        }
    }

    public void MoveBackward()
    {
        if (historyIndex > 0)
        {
            historyIndex--;
            NavigateTo(history[historyIndex]);
        }
    }

    public void MoveForward()
    {
        if (historyIndex < history.Count - 1)
        {
            historyIndex++;
            NavigateTo(history[historyIndex]);
        }
    }

    public void MoveUpward()
    {
        var parent = folderTree?.SelectedNode?.Parent;
        if (parent != null)
        {
            folderTree.SelectedNode = parent; // läuft über Tree_AfterSelect in den Verlauf
            parent.EnsureVisible();
        }
    }

    public void ShowDropDown()
    {
        if (history.Count == 0) { return; }
        RebuildDropDown();
        var anchor = dropDownAnchor ?? this;
        historyMenu.Show(anchor, new Point(anchor.Width, anchor.Height), ToolStripDropDownDirection.BelowLeft); // rechtsbündig unter dem Anker
    }

    private void Tree_AfterSelect(object sender, TreeViewEventArgs e)
    {
        if (navigating) { UpdateButtons(); return; }
        var path = folderTree.SelectedPath;
        if (string.IsNullOrEmpty(path) || (historyIndex >= 0 && history[historyIndex] == path)) { UpdateButtons(); return; }
        if (historyIndex < history.Count - 1) { history.RemoveRange(historyIndex + 1, history.Count - historyIndex - 1); } // Vorwärts-Verlauf verwerfen
        history.Add(path);
        if (history.Count > MaxHistoryEntries) { history.RemoveAt(0); }
        historyIndex = history.Count - 1;
        UpdateButtons();
    }

    private void NavigateTo(string path)
    {
        navigating = true;
        try { if (Directory.Exists(path)) { folderTree.SelectedPath = path; } }
        finally { navigating = false; }
        UpdateButtons();
    }

    private void RebuildDropDown()
    {
        historyMenu.Items.Clear();
        for (var i = history.Count - 1; i >= 0; i--) // neueste Einträge oben
        {
            var index = i;
            ToolStripMenuItem item = new(history[i]) { Checked = i == historyIndex };
            item.Click += (sender, e) => { historyIndex = index; NavigateTo(history[index]); };
            historyMenu.Items.Add(item);
        }
    }

    private void UpdateButtons()
    {
        btnBack.Enabled = historyIndex > 0;
        btnForward.Enabled = historyIndex < history.Count - 1;
        btnUp.Enabled = folderTree?.SelectedNode?.Parent != null;
        dropDownAnchor?.Enabled = history.Count > 0;
    }
}
