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

internal record PdfInfo(string Title, string Author, string Subject, string Keywords, int PageCount, string Version, string Creator, string Producer);

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

/// <param name="Encrypted">Die Datei ist verschlüsselt, lässt sich aber ohne Kennwort lesen (nur Besitzerkennwort): PDFsharp öffnet sie zum
/// Bearbeiten nicht – jede Änderung braucht vorher „Kennwort entfernen“ (Fehlerbericht 20.09.2026).</param>
internal record PdfStatus(int PageCount, string? Version, string? PdfALevel, double PageWidthPt = 0, double PageHeightPt = 0, int AnnotationCount = 0, int OutlineCount = 0, bool Encrypted = false);

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
            return new PdfStatus(document.PageCount, $"{v / 10}.{v % 10}", GetPdfALevel(document), first?.Width.Point ?? 0, first?.Height.Point ?? 0, Guarded(() => CountAnnotations(document)), Guarded(() => CountOutlines(document)),
                Guarded(() => document.SecurityHandler.Elements.ContainsKey("/Filter") ? 1 : 0) == 1); // SecuritySettings.IsEncrypted liefert im Import-Lauf false (geprüft 20.09.2026) – der geladene Handler trägt dagegen das /Encrypt-Wörterbuch
        }
        catch (Exception ex) when (IsPdfReadError(ex)) { return new PdfStatus(-1, null, null); }
    }

    /// <summary>Ein Zähler für die Statuszeile; bei einem Lesefehler in dem Teil 0 statt Abbruch – die Seitenzahl und damit die
    /// Bearbeitung bleiben verfügbar (ein Canva-Export mit ungültigem /Annots-Eintrag ließ sonst die ganze Datei als unlesbar gelten, 19.09.2026).</summary>
    private static int Guarded(Func<int> count)
    {
        try { return count(); }
        catch (Exception ex) when (IsPdfReadError(ex)) { return 0; }
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
        var removedPages = pages.Select(p => document.Pages[p - 1].Reference?.ObjectID).OfType<PdfObjectID>().ToHashSet();
        foreach (var page in pages.OrderByDescending(p => p)) { document.Pages.RemoveAt(page - 1); }
        PruneOutlines(document, removedPages);
        document.Save(path);
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
        var removed = AnnotationAt(annotations, index); // VOR dem Entfernen greifen – danach zeigt der Index ins Leere (IDE-Umformungen haben das schon einmal vertauscht)
        annotations.Elements.RemoveAt(index);
        if (removed.Elements["/Popup"] is PdfReference popup)
        {
            for (var i = annotations.Elements.Count - 1; i >= 0; i--)
            {
                if (annotations.Elements[i] is PdfReference reference && reference.ObjectID == popup.ObjectID) { annotations.Elements.RemoveAt(i); }
            }
        }
        document.Save(path);
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
        if (count > 0) { document.Save(path); }
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

    /// <summary>Zahl der Lesezeichen (Gliederung) samt Unterebenen – schaltet „Lesezeichen entfernen“ frei.</summary>
    private static int CountOutlines(PdfDocument document)
    {
        try { return CountOutlines(document.Outlines); }
        catch (Exception ex) when (IsPdfReadError(ex)) { return 0; } // kaputte Gliederung: dann eben keine
    }

    private static int CountOutlines(PdfOutlineCollection outlines) => outlines.Count + outlines.Sum(o => CountOutlines(o.Outlines));

    /// <summary>Entfernt die komplette Gliederung (Lesezeichen) der Datei und schaltet den Seitenmodus auf „ohne Leiste“,
    /// sonst zeigt der Viewer weiter eine leere Lesezeichenleiste. Die Seiten bleiben unberührt.</summary>
    public static void RemoveOutlines(string path)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        RemoveOutlines(document);
        document.Save(path);
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
            document.Save(path);
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
        document.Save(path);

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

    private static int CountAnnotations(PdfDocument document)
    {
        var count = 0;
        for (var p = 0; p < document.PageCount; p++)
        {
            foreach (var (_, annotation) in AnnotationDictionaries(document.Pages[p].Annotations))
            {
                if (IsManageable(annotation.Elements.GetName("/Subtype").TrimStart('/'))) { count++; }
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
        document.Save(path);
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
        document.Save(path);
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
        content.Append(CultureInfo.InvariantCulture, $"BT /Helv {fontSize:0.##} Tf {textColor.R / 255.0:0.###} {textColor.G / 255.0:0.###} {textColor.B / 255.0:0.###} rg {leading:0.##} TL {Padding:0.##} {height - Padding - fontSize * 0.8:0.##} Td ");
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

    /// <summary>Speichert die Datei ohne Kennwortschutz neu. Mit dem Besitzerkennwort öffnet PDFsharp zum Ändern, dann wird nur die
    /// Verschlüsselung abgeschaltet und alles bleibt (Lesezeichen, Formulare, Metadaten). Ist das Kennwort bloß das Benutzerkennwort,
    /// verweigert PDFsharp das Ändern – dann Rückfall auf den Neuaufbau (<see cref="Rebuild"/>). Geprüft 21.09.2026.</summary>
    public static void RemovePassword(string path, string password)
    {
        var bytes = File.ReadAllBytes(path); // Quelle in den Speicher, damit dieselbe Datei überschrieben werden kann
        try
        {
            using MemoryStream stream = new(bytes);
            using var document = PdfReader.Open(stream, password, PdfDocumentOpenMode.Modify);
            document.SecurityHandler.SetEncryptionToNoneAndResetPasswords();
            document.Save(path);
        }
        catch (PdfReaderException) { Rebuild(bytes, path, password); } // „owner password required“: nur Benutzerrechte
    }

    /// <summary>Nur mit Besitzerkennwort geschützte, ohne Kennwort lesbare Datei (Berechtigungen eingeschränkt) ohne das Kennwort
    /// entsperren: Neuaufbau aus den Seiten wie beim Drucken „Als PDF speichern“, nur ohne Rendering. Lesezeichen und Formularfelder
    /// hängen am Katalog und gehen verloren, Anmerkungen und Metadaten bleiben (Wunsch vom 21.09.2026).</summary>
    public static void RemoveRestrictions(string path) => Rebuild(File.ReadAllBytes(path), path, string.Empty);

    /// <summary>Seiten und Metadaten in ein neues, unverschlüsseltes Dokument übernehmen – unabhängig vom Verfahren der Quelle.</summary>
    private static void Rebuild(byte[] bytes, string path, string password)
    {
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

    /// <summary>Entfernt alle Metadaten: das gesamte Info-Wörterbuch (Titel, Autor, Betreff, Stichwörter, Anwendung, Produzent,
    /// Datumsangaben und private Einträge) sowie die XMP-Metadaten von Katalog und Seiten; anschließend werden die vier
    /// sichtbaren Felder neu gesetzt, sofern nicht leer. PDFsharp schreibt beim Speichern eigene XMP-Daten samt Produzent –
    /// die nennen dann nur noch PDFsharp und das Speicherdatum, nichts aus der Herkunft der Datei.</summary>
    public static void RemoveMetadata(string path, string title, string author, string subject, string keywords)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        foreach (var key in document.Info.Elements.Keys.ToList()) { document.Info.Elements.Remove(key); }
        StripXmp(document.Internals.Catalog);
        foreach (var page in document.Pages) { StripXmp(page); }
        if (title.Length > 0) { document.Info.Title = title; }
        if (author.Length > 0) { document.Info.Author = author; }
        if (subject.Length > 0) { document.Info.Subject = subject; }
        if (keywords.Length > 0) { document.Info.Keywords = keywords; }
        document.Save(path);
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

    /// <summary>Fügt eine leere Seite vor oder nach der Seite ein (1-basiert), im Format dieser Nachbarseite; eine gedrehte
    /// Nachbarseite ergibt eine ungedrehte Seite mit vertauschten Maßen, damit die Anzeige gleich aussieht und ein Bild
    /// aufrecht steht. Ein Bild wird mit 10 mm Rand seitenfüllend eingepasst (Seitenverhältnis bleibt). Liefert die neue Seitennummer.</summary>
    public static int InsertBlankPage(string path, int page, bool after, string? imagePath)
    {
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
        var neighbour = document.Pages[page - 1];
        var rotated = neighbour.Rotate % 180 != 0;
        var newPage = document.Pages.Insert(after ? page : page - 1);
        newPage.Width = rotated ? neighbour.Height : neighbour.Width;
        newPage.Height = rotated ? neighbour.Width : neighbour.Height;
        newPage.Rotate = 0;
        if (imagePath != null)
        {
            using var image = LoadImage(imagePath);
            using var gfx = XGraphics.FromPdfPage(newPage);
                var box = new XRect(PageMarginPt, PageMarginPt, newPage.Width.Point - 2 * PageMarginPt, newPage.Height.Point - 2 * PageMarginPt);
            var scale = Math.Min(box.Width / image.PointWidth, box.Height / image.PointHeight);
            var width = image.PointWidth * scale;
            var height = image.PointHeight * scale;
            gfx.DrawImage(image, box.X + (box.Width - width) / 2, box.Y + (box.Height - height) / 2, width, height);
        }
        document.Save(path);
        return after ? page + 1 : page;
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
