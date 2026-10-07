using System.ComponentModel;

namespace PDFLight.Viewer;

/// <summary>Selbst gezeichnete Seitenminiaturen für die Seitenleiste: eine Spalte, so breit wie das Steuerelement (mit
/// <see cref="SingleColumn"/> = false ein Raster mit fester Zellgröße, Strg+Mausrad ändert sie). Gerendert werden nur die sichtbaren
/// Seiten (im Hilfsprogramm); Aufträge für Seiten, die inzwischen aus dem Bild gescrollt sind, verfallen. Fertige Miniaturen liegen in
/// einem begrenzten Zwischenspeicher. Die angezeigte Seite (<see cref="CurrentPage"/>) ist hervorgehoben; ein Klick oder die
/// Pfeiltasten melden die gewählte Seite über <see cref="PageActivated"/>.</summary>
internal sealed class ThumbnailGrid : ScrollableControl
{
    private const int CellPadding = 12;     // Abstand zwischen den Zellen (logische px)
    private const int ColumnLeft = 32, ColumnRight = 48; // Rand der Spalte (Seitenleiste): rechts etwas mehr – links steht schon die Umschaltleiste
    private const int LabelHeight = 20;     // Platz für die Seitennummer unter der Miniatur
    private const int MinSize = 60, MaxSize = 480, SizeStep = 40;
    private const int CacheLimit = 300;     // so viele fertige Miniaturen höchstens im Speicher

    private PdfiumDocument? document;
    private readonly Dictionary<int, Bitmap> cache = [];
    private readonly HashSet<int> pending = [];
    private int generation;                 // jede neue Datei oder Größe macht laufende Aufträge ungültig
    private int thumbnailSize = 160;
    private int currentPage = -1;
    private bool singleColumn = true;
    private Size lastBox;
    private double tallest = 1.414;         // größtes Verhältnis Höhe/Breite der Seiten: Feldhöhe in der Spalte
    private Color currentPageColor = Color.FromArgb(0, 120, 215);

    /// <summary>Klick auf eine Miniatur oder Pfeiltaste – Seitennummer 0-basiert.</summary>
    public event EventHandler<int>? PageActivated;

