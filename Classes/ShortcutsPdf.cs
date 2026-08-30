using System.Reflection;
using System.Runtime.InteropServices;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PDFLight.Classes;

/// <summary>Erstellt die druckbare Tastenkürzel-Übersicht als PDF in der aktuellen Programmsprache —
/// dynamisch mit PDFsharp (die Schriften löst der PlatformFontResolver über die Windows-Schriften auf).</summary>
internal static partial class ShortcutsPdf
{
    private const double Margin = 50;      // Seitenränder in Punkt
    private const double DetailIndent = 150; // Einzug der Kurztext-/Erklärungsspalte

    /// <summary>Der Standard-Ablageort der Übersicht: Downloads-Ordner, Dateiname in der Programmsprache.</summary>
    public static string DefaultPath => Path.Combine(GetDownloadsPath(), Lng.T("PDFlight-Tastenkürzel") + ".pdf");

    /// <summary>Schreibt die Übersicht in den angegebenen Ordner (null = Downloads) und liefert den Dateipfad.</summary>
    public static string Create(string directory = null)
    {
        var path = directory == null ? DefaultPath : Path.Combine(directory, Lng.T("PDFlight-Tastenkürzel") + ".pdf");
        using PdfDocument document = new();
        document.Options.ColorMode = PdfColorMode.Rgb;
        document.Info.Title = Application.ProductName + " – " + Lng.T("Tastenkürzel");
        document.Info.Author = Application.ProductName;
        XFont titleFont = new("Segoe UI", 17, XFontStyleEx.Bold);
        XFont subFont = new("Segoe UI", 9);
        XFont keyFont = new("Segoe UI", 10, XFontStyleEx.Bold);
        XFont textFont = new("Segoe UI", 10);
        XFont detailFont = new("Segoe UI", 9);
        XBrush detailBrush = new XSolidBrush(XColor.FromArgb(90, 90, 90));

        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);
        DrawPageBackground(gfx, page);
        var width = page.Width.Point - 2 * Margin;
        var y = Margin;
        var iconHeight = DrawAppIcon(gfx, page.Width.Point - Margin); // Programm-Icon rechts oben, 128 px unskaliert
        gfx.DrawString(Application.ProductName + " – " + Lng.T("Tastenkürzel"), titleFont, XBrushes.Black, Margin, y + 17);
        y += 26;
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
        gfx.DrawString("Version " + version + " – " + DateTime.Now.ToString("d", System.Globalization.CultureInfo.GetCultureInfo(Lng.CultureCode)), subFont, detailBrush, Margin, y + 9);
        y += 30;
        y = Math.Max(y, Margin - 4 + iconHeight + 14); // die Kürzelzeilen beginnen unterhalb des Icons

