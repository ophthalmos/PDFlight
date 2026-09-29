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
        if (ImagePath is { } path && File.Exists(path))
        {
            try
            {
                using var stream = File.OpenRead(path);
                using var image = Image.FromStream(stream, useEmbeddedColorManagement: false, validateImageData: false);
                imagePixels = image.Size;
                if (image.HorizontalResolution is >= (float)MinDpi and <= (float)MaxDpi)
                {
                    comboDpi.Text = Math.Round(image.HorizontalResolution).ToString(CultureInfo.CurrentCulture);
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
        labelImageSize.Text = !imagePixels.IsEmpty && dpi >= MinDpi && dpi <= MaxDpi
            ? string.Format(Lng.T("{0} × {1} Pixel, bei {2} dpi {3:0.#} × {4:0.#} cm"), imagePixels.Width, imagePixels.Height, dpi,
                imagePixels.Width / dpi * 2.54, imagePixels.Height / dpi * 2.54)
            : string.Empty;
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
