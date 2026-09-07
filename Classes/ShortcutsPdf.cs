using System.Reflection;
using System.Runtime.InteropServices;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PDFLight.Classes;

/// <summary>Erstellt die druckbare Hilfedatei (Hinweis zu Anzeigen/Bearbeiten und Tastenkürzel-Übersicht)
/// als PDF in der aktuellen Programmsprache — dynamisch mit PDFsharp (die Schriften löst der
/// PlatformFontResolver über die Windows-Schriften auf). Planmäßig eine A4-Seite: Die Zeilenhöhe der
/// Kürzeltabelle passt sich dem verbleibenden Platz an.</summary>
internal static partial class ShortcutsPdf
{
    private const double Margin = 50;      // Seitenränder in Punkt
    private const double DetailIndent = 150; // Einzug der Kurztext-/Erklärungsspalte
    private const double FooterHeight = 44; // Platz für Trennlinie und Fußzeile

    private static readonly XColor Accent = XColor.FromArgb(0x1E, 0x5A, 0x96);
    private static readonly XColor RuleColor = XColor.FromArgb(180, 190, 200);

    /// <summary>Der Standard-Ablageort der Hilfedatei: Downloads-Ordner, Dateiname in der Programmsprache.</summary>
    public static string DefaultPath => Path.Combine(GetDownloadsPath(), Lng.T("PDFlight-Hilfe") + ".pdf");

    /// <summary>Schreibt die Hilfedatei in den angegebenen Ordner (null = Downloads) und liefert den Dateipfad.</summary>
    public static string Create(string? directory = null)
    {
        var path = directory == null ? DefaultPath : Path.Combine(directory, Lng.T("PDFlight-Hilfe") + ".pdf");
        using PdfDocument document = new();
        document.Options.ColorMode = PdfColorMode.Rgb;
        document.Info.Title = Application.ProductName + " – " + Lng.T("Hilfe");
        document.Info.Author = Application.ProductName ?? string.Empty;
        XFont titleFont = new("Segoe UI", 17, XFontStyleEx.Bold);
        XFont subFont = new("Segoe UI", 9);
        XFont sectionFont = new("Segoe UI", 12, XFontStyleEx.Bold);
        XFont detailFont = new("Segoe UI", 9);
        XBrush detailBrush = new XSolidBrush(XColor.FromArgb(90, 90, 90));

        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);
        DrawPageBackground(gfx, page);
        var width = page.Width.Point - 2 * Margin;
        var y = Margin;
        var iconHeight = DrawAppIcon(gfx, page.Width.Point - Margin); // Programm-Icon rechts oben, 128 px unskaliert
        gfx.DrawString(Application.ProductName + " – " + Lng.T("Hilfe"), titleFont, XBrushes.Black, Margin, y + 17);
        y += 26;
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
        gfx.DrawString("Version " + version + " – " + DateTime.Now.ToString("d", System.Globalization.CultureInfo.GetCultureInfo(Lng.CultureCode)), subFont, detailBrush, Margin, y + 9);
        y += 30;
        y = Math.Max(y, Margin - 4 + iconHeight + 12); // der Hinweiskasten beginnt unterhalb des Icons

        y = DrawNoteBox(gfx, Margin, y, width) + 20;

        // Abschnittsüberschrift der Kürzeltabelle
        gfx.DrawString(Lng.T("Tastenkürzel"), sectionFont, new XSolidBrush(Accent), Margin, y + 12);
        gfx.DrawLine(new XPen(RuleColor, 0.7), Margin, y + 18, Margin + width, y + 18);
        y += 27;

        // Zeilenhöhe so wählen, dass die Tabelle auf die Seite passt (12–17 Punkt); die Erklärungszeilen
        // (nur wo hinterlegt, z.B. F7) haben feste Höhe
        var rows = TaskDlg.ShortcutRows;
        var detailLines = rows.Select(r => r.Detail == null ? null : Wrap(gfx, Lng.T(r.Detail), detailFont, width - DetailIndent)).ToList();
        var detailHeight = detailLines.Sum(l => l == null ? 0 : l.Count * 12 + 4);
        var footerTop = page.Height.Point - FooterHeight - 6;
        var rowHeight = Math.Clamp(Math.Floor((footerTop - y - detailHeight) / rows.Length), 12, 17);
        var fontSize = rowHeight < 15 ? 9 : 10;
        XFont keyFont = new("Segoe UI", fontSize, XFontStyleEx.Bold);
        XFont textFont = new("Segoe UI", fontSize);