        foreach (var (key, text, detail) in TaskDlg.ShortcutRows)
        {
            // kompakt: eine Zeile je Kürzel; Zusatzerklärung nur, wo eine hinterlegt ist (F7) —
            // so passt die Übersicht auf eine A4-Seite
            var detailLines = detail == null ? null : Wrap(gfx, Lng.T(detail), detailFont, width - DetailIndent);
            var blockHeight = 17 + (detailLines?.Count ?? 0) * 12 + (detailLines == null ? 0 : 4);
            if (y + blockHeight > page.Height.Point - Margin) // Seitenumbruch (zur Sicherheit — planmäßig eine Seite)
            {
                gfx.Dispose();
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                DrawPageBackground(gfx, page);
                y = Margin;
            }
            gfx.DrawString(Lng.T(key), keyFont, XBrushes.Black, Margin, y + 11);
            gfx.DrawString(Lng.T(text), textFont, XBrushes.Black, Margin + DetailIndent, y + 11);
            y += 17;
            if (detailLines != null)
            {
                foreach (var line in detailLines)
                {
                    gfx.DrawString(line, detailFont, detailBrush, Margin + DetailIndent, y + 10);
                    y += 12;
                }
                y += 4;
            }
        }
        DrawFooter(gfx, page);
        gfx.Dispose();
        document.Save(path);
        return path;
    }

    /// <summary>Fußzeile: Trennlinie, darunter zentriert Copyright und die Webadresse als klickbarer Link.</summary>
    private static void DrawFooter(XGraphics gfx, PdfSharp.Pdf.PdfPage page)
    {
        const string LinkText = "www.netradio.info";
        const string LinkUrl = "https://www.netradio.info/pdf/";
        XFont font = new("Segoe UI", 9);
        var lineY = page.Height.Point - 44;
        gfx.DrawLine(new XPen(XColor.FromArgb(180, 190, 200), 0.7), Margin, lineY, page.Width.Point - Margin, lineY);
        var copyright = $"© {DateTime.Now.Year} Wilhelm Happe   ·   ";
        var copyWidth = gfx.MeasureString(copyright, font).Width;
        var linkWidth = gfx.MeasureString(LinkText, font).Width;
        var x = (page.Width.Point - copyWidth - linkWidth) / 2;
        var textY = lineY + 16;
        gfx.DrawString(copyright, font, new XSolidBrush(XColor.FromArgb(90, 90, 90)), x, textY);
        gfx.DrawString(LinkText, font, new XSolidBrush(XColor.FromArgb(0x1E, 0x5A, 0x96)), x + copyWidth, textY);
        // klickbare Fläche über dem Linktext (WorldToDefaultPage rechnet ins PDF-Koordinatensystem um)
        var linkRect = gfx.Transformer.WorldToDefaultPage(new XRect(x + copyWidth, textY - 10, linkWidth, 13));
        page.AddWebLink(new PdfSharp.Pdf.PdfRectangle(linkRect), LinkUrl);
    }

    /// <summary>Dezent hellblauer Seitenhintergrund.</summary>
    /// gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(0xF1, 0xF6, 0xFB)), 0, 0, page.Width.Point, page.Height.Point);
    private static void DrawPageBackground(XGraphics gfx, PdfPage page) =>
        gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(249, 252, 255)), 0, 0, page.Width.Point, page.Height.Point);

    internal static string IconDiag = "nicht aufgerufen"; // nur für die Test-Diagnose

    /// <summary>Zeichnet das 128-px-Programm-Icon (aus der EXE extrahiert) rechts oben in seiner
    /// natürlichen Größe — unskaliert, damit PDFsharp keine Unschärfe hineinrechnet. Liefert die
    /// gezeichnete Höhe in Punkt (0 ohne Icon).</summary>
    private static double DrawAppIcon(XGraphics gfx, double rightEdge)
    {
        // Quelle ist die PDFlight-EXE neben der Programm-Assembly (identisch mit ExecutablePath,
        // wenn PDFlight selbst läuft — aber auch aus Test-Treibern heraus korrekt)
        var iconSource = Path.ChangeExtension(typeof(ShortcutsPdf).Assembly.Location, ".exe");
        if (!File.Exists(iconSource)) { iconSource = Application.ExecutablePath; }
        var hr = SHDefExtractIcon(iconSource, 0, 0, out var hIcon, out var hIconSmall, 128);
        if (hIconSmall != 0) { _ = DestroyIcon(hIconSmall); }
        if (hr != 0 || hIcon == 0) { IconDiag = $"hr={hr} hIcon={hIcon}"; return 0; }
        try
        {
            using var icon = Icon.FromHandle(hIcon);
            using var bitmap = icon.ToBitmap();
            using var image = XImage.FromGdiPlusImage(bitmap);
            // ohne Zielmaße zeichnet PDFsharp in der natürlichen Punktgröße des Bildes
            // (128 px bei 96 dpi = 96 pt) — pixelgenau, keine Skalierungsunschärfe
            gfx.DrawImage(image, rightEdge - image.PointWidth, Margin - 4);
            IconDiag = "gezeichnet";
            return image.PointHeight;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or System.Runtime.InteropServices.ExternalException)
        {
            IconDiag = ex.GetType().Name + ": " + ex.Message;
            return 0;
        }
        finally { _ = DestroyIcon(hIcon); }
    }

    [LibraryImport("shell32.dll", EntryPoint = "SHDefExtractIconW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int SHDefExtractIcon(string iconFile, int iconIndex, uint flags, out nint hIconLarge, out nint hIconSmall, uint iconSize);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyIcon(nint hIcon);

    /// <summary>Einfacher Zeilenumbruch: bricht text an Wortgrenzen auf maxWidth Punkt um.</summary>
    private static List<string> Wrap(XGraphics gfx, string text, XFont font, double maxWidth)
    {
        List<string> lines = [];
        var line = string.Empty;
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = line.Length == 0 ? word : line + " " + word;
            if (gfx.MeasureString(candidate, font).Width > maxWidth && line.Length > 0)
            {
                lines.Add(line);
                line = word;
            }
            else { line = candidate; }
        }
        if (line.Length > 0) { lines.Add(line); }
        return lines;
    }

    /// <summary>Der Downloads-Ordner des Benutzers (kein Environment.SpecialFolder vorhanden).</summary>
    private static string GetDownloadsPath()
    {
        Guid downloads = new("374DE290-123F-4565-9164-39C4925E467B"); // FOLDERID_Downloads
        var hr = SHGetKnownFolderPath(in downloads, 0, 0, out var pathPtr);
        try
        {
            if (hr == 0)
            {
                var path = Marshal.PtrToStringUni(pathPtr);
                if (!string.IsNullOrEmpty(path)) { return path; }
            }
        }
        finally { Marshal.FreeCoTaskMem(pathPtr); }
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    }

    [LibraryImport("shell32.dll")]
    private static partial int SHGetKnownFolderPath(in Guid rfid, uint flags, nint token, out nint path);
}
