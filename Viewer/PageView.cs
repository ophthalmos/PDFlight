using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PDFLight.Viewer;

/// <summary>Fortlaufende Seitenansicht: alle Seiten untereinander, gerendert in Kacheln (nur sichtbare, auf dem PDFium-Thread). Beim
/// Zoomen steht sofort eine kleine Vorschau der Seite da, bis die scharfen Kacheln nachkommen. Text lässt sich markieren (Ziehen,
/// Doppelklick = Wort, Strg+A = alles) und kopieren (Strg+C).
/// Koordinaten: „Dokument“ = die ganze Fläche in Gerätepixeln (Bildlauf: Client = Dokument + AutoScrollPosition), „Seitenpixel“ =
/// die Seite als Bild in der aktuellen Zoomstufe, Ursprung links oben.</summary>
internal sealed class PageView : ScrollableControl
{
    private const int PageGap = 12, PageMargin = 16;   // logische px
    private const int TileSize = 512;                  // Gerätepixel
    private const int TileLimit = 96, PreviewLimit = 80;
    private const int PreviewWidth = 360;              // Vorschaubild je Seite, Gerätepixel
    private static readonly float[] ZoomSteps = [0.25f, 0.33f, 0.5f, 0.67f, 0.75f, 0.8f, 0.9f, 1f, 1.1f, 1.25f, 1.5f, 1.75f, 2f, 2.5f, 3f, 4f, 5f];
    private static readonly Color SelectionColor = Color.FromArgb(90, 0, 120, 215);

    private PdfiumDocument? document;
    private float zoom = 1f;
    private PageFit fit = PageFit.Height;
    private Rectangle[] pageRects = [];                // Dokumentkoordinaten
    private int generation;                            // neue Datei oder Zoomstufe: laufende Kachelaufträge verfallen
    private int documentGeneration;                    // neue Datei: auch Vorschaubilder und Textdaten verfallen
    private readonly Dictionary<(int Page, int X, int Y), Bitmap> tiles = [];
    private readonly Dictionary<(int Page, int X, int Y), long> tileUse = [];
    private readonly HashSet<(int Page, int X, int Y)> pendingTiles = [];
    private readonly Dictionary<int, Bitmap> previews = [];
    private readonly HashSet<int> pendingPreviews = [];
    private long useCounter;
    private int currentPage = -1;

    // Markierung: Anfang und Ende als (Seite, Zeichen) – in Zugreihenfolge, nicht sortiert
    private record struct TextPos(int Page, int Char);
    private TextPos? anchor, focus;
    private bool selectionActive;                      // erst eine Strecke (oder Wort, alles) ist eine Markierung
    private bool dragging;
    private Point? pendingDragPoint;
    private bool dragQueryRunning;
    private readonly Dictionary<int, int> charCounts = [];
    private readonly Dictionary<int, List<Rectangle>> selectionRects = []; // je Seite, in Seitenpixeln der aktuellen Zoomstufe
    private readonly HashSet<int> pendingSelectionRects = [];
    private int selectionVersion;

    public event EventHandler? ZoomChanged;
    public event EventHandler? CurrentPageChanged;
    public event EventHandler? SelectionChanged;

    /// <summary>Eine Markierung mit der Maus ist fertig (Ziehen losgelassen oder Doppelklick aufs Wort) – für den Hervorheben-Modus.</summary>
    public event EventHandler? SelectionFinished;

