using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;

namespace PDFLight.Classes;

internal record PdfInfo(string Title, string Author, string Subject, string Keywords, int PageCount, string Version, string Creator, string Producer);

/// <summary>Kenndaten einer Datei; PageWidthPt/PageHeightPt = Größe der ersten Seite in Punkt (Referenz für die Zoomanzeige; 0 = unbekannt),
/// AnnotationCount = verwaltbare Anmerkungen (s. ListAnnotations) – schaltet den Verwaltungsdialog frei.</summary>
/// <summary>Eine Anmerkung fürs Verwalten (s. PdfEditService.ListAnnotations): Index = Position im Annots-Array der Seite,
/// ObjectNumber = PDF-Objektnummer (0 bei direkt eingebetteten Wörterbüchern) – sie identifiziert die Anmerkung beim
/// erneuten Öffnen sicher, der Index dient nur als Rückfall.</summary>
internal record AnnotationInfo(int Page, int Index, int ObjectNumber, string Subtype, string Contents, double LeftMm, double TopMm, double FontSize, AnnotationStyle Style);

/// <summary>Gestaltung einer Textanmerkung: Rahmen ja/nein, Hintergrundfarbe (null = transparent) und Schriftfarbe.</summary>
internal sealed record AnnotationStyle(bool Border, Color? Background, Color TextColor)
{
    public static readonly AnnotationStyle Default = new(true, Color.FromArgb(255, 255, 204), Color.Black);

    /// <summary>Hintergrund als RRGGBB für die Einstellungen; leer = transparent.</summary>
    public string BackgroundHex => Background is { } color ? ToHex(color) : string.Empty;

    public string TextColorHex => ToHex(TextColor);

    public static string ToHex(Color color) => $"{color.R:X2}{color.G:X2}{color.B:X2}";

    public static Color? ParseHex(string hex) =>
        hex.Length == 6 && int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb) ? Color.FromArgb(rgb >> 16 & 0xFF, rgb >> 8 & 0xFF, rgb & 0xFF) : null;
}

internal record PdfStatus(int PageCount, string? Version, string? PdfALevel, double PageWidthPt = 0, double PageHeightPt = 0, int AnnotationCount = 0);

