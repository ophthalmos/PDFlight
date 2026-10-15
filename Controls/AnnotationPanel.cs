using System.Globalization;
using PDFLight.Classes;
using PDFLight.Forms;
using PDFLight.Viewer;

namespace PDFLight.Controls;

/// <summary>Anmerkungen in der Seitenleiste (Wunsch vom 15.10.2026 nach dem Vorbild von Acrobats Kommentarliste – ersetzt den Dialog
/// „Anmerkungen verwalten“): oben Anzahl, Suche, Sortieren (nach Seite, Datum, Autor oder Art – zugleich die Gruppierung), Filter
/// (Art, Autor, Farbe) und „…“ (alles auf-/zuklappen, alle löschen), darunter die Liste. Die Einträge liefert das Hauptfenster
/// (<see cref="SetEntries"/>); Springen, Bearbeiten und Löschen meldet das Bedienfeld nur weiter.</summary>
public partial class AnnotationPanel : UserControl
{
    private enum SortOrder { Page, Date, Author, Type }

    private List<AnnotationEntry> all = [];
    private bool loading;
    private SortOrder sort = SortOrder.Page;
    private AnnotationFilter filter = new();
    private AnnotationFilterForm? filterForm;
    private long filterClosedAt;
    private bool dark;
    private Color iconColor = Color.FromArgb(64, 64, 64), accentColor = Color.FromArgb(0, 120, 212);

    /// <summary>Klick oder Pfeiltaste auf einen Eintrag: zur Anmerkung springen.</summary>
    public event EventHandler<AnnotationEntry>? EntryActivated;

    /// <summary>Doppelklick, Enter oder „Bearbeiten…“ (nur bei <see cref="CanEdit"/>).</summary>
    public event EventHandler<AnnotationEntry>? EditRequested;

    /// <summary>Entf oder „Löschen“.</summary>
    public event EventHandler<AnnotationEntry>? DeleteRequested;

    /// <summary>„Alle löschen…“ – die Rückfrage stellt das Hauptfenster.</summary>
    public event EventHandler? DeleteAllRequested;

    public AnnotationPanel()
    {
        InitializeComponent();
        Lng.Apply(this);
        Lng.Apply(contextMenuEntry); // Kontextmenüs hängen nicht im Control-Baum
        textSearch.PlaceholderText = Lng.T(textSearch.PlaceholderText);
        listView.TypeName = e => TypeName(e.Subtype);
        listView.EmptyText = Lng.T("(keine Anmerkungen)");
        // Haken in eigener Spalte vor den Symbolen – sonst überdeckte er das Symbol (dunkel) bzw. war kaum zu sehen (hell); das Menü des
        // Knopfs entsteht erst zur Laufzeit, der Designer kommt an die Eigenschaft nicht heran
        ((ToolStripDropDownMenu)btnSort.DropDown).ShowCheckMargin = true;
        UpdateSortChecks();
        UpdateCount();
    }

    /// <summary>Bearbeiten und Löschen erlaubt (nicht bei gesperrter Bearbeitung: PDF/A ohne Freigabe, verschlüsselt, Lesezeichen-Editor offen).</summary>
    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool Editable { get; set; } = true;

    /// <summary>Anzahl aller Anmerkungen, ungefiltert.</summary>
    [System.ComponentModel.Browsable(false)]
    public int TotalCount => all.Count;

    /// <summary>Die Liste bekommt den Fokus (Tastatur nach Strg+Umschalt+T).</summary>
    public void FocusList() => listView.Focus();

    /// <summary>Gibt es einen eigenen Bearbeiten-Dialog? Freitext und Stempel (ihre Dialoge) und Notizen (ihre Karte).</summary>
    internal static bool CanEdit(AnnotationEntry entry) => entry.Subtype is 1 or 3 or 13;

    /// <summary>Wie die Art in der Liste, im Filter und als Gruppe heißt.</summary>
    internal static string TypeName(int subtype) => subtype switch
    {
        1 => Lng.T("Notiz"),
        3 => Lng.T("Freitext"),
        4 => Lng.T("Linie"),
        5 => Lng.T("Rechteck"),
        6 => Lng.T("Ellipse"),
        7 => Lng.T("Polygon"),
        8 => Lng.T("Polylinie"),
        9 => Lng.T("Hervorhebung"),
        10 => Lng.T("Unterstreichung"),
        11 => Lng.T("Wellenlinie"),
        12 => Lng.T("Durchstreichung"),
        13 => Lng.T("Stempel"),
        14 => Lng.T("Einfügemarke"),
        15 => Lng.T("Zeichnung"),
        17 => Lng.T("Dateianhang"),
        18 => Lng.T("Ton"),
        28 => Lng.T("Schwärzung"),
        _ => Lng.T("Anmerkung"),
    };

