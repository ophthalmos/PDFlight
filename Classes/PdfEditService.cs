using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;

namespace PDFLight.Classes;

/// <summary>Angaben für den Eigenschaften-Dialog. Created/Modified aus dem Info-Wörterbuch (null = nicht angegeben), Tagged = Strukturbaum
/// bzw. /MarkInfo /Marked, PageWidthPt/PageHeightPt = erste Seite wie angezeigt (/Rotate berücksichtigt).</summary>
internal record PdfInfo(string Title, string Author, string Subject, string Keywords, int PageCount, string Version, string Creator, string Producer,
    DateTime? Created = null, DateTime? Modified = null, bool Tagged = false, double PageWidthPt = 0, double PageHeightPt = 0, IReadOnlyList<FontInfo>? Fonts = null);

/// <summary>Eine im Dokument verwendete Schrift (Reiter „Schriften“): Name ohne Untergruppen-Präfix, eingebettet bzw. als Untergruppe,
/// Typ (TrueType, Type 1, CID …) und Kodierung.</summary>
internal sealed record FontInfo(string Name, bool Embedded, bool Subset, string Type, string Encoding);

/// <summary>Die vier bearbeitbaren Metadaten des Eigenschaften-Dialogs.</summary>
internal sealed record DocumentText(string Title, string Author, string Subject, string Keywords);

/// <summary>Was „OK“ im Eigenschaften-Dialog schreibt (<see cref="PdfEditService.WriteProperties"/>): neue Metadaten (null = unverändert),
/// vorher alle Metadaten entfernen, Kennwort zum Öffnen (null = unverschlüsselt speichern).</summary>
internal sealed record PropertyChanges(DocumentText? Info, bool RemoveMetadata, string? Password);

/// <summary>Kenndaten einer Datei; PageWidthPt/PageHeightPt = Größe der ersten Seite in Punkt (Referenz für die Zoomanzeige; 0 = unbekannt),
/// AnnotationCount = verwaltbare Anmerkungen (s. ListAnnotations) – schaltet den Verwaltungsdialog frei.</summary>
/// <summary>Eine Anmerkung fürs Verwalten (s. PdfEditService.ListAnnotations): Index = Position im Annots-Array der Seite,
/// ObjectNumber = PDF-Objektnummer (0 bei direkt eingebetteten Wörterbüchern) – sie identifiziert die Anmerkung beim
/// erneuten Öffnen sicher, der Index dient nur als Rückfall.</summary>
internal record AnnotationInfo(int Page, int Index, int ObjectNumber, string Subtype, string Contents, double LeftMm, double TopMm, double FontSize, AnnotationStyle Style);

/// <summary>Gestaltung einer Textanmerkung: Rahmenfarbe (null = kein Rahmen), Hintergrundfarbe (null = transparent) und Schriftfarbe.</summary>
internal sealed record AnnotationStyle(Color? BorderColor, Color? Background, Color TextColor)
{
    public static readonly AnnotationStyle Default = new(Color.FromArgb(128, 128, 128), Color.FromArgb(255, 255, 204), Color.Black);

    /// <summary>Rahmen als RRGGBB für die Einstellungen; leer = kein Rahmen.</summary>
    public string BorderColorHex => BorderColor is { } color ? ToHex(color) : string.Empty;

    /// <summary>Hintergrund als RRGGBB für die Einstellungen; leer = transparent.</summary>
    public string BackgroundHex => Background is { } color ? ToHex(color) : string.Empty;

    public string TextColorHex => ToHex(TextColor);

    public static string ToHex(Color color) => $"{color.R:X2}{color.G:X2}{color.B:X2}";

    public static Color? ParseHex(string hex) =>
        hex.Length == 6 && int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb) ? Color.FromArgb(rgb >> 16 & 0xFF, rgb >> 8 & 0xFF, rgb & 0xFF) : null;
}

/// <summary>Wohin die neue Seite kommt (<see cref="PdfEditService.InsertBlankPage"/>): relativ zur angezeigten Seite oder ans Ende.</summary>
internal enum InsertPosition { After, Before, First, Last }

/// <summary>Format der neuen Seite: wie die angezeigte Seite, DIN A4, US Letter oder in den Maßen des Bildes (ohne Rand).</summary>
internal enum NewPageFormat { LikePage, A4, Letter, FromImage }

/// <summary>Einstellungen aus dem Dialog „Leere Seite einfügen“. Landscape gilt nur für A4/Letter. Das Bild steht in Originalgröße
/// (Maße aus den Pixeln und <paramref name="Dpi"/>, nur verkleinert, wenn es nicht passt) oder mit 10 mm Rand eingepasst
/// (<paramref name="FitImage"/>, auch vergrößert); bei <see cref="NewPageFormat.FromImage"/> bestimmen Pixel und Dpi die Seitengröße.</summary>
internal sealed record NewPageOptions(InsertPosition Position, NewPageFormat Format, bool Landscape, string? ImagePath, bool FitImage, double Dpi);

