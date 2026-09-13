using Microsoft.Web.WebView2.Core;
using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Fragt Text, Position und Schriftgröße für eine Textanmerkung auf der angezeigten Seite ab (Maße in
/// Millimetern von der linken oberen Ecke). Rechts zeigt ein Vorschaubild die Seite mit dem geplanten Kasten;
/// ein Klick ins Bild übernimmt die Position.
/// Die Vorschau entsteht ohne eigenen PDF-Renderer: Ein zweites WebView2 (dieselbe Umgebung wie der Viewer) lädt die
/// Seite als Einzelseiten-PDF ohne Werkzeugleiste, CapturePreview liefert das Bild, und die Seitenfläche darin wird
/// über die Hintergrundfarbe erkannt. Danach übernimmt eine PictureBox mit eigenem Zeichnen (WinForms kann nichts
/// über ein WebView2 legen).</summary>
public partial class AnnotationForm : Form
{
    public string AnnotationText => textBoxText.Text.Trim();
    public double LeftMm => (double)numLeft.Value;
    public double TopMm => (double)numTop.Value;
    public double FontSize => (double)numSize.Value;

    private const double MmPerPoint = 25.4 / 72;
    private readonly string filePath;
    private readonly int page;
    private readonly string previewPdf = Path.Combine(Path.GetTempPath(), $"pdflight-preview-{Environment.ProcessId}.pdf");
    private (double Width, double Height) pageSizePt;
    private Bitmap? previewImage;
    private Rectangle pageRect; // die Seitenfläche im Vorschaubild (Bildpixel)

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
            pageSizePt = PdfEditService.ExtractPageForPreview(filePath, previewPdf, page);
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
                Bitmap bitmap = new(stream);
                var rect = FindPageRect(bitmap);
                if (rect.Width < 20 || rect.Height < 20) { bitmap.Dispose(); continue; } // Seite noch nicht gezeichnet
                previewImage?.Dispose();
                previewImage = bitmap;
                pageRect = rect;
                core.Navigate("about:blank"); // gibt die Temp-Datei frei, sonst hält Chromium sie bis zum Dispose gesperrt
                previewWebView.Visible = false;
                picturePreview.Visible = true;
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

    /// <summary>Bild samt Seitenrahmen so skaliert, dass es in die PictureBox passt (zentriert).</summary>
    private (float Scale, PointF Offset) ImageLayout()
    {
        if (previewImage == null) { return (1, PointF.Empty); }
        var scale = Math.Min((float)picturePreview.ClientSize.Width / previewImage.Width, (float)picturePreview.ClientSize.Height / previewImage.Height);
        var offset = new PointF((picturePreview.ClientSize.Width - previewImage.Width * scale) / 2, (picturePreview.ClientSize.Height - previewImage.Height * scale) / 2);
        return (scale, offset);
    }

    private void PicturePreview_Paint(object? sender, PaintEventArgs e)
    {
        if (previewImage == null || pageRect.IsEmpty || pageSizePt.Width <= 0) { return; }
        var (scale, offset) = ImageLayout();
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        e.Graphics.DrawImage(previewImage, offset.X, offset.Y, previewImage.Width * scale, previewImage.Height * scale);
        // Kasten der Anmerkung: Millimeter → Seitenpixel → Bildschirm (der Rahmen liegt innerhalb der Seite, daher die volle Seitenbreite)
        var pxPerMm = pageRect.Width * scale / (pageSizePt.Width * MmPerPoint);
        var (widthPt, heightPt) = PdfEditService.MeasureAnnotation(AnnotationText.Length > 0 ? AnnotationText : "Text", FontSize);
        var box = new RectangleF(
            offset.X + pageRect.X * scale + (float)(LeftMm * pxPerMm),
            offset.Y + pageRect.Y * scale + (float)(TopMm * pxPerMm),
            (float)(widthPt * MmPerPoint * pxPerMm), (float)(heightPt * MmPerPoint * pxPerMm));
        using SolidBrush fill = new(Color.FromArgb(200, 255, 255, 204));
        using Pen border = new(Color.FromArgb(153, 153, 102));
        e.Graphics.FillRectangle(fill, box);
        e.Graphics.DrawRectangle(border, box.X, box.Y, box.Width, box.Height);
    }

    private void PicturePreview_MouseClick(object? sender, MouseEventArgs e)
    {
        if (previewImage == null || pageRect.IsEmpty || pageSizePt.Width <= 0) { return; }
        var (scale, offset) = ImageLayout();
        var pageX = (e.X - offset.X) / scale - pageRect.X; // Klick in Seitenpixel des Vorschaubilds
        var pageY = (e.Y - offset.Y) / scale - pageRect.Y;
        var mmPerPx = pageSizePt.Width * MmPerPoint / pageRect.Width;
        numLeft.Value = Math.Clamp((decimal)Math.Round(pageX * mmPerPx, 1), numLeft.Minimum, numLeft.Maximum);
        numTop.Value = Math.Clamp((decimal)Math.Round(pageY * mmPerPx, 1), numTop.Minimum, numTop.Maximum);
    }

    private void Preview_Changed(object? sender, EventArgs e) => picturePreview.Invalidate(); // Text, Größe oder Position geändert

    private void AnnotationForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        previewImage?.Dispose();
        try { File.Delete(previewPdf); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { } // Temp-Datei — notfalls räumt Windows auf
    }
}
