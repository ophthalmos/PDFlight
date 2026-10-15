using System.Globalization;
using PDFLight.Classes;
using PDFLight.Viewer;

namespace PDFLight.Controls;

/// <summary>Anhänge (eingebettete Dateien) in der Seitenleiste (Wunsch vom 15.10.2026 nach dem Vorbild von Acrobats Anlagen – ersetzt den
/// Reiter „Anhänge“ im Eigenschaften-Dialog und die Büroklammer in der Statusleiste): oben Anzahl und rechts Öffnen,
/// Speichern unter und Alle speichern (bis 15.10.2026 hinter „…“), darunter die Liste mit Datei-Icons. Die Einträge liefert das Hauptfenster
/// (<see cref="SetEntries"/>); was die Befehle tun, entscheidet es ebenfalls – das Bedienfeld meldet sie nur weiter.</summary>
public partial class AttachmentPanel : UserControl
{
    private Color iconColor = Color.FromArgb(64, 64, 64);

    /// <summary>Doppelklick, Enter oder „Öffnen“.</summary>
    public event EventHandler<AttachmentEntry>? OpenRequested;

    /// <summary>„Speichern unter…“.</summary>
    public event EventHandler<AttachmentEntry>? SaveRequested;

    /// <summary>„Alle speichern…“.</summary>
    public event EventHandler? SaveAllRequested;

    public AttachmentPanel()
    {
        InitializeComponent();
        Lng.Apply(this);
        Lng.Apply(contextMenuEntry); // Kontextmenüs hängen nicht im Control-Baum
        listView.EmptyText = Lng.T("(keine Anhänge)");
        UpdateCount();
    }

    /// <summary>Alle Anhänge in Listenreihenfolge.</summary>
    [System.ComponentModel.Browsable(false)]
    internal IReadOnlyList<AttachmentEntry> Entries => listView.Entries;

    /// <summary>Die Liste bekommt den Fokus.</summary>
    public void FocusList() => listView.Focus();

    internal void SetEntries(IReadOnlyList<AttachmentEntry> entries)
    {
        listView.SetEntries(entries);
        UpdateCount();
        UpdateButtons();
    }

    /// <summary>Nach dem Hinzufügen den neuen Anhang auswählen.</summary>
    internal void SelectByName(string name) => listView.SelectByName(name);

    public void Clear() => SetEntries([]);

    private void UpdateCount()
    {
        var count = listView.Entries.Count;
        labelCount.Text = count == 1 ? Lng.T("1 Anhang") : string.Format(CultureInfo.CurrentCulture, Lng.T("{0} Anhänge"), count);
    }

    private void UpdateButtons()
    {
        var selected = listView.SelectedEntry != null;
        btnOpen.Enabled = selected && !ShellUtil.IsDangerousFileType(AttachmentFiles.SafeFileName(listView.SelectedEntry!.Name)); // gefährliche Typen nur speichern
        btnSaveAs.Enabled = selected;
        btnSaveAll.Enabled = listView.Entries.Count > 0;
    }

    /// <summary>Farben wie die übrige Seitenleiste (hell oder dunkel).</summary>
    public void ApplyTheme(Color back, Color fore, Color secondary, Color selection, Color hover, Color icon, ToolStripRenderer renderer)
    {
        BackColor = listView.BackColor = back;
        ForeColor = listView.ForeColor = fore;
        listView.SecondaryForeColor = secondary;
        listView.SelectionBackColor = selection;
        listView.HoverBackColor = hover;
        toolStripTop.Renderer = renderer;
        toolStripTop.BackColor = back;
        labelCount.ForeColor = fore;
        iconColor = icon;
        UpdateIcons();
        UpdateMenuIcons();
    }

    /// <summary>Symbole der oberen Zeile in der Größe der Viewer-Leiste.</summary>
    private void UpdateIcons()
    {
        if (!ToolbarIcons.FontAvailable) { return; }
        var size = new Size(LogicalToDeviceUnits(16), LogicalToDeviceUnits(16));
        toolStripTop.ImageScalingSize = size;
        foreach (var (item, glyph) in new (ToolStripItem, char)[] { (btnOpen, ToolbarIcons.OpenFile), (btnSaveAs, ToolbarIcons.SaveAs), (btnSaveAll, ToolbarIcons.FolderOpen) })
        {
            item.Image = ToolbarIcons.Get(glyph, size, iconColor);
            item.DisplayStyle = ToolStripItemDisplayStyle.Image;
        }
    }

    /// <summary>Symbole des Kontextmenüs (nach der Einstellung „Symbole anzeigen“ – MainForm.ApplyToolbarIcons); es bleibt hell wie alle Kontextmenüs.</summary>
    public void UpdateMenuIcons()
    {
        mnuOpen.Image = ToolbarIcons.MenuIcon(ToolbarIcons.OpenFile, this);
        mnuSaveAs.Image = ToolbarIcons.MenuIcon(ToolbarIcons.SaveAs, this);
        mnuCtxSaveAll.Image = ToolbarIcons.MenuIcon(ToolbarIcons.FolderOpen, this);
    }

    // ================================================================== Befehle

    private void ListView_SelectionChanged(object? sender, EventArgs e) => UpdateButtons();

    private void ListView_OpenRequested(object? sender, AttachmentEntry e) => OpenRequested?.Invoke(this, e);

    private void MnuOpen_Click(object? sender, EventArgs e)
    {
        if (listView.SelectedEntry is { } entry) { OpenRequested?.Invoke(this, entry); }
    }

    private void MnuSaveAs_Click(object? sender, EventArgs e)
    {
        if (listView.SelectedEntry is { } entry) { SaveRequested?.Invoke(this, entry); }
    }

    private void MnuSaveAll_Click(object? sender, EventArgs e) => SaveAllRequested?.Invoke(this, EventArgs.Empty);

    private void ContextMenuEntry_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        var selected = listView.SelectedEntry != null;
        mnuOpen.Visible = mnuSaveAs.Visible = toolStripSeparatorC1.Visible = selected; // Rechtsklick neben die Einträge: nur Alle speichern
        mnuOpen.Enabled = btnOpen.Enabled;
        mnuCtxSaveAll.Enabled = listView.Entries.Count > 0;
    }
}