/// <summary>Dokumentoperationen mit PDFsharp. Alle Methoden arbeiten direkt auf der Datei;
/// die Anzeige bleibt davon unberührt, weil der Viewer aus dem Speicher liest.</summary>
internal static partial class PdfEditService
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

    /// <summary>Seitenzahl, PDF-Version und PDF/A-Stufe der Datei für die Statusleiste;
    /// PageCount -1 und alles Weitere null, wenn die Datei nicht lesbar ist (z.B. verschlüsselt).</summary>
    public static PdfStatus TryReadStatus(string path)
    {
        try
        {
            using var document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            var v = document.Version;
            var first = document.PageCount > 0 ? document.Pages[0] : null;
            return new PdfStatus(document.PageCount, $"{v / 10}.{v % 10}", GetPdfALevel(document), first?.Width.Point ?? 0, first?.Height.Point ?? 0, CountAnnotations(document));
        }
        catch (Exception ex) when (IsPdfReadError(ex)) { return new PdfStatus(-1, null, null); }
    }

    /// <summary>Liest die deklarierte PDF/A-Stufe (z.B. "2b") aus den XMP-Metadaten des Dokuments;
    /// null, wenn keine deklariert ist. Erkennt Attribut- und Element-Schreibweise der pdfaid-Einträge.</summary>
    private static string? GetPdfALevel(PdfDocument document)
    {
        try
        {
            if (document.Internals.Catalog.Elements.GetDictionary("/Metadata")?.Stream is not { } stream) { return null; }
            var xmp = Encoding.UTF8.GetString(stream.UnfilteredValue);
            var part = PdfAPartRegex().Match(xmp).Groups[1].Value;
            if (part.Length == 0) { return null; }
            var conformance = PdfAConformanceRegex().Match(xmp).Groups[1].Value;
            return part + conformance.ToLowerInvariant();
        }
        catch (Exception ex) when (IsPdfReadError(ex)) { return null; }
    }

    // pdfaid:part="2" bzw. <pdfaid:part>2</pdfaid:part> — beide Schreibweisen kommen vor
    [GeneratedRegex(@"pdfaid:part(?:\s*=\s*[""']|\s*>\s*)(\d+)")]
    private static partial Regex PdfAPartRegex();

    [GeneratedRegex(@"pdfaid:conformance(?:\s*=\s*[""']|\s*>\s*)([A-Za-z])")]
    private static partial Regex PdfAConformanceRegex();

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

    /// <summary>Fügt eine FreeText-Anmerkung ein: ein gelber Textkasten an der Position (Millimeter von links/oben,
    /// unrotierte Seite) mit eigenem Erscheinungsbild – der Chromium-Viewer zeichnet Anmerkungen ohne
    /// Darstellungsstrom nicht (PDFsharps PdfTextAnnotation bliebe dort ein stummes Symbol). Schrift Helvetica
    /// (Standardschrift, nichts einzubetten), Text in WinAnsi; die Anmerkung bleibt als solche entfernbar.</summary>
    public static void AddFreeTextAnnotation(string path, int page, string text, double leftMm, double topMm, double fontSize, AnnotationStyle style)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        AppendFreeText(document, document.Pages[page - 1], text, leftMm, topMm, fontSize, style);
        document.Save(path);
    }

    /// <summary>Ersetzt eine FreeText-Anmerkung (Index im Annots-Array der Seite) durch eine neue mit geänderten Werten –
    /// auch fremde FreeText-Anmerkungen bekommen dabei PDFlights Kasten.</summary>
    public static void UpdateFreeTextAnnotation(string path, int page, int index, int objectNumber, string text, double leftMm, double topMm, double fontSize, AnnotationStyle style)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        var pdfPage = document.Pages[page - 1];
        pdfPage.Annotations.Elements.RemoveAt(ResolveIndex(pdfPage.Annotations, objectNumber, index));
        AppendFreeText(document, pdfPage, text, leftMm, topMm, fontSize, style);
        document.Save(path);
    }

    /// <summary>Entfernt eine Anmerkung (Index im Annots-Array der Seite) samt zugehörigem Popup.</summary>
    public static void DeleteAnnotation(string path, int page, int index, int objectNumber)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        var annotations = document.Pages[page - 1].Annotations;
        index = ResolveIndex(annotations, objectNumber, index);
        var popup = annotations[index].Elements["/Popup"] as PdfReference; // vor dem Entfernen lesen – danach zeigt der Index ins Leere
        annotations.Elements.RemoveAt(index);
        if (popup != null)
        {
            for (var i = annotations.Elements.Count - 1; i >= 0; i--)
            {
                if (annotations.Elements[i] is PdfReference reference && reference.ObjectID == popup.ObjectID) { annotations.Elements.RemoveAt(i); }
            }
        }
        document.Save(path);
    }

    private static bool IsManageable(string subtype) => subtype is not ("Link" or "Popup" or "Widget");

    private static int CountAnnotations(PdfDocument document)
    {
        var count = 0;
        for (var p = 0; p < document.PageCount; p++)
        {
            var annotations = document.Pages[p].Annotations;
            for (var i = 0; i < annotations.Count; i++)
            {
                if (IsManageable(annotations[i].Elements.GetName("/Subtype").TrimStart('/'))) { count++; }
            }
        }
        return count;
    }

    /// <summary>Die Position der Anmerkung mit dieser Objektnummer im Annots-Array; ohne Treffer (direkt eingebettetes
    /// Wörterbuch) der gemerkte Index, sofern er noch ins Array passt.</summary>
    private static int ResolveIndex(PdfAnnotations annotations, int objectNumber, int index)
    {
        if (objectNumber > 0)
        {
            for (var i = 0; i < annotations.Elements.Count; i++)
            {
                if (annotations.Elements[i] is PdfReference reference && reference.ObjectNumber == objectNumber) { return i; }
            }
        }
        if (index < 0 || index >= annotations.Elements.Count) { throw new InvalidOperationException("Die Anmerkung wurde in der Datei nicht mehr gefunden."); }
        return index;
    }

    /// <summary>Alle Anmerkungen des Dokuments fürs Verwalten – ohne Links, Popups und Formularfelder. Index = Position im
    /// Annots-Array der Seite (die übersprungenen Einträge zählen mit), Position in Millimetern von links/oben.</summary>
    public static List<AnnotationInfo> ListAnnotations(string path)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
        List<AnnotationInfo> result = [];
        for (var p = 0; p < document.PageCount; p++)
        {
            var page = document.Pages[p];
            var annotations = page.Annotations;
            for (var i = 0; i < annotations.Count; i++)
            {
                var a = annotations[i];
                var subtype = a.Elements.GetName("/Subtype").TrimStart('/');
                if (!IsManageable(subtype)) { continue; }
                var rect = a.Elements.GetRectangle("/Rect");
                var fontSize = 12.0;
                var match = FontSizeInDa().Match(a.Elements.GetString("/DA"));
                if (match.Success && double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var size) && size > 0) { fontSize = size; }
                var objectNumber = (annotations.Elements[i] as PdfReference)?.ObjectNumber ?? 0;
                result.Add(new AnnotationInfo(p + 1, i, objectNumber, subtype, a.Elements.GetString("/Contents"), rect.X1 * 25.4 / 72, (page.Height.Point - rect.Y2) * 25.4 / 72, fontSize, ReadStyle(a)));
            }
        }
        return result;
    }

    /// <summary>Gestaltung aus /C (Hintergrund: drei Komponenten = RGB, leeres Array = transparent, sonst Standard) und /BS /W (0 = kein Rahmen).</summary>
    private static AnnotationStyle ReadStyle(PdfAnnotation annotation)
    {
        var background = AnnotationStyle.Default.Background;
        if (annotation.Elements.GetArray("/C") is { } c)
        {
            background = c.Elements.Count == 3 ? Color.FromArgb(Channel(c, 0), Channel(c, 1), Channel(c, 2)) : c.Elements.Count == 0 ? null : background;
        }
        var border = true;
        if (annotation.Elements.GetDictionary("/BS") is { } bs && bs.Elements.ContainsKey("/W")) { border = bs.Elements.GetReal("/W") > 0; }
        var textColor = Color.Black;
        var rgb = TextColorInDa().Match(annotation.Elements.GetString("/DA")); // „r g b rg“ im Standarderscheinungsbild; „g“ (Grau) bleibt Schwarz
        if (rgb.Success) { textColor = Color.FromArgb(Component(rgb.Groups[1].Value), Component(rgb.Groups[2].Value), Component(rgb.Groups[3].Value)); }
        return new AnnotationStyle(border, background, textColor);
    }

    private static int Channel(PdfArray array, int index) => (int)Math.Round(Math.Clamp(array.Elements.GetReal(index), 0, 1) * 255);

    private static int Component(string value) => (int)Math.Round(Math.Clamp(double.Parse(value, CultureInfo.InvariantCulture), 0, 1) * 255);

    [GeneratedRegex(@"(\d*\.?\d+)\s+(\d*\.?\d+)\s+(\d*\.?\d+)\s+rg")]
    private static partial Regex TextColorInDa();

    [GeneratedRegex(@"(\d+(?:\.\d+)?)\s+Tf")]
    private static partial Regex FontSizeInDa();

    private static void AppendFreeText(PdfDocument document, PdfPage pdfPage, string text, double leftMm, double topMm, double fontSize, AnnotationStyle style)
    {
        var lines = SplitLines(text);
        var leading = fontSize * LeadingFactor;
        var (width, height) = MeasureAnnotation(text, fontSize);
        var left = leftMm * 72 / 25.4;
        var top = pdfPage.Height.Point - topMm * 72 / 25.4;

        var font = new PdfDictionary(document);
        font.Elements.SetName("/Type", "/Font");
        font.Elements.SetName("/Subtype", "/Type1");
        font.Elements.SetName("/BaseFont", "/Helvetica");
        font.Elements.SetName("/Encoding", "/WinAnsiEncoding");
        document.Internals.AddObject(font);
        var fonts = new PdfDictionary(document);
        fonts.Elements.SetReference("/Helv", font);
        var resources = new PdfDictionary(document);
        resources.Elements.SetObject("/Font", fonts);

        var content = new StringBuilder("q ");
        if (style.Background is { } fill)
        {
            content.Append(CultureInfo.InvariantCulture, $"{fill.R / 255.0:0.###} {fill.G / 255.0:0.###} {fill.B / 255.0:0.###} rg 0 0 {width:0.##} {height:0.##} re f ");
        }
        if (style.Border)
        {
            content.Append(CultureInfo.InvariantCulture, $"0.6 0.6 0.4 RG 0.5 w 0.25 0.25 {width - 0.5:0.##} {height - 0.5:0.##} re S ");
        }
        content.Append("Q ");
        var textColor = style.TextColor;
        content.Append(CultureInfo.InvariantCulture, $"BT /Helv {fontSize:0.##} Tf {textColor.R / 255.0:0.###} {textColor.G / 255.0:0.###} {textColor.B / 255.0:0.###} rg {leading:0.##} TL {Padding:0.##} {height - Padding - fontSize * 0.8:0.##} Td ");
        foreach (var line in lines)
        {
            content.Append('(').Append(line.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)")).Append(") Tj T* ");
        }
        content.Append("ET");
        var appearance = new PdfDictionary(document);
        appearance.Elements.SetName("/Type", "/XObject");
        appearance.Elements.SetName("/Subtype", "/Form");
        appearance.Elements.SetRectangle("/BBox", new PdfRectangle(new XRect(0, 0, width, height)));
        appearance.Elements.SetObject("/Resources", resources);
        appearance.CreateStream(ToWinAnsi(content.ToString()));
        document.Internals.AddObject(appearance);
        var appearances = new PdfDictionary(document);
        appearances.Elements.SetReference("/N", appearance);

        var annotation = new PdfDictionary(document);
        annotation.Elements.SetName("/Type", "/Annot");
        annotation.Elements.SetName("/Subtype", "/FreeText");
        annotation.Elements.SetRectangle("/Rect", new PdfRectangle(new XRect(left, top - height, width, height)));
        annotation.Elements.SetString("/Contents", text);
        annotation.Elements.SetString("/DA", string.Create(CultureInfo.InvariantCulture, $"/Helv {fontSize:0.##} Tf {textColor.R / 255.0:0.###} {textColor.G / 255.0:0.###} {textColor.B / 255.0:0.###} rg"));
        annotation.Elements.SetInteger("/F", 4); // drucken
        var color = new PdfArray(document); // /C = Hintergrund; leer = transparent (auch für Viewer, die das Erscheinungsbild neu aufbauen)
        if (style.Background is { } background)
        {
            color.Elements.Add(new PdfReal(background.R / 255.0));
            color.Elements.Add(new PdfReal(background.G / 255.0));
            color.Elements.Add(new PdfReal(background.B / 255.0));
        }
        annotation.Elements.SetObject("/C", color);
        var borderStyle = new PdfDictionary(document);
        borderStyle.Elements.SetName("/Type", "/Border");
        borderStyle.Elements.SetInteger("/W", style.Border ? 1 : 0);
        annotation.Elements.SetObject("/BS", borderStyle);
        annotation.Elements.SetObject("/AP", appearances);
        annotation.Elements.SetDateTime("/M", DateTime.Now);
        document.Internals.AddObject(annotation);
        pdfPage.Annotations.Elements.Add(annotation.Reference!); // nach AddObject hat das Objekt eine Referenz
    }

    public const double Padding = 4;           // Innenabstand des Anmerkungskastens (Punkt) – die Vorschau zeichnet damit
    public const double LeadingFactor = 1.25;  // Zeilenabstand relativ zur Schriftgröße

    /// <summary>Zeilen eines Anmerkungstexts – Zeilenumbrüche als CRLF, LF oder auch nur CR (so schreibt sie Acrobat).</summary>
    public static string[] SplitLines(string text) => text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

    /// <summary>Größe des Anmerkungskastens in Punkt für Text und Schriftgröße – dieselbe Rechnung wie beim Einfügen,
    /// damit die Vorschau im Dialog stimmt (Arial-Metriken stehen für Helvetica).</summary>
    public static (double Width, double Height) MeasureAnnotation(string text, double fontSize)
    {
        var lines = SplitLines(text);
        using var measure = XGraphics.CreateMeasureContext(new XSize(1000, 1000), XGraphicsUnit.Point, XPageDirection.Downwards);
        var font = new XFont("Arial", fontSize);
        var textWidth = Math.Max(lines.Max(line => measure.MeasureString(line, font).Width), fontSize * 2); // leerer Text: ein kleiner Kasten statt nichts
        return (textWidth + 2 * Padding, lines.Length * fontSize * LeadingFactor + 2 * Padding);
    }

    /// <summary>Speichert eine Seite als Einzelseiten-PDF für die Vorschau im Anmerkungsdialog – mit einem magentafarbenen
    /// Rahmen am Seitenrand, an dem der Dialog die Seitenfläche im abfotografierten Viewerbild sicher wiederfindet
    /// (Weiß gegen den hellgrauen Viewerhintergrund wäre zu unsicher). Liefert die Seitengröße in Punkt.
    /// excludeAnnotationIndex blendet die gerade bearbeitete Anmerkung aus.</summary>
    public static (double Width, double Height) ExtractPageForPreview(string sourcePath, string destinationPath, int page, int excludeAnnotationIndex = -1)
    {
        using var source = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import);
        using PdfDocument destination = new();
        var copy = destination.AddPage(source.Pages[page - 1]);
        if (excludeAnnotationIndex >= 0 && excludeAnnotationIndex < copy.Annotations.Count)
        {
            copy.Annotations.Elements.RemoveAt(excludeAnnotationIndex); // beim Bearbeiten zeigt die Vorschau nur den neuen Kasten
        }
        var (width, height) = (copy.Width.Point, copy.Height.Point);
        using (var gfx = XGraphics.FromPdfPage(copy, XGraphicsPdfPageOptions.Append))
        {
            gfx.DrawRectangle(new XPen(XColors.Magenta, PreviewFrameWidth), PreviewFrameWidth / 2, PreviewFrameWidth / 2, width - PreviewFrameWidth, height - PreviewFrameWidth);
        }
        destination.Save(destinationPath);
        return (width, height);
    }

    public const double PreviewFrameWidth = 3;  // Punkt – auch bei kleiner Vorschau noch ein erkennbarer Streifen

    /// <summary>Breite und Höhe einer Seite in Punkt (1-basiert).</summary>
    public static (double Width, double Height) GetPageSize(string path, int page)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
        var p = document.Pages[page - 1];
        return (p.Width.Point, p.Height.Point);
    }

    /// <summary>Kodiert Text für einen Inhaltsstrom in WinAnsi: Latin-1 direkt, die Windows-1252-Sonderzeichen
    /// (Euro, typografische Anführungszeichen, Gedankenstrich, Auslassungspunkte …) über die Tabelle, alles andere als „?“.</summary>
    private static byte[] ToWinAnsi(string text)
    {
        var bytes = new byte[text.Length];
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            bytes[i] = ch < 0x80 || (ch >= 0xA0 && ch <= 0xFF) ? (byte)ch : (byte)(ch switch
            {
                '€' => 0x80, '‚' => 0x82, 'ƒ' => 0x83, '„' => 0x84, '…' => 0x85, '†' => 0x86, '‡' => 0x87, 'ˆ' => 0x88, '‰' => 0x89,
                'Š' => 0x8A, '‹' => 0x8B, 'Œ' => 0x8C, 'Ž' => 0x8E, '‘' => 0x91, '’' => 0x92, '“' => 0x93, '”' => 0x94, '•' => 0x95,
                '–' => 0x96, '—' => 0x97, '˜' => 0x98, '™' => 0x99, 'š' => 0x9A, '›' => 0x9B, 'œ' => 0x9C, 'ž' => 0x9E, 'Ÿ' => 0x9F,
                _ => '?',
            });
        }
        return bytes;
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

    /// <summary>True, wenn sich die Datei mit dem Kennwort öffnen lässt (null/leer = ohne Kennwort).</summary>
    public static bool CanOpen(string path, string? password)
    {
        try
        {
            using var document = string.IsNullOrEmpty(password)
                ? PdfReader.Open(path, PdfDocumentOpenMode.Import)
                : PdfReader.Open(path, password, PdfDocumentOpenMode.Import);
            return true;
        }
        catch (Exception ex) when (IsPdfReadError(ex)) { return false; }
    }

    /// <summary>Speichert die Datei ohne Kennwortschutz neu: Seiten und Metadaten werden in ein
    /// unverschlüsseltes Dokument übernommen — unabhängig vom Verschlüsselungsverfahren der Quelle.</summary>
    public static void RemovePassword(string path, string password)
    {
        var bytes = File.ReadAllBytes(path); // Quelle in den Speicher, damit dieselbe Datei überschrieben werden kann
        using MemoryStream stream = new(bytes);
        using var source = PdfReader.Open(stream, password, PdfDocumentOpenMode.Import);
        using PdfDocument target = new();
        foreach (var page in source.Pages) { target.AddPage(page); }
        CopyInfo(source, target);
        target.Save(path);
    }

    /// <summary>Verschlüsselt die Datei mit AES-256 (PDF 2.0) und dem angegebenen Benutzer-Kennwort.
    /// Wie beim Entfernen wird die Datei aus einer Speicherkopie neu aufgebaut.</summary>
    public static void SetPassword(string path, string password)
    {
        var bytes = File.ReadAllBytes(path);
        using MemoryStream stream = new(bytes);
        using var source = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
        using PdfDocument target = new();
        foreach (var page in source.Pages) { target.AddPage(page); }
        CopyInfo(source, target);
        target.SecuritySettings.UserPassword = password;
        target.SecurityHandler.SetEncryptionToV5(); // AES-256, PDF 2.0
        target.Save(path);
    }

    /// <summary>Duplex-Zusammenführung: verzahnt hinter jede Seite der Datei die passende Rückseite aus
    /// backPath (bei backsReversed von hinten gezählt — der übliche Fall, wenn der Stapel zum Scannen
    /// gewendet wurde). Beide Dateien müssen gleich viele Seiten haben (prüft der Aufrufer).</summary>
    public static void MergeDuplex(string path, string backPath, bool backsReversed)
    {
        var bytes = File.ReadAllBytes(path);
        using MemoryStream stream = new(bytes);
        using var fronts = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
        using var backs = PdfReader.Open(backPath, PdfDocumentOpenMode.Import);
        using PdfDocument target = new();
        for (var i = 0; i < fronts.PageCount; i++)
        {
            target.AddPage(fronts.Pages[i]);
            target.AddPage(backs.Pages[backsReversed ? backs.PageCount - 1 - i : i]);
        }
        CopyInfo(fronts, target);
        target.Save(path);
    }

    private static void CopyInfo(PdfDocument source, PdfDocument target)
    {
        target.Info.Title = source.Info.Title;
        target.Info.Author = source.Info.Author;
        target.Info.Subject = source.Info.Subject;
        target.Info.Keywords = source.Info.Keywords;
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
    public static List<int>? ParsePageRange(string? input, int pageCount)
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