    public ThumbnailGrid()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        AutoScroll = true;
        SetStyle(ControlStyles.Selectable, true);
    }

    /// <summary>Eine Spalte in voller Breite (Seitenleiste) statt eines Rasters mit fester Zellgröße.</summary>
    [Category("Darstellung"), DefaultValue(true), Description("Eine Spalte in voller Breite statt eines Rasters mit fester Zellgröße.")]
    public bool SingleColumn
    {
        get => singleColumn;
        set { singleColumn = value; ClearCache(); UpdateScrollSize(); Invalidate(); }
    }

    /// <summary>Farbe des Rahmens um die Miniatur der angezeigten Seite.</summary>
    [Category("Darstellung"), DefaultValue(typeof(Color), "0, 120, 215"), Description("Farbe des Rahmens um die Miniatur der angezeigten Seite.")]
    public Color CurrentPageColor
    {
        get => currentPageColor;
        set { currentPageColor = value; Invalidate(); }
    }

    /// <summary>Breite des Rahmens um die Miniatur der angezeigten Seite (logische Pixel; Chrome: 6).</summary>
    [Category("Darstellung"), DefaultValue(6), Description("Breite des Rahmens um die Miniatur der angezeigten Seite (logische Pixel).")]
    public int CurrentPageBorderWidth
    {
        get;
        set { field = Math.Clamp(value, 1, CellPadding / 2 + 1); Invalidate(); } // breiter ließe der Zellabstand nicht zu
    } = 6;

    /// <summary>Kantenlänge des quadratischen Feldes je Miniatur in logischen Pixeln (nur im Raster).</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public int ThumbnailSize
    {
        get => thumbnailSize;
        set
        {
            var size = Math.Clamp(value, MinSize, MaxSize);
            if (size == thumbnailSize) { return; }
            thumbnailSize = size;
            ClearCache();
            UpdateScrollSize();
            Invalidate();
        }
    }

    /// <summary>Die angezeigte Seite (0-basiert, -1 = keine): hervorgehoben und bei Bedarf ins Bild gescrollt.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public int CurrentPage
    {
        get => currentPage;
        set
        {
            if (value == currentPage) { return; }
            currentPage = value;
            EnsureVisible(value);
            Invalidate();
        }
    }

    /// <summary>Zeigt ein Dokument (oder keins). Das bisherige gibt der Aufrufer selbst frei.</summary>
    public void SetDocument(PdfiumDocument? value)
    {
        document = value;
        currentPage = -1;
        tallest = value is { PageCount: > 0 } ? Math.Clamp(value.PageSizes.Max(p => p.Height / Math.Max(1, p.Width)), 0.25, 4) : 1.414;
        lastBox = Box;
        ClearCache();
        AutoScrollPosition = Point.Empty;
        UpdateScrollSize();
        Invalidate();
    }

    /// <summary>Verwirft die Miniatur einer Seite (sie hat sich geändert: Formularwert, Hervorhebung) – beim nächsten Zeichnen neu.</summary>
    public void InvalidatePage(int index)
    {
        if (!cache.Remove(index, out var bitmap)) { return; }
        bitmap.Dispose();
        if (document != null && index < document.PageCount) { Invalidate(CellBounds(index)); }
    }

    private void ClearCache()
    {
        generation++;
        foreach (var bitmap in cache.Values) { bitmap.Dispose(); }
        cache.Clear();
        pending.Clear();
    }

    // ================================================================== Raster

    private int Gap => LogicalToDeviceUnits(CellPadding);
    // Feld je Miniatur: im Raster quadratisch, in der Spalte (Seitenleiste) so breit wie die Leiste und so hoch wie die höchste Seite –
    // Hochformat-Seiten füllen dann die Breite, statt in einem Quadrat mittig mit breitem Rand zu stehen (Wunsch vom 03.10.2026)
    private Size Box
    {
        get
        {
            if (!singleColumn) { var side = LogicalToDeviceUnits(thumbnailSize); return new Size(side, side); }
            var width = Math.Max(LogicalToDeviceUnits(MinSize), ClientSize.Width - LogicalToDeviceUnits(ColumnLeft + ColumnRight));
            return new Size(width, Math.Max(1, (int)Math.Round(width * tallest)));
        }
    }
    private int CellWidth => Box.Width + Gap;
    private int CellHeight => Box.Height + LogicalToDeviceUnits(LabelHeight) + Gap;
    private int Columns => singleColumn ? 1 : Math.Max(1, (ClientSize.Width - Gap) / CellWidth);

    private void UpdateScrollSize()
    {
        if (document == null) { AutoScrollMinSize = Size.Empty; return; }
        var rows = (document.PageCount + Columns - 1) / Columns;
        AutoScrollMinSize = new Size(0, rows * CellHeight + Gap);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (singleColumn && Box != lastBox) { lastBox = Box; ClearCache(); } // die Miniaturen wachsen mit der Breite der Leiste
        UpdateScrollSize();
    }

    /// <summary>Feld der Seite im Steuerelement (Bildlauf berücksichtigt); das Raster steht waagerecht in der Mitte.</summary>
    private Rectangle CellBounds(int index)
    {
        var columns = Columns;
        var left = singleColumn ? LogicalToDeviceUnits(ColumnLeft) : (ClientSize.Width - columns * CellWidth + Gap) / 2; // Spalte mit festem Rand, das Raster mittig
        return new Rectangle(new Point(left + index % columns * CellWidth, Gap + index / columns * CellHeight + AutoScrollPosition.Y), Box);
    }

    /// <summary>Die Miniatur selbst im Feld (mittig) – Seitenverhältnis der Seite, so groß, wie das Feld zulässt.</summary>
    private Rectangle ThumbBounds(int index)
    {
        var cell = CellBounds(index);
        var page = document!.PageSizes[index]; // nur mit Dokument aufgerufen
        var scale = Math.Min(cell.Width / page.Width, cell.Height / page.Height);
        var size = new Size(Math.Max(1, (int)Math.Round(page.Width * scale)), Math.Max(1, (int)Math.Round(page.Height * scale)));
        return new Rectangle(cell.X + (cell.Width - size.Width) / 2, cell.Y + (cell.Height - size.Height) / 2, size.Width, size.Height);
    }

    private IEnumerable<int> VisiblePages()
    {
        if (document == null) { yield break; }
        var columns = Columns;
        var firstRow = Math.Max(0, (-AutoScrollPosition.Y - Gap) / CellHeight);
        var lastRow = (-AutoScrollPosition.Y + ClientSize.Height) / CellHeight;
        for (var i = firstRow * columns; i < Math.Min(document.PageCount, (lastRow + 1) * columns); i++) { yield return i; }
    }

    private bool IsVisible(int index) => ClientRectangle.IntersectsWith(CellBounds(index));

    /// <summary>Scrollt nur, wenn die Miniatur samt Nummer nicht vollständig zu sehen ist – dann steht sie oben bzw. unten am Rand.</summary>
    private void EnsureVisible(int index)
    {
        if (document == null || index < 0 || index >= document.PageCount) { return; }
        var cell = CellBounds(index);
        var top = cell.Top - Gap / 2;
        var bottom = cell.Bottom + LogicalToDeviceUnits(LabelHeight) + Gap / 2;
        if (top >= 0 && bottom <= ClientSize.Height) { return; }
        var y = -AutoScrollPosition.Y + (top < 0 ? top : bottom - ClientSize.Height);
        AutoScrollPosition = new Point(0, Math.Max(0, y));
        Invalidate();
    }

    // ================================================================== Zeichnen und Rendern

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (document == null) { return; }
        using var numberFont = new Font(Font.FontFamily, 9f);
        foreach (var index in VisiblePages())
        {
            var cell = CellBounds(index);
            var thumb = ThumbBounds(index);
            if (cache.TryGetValue(index, out var bitmap)) { e.Graphics.DrawImage(bitmap, thumb); }
            else
            {
                e.Graphics.FillRectangle(Brushes.White, thumb); // Platzhalter, bis die Miniatur fertig ist
                Request(index, thumb.Size);
            }
            if (index == currentPage)
            {
                var width = LogicalToDeviceUnits(CurrentPageBorderWidth);
                using var pen = new Pen(currentPageColor, width) { Alignment = System.Drawing.Drawing2D.PenAlignment.Inset };
                e.Graphics.DrawRectangle(pen, thumb.X - width, thumb.Y - width, thumb.Width + 2 * width - 1, thumb.Height + 2 * width - 1);
            }
            else { e.Graphics.DrawRectangle(Pens.Gray, thumb.X - 1, thumb.Y - 1, thumb.Width + 1, thumb.Height + 1); }
            var label = new Rectangle(cell.X, cell.Bottom, cell.Width, LogicalToDeviceUnits(LabelHeight));
            TextRenderer.DrawText(e.Graphics, (index + 1).ToString(), numberFont, label, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    private async void Request(int index, Size pixels)
    {
        if (document == null || !pending.Add(index)) { return; }
        var requestGeneration = generation;
        var doc = document;
        Bitmap? bitmap;
        try
        {
            // die Sichtbarkeitsprüfung fragt das Steuerelement – vom Sandbox-Thread aus per Invoke
            bitmap = await doc.RenderAsync(index, pixels.Width, pixels.Height,
                () => requestGeneration == generation && !IsDisposed && (bool)Invoke(() => IsVisible(index)));
        }
        catch (InvalidOperationException) { bitmap = null; } // Fenster beim Schließen: Invoke geht nicht mehr
        if (requestGeneration != generation || IsDisposed)
        {
            bitmap?.Dispose();
            return;
        }
        pending.Remove(index);
        if (bitmap == null) { return; } // verfallen – beim nächsten Zeichnen wird neu angefragt, falls die Seite wieder sichtbar ist
        if (cache.Remove(index, out var old)) { old.Dispose(); }
        cache[index] = bitmap;
        TrimCache();
        Invalidate(CellBounds(index));
    }

    /// <summary>Hält den Zwischenspeicher klein: verwirft die Miniaturen, die am weitesten von der sichtbaren Mitte entfernt sind.</summary>
    private void TrimCache()
    {
        if (cache.Count <= CacheLimit) { return; }
        var visible = VisiblePages().ToList();
        var center = visible.Count > 0 ? visible[visible.Count / 2] : 0;
        foreach (var index in cache.Keys.OrderByDescending(i => Math.Abs(i - center)).Take(cache.Count - CacheLimit).ToList())
        {
            cache[index].Dispose();
            cache.Remove(index);
        }
    }

    protected override void OnScroll(ScrollEventArgs se)
    {
        base.OnScroll(se);
        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (!singleColumn && (ModifierKeys & Keys.Control) == Keys.Control)
        {
            ThumbnailSize += Math.Sign(e.Delta) * SizeStep;
            return;
        }
        base.OnMouseWheel(e);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus(); // für Mausrad und Pfeiltasten
        if (e.Button is not (MouseButtons.Left or MouseButtons.Right) || document == null) { return; } // rechts: die Seite fürs Kontextmenü anzeigen
        var hit = VisiblePages().FirstOrDefault(i => CellBounds(i).Contains(e.Location), -1);
        if (hit >= 0) { Activate(hit); }
    }

    private void Activate(int index)
    {
        if (document == null || index < 0 || index >= document.PageCount) { return; }
        CurrentPage = index;
        PageActivated?.Invoke(this, index);
    }

    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Up or Keys.Down or Keys.Left or Keys.Right or Keys.Home or Keys.End or Keys.PageUp or Keys.PageDown || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (document == null || document.PageCount == 0) { return; }
        var page = Math.Max(0, currentPage);
        var rows = Math.Max(1, ClientSize.Height / CellHeight);
        int? target = e.KeyData switch
        {
            Keys.Up => page - Columns,
            Keys.Down => page + Columns,
            Keys.Left => page - 1,
            Keys.Right => page + 1,
            Keys.PageUp => page - rows * Columns,
            Keys.PageDown => page + rows * Columns,
            Keys.Home => 0,
            Keys.End => document.PageCount - 1,
            _ => null,
        };
        if (target is not { } next) { return; }
        e.Handled = true;
        Activate(Math.Clamp(next, 0, document.PageCount - 1));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { ClearCache(); }
        base.Dispose(disposing);
    }
}
