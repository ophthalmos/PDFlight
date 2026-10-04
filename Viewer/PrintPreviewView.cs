using System.ComponentModel;
using System.Drawing.Imaging;

namespace PDFLight.Viewer;

/// <summary>Vorschau des Drucks wie im Druckdialog von Edge: die Druckbögen untereinander auf grauem Grund, jeder so groß, dass er in die
/// Höhe passt. Papierformat, bedruckbarer Bereich, Ausrichtung und Größe der Seite rechnet <see cref="PrintLayout"/> – genau wie beim
/// Druck. Gerendert werden nur die sichtbaren Bögen (im Hilfsprogramm), Aufträge für weggescrollte verfallen.</summary>
internal sealed class PrintPreviewView : ScrollableControl
{
    private const int Gap = 16;            // Abstand zwischen den Bögen und zum Rand (logische px)
    private const int CacheLimit = 40;

    private PdfiumDocument? document;
    private IReadOnlyList<int> pages = [];
    private SizeF paper = new(827, 1169);  // Hundertstel Zoll, Hochformat (A4)
    private RectangleF printable = new(0, 0, 827, 1169);
    private PrintOrientation orientation;
    private PrintScaling scaling;
    private int percent = 100;
    private bool grayscale;
    private Rectangle[] sheetRects = [];   // Bögen in Dokumentkoordinaten (Bildlauf: Client = Dokument + AutoScrollPosition)
    private readonly Dictionary<int, Bitmap> cache = [];
    private readonly HashSet<int> pending = [];
    private int generation;

