using System.Globalization;
using PDFLight.Classes;
using PDFLight.Viewer;

namespace PDFLight.Controls;

/// <summary>Startseite statt der leeren Seitenansicht, solange kein Dokument angezeigt wird (Wunsch vom 23.10.2026 – nach dem Vorbild von
/// Acrobat, SumatraPDF und PDFMover): oben Suchfeld und „Dokument öffnen…“, darunter Favoriten und zuletzt geöffnete Dateien mit
/// Miniatur, Ordner, Datum und Größe (<see cref="RecentFilesView"/>), unten „Liste leeren“. Der Inhalt steht als Spalte von höchstens
/// 1000 logischen px in der Mitte (die Liste reicht bis zum Rand, ihre Bildlaufleiste steht dort). Was die Befehle tun, entscheidet das Hauptfenster – das Bedienfeld meldet sie nur weiter.</summary>
public partial class StartPage : UserControl
{
    /// <summary>Klick, Enter oder „Öffnen“ auf einer Datei.</summary>
    public event EventHandler<RecentEntry>? OpenRequested;

    /// <summary>„Dokument öffnen…“.</summary>
    public event EventHandler? OpenDialogRequested;

    /// <summary>„Im Ordner anzeigen“.</summary>
    public event EventHandler<RecentEntry>? ShowInFolderRequested;

    /// <summary>„Zu den Favoriten hinzufügen…“.</summary>
    public event EventHandler<RecentEntry>? AddFavoriteRequested;

    /// <summary>„Aus den Favoriten entfernen“.</summary>
    public event EventHandler<RecentEntry>? RemoveFavoriteRequested;

    /// <summary>„Aus der Liste entfernen“ (Entf).</summary>
    public event EventHandler<RecentEntry>? RemoveRecentRequested;

    /// <summary>„Liste leeren“.</summary>
    public event EventHandler? ClearRequested;

    /// <summary>Befehlsleiste der angehakten Dateien: „Öffnen“ (je Datei ein Fenster).</summary>
    public event EventHandler<IReadOnlyList<RecentEntry>>? OpenCheckedRequested;

    /// <summary>Befehlsleiste: „Zusammenführen…“ (neue Sammelmappe).</summary>
    public event EventHandler<IReadOnlyList<RecentEntry>>? MergeRequested;

    /// <summary>Befehlsleiste: „Aus ‚Zuletzt geöffnet‘ entfernen“ – nur die Einträge der Zuletzt-Liste, Favoriten bleiben.</summary>
    public event EventHandler<IReadOnlyList<RecentEntry>>? RemoveCheckedRequested;

    /// <summary>Eine Gruppe wurde auf- oder zugeklappt (<see cref="FavoritesCollapsed"/>, <see cref="RecentCollapsed"/>).</summary>
    public event EventHandler? CollapsedChanged;

    private bool favoritesEnabled;
    private HashSet<string> favoritePaths = new(StringComparer.OrdinalIgnoreCase);
    private Color iconColor = Color.FromArgb(64, 64, 64);

    public StartPage()
    {
        InitializeComponent();
        Lng.Apply(this);
        Lng.Apply(contextMenuEntry); // Kontextmenüs hängen nicht im Control-Baum
        listView.FavoritesHeader = Lng.T("Favoriten");
        listView.RecentHeader = Lng.T("Zuletzt geöffnet");
        listView.NameColumnHeader = Lng.T("Name");
        listView.OpenedColumnHeader = Lng.T("Geöffnet");
        listView.SizeColumnHeader = Lng.T("Größe");
        listView.NoMatchText = Lng.T("Keine Treffer");
        textSearch.PlaceholderText = TextBoxMargins.Placeholder(Lng.T("Suchen")); // Lng.Apply übersetzt nur Text-Eigenschaften
        TextBoxMargins.Keep(textSearch, 2); // Luft links und rechts, auch nach dem Rahmenwechsel in ApplyTheme (neues Handle)
        SearchClear.Attach(textSearch); // „✕“ am rechten Rand, sobald etwas eingetippt ist (seit 23.10.2026)
    }

    /// <summary>Gruppe „Favoriten“ zugeklappt (gemerkt in den Einstellungen).</summary>
    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool FavoritesCollapsed { get => listView.FavoritesCollapsed; set => listView.FavoritesCollapsed = value; }