    /// <summary>Neue Einträge. <paramref name="stillLoading"/>: weitere Seiten folgen (die Anzahl zeigt das an). Mit
    /// <paramref name="keepPosition"/> rückt nach dem Löschen der Eintrag an die Stelle des gelöschten.</summary>
    internal void SetEntries(IReadOnlyList<AnnotationEntry> entries, bool stillLoading, bool keepPosition = false)
    {
        all = [.. entries];
        loading = stillLoading;
        Rebuild(keepPosition);
    }

    /// <summary>Kein Dokument: leer, Filter und Suche zurückgesetzt.</summary>
    public void Clear()
    {
        filter = new();
        textSearch.Text = string.Empty;
        btnSearch.Checked = false;
        SetEntries([], stillLoading: false);
    }

    /// <summary>Farben wie die übrige Seitenleiste (hell oder dunkel); das Suchfeld wie das der Viewer-Leiste.</summary>
    public void ApplyTheme(bool darkTheme, Color back, Color fore, Color secondary, Color header, Color selection, Color hover, Color icon, Color accent, ToolStripRenderer renderer)
    {
        dark = darkTheme;
        textSearch.BackColor = dark ? ViewerStripRenderer.FieldBackground : SystemColors.Window;
        textSearch.ForeColor = dark ? ViewerStripRenderer.Foreground : SystemColors.WindowText;
        textSearch.BorderStyle = dark ? BorderStyle.FixedSingle : BorderStyle.Fixed3D; // der helle 3D-Rand stach im Dunkeln heraus
        BackColor = listView.BackColor = panelSearch.BackColor = back;
        ForeColor = listView.ForeColor = fore;
        listView.SecondaryForeColor = secondary;
        listView.HeaderBackColor = header;
        listView.SelectionBackColor = selection;
        listView.HoverBackColor = hover;
        toolStripTop.Renderer = renderer;
        // die Menüs der Knöpfe erbten sonst den Renderer der Seitenleiste – ohne Hervorhebung beim Überfahren; so wie die Dropdowns der Viewer-Leiste
        btnMore.DropDown.Renderer = btnSort.DropDown.Renderer = new ViewerStripRenderer(dark);
        toolStripTop.BackColor = back;
        labelCount.ForeColor = fore;
        (iconColor, accentColor) = (icon, accent);
        UpdateIcons();
        UpdateMenuIcons();
    }

    /// <summary>Symbole der oberen Zeile in der Größe der Viewer-Leiste; ein aktiver Filter oder eine Suche in der Akzentfarbe.</summary>
    private void UpdateIcons()
    {
        if (!ToolbarIcons.FontAvailable) { return; }
        var size = new Size(LogicalToDeviceUnits(16), LogicalToDeviceUnits(16));
        toolStripTop.ImageScalingSize = size;
        var filtered = !filter.IsEmpty;
        foreach (var (item, glyph, active) in new (ToolStripItem, char, bool)[]
        {
            (btnSearch, ToolbarIcons.Search, textSearch.Text.Length > 0),
            (btnSort, ToolbarIcons.Sort, false),
            (btnFilter, ToolbarIcons.Filter, filtered),
            (btnMore, ToolbarIcons.More, false),
        })
        {
            item.Image = ToolbarIcons.Get(glyph, size, active ? accentColor : iconColor);
            item.DisplayStyle = ToolStripItemDisplayStyle.Image;
        }
    }

    // ================================================================== Filtern, Sortieren, Gruppieren

    /// <summary>Menüsymbole in der Größe der übrigen Menüs (nach der Einstellung „Symbole anzeigen“ – MainForm.ApplyToolbarIcons); die
    /// Menüs der Knöpfe sind im Dunkelmodus dunkel und bekommen helle Symbole, das Kontextmenü bleibt hell wie alle Kontextmenüs.</summary>
    public void UpdateMenuIcons()
    {
        Image? DropIcon(char glyph) => dark && ToolbarIcons.MenuIconsEnabled
            ? ToolbarIcons.Get(glyph, LogicalToDeviceUnits(new Size(16, 16)), ViewerStripRenderer.Foreground) : ToolbarIcons.MenuIcon(glyph, this);
        mnuExpandAll.Image = DropIcon(ToolbarIcons.ChevronDown);
        mnuCollapseAll.Image = DropIcon(ToolbarIcons.ChevronUp);
        mnuDeleteAll.Image = DropIcon(ToolbarIcons.Clear);
        mnuSortPage.Image = DropIcon(ToolbarIcons.Page);
        mnuSortDate.Image = DropIcon(ToolbarIcons.Calendar);
        mnuSortAuthor.Image = DropIcon(ToolbarIcons.Person);
        mnuSortType.Image = DropIcon(ToolbarIcons.Tag);
        mnuCtxExpandAll.Image = ToolbarIcons.MenuIcon(ToolbarIcons.ChevronDown, this);
        mnuCtxCollapseAll.Image = ToolbarIcons.MenuIcon(ToolbarIcons.ChevronUp, this);
        mnuEdit.Image = ToolbarIcons.MenuIcon(ToolbarIcons.Edit, this);
        mnuDelete.Image = ToolbarIcons.MenuIcon(ToolbarIcons.Delete, this);
    }