        for (var i = 0; i < rows.Length; i++)
        {
            var (key, text, _) = rows[i];
            var lines = detailLines[i];
            var blockHeight = rowHeight + (lines == null ? 0 : lines.Count * 12 + 4);
            if (y + blockHeight > footerTop) // Seitenumbruch (zur Sicherheit — planmäßig eine Seite)
            {
                DrawFooter(gfx, page);
                gfx.Dispose();
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                DrawPageBackground(gfx, page);
                y = Margin;
            }
            gfx.DrawString(Lng.T(key), keyFont, XBrushes.Black, Margin, y + rowHeight - 6);
            gfx.DrawString(Lng.T(text), textFont, XBrushes.Black, Margin + DetailIndent, y + rowHeight - 6);
            y += rowHeight;
            if (lines != null)
            {
                foreach (var line in lines)
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

    /// <summary>Hinweiskasten „Anzeigen und Bearbeiten“: abgerundete, hellblaue Fläche mit Überschrift in der
    /// Akzentfarbe und drei Absätzen. Liefert die Unterkante des Kastens.</summary>
    private static double DrawNoteBox(XGraphics gfx, double x, double y, double width)
    {
        const double Pad = 12;
        const double LineHeight = 12.5;
        const double ParagraphGap = 5;
        XFont headFont = new("Segoe UI", 10.5, XFontStyleEx.Bold);
        XFont font = new("Segoe UI", 9);
        string[] paragraphs =
        [
            Lng.T("PDFlight lädt die Datei zum Anzeigen in den Arbeitsspeicher. Die Datei selbst bleibt dabei frei: andere Programme können sie jederzeit ändern oder verschieben. Wird sie geändert, zeigt PDFlight den neuen Stand, sobald du zum Fenster zurückkehrst."),
            Lng.T("Die Werkzeuge in der Anzeige (Zoom, Ansicht drehen, Suchen) verändern nur die Darstellung, nie die Datei. Alles, was du dort drehst, ist beim nächsten Öffnen wieder wie vorher."),
            Lng.T("Die Befehle im Menü „Bearbeiten“ und in der Symbolleiste (Seiten löschen oder drehen, anhängen, Kennwort, Eigenschaften) ändern die Datei dagegen wirklich – und zwar sofort, ohne gesonderten Speichern-Schritt. Einen Fehlgriff machst du mit Strg+Z rückgängig."),
        ];
        var lines = paragraphs.Select(p => Wrap(gfx, p, font, width - 2 * Pad)).ToList();
        var height = Pad + 18 + lines.Sum(l => l.Count * LineHeight) + (lines.Count - 1) * ParagraphGap + Pad;
        gfx.DrawRoundedRectangle(new XPen(RuleColor, 0.7), new XSolidBrush(XColor.FromArgb(0xEA, 0xF2, 0xFA)), x, y, width, height, 10, 10);
        var ty = y + Pad;
        gfx.DrawString(Lng.T("Anzeigen und Bearbeiten – ein Hinweis"), headFont, new XSolidBrush(Accent), x + Pad, ty + 10);
        ty += 18;
        foreach (var paragraph in lines)
        {
            foreach (var line in paragraph)
            {
                gfx.DrawString(line, font, XBrushes.Black, x + Pad, ty + 9);
                ty += LineHeight;
            }
            ty += ParagraphGap;
        }
        return y + height;
    }

    /// <summary>Fußzeile: Trennlinie, darunter zentriert Copyright und die Webadresse als klickbarer Link.</summary>
    private static void DrawFooter(XGraphics gfx, PdfSharp.Pdf.PdfPage page)
    {
        const string LinkText = "www.netradio.info";
        const string LinkUrl = "https://www.netradio.info/pdf/";
        XFont font = new("Segoe UI", 9);
        var lineY = page.Height.Point - FooterHeight;
        gfx.DrawLine(new XPen(RuleColor, 0.7), Margin, lineY, page.Width.Point - Margin, lineY);
        var copyright = $"© {DateTime.Now.Year} Wilhelm Happe   ·   ";
        var copyWidth = gfx.MeasureString(copyright, font).Width;
        var linkWidth = gfx.MeasureString(LinkText, font).Width;
        var x = (page.Width.Point - copyWidth - linkWidth) / 2;
        var textY = lineY + 16;
        gfx.DrawString(copyright, font, new XSolidBrush(XColor.FromArgb(90, 90, 90)), x, textY);
        gfx.DrawString(LinkText, font, new XSolidBrush(Accent), x + copyWidth, textY);
        // klickbare Fläche über dem Linktext (WorldToDefaultPage rechnet ins PDF-Koordinatensystem um)
        var linkRect = gfx.Transformer.WorldToDefaultPage(new XRect(x + copyWidth, textY - 10, linkWidth, 13));
        page.AddWebLink(new PdfSharp.Pdf.PdfRectangle(linkRect), LinkUrl);
    }

    /// <summary>Dezent hellblauer Seitenhintergrund.</summary>
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
        if (hIconSmall != 0) { _ = NativeMethods.DestroyIcon(hIconSmall); }
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
        finally { _ = NativeMethods.DestroyIcon(hIcon); }
    }

    [LibraryImport("shell32.dll", EntryPoint = "SHDefExtractIconW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int SHDefExtractIcon(string iconFile, int iconIndex, uint flags, out nint hIconLarge, out nint hIconSmall, uint iconSize);

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