    public PrintPreviewView()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(232, 232, 232);
        ResizeRedraw = true;
        AutoScroll = true;
        SetStyle(ControlStyles.Selectable, true);
    }

    /// <summary>Farbe der Fläche hinter den Bögen.</summary>
    [Category("Darstellung"), DefaultValue(typeof(Color), "232, 232, 232"), Description("Farbe der Fläche hinter den Bögen.")]
    public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }

    /// <summary>Zahl der Bögen in der Vorschau (je Seite einer).</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public int SheetCount => pages.Count;

    /// <summary>Stellt Dokument und Druckeinstellungen ein; <paramref name="paperSize"/> und <paramref name="printableArea"/> in Hundertstel
    /// Zoll für das Hochformat (der Bereich relativ zur linken oberen Papierecke).</summary>
    public void Configure(PdfiumDocument? source, IReadOnlyList<int> sheets, SizeF paperSize, RectangleF printableArea, PrintOrientation sheetOrientation,
        PrintScaling pageScaling, int scalePercent, bool gray)
    {
        document = source;
        pages = sheets;
        paper = paperSize.Width > 0 && paperSize.Height > 0 ? paperSize : new SizeF(827, 1169);
        printable = printableArea.Width > 0 && printableArea.Height > 0 ? printableArea : new RectangleF(PointF.Empty, paper);
        orientation = sheetOrientation;
        scaling = pageScaling;
        percent = scalePercent;
        grayscale = gray;
        ClearCache();
        LayoutSheets();
        Invalidate();
    }

    private void ClearCache()
    {
        generation++;
        foreach (var bitmap in cache.Values) { bitmap.Dispose(); }
        cache.Clear();
        pending.Clear();
    }

    // ================================================================== Bögen

    private bool Landscape(int sheet) => document != null && PrintLayout.IsLandscape(orientation, document.PageSizes[pages[sheet]]);

    private SizeF SheetPaper(int sheet) => Landscape(sheet) ? new SizeF(paper.Height, paper.Width) : paper;

    private RectangleF SheetPrintable(int sheet) => Landscape(sheet) ? PrintLayout.Rotate(printable, paper) : printable;

    /// <summary>Alle Bögen im selben Maßstab: der größte passt ganz ins Fenster.</summary>
    private void LayoutSheets()
    {
        if (document == null || pages.Count == 0) { sheetRects = []; AutoScrollMinSize = Size.Empty; return; }
        var gap = LogicalToDeviceUnits(Gap);
        var maxWidth = Enumerable.Range(0, pages.Count).Max(s => SheetPaper(s).Width);
        var maxHeight = Enumerable.Range(0, pages.Count).Max(s => SheetPaper(s).Height);
        var scale = Math.Min((ClientSize.Width - 2 * gap - SystemInformation.VerticalScrollBarWidth) / maxWidth, (ClientSize.Height - 2 * gap) / maxHeight);
        scale = Math.Max(scale, 0.05f);
        var rects = new Rectangle[pages.Count];
        var y = gap;
        for (var s = 0; s < pages.Count; s++)
        {
            var size = SheetPaper(s);
            var width = Math.Max(1, (int)(size.Width * scale));
            var height = Math.Max(1, (int)(size.Height * scale));
            rects[s] = new Rectangle((ClientSize.Width - SystemInformation.VerticalScrollBarWidth - width) / 2, y, width, height);
            y += height + gap;
        }
        sheetRects = rects;
        AutoScrollMinSize = new Size(0, y);
    }

    /// <summary>Die Seite auf dem Bogen, in Pixeln relativ zur linken oberen Bogenecke.</summary>
    private Rectangle PageRect(int sheet)
    {
        var rect = sheetRects[sheet];
        var size = SheetPaper(sheet);
        var pixelsPerHundredth = rect.Width / size.Width;
        var place = PrintLayout.Place(document!.PageSizes[pages[sheet]], SheetPrintable(sheet), 100f / 72f, scaling, percent); // nur mit Dokument aufgerufen
        return Rectangle.Round(new RectangleF(place.X * pixelsPerHundredth, place.Y * pixelsPerHundredth, place.Width * pixelsPerHundredth, place.Height * pixelsPerHundredth));
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (document == null) { return; }
        ClearCache(); // andere Größe – neu rendern
        LayoutSheets();
    }

    private Rectangle ToClient(Rectangle rect) => new(rect.X + AutoScrollPosition.X, rect.Y + AutoScrollPosition.Y, rect.Width, rect.Height);

    private bool IsVisible(int sheet) => sheet < sheetRects.Length && ClientRectangle.IntersectsWith(ToClient(sheetRects[sheet]));

    // ================================================================== Zeichnen

    private static readonly ImageAttributes Gray = CreateGrayAttributes();

    private static ImageAttributes CreateGrayAttributes()
    {
        var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix(
        [
            [0.299f, 0.299f, 0.299f, 0f, 0f],
            [0.587f, 0.587f, 0.587f, 0f, 0f],
            [0.114f, 0.114f, 0.114f, 0f, 0f],
            [0f, 0f, 0f, 1f, 0f],
            [0f, 0f, 0f, 0f, 1f],
        ]));
        return attributes;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (document == null) { return; }
        var g = e.Graphics;
        using var shadow = new SolidBrush(Color.FromArgb(40, 0, 0, 0));
        for (var s = 0; s < sheetRects.Length; s++)
        {
            var sheet = ToClient(sheetRects[s]);
            if (!e.ClipRectangle.IntersectsWith(sheet)) { continue; }
            g.FillRectangle(shadow, sheet.X + 2, sheet.Y + 2, sheet.Width, sheet.Height);
            g.FillRectangle(Brushes.White, sheet);
            var page = PageRect(s);
            page.Offset(sheet.Location);
            var state = g.Save();
            g.SetClip(Rectangle.Intersect(sheet, page)); // in Originalgröße kann die Seite über den Bogen hinausragen
            if (cache.TryGetValue(s, out var bitmap))
            {
                if (grayscale) { g.DrawImage(bitmap, page, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, Gray); }
                else { g.DrawImageUnscaled(bitmap, page.Location); }
            }
            else { Request(s, page.Size); }
            g.Restore(state);
        }
    }

    private async void Request(int sheet, Size size)
    {
        if (document == null || size.Width <= 0 || size.Height <= 0 || !pending.Add(sheet)) { return; }
        var requestGeneration = generation;
        var doc = document;
        Bitmap? bitmap;
        try
        {
            bitmap = await doc.RenderAsync(pages[sheet], size.Width, size.Height,
                () => requestGeneration == generation && !IsDisposed && (bool)Invoke(() => IsVisible(sheet)));
        }
        catch (InvalidOperationException) { bitmap = null; } // Fenster beim Schließen: Invoke geht nicht mehr
        if (requestGeneration != generation || IsDisposed) { bitmap?.Dispose(); return; }
        pending.Remove(sheet);
        if (bitmap == null) { return; }
        cache[sheet] = bitmap;
        if (cache.Count > CacheLimit) // die am weitesten entfernten Bögen verwerfen
        {
            foreach (var far in cache.Keys.OrderByDescending(k => Math.Abs(k - sheet)).Take(cache.Count - CacheLimit).ToList())
            {
                cache[far].Dispose();
                cache.Remove(far);
            }
        }
        if (sheet < sheetRects.Length) { Invalidate(ToClient(sheetRects[sheet])); }
    }

    protected override void OnScroll(ScrollEventArgs se)
    {
        base.OnScroll(se);
        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus(); // für Mausrad und Bildtasten
    }

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Up or Keys.Down or Keys.PageUp or Keys.PageDown or Keys.Home or Keys.End || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var step = e.KeyCode switch
        {
            Keys.Up => -LogicalToDeviceUnits(40),
            Keys.Down => LogicalToDeviceUnits(40),
            Keys.PageUp => -ClientSize.Height,
            Keys.PageDown => ClientSize.Height,
            Keys.Home => int.MinValue / 2,
            Keys.End => int.MaxValue / 2,
            _ => 0,
        };
        if (step == 0) { return; }
        AutoScrollPosition = new Point(0, Math.Max(0, -AutoScrollPosition.Y + step));
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { ClearCache(); }
        base.Dispose(disposing);
    }
}