    private bool Matches(AnnotationEntry entry)
    {
        if (!filter.Matches(entry)) { return false; }
        var search = textSearch.Text.Trim();
        return search.Length == 0
            || entry.Contents.Contains(search, StringComparison.CurrentCultureIgnoreCase)
            || entry.Excerpt.Contains(search, StringComparison.CurrentCultureIgnoreCase)
            || entry.Author.Contains(search, StringComparison.CurrentCultureIgnoreCase)
            || TypeName(entry.Subtype).Contains(search, StringComparison.CurrentCultureIgnoreCase);
    }

    private void Rebuild(bool keepPosition = false)
    {
        var shown = all.Where(Matches).ToList();
        var culture = CultureInfo.CurrentCulture;
        var groups = sort switch
        {
            SortOrder.Date => shown
                .GroupBy(e => e.Modified?.Date)
                .OrderByDescending(g => g.Key ?? DateTime.MinValue)
                .Select(g => new AnnotationGroup("d" + g.Key?.ToString("yyyyMMdd", CultureInfo.InvariantCulture), g.Key?.ToString("D", culture) ?? Lng.T("Ohne Datum"),
                    [.. g.OrderByDescending(e => e.Modified)])),
            SortOrder.Author => shown
                .GroupBy(e => e.Author)
                .OrderBy(g => g.Key.Length == 0).ThenBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase)
                .Select(g => new AnnotationGroup("a" + g.Key, g.Key.Length > 0 ? g.Key : Lng.T("Ohne Autor"), [.. g.OrderBy(e => e.Page).ThenBy(e => e.Number)])),
            SortOrder.Type => shown
                .GroupBy(e => TypeName(e.Subtype))
                .OrderBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase)
                .Select(g => new AnnotationGroup("t" + g.Key, g.Key, [.. g.OrderBy(e => e.Page).ThenBy(e => e.Number)])),
            _ => shown
                .GroupBy(e => e.Page)
                .OrderBy(g => g.Key)
                .Select(g => new AnnotationGroup("p" + g.Key.ToString(CultureInfo.InvariantCulture), string.Format(Lng.T("Seite {0}"), g.Key + 1), [.. g.OrderBy(e => e.Number)])),
        };
        listView.EmptyText = all.Count == 0 ? (loading ? Lng.T("Wird gelesen…") : Lng.T("(keine Anmerkungen)")) : Lng.T("Keine passenden Anmerkungen");
        listView.SetGroups([.. groups], keepPosition);
        btnFilter.Enabled = all.Count > 0 || !filter.IsEmpty;
        UpdateCount(shown.Count);
        UpdateIcons();
    }

    private void UpdateCount(int? shown = null)
    {
        var total = all.Count;
        var visible = shown ?? total;
        labelCount.Text = visible != total ? string.Format(Lng.T("{0} von {1}"), visible, total)
            : total == 1 ? Lng.T("1 Anmerkung")
            : string.Format(Lng.T("{0} Anmerkungen"), total);
        if (loading) { labelCount.Text += " …"; }
    }

    private void UpdateSortChecks()
    {
        mnuSortPage.Checked = sort == SortOrder.Page;
        mnuSortDate.Checked = sort == SortOrder.Date;
        mnuSortAuthor.Checked = sort == SortOrder.Author;
        mnuSortType.Checked = sort == SortOrder.Type;
    }

    private void MnuSort_Click(object? sender, EventArgs e)
    {
        sort = sender == mnuSortDate ? SortOrder.Date : sender == mnuSortAuthor ? SortOrder.Author : sender == mnuSortType ? SortOrder.Type : SortOrder.Page;
        UpdateSortChecks();
        Rebuild();
    }

    /// <summary>Filter-Fenster (seit 15.10.2026 statt des Menüs mit Untermenüs und Haken – Wunsch Wilhelms): je Autor, Art und bekannter
    /// Farbe eine Schaltfläche, darunter „Alle anzeigen“ und „Anwenden“. Ein zweiter Klick auf den Knopf, der das offene Fenster gerade
    /// geschlossen hat, öffnet es nicht gleich wieder.</summary>
    private void BtnFilter_Click(object? sender, EventArgs e)
    {
        if (filterForm != null || Environment.TickCount64 - filterClosedAt < 300) { return; }
        var authors = all.Select(a => a.Author).Distinct().OrderBy(a => a.Length == 0).ThenBy(a => a, StringComparer.CurrentCultureIgnoreCase);
        var types = all.Select(a => a.Subtype).Distinct().Select(t => (t, TypeName(t))).OrderBy(t => t.Item2, StringComparer.CurrentCultureIgnoreCase);
        // die Farbe verrät PDFium nur bei Anmerkungen ohne eigenes Erscheinungsbild (FPDFAnnot_GetColor) – fast alle haben eins
        var colors = all.Where(a => a.Color != null).Select(a => a.Color!.Value.ToArgb()).Distinct().OrderBy(c => c); // Where prüft Color
        var anchor = toolStripTop.PointToScreen(new Point(btnFilter.Bounds.Right, btnFilter.Bounds.Bottom));
        AnnotationFilterForm form = new(authors, types, colors, filter, anchor, dark);
        form.FormClosed += FilterForm_FormClosed;
        filterForm = form;
        form.Show(FindForm());
    }

    private void FilterForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        if (sender is not AnnotationFilterForm form) { return; }
        filterForm = null;
        filterClosedAt = Environment.TickCount64;
        if (form.DialogResult != DialogResult.OK) { return; }
        filter = form.Result;
        Rebuild();
    }

    // ================================================================== Suche

    private void BtnSearch_CheckedChanged(object? sender, EventArgs e)
    {
        panelSearch.Visible = btnSearch.Checked;
        if (btnSearch.Checked) { textSearch.Focus(); }
        else if (textSearch.Text.Length > 0) { textSearch.Text = string.Empty; } // TextChanged baut neu
    }

    private void TextSearch_TextChanged(object? sender, EventArgs e) => Rebuild();

    private void TextSearch_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape) { e.Handled = e.SuppressKeyPress = true; btnSearch.Checked = false; listView.Focus(); }
        else if (e.KeyCode is Keys.Down or Keys.Enter) { e.Handled = e.SuppressKeyPress = true; listView.Focus(); }
    }

    // ================================================================== Befehle

    private void BtnMore_DropDownOpening(object? sender, EventArgs e)
    {
        mnuExpandAll.Enabled = !listView.AllExpanded;
        mnuCollapseAll.Enabled = !listView.AllCollapsed;
        mnuDeleteAll.Enabled = Editable && all.Count > 0 && !loading;
    }

    private void ContextMenuEntry_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        var selected = listView.SelectedEntry;
        if (!listView.Entries.Any()) { e.Cancel = true; return; }
        mnuEdit.Enabled = Editable && selected != null && CanEdit(selected);
        mnuDelete.Enabled = Editable && selected != null;
        mnuCtxExpandAll.Enabled = !listView.AllExpanded;
        mnuCtxCollapseAll.Enabled = !listView.AllCollapsed;
    }

    private void MnuExpandAll_Click(object? sender, EventArgs e) => listView.ExpandAll();

    private void MnuCollapseAll_Click(object? sender, EventArgs e) => listView.CollapseAll();

    private void MnuDeleteAll_Click(object? sender, EventArgs e) => DeleteAllRequested?.Invoke(this, EventArgs.Empty);

    private void MnuEdit_Click(object? sender, EventArgs e)
    {
        if (listView.SelectedEntry is { } entry) { ListView_EditRequested(sender, entry); }
    }

    private void MnuDelete_Click(object? sender, EventArgs e)
    {
        if (listView.SelectedEntry is { } entry) { ListView_DeleteRequested(sender, entry); }
    }

    private void ListView_EntryActivated(object? sender, AnnotationEntry e) => EntryActivated?.Invoke(this, e);

    private void ListView_EditRequested(object? sender, AnnotationEntry e)
    {
        if (Editable && CanEdit(e)) { EditRequested?.Invoke(this, e); }
    }

    private void ListView_DeleteRequested(object? sender, AnnotationEntry e)
    {
        if (Editable) { DeleteRequested?.Invoke(this, e); }
    }
}