    public PageView()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        AutoScroll = true;
        SetStyle(ControlStyles.Selectable, true);
        autoScrollTimer.Tick += AutoScrollTimer_Tick;
    }

    // Mitscrollen beim Markieren: steht die Maus beim Ziehen außerhalb des sichtbaren Bereichs, scrollt dieser Timer weiter –
    // umso schneller, je weiter sie draußen ist – und zieht die Markierung bis zum Zeichen unter der Mausposition nach
    private const int AutoScrollInterval = 30;              // ms
    private const int AutoScrollMinStep = 4, AutoScrollMaxStep = 120; // Gerätepixel je Schritt
    private readonly System.Windows.Forms.Timer autoScrollTimer = new() { Interval = AutoScrollInterval };
    private Point lastDragPoint;                            // letzte Mausposition beim Ziehen (Client, darf außerhalb liegen)

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public float Zoom => zoom;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public bool IsFitWidth => fit == PageFit.Width;

    /// <summary>Wie sich die Zoomstufe nach dem Fenster richtet (None = feste Stufe).</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public PageFit Fit => fit;

    /// <summary>Anpassung beim Öffnen eines Dokuments.</summary>
    [Category("Verhalten"), DefaultValue(PageFit.Height), Description("Anpassung der Zoomstufe beim Öffnen eines Dokuments.")]
    public PageFit DefaultFit { get; set; } = PageFit.Height;

    // Hervorhebung der aktiven Seite (wie ShowCurrentPageHighlight/CurrentPageHighlightColor im Pdfium.Net SDK): ein farbiger Rahmen
    // um die Seite in der Mitte des sichtbaren Bereichs (s. CurrentPage) statt des grauen Randes
    private bool showCurrentPageHighlight;
    private Color currentPageHighlightColor = Color.FromArgb(0, 120, 215);

    /// <summary>Rahmen in <see cref="CurrentPageHighlightColor"/> um die aktive Seite.</summary>
    [Category("Darstellung"), DefaultValue(false), Description("Rahmen in CurrentPageHighlightColor um die aktive Seite.")]
    public bool ShowCurrentPageHighlight
    {
        get => showCurrentPageHighlight;
        set { showCurrentPageHighlight = value; Invalidate(); }
    }

    /// <summary>Breite des Rahmens um die aktive Seite in logischen Pixeln (er liegt im Abstand zwischen den Seiten).</summary>
    [Category("Darstellung"), DefaultValue(4), Description("Breite des Rahmens um die aktive Seite (logische Pixel).")]
    public int CurrentPageHighlightWidth
    {
        get;
        set { field = Math.Clamp(value, 1, PageGap / 2); Invalidate(); } // breiter rührte er an die Nachbarseite
    } = 4;

    /// <summary>Farbe des Rahmens um die aktive Seite.</summary>
    [Category("Darstellung"), DefaultValue(typeof(Color), "0, 120, 215"), Description("Farbe des Rahmens um die aktive Seite.")]
    public Color CurrentPageHighlightColor
    {
        get => currentPageHighlightColor;
        set { currentPageHighlightColor = value; Invalidate(); }
    }

    /// <summary>Seite (0-basiert) in der Mitte des sichtbaren Bereichs; -1 ohne Dokument.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public int CurrentPage => currentPage;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public bool HasSelection => OrderedSelection != null;

    public void SetDocument(PdfiumDocument? value)
    {
        document?.FormInvalidated -= Document_FormInvalidated;
        document = value;
        document?.FormInvalidated += Document_FormInvalidated;
        formFocused = false;
        formPress = null;
        formDragPage = -1;
        CancelPick();
        documentGeneration++;
        foreach (var bitmap in previews.Values) { bitmap.Dispose(); }
        previews.Clear();
        pendingPreviews.Clear();
        stalePreviews.Clear();
        charCounts.Clear();
        ClearSelection();
        ResetSearch();
        hoverLink = linkOnMouseDown = null;
        markupUnderMouse = inkUnderMouse = null;
        hoverAnnotation = null;
        ClearAnnotationGhost();
        inkStroke = null;
        pendingInk.Clear();
        SearchChanged?.Invoke(this, EventArgs.Empty);
        fit = DefaultFit == PageFit.None ? PageFit.Height : DefaultFit;
        currentPage = -1; // die Seitenbreite richtet sich dann nach Seite 1 – nicht nach der Seite des vorigen Dokuments
        ClearTiles();
        LayoutPages();
        // waagerecht mittig: sind einzelne Seiten (Querformat) breiter als das Fenster, wird die Fläche breiter – die erste Seite soll
        // trotzdem vollständig und mittig zu sehen sein
        AutoScrollPosition = new Point(Math.Max(0, (AutoScrollMinSize.Width - ClientSize.Width) / 2), 0);
        UpdateCurrentPage();
        Invalidate();
        ZoomChanged?.Invoke(this, EventArgs.Empty);
    }

    // ================================================================== Layout und Zoom

    private float PixelsPerPoint => DeviceDpi / 72f * zoom;

    private void LayoutPages()
    {
        if (document == null) { pageRects = []; AutoScrollMinSize = Size.Empty; return; }
        var margin = LogicalToDeviceUnits(PageMargin);
        var gap = LogicalToDeviceUnits(PageGap);
        if (fit != PageFit.None)
        {
            // nach der angezeigten Seite (wie Chromium), nicht nach der größten: sonst wären Hochformat-Seiten in einem Dokument mit
            // einzelnen Querformat-Seiten kleiner als das Fenster. Beim Blättern bleibt die Zoomstufe – neu gerechnet wird nur beim
            // Einschalten, beim Öffnen und bei Größenänderungen. Zweiseitig zählt das Seitenpaar samt Abstand.
            var row = RowPages(Math.Clamp(currentPage, 0, document.PageCount - 1)).ToList();
            var pixelsPerPoint = DeviceDpi / 72f;
            var availableWidth = ClientSize.Width - 2 * margin - SystemInformation.VerticalScrollBarWidth - (row.Count - 1) * gap; // Platz für die Bildlaufleiste gleich mit abziehen
            var availableHeight = ClientSize.Height - 2 * margin;
            var byWidth = availableWidth / (row.Sum(p => document.PageSizes[p].Width) * pixelsPerPoint);
            var byHeight = availableHeight / (row.Max(p => document.PageSizes[p].Height) * pixelsPerPoint);
            var fitted = fit switch { PageFit.Width => byWidth, PageFit.Height => byHeight, _ => Math.Min(byWidth, byHeight) };
            zoom = Math.Clamp(fitted, ZoomSteps[0], ZoomSteps[^1]);
        }
        var scale = PixelsPerPoint;
        var sizes = document.PageSizes.Select(s => new Size(Math.Max(1, (int)Math.Round(s.Width * scale)), Math.Max(1, (int)Math.Round(s.Height * scale)))).ToArray();
        var rowCount = twoPage ? (sizes.Length + 1) / 2 : sizes.Length;
        int RowWidth(int row) => twoPage && 2 * row + 1 < sizes.Length ? sizes[2 * row].Width + gap + sizes[2 * row + 1].Width : sizes[twoPage ? 2 * row : row].Width;
        var contentWidth = Enumerable.Range(0, rowCount).Max(RowWidth) + 2 * margin;
        var areaWidth = Math.Max(contentWidth, ClientSize.Width);
        var rects = new Rectangle[sizes.Length];
        var y = margin;
        for (var row = 0; row < rowCount; row++)
        {
            // eine Seite je Zeile, zweiseitig ein Paar (1–2, 3–4 …) nebeneinander, oben bündig und als Paar mittig
            var x = (areaWidth - RowWidth(row)) / 2;
            var height = 0;
            foreach (var i in twoPage ? Enumerable.Range(2 * row, Math.Min(2, sizes.Length - 2 * row)) : [row])
            {
                rects[i] = new Rectangle(x, y, sizes[i].Width, sizes[i].Height);
                x += sizes[i].Width + gap;
                height = Math.Max(height, sizes[i].Height);
            }
            y += height + gap;
        }
        pageRects = rects;
        AutoScrollMinSize = new Size(contentWidth, y - gap + margin);
    }

    /// <summary>Die Seiten in der Zeile einer Seite – zweiseitig das Paar, sonst nur sie selbst.</summary>
    private IEnumerable<int> RowPages(int page) =>
        !twoPage || document == null ? [page] : Enumerable.Range(page / 2 * 2, Math.Min(2, document.PageCount - page / 2 * 2));

    // Zweiseitige Ansicht (Strg+Leertaste, wie im Chromium-Viewer bis PDFlight 1.0.3): Seitenpaare 1–2, 3–4 … nebeneinander
    private bool twoPage;

    /// <summary>Zwei Seiten nebeneinander (Paare 1–2, 3–4 …) statt einer Seite je Zeile.</summary>
    [Category("Darstellung"), DefaultValue(false), Description("Zwei Seiten nebeneinander (Paare 1–2, 3–4 …) statt einer Seite je Zeile.")]
    public bool TwoPageLayout
    {
        get => twoPage;
        set
        {
            if (value == twoPage) { return; }
            twoPage = value;
            if (document == null) { return; }
            var page = Math.Max(0, currentPage);
            ClearTiles();
            LayoutPages();
            GoToPage(page);
            CenterOnPage(page);
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (document == null) { return; }
        var before = zoom;
        var anchorPoint = new Point(ClientSize.Width / 2, 0);
        var keep = CapturePosition(anchorPoint);
        LayoutPages();
        if (zoom != before) { ClearTiles(); ZoomChanged?.Invoke(this, EventArgs.Empty); }
        RestorePosition(keep, anchorPoint);
        if (fit != PageFit.None) { CenterOnPage(keep.Page); } // sonst stünde die Seite nach dem Ein-/Ausblenden der Lesezeichen-Leiste verschoben da
    }

    /// <summary>Waagerechter Bildlauf so, dass die Seite mittig steht (bei „Seitenbreite“ passt sie dann genau ins Fenster).</summary>
    private void CenterOnPage(int page)
    {
        if (page < 0 || page >= pageRects.Length) { return; }
        var rect = RowPages(page).Select(p => pageRects[p]).Aggregate(Rectangle.Union); // zweiseitig: das Paar
        AutoScrollPosition = new Point(Math.Max(0, rect.X + rect.Width / 2 - ClientSize.Width / 2), -AutoScrollPosition.Y);
        Invalidate();
    }


    public void ZoomIn() => SetZoom(ZoomSteps.FirstOrDefault(s => s > zoom * 1.01f, ZoomSteps[^1]), null);

    public void ZoomOut() => SetZoom(ZoomSteps.LastOrDefault(s => s < zoom * 0.99f, ZoomSteps[0]), null);

    /// <summary>Zoomstufe passend zum Fenster: an Breite, Höhe oder ganze Seite (<see cref="PageFit"/>); sie wird bei jeder Größenänderung
    /// neu gerechnet, bis eine feste Stufe gewählt wird. Bei Höhe und Seite steht die angezeigte Seite danach oben im Bild.</summary>
    public void SetFit(PageFit value)
    {
        if (value == PageFit.None) { return; }
        if (document == null) { fit = value; return; }
        var anchorPoint = new Point(ClientSize.Width / 2, ClientSize.Height / 2);
        var keep = CapturePosition(anchorPoint);
        fit = value;
        ClearTiles();
        LayoutPages();
        if (value == PageFit.Width) { RestorePosition(keep, anchorPoint); } else { GoToPage(keep.Page); } // ganze Seite: von ihrem Anfang an
        CenterOnPage(keep.Page);
        Invalidate();
        ZoomChanged?.Invoke(this, EventArgs.Empty);
    }

    public void FitWidth() => SetFit(PageFit.Width);

    /// <summary>Neue Zoomstufe; der Punkt unter <paramref name="anchorClient"/> (sonst die Mitte) bleibt an seinem Platz.</summary>
    public void SetZoom(float value, Point? anchorClient)
    {
        if (document == null) { return; }
        value = Math.Clamp(value, ZoomSteps[0], ZoomSteps[^1]);
        if (fit == PageFit.None && Math.Abs(value - zoom) < 0.001f) { return; }
        var anchorPoint = anchorClient ?? new Point(ClientSize.Width / 2, ClientSize.Height / 2);
        var keep = CapturePosition(anchorPoint);
        fit = PageFit.None;
        zoom = value;
        ClearTiles();
        LayoutPages();
        RestorePosition(keep, anchorPoint);
        Invalidate();
        ZoomChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Welche Seite liegt unter dem Punkt, und wo darin (als Anteil von Breite und Höhe)?</summary>
    private (int Page, float X, float Y) CapturePosition(Point client)
    {
        if (pageRects.Length == 0) { return (0, 0, 0); }
        var (page, point) = PageAt(client, clamp: true);
        var rect = pageRects[page];
        return (page, point.X / (float)rect.Width, point.Y / (float)rect.Height);
    }

    private void RestorePosition((int Page, float X, float Y) keep, Point client)
    {
        if (pageRects.Length == 0) { return; }
        var rect = pageRects[Math.Min(keep.Page, pageRects.Length - 1)];
        var docX = rect.X + (int)(keep.X * rect.Width);
        var docY = rect.Y + (int)(keep.Y * rect.Height);
        AutoScrollPosition = new Point(Math.Max(0, docX - client.X), Math.Max(0, docY - client.Y));
        UpdateCurrentPage();
    }

    public void GoToPage(int index)
    {
        if (index < 0 || index >= pageRects.Length) { return; }
        AutoScrollPosition = new Point(-AutoScrollPosition.X, pageRects[index].Y - LogicalToDeviceUnits(PageMargin));
        UpdateCurrentPage();
        Invalidate();
    }

    private Rectangle Viewport => new(-AutoScrollPosition.X, -AutoScrollPosition.Y, ClientSize.Width, ClientSize.Height);

    private Rectangle ToClient(Rectangle documentRect) => new(documentRect.X + AutoScrollPosition.X, documentRect.Y + AutoScrollPosition.Y, documentRect.Width, documentRect.Height);

    /// <summary>Seite unter einem Client-Punkt und der Punkt in Seitenpixeln; mit <paramref name="clamp"/> die nächstgelegene Seite
    /// (Punkt auf sie begrenzt), sonst -1 neben den Seiten.</summary>
    private (int Page, Point Point) PageAt(Point client, bool clamp)
    {
        var doc = new Point(client.X - AutoScrollPosition.X, client.Y - AutoScrollPosition.Y);
        var best = -1;
        var bestDistance = long.MaxValue;
        for (var i = 0; i < pageRects.Length; i++)
        {
            var rect = pageRects[i];
            if (rect.Contains(doc)) { return (i, new Point(doc.X - rect.X, doc.Y - rect.Y)); }
            var dy = doc.Y < rect.Top ? rect.Top - doc.Y : doc.Y > rect.Bottom ? doc.Y - rect.Bottom : 0;
            var dx = doc.X < rect.Left ? rect.Left - doc.X : doc.X > rect.Right ? doc.X - rect.Right : 0;
            var distance = (long)dy << 32 | (uint)dx; // zuerst senkrecht, bei gleicher Zeile (zweiseitig) waagerecht
            if (distance < bestDistance) { bestDistance = distance; best = i; }
        }
        if (!clamp || best < 0) { return (-1, Point.Empty); }
        var r = pageRects[best];
        return (best, new Point(Math.Clamp(doc.X - r.X, 0, r.Width - 1), Math.Clamp(doc.Y - r.Y, 0, r.Height - 1)));
    }

    private IEnumerable<int> VisiblePages()
    {
        var viewport = Viewport;
        for (var i = 0; i < pageRects.Length; i++)
        {
            if (pageRects[i].Bottom < viewport.Top) { continue; }
            if (pageRects[i].Top > viewport.Bottom) { yield break; }
            yield return i;
        }
    }

    private void UpdateCurrentPage()
    {
        var page = pageRects.Length == 0 ? -1 : PageAt(new Point(ClientSize.Width / 2, ClientSize.Height / 2), clamp: true).Page;
        if (page == currentPage) { return; }
        currentPage = page;
        if (ShowCurrentPageHighlight) { Invalidate(); } // der Rahmen wandert mit
        document?.CurrentPageIndex = page;
        CurrentPageChanged?.Invoke(this, EventArgs.Empty);
    }

    // ================================================================== Zeichnen

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (document == null) { return; }
        var g = e.Graphics;
        var viewport = Viewport;
        foreach (var page in VisiblePages())
        {
            var rect = ToClient(pageRects[page]);
            using (var shadow = new SolidBrush(Color.FromArgb(40, 0, 0, 0))) { g.FillRectangle(shadow, rect.X + 2, rect.Y + 2, rect.Width, rect.Height); }
            g.FillRectangle(Brushes.White, rect);
            if (previews.TryGetValue(page, out var preview))
            {
                g.InterpolationMode = InterpolationMode.Bilinear;
                g.DrawImage(preview, rect); // unscharf, aber sofort da – die Kacheln zeichnen darüber
                if (stalePreviews.Contains(page)) { RequestPreview(page); } // Formularwert geändert: bis zum neuen Bild bleibt das alte stehen
            }
            else { RequestPreview(page); }
            DrawTiles(g, page, rect, viewport);
            DrawHits(g, page, rect);
            DrawSelection(g, page, rect);
            if (page == currentPage && ShowCurrentPageHighlight)
            {
                // Rahmen um die aktive Seite, außen um die Seite gelegt – er verdeckt nichts vom Inhalt
                var width = LogicalToDeviceUnits(CurrentPageHighlightWidth);
                using var pen = new Pen(CurrentPageHighlightColor, width) { Alignment = PenAlignment.Inset };
                g.DrawRectangle(pen, rect.X - width, rect.Y - width, rect.Width + 2 * width - 1, rect.Height + 2 * width - 1);
            }
            else { g.DrawRectangle(Pens.DarkGray, rect.X - 1, rect.Y - 1, rect.Width + 1, rect.Height + 1); }
        }
        DrawAnnotationGhost(g);
        DrawInk(g);
    }

    private void DrawTiles(Graphics g, int page, Rectangle clientRect, Rectangle viewport)
    {
        var docRect = pageRects[page];
        var visible = Rectangle.Intersect(viewport, docRect);
        visible.Offset(-docRect.X, -docRect.Y); // in Seitenpixeln
        for (var ty = visible.Top / TileSize; ty <= (visible.Bottom - 1) / TileSize; ty++)
        {
            for (var tx = visible.Left / TileSize; tx <= (visible.Right - 1) / TileSize; tx++)
            {
                var key = (page, tx, ty);
                if (tiles.TryGetValue(key, out var tile))
                {
                    g.DrawImageUnscaled(tile, clientRect.X + tx * TileSize, clientRect.Y + ty * TileSize);
                    tileUse[key] = ++useCounter;
                    if (staleTiles.Contains(key)) { RequestTile(page, tx, ty); } // veraltet: neu rendern, bis dahin das alte Bild – kein Flackern
                }
                else { RequestTile(page, tx, ty); }
            }
        }
    }

    private Rectangle TileRect(int page, int tx, int ty) =>
        Rectangle.Intersect(new Rectangle(tx * TileSize, ty * TileSize, TileSize, TileSize), new Rectangle(Point.Empty, pageRects[page].Size));

    private bool TileVisible(int page, Rectangle tile)
    {
        if (page >= pageRects.Length) { return false; }
        tile.Offset(pageRects[page].Location);
        return Viewport.IntersectsWith(tile);
    }

    private async void RequestTile(int page, int tx, int ty)
    {
        var key = (page, tx, ty);
        if (document == null || !pendingTiles.Add(key)) { return; }
        var requestGeneration = generation;
        var wasStale = staleTiles.Remove(key); // wird die Kachel währenddessen wieder ungültig, landet sie erneut in staleTiles
        var tile = TileRect(page, tx, ty);
        var size = pageRects[page].Size;
        Bitmap? bitmap;
        try
        {
            bitmap = await document.RenderAsync(page, size.Width, size.Height, tile,
                () => requestGeneration == generation && !IsDisposed && (bool)Invoke(() => TileVisible(page, tile)));
        }
        catch (InvalidOperationException) { bitmap = null; } // Fenster beim Schließen: Invoke geht nicht mehr
        if (requestGeneration != generation || IsDisposed) { bitmap?.Dispose(); return; }
        pendingTiles.Remove(key);
        if (bitmap == null) // verfallen – beim nächsten Zeichnen wird neu angefragt, falls sichtbar
        {
            if (wasStale && tiles.ContainsKey(key)) { staleTiles.Add(key); }
            return;
        }
        if (tiles.TryGetValue(key, out var old)) { old.Dispose(); }
        tiles[key] = bitmap;
        tileUse[key] = ++useCounter;
        TrimTiles();
        tile.Offset(pageRects[page].Location);
        Invalidate(ToClient(tile));
    }

    private void TrimTiles()
    {
        if (tiles.Count <= TileLimit) { return; }
        foreach (var key in tileUse.OrderBy(t => t.Value).Take(tiles.Count - TileLimit).Select(t => t.Key).ToList())
        {
            tiles[key].Dispose();
            tiles.Remove(key);
            tileUse.Remove(key);
            staleTiles.Remove(key);
        }
    }

    private void ClearTiles()
    {
        generation++;
        foreach (var bitmap in tiles.Values) { bitmap.Dispose(); }
        tiles.Clear();
        tileUse.Clear();
        pendingTiles.Clear();
        staleTiles.Clear();
        selectionRects.Clear(); // in Seitenpixeln – gelten nur für die bisherige Zoomstufe
        pendingSelectionRects.Clear();
        hitRects.Clear();
        pendingHitRects.Clear();
    }

    private async void RequestPreview(int page)
    {
        if (document == null || !pendingPreviews.Add(page)) { return; }
        stalePreviews.Remove(page);
        var requestGeneration = documentGeneration;
        var pageSize = document.PageSizes[page];
        var width = Math.Min(PreviewWidth, (int)Math.Ceiling(pageSize.Width * DeviceDpi / 72f));
        var height = Math.Max(1, (int)Math.Round(width * pageSize.Height / pageSize.Width));
        Bitmap? bitmap;
        try
        {
            bitmap = await document.RenderAsync(page, width, height,
                () => requestGeneration == documentGeneration && !IsDisposed && (bool)Invoke(() => VisiblePages().Contains(page)));
        }
        catch (InvalidOperationException) { bitmap = null; }
        if (requestGeneration != documentGeneration || IsDisposed) { bitmap?.Dispose(); return; }
        pendingPreviews.Remove(page);
        if (bitmap == null) { return; }
        if (previews.TryGetValue(page, out var old)) { old.Dispose(); }
        previews[page] = bitmap;
        if (previews.Count > PreviewLimit) // die am weitesten entfernten Vorschaubilder verwerfen
        {
            foreach (var far in previews.Keys.OrderByDescending(p => Math.Abs(p - currentPage)).Take(previews.Count - PreviewLimit).ToList())
            {
                previews[far].Dispose();
                previews.Remove(far);
            }
        }
        if (page < pageRects.Length) { Invalidate(ToClient(pageRects[page])); }
    }

    // ================================================================== Markierung

    /// <summary>Anfang und Ende der Markierung in Dokumentreihenfolge.</summary>
    private (TextPos Start, TextPos End)? OrderedSelection
    {
        get
        {
            if (!selectionActive || anchor is not { } a || focus is not { } f) { return null; }
            return a.Page < f.Page || (a.Page == f.Page && a.Char <= f.Char) ? (a, f) : (f, a);
        }
    }

    /// <summary>Markierter Zeichenbereich einer Seite (erstes Zeichen, Anzahl); null, wenn die Seite nicht betroffen ist oder ihre
    /// Zeichenzahl noch nicht bekannt ist (wird dann nachgeladen).</summary>
    private (int Start, int Count)? SelectedRange(int page)
    {
        if (OrderedSelection is not { } selection || page < selection.Start.Page || page > selection.End.Page) { return null; }
        var start = page == selection.Start.Page ? selection.Start.Char : 0;
        int end;
        if (page == selection.End.Page) { end = selection.End.Char; }
        else if (charCounts.TryGetValue(page, out var count)) { end = count - 1; }
        else { RequestCharCount(page); return null; }
        return end >= start ? (start, end - start + 1) : null;
    }

    private async void RequestCharCount(int page)
    {
        if (document == null || charCounts.ContainsKey(page)) { return; }
        charCounts[page] = await document.CharCountAsync(page);
        Invalidate();
    }

    private void DrawSelection(Graphics g, int page, Rectangle clientRect)
    {
        if (SelectedRange(page) is not { } range) { return; }
        if (!selectionRects.TryGetValue(page, out var rects)) { RequestSelectionRects(page, range); return; }
        using var brush = new SolidBrush(SelectionColor);
        foreach (var r in rects) { g.FillRectangle(brush, clientRect.X + r.X, clientRect.Y + r.Y, r.Width, r.Height); }
    }

    private async void RequestSelectionRects(int page, (int Start, int Count) range)
    {
        if (document == null || !pendingSelectionRects.Add(page)) { return; }
        var version = selectionVersion;
        var zoomGeneration = generation;
        var size = pageRects[page].Size;
        var rects = await document.SelectionRectsAsync(page, size.Width, size.Height, range.Start, range.Count);
        if (version != selectionVersion || zoomGeneration != generation || IsDisposed) { return; }
        pendingSelectionRects.Remove(page);
        selectionRects[page] = rects;
        Invalidate(ToClient(pageRects[page]));
    }

    private void SelectionUpdated()
    {
        selectionVersion++;
        selectionRects.Clear();
        pendingSelectionRects.Clear();
        Invalidate();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearSelection()
    {
        anchor = focus = null;
        selectionActive = false;
        SelectionUpdated();
    }

    public async void SelectAll()
    {
        if (document == null || document.PageCount == 0) { return; }
        var last = document.PageCount - 1;
        var count = await document.CharCountAsync(last);
        anchor = new TextPos(0, 0);
        focus = new TextPos(last, Math.Max(0, count - 1));
        selectionActive = true;
        SelectionUpdated();
    }

    /// <summary>Kopiert den markierten Text in die Zwischenablage (Seiten durch Zeilenumbruch getrennt); liefert die Zeichenzahl.</summary>
    public async Task<int> CopySelectionAsync()
    {
        if (document == null || OrderedSelection is not { } selection) { return 0; }
        var parts = new List<string>();
        for (var page = selection.Start.Page; page <= selection.End.Page; page++)
        {
            if (!charCounts.ContainsKey(page)) { charCounts[page] = await document.CharCountAsync(page); }
            if (SelectedRange(page) is { } range) { parts.Add(await document.TextAsync(page, range.Start, range.Count)); }
        }
        var text = string.Join(Environment.NewLine, parts);
        if (text.Length > 0) { Clipboard.SetText(text); }
        return text.Length;
    }

    // ================================================================== Hervorhebungen

    // Hervorgehoben wird der markierte Text (je Seite eine Anmerkung); das Hilfsprogramm meldet die Seite danach als neu zu zeichnen.
    // Für das Entfernen merkt sich ein Rechtsklick, ob unter der Maus eine Textmarkierung liegt – das Kontextmenü fragt danach.
    private (int Page, int Number)? markupUnderMouse;
    private (int Page, int Number)? inkUnderMouse;   // ebenso eine Zeichnung (Freihand-Strich)
    private Task? markupQuery;

    /// <summary>Hebt den markierten Text hervor (Farbe 0xRRGGBB); liefert die Zahl der Seiten, auf denen eine Hervorhebung entstand.</summary>
    public async Task<int> HighlightSelectionAsync(int color = PdfiumDocument.AcrobatYellow)
    {
        if (document == null || OrderedSelection is not { } selection) { return 0; }
        var doc = document;
        var pages = 0;
        for (var page = selection.Start.Page; page <= selection.End.Page; page++)
        {
            if (!charCounts.ContainsKey(page)) { charCounts[page] = await doc.CharCountAsync(page); }
            if (SelectedRange(page) is { } range && await doc.AddHighlightAsync(page, range.Start, range.Count, color)) { pages++; }
        }
        if (doc == document) { ClearSelection(); } // die Hervorhebung soll man sehen, nicht die Markierung darüber
        return pages;
    }

    /// <summary>Liegt unter dem letzten Rechtsklick eine Textmarkierung (Hervorhebung u. ä.)? Die Abfrage läuft beim Drücken; ist sie
    /// beim Öffnen des Kontextmenüs noch nicht fertig, gilt „nein“.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public bool HasMarkupUnderMouse => markupQuery?.IsCompleted == true && markupUnderMouse != null;

    /// <summary>Entfernt die Textmarkierung unter dem letzten Rechtsklick.</summary>
    public async Task<bool> RemoveMarkupUnderMouseAsync()
    {
        if (document == null || markupUnderMouse is not { } markup) { return false; }
        markupUnderMouse = null;
        return await document.RemoveMarkupAsync(markup.Page, markup.Number);
    }

    /// <summary>Liegt unter dem letzten Rechtsklick eine Zeichnung (die Linie selbst, nicht nur ihr Rechteck)?</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public bool HasInkUnderMouse => markupQuery?.IsCompleted == true && inkUnderMouse != null;

    /// <summary>Entfernt die Zeichnung unter dem letzten Rechtsklick.</summary>
    public async Task<bool> RemoveInkUnderMouseAsync()
    {
        if (document == null || inkUnderMouse is not { } ink) { return false; }
        inkUnderMouse = null;
        return await document.RemoveMarkupAsync(ink.Page, ink.Number);
    }

    private async Task QueryMarkupAsync(Point client)
    {
        markupUnderMouse = inkUnderMouse = null;
        var (page, point) = PageAt(client, clamp: false);
        if (document == null || page < 0) { return; }
        var (width, height) = (pageRects[page].Width, pageRects[page].Height);
        var number = await document.MarkupAtAsync(page, width, height, point);
        markupUnderMouse = number >= 0 ? (page, number) : null;
        if (document == null) { return; }
        var ink = await document.InkAtAsync(page, width, height, point);
        inkUnderMouse = ink >= 0 ? (page, ink) : null;
    }

    // ================================================================== Suche

    // Gesucht wird Seite für Seite auf dem PDFium-Thread – dazwischen kommen Render-Aufträge dran, die Ansicht bleibt flüssig, und die
    // ersten Treffer erscheinen sofort. Ein neuer Suchbegriff (searchVersion) lässt die laufende Suche ins Leere laufen.
    private static readonly Color HitColor = Color.FromArgb(110, 255, 210, 0);
    private static readonly Color CurrentHitColor = Color.FromArgb(150, 255, 120, 0);
    private record struct SearchHit(int Page, int Start, int Count);
    private readonly List<SearchHit> hits = [];                         // in Dokumentreihenfolge
    private readonly Dictionary<int, List<int>> hitsByPage = [];        // Seite → Nummern in hits
    private readonly Dictionary<int, List<(int Hit, Rectangle Rect)>> hitRects = []; // in Seitenpixeln der aktuellen Zoomstufe
    private readonly HashSet<int> pendingHitRects = [];
    private int searchVersion;
    private int currentHit = -1;

    /// <summary>Trefferzahl, aktueller Treffer oder Fortschritt haben sich geändert.</summary>
    public event EventHandler? SearchChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public int SearchHitCount => hits.Count;

    /// <summary>Nummer des aktuellen Treffers (0-basiert), -1 ohne.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public int CurrentHit => currentHit;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public bool SearchRunning { get; private set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public int SearchedPages { get; private set; }

    /// <summary>Sucht im ganzen Dokument; der erste Treffer ab der angezeigten Seite wird angesprungen (sonst der erste überhaupt).</summary>
    public async void Search(string query, bool matchCase, bool wholeWord = false)
    {
        var version = ResetSearch();
        if (document == null || query.Length == 0) { SearchChanged?.Invoke(this, EventArgs.Empty); return; }
        var doc = document;
        var startPage = Math.Max(0, currentPage);
        SearchRunning = true;
        SearchChanged?.Invoke(this, EventArgs.Empty);
        for (var page = 0; page < doc.PageCount; page++)
        {
            var found = await doc.SearchPageAsync(page, query, matchCase, wholeWord);
            if (version != searchVersion || doc != document || IsDisposed) { return; }
            if (found.Count > 0)
            {
                var numbers = new List<int>(found.Count);
                foreach (var (start, count) in found)
                {
                    numbers.Add(hits.Count);
                    hits.Add(new SearchHit(page, start, count));
                }
                hitsByPage[page] = numbers;
                if (currentHit < 0 && page >= startPage) { currentHit = numbers[0]; ScrollToHit(currentHit); }
                if (VisiblePages().Contains(page)) { Invalidate(ToClient(pageRects[page])); }
            }
            SearchedPages = page + 1;
            SearchChanged?.Invoke(this, EventArgs.Empty);
        }
        SearchRunning = false;
        if (currentHit < 0 && hits.Count > 0) { currentHit = 0; ScrollToHit(0); } // nur vor der angezeigten Seite gefunden
        SearchChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearSearch()
    {
        ResetSearch();
        Invalidate();
        SearchChanged?.Invoke(this, EventArgs.Empty);
    }

    private int ResetSearch()
    {
        hits.Clear();
        hitsByPage.Clear();
        hitRects.Clear();
        pendingHitRects.Clear();
        currentHit = -1;
        SearchRunning = false;
        SearchedPages = 0;
        Invalidate();
        return ++searchVersion;
    }

    /// <summary>Zum nächsten bzw. vorigen Treffer, am Ende wieder von vorn.</summary>
    public void NextHit(bool backwards)
    {
        if (hits.Count == 0) { return; }
        currentHit = currentHit < 0 ? 0 : (currentHit + (backwards ? -1 : 1) + hits.Count) % hits.Count;
        ScrollToHit(currentHit);
        Invalidate();
        SearchChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Bringt den Treffer ins Bild – nur wenn er nicht schon sichtbar ist, dann im oberen Drittel.</summary>
    private async void ScrollToHit(int index)
    {
        if (document == null || index < 0 || index >= hits.Count) { return; }
        var hit = hits[index];
        var version = searchVersion;
        var size = pageRects[hit.Page].Size;
        var rects = await document.SelectionRectsAsync(hit.Page, size.Width, size.Height, hit.Start, hit.Count);
        if (version != searchVersion || IsDisposed || hit.Page >= pageRects.Length) { return; }
        if (rects.Count == 0) { GoToPage(hit.Page); return; }
        var target = rects.Aggregate(Rectangle.Union);
        target.Offset(pageRects[hit.Page].Location);
        var viewport = Viewport;
        if (viewport.Contains(target)) { return; }
        var x = target.Left >= viewport.Left && target.Right <= viewport.Right ? viewport.X : target.X - ClientSize.Width / 3;
        AutoScrollPosition = new Point(Math.Max(0, x), Math.Max(0, target.Y - ClientSize.Height / 3));
        UpdateCurrentPage();
        Invalidate();
    }

    private void DrawHits(Graphics g, int page, Rectangle clientRect)
    {
        if (!hitsByPage.ContainsKey(page)) { return; }
        if (!hitRects.TryGetValue(page, out var rects)) { RequestHitRects(page); return; }
        using var hitBrush = new SolidBrush(HitColor);
        using var currentBrush = new SolidBrush(CurrentHitColor);
        foreach (var (hit, r) in rects) { g.FillRectangle(hit == currentHit ? currentBrush : hitBrush, clientRect.X + r.X, clientRect.Y + r.Y, r.Width, r.Height); }
    }

    private async void RequestHitRects(int page)
    {
        if (document == null || !pendingHitRects.Add(page)) { return; }
        var version = searchVersion;
        var zoomGeneration = generation;
        var size = pageRects[page].Size;
        var result = new List<(int, Rectangle)>();
        foreach (var number in hitsByPage[page].ToList())
        {
            var hit = hits[number];
            foreach (var rect in await document.SelectionRectsAsync(page, size.Width, size.Height, hit.Start, hit.Count)) { result.Add((number, rect)); }
            if (version != searchVersion || zoomGeneration != generation || IsDisposed) { return; }
        }
        pendingHitRects.Remove(page);
        hitRects[page] = result;
        Invalidate(ToClient(pageRects[page]));
    }

    // ================================================================== Links

    // Beim Überfahren fragt die Ansicht im Hintergrund, ob unter der Maus ein Link liegt (immer nur eine Abfrage zugleich, die letzte
    // Mausposition zählt). Ein Klick ohne Ziehen folgt ihm: Seitenziele springt die Ansicht selbst an, Webadressen meldet sie.
    private LinkTarget? hoverLink;
    private LinkTarget? linkOnMouseDown;
    private Point? pendingHoverPoint;
    private bool hoverQueryRunning;

    /// <summary>Der Link unter der Maus hat sich geändert (null = keiner).</summary>
    public event EventHandler<LinkTarget?>? LinkHover;

    /// <summary>Klick auf einen Link zu einer Webadresse – öffnen muss der Aufrufer.</summary>
    public event EventHandler<LinkTarget>? ExternalLinkClicked;

    private async void RunHoverQueries()
    {
        hoverQueryRunning = true;
        try
        {
            while (pendingHoverPoint is { } point && document != null)
            {
                pendingHoverPoint = null;
                var (page, pagePoint) = PageAt(point, clamp: false);
                var field = -1;
                if (page >= 0 && document.HasForms)
                {
                    field = await document.FormMouseMoveAsync(page, pageRects[page].Width, pageRects[page].Height, pagePoint, FormModifiers(ModifierKeys, false));
                    if (document == null) { break; }
                }
                (int Number, int Subtype, Rectangle Rect)? annotation = null;
                if (page >= 0 && field < 0 && AllowAnnotationMove && ghost == null)
                {
                    annotation = await document.AnnotationAtAsync(page, pageRects[page].Width, pageRects[page].Height, pagePoint, MovableAnnotations);
                    if (document == null) { break; }
                }
                var link = page < 0 || field >= 0 || annotation != null ? null : await document.LinkAtAsync(page, pageRects[page].Width, pageRects[page].Height, pagePoint);
                if (dragging || formDragPage >= 0 || pick != null || ghost != null || InkMode || IsDisposed) { continue; } // beim Wählen einer Stelle und beim Zeichnen bleibt das Fadenkreuz
                hoverAnnotation = annotation is { } a && page < pageRects.Length ? (page, a.Number, a.Subtype, a.Rect, pageRects[page].Size) : null;
                Cursor = annotation != null ? Cursors.SizeAll
                    : field is PdfiumForms.FieldTextField or PdfiumForms.FieldComboBox ? Cursors.IBeam
                    : field >= 0 || link != null ? Cursors.Hand
                    : page >= 0 ? Cursors.IBeam : Cursors.Default;
                if (link != hoverLink)
                {
                    hoverLink = link;
                    LinkHover?.Invoke(this, link);
                }
            }
        }
        finally { hoverQueryRunning = false; }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        pendingHoverPoint = null;
        if (!ghostDragging) { hoverAnnotation = null; }
        if (hoverLink != null) { hoverLink = null; LinkHover?.Invoke(this, null); }
    }

    private void FollowLink(LinkTarget link)
    {
        if (link.Uri != null) { ExternalLinkClicked?.Invoke(this, link); }
        else { GoToTarget(link); }
    }

    /// <summary>Springt zu einem Seitenziel – mit Höhenangabe an diese Stelle der Seite, sonst an den Seitenanfang.</summary>
    public void GoToTarget(LinkTarget target)
    {
        if (document == null || target.Page < 0 || target.Page >= pageRects.Length) { return; }
        var rect = pageRects[target.Page];
        var offset = 0;
        if (target.TopPt is { } top)
        {
            var heightPt = document.PageSizes[target.Page].Height;
            offset = Math.Clamp((int)((heightPt - top) * PixelsPerPoint), 0, rect.Height); // PDF-Höhen zählen von unten
        }
        AutoScrollPosition = new Point(-AutoScrollPosition.X, rect.Y + offset - LogicalToDeviceUnits(PageMargin));
        UpdateCurrentPage();
        Invalidate();
    }

    // ================================================================== Formulare

    // Klicks auf ein Formularfeld gehen an PDFium statt an die Textauswahl; danach hat das Feld den Fokus, und die Tasten gehen dorthin,
    // bis neben die Felder geklickt oder Esc gedrückt wird. Was PDFium nicht verarbeitet (Bild ab/auf, Pfeile bei einem Häkchen), nutzt
    // die Ansicht. Meldet PDFium einen Bereich als ungültig, werden die betroffenen Kacheln neu gerendert; bis dahin bleibt das alte Bild.
    private readonly HashSet<(int Page, int X, int Y)> staleTiles = [];
    private readonly HashSet<int> stalePreviews = [];
    private bool formFocused;                          // ein Formularfeld hat den Tastaturfokus
    private Task<int>? formPress;                      // Abfrage beim Drücken: Feldart unter der Maus (-1 = keins)
    private int formDragPage = -1;                     // Maustaste auf einem Feld gedrückt: Seite des Felds

    /// <summary>Ein Formularfeld hat den Tastaturfokus.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public bool FormFocused => formFocused;

    private void Document_FormInvalidated(object? sender, FormInvalidatedEventArgs e)
    {
        // kommt auf dem PDFium-Thread
        if (IsHandleCreated && !IsDisposed) { BeginInvoke(() => InvalidateFormArea(sender, e)); }
    }

    private void InvalidateFormArea(object? sender, FormInvalidatedEventArgs e)
    {
        if (sender != document || e.Page >= pageRects.Length) { return; }
        var size = pageRects[e.Page].Size;
        var area = Rectangle.FromLTRB((int)(e.Area.Left * size.Width) - 2, (int)(e.Area.Top * size.Height) - 2,
            (int)Math.Ceiling(e.Area.Right * size.Width) + 2, (int)Math.Ceiling(e.Area.Bottom * size.Height) + 2);
        if (RefreshPageArea(e.Page, area)) { PageContentChanged?.Invoke(this, e.Page); }
    }

    /// <summary>Bereich einer Seite (Seitenpixel) neu rendern; bis die Kacheln da sind, bleibt das alte Bild. false = nichts zu tun.</summary>
    private bool RefreshPageArea(int page, Rectangle area)
    {
        area.Intersect(new Rectangle(Point.Empty, pageRects[page].Size));
        if (area.IsEmpty) { return false; }
        foreach (var key in tiles.Keys.Where(k => k.Page == page && TileRect(k.Page, k.X, k.Y).IntersectsWith(area))) { staleTiles.Add(key); }
        stalePreviews.Add(page);
        area.Offset(pageRects[page].Location);
        Invalidate(ToClient(area));
        return true;
    }

    /// <summary>Der Inhalt einer Seite hat sich geändert (Formularwert, Hervorhebung) – etwa für die Miniaturen.</summary>
    public event EventHandler<int>? PageContentChanged;

    /// <summary>Nimmt den Fokus aus dem Formularfeld (PDFium übernimmt den eingetippten Wert).</summary>
    public async Task EndFormInputAsync()
    {
        if (document == null || !formFocused) { return; }
        formFocused = false;
        await document.FormKillFocusAsync();
    }

    private static int FormModifiers(Keys modifiers, bool leftButton) =>
        ((modifiers & Keys.Shift) != 0 ? PdfiumForms.ModShift : 0) | ((modifiers & Keys.Control) != 0 ? PdfiumForms.ModControl : 0)
        | ((modifiers & Keys.Alt) != 0 ? PdfiumForms.ModAlt : 0) | (leftButton ? PdfiumForms.ModLeftButton : 0);

    /// <summary>Punkt (Client) in Seitenpixeln einer bestimmten Seite – auch außerhalb von ihr (Ziehen über den Feldrand hinaus).</summary>
    private Point ToPagePoint(int page, Point client) =>
        new(client.X - AutoScrollPosition.X - pageRects[page].X, client.Y - AutoScrollPosition.Y - pageRects[page].Y);

    /// <summary>Taste bei Fokus im Formularfeld: Zwischenablage und Rückgängig selbst, alles andere an PDFium. true = erledigt.</summary>
    private async Task<bool> FormKeyDownAsync(KeyEventArgs e)
    {
        if (document is not { } doc) { return false; }
        switch (e.KeyData)
        {
            case Keys.Escape:
                ClearSelection();
                await EndFormInputAsync();
                return true;
            case Keys.Control | Keys.C:
            case Keys.Control | Keys.Insert:
                var copy = await doc.FormSelectedTextAsync();
                if (copy.Length > 0) { Clipboard.SetText(copy); }
                return true;
            case Keys.Control | Keys.X:
            case Keys.Shift | Keys.Delete:
                var cut = await doc.FormSelectedTextAsync();
                if (cut.Length > 0) { Clipboard.SetText(cut); await doc.FormReplaceSelectionAsync(string.Empty); }
                return true;
            case Keys.Control | Keys.V:
            case Keys.Shift | Keys.Insert:
                if (Clipboard.ContainsText()) { await doc.FormReplaceSelectionAsync(Clipboard.GetText()); }
                return true;
            case Keys.Control | Keys.A: await doc.FormSelectAllAsync(); return true;
            case Keys.Control | Keys.Z: await doc.FormUndoAsync(); return true;
            case Keys.Control | Keys.Y:
            case Keys.Control | Keys.Shift | Keys.Z: await doc.FormRedoAsync(); return true;
        }
        return await doc.FormKeyDownAsync((int)e.KeyCode, FormModifiers(e.Modifiers, false));
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
        // Strg+Buchstabe liefert Steuerzeichen (< 0x20) – die erledigt OnKeyDown; Rückschritt und Enter braucht das Feld als Zeichen
        if (!formFocused || document == null || (e.KeyChar < ' ' && e.KeyChar is not ('\b' or '\r'))) { return; }
        e.Handled = true;
        _ = document.FormCharAsync(e.KeyChar, FormModifiers(ModifierKeys, false));
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (formFocused && document != null) { _ = document.FormKeyUpAsync((int)e.KeyCode, FormModifiers(e.Modifiers, false)); }
    }

    // ================================================================== Stelle wählen (Anmerkungen)

    // Zum Einfügen an einer Stelle der Seite: PickPointAsync schaltet auf ein Fadenkreuz um; ein Linksklick auf eine Seite liefert die
    // Stelle, Esc oder ein Klick neben die Seiten bricht ab (null). Die Stelle unter dem letzten Rechtsklick merkt ContextMenuPoint.
    private TaskCompletionSource<Point?>? pick;

    /// <summary>Wartet auf einen Klick auf eine Seite (Client-Koordinaten); null bei Abbruch.</summary>
    public Task<Point?> PickPointAsync()
    {
        CancelPick();
        if (document == null) { return Task.FromResult<Point?>(null); }
        pick = new TaskCompletionSource<Point?>();
        Cursor = Cursors.Cross;
        Focus();
        return pick.Task;
    }

    /// <summary>Liegt die Stelle (Client-Koordinaten) auf einer Seite?</summary>
    public bool IsOnPage(Point client) => PageAt(client, clamp: false).Page >= 0;

    /// <summary>Ist die Seite (0-basiert) gerade ganz oder teilweise zu sehen?</summary>
    public bool IsPageVisible(int index) => VisiblePages().Contains(index);

    /// <summary>Bildlaufposition (Dokumentkoordinaten der linken oberen Ecke) – zum Wiederherstellen nach dem Neuladen derselben Datei.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public Point ScrollOffset
    {
        get => new(-AutoScrollPosition.X, -AutoScrollPosition.Y);
        set
        {
            AutoScrollPosition = value;
            UpdateCurrentPage();
            Invalidate();
        }
    }

    /// <summary>Gerade wird eine Stelle gewählt (Fadenkreuz).</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public bool IsPicking => pick != null;

    public void CancelPick() => EndPick(null);

    private void EndPick(Point? result)
    {
        if (pick is not { } pending) { return; }
        pick = null;
        Cursor = Cursors.Default;
        pending.TrySetResult(result);
    }

    /// <summary>Stelle des letzten Rechtsklicks (Client-Koordinaten) – für Einträge des Kontextmenüs, die dort etwas einfügen.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public Point ContextMenuPoint { get; private set; }

    /// <summary>Seite (0-basiert) und PDF-Koordinaten (Punkt, Ursprung unten links) unter einer Stelle der Ansicht; null neben den Seiten.</summary>
    public async Task<(int Page, double X, double Y)?> PagePointAsync(Point client)
    {
        var (page, point) = PageAt(client, clamp: false);
        if (document == null || page < 0) { return null; }
        return await document.PagePointAsync(page, pageRects[page].Width, pageRects[page].Height, point) is { } pdf ? (page, pdf.X, pdf.Y) : null;
    }

    // ================================================================== Textanmerkungen und Stempel verschieben

    // Mit AllowAnnotationMove fragt die Ansicht beim Überfahren auch nach Freitext-Anmerkungen und Stempeln (Cursor: Verschiebekreuz).
    // Ziehen zeigt eine halbdurchsichtige Kopie des Kastens an der neuen Stelle (immer ganz auf der Seite); das Original blendet das
    // Hilfsprogramm dafür nur in der Anzeige aus (sonst stünde es bis zum Loslassen doppelt da). Esc bricht ab. Beim Loslassen meldet
    // AnnotationMoved den Versatz in PDF-Punkten – in die Datei schreiben und neu laden muss der Aufrufer; bis dahin bleibt die Kopie an
    // der neuen Stelle stehen (ClearAnnotationGhost, wenn es scheitert – das blendet das Original wieder ein). Ein Doppelklick meldet
    // AnnotationDoubleClicked (Bearbeiten).
    private const int MovableAnnotations = (1 << Protocol.AnnotFreeText) | (1 << Protocol.AnnotStamp);

    private sealed class AnnotationGhost(PdfiumDocument document, int page, int number, int subtype, Rectangle rect, Size pageSize, Point start, Bitmap image)
    {
        public PdfiumDocument Document { get; } = document;
        public int Page { get; } = page;
        public int Number { get; } = number;       // Position im /Annots-Array der Seite
        public int Subtype { get; } = subtype;     // FPDF_ANNOT_*
        public Rectangle Rect { get; } = rect;     // Seitenpixel bei pageSize
        public Size PageSize { get; } = pageSize;
        public Point Start { get; } = start;       // Seitenpixel der Druckstelle
        public Bitmap Image { get; } = image;      // der Kasten, wie er gerade zu sehen war
        public Size Offset { get; set; }           // Seitenpixel
        public bool Moved { get; set; }            // über die Ziehschwelle hinaus bewegt
        public bool Hidden { get; set; }           // Original in der Anzeige ausgeblendet
    }

    private (int Page, int Number, int Subtype, Rectangle Rect, Size PageSize)? hoverAnnotation;
    private AnnotationGhost? ghost;
    private bool ghostDragging;
    private bool annotationClick; // Doppelklick auf eine Anmerkung: das folgende Loslassen gehört ihr (kein Link, keine Markierung)

    /// <summary>Freitext-Anmerkungen lassen sich mit der Maus verschieben (<see cref="AnnotationMoved"/>).</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public bool AllowAnnotationMove
    {
        get;
        set { field = value; if (!value) { hoverAnnotation = null; ClearAnnotationGhost(); } }
    }

    /// <summary>Gerade wird eine Textanmerkung gezogen.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public bool IsMovingAnnotation => ghostDragging;

    /// <summary>Eine Freitext-Anmerkung oder ein Stempel wurde an eine neue Stelle gezogen.</summary>
    public event EventHandler<AnnotationMovedEventArgs>? AnnotationMoved;

    /// <summary>Doppelklick auf eine Freitext-Anmerkung oder einen Stempel.</summary>
    public event EventHandler<AnnotationEventArgs>? AnnotationDoubleClicked;

    /// <summary>Bricht das Ziehen ab bzw. nimmt die Kopie an der neuen Stelle wieder weg und blendet das Original wieder ein.</summary>
    public void ClearAnnotationGhost()
    {
        ghostDragging = false;
        if (ghost is not { } gh) { return; }
        ghost = null;
        gh.Image.Dispose();
        if (gh.Document == document) // nach dem Neuladen gibt es das alte Dokument nicht mehr
        {
            if (gh.Hidden) { SetOriginalHidden(gh, false); }
            else if (gh.Moved) { RefreshOriginalArea(gh); } // schon wieder eingeblendet (RestoreHiddenAnnotationAsync), die Kacheln zeigen es aber noch nicht
        }
        Invalidate();
    }

    /// <summary>Das Original nur in den Flags wieder einblenden, ohne neu zu rendern – vor dem Merken des Stands beim Verschieben
    /// (sonst ginge es ausgeblendet in den Stand ein); die Kacheln und die Kopie an der neuen Stelle bleiben, bis neu angezeigt wird.</summary>
    public async Task RestoreHiddenAnnotationAsync()
    {
        if (ghost is not { Hidden: true } gh || gh.Document != document) { return; }
        gh.Hidden = false;
        await gh.Document.SetAnnotationHiddenAsync(gh.Page, gh.Number, false);
    }

    /// <summary>Die Anmerkung unter der Maus (aus der letzten Abfrage beim Überfahren), wenn die Stelle in ihrem Rechteck liegt.</summary>
    private (int Page, int Number, int Subtype, Rectangle Rect, Size PageSize, Point Point)? AnnotationUnder(Point client)
    {
        if (!AllowAnnotationMove || hoverAnnotation is not { } hover || hover.Page >= pageRects.Length || hover.PageSize != pageRects[hover.Page].Size) { return null; }
        var (page, point) = PageAt(client, clamp: false);
        return page == hover.Page && hover.Rect.Contains(point) ? (hover.Page, hover.Number, hover.Subtype, hover.Rect, hover.PageSize, point) : null;
    }

    private bool TryStartAnnotationDrag(Point client)
    {
        if (ghost != null || document == null || AnnotationUnder(client) is not { } hit) { return false; }
        ghost = new AnnotationGhost(document, hit.Page, hit.Number, hit.Subtype, hit.Rect, hit.PageSize, hit.Point, CopyPageArea(hit.Page, hit.Rect));
        ghostDragging = true;
        Cursor = Cursors.SizeAll;
        return true;
    }

    private bool TryAnnotationDoubleClick(Point client)
    {
        if (AnnotationUnder(client) is not { } hit) { return false; }
        ClearAnnotationGhost();
        annotationClick = true;
        var args = new AnnotationEventArgs(hit.Page, hit.Number, hit.Subtype);
        BeginInvoke(() => AnnotationDoubleClicked?.Invoke(this, args)); // erst nach der Mausnachricht – der Empfänger öffnet einen Dialog
        return true;
    }

    /// <summary>Original nur in der Anzeige aus- bzw. wieder einblenden und seine Fläche neu rendern (bis dahin bleibt das alte Bild).</summary>
    private async void SetOriginalHidden(AnnotationGhost gh, bool hidden)
    {
        gh.Hidden = hidden;
        if (!await gh.Document.SetAnnotationHiddenAsync(gh.Page, gh.Number, hidden) || gh.Document != document || gh.Page >= pageRects.Length) { return; }
        RefreshOriginalArea(gh);
    }

    private void RefreshOriginalArea(AnnotationGhost gh)
    {
        if (gh.Page >= pageRects.Length) { return; }
        var size = pageRects[gh.Page].Size;
        var (sx, sy) = ((double)size.Width / gh.PageSize.Width, (double)size.Height / gh.PageSize.Height);
        var area = Rectangle.FromLTRB((int)(gh.Rect.Left * sx) - 2, (int)(gh.Rect.Top * sy) - 2, (int)Math.Ceiling(gh.Rect.Right * sx) + 2, (int)Math.Ceiling(gh.Rect.Bottom * sy) + 2);
        RefreshPageArea(gh.Page, area);
    }

    private void DragAnnotation(Point client)
    {
        if (ghost is not { } gh || gh.Page >= pageRects.Length) { return; }
        static int Keep(int value, int min, int max) => max < min ? min : Math.Clamp(value, min, max);
        var point = ToPagePoint(gh.Page, client);
        var (dx, dy) = (point.X - gh.Start.X, point.Y - gh.Start.Y);
        if (!gh.Moved && (Math.Abs(dx) >= SystemInformation.DragSize.Width || Math.Abs(dy) >= SystemInformation.DragSize.Height))
        {
            gh.Moved = true;
            SetOriginalHidden(gh, true); // erst jetzt: ein bloßer Klick soll nichts flackern lassen
        }
        gh.Offset = new Size(Keep(dx, -gh.Rect.X, gh.PageSize.Width - gh.Rect.Right), Keep(dy, -gh.Rect.Y, gh.PageSize.Height - gh.Rect.Bottom));
        Invalidate();
    }

    private async Task FinishAnnotationDragAsync()
    {
        ghostDragging = false;
        if (ghost is not { } gh || document is not { } doc) { return; }
        if (!gh.Moved || gh.Offset == Size.Empty || gh.Page >= pageRects.Length || gh.PageSize != pageRects[gh.Page].Size) { ClearAnnotationGhost(); return; }
        // Versatz über PDFium in PDF-Punkte umrechnen – so stimmen auch gedrehte Seiten und Ansichten
        var from = await doc.PagePointAsync(gh.Page, gh.PageSize.Width, gh.PageSize.Height, gh.Start);
        var to = await doc.PagePointAsync(gh.Page, gh.PageSize.Width, gh.PageSize.Height, gh.Start + gh.Offset);
        if (ghost != gh) { return; }
        if (doc != document || from is not { } a || to is not { } b) { ClearAnnotationGhost(); return; }
        AnnotationMoved?.Invoke(this, new AnnotationMovedEventArgs(gh.Page, gh.Number, gh.Subtype, b.X - a.X, b.Y - a.Y));
    }

    /// <summary>Ein Ausschnitt der Seite (Seitenpixel), wie er gerade zu sehen ist: Vorschaubild, darüber die vorhandenen Kacheln.</summary>
    private Bitmap CopyPageArea(int page, Rectangle area)
    {
        var image = new Bitmap(Math.Max(1, area.Width), Math.Max(1, area.Height));
        using var g = Graphics.FromImage(image);
        g.Clear(Color.White);
        if (previews.TryGetValue(page, out var preview)) { g.DrawImage(preview, new Rectangle(-area.X, -area.Y, pageRects[page].Width, pageRects[page].Height)); }
        foreach (var (key, tile) in tiles)
        {
            if (key.Page == page) { g.DrawImageUnscaled(tile, key.X * TileSize - area.X, key.Y * TileSize - area.Y); }
        }
        return image;
    }

    private void DrawAnnotationGhost(Graphics g)
    {
        if (ghost is not { Moved: true } gh || gh.Page >= pageRects.Length) { return; }
        var page = ToClient(pageRects[gh.Page]);
        var (sx, sy) = ((float)page.Width / gh.PageSize.Width, (float)page.Height / gh.PageSize.Height); // Zoom inzwischen geändert
        var rect = gh.Rect;
        rect.Offset(gh.Offset.Width, gh.Offset.Height);
        var target = Rectangle.Round(new RectangleF(page.X + rect.X * sx, page.Y + rect.Y * sy, rect.Width * sx, rect.Height * sy));
        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix { Matrix33 = 0.7f });
        g.DrawImage(gh.Image, target, 0, 0, gh.Image.Width, gh.Image.Height, GraphicsUnit.Pixel, attributes);
        using var pen = new Pen(Color.FromArgb(0, 120, 215)) { DashStyle = DashStyle.Dash };
        g.DrawRectangle(pen, target);
    }

    // ================================================================== Zeichnen (Freihand)

    // Mit InkMode zeichnet die linke Maustaste auf der Seite einen Freihand-Strich: Er erscheint sofort (eigene Zeichnung über den
    // Kacheln) und geht beim Loslassen als Ink-Anmerkung ans Hilfsprogramm; die eigene Zeichnung bleibt stehen, bis die neu gerenderten
    // Kacheln ihn zeigen. InkAdded meldet den fertigen Strich (speichern muss der Aufrufer).
    private sealed class InkStroke(int page, Size pageSize, Color color, float widthPt)
    {
        public int Page { get; } = page;
        public Size PageSize { get; } = pageSize;  // Seitenpixel beim Zeichnen (Zoom kann sich danach ändern)
        public Color Color { get; } = color;
        public float WidthPt { get; } = widthPt;
        public List<Point> Points { get; } = [];   // Seitenpixel
        public bool Sent { get; set; }             // ans Hilfsprogramm übergeben: weg, sobald die Kacheln frisch sind
        public bool Straight { get; set; }         // Strg gedrückt: gerade Linie
        public int ConstraintStart { get; set; }   // Punkt, an dem Strg kam: von dort geht die gerade Linie aus
        public int Direction { get; set; }         // 0 = noch offen, 1 = waagerecht, 2 = senkrecht (nach der ersten Bewegung)
    }

    private const int DirectionThreshold = 6;      // logische px Mausweg, bis bei gedrückter Strg-Taste die Richtung feststeht
    private InkStroke? inkStroke;                  // wird gerade gezeichnet
    private readonly List<InkStroke> pendingInk = [];

    /// <summary>Zeichenmodus: Ziehen mit der linken Maustaste legt Freihand-Striche an.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public bool InkMode
    {
        get;
        set
        {
            field = value;
            inkStroke = null;
            if (value) { CancelPick(); ClearSelection(); Cursor = Cursors.Cross; } else { Cursor = Cursors.Default; }
            Invalidate();
        }
    }

    /// <summary>Farbe der Striche.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public Color InkColor { get; set; } = Color.FromArgb(0, 80, 230);

    /// <summary>Strichstärke in Punkt.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public float InkWidth { get; set; } = 2f;

    /// <summary>Ein Strich beginnt (Maustaste auf der Seite gedrückt) – etwa um den Änderungsstand davor zu merken.</summary>
    public event EventHandler? InkStarted;

    /// <summary>Ein Strich ist im Dokument angelegt; dazu der Stand davor (für Rückgängig des Aufrufers).</summary>
    public event EventHandler<InkAddedEventArgs>? InkAdded;

    private bool TryStartInk(Point client)
    {
        if (!InkMode || document == null) { return false; }
        var (page, point) = PageAt(client, clamp: false);
        if (page < 0) { return true; } // neben den Seiten: nichts, aber auch keine Textauswahl
        inkStroke = new InkStroke(page, pageRects[page].Size, InkColor, InkWidth);
        inkStroke.Points.Add(point);
        Invalidate();
        InkStarted?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private void ContinueInk(Point client)
    {
        if (inkStroke is not { } stroke || stroke.Page >= pageRects.Length) { return; }
        var size = pageRects[stroke.Page].Size;
        var p = ToPagePoint(stroke.Page, client);
        p = new Point(Math.Clamp(p.X, 0, size.Width - 1), Math.Clamp(p.Y, 0, size.Height - 1)); // auf der Seite bleiben
        // gerade Linien (Wunsch vom 03.10.2026): solange Strg gedrückt ist, eine waagerechte oder senkrechte Linie vom Punkt aus, an dem
        // die Taste kam – die ersten Pixel der Bewegung entscheiden die Richtung, sie bleibt bis zum Loslassen; die Zwischenpunkte fallen
        // weg, danach geht es frei weiter
        var straight = (ModifierKeys & (Keys.Control | Keys.Alt | Keys.Shift)) == Keys.Control;
        if (straight != stroke.Straight) { (stroke.Straight, stroke.ConstraintStart, stroke.Direction) = (straight, stroke.Points.Count - 1, 0); }
        if (straight)
        {
            var anchor = stroke.Points[stroke.ConstraintStart];
            var (dx, dy) = (Math.Abs(p.X - anchor.X), Math.Abs(p.Y - anchor.Y));
            if (stroke.Direction == 0)
            {
                if (Math.Max(dx, dy) < LogicalToDeviceUnits(DirectionThreshold)) { return; } // Richtung noch offen
                stroke.Direction = dx >= dy ? 1 : 2;
            }
            p = stroke.Direction == 1 ? new Point(p.X, anchor.Y) : new Point(anchor.X, p.Y);
            stroke.Points.RemoveRange(stroke.ConstraintStart + 1, stroke.Points.Count - stroke.ConstraintStart - 1);
        }
        if (stroke.Points[^1] == p) { Invalidate(); return; }
        stroke.Points.Add(p);
        Invalidate();
    }

    private async Task FinishInkAsync()
    {
        if (inkStroke is not { } stroke || document is not { } doc) { inkStroke = null; return; }
        inkStroke = null;
        if (stroke.PageSize != pageRects[stroke.Page].Size) { Invalidate(); return; } // Zoom während des Zeichnens geändert
        pendingInk.Add(stroke);
        byte[] before;
        try { before = await doc.SaveAsync(); } // der Stand vor dem Strich – bis dahin bleibt er als eigene Zeichnung stehen
        catch (InvalidOperationException) { pendingInk.Remove(stroke); Invalidate(); return; }
        if (doc != document) { return; }
        var color = stroke.Color.R << 16 | stroke.Color.G << 8 | stroke.Color.B;
        var added = await doc.AddInkAsync(stroke.Page, stroke.PageSize.Width, stroke.PageSize.Height, color, stroke.WidthPt, Simplify(stroke.Points));
        if (doc != document) { return; }
        if (!added) { pendingInk.Remove(stroke); Invalidate(); return; }
        stroke.Sent = true;
        InkAdded?.Invoke(this, new InkAddedEventArgs(stroke.Page, before));
    }

    /// <summary>Punkte, die fast auf der Verbindungslinie ihrer Nachbarn liegen, fallen weg (Ramer-Douglas-Peucker, 0,5 Pixel) – die
    /// Maus liefert oft Hunderte Punkte je Strich.</summary>
    private static List<Point> Simplify(List<Point> points)
    {
        if (points.Count < 3) { return [.. points]; }
        var keep = new bool[points.Count];
        keep[0] = keep[^1] = true;
        var stack = new Stack<(int First, int Last)>();
        stack.Push((0, points.Count - 1));
        while (stack.Count > 0)
        {
            var (first, last) = stack.Pop();
            var (a, b) = (points[first], points[last]);
            double dx = b.X - a.X, dy = b.Y - a.Y, length = Math.Sqrt(dx * dx + dy * dy);
            var (index, max) = (-1, 0.5);
            for (var i = first + 1; i < last; i++)
            {
                var p = points[i];
                var distance = length < 0.001 ? Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y)) : Math.Abs(dy * (p.X - a.X) - dx * (p.Y - a.Y)) / length;
                if (distance > max) { (index, max) = (i, distance); }
            }
            if (index < 0) { continue; }
            keep[index] = true;
            stack.Push((first, index));
            stack.Push((index, last));
        }
        return [.. points.Where((_, i) => keep[i])];
    }

    private void DrawInk(Graphics g)
    {
        var strokes = inkStroke is { } current ? pendingInk.Append(current) : pendingInk;
        foreach (var stroke in strokes.ToList())
        {
            if (stroke.Page >= pageRects.Length) { continue; }
            if (stroke.Sent && !staleTiles.Any(k => k.Page == stroke.Page)) { pendingInk.Remove(stroke); continue; } // die Kacheln zeigen ihn jetzt selbst
            var page = ToClient(pageRects[stroke.Page]);
            var (sx, sy) = ((float)page.Width / stroke.PageSize.Width, (float)page.Height / stroke.PageSize.Height);
            var points = stroke.Points.Select(p => new PointF(page.X + p.X * sx, page.Y + p.Y * sy)).ToArray();
            var width = Math.Max(1f, stroke.WidthPt * PixelsPerPoint);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (points.Length == 1)
            {
                using var brush = new SolidBrush(stroke.Color);
                g.FillEllipse(brush, points[0].X - width / 2, points[0].Y - width / 2, width, width);
                continue;
            }
            using var pen = new Pen(stroke.Color, width) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            g.DrawLines(pen, points);
        }
    }

    // ================================================================== Maus und Tastatur

    private (int Page, Point Point) dragStart;

    /// <summary>Klick: hebt die Markierung auf und merkt sich die Stelle – die Markierung entsteht erst beim Ziehen (Anfang = das der
    /// Stelle nächste Zeichen, auch wenn sie neben dem Text liegt). Doppelklick: das Wort unter der Maus.</summary>
    protected async override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        if (pick != null) // Stelle wählen: der Klick gehört nur dem Fadenkreuz
        {
            if (e.Button == MouseButtons.Left) { EndPick(PageAt(e.Location, clamp: false).Page >= 0 ? e.Location : null); }
            return;
        }
        if (e.Button == MouseButtons.Right) { ContextMenuPoint = e.Location; }
        if (e.Button == MouseButtons.Right && document != null) { markupQuery = QueryMarkupAsync(e.Location); } // fürs Kontextmenü
        if (e.Button != MouseButtons.Left || document == null || pageRects.Length == 0) { return; }
        if (TryStartInk(e.Location)) { return; } // Zeichenmodus
        if (e.Clicks == 1 && TryStartAnnotationDrag(e.Location)) { ClearSelection(); return; } // Textanmerkung oder Stempel verschieben
        if (e.Clicks == 2 && TryAnnotationDoubleClick(e.Location)) { ClearSelection(); return; } // … bearbeiten
        var (page, point) = PageAt(e.Location, clamp: true);
        dragStart = (page, point); // schon hier: wird losgelassen, bevor PDFium geantwortet hat, sucht OnMouseUp hier nach einem Link
        if (document.HasForms)
        {
            // erst PDFium fragen: Liegt dort ein Feld, gehört ihm der Klick (Textauswahl und Links bleiben dann aus)
            var doc = document;
            var onPage = PageAt(e.Location, clamp: false).Page >= 0;
            var size = pageRects[page].Size;
            var press = formPress = onPage ? doc.FormMouseDownAsync(page, size.Width, size.Height, point, FormModifiers(ModifierKeys, true)) : Task.FromResult(-1);
            var field = await press;
            if (doc != document) { return; }
            if (field >= 0)
            {
                formFocused = true;
                if (formPress == press) { formDragPage = page; } // noch gedrückt (sonst hat OnMouseUp schon übernommen)
                ClearSelection();
                return;
            }
            if (!onPage && formFocused) { await doc.FormKillFocusAsync(); } // neben die Seiten geklickt
            formFocused = false;
            if (formPress != press && e.Clicks != 2) { return; } // schon losgelassen: kein Ziehen mehr beginnen – die Wortauswahl braucht keine gedrückte Taste
        }
        if (e.Clicks == 2)
        {
            dragging = false;
            var size = pageRects[page].Size;
            var doc = document;
            var index = await doc.CharAtAsync(page, size.Width, size.Height, point);
            if (doc != document || index < 0) { return; }
            var (start, count) = await doc.WordAtAsync(page, index);
            anchor = new TextPos(page, start);
            focus = new TextPos(page, start + count - 1);
            selectionActive = true;
            SelectionUpdated();
            SelectionFinished?.Invoke(this, EventArgs.Empty);
            return;
        }
        anchor = focus = null;
        selectionActive = false;
        dragStart = (page, point);
        dragging = true;
        dragCaptured = Capture;
        linkOnMouseDown = hoverLink; // Link unter der Maus: ihm folgen, wenn beim Loslassen nicht markiert wurde
        SelectionUpdated();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (pick != null)
        {
            Cursor = PageAt(e.Location, clamp: false).Page >= 0 ? Cursors.Cross : Cursors.No;
            return;
        }
        if (formDragPage >= 0 && document != null) // Ziehen in einem Feld: Text darin markieren
        {
            var size = pageRects[formDragPage].Size;
            _ = document.FormMouseMoveAsync(formDragPage, size.Width, size.Height, ToPagePoint(formDragPage, e.Location), FormModifiers(ModifierKeys, true));
            return;
        }
        if (InkMode) { ContinueInk(e.Location); return; }
        if (ghostDragging) { DragAnnotation(e.Location); return; }
        if (ghost != null) { return; } // verschoben, wartet aufs Neuladen: das Verschiebekreuz bleibt
        if (!dragging)
        {
            if (PageAt(e.Location, clamp: false).Page < 0) { Cursor = Cursors.Default; } // auf der Seite entscheidet die Link-Abfrage: Hand oder Textcursor
            pendingHoverPoint = e.Location;
            if (!hoverQueryRunning) { RunHoverQueries(); }
            return;
        }
        lastDragPoint = e.Location;
        autoScrollTimer.Enabled = AutoScrollDelta(e.Location) != Size.Empty;
        QueueDragQuery(e.Location);
    }

    private void QueueDragQuery(Point point)
    {
        pendingDragPoint = point;
        if (!dragQueryRunning) { RunDragQueries(); }
    }

    /// <summary>Wie weit die Maus außerhalb des sichtbaren Bereichs steht, als Scrollschritt (0 innerhalb).</summary>
    private Size AutoScrollDelta(Point point)
    {
        static int Step(int outside) => outside == 0 ? 0 : Math.Sign(outside) * Math.Clamp(Math.Abs(outside) / 2, AutoScrollMinStep, AutoScrollMaxStep);
        var dx = point.X < 0 ? point.X : point.X > ClientSize.Width ? point.X - ClientSize.Width : 0;
        var dy = point.Y < 0 ? point.Y : point.Y > ClientSize.Height ? point.Y - ClientSize.Height : 0;
        return new Size(Step(dx), Step(dy));
    }

    private void AutoScrollTimer_Tick(object? sender, EventArgs e)
    {
        var delta = AutoScrollDelta(lastDragPoint);
        if (!dragging || delta == Size.Empty) { autoScrollTimer.Stop(); return; }
        var before = AutoScrollPosition;
        ScrollBy(delta.Width, delta.Height);
        if (AutoScrollPosition != before) { QueueDragQuery(lastDragPoint); } // dieselbe Mausposition zeigt jetzt auf eine andere Stelle
    }

    private bool dragCaptured; // hatte das Steuerelement beim Drücken den Mausfang? (WinForms fängt die Maus beim Drücken ein)

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);
        if (dragCaptured && !Capture) { StopDragging(); } // gehaltenen Mausfang verloren (Fensterwechsel, Dialog): Ziehen beenden
        if (!Capture) { formDragPage = -1; }              // ebenso das Ziehen in einem Formularfeld
        // ebenso das Verschieben einer Textanmerkung – erst danach prüfen: beim Loslassen endet der Mausfang womöglich vor OnMouseUp
        if (ghostDragging && !Capture && IsHandleCreated) { BeginInvoke(() => { if (ghostDragging) { ClearAnnotationGhost(); } }); }
        if (inkStroke != null && !Capture && IsHandleCreated) { BeginInvoke(() => { if (inkStroke != null && !Capture) { inkStroke = null; Invalidate(); } }); } // ebenso ein angefangener Strich
    }

    private void StopDragging()
    {
        dragging = false;
        dragCaptured = false;
        autoScrollTimer.Stop();
    }

    /// <summary>Beim Ziehen immer nur eine Abfrage zugleich; was dazwischen an Mausbewegung kommt, zählt nur mit dem letzten Punkt.</summary>
    private async void RunDragQueries()
    {
        dragQueryRunning = true;
        try
        {
            while (pendingDragPoint is { } point && document != null)
            {
                pendingDragPoint = null;
                if (anchor == null) // erste Bewegung: Anfang = das Zeichen, das der Klickstelle am nächsten liegt
                {
                    var startSize = pageRects[dragStart.Page].Size;
                    var start = await document.NearestCharAsync(dragStart.Page, startSize.Width, startSize.Height, dragStart.Point);
                    if (start < 0 || !dragging) { continue; }
                    anchor = new TextPos(dragStart.Page, start);
                }
                var (page, pagePoint) = PageAt(point, clamp: true);
                var size = pageRects[page].Size;
                var index = await document.NearestCharAsync(page, size.Width, size.Height, pagePoint);
                if (index < 0 || focus == new TextPos(page, index)) { continue; } // Seite ohne Text oder keine Änderung
                focus = new TextPos(page, index);
                selectionActive = focus != anchor; // erst eine echte Strecke ist eine Markierung, kein einzelnes Zeichen unter dem Klick
                SelectionUpdated();
            }
        }
        finally { dragQueryRunning = false; }
    }

    protected async override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (InkMode)
        {
            if (e.Button == MouseButtons.Left) { await FinishInkAsync(); }
            return;
        }
        if (ghostDragging)
        {
            if (e.Button == MouseButtons.Left) { await FinishAnnotationDragAsync(); }
            return;
        }
        if (annotationClick) { annotationClick = false; return; }
        if (e.Button == MouseButtons.Left && formPress is { } press)
        {
            formPress = null;
            var doc = document;
                        var field = await press; // OnMouseDown hat danach schon entschieden (seine Fortsetzung läuft zuerst)
            if (field >= 0 && doc == document && doc != null)
            {
                var page = dragStart.Page;
                formDragPage = -1;
                var size = pageRects[page].Size;
                await doc.FormMouseUpAsync(page, size.Width, size.Height, ToPagePoint(page, e.Location), FormModifiers(ModifierKeys, false));
                return;
            }
        }
        var wasDragging = dragging;
        StopDragging();
        if (wasDragging && e.Button == MouseButtons.Left && SelectionFinished != null)
        {
            for (var i = 0; i < 100 && (dragQueryRunning || pendingDragPoint != null); i++) { await Task.Delay(10); } // die letzte Zeichenabfrage abwarten
            if (selectionActive) { SelectionFinished.Invoke(this, EventArgs.Empty); }
        }
        var link = linkOnMouseDown;
        linkOnMouseDown = null;
        if (link == null && e.Button == MouseButtons.Left && !selectionActive && document != null && dragStart.Page < pageRects.Length)
        {
            // die Abfrage beim Überfahren war noch nicht fertig (schneller Klick): direkt an der Klickstelle nachsehen
            var size = pageRects[dragStart.Page].Size;
            link = await document.LinkAtAsync(dragStart.Page, size.Width, size.Height, dragStart.Point);
        }
        if (e.Button == MouseButtons.Left && link != null && !selectionActive) { FollowLink(link); } // gezogen = markiert, nicht geklickt
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if ((ModifierKeys & Keys.Control) == Keys.Control)
        {
            if (e.Delta > 0) { SetZoom(ZoomSteps.FirstOrDefault(s => s > zoom * 1.01f, ZoomSteps[^1]), e.Location); }
            else { SetZoom(ZoomSteps.LastOrDefault(s => s < zoom * 0.99f, ZoomSteps[0]), e.Location); }
            return;
        }
        base.OnMouseWheel(e);
        UpdateCurrentPage();
        Invalidate();
    }

    protected override void OnScroll(ScrollEventArgs se)
    {
        base.OnScroll(se);
        UpdateCurrentPage();
        Invalidate();
    }

    protected override bool IsInputKey(Keys keyData) =>
        (keyData & Keys.KeyCode) is Keys.Up or Keys.Down or Keys.Left or Keys.Right or Keys.PageUp or Keys.PageDown or Keys.Home or Keys.End
        || (formFocused && (keyData & Keys.KeyCode) is Keys.Tab or Keys.Enter or Keys.Escape) // Tab: nächstes Feld, Enter: Zeilenumbruch
        || base.IsInputKey(keyData);

    protected async override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (pick != null && e.KeyCode == Keys.Escape) { e.Handled = true; CancelPick(); return; }
        if (ghostDragging && e.KeyCode == Keys.Escape) { e.Handled = true; ClearAnnotationGhost(); return; }
        if (inkStroke != null && e.KeyCode == Keys.Escape) { e.Handled = true; inkStroke = null; Invalidate(); return; } // nur den angefangenen Strich verwerfen
        if (formFocused && !e.Handled)
        {
            e.Handled = true; // die Antwort von PDFium kommt erst nach dem Ereignis – was es nicht nimmt, erledigt die Ansicht danach
            if (await FormKeyDownAsync(e)) { return; }
        }
        var line = LogicalToDeviceUnits(40);
        switch (e.KeyData)
        {
            case Keys.Control | Keys.C: e.Handled = true; await CopySelectionAsync(); break;
            case Keys.Control | Keys.A: e.Handled = true; SelectAll(); break;
            case Keys.Control | Keys.Add:
            case Keys.Control | Keys.Oemplus: e.Handled = true; ZoomIn(); break;
            case Keys.Control | Keys.Subtract:
            case Keys.Control | Keys.OemMinus: e.Handled = true; ZoomOut(); break;
            case Keys.Control | Keys.D0:
            case Keys.Control | Keys.NumPad0: e.Handled = true; SetZoom(1f, null); break;
            case Keys.Escape: ClearSelection(); break;
            case Keys.Down: ScrollBy(0, line); break;
            case Keys.Up: ScrollBy(0, -line); break;
            case Keys.Right: ScrollBy(line, 0); break;
            case Keys.Left: ScrollBy(-line, 0); break;
            case Keys.PageDown: ScrollBy(0, ClientSize.Height - line); break;
            case Keys.PageUp: ScrollBy(0, -(ClientSize.Height - line)); break;
            case Keys.Control | Keys.Home: GoToPage(0); break;
            case Keys.Control | Keys.End: if (pageRects.Length > 0) { GoToPage(pageRects.Length - 1); } break;
        }
    }

    private void ScrollBy(int dx, int dy)
    {
        AutoScrollPosition = new Point(-AutoScrollPosition.X + dx, -AutoScrollPosition.Y + dy);
        UpdateCurrentPage();
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            autoScrollTimer.Dispose();
            ClearTiles();
            foreach (var bitmap in previews.Values) { bitmap.Dispose(); }
            previews.Clear();
        }
        base.Dispose(disposing);
    }
}