    /// <summary>Gruppe „Zuletzt geöffnet“ zugeklappt (gemerkt in den Einstellungen).</summary>
    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool RecentCollapsed { get => listView.RecentCollapsed; set => listView.RecentCollapsed = value; }

    /// <summary>Neue Einträge (Favoriten zuerst). <paramref name="favorites"/>: die Einstellung „Favoriten“ ist an (Menüeinträge);
    /// <paramref name="historyEnabled"/>: die Zuletzt-Liste ist nicht abgeschaltet.</summary>
    internal void SetEntries(IReadOnlyList<RecentEntry> entries, bool favorites, bool historyEnabled)
    {
        favoritesEnabled = favorites;
        favoritePaths = entries.Where(e => e.Favorite).Select(e => e.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);
        listView.EmptyText = historyEnabled ? Lng.T("(keine zuletzt geöffneten Dateien)") : Lng.T("(Verlauf in den Einstellungen abgeschaltet)");
        listView.SetEntries(entries);
        var recent = entries.Count(e => !e.Favorite);
        linkClear.Text = string.Format(CultureInfo.CurrentCulture, Lng.T("Liste leeren ({0})"), recent);
        linkClear.Visible = recent > 0;
    }

    /// <summary>Die Liste bekommt den Fokus (beim Erscheinen – ↓ wählt die erste Datei, Enter öffnet).</summary>
    public void FocusList() => listView.Focus();

    /// <summary>Strg+F: ins Suchfeld.</summary>
    public void FocusSearch()
    {
        textSearch.Focus();
        textSearch.SelectAll();
    }

    /// <summary>Das Suchfeld hat den Fokus – die Bearbeitungstasten gehören dann ihm (MainForm.TextInputFocused).</summary>
    [System.ComponentModel.Browsable(false)]
    public bool SearchFocused => textSearch.Focused;

    /// <summary>Im Suchfeld steht etwas – Esc leert es dann zuerst (MainForm.ResolveEscape).</summary>
    public bool HasSearchText => textSearch.TextLength > 0;

    public void ClearSearch() => textSearch.Clear();

    /// <summary>Ausgeblendet: Suche zurücksetzen und das Nachladen der Miniaturen abbrechen.</summary>
    public void Deactivate()
    {
        listView.CancelThumbnails();
        textSearch.Clear();
        listView.ClearChecked();
    }

    /// <summary>Farben wie die Fläche der Seitenansicht (hell oder dunkel).</summary>
    public void ApplyTheme(bool dark, Color back, Color fore, Color secondary, Color selection, Color hover, Color fieldBack, Color fieldFore, Color link,
        Color accent, ToolStripRenderer renderer, Color icon)
    {
        BackColor = panelTop.BackColor = panelBottom.BackColor = listView.BackColor = toolStripActions.BackColor = back;
        ForeColor = listView.ForeColor = labelChecked.ForeColor = fore;
        listView.SecondaryForeColor = secondary;
        listView.SelectionBackColor = selection;
        listView.HoverBackColor = hover;
        listView.ThumbnailBorderColor = secondary;
        listView.AccentColor = accent;
        listView.CheckBackColor = fieldBack;
        toolStripActions.Renderer = renderer;
        foreach (ToolStripItem item in toolStripActions.Items) { item.ForeColor = fore; }
        iconColor = icon;
        UpdateActionIcons();
        textSearch.BackColor = fieldBack;
        textSearch.ForeColor = fieldFore;
        textSearch.BorderStyle = dark ? BorderStyle.FixedSingle : BorderStyle.Fixed3D; // wie das Suchfeld der Anmerkungsliste
        SearchClear.Update(textSearch); // neues Handle und neue Farben: Knopf und rechten Abstand nachführen
        linkOpen.LinkColor = linkClear.LinkColor = link;
        linkOpen.ActiveLinkColor = linkClear.ActiveLinkColor = link;
    }

