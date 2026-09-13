using Microsoft.Web.WebView2.Core;
using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Fragt Text, Position und Schriftgröße für eine Textanmerkung auf der angezeigten Seite ab (Maße in
/// Millimetern von der linken oberen Ecke). Rechts zeigt ein Vorschaubild die Seite mit dem geplanten Kasten;
/// ein Klick ins Bild übernimmt die Position.
/// Die Vorschau entsteht ohne eigenen PDF-Renderer: Ein zweites WebView2 (dieselbe Umgebung wie der Viewer) lädt die
/// Seite als Einzelseiten-PDF ohne Werkzeugleiste, CapturePreview liefert das Bild, und ein magentafarbener Rahmen
/// im Vorschau-PDF verrät die Seitenfläche. Das WebView liegt die ganze Zeit hinter der PictureBox, die zunächst nur
/// den Hintergrund zeigt und nach dem Abfotografieren das fertig skalierte Bild – so flackert beim Laden nichts.</summary>
public partial class AnnotationForm : Form
{
    public string AnnotationText => textBoxText.Text.Trim();
    public double LeftMm => (double)numLeft.Value;
    public double TopMm => (double)numTop.Value;
    public double FontSize => (double)numSize.Value;
    internal AnnotationStyle Style => new(cbBorder.Checked, Backgrounds[Math.Max(0, comboBackground.SelectedIndex)].Color);

    /// <summary>Die wählbaren Hintergründe (Namen sind Lng-Schlüssel); null = transparent.</summary>
    private static readonly (string Name, Color? Color)[] Backgrounds =
    [
        ("Gelb", Color.FromArgb(255, 255, 204)), ("Weiß", Color.White), ("Hellblau", Color.FromArgb(221, 238, 255)),
        ("Hellgrün", Color.FromArgb(221, 255, 221)), ("Rosa", Color.FromArgb(255, 221, 238)), ("Transparent", null),
    ];

    private const double MmPerPoint = 25.4 / 72;
    private readonly string filePath;
    private readonly int page;
    private readonly string previewPdf = Path.Combine(Path.GetTempPath(), $"pdflight-preview-{Environment.ProcessId}.pdf");
    private (double Width, double Height) pageSizePt;
    private Bitmap? scaledImage;       // Vorschaubild, bereits auf die PictureBox skaliert (das Zeichnen bleibt billig)
    private RectangleF scaledPageRect; // die Seitenfläche darin (PictureBox-Pixel, relativ zum Bild)
    private PointF imageOffset;
    private (double Width, double Height) boxPt; // Kastenmaß für den aktuellen Text (gemessen nur bei Änderung)
    private string measuredText = string.Empty;
    private double measuredSize;
    private int excludeAnnotationIndex = -1; // beim Bearbeiten: diese Anmerkung fehlt in der Vorschau

    public AnnotationForm(string filePath, int pageCount, int page)
    {
        InitializeComponent();
        TextBoxMargins.Apply(this);
        Lng.Apply(this);
        this.filePath = filePath;
        this.page = Math.Clamp(page, 1, Math.Max(1, pageCount));
        labelFileValue.Text = Path.GetFileName(filePath);
        labelPage.Text = string.Format(Lng.T("Seite {0} von {1}"), this.page, pageCount);
        labelPreviewState.Text = Lng.T("Vorschau wird erstellt …");
        comboBackground.Items.AddRange([.. Backgrounds.Select(bg => (object)Lng.T(bg.Name))]);
        SetStyle(AnnotationStyle.Default);
    }

    /// <summary>Rahmen und Hintergrund vorbelegen (zuletzt gewählte Werte bzw. die der bearbeiteten Anmerkung); eine fremde
    /// Farbe außerhalb der Auswahl fällt auf Gelb zurück.</summary>
    internal void SetStyle(AnnotationStyle style)
    {
        cbBorder.Checked = style.Border;
        var index = Array.FindIndex(Backgrounds, bg => bg.Color?.ToArgb() == style.Background?.ToArgb());
        comboBackground.SelectedIndex = index >= 0 ? index : 0;
    }

    /// <summary>Bearbeiten einer vorhandenen Anmerkung: Werte vorbelegen und den Titel anpassen.</summary>
    internal void Preset(string text, double leftMm, double topMm, double fontSize, AnnotationStyle style, int annotationIndex)
    {
        Text = Lng.T("Textanmerkung bearbeiten");
        excludeAnnotationIndex = annotationIndex;
        SetStyle(style);
        textBoxText.Text = string.Join(Environment.NewLine, PdfEditService.SplitLines(text));
        numLeft.Value = Math.Clamp((decimal)leftMm, numLeft.Minimum, numLeft.Maximum);
        numTop.Value = Math.Clamp((decimal)topMm, numTop.Minimum, numTop.Maximum);
        numSize.Value = Math.Clamp((decimal)fontSize, numSize.Minimum, numSize.Maximum);
    }

    private void ButtonOK_Click(object? sender, EventArgs e)
    {
        if (AnnotationText.Length == 0)
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Bitte gib einen Text ein."), null, TaskDialogIcon.Warning);
            textBoxText.Focus();
            return;
        }
        DialogResult = DialogResult.OK;
    }

    private async void AnnotationForm_Shown(object? sender, EventArgs e)
    {
        try
        {
            pageSizePt = PdfEditService.ExtractPageForPreview(filePath, previewPdf, page, excludeAnnotationIndex);
            if (PdfViewHost.SharedEnvironment == null) { throw new InvalidOperationException("WebView2-Umgebung fehlt"); }
            await previewWebView.EnsureCoreWebView2Async(PdfViewHost.SharedEnvironment);
            var core = previewWebView.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.IsZoomControlEnabled = false;
            TaskCompletionSource loaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
            core.NavigationCompleted += (s, args) => loaded.TrySetResult();
            core.Navigate(new Uri(previewPdf).AbsoluteUri + "#toolbar=0&view=Fit"); // Chromium-Parameter: ohne Leiste, ganze Seite
            await loaded.Task;
            for (var attempt = 0; attempt < 8 && !IsDisposed; attempt++)
            {
                await Task.Delay(250); // der Viewer rendert die Seite erst nach dem Laden
                using MemoryStream stream = new();
                await core.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
                stream.Position = 0;
                using Bitmap bitmap = new(stream);
                var rect = FindPageRect(bitmap);
                if (rect.Width < 20 || rect.Height < 20) { continue; } // Seite noch nicht gezeichnet
                PrepareScaledImage(bitmap, rect);
                core.Navigate("about:blank"); // gibt die Temp-Datei frei, sonst hält Chromium sie bis zum Dispose gesperrt
                labelPreviewState.Text = Lng.T("Klick ins Vorschaubild setzt die Position.");
                picturePreview.Invalidate();
                return;
            }
            labelPreviewState.Text = Lng.T("Vorschau nicht verfügbar");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException
            or System.Runtime.InteropServices.COMException or WebView2RuntimeNotFoundException || PdfEditService.IsPdfReadError(ex))
        {
            if (!IsDisposed) { labelPreviewState.Text = Lng.T("Vorschau nicht verfügbar"); }
        }
    }

    /// <summary>Die Seitenfläche im Vorschaubild: der umschließende Kasten aller magentafarbenen Pixel – das ist der Rahmen,
    /// den ExtractPageForPreview an den Seitenrand gezeichnet hat (auch angeschnitten und weichgezeichnet noch eindeutig).</summary>
    private static Rectangle FindPageRect(Bitmap bitmap)
    {
        int left = int.MaxValue, top = int.MaxValue, right = -1, bottom = -1;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var c = bitmap.GetPixel(x, y);
                if (c.R < 170 || c.B < 170 || c.G > 140 || c.R - c.G < 80) { continue; } // kein Magenta
                if (x < left) { left = x; }
                if (x > right) { right = x; }
                if (y < top) { top = y; }
                if (y > bottom) { bottom = y; }
            }
        }
        return right < 0 ? Rectangle.Empty : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
    }

    /// <summary>Schneidet das abfotografierte Bild auf die Seitenfläche zu, skaliert es einmal auf die PictureBox (zentriert)
    /// und rechnet die Seitenfläche mit um.</summary>
    private void PrepareScaledImage(Bitmap bitmap, Rectangle pageRect)
    {
        // nur die Seitenfläche verwenden – Bildlaufleiste und grauer Rand des Viewers bleiben außen vor
        var scale = Math.Min((float)picturePreview.ClientSize.Width / pageRect.Width, (float)picturePreview.ClientSize.Height / pageRect.Height);
        var size = new Size(Math.Max(1, (int)(pageRect.Width * scale)), Math.Max(1, (int)(pageRect.Height * scale)));
        scaledImage?.Dispose();
        scaledImage = new Bitmap(size.Width, size.Height);
        using (var g = Graphics.FromImage(scaledImage))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(bitmap, new Rectangle(0, 0, size.Width, size.Height), pageRect, GraphicsUnit.Pixel);
        }
        pageRect = new Rectangle(0, 0, pageRect.Width, pageRect.Height); // ab hier zählt das zugeschnittene Bild
        imageOffset = new PointF((picturePreview.ClientSize.Width - size.Width) / 2f, (picturePreview.ClientSize.Height - size.Height) / 2f);
        scaledPageRect = new RectangleF(pageRect.X * scale, pageRect.Y * scale, pageRect.Width * scale, pageRect.Height * scale);
        // den magentafarbenen Erkennungsrahmen übermalen: weißer Streifen in Rahmenbreite, darüber eine dezente Seitenkante
        using (var g = Graphics.FromImage(scaledImage))
        {
            var band = (int)Math.Ceiling(PdfEditService.PreviewFrameWidth * scaledPageRect.Width / pageSizePt.Width) + 1;
            using Pen white = new(Color.White, band * 2) { Alignment = System.Drawing.Drawing2D.PenAlignment.Inset };
            g.DrawRectangle(white, scaledPageRect.X, scaledPageRect.Y, scaledPageRect.Width - 1, scaledPageRect.Height - 1);
            using Pen edge = new(Color.FromArgb(200, 200, 200));
            g.DrawRectangle(edge, scaledPageRect.X, scaledPageRect.Y, scaledPageRect.Width - 1, scaledPageRect.Height - 1);
        }
    }

    private void PicturePreview_Paint(object? sender, PaintEventArgs e)
    {
        if (scaledImage == null || pageSizePt.Width <= 0) { return; }
        e.Graphics.DrawImageUnscaled(scaledImage, (int)imageOffset.X, (int)imageOffset.Y);
        var text = AnnotationText; // leer: ein kleiner leerer Kasten
        if (text != measuredText || FontSize != measuredSize)
        {
            boxPt = PdfEditService.MeasureAnnotation(text, FontSize); // nur bei geändertem Text oder geänderter Größe messen
            measuredText = text;
            measuredSize = FontSize;
        }
        var pxPerMm = scaledPageRect.Width / (pageSizePt.Width * MmPerPoint);
        var box = new RectangleF(
            imageOffset.X + scaledPageRect.X + (float)(LeftMm * pxPerMm),
            imageOffset.Y + scaledPageRect.Y + (float)(TopMm * pxPerMm),
            (float)(boxPt.Width * MmPerPoint * pxPerMm), (float)(boxPt.Height * MmPerPoint * pxPerMm));
        var style = Style;
        if (style.Background is { } background)
        {
            using SolidBrush fill = new(Color.FromArgb(200, background));
            e.Graphics.FillRectangle(fill, box);
        }
        if (style.Border)
        {
            using Pen border = new(Color.FromArgb(153, 153, 102));
            e.Graphics.DrawRectangle(border, box.X, box.Y, box.Width, box.Height);
        }
        // der Text im Kasten, im Maßstab der Vorschau (Arial steht für Helvetica – wie bei der Messung)
        var pxPerPt = pxPerMm * MmPerPoint;
        var fontPx = (float)(FontSize * pxPerPt);
        if (fontPx < 3 || text.Length == 0) { return; }
        using Font font = new("Arial", fontPx, GraphicsUnit.Pixel);
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        var format = StringFormat.GenericTypographic; // ohne den Zusatzabstand des Standardformats, sonst ragt der Text rechts über den Kasten
        var y = box.Y + (float)(PdfEditService.Padding * pxPerPt);
        foreach (var line in PdfEditService.SplitLines(text))
        {
            e.Graphics.DrawString(line, font, Brushes.Black, box.X + (float)(PdfEditService.Padding * pxPerPt), y, format);
            y += (float)(FontSize * PdfEditService.LeadingFactor * pxPerPt);
        }
    }

    private void PicturePreview_MouseClick(object? sender, MouseEventArgs e)
    {
        if (scaledImage == null || pageSizePt.Width <= 0) { return; }
        var mmPerPx = pageSizePt.Width * MmPerPoint / scaledPageRect.Width;
        var pageX = e.X - imageOffset.X - scaledPageRect.X; // Klick in Seitenpixel der Vorschau
        var pageY = e.Y - imageOffset.Y - scaledPageRect.Y;
        numLeft.Value = Math.Clamp((decimal)Math.Round(pageX * mmPerPx, 1), numLeft.Minimum, numLeft.Maximum);
        numTop.Value = Math.Clamp((decimal)Math.Round(pageY * mmPerPx, 1), numTop.Minimum, numTop.Maximum);
    }

    private void Preview_Changed(object? sender, EventArgs e)
    {
        if (scaledImage != null) { picturePreview.Invalidate(); } // Text, Größe oder Position geändert
    }

    private void AnnotationForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        scaledImage?.Dispose();
        try { File.Delete(previewPdf); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { } // Temp-Datei — notfalls räumt Windows auf
    }
}
