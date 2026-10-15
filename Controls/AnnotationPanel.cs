using System.Globalization;
using PDFLight.Classes;
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
    private readonly HashSet<int> hiddenTypes = [];
    private readonly HashSet<string> hiddenAuthors = [];
    private readonly HashSet<int> hiddenColors = []; // ARGB, NoColor = ohne Farbe
    private const int NoColor = 0;
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
        hiddenTypes.Clear();
        hiddenAuthors.Clear();
        hiddenColors.Clear();
        textSearch.Text = string.Empty;
        btnSearch.Checked = false;
        SetEntries([], stillLoading: false);
    }

    /// <summary>Farben wie die übrige Seitenleiste (hell oder dunkel).</summary>
    public void ApplyTheme(Color back, Color fore, Color secondary, Color header, Color selection, Color hover, Color icon, Color accent, ToolStripRenderer renderer)
    {
        BackColor = listView.BackColor = panelSearch.BackColor = back;
        ForeColor = listView.ForeColor = fore;
        listView.SecondaryForeColor = secondary;
        listView.HeaderBackColor = header;
        listView.SelectionBackColor = selection;
        listView.HoverBackColor = hover;
        toolStripTop.Renderer = renderer;
        toolStripTop.BackColor = back;
        labelCount.ForeColor = fore;
        (iconColor, accentColor) = (icon, accent);
        UpdateIcons();
    }

    /// <summary>Symbole der oberen Zeile in der Größe der Viewer-Leiste; ein aktiver Filter oder eine Suche in der Akzentfarbe.</summary>
    private void UpdateIcons()
    {
        if (!ToolbarIcons.FontAvailable) { return; }
        var size = new Size(LogicalToDeviceUnits(16), LogicalToDeviceUnits(16));
        toolStripTop.ImageScalingSize = size;
        var filtered = hiddenTypes.Count + hiddenAuthors.Count + hiddenColors.Count > 0;
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

    private static int ColorKey(AnnotationEntry entry) => entry.Color?.ToArgb() ?? NoColor;

    private bool Matches(AnnotationEntry entry)
    {
        if (hiddenTypes.Contains(entry.Subtype) || hiddenAuthors.Contains(entry.Author) || hiddenColors.Contains(ColorKey(entry))) { return false; }
        var search = textSearch.Text.Trim();
        return search.Length == 0
            || entry.Contents.Contains(search, StringComparison.CurrentCultureIgnoreCase)
            || entry.Author.Contains(search, StringComparison.CurrentCultureIgnoreCase)
            || TypeName(entry.Subtype).Contains(search, StringComparison.CurrentCultureIgnoreCase);
    }

    private void Rebuild(bool keepPosition = false)
    {
        var shown = all.Where(Matches).ToList();
        var culture = CultureInfo.CurrentCulture;
        IEnumerable<AnnotationGroup> groups = sort switch
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

    /// <summary>Filtermenü: je Art, Autor und Farbe, die im Dokument vorkommen, ein Haken (gesetzt = sichtbar). Das Menü bleibt beim
    /// Anklicken offen, damit sich mehrere Haken in einem Zug setzen lassen.</summary>
    private void BtnFilter_DropDownOpening(object? sender, EventArgs e)
    {
        Fill(mnuFilterType, all.Select(a => a.Subtype).Distinct().OrderBy(TypeName, StringComparer.CurrentCultureIgnoreCase)
            .Select(t => (TypeName(t), (object)t, !hiddenTypes.Contains(t), (Image?)null)));
        Fill(mnuFilterAuthor, all.Select(a => a.Author).Distinct().OrderBy(a => a.Length == 0).ThenBy(a => a, StringComparer.CurrentCultureIgnoreCase)
            .Select(a => (a.Length > 0 ? a : Lng.T("Ohne Autor"), (object)a, !hiddenAuthors.Contains(a), (Image?)null)));
        Fill(mnuFilterColor, all.Select(ColorKey).Distinct().OrderBy(c => c)
            .Select(c => (c == NoColor ? Lng.T("Ohne Farbe") : "#" + (c & 0xFFFFFF).ToString("X6", CultureInfo.InvariantCulture), (object)c, !hiddenColors.Contains(c), c == NoColor ? null : (Image?)Swatch(Color.FromArgb(c)))));
        // die Farbe verrät PDFium nur bei Anmerkungen ohne eigenes Erscheinungsbild (FPDFAnnot_GetColor) – fast alle haben eins
        mnuFilterColor.Visible = all.Any(a => a.Color != null);
        mnuFilterReset.Enabled = hiddenTypes.Count + hiddenAuthors.Count + hiddenColors.Count > 0;
    }

    private void Fill(ToolStripMenuItem menu, IEnumerable<(string Text, object Key, bool Checked, Image? Image)> items)
    {
        foreach (ToolStripItem old in menu.DropDownItems) { old.Image?.Dispose(); }
        menu.DropDownItems.Clear();
        foreach (var (text, key, isChecked, image) in items)
        {
            ToolStripMenuItem item = new(text) { Tag = key, Checked = isChecked, Image = image };
            item.Click += FilterItem_Click;
            menu.DropDownItems.Add(item);
        }
        menu.Enabled = menu.DropDownItems.Count > 0;
        menu.DropDown.Closing -= FilterDropDown_Closing;
        menu.DropDown.Closing += FilterDropDown_Closing;
    }

    private Bitmap Swatch(Color color)
    {
        var size = LogicalToDeviceUnits(14);
        Bitmap bitmap = new(size, size);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using (SolidBrush brush = new(color)) { g.FillEllipse(brush, 1, 1, size - 3, size - 3); }
        using (Pen pen = new(Color.FromArgb(160, 160, 160))) { g.DrawEllipse(pen, 1, 1, size - 3, size - 3); }
        return bitmap;
    }

    private void FilterDropDown_Closing(object? sender, ToolStripDropDownClosingEventArgs e)
    {
        if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked) { e.Cancel = true; } // mehrere Haken in einem Zug
    }

    private void FilterItem_Click(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem { Tag: { } key } item) { return; }
        item.Checked = !item.Checked;
        switch (key)
        {
            case int subtype when item.OwnerItem == mnuFilterType: Toggle(hiddenTypes, subtype, item.Checked); break;
            case string author: Toggle(hiddenAuthors, author, item.Checked); break;
            case int color: Toggle(hiddenColors, color, item.Checked); break;
        }
        mnuFilterReset.Enabled = hiddenTypes.Count + hiddenAuthors.Count + hiddenColors.Count > 0;
        Rebuild();

        static void Toggle<T>(HashSet<T> hidden, T key, bool visible)
        {
            if (visible) { hidden.Remove(key); } else { hidden.Add(key); }
        }
    }

    private void MnuFilterReset_Click(object? sender, EventArgs e)
    {
        hiddenTypes.Clear();
        hiddenAuthors.Clear();
        hiddenColors.Clear();
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
