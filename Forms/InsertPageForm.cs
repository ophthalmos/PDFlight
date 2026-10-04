using System.Globalization;
using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Leere Seite einfügen: Format (wie die angezeigte Seite – mit erkanntem Papierformat –, DIN A4, US Letter oder wie das Bild),
/// Position (vor/nach der angezeigten Seite, Anfang, Ende) und wahlweise ein Bild in Originalgröße oder eingepasst. Die Auflösung für
/// die Originalgröße ist mit der DPI-Angabe der Bilddatei vorbelegt, aber änderbar – Handyfotos tragen oft 72 DPI.</summary>
public partial class InsertPageForm : Form
{
    private const double MinDpi = 10;
    private const double MaxDpi = 2400;
    private Size imagePixels; // Pixelmaße des gewählten Bildes; leer, solange keins lesbar ist
    private double fileDpi;   // DPI-Angabe der Bilddatei (gerundet); 0 = keins gelesen oder außerhalb des erlaubten Bereichs
    private Bitmap? thumbnail; // verkleinerte Kopie des Bildes für die Vorschau
    private readonly double referenceWidth, referenceHeight; // sichtbare Maße der angezeigten Seite in Punkt (Format „wie Seite N“)
    private const int ThumbnailSize = 480; // längste Seite der Vorschau-Kopie in Pixeln – genug auch für hohe Bildschirmskalierung

    /// <summary>Pfad der Bilddatei für die neue Seite; null = leere Seite.</summary>
    public string? ImagePath => textBoxImage.Text.Trim().Length > 0 ? textBoxImage.Text.Trim() : null;

    /// <summary>Alle Einstellungen für <see cref="PdfEditService.InsertBlankPage"/>.</summary>
    internal NewPageOptions Options => new(Position, Format, comboOrientation.SelectedIndex == 1, ImagePath, radioFit.Checked, Dpi);

    private InsertPosition Position =>
        radioBefore.Checked ? InsertPosition.Before : radioFirst.Checked ? InsertPosition.First : radioLast.Checked ? InsertPosition.Last : InsertPosition.After;

    private NewPageFormat Format =>
        radioA4.Checked ? NewPageFormat.A4 : radioLetter.Checked ? NewPageFormat.Letter
        : radioFromImage.Checked && ImagePath != null ? NewPageFormat.FromImage : NewPageFormat.LikePage;

    private double Dpi => double.TryParse(comboDpi.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out var dpi) ? dpi : 0;

    /// <summary>Braucht die Auswahl eine Auflösung? Originalgröße und „wie das Bild“ rechnen mit ihr, das Einpassen nicht.</summary>
    private bool NeedsDpi => ImagePath != null && (radioFromImage.Checked || radioOriginal.Checked);

    /// <param name="page">die angezeigte Seite (1-basiert)</param>
    /// <param name="pageWidthPt">sichtbare Breite dieser Seite in Punkt (0 = unbekannt)</param>
    /// <param name="pageHeightPt">sichtbare Höhe dieser Seite in Punkt</param>
    internal InsertPageForm(int page, double pageWidthPt, double pageHeightPt)
    {
        referenceWidth = pageWidthPt; // vor InitializeComponent: dessen Ereignisse zeichnen schon die Vorschau
        referenceHeight = pageHeightPt;
        InitializeComponent();
        Lng.Apply(this);
        TextBoxMargins.Apply(this);
        // die Designer-Texte sind die (bereits übersetzten) Formatstrings
        radioLikePage.Text = string.Format(radioLikePage.Text, page, PaperFormat.Describe(pageWidthPt, pageHeightPt)).TrimEnd(' ', ':');
        radioAfter.Text = string.Format(radioAfter.Text, page);
        radioBefore.Text = string.Format(radioBefore.Text, page);
        comboOrientation.Items.AddRange([Lng.T("Hochformat"), Lng.T("Querformat")]);
        comboOrientation.SelectedIndex = pageWidthPt > pageHeightPt ? 1 : 0; // Vorgabe wie die angezeigte Seite
    }

