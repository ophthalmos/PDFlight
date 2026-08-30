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

    /// <summary>Schreibt die Übersicht in den angegebenen Ordner (null = Downloads) und liefert den Dateipfad.</summary>
    public static string Create(string directory = null)
    {
        var path = Path.Combine(directory ?? GetDownloadsPath(), Lng.T("PDFlight-Tastenkürzel") + ".pdf");
        using PdfDocument document = new();
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
        var width = page.Width.Point - 2 * Margin;
        var y = Margin;
        gfx.DrawString(Application.ProductName + " – " + Lng.T("Tastenkürzel"), titleFont, XBrushes.Black, Margin, y + 17);
        y += 26;
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
        gfx.DrawString("Version " + version + " – " + DateTime.Now.ToString("d", System.Globalization.CultureInfo.GetCultureInfo(Lng.CultureCode)), subFont, detailBrush, Margin, y + 9);
        y += 30;

        foreach (var (key, text, detail) in TaskDlg.ShortcutRows)
        {
            var detailLines = Wrap(gfx, Lng.T(detail), detailFont, width - DetailIndent);
            var blockHeight = 15 + detailLines.Count * 12 + 9;
            if (y + blockHeight > page.Height.Point - Margin) // Seitenumbruch
            {
                gfx.Dispose();
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                y = Margin;
            }
            gfx.DrawString(Lng.T(key), keyFont, XBrushes.Black, Margin, y + 11);
            gfx.DrawString(Lng.T(text), textFont, XBrushes.Black, Margin + DetailIndent, y + 11);
            y += 15;
            foreach (var line in detailLines)
            {
                gfx.DrawString(line, detailFont, detailBrush, Margin + DetailIndent, y + 10);
                y += 12;
            }
            y += 9;
        }
        gfx.Dispose();
        document.Save(path);
        return path;
    }

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
