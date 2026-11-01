using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Globalization;
using PDFLight.Classes;
using PDFLight.Controls;

namespace PDFLight.Viewer;

/// <summary>Eine Datei der Startseite: <see cref="Title"/> = Dateiname bzw. eigener Name des Favoriten, <see cref="Location"/> = Ordner
/// bzw. bei benannten Favoriten der volle Pfad; <see cref="Opened"/> = wann zuletzt geöffnet (null bei Einträgen von vor dem 23.10.2026),
/// <see cref="Size"/> -1 bei fehlender Datei.</summary>
public sealed record RecentEntry(string Path, string Title, string Location, DateTime? Opened, long Size, DateTime LastWriteUtc, bool Exists, bool Favorite)
{
    /// <summary>Schlüssel der Miniatur im Speicher – eine geänderte Datei bekommt eine neue.</summary>
    internal string ThumbnailKey => $"{Path}|{LastWriteUtc.Ticks}|{Size}";

    /// <summary>Schlüssel für Häkchen und Auswahl: dieselbe Datei kann als Favorit und in der Zuletzt-Liste stehen.</summary>
    internal string Key => (Favorite ? "F|" : "R|") + Path.ToUpperInvariant();
}

/// <summary>Selbst gezeichnete Liste der Startseite (Wunsch vom 23.10.2026 nach dem Vorbild von Acrobats „Zuletzt verwendet“): oben die
/// Spaltenköpfe Name · Geöffnet · Größe mit einem Kästchen für alle, darunter die Gruppen „Favoriten“ und „Zuletzt geöffnet“ (per Klick auf-
/// und zuzuklappen, <see cref="CollapsedChanged"/>), je Datei ein Kästchen (beim Überfahren und sobald etwas angehakt ist), eine Miniatur der
/// ersten Seite (<see cref="RecentThumbnails"/>, nachgeladen für die sichtbaren Zeilen), Name, darunter grau der Ordner, rechts Datum und
/// Größe. Ein Klick wählt aus, ein Doppelklick öffnet (seit 23.10.2026, Wunsch Wilhelms – vorher öffnete schon ein Klick wie bei Acrobat und SumatraPDF), ein Klick ins Kästchen oder Strg+Klick hakt an; Pfeiltasten wählen, Leertaste
/// hakt an, Enter öffnet, Entf meldet <see cref="RemoveRequested"/>. Der Inhalt steht als Spalte in der Mitte, die Liste selbst reicht bis
/// zum Rand – ihre Bildlaufleiste steht dort. Aufbau wie <see cref="AttachmentListView"/>.</summary>
internal sealed class RecentFilesView : ScrollableControl
{
    private const int RowPadding = 6;      // logische px
    private const int ColumnsHeight = 30;
    private const int GroupHeight = 34;
    private const int CheckArea = 30;      // Spalte der Kästchen bzw. des Dreiecks
    private const int CheckSize = 14;
    private const int ExpanderWidth = 18;  // Dreieck der Gruppenköpfe, rechts neben ihrem Kästchen
    private const int ThumbBox = 48;
    private const int ThumbGap = 14;
    private const int ColumnGap = 24;
    private const int SideMargin = 8;
    private const int MaxColumnWidth = 1000; // der Inhalt als Spalte in der Mitte – auf breiten Bildschirmen stünden Datum und Größe sonst weit vom Namen
    private const int MinColumnMargin = 24;

    private const TextFormatFlags TextFlags = TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
    private const TextFormatFlags RightFlags = TextFormatFlags.NoPrefix | TextFormatFlags.Right | TextFormatFlags.Top | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

    private enum RowKind { Columns, Group, File }

    private sealed record Row(RowKind Kind, bool Favorite, RecentEntry? Entry = null, int Count = 0);

    private readonly List<RecentEntry> entries = [];
    private readonly List<Row> rows = [];
    private readonly HashSet<string> checkedKeys = [];
    private RecentEntry? selected;
    private int hover = -1;
    private string filter = string.Empty;
    private bool favoritesCollapsed, recentCollapsed;
    private Font? smallFont, headerFont;

    // Miniaturen: im Speicher je Schlüssel (null = es gibt keine), nachgeladen der Reihe nach
    private readonly Dictionary<string, Bitmap?> thumbnails = [];
    private readonly Queue<RecentEntry> thumbnailQueue = new();
    private readonly HashSet<string> thumbnailQueued = [];
    private bool thumbnailPumpRunning;
    private volatile int thumbnailGeneration;