    private void ButtonBrowse_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new()
        {
            Filter = Lng.T("Bilddateien") + " (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff",
            Title = Lng.T("Bild für die neue Seite wählen"),
        };
        if (dialog.ShowDialog(this) == DialogResult.OK) { textBoxImage.Text = dialog.FileName; }
    }

    /// <summary>Liest Pixelmaße und DPI-Angabe des Bildes (nur den Kopf, ohne die Bilddaten zu prüfen) und belegt die Auflösung vor.</summary>
    private void TextBoxImage_TextChanged(object? sender, EventArgs e)
    {
        imagePixels = Size.Empty;
        fileDpi = 0;
        thumbnail?.Dispose();
        thumbnail = null;
        if (ImagePath is { } path && File.Exists(path))
        {
            try
            {
                using var stream = File.OpenRead(path);
                using var image = Image.FromStream(stream, useEmbeddedColorManagement: false, validateImageData: false);
                imagePixels = image.Size;
                var factor = Math.Min(1, (double)ThumbnailSize / Math.Max(image.Width, image.Height));
                thumbnail = new Bitmap(image, Math.Max(1, (int)(image.Width * factor)), Math.Max(1, (int)(image.Height * factor)));
                if (image.HorizontalResolution is >= (float)MinDpi and <= (float)MaxDpi)
                {
                    fileDpi = Math.Round(image.HorizontalResolution); // ohne Angabe in der Datei meldet GDI+ meist 96
                    comboDpi.Text = fileDpi.ToString(CultureInfo.CurrentCulture);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or OutOfMemoryException) { } // kein lesbares Bild: das meldet erst das Einfügen
        }
        UpdateOptions();
    }

    private void Option_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateOptions();
    }

    private void ComboDpi_TextChanged(object? sender, EventArgs e)
    {
        UpdateOptions();
    }

    private void ComboOrientation_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateOptions();
    }

    private void InsertPageForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        thumbnail?.Dispose();
        thumbnail = null;
    }

    /// <summary>Aktiviert, was zur Auswahl passt: Ausrichtung nur für die festen Formate, „wie das Bild“ und die Bildgröße nur mit Bild,
    /// die Auflösung nur, wo sie gebraucht wird; darunter die Druckgröße des Bildes bei der gewählten Auflösung.</summary>
    private void UpdateOptions()
    {
        var hasImage = ImagePath != null;
        radioFromImage.Enabled = hasImage;
        if (!hasImage && radioFromImage.Checked) { radioLikePage.Checked = true; }
        comboOrientation.Enabled = radioA4.Checked || radioLetter.Checked;
        radioOriginal.Enabled = radioFit.Enabled = hasImage && !radioFromImage.Checked;
        comboDpi.Enabled = labelDpi.Enabled = labelDpiHint.Enabled = NeedsDpi;
        var dpi = Dpi;
        // woher der Wert stammt: unverändert aus der Datei, sonst deren Angabe zum Vergleich
        labelDpiSource.Text = fileDpi <= 0 || ImagePath == null ? string.Empty
            : dpi == fileDpi ? Lng.T("(aus der Bilddatei)")
            : string.Format(Lng.T("(Bilddatei: {0} dpi)"), fileDpi);
        labelDpiSource.Enabled = NeedsDpi;
        UpdatePreview();
    }

    // ================================================================== Vorschau

    /// <summary>Seitenmaße und Bildlage wie beim Einfügen (dieselbe Rechnung in <see cref="PdfEditService.LayoutNewPage"/>); ohne
    /// lesbares Bild oder mit ungültiger Auflösung nur die Seite.</summary>
    private NewPageLayout PreviewLayout()
    {
        var usable = !imagePixels.IsEmpty && (!NeedsDpi || Dpi is >= MinDpi and <= MaxDpi);
        return PdfEditService.LayoutNewPage(Options, referenceWidth, referenceHeight, usable ? imagePixels.Width : 0, usable ? imagePixels.Height : 0);
    }

    /// <summary>Seitenmaße unter der Vorschau, die Druckgröße des Bildes (bei Originalgröße und „wie das Bild“ samt Verkleinerungsgrad,
    /// wenn es nicht passt) und neu zeichnen.</summary>
    private void UpdatePreview()
    {
        var layout = PreviewLayout();
        const double MmPerPt = 25.4 / 72;
        labelPreviewPage.Text = layout.Width > 0 ? $"{layout.Width * MmPerPt:0} × {layout.Height * MmPerPt:0} mm" : string.Empty;
        var dpi = Dpi;
        var sizeText = !imagePixels.IsEmpty && dpi >= MinDpi && dpi <= MaxDpi
            ? string.Format(Lng.T("{0} × {1} Pixel, bei {2} dpi {3:0.#} × {4:0.#} cm"), imagePixels.Width, imagePixels.Height, dpi,
                imagePixels.Width / dpi * 2.54, imagePixels.Height / dpi * 2.54)
            : string.Empty;
        if (sizeText.Length > 0 && layout.Image != null && (radioFromImage.Checked || !radioFit.Checked) && layout.Scale < 0.995)
        {
            sizeText += ", " + string.Format(Lng.T("auf {0:0} % verkleinert"), layout.Scale * 100); // beim Einpassen ist Größenänderung gewollt
        }
        labelImageSize.Text = sizeText;
        pictureBoxPreview.Invalidate();
    }

    /// <summary>Zeichnet die neue Seite maßstäblich: weiße Fläche mit Rahmen, das Bild als Miniatur in seiner späteren Größe und Lage,
    /// beim Einpassen den 10-mm-Rand gestrichelt.</summary>
    private void PictureBoxPreview_Paint(object? sender, PaintEventArgs e)
    {
        var layout = PreviewLayout();
        if (layout.Width <= 0 || layout.Height <= 0) { return; }
        var area = Rectangle.Inflate(pictureBoxPreview.ClientRectangle, -LogicalToDeviceUnits(6), -LogicalToDeviceUnits(6));
        var zoom = Math.Min(area.Width / layout.Width, area.Height / layout.Height);
        var page = new RectangleF((float)(area.X + (area.Width - layout.Width * zoom) / 2), (float)(area.Y + (area.Height - layout.Height * zoom) / 2),
            (float)(layout.Width * zoom), (float)(layout.Height * zoom));
        var g = e.Graphics;
        g.FillRectangle(Brushes.White, page);
        if (layout.Image is { } rect && thumbnail != null)
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(thumbnail, new RectangleF(page.X + (float)(rect.X * zoom), page.Y + (float)(rect.Y * zoom), (float)(rect.Width * zoom), (float)(rect.Height * zoom)));
            if (radioFit.Checked && !radioFromImage.Checked)
            {
                var margin = (float)(PdfEditService.ImageMarginPt * zoom);
                using var dash = new Pen(Color.FromArgb(150, 150, 150)) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                g.DrawRectangle(dash, page.X + margin, page.Y + margin, page.Width - 2 * margin, page.Height - 2 * margin);
            }
        }
        using var border = new Pen(Color.FromArgb(120, 120, 120));
        g.DrawRectangle(border, page.X, page.Y, page.Width - 1, page.Height - 1);
    }

    private void ButtonOK_Click(object? sender, EventArgs e)
    {
        if (ImagePath is { } image && !File.Exists(image))
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Die Bilddatei wurde nicht gefunden."), image, TaskDialogIcon.Warning);
            textBoxImage.SelectAll();
            textBoxImage.Focus();
            return;
        }
        if (NeedsDpi && (Dpi < MinDpi || Dpi > MaxDpi))
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Bitte gib eine Auflösung zwischen 10 und 2400 dpi an."), comboDpi.Text, TaskDialogIcon.Warning);
            comboDpi.Focus();
            return;
        }
        DialogResult = DialogResult.OK;
    }
}