/// <summary>Ergebnis von <see cref="PdfEditService.LayoutNewPage"/>: Seitenmaße und Bildlage in Punkt (Ursprung links oben).</summary>
internal readonly record struct NewPageLayout(double Width, double Height, XRect? Image, double Scale);

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

    /// <summary>Löscht die angegebenen Seiten (1-basiert); mindestens eine Seite muss übrig bleiben.</summary>
    public static void DeletePages(string path, IReadOnlyList<int> pages)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        var removedPages = pages.Select(p => document.Pages[p - 1].Reference?.ObjectID).OfType<PdfObjectID>().ToHashSet();
        foreach (var page in pages.OrderByDescending(p => p)) { document.Pages.RemoveAt(page - 1); }
        PruneOutlines(document, removedPages);
        SaveCompact(document, path);
    }

    /// <summary>Nach dem Löschen von Seiten: Lesezeichen, die auf eine gelöschte Seite zeigen, werden entfernt – ihre Unterpunkte
    /// rücken eine Ebene hoch. Sonst blieben Einträge ins Leere zurück (PDFsharp schreibt die verwaiste Seite sogar mit).
    /// Ziele als /Dest oder GoTo-Aktion, jeweils als Array oder als benannte Zielmarke (Katalog /Dests bzw. /Names-Baum).</summary>
    private static void PruneOutlines(PdfDocument document, HashSet<PdfObjectID> removedPages)
    {
        if (removedPages.Count == 0 || document.Internals.Catalog.Elements["/Outlines"] == null) { return; }
        try { Prune(document.Outlines); }
        catch (Exception ex) when (IsPdfReadError(ex)) { } // kaputte Gliederung: lieber unverändert lassen als den Vorgang abbrechen

        void Prune(PdfOutlineCollection outlines)
        {
            var i = 0;
            while (i < outlines.Count)
            {
                var outline = outlines[i];
                Prune(outline.Outlines); // Unterpunkte zuerst – die hochgezogenen sind damit schon geprüft
                if (DestinationPageId(document, outline) is { } id && removedPages.Contains(id))
                {
                    var children = outline.Outlines.ToList();
                    outlines.RemoveAt(i);
                    foreach (var child in children) { outline.Outlines.Remove(child); outlines.Insert(i++, child); } // i steht danach hinter den Kindern
                }
                else { i++; }
            }
        }
    }

    /// <summary>Die Seite, auf die ein Lesezeichen zeigt (Objektkennung); null, wenn das Ziel nicht auflösbar ist.</summary>
    private static PdfObjectID? DestinationPageId(PdfDocument document, PdfDictionary outline)
    {
        var destination = outline.Elements["/Dest"];
        if (destination == null && outline.Elements.GetDictionary("/A") is { } action && action.Elements.GetName("/S") == "/GoTo") { destination = action.Elements["/D"]; }
        if (destination is PdfReference reference) { destination = reference.Value; }
        if (destination is PdfString or PdfName) { destination = ResolveNamedDestination(document, destination is PdfName name ? name.Value.TrimStart('/') : ((PdfString)destination).Value); }
        if (destination is PdfDictionary dictionary) { destination = dictionary.Elements["/D"]; } // benannte Ziele stehen oft als << /D [...] >>
        return destination is PdfArray array && array.Elements.Count > 0 && array.Elements[0] is PdfReference page ? page.ObjectID : null;
    }

    /// <summary>Benannte Zielmarke auflösen: erst das alte /Dests-Wörterbuch des Katalogs, dann der /Names-Baum (/Dests, Kids/Names).</summary>
    private static PdfItem? ResolveNamedDestination(PdfDocument document, string name)
    {
        var catalog = document.Internals.Catalog;
        if (catalog.Elements.GetDictionary("/Dests") is { } dests && dests.Elements["/" + name] is { } direct) { return direct is PdfReference r ? r.Value : direct; }
        return catalog.Elements.GetDictionary("/Names")?.Elements.GetDictionary("/Dests") is { } tree ? FindInNameTree(tree, name) : null;
    }

    private static PdfItem? FindInNameTree(PdfDictionary node, string name)
    {
        if (node.Elements.GetArray("/Names") is { } names)
        {
            for (var i = 0; i + 1 < names.Elements.Count; i += 2)
            {
                if (names.Elements[i] is PdfString key && key.Value == name) { return names.Elements[i + 1] is PdfReference r ? r.Value : names.Elements[i + 1]; }
            }
        }
        if (node.Elements.GetArray("/Kids") is { } kids)
        {
            foreach (var kid in kids.Elements)
            {
                var child = kid is PdfReference kr ? kr.Value as PdfDictionary : kid as PdfDictionary;
                if (child == null) { continue; }
                if (child.Elements.GetArray("/Limits") is { Elements.Count: 2 } limits
                    && (string.CompareOrdinal(name, (limits.Elements[0] as PdfString)?.Value) < 0 || string.CompareOrdinal(name, (limits.Elements[1] as PdfString)?.Value) > 0)) { continue; }
                if (FindInNameTree(child, name) is { } found) { return found; }
            }
        }
        return null;
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
        SaveCompact(document, path);
    }

    /// <summary>Fügt eine FreeText-Anmerkung ein: ein gelber Textkasten an der Position (Millimeter von links/oben,
    /// unrotierte Seite) mit eigenem Erscheinungsbild – manche Betrachter (etwa Chromium) zeichnen Anmerkungen ohne
    /// Darstellungsstrom nicht (PDFsharps PdfTextAnnotation bliebe dort ein stummes Symbol). Schrift Helvetica
    /// (Standardschrift, nichts einzubetten), Text in WinAnsi; die Anmerkung bleibt als solche entfernbar.</summary>
    public static void AddFreeTextAnnotation(string path, int page, string text, double leftMm, double topMm, double fontSize, AnnotationStyle style)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        AppendFreeText(document, document.Pages[page - 1], text, leftMm, topMm, fontSize, style);
        SaveCompact(document, path);
    }

    /// <summary>Ersetzt eine FreeText-Anmerkung (Index im Annots-Array der Seite) durch eine neue mit geänderten Werten –
    /// auch fremde FreeText-Anmerkungen bekommen dabei PDFlights Kasten.</summary>
    public static void UpdateFreeTextAnnotation(string path, int page, int index, int objectNumber, string text, double leftMm, double topMm, double fontSize, AnnotationStyle style)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        var pdfPage = document.Pages[page - 1];
        pdfPage.Annotations.Elements.RemoveAt(ResolveIndex(pdfPage.Annotations, objectNumber, index));
        AppendFreeText(document, pdfPage, text, leftMm, topMm, fontSize, style);
        SaveCompact(document, path);
    }

    /// <summary>Verschiebt eine Freitext-Anmerkung oder einen Stempel (Index im Annots-Array der Seite) um dx/dy Punkt: nur ihr /Rect –
    /// der Darstellungsstrom wird auf /Rect abgebildet und wandert damit mit. Der Kasten bleibt ganz auf der Seite (MediaBox).</summary>
    public static void MoveAnnotation(string path, int page, int index, double dx, double dy)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        var pdfPage = document.Pages[page - 1];
        var annotation = AnnotationAt(pdfPage.Annotations, index);
        if (annotation.Elements.GetName("/Subtype") is not ("/FreeText" or "/Stamp")) { throw new InvalidOperationException("An dieser Stelle steht keine Textanmerkung und kein Stempel mehr."); }
        var rect = annotation.Elements.GetRectangle("/Rect");
        var box = pdfPage.MediaBox;
        var (width, height) = (rect.Width, rect.Height);
        var x = Math.Clamp(Math.Min(rect.X1, rect.X2) + dx, box.X1, Math.Max(box.X1, box.X2 - width));
        var y = Math.Clamp(Math.Min(rect.Y1, rect.Y2) + dy, box.Y1, Math.Max(box.Y1, box.Y2 - height));
        annotation.Elements.SetRectangle("/Rect", new PdfRectangle(new XRect(x, y, width, height)));
        SaveCompact(document, path);
    }

    /// <summary>Entfernt eine Anmerkung (Index im Annots-Array der Seite) samt zugehörigem Popup.</summary>
    public static void DeleteAnnotation(string path, int page, int index, int objectNumber)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        var annotations = document.Pages[page - 1].Annotations;
        index = ResolveIndex(annotations, objectNumber, index);
        var removed = AnnotationAt(annotations, index); // VOR dem Entfernen greifen – danach zeigt der Index ins Leere (IDE-Umformungen haben das schon einmal vertauscht)
        annotations.Elements.RemoveAt(index);
        if (removed.Elements["/Popup"] is PdfReference popup)
        {
            for (var i = annotations.Elements.Count - 1; i >= 0; i--)
            {
                if (annotations.Elements[i] is PdfReference reference && reference.ObjectID == popup.ObjectID) { annotations.Elements.RemoveAt(i); }
            }
        }
        SaveCompact(document, path);
    }

    /// <summary>Löscht alle verwaltbaren Anmerkungen (s. ListAnnotations) auf allen Seiten – Links, Popups und Formularfelder bleiben,
    /// Popups verschwinden nur mit ihrer Anmerkung. Ein Speichervorgang, also ein Rückgängig. Liefert die Anzahl.</summary>
    public static int DeleteAllAnnotations(string path)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        var count = 0;
        for (var p = 0; p < document.PageCount; p++)
        {
            var annotations = document.Pages[p].Annotations;
            List<PdfObjectID> popups = [];
            foreach (var (i, annotation) in AnnotationDictionaries(annotations).Reverse())
            {
                if (!IsManageable(annotation.Elements.GetName("/Subtype").TrimStart('/'))) { continue; }
                if (annotation.Elements["/Popup"] is PdfReference popup) { popups.Add(popup.ObjectID); }
                annotations.Elements.RemoveAt(i);
                count++;
            }
            for (var i = annotations.Elements.Count - 1; i >= 0; i--)
            {
                if (annotations.Elements[i] is PdfReference reference && popups.Contains(reference.ObjectID)) { annotations.Elements.RemoveAt(i); }
            }
        }
        if (count > 0) { SaveCompact(document, path); }
        return count;
    }

    private static bool IsManageable(string subtype) => subtype is not ("Link" or "Popup" or "Widget");

    /// <summary>Die Wörterbücher im /Annots-Array einer Seite mit ihrem Index – direkt aus den Elementen, denn PDFsharps
    /// Indexer baut daraus PdfAnnotation-Objekte und stürzt über Einträge, die kein Wörterbuch sind (null-Objekt, Zahl, kaputte
    /// Referenz – gesehen in einem Canva-Export). Solche Einträge werden übersprungen.</summary>
    private static IEnumerable<(int Index, PdfDictionary Dictionary)> AnnotationDictionaries(PdfAnnotations annotations)
    {
        for (var i = 0; i < annotations.Elements.Count; i++)
        {
            var item = annotations.Elements[i];
            var dictionary = item is PdfReference reference ? reference.Value as PdfDictionary : item as PdfDictionary;
            if (dictionary != null) { yield return (i, dictionary); }
        }
    }

    private static PdfDictionary AnnotationAt(PdfAnnotations annotations, int index)
    {
        var item = annotations.Elements[index];
        return (item is PdfReference reference ? reference.Value as PdfDictionary : item as PdfDictionary)
            ?? throw new InvalidOperationException("Die Anmerkung wurde in der Datei nicht mehr gefunden.");
    }

    /// <summary>Entfernt die komplette Gliederung (Lesezeichen) der Datei und schaltet den Seitenmodus auf „ohne Leiste“,
    /// sonst zeigt der Viewer weiter eine leere Lesezeichenleiste. Die Seiten bleiben unberührt.</summary>
    public static void RemoveOutlines(string path)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        RemoveOutlines(document);
        SaveCompact(document, path);
    }

    private static void RemoveOutlines(PdfDocument document)
    {
        var catalog = document.Internals.Catalog;
        catalog.Elements.Remove("/Outlines");
        if (catalog.Elements.GetName("/PageMode") == "/UseOutlines") { catalog.Elements.Remove("/PageMode"); }
    }

    /// <summary>Die Kinder eines Gliederungseintrags als rohe Wörterbücher (/First, dann die /Next-Kette) – bewusst nicht über PDFsharps
    /// PdfOutlineCollection: die legt im Katalog ein Outline-Objekt an, das beim Speichern (PrepareForSave) /PageMode /UseOutlines und
    /// die alte Verkettung wieder einträgt (Review 20.09.2026). Lesen und Schreiben nutzen denselben Durchlauf, damit
    /// <see cref="Bookmark.Id"/> auf beiden Seiten dieselbe Position bezeichnet; ein Zyklus in /Next bricht über die besuchten Objekte ab.</summary>
    private static List<PdfDictionary> OutlineChildren(PdfDictionary parent, HashSet<PdfDictionary> visited)
    {
        List<PdfDictionary> children = [];
        for (var item = parent.Elements.GetDictionary("/First"); item != null && visited.Add(item); item = item.Elements.GetDictionary("/Next")) { children.Add(item); }
        return children;
    }

    /// <summary>Liest die Gliederung (Lesezeichen) für den Editor: Titel, Zielseite (1-basiert; 0 = nicht auflösbar), Aufklappzustand
    /// und Unterpunkte; ohne /Outlines eine leere Liste. <see cref="Bookmark.Id"/> zählt den Vorordnungs-Durchlauf mit – denselben
    /// nimmt <see cref="WriteOutlines"/>, um Ziele, Farben und Schriftstile unveränderter Einträge zu übernehmen.</summary>
    public static List<Bookmark> ReadOutlines(string path)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
        List<Bookmark> result = [];
        if (document.Internals.Catalog.Elements.GetDictionary("/Outlines") is not { } root) { return result; }
        var pageNumbers = new Dictionary<PdfObjectID, int>();
        for (var i = 0; i < document.PageCount; i++) { if (document.Pages[i].Reference is { } reference) { pageNumbers[reference.ObjectID] = i + 1; } }
        HashSet<PdfDictionary> visited = [];
        var id = 0;
        Read(root, result);
        return result;

        void Read(PdfDictionary parent, List<Bookmark> target)
        {
            foreach (var item in OutlineChildren(parent, visited))
            {
                var page = DestinationPageId(document, item) is { } pageId && pageNumbers.TryGetValue(pageId, out var number) ? number : 0;
                Bookmark bookmark = new() { Title = item.Elements.GetString("/Title"), Page = page, OriginalPage = page, Open = item.Elements.GetInteger("/Count") > 0, Id = id++ };
                target.Add(bookmark);
                Read(item, bookmark.Children);
            }
        }
    }

    /// <summary>Alle Gliederungseinträge in Vorordnung (wie <see cref="ReadOutlines"/>), samt Wurzel als erstem Element der Rückgabe.</summary>
    private static (PdfDictionary? Root, List<PdfDictionary> Items) CollectOutlineItems(PdfDocument document)
    {
        List<PdfDictionary> items = [];
        if (document.Internals.Catalog.Elements.GetDictionary("/Outlines") is not { } root) { return (null, items); }
        HashSet<PdfDictionary> visited = [];
        Collect(root);
        return (root, items);

        void Collect(PdfDictionary parent)
        {
            foreach (var item in OutlineChildren(parent, visited)) { items.Add(item); Collect(item); }
        }
    }

    /// <summary>Die übernehmbaren Teile eines vorhandenen Gliederungseintrags.</summary>
    private sealed record OutlineParts(PdfItem? Destination, PdfItem? Action, PdfItem? Color, PdfItem? Flags);

    /// <summary>Schreibt die Gliederung aus dem Editor neu: /Outlines wird verworfen und aus dem Modell frisch aufgebaut (/Title als
    /// Unicode, /Parent, /First, /Last, /Prev, /Next, /Count mit Vorzeichen für den Aufklappzustand). Einträge mit unveränderter
    /// Zielseite behalten ihr ursprüngliches Ziel (/Dest oder GoTo-/A samt Position und Zoom) sowie Farbe (/C) und Schriftstil (/F);
    /// neue oder umgehängte Ziele zeigen auf den Seitenanfang bei unverändertem Zoom (/XYZ null oben null). Ohne Lesezeichen wird
    /// die Gliederung entfernt wie in <see cref="RemoveOutlines(string)"/>. Die alten Wörterbücher werden geleert: PDFsharp schreibt
    /// verwaiste Objekte mit, leer kosten sie nur wenige Byte und verraten nichts mehr (Review 20.09.2026).</summary>
    public static void WriteOutlines(string path, IReadOnlyList<Bookmark> bookmarks)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        if (bookmarks.Count == 0)
        {
            RemoveOutlines(document);
            SaveCompact(document, path);
            return;
        }
        var catalog = document.Internals.Catalog;
        var (oldRoot, oldItems) = CollectOutlineItems(document);
        var original = oldItems.Select(i => new OutlineParts(i.Elements["/Dest"], i.Elements["/A"], i.Elements["/C"], i.Elements["/F"])).ToList();
        var root = new PdfDictionary(document);
        root.Elements.SetName("/Type", "/Outlines");
        document.Internals.AddObject(root);
        Write(root, bookmarks, open: true);
        catalog.Elements.SetReference("/Outlines", root);
        oldRoot?.Elements.Clear();
        foreach (var item in oldItems) { item.Elements.Clear(); } // die übernommenen Teile hängen jetzt an den neuen Einträgen
        SaveCompact(document, path);

        void Write(PdfDictionary parent, IReadOnlyList<Bookmark> items, bool open)
        {
            PdfDictionary? previous = null;
            var visible = 0; // sichtbare Nachkommen, wenn der Elternknoten aufgeklappt ist (PDF-Referenz: /Count)
            foreach (var item in items)
            {
                var entry = new PdfDictionary(document);
                document.Internals.AddObject(entry);
                entry.Elements["/Title"] = new PdfString(item.Title, PdfStringEncoding.Unicode);
                entry.Elements.SetReference("/Parent", parent);
                if (previous != null) { entry.Elements.SetReference("/Prev", previous); previous.Elements.SetReference("/Next", entry); }
                else { parent.Elements.SetReference("/First", entry); }
                var keep = item.Id >= 0 && item.Id < original.Count && item.Page == item.OriginalPage ? original[item.Id] : null; // auch Einträge ohne Seitenziel (URI, GoToR, JavaScript) behalten ihre Aktion, solange keine Seite gesetzt wurde
                if (keep?.Destination != null) { entry.Elements["/Dest"] = keep.Destination; }
                else if (keep?.Action != null) { entry.Elements["/A"] = keep.Action; }
                else if (item.Page >= 1 && item.Page <= document.PageCount)
                {
                    var page = document.Pages[item.Page - 1];
                    entry.Elements["/Dest"] = new PdfArray(document, page.Reference!, new PdfName("/XYZ"), PdfNull.Value, new PdfReal(page.Height.Point), PdfNull.Value); // Seiten sind indirekte Objekte
                }
                if (keep?.Color != null) { entry.Elements["/C"] = keep.Color; }
                if (keep?.Flags != null) { entry.Elements["/F"] = keep.Flags; }
                if (item.Children.Count > 0) { Write(entry, item.Children, item.Open); }
                visible += 1 + (item.Open && item.Children.Count > 0 ? Math.Abs(entry.Elements.GetInteger("/Count")) : 0);
                previous = entry;
            }
            if (previous != null) { parent.Elements.SetReference("/Last", previous); }
            parent.Elements.SetInteger("/Count", open ? visible : -visible);
        }
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
            foreach (var (i, a) in AnnotationDictionaries(annotations))
            {
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

    /// <summary>Gestaltung aus /C (Hintergrund: drei Komponenten = RGB, leeres Array = transparent, sonst Standard), /BS /W (0 = kein Rahmen)
    /// und der Rahmenfarbe aus dem eigenen Darstellungsstrom („r g b RG“ – die Anmerkung selbst kennt keine Rahmenfarbe).</summary>
    private static AnnotationStyle ReadStyle(PdfDictionary annotation)
    {
        var background = AnnotationStyle.Default.Background;
        if (annotation.Elements.GetArray("/C") is { } c)
        {
            background = c.Elements.Count == 3 ? Color.FromArgb(Channel(c, 0), Channel(c, 1), Channel(c, 2)) : c.Elements.Count == 0 ? null : background;
        }
        var borderColor = AnnotationStyle.Default.BorderColor;
        if (annotation.Elements.GetDictionary("/BS") is { } bs && bs.Elements.ContainsKey("/W") && bs.Elements.GetReal("/W") <= 0) { borderColor = null; }
        else if (annotation.Elements.GetDictionary("/AP")?.Elements.GetDictionary("/N") is { Stream: { } stream })
        {
            var strokeColor = BorderColorInStream().Match(PdfEncoders.RawEncoding.GetString(stream.UnfilteredValue));
            if (strokeColor.Success) { borderColor = Color.FromArgb(Component(strokeColor.Groups[1].Value), Component(strokeColor.Groups[2].Value), Component(strokeColor.Groups[3].Value)); }
        }
        var textColor = Color.Black;
        var rgb = TextColorInDa().Match(annotation.Elements.GetString("/DA")); // „r g b rg“ im Standarderscheinungsbild; „g“ (Grau) bleibt Schwarz
        if (rgb.Success) { textColor = Color.FromArgb(Component(rgb.Groups[1].Value), Component(rgb.Groups[2].Value), Component(rgb.Groups[3].Value)); }
        return new AnnotationStyle(borderColor, background, textColor);
    }

    private static int Channel(PdfArray array, int index) => (int)Math.Round(Math.Clamp(array.Elements.GetReal(index), 0, 1) * 255);

    private static int Component(string value) => (int)Math.Round(Math.Clamp(double.Parse(value, CultureInfo.InvariantCulture), 0, 1) * 255);

    [GeneratedRegex(@"(\d*\.?\d+)\s+(\d*\.?\d+)\s+(\d*\.?\d+)\s+rg")]
    private static partial Regex TextColorInDa();

    [GeneratedRegex(@"(\d*\.?\d+)\s+(\d*\.?\d+)\s+(\d*\.?\d+)\s+RG")]
    private static partial Regex BorderColorInStream();

    [GeneratedRegex(@"(\d+(?:\.\d+)?)\s+Tf")]
    private static partial Regex FontSizeInDa();

    [GeneratedRegex(@"\s*\([^)]*\)\s*$")] // „(WH)“ am Ende der Datumszeile eines Stempels
    private static partial Regex InitialsSuffix();

    /// <summary>Setzt einen Stempel der Palette auf eine Seite: Stamp-Anmerkung mit eigenem Darstellungsstrom (Helvetica-Bold,
    /// Rahmen in der Schriftfarbe, wahlweise Hintergrund) an einer der festen Positionen, 10 mm vom Rand der unrotierten Seite.</summary>
    /// <param name="replaceIndex">≥ 0: dieser vorhandene Stempel (Index im Annots-Array, ObjectNumber als sichere Kennung) wird vorher entfernt.</param>
    /// <param name="top">Oberkante des Kastens in PDF-Koordinaten (Stapelplatz, s. FindStampStack); null = die feste Position des Stempels.</param>
    public static void AddStamp(string path, int page, Stamp stamp, int replaceIndex = -1, int replaceObjectNumber = 0, double? top = null)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        var pdfPage = document.Pages[page - 1];
        if (replaceIndex >= 0) { pdfPage.Annotations.Elements.RemoveAt(ResolveIndex(pdfPage.Annotations, replaceObjectNumber, replaceIndex)); }
        AppendStamp(document, pdfPage, stamp, stamp.SecondLine(DateTime.Now), top); // Datum, Uhrzeit und Kürzel klein unter dem Text
        SaveCompact(document, path);
    }

    /// <summary>Sucht auf der Seite einen PDFlight-Stempel, der den Platz des neuen Stempels überlappt (gleiche feste Position);
    /// null, wenn dort keiner liegt. Liefert Index, Objektnummer und Text für die Rückfrage vor dem Einfügen.</summary>
    public const int StackLimit = 3;                    // Stempel je Position: der erste plus zwei gestapelte
    private const double PageMarginPt = 10 * 72 / 25.4; // Abstand vom Seitenrand für Stempel und eingefügte Bilder (10 mm)
    private const double StackGap = 3 * 72 / 25.4;      // Abstand zwischen gestapelten Stempeln (3 mm)

    /// <summary>Ein vorhandener PDFlight-Stempel am Platz des neuen (Index, Objektnummer, Text, Datumszeile, Oberkante).</summary>
    internal sealed record StampSlot(int Index, int ObjectNumber, string Text, string SecondLine, double Top);

    /// <summary>Der Stapel am Platz des neuen Stempels (leer, wenn dort nichts liegt) und die Oberkante des nächsten freien Platzes –
    /// null, wenn der Stapel voll ist oder der nächste Platz nicht mehr auf die Seite passt. Gestapelt wird unter den vorhandenen
    /// Stempel; bei der Seitenmitte kommt der zweite darüber und der dritte darunter.</summary>
    public static (List<StampSlot> Stack, double? FreeTop) FindStampStack(string path, int page, Stamp stamp)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
        var pdfPage = document.Pages[page - 1];
        var planned = StampRect(pdfPage, stamp, stamp.SecondLine(DateTime.Now));
        var annotations = pdfPage.Annotations;
        List<(int Index, int ObjectNumber, string Contents, XRect Rect)> own = [];
        foreach (var (i, a) in AnnotationDictionaries(annotations))
        {
            if (a.Elements.GetName("/Subtype") != "/Stamp" || a.Elements.GetName("/Name") != "/PDFlightStamp") { continue; }
            var r = a.Elements.GetRectangle("/Rect");
            var objectNumber = annotations.Elements[i] is PdfReference reference ? reference.ObjectNumber : 0;
            own.Add((i, objectNumber, a.Elements.GetString("/Contents"), new XRect(Math.Min(r.X1, r.X2), Math.Min(r.Y1, r.Y2), Math.Abs(r.X2 - r.X1), Math.Abs(r.Y2 - r.Y1))));
        }
        List<StampSlot> stack = [];
        var probe = planned;
        for (var slot = 0; slot < StackLimit; slot++)
        {
            var hit = own.FirstOrDefault(o => o.Rect.IntersectsWith(probe) && stack.All(s => s.Index != o.Index));
            if (hit.Contents == null) { break; } // Platz frei
            var lines = SplitLines(hit.Contents);
            stack.Add(new StampSlot(hit.Index, hit.ObjectNumber, lines[0], lines.Length > 1 ? lines[^1] : string.Empty, hit.Rect.Y + hit.Rect.Height));
            var anchor = stamp.Position == StampPosition.Center && slot == 1 ? stack[0] : stack[^1]; // Seitenmitte: 2. darüber, 3. unter dem 1.
            var above = stamp.Position == StampPosition.Center && slot == 0;
            var top = above ? anchor.Top + StackGap + planned.Height : (slot == 0 ? hit.Rect.Y : own.First(o => o.Index == anchor.Index).Rect.Y) - StackGap;
            probe = new XRect(planned.X, top - planned.Height, planned.Width, planned.Height);
        }
        var fits = probe.Y >= PageMarginPt && probe.Y + probe.Height <= pdfPage.Height.Point - PageMarginPt;
        return (stack, stack.Count < StackLimit && fits ? probe.Y + probe.Height : null);
    }

    /// <summary>Kasten des Stempels auf der Seite (PDF-Koordinaten, Ursprung links unten): feste Position, 10 mm vom Rand.</summary>
    private static XRect StampRect(PdfPage pdfPage, Stamp stamp, string dateLine, double? topOverride = null)
    {
        var (width, height, _, _) = MeasureStamp(stamp.Text, stamp.FontSize, dateLine);
        var pageWidth = pdfPage.Width.Point;
        var pageHeight = pdfPage.Height.Point;
        var left = stamp.Position switch
        {
            StampPosition.TopLeft => PageMarginPt,
            StampPosition.TopRight => pageWidth - PageMarginPt - width,
            _ => (pageWidth - width) / 2,
        };
        var top = topOverride ?? (stamp.Position == StampPosition.Center ? (pageHeight + height) / 2 : pageHeight - PageMarginPt);
        return new XRect(left, top - height, width, height);
    }

    /// <summary>Privater Schlüssel in der Stempel-Anmerkung mit der Stempeldefinition als JSON (Text, Größe, Farben, Position, Deckkraft …).</summary>
    private const string StampDataKey = "/PDFlightStampData";

    /// <summary>Liest die Stempeldefinition aus einer PDFlight-Stempel-Anmerkung. Ohne gespeicherte Definition (Stempel aus einer
    /// früheren Fassung) entsteht ein Stempel aus Text und Farbe der Anmerkung.</summary>
    public static Stamp ReadStamp(string path, int page, int index, int objectNumber)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
        var annotations = document.Pages[page - 1].Annotations;
        var annotation = AnnotationAt(annotations, ResolveIndex(annotations, objectNumber, index));
        var json = annotation.Elements.GetString(StampDataKey);
        if (json.Length > 0)
        {
            try { if (JsonSerializer.Deserialize<Stamp>(json) is { } stored) { return stored; } }
            catch (JsonException) { } // beschädigte Definition – dann der Rückfall aus den Anmerkungsdaten
        }
        var lines = SplitLines(annotation.Elements.GetString("/Contents"));
        Stamp stamp = new() { Text = lines[0], WithDate = lines.Length > 1 };
        if (annotation.Elements.GetArray("/C") is { Elements.Count: 3 } color)
        {
            stamp.TextColor = AnnotationStyle.ToHex(Color.FromArgb(Channel(color, 0), Channel(color, 1), Channel(color, 2)));
        }
        return stamp;
    }

    /// <summary>Ersetzt einen Stempel durch die (geänderte) Definition – wie beim Bearbeiten einer Textanmerkung wird der
    /// Eintrag neu gezeichnet. Die ursprüngliche Datumszeile bleibt erhalten, sofern der neue Stempel eine hat.</summary>
    public static void UpdateStamp(string path, int page, int index, int objectNumber, Stamp stamp)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        var pdfPage = document.Pages[page - 1];
        index = ResolveIndex(pdfPage.Annotations, objectNumber, index);
        var old = AnnotationAt(pdfPage.Annotations, index);
        var oldLines = SplitLines(old.Elements.GetString("/Contents"));
        var oldRect = old.Elements.GetRectangle("/Rect");
        var oldTop = Math.Max(oldRect.Y1, oldRect.Y2);
        var (oldLeft, oldRight) = (Math.Min(oldRect.X1, oldRect.X2), Math.Max(oldRect.X1, oldRect.X2));
        var oldDate = oldLines.Length > 1 ? InitialsSuffix().Replace(oldLines[^1], string.Empty).Trim() : string.Empty; // Datum ohne das alte Kürzel
        var dateLine = stamp.SecondLine(oldDate.Length > 0 ? oldDate : Stamp.DateText(DateTime.Now));
        // Der Platz im Stapel bleibt nur, wenn der neue Stempel dieselbe Position hat wie der alte – erkennbar daran, dass der alte
        // Kasten in der Spalte und im Stapelbereich des neuen liegt. Sonst (z.B. oben rechts → oben links) gilt die feste Position
        // des neuen Stempels; ein Stempel oben links auf Stapelhöhe zwei wäre Unsinn (Fehlerbericht 19.09.2026).
        var planned = StampRect(pdfPage, stamp, dateLine);
        var sameColumn = oldRight > planned.X && oldLeft < planned.X + planned.Width;
        var sameStack = sameColumn && Math.Abs(oldTop - (planned.Y + planned.Height)) <= StackLimit * (planned.Height + StackGap);
        pdfPage.Annotations.Elements.RemoveAt(index);
        AppendStamp(document, pdfPage, stamp, dateLine, sameStack ? oldTop : null);
        SaveCompact(document, path);
    }

    private static void AppendStamp(PdfDocument document, PdfPage pdfPage, Stamp stamp, string dateLine, double? top = null)
    {
        var dateSize = stamp.FontSize * Stamp.DateFactor;
        var (width, height, textWidth, dateWidth) = MeasureStamp(stamp.Text, stamp.FontSize, dateLine);
        var rect = StampRect(pdfPage, stamp, dateLine, top);

        var fonts = new PdfDictionary(document);
        fonts.Elements.SetReference("/HeBo", StandardFont(document, "/Helvetica-Bold"));
        fonts.Elements.SetReference("/Helv", StandardFont(document, "/Helvetica"));
        var resources = new PdfDictionary(document);
        resources.Elements.SetObject("/Font", fonts);
        var graphicsState = new PdfDictionary(document); // Deckkraft für Füllung, Rahmen und Text (Extended Graphics State)
        graphicsState.Elements.SetName("/Type", "/ExtGState");
        graphicsState.Elements.SetReal("/CA", stamp.Alpha);
        graphicsState.Elements.SetReal("/ca", stamp.Alpha);
        var graphicsStates = new PdfDictionary(document);
        graphicsStates.Elements.SetObject("/GS0", graphicsState);
        resources.Elements.SetObject("/ExtGState", graphicsStates);

        var color = stamp.Color;
        var rgb = string.Create(CultureInfo.InvariantCulture, $"{color.R / 255.0:0.###} {color.G / 255.0:0.###} {color.B / 255.0:0.###}");
        var content = new StringBuilder("/GS0 gs q "); // die Deckkraft gilt vor dem ersten q, damit sie auch den Text nach Q erfasst
        var radius = stamp.CornerRadius;
        if (stamp.BackgroundColor is { } fill)
        {
            content.Append(CultureInfo.InvariantCulture, $"{fill.R / 255.0:0.###} {fill.G / 255.0:0.###} {fill.B / 255.0:0.###} rg ").Append(RectanglePath(0, 0, width, height, radius)).Append("f ");
        }
        if (stamp.Border)
        {
            var stroke = Math.Max(1, stamp.FontSize / 12); // Rahmenstärke wächst mit der Schrift
            content.Append(CultureInfo.InvariantCulture, $"{rgb} RG {stroke:0.##} w ").Append(RectanglePath(stroke / 2, stroke / 2, width - stroke, height - stroke, radius)).Append("S ");
        }
        content.Append("Q ");
        var padding = stamp.FontSize * Stamp.PaddingFactor;
        var textBaseline = height - padding - stamp.FontSize * 0.72; // Versalhöhe von Helvetica ≈ 0,72 em
        content.Append(CultureInfo.InvariantCulture, $"BT /HeBo {stamp.FontSize:0.##} Tf {rgb} rg {(width - textWidth) / 2:0.##} {textBaseline:0.##} Td (")
               .Append(Escape(stamp.Text)).Append(") Tj ET");
        if (dateLine.Length > 0)
        {
            var dateBaseline = padding + dateSize * 0.22; // Unterlänge ≈ 0,22 em
            content.Append(CultureInfo.InvariantCulture, $" BT /Helv {dateSize:0.##} Tf {rgb} rg {(width - dateWidth) / 2:0.##} {dateBaseline:0.##} Td (")
                   .Append(Escape(dateLine)).Append(") Tj ET");
        }
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
        annotation.Elements.SetName("/Subtype", "/Stamp");
        annotation.Elements.SetName("/Name", "/PDFlightStamp"); // kein Standardsymbol – Betrachter nehmen das Erscheinungsbild
        annotation.Elements.SetRectangle("/Rect", new PdfRectangle(rect));
        annotation.Elements.SetString("/Contents", dateLine.Length > 0 ? stamp.Text + "\n" + dateLine : stamp.Text);
        annotation.Elements.SetString(StampDataKey, JsonSerializer.Serialize(stamp)); // damit „Bearbeiten“ in der Anmerkungsliste alle Eigenschaften wiederfindet
        annotation.Elements.SetInteger("/F", 4); // drucken
        var colorArray = new PdfArray(document);
        colorArray.Elements.Add(new PdfReal(color.R / 255.0));
        colorArray.Elements.Add(new PdfReal(color.G / 255.0));
        colorArray.Elements.Add(new PdfReal(color.B / 255.0));
        annotation.Elements.SetObject("/C", colorArray);
        annotation.Elements.SetObject("/AP", appearances);
        annotation.Elements.SetDateTime("/M", DateTime.Now);
        document.Internals.AddObject(annotation);
        pdfPage.Annotations.Elements.Add(annotation.Reference!); // nach AddObject hat das Objekt eine Referenz
    }

    /// <summary>Weißer Marker (Wunsch vom 04.10.2026): Hervorhebungen, deren Darstellung die Farben unter dem Marker umkehrt – Mischmodus
    /// Differenz mit Weiß bei voller Deckkraft, heller Text auf dunklem Grund wird dunkel auf hellem. Im Modus Multiplizieren, in dem PDFium
    /// Hervorhebungen zeichnet, bliebe Weiß auf jedem Grund unsichtbar. Es bleibt eine gewöhnliche Highlight-Anmerkung (/QuadPoints, /C
    /// Weiß), die Radierer, „Hervorhebung entfernen“ und andere Programme erkennen; nur die Darstellung schreibt PDFlight selbst, weil
    /// PDFium keinen anderen Mischmodus einträgt. Vierecke je Seite (0-basiert) in PDF-Koordinaten aus <c>PageView.SelectionBoxesAsync</c>.</summary>
    public static void AddInvertHighlights(string path, IEnumerable<(int Page, List<(double Left, double Top, double Right, double Bottom)> Boxes)> pages)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        foreach (var (page, boxes) in pages)
        {
            if (page < 0 || page >= document.PageCount || boxes.Count == 0) { continue; }
            var pdfPage = document.Pages[page];
            var (left, bottom, right, top) = (boxes.Min(b => b.Left), boxes.Min(b => b.Bottom), boxes.Max(b => b.Right), boxes.Max(b => b.Top));
            var bounds = new PdfRectangle(new XRect(left, bottom, right - left, top - bottom));

            var graphicsState = new PdfDictionary(document);
            graphicsState.Elements.SetName("/Type", "/ExtGState");
            graphicsState.Elements.SetName("/BM", "/Difference");
            graphicsState.Elements.SetReal("/CA", 1);
            graphicsState.Elements.SetReal("/ca", 1);
            var graphicsStates = new PdfDictionary(document);
            graphicsStates.Elements.SetObject("/GS0", graphicsState);
            var resources = new PdfDictionary(document);
            resources.Elements.SetObject("/ExtGState", graphicsStates);
            // /BBox = /Rect: die Darstellung liegt ohne Umrechnung in Seitenkoordinaten
            var content = new StringBuilder("/GS0 gs 1 1 1 rg ");
            foreach (var (Left, Top, Right, Bottom) in boxes) { content.Append(CultureInfo.InvariantCulture, $"{Left:0.###} {Bottom:0.###} {Right - Left:0.###} {Top - Bottom:0.###} re "); }
            content.Append('f');
            var appearance = new PdfDictionary(document);
            appearance.Elements.SetName("/Type", "/XObject");
            appearance.Elements.SetName("/Subtype", "/Form");
            appearance.Elements.SetRectangle("/BBox", bounds);
            appearance.Elements.SetObject("/Resources", resources);
            appearance.CreateStream(ToWinAnsi(content.ToString()));
            document.Internals.AddObject(appearance);
            var appearances = new PdfDictionary(document);
            appearances.Elements.SetReference("/N", appearance);

            var quads = new PdfArray(document); // je Zeile links oben, rechts oben, links unten, rechts unten – wie PDFiums Hervorhebungen
            foreach (var (Left, Top, Right, Bottom) in boxes)
            {
                foreach (var value in new[] { Left, Top, Right, Top, Left, Bottom, Right, Bottom }) { quads.Elements.Add(new PdfReal(value)); }
            }
            var white = new PdfArray(document);
            for (var i = 0; i < 3; i++) { white.Elements.Add(new PdfReal(1)); }
            var annotation = new PdfDictionary(document);
            annotation.Elements.SetName("/Type", "/Annot");
            annotation.Elements.SetName("/Subtype", "/Highlight");
            annotation.Elements.SetRectangle("/Rect", bounds);
            annotation.Elements.SetObject("/QuadPoints", quads);
            annotation.Elements.SetObject("/C", white);
            annotation.Elements.SetReal("/CA", 1);
            annotation.Elements.SetInteger("/F", 4); // drucken
            annotation.Elements.SetObject("/AP", appearances);
            annotation.Elements.SetDateTime("/M", DateTime.Now);
            document.Internals.AddObject(annotation);
            pdfPage.Annotations.Elements.Add(annotation.Reference!); // nach AddObject hat das Objekt eine Referenz
        }
        SaveCompact(document, path);
    }

    /// <summary>Kastenmaß eines Stempels in Punkt (Arial steht für Helvetica): fetter Text, darunter die Datumszeile in kleinerer
    /// Schrift, plus Innenabstand relativ zur Schriftgröße.</summary>
    public static (double Width, double Height, double TextWidth, double DateWidth) MeasureStamp(string text, double fontSize, string dateLine)
    {
        using var measure = XGraphics.CreateMeasureContext(new XSize(1000, 1000), XGraphicsUnit.Point, XPageDirection.Downwards);
        var dateSize = dateLine.Length > 0 ? fontSize * Stamp.DateFactor : 0;
        var textWidth = Math.Max(measure.MeasureString(text, new XFont("Arial", fontSize, XFontStyleEx.Bold)).Width, fontSize);
        var dateWidth = dateSize > 0 ? measure.MeasureString(dateLine, new XFont("Arial", dateSize)).Width : 0;
        var padding = fontSize * Stamp.PaddingFactor;
        return (Math.Max(textWidth, dateWidth) + 2 * padding, fontSize * 1.0 + dateSize * 1.3 + 2 * padding, textWidth, dateWidth);
    }

    private static string Escape(string text) => text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    /// <summary>Eine der 14 Standardschriften (nichts einzubetten) mit WinAnsi-Kodierung, als eigenes Objekt im Dokument.</summary>
    private static PdfDictionary StandardFont(PdfDocument document, string baseFont)
    {
        var font = new PdfDictionary(document);
        font.Elements.SetName("/Type", "/Font");
        font.Elements.SetName("/Subtype", "/Type1");
        font.Elements.SetName("/BaseFont", baseFont);
        font.Elements.SetName("/Encoding", "/WinAnsiEncoding");
        document.Internals.AddObject(font);
        return font;
    }

    /// <summary>Pfad eines Rechtecks für den Inhaltsstrom, mit Radius als abgerundetes Rechteck aus vier Bézier-Bögen.</summary>
    private static string RectanglePath(double x, double y, double w, double h, double radius)
    {
        if (radius <= 0) { return string.Create(CultureInfo.InvariantCulture, $"{x:0.##} {y:0.##} {w:0.##} {h:0.##} re "); }
        var r = Math.Min(radius, Math.Min(w, h) / 2);
        var k = r * 0.5523; // Kreisbogen-Näherung
        var (x1, y1, x2, y2) = (x, y, x + w, y + h);
        return string.Create(CultureInfo.InvariantCulture,
            $"{x1 + r:0.##} {y1:0.##} m {x2 - r:0.##} {y1:0.##} l {x2 - r + k:0.##} {y1:0.##} {x2:0.##} {y1 + r - k:0.##} {x2:0.##} {y1 + r:0.##} c " +
            $"{x2:0.##} {y2 - r:0.##} l {x2:0.##} {y2 - r + k:0.##} {x2 - r + k:0.##} {y2:0.##} {x2 - r:0.##} {y2:0.##} c " +
            $"{x1 + r:0.##} {y2:0.##} l {x1 + r - k:0.##} {y2:0.##} {x1:0.##} {y2 - r + k:0.##} {x1:0.##} {y2 - r:0.##} c " +
            $"{x1:0.##} {y1 + r:0.##} l {x1:0.##} {y1 + r - k:0.##} {x1 + r - k:0.##} {y1:0.##} {x1 + r:0.##} {y1:0.##} c h ");
    }

    private static void AppendFreeText(PdfDocument document, PdfPage pdfPage, string text, double leftMm, double topMm, double fontSize, AnnotationStyle style)
    {
        var lines = SplitLines(text);
        var leading = fontSize * LeadingFactor;
        var (width, height) = MeasureAnnotation(text, fontSize);
        var left = leftMm * 72 / 25.4;
        var top = pdfPage.Height.Point - topMm * 72 / 25.4;

        var fonts = new PdfDictionary(document);
        fonts.Elements.SetReference("/Helv", StandardFont(document, "/Helvetica"));
        var resources = new PdfDictionary(document);
        resources.Elements.SetObject("/Font", fonts);

        var content = new StringBuilder("q ");
        if (style.Background is { } fill)
        {
            content.Append(CultureInfo.InvariantCulture, $"{fill.R / 255.0:0.###} {fill.G / 255.0:0.###} {fill.B / 255.0:0.###} rg 0 0 {width:0.##} {height:0.##} re f ");
        }
        if (style.BorderColor is { } stroke)
        {
            content.Append(CultureInfo.InvariantCulture, $"{stroke.R / 255.0:0.###} {stroke.G / 255.0:0.###} {stroke.B / 255.0:0.###} RG 0.5 w 0.25 0.25 {width - 0.5:0.##} {height - 0.5:0.##} re S ");
        }
        content.Append("Q ");
        var textColor = style.TextColor;
        // Grundlinien so, dass der Text optisch mittig im Kasten steht: von der Oberkante der ersten Zeile (Versalien/Oberlängen oder nur
        // x-Höhe) bis zur Unterkante der letzten (mit oder ohne Unterlängen). Vorher pauschal 0,8 em unter dem Innenrand – unten blieb
        // sichtbar mehr Platz (Hinweis Wilhelms vom 04.10.2026); der Kasten selbst bleibt so groß wie bisher.
        var block = fontSize * (TopExtent(lines[0]) + (lines.Length - 1) * LeadingFactor + BottomExtent(lines[^1]));
        var lastBaseline = (height - block) / 2 + fontSize * BottomExtent(lines[^1]);
        var firstBaseline = lastBaseline + (lines.Length - 1) * leading;
        content.Append(CultureInfo.InvariantCulture, $"BT /Helv {fontSize:0.##} Tf {textColor.R / 255.0:0.###} {textColor.G / 255.0:0.###} {textColor.B / 255.0:0.###} rg {leading:0.##} TL {Padding:0.##} {firstBaseline:0.##} Td ");
        foreach (var line in lines)
        {
            content.Append('(').Append(Escape(line)).Append(") Tj T* ");
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
        borderStyle.Elements.SetInteger("/W", style.BorderColor != null ? 1 : 0);
        annotation.Elements.SetObject("/BS", borderStyle);
        annotation.Elements.SetObject("/AP", appearances);
        annotation.Elements.SetDateTime("/M", DateTime.Now);
        document.Internals.AddObject(annotation);
        pdfPage.Annotations.Elements.Add(annotation.Reference!); // nach AddObject hat das Objekt eine Referenz
    }

    public const double Padding = 4;           // Innenabstand des Anmerkungskastens (Punkt) – die Vorschau zeichnet damit
    public const double LeadingFactor = 1.25;  // Zeilenabstand relativ zur Schriftgröße
    private const double CapHeight = 0.718;    // Helvetica (AFM): Versalhöhe, …
    private const double XHeight = 0.523;      // … x-Höhe …
    private const double Descender = 0.207;    // … und Unterlänge relativ zur Schriftgröße

    /// <summary>Wie hoch eine Zeile über die Grundlinie reicht: nur x-Höhe, wenn sie ausschließlich aus Kleinbuchstaben ohne Oberlänge,
    /// Punkt oder Akzent besteht (a, c, e, m …), sonst Versalhöhe – Oberlängen, i-Punkte und Umlaute liegen etwa dort.</summary>
    private static double TopExtent(string line) =>
        line.Trim().Length > 0 && line.All(c => char.IsWhiteSpace(c) || "acegmnopqrsuvwxyz.,:;-_~".Contains(c)) ? XHeight : CapHeight;

    /// <summary>Wie tief eine Zeile unter die Grundlinie reicht: Unterlänge bei g, j, p, q, y, Q, Klammern und Satzzeichen mit Unterlänge.</summary>
    private static double BottomExtent(string line) => line.Any(c => "gjpqyQç(),;[]{}|_@µ".Contains(c)) ? Descender : 0;

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

    /// <summary>Hängt mehrere PDF-Dateien in der angegebenen Reihenfolge an – ein Öffnen und Speichern, also auch eine einzige
    /// Rückgängig-Sicherung; liefert die neue Gesamtseitenzahl.</summary>
    public static int AppendPdfs(string path, IReadOnlyList<string> otherPdfs)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        foreach (var otherPdf in otherPdfs)
        {
            using var other = PdfReader.Open(otherPdf, PdfDocumentOpenMode.Import);
            foreach (var page in other.Pages) { document.AddPage(page); }
        }
        var pageCount = document.PageCount; // muss vor Save() gelesen werden — danach ist das Dokument gesperrt
        SaveCompact(document, path);
        return pageCount;
    }

    /// <summary>Speichert die angegebenen Seiten (1-basiert) als neue Datei.</summary>
    public static void ExtractPages(string sourcePath, string destinationPath, IReadOnlyList<int> pages)
    {
        using var source = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import);
        using PdfDocument destination = new();
        foreach (var page in pages) { destination.AddPage(source.Pages[page - 1]); }
        SaveCompact(destination, destinationPath);
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
        SaveCompact(target, path);
    }

    private static void CopyInfo(PdfDocument source, PdfDocument target)
    {
        target.Info.Title = source.Info.Title;
        target.Info.Author = source.Info.Author;
        target.Info.Subject = source.Info.Subject;
        target.Info.Keywords = source.Info.Keywords;
    }

    public static PdfInfo ReadInfo(string path, string? password = null)
    {
        // Import = lesender Zugriff (ReadOnly ist in PDFsharp 6 nicht implementiert); das Kennwort für Dateien mit Kennwort zum Öffnen
        using var document = string.IsNullOrEmpty(password)
            ? PdfReader.Open(path, PdfDocumentOpenMode.Import)
            : PdfReader.Open(path, password, PdfDocumentOpenMode.Import);
        var v = document.Version; // z.B. 14 → "1.4"
        static DateTime? Date(DateTime value) => value == DateTime.MinValue ? null : value; // PDFsharp: MinValue = nicht angegeben
        var catalog = document.Internals.Catalog;
        var tagged = catalog.Elements.ContainsKey("/StructTreeRoot") || catalog.Elements.GetDictionary("/MarkInfo")?.Elements.GetBoolean("/Marked") == true;
        var (width, height) = (0.0, 0.0);
        if (document.PageCount > 0)
        {
            var first = document.Pages[0];
            var box = first.MediaBox;
            (width, height) = first.Rotate % 180 == 0 ? (box.Width, box.Height) : (box.Height, box.Width);
        }
        IReadOnlyList<FontInfo> fonts;
        try { fonts = CollectFonts(document); }
        catch (Exception ex) when (IsPdfReadError(ex)) { fonts = []; } // ein kaputter Schrifteintrag soll den Dialog nicht verhindern
        return new PdfInfo(document.Info.Title, document.Info.Author, document.Info.Subject, document.Info.Keywords,
            document.PageCount, $"{v / 10}.{v % 10}", document.Info.Creator, document.Info.Producer,
            Date(document.Info.CreationDate), Date(document.Info.ModificationDate), tagged, width, height, fonts);
    }

    /// <summary>Die Schriften aus den Ressourcen der Seiten (auch geerbte und die von Formular-XObjects) – nur die Schrift-Wörterbücher, die
    /// Seiteninhalte werden nicht zerlegt; so bleibt es auch bei großen Dateien schnell. Je Name, Typ, Kodierung und Einbettung ein Eintrag.</summary>
    private static List<FontInfo> CollectFonts(PdfDocument document)
    {
        var fonts = new Dictionary<string, FontInfo>(StringComparer.Ordinal);
        var visited = new HashSet<PdfDictionary>(ReferenceEqualityComparer.Instance);
        void Resources(PdfDictionary? resources, int depth)
        {
            if (resources == null || depth > 8 || !visited.Add(resources)) { return; }
            if (resources.Elements.GetDictionary("/Font") is { } fontDictionary)
            {
                foreach (var key in fontDictionary.Elements.Keys)
                {
                    if (fontDictionary.Elements.GetDictionary(key) is { } font && visited.Add(font) && Describe(font) is { } info)
                    {
                        fonts.TryAdd($"{info.Name}|{info.Type}|{info.Encoding}|{info.Embedded}|{info.Subset}", info);
                    }
                }
            }
            if (resources.Elements.GetDictionary("/XObject") is { } xObjects)
            {
                foreach (var key in xObjects.Elements.Keys)
                {
                    if (xObjects.Elements.GetDictionary(key) is { } xObject && xObject.Elements.GetName("/Subtype") == "/Form") { Resources(xObject.Elements.GetDictionary("/Resources"), depth + 1); }
                }
            }
        }
        foreach (var page in document.Pages)
        {
            // /Resources darf von einem /Pages-Knoten geerbt sein
            PdfDictionary? node = page;
            for (var level = 0; node != null && level < 32; level++)
            {
                if (node.Elements.GetDictionary("/Resources") is { } resources) { Resources(resources, 0); break; }
                node = node.Elements.GetDictionary("/Parent");
            }
        }
        return [.. fonts.Values.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)];
    }

    private static FontInfo? Describe(PdfDictionary font)
    {
        var subtype = font.Elements.GetName("/Subtype");
        var baseFont = font.Elements.GetName("/BaseFont").TrimStart('/');
        var descriptor = font.Elements.GetDictionary("/FontDescriptor");
        string type;
        if (subtype == "/Type0")
        {
            var descendant = font.Elements.GetArray("/DescendantFonts")?.Elements is { Count: > 0 } descendants ? Resolve(descendants[0]) as PdfDictionary : null;
            type = descendant?.Elements.GetName("/Subtype") == "/CIDFontType2" ? "TrueType (CID)" : "Type 1 (CID)";
            descriptor = descendant?.Elements.GetDictionary("/FontDescriptor");
        }
        else
        {
            type = subtype switch { "/TrueType" => "TrueType", "/Type1" => "Type 1", "/MMType1" => "Type 1 (Multiple Master)", "/Type3" => "Type 3", _ => subtype.TrimStart('/') };
        }
        if (descriptor?.Elements.GetDictionary("/FontFile3")?.Elements.GetName("/Subtype") == "/OpenType") { type = "OpenType"; }
        var embedded = subtype == "/Type3" || descriptor != null && (descriptor.Elements.ContainsKey("/FontFile") || descriptor.Elements.ContainsKey("/FontFile2") || descriptor.Elements.ContainsKey("/FontFile3"));
        var subset = baseFont.Length > 7 && baseFont[6] == '+' && baseFont[..6].All(char.IsAsciiLetterUpper); // „ABCDEF+Arial“
        var name = subset ? baseFont[7..] : baseFont;
        if (name.Length == 0) { name = subtype == "/Type3" ? "Type 3" : "?"; }
        var encoding = Resolve(font.Elements["/Encoding"]) switch
        {
            PdfName { Value: "/WinAnsiEncoding" } => "Ansi",
            PdfName { Value: "/MacRomanEncoding" } => "Roman",
            PdfName { Value: "/StandardEncoding" } => "Standard",
            PdfName other => other.Value.TrimStart('/'),
            PdfDictionary => "*", // eigene Kodierung (Differences) – der Dialog übersetzt
            _ => string.Empty,    // eingebaute Kodierung der Schrift – der Dialog übersetzt
        };
        return new FontInfo(name, embedded, subset, type, encoding);
    }

    private static PdfItem? Resolve(PdfItem? item) => item is PdfReference reference ? reference.Value : item;

    /// <summary>Speichert und lässt dabei nichts Unerreichbares zurück. PDFsharp verwirft beim Speichern alle Objekte, auf die nichts mehr
    /// verweist, ersetzt die XMP-Metadaten des Katalogs aber erst danach durch eigene – der alte XMP-Strom blieb so bei jeder Bearbeitung als
    /// Waise in der Datei (geprüft 04.10.2026). Ohne Verweis darauf fällt er gleich mit weg; die neuen XMP-Daten schreibt PDFsharp trotzdem.</summary>
    private static void SaveCompact(PdfDocument document, string path)
    {
        document.Internals.Catalog.Elements.Remove("/Metadata");
        document.Save(path);
    }

    /// <summary>„OK“ im Eigenschaften-Dialog: Metadaten und Kennwortschutz in einem Durchgang schreiben (ein Rückgängig-Schritt).
    /// <paramref name="decrypted"/> ist bei verschlüsselten Dateien die von PDFium entschlüsselte Fassung (PDFsharp kann verschlüsselte
    /// Dateien nicht ändern), sonst null = die Datei selbst. Ändert sich weder an den Metadaten noch am Kennwort etwas – Schutz oder
    /// Einschränkungen nur aufheben –, kommt PDFiums Fassung unverändert und damit verlustfrei in die Datei. Verschlüsselt wird im
    /// Modify-Modus: Lesezeichen und Formularfelder bleiben (bis 06.10.2026 baute „Kennwort vergeben“ die Datei aus ihren Seiten neu auf
    /// und verlor beides).</summary>
    public static void WriteProperties(string path, byte[]? decrypted, PropertyChanges changes)
    {
        var bytes = decrypted ?? File.ReadAllBytes(path);
        if (changes.Info == null && !changes.RemoveMetadata && changes.Password == null)
        {
            File.WriteAllBytes(path, bytes);
            return;
        }
        using MemoryStream stream = new(bytes);
        using var document = PdfReader.Open(stream, PdfDocumentOpenMode.Modify);
        if (changes.RemoveMetadata) { ClearMetadata(document); }
        if (changes.Info is { } info)
        {
            // nach dem Leeren nur, was eingetippt ist – sonst stünden leere Einträge in der Datei
            if (!changes.RemoveMetadata || info.Title.Length > 0) { document.Info.Title = info.Title; }
            if (!changes.RemoveMetadata || info.Author.Length > 0) { document.Info.Author = info.Author; }
            if (!changes.RemoveMetadata || info.Subject.Length > 0) { document.Info.Subject = info.Subject; }
            if (!changes.RemoveMetadata || info.Keywords.Length > 0) { document.Info.Keywords = info.Keywords; }
        }
        if (changes.Password != null)
        {
            document.SecuritySettings.UserPassword = changes.Password;
            document.SecurityHandler.SetEncryptionToV5(); // AES-256, PDF 2.0; ohne eigenes Berechtigungskennwort – alles erlaubt
        }
        SaveCompact(document, path);
    }

    /// <summary>Entfernt alle Metadaten: das gesamte Info-Wörterbuch (Titel, Autor, Betreff, Stichwörter, Anwendung, Produzent,
    /// Datumsangaben und private Einträge) sowie die XMP-Metadaten von Katalog und Seiten. PDFsharp schreibt beim Speichern eigene
    /// XMP-Daten samt Produzent – die nennen dann nur noch PDFsharp und das Speicherdatum, nichts aus der Herkunft der Datei.</summary>
    private static void ClearMetadata(PdfDocument document)
    {
        foreach (var key in document.Info.Elements.Keys.ToList()) { document.Info.Elements.Remove(key); }
        StripXmp(document.Internals.Catalog);
        foreach (var page in document.Pages) { StripXmp(page); }
    }

    /// <summary>XMP-Strom eines Wörterbuchs entfernen. PDFsharp schreibt auch nicht mehr referenzierte Objekte in die Datei,
    /// deshalb wird der Strom zusätzlich geleert – sonst stünde die alte Herkunft weiter lesbar als verwaistes Objekt darin.</summary>
    private static void StripXmp(PdfDictionary owner)
    {
        if (owner.Elements.GetDictionary("/Metadata") is { Stream: { } stream } xmp)
        {
            stream.Value = [];
            xmp.Elements.Remove("/Filter"); // der leere Inhalt ist nicht mehr komprimiert; /Length setzt PDFsharp beim Schreiben
        }
        owner.Elements.Remove("/Metadata");
    }

    /// <summary>Fügt eine leere Seite ein – vor oder nach der angezeigten Seite (1-basiert), am Anfang oder am Ende. Format „wie die Seite“:
    /// eine gedrehte Seite ergibt eine ungedrehte mit vertauschten Maßen, damit die Anzeige gleich aussieht und ein Bild aufrecht steht.
    /// Ein Bild steht zentriert, das Seitenverhältnis bleibt (s. <see cref="NewPageOptions"/>). Liefert die neue Seitennummer.</summary>
    public static int InsertBlankPage(string path, int page, NewPageOptions options)
    {
        using var image = options.ImagePath == null ? null : LoadImage(options.ImagePath); // vor dem Öffnen: ein unlesbares Bild bricht ab, bevor etwas geändert ist
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        var reference = document.Pages[page - 1];
        var (referenceWidth, referenceHeight) = reference.Rotate % 180 != 0 ? (reference.Height.Point, reference.Width.Point) : (reference.Width.Point, reference.Height.Point);
        var layout = LayoutNewPage(options, referenceWidth, referenceHeight, image?.PixelWidth ?? 0, image?.PixelHeight ?? 0);
        var index = options.Position switch
        {
            InsertPosition.Before => page - 1,
            InsertPosition.First => 0,
            InsertPosition.Last => document.PageCount,
            _ => page,
        };
        var newPage = document.Pages.Insert(index);
        newPage.Width = XUnit.FromPoint(layout.Width);
        newPage.Height = XUnit.FromPoint(layout.Height);
        newPage.Rotate = 0;
        if (image != null && layout.Image is { } rect)
        {
            using var gfx = XGraphics.FromPdfPage(newPage);
            gfx.DrawImage(image, rect);
        }
        SaveCompact(document, path);
        return index + 1;
    }

    /// <summary>Maße der neuen Seite und Lage des Bildes darauf (Punkt, Ursprung links oben) – dieselbe Rechnung für das Einfügen und die
    /// Vorschau im Dialog, damit beide übereinstimmen. <paramref name="referenceWidth"/>/<paramref name="referenceHeight"/>: sichtbare Maße
    /// der angezeigten Seite; ohne Bild (Pixel 0) bleibt <see cref="NewPageLayout.Image"/> leer. Scale = Faktor gegenüber der Größe bei
    /// der gewählten Auflösung (&lt; 1: verkleinert).</summary>
    public static NewPageLayout LayoutNewPage(NewPageOptions options, double referenceWidth, double referenceHeight, int imagePixelWidth, int imagePixelHeight)
    {
        var hasImage = imagePixelWidth > 0 && imagePixelHeight > 0;
        var dpi = options.Dpi > 0 ? options.Dpi : 72; // beim Einpassen spielt die Auflösung keine Rolle, das Feld ist dann ausgegraut
        var (imageWidth, imageHeight) = (imagePixelWidth * 72 / dpi, imagePixelHeight * 72 / dpi); // Druckgröße bei der gewählten Auflösung, nicht der Datei-Angabe
        var (width, height) = options.Format switch
        {
            NewPageFormat.A4 => PaperFormat.A4,
            NewPageFormat.Letter => PaperFormat.Letter,
            NewPageFormat.FromImage when hasImage => LimitPageSize((imageWidth, imageHeight)),
            _ => (referenceWidth, referenceHeight),
        };
        if (options.Landscape && options.Format is NewPageFormat.A4 or NewPageFormat.Letter) { (width, height) = (height, width); }
        if (!hasImage) { return new NewPageLayout(width, height, null, 1); }
        if (options.Format == NewPageFormat.FromImage) { return new NewPageLayout(width, height, new XRect(0, 0, width, height), width / imageWidth); }
        var box = new XRect(PageMarginPt, PageMarginPt, width - 2 * PageMarginPt, height - 2 * PageMarginPt);
        var scale = Math.Min(box.Width / imageWidth, box.Height / imageHeight);
        if (!options.FitImage) { scale = Math.Min(scale, 1); } // Originalgröße: nur verkleinern, wenn es nicht passt
        var rect = new XRect(box.X + (box.Width - imageWidth * scale) / 2, box.Y + (box.Height - imageHeight * scale) / 2, imageWidth * scale, imageHeight * scale);
        return new NewPageLayout(width, height, rect, scale);
    }

    /// <summary>Rand für eingepasste Bilder in Punkt (10 mm) – auch für die gestrichelte Linie in der Vorschau.</summary>
    public static double ImageMarginPt => PageMarginPt;

    /// <summary>PDF-Seiten dürfen höchstens 14 400 pt (200 Zoll) messen (ohne /UserUnit) – größere proportional verkleinern.</summary>
    private static (double Width, double Height) LimitPageSize((double Width, double Height) size)
    {
        var factor = Math.Min(1, MaxPageSizePt / Math.Max(size.Width, size.Height));
        return (size.Width * factor, size.Height * factor);
    }

    private const double MaxPageSizePt = 14400;

    /// <summary>Sichtbare Maße einer Seite (1-basiert) in Punkt – bei gedrehter Seite vertauscht; (0, 0), wenn sie sich nicht lesen lässt.</summary>
    public static (double Width, double Height) PageSizePt(string path, int page)
    {
        try
        {
            using var document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            var p = document.Pages[Math.Clamp(page, 1, document.PageCount) - 1];
            return p.Rotate % 180 != 0 ? (p.Height.Point, p.Width.Point) : (p.Width.Point, p.Height.Point);
        }
        catch (Exception ex) when (IsPdfReadError(ex)) { return (0, 0); }
    }

    /// <summary>Bild für eine neue Seite laden. GDI+ meldet unlesbare oder unbekannte Bilddaten als OutOfMemoryException – die
    /// wird hier zu einer normalen Fehlermeldung, sonst käme sie ungefangen bis zum Absturz durch.</summary>
    private static XImage LoadImage(string path)
    {
        try { return XImage.FromFile(path); }
        catch (Exception ex) when (ex is OutOfMemoryException or System.Runtime.InteropServices.ExternalException)
        {
            throw new InvalidOperationException(Lng.T("Die Bilddatei konnte nicht gelesen werden.") + " " + path, ex);
        }
    }

    /// <summary>Verschiebt eine Seite an eine andere Position (beide 1-basiert); die übrigen Seiten rücken auf.</summary>
    public static void MovePage(string path, int page, int targetPage)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        document.Pages.MovePage(page - 1, targetPage - 1);
        SaveCompact(document, path);
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