    /// <summary>Klick oder Enter auf eine Datei.</summary>
    public event EventHandler<RecentEntry>? OpenRequested;

    /// <summary>Entf auf einer Datei.</summary>
    public event EventHandler<RecentEntry>? RemoveRequested;

    /// <summary>↑ in der ersten Zeile – zurück ins Suchfeld.</summary>
    public event EventHandler? LeaveUpRequested;

    /// <summary>Ein druckbares Zeichen wurde getippt – es gehört ins Suchfeld (Wunsch vom 23.10.2026, wie das Tippen auf einer Startseite im Browser).</summary>
    public event EventHandler<char>? TypeAheadRequested;

    /// <summary>Häkchen gesetzt oder entfernt (Befehlsleiste der Startseite nachführen).</summary>
    public event EventHandler? CheckedChanged;

    /// <summary>Eine Gruppe wurde auf- oder zugeklappt (das Hauptfenster merkt es sich).</summary>
    public event EventHandler? CollapsedChanged;

    public RecentFilesView()
    {
        DoubleBuffered = true;
        AutoScroll = true;
        SetStyle(ControlStyles.Selectable | ControlStyles.ResizeRedraw, true);
    }

    [Category("Darstellung"), DefaultValue(typeof(Color), "204, 228, 247"), Description("Hintergrund des gewählten bzw. angehakten Eintrags.")]
    public Color SelectionBackColor { get; set { field = value; Invalidate(); } } = Color.FromArgb(204, 228, 247);

    [Category("Darstellung"), DefaultValue(typeof(Color), "229, 243, 255"), Description("Hintergrund des Eintrags unter der Maus.")]
    public Color HoverBackColor { get; set { field = value; Invalidate(); } } = Color.FromArgb(229, 243, 255);

    [Category("Darstellung"), DefaultValue(typeof(Color), "GrayText"), Description("Farbe für Ordner, Datum, Größe und Spaltenköpfe.")]
    public Color SecondaryForeColor { get; set { field = value; Invalidate(); } } = SystemColors.GrayText;

    [Category("Darstellung"), DefaultValue(typeof(Color), "Gray"), Description("Rahmen um die Miniaturen und die Kästchen.")]
    public Color ThumbnailBorderColor { get; set { field = value; Invalidate(); } } = Color.Gray;

    [Category("Darstellung"), DefaultValue(typeof(Color), "0, 120, 212"), Description("Füllung angehakter Kästchen.")]
    public Color AccentColor { get; set { field = value; Invalidate(); } } = Color.FromArgb(0, 120, 212);

    [Category("Darstellung"), DefaultValue(typeof(Color), "White"), Description("Fläche nicht angehakter Kästchen.")]
    public Color CheckBackColor { get; set { field = value; Invalidate(); } } = Color.White;

    [Category("Darstellung"), DefaultValue(""), Description("Text, der ohne Einträge erscheint.")]
    public string EmptyText { get; set { field = value; Invalidate(); } } = string.Empty;

    [Category("Darstellung"), DefaultValue(""), Description("Text, wenn die Suche nichts findet.")]
    public string NoMatchText { get; set { field = value; Invalidate(); } } = string.Empty;

    [Category("Darstellung"), DefaultValue("Favoriten"), Description("Überschrift der Favoriten.")]
    public string FavoritesHeader { get; set { field = value; Rebuild(); } } = "Favoriten";

    [Category("Darstellung"), DefaultValue("Zuletzt geöffnet"), Description("Überschrift der zuletzt geöffneten Dateien.")]
    public string RecentHeader { get; set { field = value; Rebuild(); } } = "Zuletzt geöffnet";

    [Category("Darstellung"), DefaultValue("Name"), Description("Spaltenkopf der Namen.")]
    public string NameColumnHeader { get; set { field = value; Invalidate(); } } = "Name";

    [Category("Darstellung"), DefaultValue("Geöffnet"), Description("Spaltenkopf des Datums.")]
    public string OpenedColumnHeader { get; set { field = value; Invalidate(); } } = "Geöffnet";

    [Category("Darstellung"), DefaultValue("Größe"), Description("Spaltenkopf der Größe.")]
    public string SizeColumnHeader { get; set { field = value; Invalidate(); } } = "Größe";