    /// <summary>Symbole des Kontextmenüs (nach der Einstellung „Symbole anzeigen“ – MainForm.ApplyToolbarIcons).</summary>
    public void UpdateMenuIcons()
    {
        mnuOpen.Image = ToolbarIcons.MenuIcon(ToolbarIcons.OpenFile, this);
        mnuShowInFolder.Image = ToolbarIcons.MenuIcon(ToolbarIcons.FolderOpen, this);
        mnuAddFavorite.Image = ToolbarIcons.MenuIcon(ToolbarIcons.Favorite, this);
        mnuRemoveFavorite.Image = ToolbarIcons.MenuIcon(ToolbarIcons.Delete, this);
        mnuRemoveRecent.Image = ToolbarIcons.MenuIcon(ToolbarIcons.Delete, this);
        UpdateActionIcons();
    }

    /// <summary>Symbole der Befehlsleiste in der Farbe der Leiste (ohne Symbolschrift nur Text).</summary>
    private void UpdateActionIcons()
    {
        var show = ToolbarIcons.FontAvailable && ToolbarIcons.MenuIconsEnabled;
        var size = new Size(LogicalToDeviceUnits(16), LogicalToDeviceUnits(16));
        toolStripActions.ImageScalingSize = size;
        foreach (var (item, glyph) in new (ToolStripItem, char)[] { (btnOpenChecked, ToolbarIcons.OpenFile), (btnMergeChecked, ToolbarIcons.Attach), (btnRemoveChecked, ToolbarIcons.Delete) })
        {
            item.Image = show ? ToolbarIcons.Get(glyph, size, iconColor) : null;
            item.DisplayStyle = show ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Text;
        }
    }

    /// <summary>Befehlsleiste über der Liste, sobald etwas angehakt ist (wie Acrobats „2 ausgewählt“, aber über der Liste statt am rechten
    /// Rand – die Liste steht als Spalte in der Mitte). Ihr Platz bleibt immer reserviert, ohne Haken ist sie leer – sonst sprang die Liste
    /// beim ersten Haken nach unten (Wunsch vom 23.10.2026). Zusammenführen braucht zwei vorhandene Dateien, Entfernen einen Eintrag der Zuletzt-Liste.</summary>
    private void UpdateActions()
    {
        var checkedEntries = listView.CheckedEntries;
        foreach (ToolStripItem item in toolStripActions.Items) { item.Visible = checkedEntries.Count > 0; }
        labelChecked.Text = string.Format(CultureInfo.CurrentCulture, Lng.T("{0} ausgewählt"), checkedEntries.Count);
        var existing = ExistingPaths(checkedEntries).Count;
        btnOpenChecked.Enabled = existing > 0;
        btnMergeChecked.Enabled = existing > 1;
        btnRemoveChecked.Enabled = checkedEntries.Any(e => !e.Favorite);
    }

    /// <summary>Vorhandene Dateien der Einträge, jede nur einmal (eine Datei kann Favorit und zuletzt geöffnet sein).</summary>
    internal static List<string> ExistingPaths(IEnumerable<RecentEntry> entries) =>
        [.. entries.Where(e => e.Exists).Select(e => e.Path).Distinct(StringComparer.OrdinalIgnoreCase)];

    /// <summary>Der Inhalt als Spalte in der Mitte – auf breiten Bildschirmen stünden Datum und Größe sonst weit vom Namen entfernt.</summary>
    private void StartPage_Resize(object? sender, EventArgs e)
    {
        var side = listView.ColumnLeftFor(ClientSize.Width); // dieselbe Spalte wie die Liste
        textSearch.Left = side + LogicalToDeviceUnits(8);
        panelTop.Padding = new Padding(0, LogicalToDeviceUnits(14), side + LogicalToDeviceUnits(8), 0);
        panelBottom.Padding = new Padding(0, LogicalToDeviceUnits(10), side + LogicalToDeviceUnits(8), 0);
        toolStripActions.Padding = new Padding(side + LogicalToDeviceUnits(4), 0, side + LogicalToDeviceUnits(4), 0);
    }

    private void ListView_CheckedChanged(object? sender, EventArgs e) => UpdateActions();

    private void ListView_CollapsedChanged(object? sender, EventArgs e) => CollapsedChanged?.Invoke(this, EventArgs.Empty);

    private void BtnOpenChecked_Click(object? sender, EventArgs e) => OpenCheckedRequested?.Invoke(this, listView.CheckedEntries);

    private void BtnMergeChecked_Click(object? sender, EventArgs e) => MergeRequested?.Invoke(this, listView.CheckedEntries);