/// <summary>Anpassung der Zoomstufe an das Fenster: keine (feste Stufe), an die Breite, an die Höhe oder ganze Seite.</summary>
internal enum PageFit { None, Width, Height, Page }

/// <summary>Eine Freitext-Anmerkung oder ein Stempel: Seite (0-basiert), Position im /Annots-Array und Art (FPDF_ANNOT_*).</summary>
internal class AnnotationEventArgs(int page, int number, int subtype) : EventArgs
{
    public int Page { get; } = page;
    public int Number { get; } = number;
    public int Subtype { get; } = subtype;
    public bool IsStamp => Subtype == Protocol.AnnotStamp;
}

/// <summary>Eine Anmerkung wurde gezogen: dazu der Versatz in PDF-Punkten (y nach oben).</summary>
internal sealed class AnnotationMovedEventArgs(int page, int number, int subtype, double dx, double dy) : AnnotationEventArgs(page, number, subtype)
{
    public double Dx { get; } = dx;
    public double Dy { get; } = dy;
}

/// <summary>Ein Freihand-Strich ist im Dokument: Seite (0-basiert) und der vollständige Stand davor (PDF-Bytes, für Rückgängig).</summary>
internal sealed class InkAddedEventArgs(int page, byte[] before) : EventArgs
{
    public int Page { get; } = page;
    public byte[] Before { get; } = before;
}
