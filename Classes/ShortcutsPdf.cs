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
        y += 22;
        // Abschnittsüberschrift links neben dem Icon, etwa auf Höhe seines unteren Randes; die Tabelle beginnt darunter
        var iconBottom = Margin - 4 + iconHeight;
        var headingY = Math.Max(y, iconBottom - 20);
        gfx.DrawString(Lng.T("Tastenkürzel"), sectionFont, new XSolidBrush(Accent), Margin, headingY + 12);
        y = Math.Max(headingY + 22, iconBottom + 8);

        // Zeilenhöhe so wählen, dass Tabelle und Hinweiskasten auf die Seite passen (12–17 Punkt); die
        // Erklärungszeilen (nur wo hinterlegt, z.B. F7) und der Kasten haben feste Höhe
        var rows = TaskDlg.ShortcutRows;
        var detailLines = rows.Select(r => r.Detail == null ? null : Wrap(gfx, Lng.T(r.Detail), detailFont, width - DetailIndent)).ToList();
        var detailHeight = detailLines.Sum(l => l == null ? 0 : l.Count * 12 + 4);
        var noteLines = NoteBoxLines(gfx, width);
        var noteHeight = NoteBoxHeight(noteLines);
        var footerTop = page.Height.Point - FooterHeight - 6;
        var rowHeight = Math.Clamp((footerTop - 2 - y - detailHeight - NoteGap - noteHeight - CommandLineHeight) / rows.Length, 12, 17); // 2 pt Reserve gegen Rundungsreste
        var fontSize = rowHeight >= 15 ? 10 : rowHeight >= 13.5 ? 9.5 : 9;
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
        y += NoteGap;
        if (y + noteHeight > footerTop) // zur Sicherheit — planmäßig eine Seite
        {
            DrawFooter(gfx, page);
            gfx.Dispose();
            page = document.AddPage();
            gfx = XGraphics.FromPdfPage(page);
            DrawPageBackground(gfx, page);
            y = Margin;
        }
        DrawNoteBox(gfx, Margin - NotePad, y, width + 2 * NotePad, noteLines, noteHeight); // Kasten um die Polsterung breiter, damit sein Text bündig mit der Tabelle steht
        y += noteHeight + 8;
        gfx.DrawString(Lng.T("Kommandozeile:") + " PDFlight.exe [" + Lng.T("Datei") + "] /max  /page:12  /print  /help   " + Lng.T("(auch mit „--“)"), detailFont, detailBrush, Margin, y + 9);
        DrawFooter(gfx, page);
        gfx.Dispose();
        document.Save(path);
        return path;
    }

    // Hinweiskasten unter der Kürzeltabelle: Anzeige aus dem Speicher, Viewer-Werkzeuge ohne Wirkung auf die
    // Datei, Bearbeiten-Befehle mit sofortigem Speichern
    private const double NoteGap = 16;
    private const double CommandLineHeight = 20; // Zeile mit den Startschaltern unter dem Hinweiskasten
    private const double NotePad = 7;
    private const double NoteLineHeight = 13.5;
    private const double NoteParagraphGap = 0; // die Absätze folgen als bloße Zeilenumbrüche
    private static readonly XFont NoteFont = new("Segoe UI", 10); // wie die Kürzeltabelle

    /// <summary>Die drei Absätze des Hinweiskastens, auf die Kastenbreite umbrochen.</summary>
    private static List<List<string>> NoteBoxLines(XGraphics gfx, double width)
    {
        string[] paragraphs =
        [
            Lng.T("PDFlight lädt die Datei zum Anzeigen in den Arbeitsspeicher. Die Datei selbst bleibt dabei frei: andere Programme können sie jederzeit ändern oder verschieben. Wird sie geändert, zeigt PDFlight den neuen Stand, sobald du zum Fenster zurückkehrst."),
            Lng.T("Die Werkzeuge in der Anzeige (Zoom, Ansicht drehen, Suchen) verändern nur die Darstellung, nie die Datei. Alles, was du dort drehst, ist beim nächsten Öffnen wieder wie vorher."),
            Lng.T("Die Befehle im Menü „Bearbeiten“ und in der Symbolleiste (Seiten löschen oder drehen, anhängen, Kennwort, Eigenschaften) ändern die Datei dagegen wirklich – und zwar sofort, ohne gesonderten Speichern-Schritt. Einen Fehlgriff machst du mit Strg+Z rückgängig."),
        ];
        return [.. paragraphs.Select(p => Wrap(gfx, p, NoteFont, width))]; // der Text steht bündig mit der Tabelle, der Kasten ragt um NotePad darüber hinaus
    }

    private static double NoteBoxHeight(List<List<string>> lines) =>
        NotePad + lines.Sum(l => l.Count * NoteLineHeight) + (lines.Count - 1) * NoteParagraphGap + NotePad;

    /// <summary>Zeichnet den Hinweiskasten: abgerundete, hellblaue Fläche mit den drei Absätzen.</summary>
    private static void DrawNoteBox(XGraphics gfx, double x, double y, double width, List<List<string>> lines, double height)
    {
        gfx.DrawRoundedRectangle(new XPen(RuleColor, 0.7), new XSolidBrush(XColor.FromArgb(0xEA, 0xF2, 0xFA)), x, y, width, height, 10, 10);
        var ty = y + NotePad;
        foreach (var paragraph in lines)
        {
            foreach (var line in paragraph)
            {
                gfx.DrawString(line, NoteFont, XBrushes.Black, x + NotePad, ty + 9);
                ty += NoteLineHeight;
            }
            ty += NoteParagraphGap;
        }
    }

    /// <summary>Fußzeile: Trennlinie, darunter zentriert Copyright und die Webadresse als klickbarer Link.</summary>
    private static void DrawFooter(XGraphics gfx, PdfSharp.Pdf.PdfPage page)
    {
        const string LinkText = "www.netradio.de/pdf";
        const string LinkUrl = "https://www.netradio.de/pdf/";
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