    [Category("Verhalten"), DefaultValue(false), Description("Gruppe „Favoriten“ zugeklappt.")]
    public bool FavoritesCollapsed { get => favoritesCollapsed; set { favoritesCollapsed = value; Rebuild(); } }

    [Category("Verhalten"), DefaultValue(false), Description("Gruppe „Zuletzt geöffnet“ zugeklappt.")]
    public bool RecentCollapsed { get => recentCollapsed; set { recentCollapsed = value; Rebuild(); } }

    [Browsable(false)]
    public RecentEntry? SelectedEntry => selected;

    /// <summary>Die angehakten Dateien in Listenreihenfolge (Favoriten zuerst) – auch in zugeklappten Gruppen und weggefilterte.</summary>
    [Browsable(false)]
    public IReadOnlyList<RecentEntry> CheckedEntries => [.. entries.Where(e => checkedKeys.Contains(e.Key))];

    /// <summary>Neue Einträge (Favoriten zuerst); Auswahl und Häkchen bleiben bei denselben Dateien derselben Gruppe.</summary>
    public void SetEntries(IReadOnlyList<RecentEntry> value)
    {
        var old = selected?.Key;
        entries.Clear();
        entries.AddRange(value);
        var keys = entries.Select(e => e.ThumbnailKey).ToHashSet();
        foreach (var key in thumbnails.Keys.Where(k => !keys.Contains(k)).ToList()) { thumbnails[key]?.Dispose(); thumbnails.Remove(key); }
        selected = old == null ? null : entries.FirstOrDefault(e => e.Key == old);
        var before = checkedKeys.Count;
        checkedKeys.IntersectWith(entries.Select(e => e.Key));
        Rebuild();
        if (checkedKeys.Count != before) { CheckedChanged?.Invoke(this, EventArgs.Empty); }
    }

    /// <summary>Suchtext: Einträge, deren Name oder Ordner ihn enthält.</summary>
    public void SetFilter(string value)
    {
        filter = value.Trim();
        Rebuild();
        AutoScrollPosition = Point.Empty;
    }

