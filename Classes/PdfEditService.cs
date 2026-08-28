using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PDFLight.Classes;

internal record PdfInfo(string Title, string Author, string Subject, string Keywords, int PageCount, string Version, string Creator, string Producer);

/// <summary>Dokumentoperationen mit PDFsharp. Alle Methoden arbeiten direkt auf der Datei;
/// die Anzeige bleibt davon unberührt, weil der Viewer aus dem Speicher liest.</summary>
internal static class PdfEditService
{
    /// <summary>Seitenzahl der Datei; -1, wenn die Datei nicht lesbar ist (z.B. verschlüsselt).</summary>
    public static int TryGetPageCount(string path)
    {
        try
        {
            using var document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            return document.PageCount;
        }
        catch (Exception ex) when (IsPdfReadError(ex)) { return -1; }
    }

    /// <summary>Löscht die angegebenen Seiten (1-basiert); mindestens eine Seite muss übrig bleiben.</summary>
    public static void DeletePages(string path, IReadOnlyList<int> pages)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        foreach (var page in pages.OrderByDescending(p => p)) { document.Pages.RemoveAt(page - 1); }
        document.Save(path);
    }

    /// <summary>Dreht die angegebenen Seiten (1-basiert) um delta Grad (±90 oder 180).</summary>
    public static void RotatePages(string path, IReadOnlyList<int> pages, int delta)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        foreach (var page in pages)
        {
            var p = document.Pages[page - 1];
            p.Rotate = ((p.Rotate + delta) % 360 + 360) % 360;
        }
        document.Save(path);
    }

    /// <summary>Hängt alle Seiten einer anderen PDF-Datei an; liefert die neue Gesamtseitenzahl.</summary>
    public static int AppendPdf(string path, string otherPdf)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        using var other = PdfReader.Open(otherPdf, PdfDocumentOpenMode.Import);
        foreach (var page in other.Pages) { document.AddPage(page); }
        var pageCount = document.PageCount; // muss vor Save() gelesen werden — danach ist das Dokument gesperrt
        document.Save(path);
        return pageCount;
    }

    /// <summary>Speichert die angegebenen Seiten (1-basiert) als neue Datei.</summary>
    public static void ExtractPages(string sourcePath, string destinationPath, IReadOnlyList<int> pages)
    {
        using var source = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import);
        using PdfDocument destination = new();
        foreach (var page in pages) { destination.AddPage(source.Pages[page - 1]); }
        destination.Save(destinationPath);
    }

    public static PdfInfo ReadInfo(string path)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Import); // Import = lesender Zugriff (ReadOnly ist in PDFsharp 6 nicht implementiert)
        var v = document.Version; // z.B. 14 → "1.4"
        return new PdfInfo(document.Info.Title, document.Info.Author, document.Info.Subject, document.Info.Keywords,
            document.PageCount, $"{v / 10}.{v % 10}", document.Info.Creator, document.Info.Producer);
    }

    public static void WriteInfo(string path, string title, string author, string subject, string keywords)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        document.Info.Title = title ?? string.Empty;
        document.Info.Author = author ?? string.Empty;
        document.Info.Subject = subject ?? string.Empty;
        document.Info.Keywords = keywords ?? string.Empty;
        document.Save(path);
    }

    /// <summary>Parst Seitenangaben wie "3", "2-5" oder "1, 4, 7-9"; null bei ungültiger Eingabe.</summary>
    public static List<int> ParsePageRange(string input, int pageCount)
    {
        List<int> pages = [];
        foreach (var part in (input ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var bounds = part.Split('-', StringSplitOptions.TrimEntries);
            if (bounds.Length == 1 && int.TryParse(bounds[0], out var single)) { pages.Add(single); }
            else if (bounds.Length == 2 && int.TryParse(bounds[0], out var from) && int.TryParse(bounds[1], out var to) && from <= to)
            {
                for (var i = from; i <= to; i++) { pages.Add(i); }
            }
            else { return null; }
        }
        pages = [.. pages.Distinct().OrderBy(p => p)];
        return pages.Count == 0 || pages[0] < 1 || pages[^1] > pageCount ? null : pages;
    }

    /// <summary>Alle Ausnahmen, die PDFsharp oder das Dateisystem beim Bearbeiten realistisch werfen.</summary>
    public static bool IsPdfReadError(Exception ex)
    {
        return ex is PdfSharp.PdfSharpException or IOException or UnauthorizedAccessException
            or InvalidOperationException or NotSupportedException or NotImplementedException
            or ArgumentException or IndexOutOfRangeException or NullReferenceException; // defekte PDFs lösen in PDFsharp mitunter auch Letzteres aus
    }
}