    private void BtnRemoveChecked_Click(object? sender, EventArgs e) => RemoveCheckedRequested?.Invoke(this, listView.CheckedEntries);

    private void BtnClearChecked_Click(object? sender, EventArgs e) => listView.ClearChecked();

    /// <summary>Alle Häkchen entfernen (nach einem Befehl der Leiste).</summary>
    public void ClearChecked() => listView.ClearChecked();

    // ================================================================== Befehle

    private void ListView_OpenRequested(object? sender, RecentEntry e) => OpenRequested?.Invoke(this, e);

    private void ListView_RemoveRequested(object? sender, RecentEntry e)
    {
        if (e.Favorite) { RemoveFavoriteRequested?.Invoke(this, e); } else { RemoveRecentRequested?.Invoke(this, e); }
    }

    private void ListView_LeaveUpRequested(object? sender, EventArgs e) => textSearch.Focus();

    /// <summary>In der Liste getippt: ins Suchfeld und das Zeichen anhängen (Schreibmarke ans Ende) – weitere Zeichen landen dann dort direkt.</summary>
    private void ListView_TypeAheadRequested(object? sender, char e)
    {
        textSearch.Focus();
        textSearch.AppendText(e.ToString()); // setzt die Schreibmarke ans Ende und löst TextChanged aus (Filter)
    }

    // Ablegen auf der Liste wie auf der Startseite selbst (das Hauptfenster öffnet abgelegte Dateien)
    private void ListView_DragEnter(object? sender, DragEventArgs e) => OnDragEnter(e);

    private void ListView_DragDrop(object? sender, DragEventArgs e) => OnDragDrop(e);

    private void LinkOpen_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e) => OpenDialogRequested?.Invoke(this, EventArgs.Empty);

    private void LinkClear_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e) => ClearRequested?.Invoke(this, EventArgs.Empty);

    private void TextSearch_TextChanged(object? sender, EventArgs e)
    {
        SearchClear.Update(textSearch);
        listView.SetFilter(textSearch.Text);
    }

    private void TextSearch_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Down: listView.Focus(); listView.SelectFirst(); break;
            case Keys.Enter: listView.SelectFirst(); if (listView.SelectedEntry is { } first) { OpenRequested?.Invoke(this, first); } break;
            case Keys.Escape when textSearch.TextLength > 0: textSearch.Clear(); break;
            default: return;
        }
        e.Handled = e.SuppressKeyPress = true;
    }

    private void MnuOpen_Click(object? sender, EventArgs e)
    {
        if (listView.SelectedEntry is { } entry) { OpenRequested?.Invoke(this, entry); }
    }

    private void MnuShowInFolder_Click(object? sender, EventArgs e)
    {
        if (listView.SelectedEntry is { } entry) { ShowInFolderRequested?.Invoke(this, entry); }
    }

    private void MnuAddFavorite_Click(object? sender, EventArgs e)
    {
        if (listView.SelectedEntry is { } entry) { AddFavoriteRequested?.Invoke(this, entry); }
    }

    private void MnuRemoveFavorite_Click(object? sender, EventArgs e)
    {
        if (listView.SelectedEntry is { } entry) { RemoveFavoriteRequested?.Invoke(this, entry); }
    }

    private void MnuRemoveRecent_Click(object? sender, EventArgs e)
    {
        if (listView.SelectedEntry is { } entry) { RemoveRecentRequested?.Invoke(this, entry); }
    }

    private void ContextMenuEntry_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (listView.SelectedEntry is not { } entry) { e.Cancel = true; return; } // Rechtsklick neben die Einträge
        mnuOpen.Enabled = mnuShowInFolder.Enabled = entry.Exists;
        mnuAddFavorite.Visible = favoritesEnabled && !entry.Favorite && !IsFavorite(entry.Path);
        mnuRemoveFavorite.Visible = entry.Favorite;
        mnuRemoveRecent.Visible = !entry.Favorite;
        toolStripSeparatorC1.Visible = true;
    }

    /// <summary>Steht die Datei schon unter den Favoriten (dann kein „Hinzufügen“ in ihrer Zeile der Zuletzt-Liste)?</summary>
    private bool IsFavorite(string path) => favoritePaths.Contains(path);
}