    /// <summary>Alle Häkchen entfernen.</summary>
    public void ClearChecked()
    {
        if (checkedKeys.Count == 0) { return; }
        checkedKeys.Clear();
        Invalidate();
        CheckedChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Wählt die erste sichtbare Datei (↓ aus dem Suchfeld).</summary>
    public void SelectFirst()
    {
        if (rows.FirstOrDefault(r => r.Kind == RowKind.File)?.Entry is { } first) { Select(first); }
    }

    /// <summary>Laufendes Nachladen der Miniaturen abbrechen (Startseite ausgeblendet).</summary>
    public void CancelThumbnails()
    {
        thumbnailGeneration++;
        thumbnailQueue.Clear();
        thumbnailQueued.Clear();
    }

    private List<RecentEntry> VisibleEntries() => filter.Length == 0 ? entries : [.. entries.Where(e =>
        e.Title.Contains(filter, StringComparison.CurrentCultureIgnoreCase) || e.Location.Contains(filter, StringComparison.CurrentCultureIgnoreCase))];

    private void Rebuild()
    {
        rows.Clear();
        var visible = VisibleEntries();
        if (visible.Count > 0) { rows.Add(new Row(RowKind.Columns, false)); }
        foreach (var favorite in new[] { true, false })
        {
            var group = visible.Where(e => e.Favorite == favorite).ToList();
            if (group.Count == 0) { continue; }
            rows.Add(new Row(RowKind.Group, favorite, Count: group.Count));
            if (!IsCollapsed(favorite)) { rows.AddRange(group.Select(e => new Row(RowKind.File, favorite, e))); }
        }
        if (selected != null && !rows.Any(r => r.Entry == selected)) { selected = null; }
        hover = -1;
        AutoScrollMinSize = new Size(0, rows.Sum(RowHeight));
        Invalidate();
    }

    private bool IsCollapsed(bool favorite) => favorite ? favoritesCollapsed : recentCollapsed;

    private void ToggleGroup(bool favorite)
    {
        if (favorite) { favoritesCollapsed = !favoritesCollapsed; } else { recentCollapsed = !recentCollapsed; }
        Rebuild();
        CollapsedChanged?.Invoke(this, EventArgs.Empty);
    }

    // ================================================================== Anordnung

    private int Scale(int logical) => LogicalToDeviceUnits(logical);

    /// <summary>Linker Rand der mittigen Spalte bei dieser Breite (Gerätepixel) – die Startseite richtet Suchfeld und Links danach aus.
    /// Die Liste selbst reicht bis zum Fensterrand, damit die Bildlaufleiste dort steht (Wunsch vom 23.10.2026).</summary>
    public int ColumnLeftFor(int width) => Math.Max(Scale(MinColumnMargin), (width - Scale(MaxColumnWidth)) / 2);

    private int ColumnLeft => ColumnLeftFor(Width); // nach der ganzen Breite samt Bildlaufleiste – bündig mit Suchfeld und Links

    private int ColumnWidth => Math.Max(Scale(100), Math.Min(Width - 2 * ColumnLeft, ClientSize.Width - ColumnLeft - Scale(4)));

    private int EntryHeight => 2 * Scale(RowPadding) + Math.Max(Scale(ThumbBox), Font.Height + Scale(4) + (smallFont ?? Font).Height);

    private int RowHeight(Row row) => row.Kind switch { RowKind.Columns => Scale(ColumnsHeight), RowKind.Group => Scale(GroupHeight), _ => EntryHeight };

    private Rectangle RowBounds(int index)
    {
        var top = AutoScrollPosition.Y;
        for (var i = 0; i < index; i++) { top += RowHeight(rows[i]); }
        return new Rectangle(ColumnLeft, top, ColumnWidth, RowHeight(rows[index]));
    }

    private int RowAt(Point point)
    {
        if (point.X < ColumnLeft || point.X >= ColumnLeft + ColumnWidth) { return -1; } // neben der Spalte: keine Zeile
        var top = AutoScrollPosition.Y;
        for (var i = 0; i < rows.Count; i++)
        {
            var height = RowHeight(rows[i]);
            if (point.Y >= top && point.Y < top + height) { return i; }
            top += height;
        }
        return -1;
    }

    /// <summary>Spalten einer Zeile: Kästchen, Miniatur, Text, Datum, Größe (Datum und Größe entfallen bei schmalem Fenster).</summary>
    private (Rectangle Check, int TextLeft, int TextRight, Rectangle Date, Rectangle Size, bool Columns) RowLayout(Rectangle bounds)
    {
        var small = smallFont ?? Font;
        var check = new Rectangle(bounds.X, bounds.Y, Scale(CheckArea), bounds.Height);
        var textLeft = check.Right + Scale(ThumbBox) + Scale(ThumbGap);
        var right = bounds.Right - Scale(SideMargin);
        var sizeWidth = TextWidth("999,99 MB", small);
        var dateWidth = Math.Max(TextWidth(new DateTime(2026, 12, 28).ToString("d", CultureInfo.CurrentCulture), small), TextWidth(OpenedColumnHeader, small));
        var size = new Rectangle(right - sizeWidth, bounds.Y, sizeWidth, bounds.Height);
        var date = new Rectangle(size.Left - Scale(ColumnGap) - dateWidth, bounds.Y, dateWidth, bounds.Height);
        var columns = date.Left - textLeft > Scale(200); // schmales Fenster: nur Name und Ordner
        return (check, textLeft, columns ? date.Left - Scale(ColumnGap) : right, date, size, columns);
    }

    private void UpdateFonts()
    {
        smallFont?.Dispose();
        headerFont?.Dispose();
        smallFont = new Font(Font.FontFamily, Font.SizeInPoints * 0.93f); // bei 10,5 pt gut 9,75 pt – so groß wie die Texte der Seitenleiste (seit 23.10.2026)
        headerFont = new Font(Font.FontFamily, Font.SizeInPoints * 1.1f, FontStyle.Bold);
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        if (smallFont == null) { UpdateFonts(); Rebuild(); }
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        UpdateFonts();
        Rebuild();
    }

    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);
        Rebuild();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CancelThumbnails();
            smallFont?.Dispose();
            headerFont?.Dispose();
            foreach (var image in thumbnails.Values) { image?.Dispose(); }
            thumbnails.Clear();
        }
        base.Dispose(disposing);
    }

    // ================================================================== Zeichnen

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        if (smallFont == null) { UpdateFonts(); }
        if (rows.Count == 0)
        {
            var text = entries.Count == 0 ? EmptyText : NoMatchText;
            if (text.Length > 0) { TextRenderer.DrawText(g, text, Font, new Point(ColumnLeft + Scale(SideMargin), Scale(RowPadding)), SecondaryForeColor, TextFlags); }
            return;
        }
        var top = AutoScrollPosition.Y;
        for (var i = 0; i < rows.Count; i++)
        {
            var bounds = new Rectangle(ColumnLeft, top, ColumnWidth, RowHeight(rows[i]));
            top += bounds.Height;
            if (bounds.Bottom < e.ClipRectangle.Top) { continue; }
            if (bounds.Top > e.ClipRectangle.Bottom) { break; }
            switch (rows[i].Kind)
            {
                case RowKind.Columns: PaintColumns(g, bounds); break;
                case RowKind.Group: PaintGroup(g, rows[i], bounds, i == hover); break;
                default: PaintEntry(g, rows[i].Entry!, bounds, i == hover); break; // Dateizeilen tragen immer eine Datei
            }
        }
    }

    /// <summary>Spaltenköpfe wie bei Acrobat, links das Kästchen für alle sichtbaren Dateien.</summary>
    private void PaintColumns(Graphics g, Rectangle bounds)
    {
        var small = smallFont!; // OnPaint hat ihn angelegt
        var layout = RowLayout(bounds);
        var files = VisibleEntries();
        var count = files.Count(e => checkedKeys.Contains(e.Key));
        DrawCheckBox(g, layout.Check, count == 0 ? CheckState.Unchecked : count == files.Count ? CheckState.Checked : CheckState.Indeterminate);
        var y = bounds.Y + (bounds.Height - small.Height) / 2;
        TextRenderer.DrawText(g, NameColumnHeader, small, new Point(layout.TextLeft, y), SecondaryForeColor, TextFlags);
        if (layout.Columns)
        {
            TextRenderer.DrawText(g, OpenedColumnHeader, small, layout.Date with { Y = y, Height = small.Height }, SecondaryForeColor, RightFlags);
            TextRenderer.DrawText(g, SizeColumnHeader, small, layout.Size with { Y = y, Height = small.Height }, SecondaryForeColor, RightFlags);
        }
        using Pen pen = new(Color.FromArgb(60, SecondaryForeColor));
        g.DrawLine(pen, bounds.X, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
    }

    /// <summary>Gruppenkopf mit Dreieck zum Auf- und Zuklappen und der Anzahl.</summary>
    private void PaintGroup(Graphics g, Row row, Rectangle bounds, bool hot)
    {
        var font = headerFont!; // OnPaint hat ihn angelegt
        var y = bounds.Bottom - font.Height - Scale(6);
        if (hot) { using SolidBrush brush = new(HoverBackColor); g.FillRectangle(brush, bounds); }
        // links das Kästchen für alle Dateien dieser Gruppe (Wunsch vom 23.10.2026 – das im Spaltenkopf erfasst beide Gruppen), daneben das Dreieck
        var files = GroupEntries(row.Favorite);
        var count = files.Count(e => checkedKeys.Contains(e.Key));
        DrawCheckBox(g, new Rectangle(bounds.X, y, Scale(CheckArea), font.Height), count == 0 ? CheckState.Unchecked : count == files.Count ? CheckState.Checked : CheckState.Indeterminate);
        var arrow = new Rectangle(bounds.X + Scale(CheckArea), y, Scale(ExpanderWidth), font.Height);
        ToolbarIcons.DrawExpander(g, arrow, !IsCollapsed(row.Favorite), ForeColor, DeviceDpi / 96f);
        var text = (row.Favorite ? FavoritesHeader : RecentHeader) + $" ({row.Count})";
        TextRenderer.DrawText(g, text, font, new Point(arrow.Right, y), ForeColor, TextFlags);
    }

    private void PaintEntry(Graphics g, RecentEntry entry, Rectangle bounds, bool hot)
    {
        var isChecked = checkedKeys.Contains(entry.Key);
        var back = entry == selected || isChecked ? SelectionBackColor : hot ? HoverBackColor : Color.Empty;
        if (back != Color.Empty) { using SolidBrush brush = new(back); g.FillRectangle(brush, bounds); }
        var layout = RowLayout(bounds);
        if (hot || checkedKeys.Count > 0) { DrawCheckBox(g, layout.Check, isChecked ? CheckState.Checked : CheckState.Unchecked); } // wie Acrobat: beim Überfahren, sonst erst im Auswahlmodus
        var box = new Rectangle(layout.Check.Right, bounds.Y + (bounds.Height - Scale(ThumbBox)) / 2, Scale(ThumbBox), Scale(ThumbBox));
        PaintThumbnail(g, entry, box);

        var small = smallFont!; // OnPaint hat ihn angelegt
        var text = entry.Exists ? ForeColor : SecondaryForeColor;
        var width = Math.Max(Scale(20), layout.TextRight - layout.TextLeft);
        var textHeight = Font.Height + Scale(4) + small.Height;
        var y = bounds.Y + (bounds.Height - textHeight) / 2;
        TextRenderer.DrawText(g, Ellipsize(entry.Title, Font, width), Font, new Point(layout.TextLeft, y), text, TextFlags);
        TextRenderer.DrawText(g, Ellipsize(entry.Location, small, width), small, new Point(layout.TextLeft, y + Font.Height + Scale(4)), SecondaryForeColor, TextFlags);
        if (!layout.Columns) { return; }
        var middle = bounds.Y + (bounds.Height - small.Height) / 2;
        if (entry.Opened is { } opened)
        {
            TextRenderer.DrawText(g, opened.ToString("d", CultureInfo.CurrentCulture), small, layout.Date with { Y = middle, Height = small.Height }, SecondaryForeColor, RightFlags);
        }
        if (entry.Size >= 0) { TextRenderer.DrawText(g, FileUtil.FormatSize(entry.Size), small, layout.Size with { Y = middle, Height = small.Height }, SecondaryForeColor, RightFlags); }
    }

    /// <summary>Kästchen mittig in der Kästchenspalte: leer mit Rand, angehakt in der Akzentfarbe mit weißem Haken, teilweise mit Strich.</summary>
    private void DrawCheckBox(Graphics g, Rectangle area, CheckState state)
    {
        var size = Scale(CheckSize);
        var box = new Rectangle(area.X + (area.Width - size) / 2, area.Y + (area.Height - size) / 2, size, size);
        var smoothing = g.SmoothingMode;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (var path = RoundedRectangle(box, Scale(3)))
        {
            using SolidBrush fill = new(state == CheckState.Unchecked ? CheckBackColor : AccentColor);
            g.FillPath(fill, path);
            using Pen border = new(state == CheckState.Unchecked ? ThumbnailBorderColor : AccentColor);
            g.DrawPath(border, path);
        }
        using Pen mark = new(Color.White, Math.Max(1.5f, Scale(2))) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        if (state == CheckState.Checked)
        {
            g.DrawLines(mark, [new PointF(box.X + size * 0.22f, box.Y + size * 0.52f), new PointF(box.X + size * 0.42f, box.Y + size * 0.72f), new PointF(box.X + size * 0.78f, box.Y + size * 0.30f)]);
        }
        else if (state == CheckState.Indeterminate) { g.DrawLine(mark, box.X + size * 0.25f, box.Y + size / 2f, box.Right - size * 0.25f, box.Y + size / 2f); }
        g.SmoothingMode = smoothing;
    }

    private static GraphicsPath RoundedRectangle(Rectangle r, int radius)
    {
        var d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    /// <summary>Miniatur im Kasten eingepasst und mit dünnem Rahmen; bis sie da ist (oder ohne) das Explorer-Icon für PDF-Dateien.</summary>
    private void PaintThumbnail(Graphics g, RecentEntry entry, Rectangle box)
    {
        if (entry.Exists && !thumbnails.ContainsKey(entry.ThumbnailKey)) { RequestThumbnail(entry); }
        if (thumbnails.GetValueOrDefault(entry.ThumbnailKey) is { } image)
        {
            var scale = Math.Min(box.Width / (float)image.Width, box.Height / (float)image.Height);
            var size = new Size(Math.Max(1, (int)(image.Width * scale)), Math.Max(1, (int)(image.Height * scale)));
            var target = new Rectangle(box.X + (box.Width - size.Width) / 2, box.Y + (box.Height - size.Height) / 2, size.Width, size.Height);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(image, target);
            g.PixelOffsetMode = PixelOffsetMode.Default;
            using Pen pen = new(ThumbnailBorderColor);
            g.DrawRectangle(pen, target.X - 1, target.Y - 1, target.Width + 1, target.Height + 1);
            return;
        }
        var iconSize = Scale(32);
        if (ShellInfo.GetTypeIcon(".pdf", iconSize) is { } icon)
        {
            g.DrawImage(icon, new Rectangle(box.X + (box.Width - iconSize) / 2, box.Y + (box.Height - iconSize) / 2, iconSize, iconSize));
        }
    }

    private static int TextWidth(string text, Font font) => TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue), TextFlags).Width;

    /// <summary>Längster Anfang, der samt „…“ passt (Intervallhalbierung – Pfade können lang sein).</summary>
    private static string Ellipsize(string text, Font font, int width)
    {
        if (TextWidth(text, font) <= width) { return text; }
        var (low, high) = (0, text.Length - 1);
        while (low < high)
        {
            var middle = (low + high + 1) / 2;
            if (TextWidth(text[..middle] + "…", font) <= width) { low = middle; } else { high = middle - 1; }
        }
        return text[..low].TrimEnd() + "…";
    }

    // ================================================================== Miniaturen

    private void RequestThumbnail(RecentEntry entry)
    {
        if (!thumbnailQueued.Add(entry.ThumbnailKey)) { return; }
        thumbnailQueue.Enqueue(entry);
        if (!thumbnailPumpRunning) { _ = PumpThumbnailsAsync(); }
    }

    /// <summary>Lädt die angeforderten Miniaturen der Reihe nach – erst aus dem Zwischenspeicher, sonst über das Hilfsprogramm.</summary>
    private async Task PumpThumbnailsAsync()
    {
        thumbnailPumpRunning = true;
        try
        {
            while (thumbnailQueue.Count > 0)
            {
                var entry = thumbnailQueue.Dequeue();
                var generation = thumbnailGeneration;
                bool StillWanted() => generation == thumbnailGeneration && !IsDisposed;
                var file = new FileInfo(entry.Path);
                Bitmap? image = null;
                var known = false;
                try
                {
                    (known, image) = await Task.Run(() => RecentThumbnails.TryLoad(file));
                    if (!known && StillWanted()) { image = await RecentThumbnails.CreateAsync(file, StillWanted); known = StillWanted(); }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { known = true; } // Datei verschwunden: Icon statt Miniatur
                if (!StillWanted() || !known) { image?.Dispose(); continue; } // abgebrochen: beim nächsten Erscheinen erneut
                if (thumbnails.Remove(entry.ThumbnailKey, out var old)) { old?.Dispose(); }
                thumbnails[entry.ThumbnailKey] = image;
                Invalidate();
            }
        }
        finally { thumbnailPumpRunning = false; }
    }

    // ================================================================== Häkchen

    private void ToggleChecked(RecentEntry entry)
    {
        if (!checkedKeys.Remove(entry.Key)) { checkedKeys.Add(entry.Key); }
        Invalidate();
        CheckedChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Die sichtbaren (nicht weggefilterten) Dateien einer Gruppe – auch wenn sie zugeklappt ist.</summary>
    private List<RecentEntry> GroupEntries(bool favorite) => [.. VisibleEntries().Where(e => e.Favorite == favorite)];

    /// <summary>Kästchen der Spaltenköpfe (alle sichtbaren Dateien) bzw. eines Gruppenkopfs (die der Gruppe): anhaken – oder, wenn schon
    /// alle angehakt sind, keine.</summary>
    private void ToggleAll(List<RecentEntry>? files = null)
    {
        files ??= VisibleEntries();
        if (files.All(e => checkedKeys.Contains(e.Key))) { checkedKeys.ExceptWith(files.Select(e => e.Key)); }
        else { checkedKeys.UnionWith(files.Select(e => e.Key)); }
        Invalidate();
        CheckedChanged?.Invoke(this, EventArgs.Empty);
    }

    // ================================================================== Maus

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var index = RowAt(e.Location);
        if (index >= 0 && rows[index].Kind == RowKind.Columns && !RowLayout(RowBounds(index)).Check.Contains(e.Location)) { index = -1; }
        // Hand nur, wo ein Klick etwas auslöst: Kästchen und Gruppenköpfe – eine Datei öffnet erst der Doppelklick
        Cursor = index >= 0 && (rows[index].Kind != RowKind.File || RowLayout(RowBounds(index)).Check.Contains(e.Location)) ? Cursors.Hand : Cursors.Default;
        if (index == hover) { return; }
        hover = index;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        hover = -1;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus(); // für Tastatur und Mausrad
        if (RowAt(e.Location) is not (>= 0 and var index) || rows[index].Entry is not { } entry) { return; }
        if (e.Button is MouseButtons.Left or MouseButtons.Right) { Select(entry); } // Rechtsklick wählt vor dem Kontextmenü
        if (e.Button == MouseButtons.Left && e.Clicks == 2 && !ModifierKeys.HasFlag(Keys.Control) && !RowLayout(RowBounds(index)).Check.Contains(e.Location)) { OpenRequested?.Invoke(this, entry); }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button != MouseButtons.Left || RowAt(e.Location) is not (>= 0 and var index)) { return; }
        var row = rows[index];
        var inCheck = RowLayout(RowBounds(index)).Check.Contains(e.Location);
        switch (row.Kind)
        {
            case RowKind.Columns when inCheck: ToggleAll(); break;
            case RowKind.Group when inCheck: ToggleAll(GroupEntries(row.Favorite)); break;
            case RowKind.Group: ToggleGroup(row.Favorite); break;
            case RowKind.File when inCheck || ModifierKeys.HasFlag(Keys.Control): ToggleChecked(row.Entry!); break; // Kästchen oder Strg+Klick: anhaken statt öffnen
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        Invalidate();
    }

    protected override void OnScroll(ScrollEventArgs se)
    {
        base.OnScroll(se);
        Invalidate();
    }

    // ================================================================== Tastatur

    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Up or Keys.Down or Keys.Home or Keys.End or Keys.PageUp or Keys.PageDown or Keys.Enter or Keys.Delete or Keys.Space || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var files = rows.Where(r => r.Kind == RowKind.File).Select(r => r.Entry!).ToList();
        if (files.Count == 0) { if (e.KeyData == Keys.Up) { LeaveUpRequested?.Invoke(this, EventArgs.Empty); } return; }
        var index = selected == null ? -1 : files.IndexOf(selected);
        var page = Math.Max(1, ClientSize.Height / Math.Max(1, EntryHeight));
        switch (e.KeyData)
        {
            case Keys.Up when index <= 0: LeaveUpRequested?.Invoke(this, EventArgs.Empty); break;
            case Keys.Up: Select(files[index - 1]); break;
            case Keys.Down: Select(files[Math.Min(files.Count - 1, index + 1)]); break;
            case Keys.PageUp: Select(files[Math.Max(0, index - page)]); break;
            case Keys.PageDown: Select(files[Math.Min(files.Count - 1, Math.Max(0, index) + page)]); break;
            case Keys.Home: Select(files[0]); break;
            case Keys.End: Select(files[^1]); break;
            case Keys.Enter when selected != null: OpenRequested?.Invoke(this, selected); break;
            case Keys.Space when selected != null: ToggleChecked(selected); break;
            case Keys.Delete when selected != null: RemoveRequested?.Invoke(this, selected); break;
            default: return;
        }
        e.Handled = e.SuppressKeyPress = true;
    }

    /// <summary>Buchstaben, Ziffern und Satzzeichen gehen ans Suchfeld; die Leertaste hakt hier an (OnKeyDown), Strg-Kombinationen
    /// liefern Steuerzeichen und bleiben außen vor.</summary>
    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
        if (e.Handled || char.IsControl(e.KeyChar) || e.KeyChar == ' ') { return; }
        TypeAheadRequested?.Invoke(this, e.KeyChar);
        e.Handled = true;
    }

    // ================================================================== Auswahl

    private void Select(RecentEntry entry)
    {
        selected = entry;
        var index = rows.FindIndex(r => r.Entry == entry);
        var bounds = RowBounds(index);
        // die Köpfe darüber (Spalten, Gruppe) mit ins Bild holen, sonst stünde die erste Datei ohne sie oben
        var first = index;
        while (first > 0 && rows[first - 1].Kind != RowKind.File) { first--; }
        var top = RowBounds(first).Top;
        if (top < 0) { AutoScrollPosition = new Point(0, top - AutoScrollPosition.Y); }
        else if (bounds.Bottom > ClientSize.Height) { AutoScrollPosition = new Point(0, bounds.Bottom - AutoScrollPosition.Y - ClientSize.Height); }
        Invalidate();
    }
}
