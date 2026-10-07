using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PDFLight.Viewer;

/// <summary>Ein PDF-Dokument – geöffnet im Hilfsprogramm (PDFium im AppContainer, s. <see cref="SandboxClient"/>). Diese Klasse ist
/// nur der Stellvertreter: Jeder Aufruf geht als Befehl über die Pipe, PDFium selbst läuft nie in diesem Prozess. Die Datei wird
/// hier gelesen und als Bytes übergeben – das Hilfsprogramm darf keine Dateien öffnen.
/// Fehlerverhalten: Abfragen der Ansicht (Kacheln, Text, Links …) liefern nach einem Absturz des Hilfsprogramms leere Ergebnisse;
/// Öffnen, Speichern, Druckkopie und EMF werfen InvalidOperationException.
/// Koordinaten: „Seitenpixel“ meint die Seite als Bild der Größe pageWidth × pageHeight (Ursprung links oben); die Umrechnung in
/// PDF-Punkte übernimmt PDFium im Hilfsprogramm.</summary>
internal sealed class PdfiumDocument : IDisposable
{
    private const double HitTolerancePt = 4; // Toleranz für „Zeichen unter der Maus“ in Punkt
    private readonly int id, generation;     // Nummer im Hilfsprogramm und dessen Startnummer

    public string Path { get; }
    public int PageCount => PageSizes.Length;

    /// <summary>Seitenmaße in Punkt, wie die Ansicht sie zeigt (/Rotate der Seite berücksichtigt).</summary>
    public SizeF[] PageSizes { get; }

    /// <summary>Die Bytes der Datei, wie sie geöffnet wurde (etwa um eine extern verschwundene Datei wiederherzustellen).</summary>
    public byte[] Bytes { get; set; } = [];


    /// <summary>Dauer von Übergabe und Laden im Hilfsprogramm samt Abfrage aller Seitenmaße.</summary>
    public TimeSpan LoadTime { get; private set; }

    /// <summary>Das Dokument hat ein Formular (AcroForm).</summary>
    public bool HasForms { get; }

    /// <summary>Angezeigte Seite – PDFium fragt bei Formular-Eingaben danach. Setzt die Ansicht.</summary>
    public int CurrentPageIndex { get; set; }

    /// <summary>Ein Bereich einer Seite muss neu gezeichnet werden (Sandbox-Thread).</summary>
    public event EventHandler<FormInvalidatedEventArgs>? FormInvalidated;

    /// <summary>Das Dokument wurde geändert – Formularwert, Hervorhebung (Sandbox-Thread).</summary>
    public event EventHandler? Changed;

    private PdfiumDocument(string path, int id, int generation, bool hasForms, SizeF[] pageSizes)
    {
        Path = path;
        this.id = id;
        this.generation = generation;
        HasForms = hasForms;
        PageSizes = pageSizes;
    }


    /// <summary>Liest die Datei und öffnet sie im Hilfsprogramm. Wirft IOException (Datei), <see cref="PasswordRequiredException"/>
    /// (Kennwort nötig oder falsch – mit Kennwort erneut aufrufen) bzw. InvalidOperationException (PDFium-Fehler, Hilfsprogramm nicht
    /// startbar oder abgestürzt).</summary>
    public static async Task<PdfiumDocument> OpenAsync(string path, string? password = null) =>
        await OpenAsync(await File.ReadAllBytesAsync(path), path, password);

    /// <summary>Öffnet ein Dokument aus Bytes (etwa eine Vorschau, die nur im Speicher liegt); <paramref name="path"/> dient nur der
    /// Anzeige.</summary>
    public static async Task<PdfiumDocument> OpenAsync(byte[] bytes, string path, string? password = null)
    {
        var document = await SandboxClient.RunAsync(p =>
        {
            var watch = Stopwatch.StartNew();
            var opened = p.Call(Protocol.Open, w => { w.Write(bytes.Length); w.Write(bytes); w.Write(password ?? string.Empty); }, ReadOpened);
            if (opened.Id == Protocol.PasswordNeeded) { throw new PasswordRequiredException(wrongPassword: !string.IsNullOrEmpty(password)); }
            var document = Register(path, opened);
            document.LoadTime = watch.Elapsed;
            return document;
        });
        document.Bytes = bytes;
        return document;
    }

    private static (int Id, bool HasForms, SizeF[] Sizes) ReadOpened(BinaryReader reader)
    {
        var id = reader.ReadInt32();
        if (id == Protocol.PasswordNeeded) { return (id, false, []); }
        var hasForms = reader.ReadBoolean();
        var sizes = new SizeF[reader.ReadInt32()];
        for (var i = 0; i < sizes.Length; i++) { sizes[i] = new SizeF(reader.ReadSingle(), reader.ReadSingle()); }
        return (id, hasForms, sizes);
    }

    /// <summary>Legt den Stellvertreter für ein frisch geöffnetes Dokument an (Sandbox-Thread).</summary>
    private static PdfiumDocument Register(string path, (int Id, bool HasForms, SizeF[] Sizes) opened)
    {
        var document = new PdfiumDocument(path, opened.Id, SandboxClient.CurrentGeneration, opened.HasForms, opened.Sizes);
        SandboxClient.Register(opened.Id, document);
        return document;
    }

    /// <summary>Abfrage, die nach einem Absturz <paramref name="fallback"/> liefert.</summary>
    private Task<T> Query<T>(T fallback, byte command, Action<BinaryWriter> arguments, Func<BinaryReader, T> result) =>
        SandboxClient.QueryAsync(generation, fallback, p => p.Call(command, w => { w.Write(id); arguments(w); }, result));

    /// <summary>Befehl, der bei Fehlern wirft (InvalidOperationException).</summary>
    private Task<T> Command<T>(byte command, Action<BinaryWriter> arguments, Func<BinaryReader, T> result) =>
        SandboxClient.RunAsync(generation, p => p.Call(command, w => { w.Write(id); arguments(w); }, result));

    // ================================================================== Rendern

    /// <summary>Rendert einen Ausschnitt der Seite: Die ganze Seite hätte pageWidth × pageHeight Pixel, geliefert wird nur das Rechteck
    /// <paramref name="tile"/> daraus (samt Formularfeldern). <paramref name="stillWanted"/> wird unmittelbar vor dem Auftrag auf dem
    /// Sandbox-Thread gefragt – Aufträge für weggescrollte Bereiche verfallen (dann null).</summary>
    public Task<Bitmap?> RenderAsync(int index, int pageWidth, int pageHeight, Rectangle tile, Func<bool> stillWanted) =>
        SandboxClient.QueryAsync<Bitmap?>(generation, null, p => !stillWanted() ? null : p.Call(Protocol.Render, w =>
        {
            w.Write(id); w.Write(index); w.Write(pageWidth); w.Write(pageHeight);
            w.Write(tile.X); w.Write(tile.Y); w.Write(tile.Width); w.Write(tile.Height);
        }, r => ToBitmap(r.ReadBytes(tile.Width * tile.Height * 4), tile.Width, tile.Height)));

    /// <summary>Rendert die ganze Seite in der angegebenen Pixelgröße (Miniaturen, Vorschau beim Zoomen).</summary>
    public Task<Bitmap?> RenderAsync(int index, int width, int height, Func<bool> stillWanted) =>
        RenderAsync(index, width, height, new Rectangle(0, 0, width, height), stillWanted);

    private static Bitmap ToBitmap(byte[] pixels, int width, int height)
    {
        if (pixels.Length != width * height * 4) { throw new InvalidDataException("Unvollständiges Bild vom Hilfsprogramm."); }
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb); // BGRA – dieselbe Byte-Reihenfolge wie FPDFBitmap_BGRA
        var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            for (var y = 0; y < height; y++) { Marshal.Copy(pixels, y * width * 4, data.Scan0 + y * data.Stride, width * 4); }
        }
        finally { bitmap.UnlockBits(data); }
        return bitmap;
    }

    /// <summary>Eine Seite als EMF (Vektor) in width × height Druckerpixeln, zum Abspielen auf dem Drucker. Formularfelder fehlen darin –
    /// für den Druck eines Formulars zuerst <see cref="CreatePrintCopyAsync"/>.</summary>
    public Task<byte[]> RenderEmfAsync(int index, int width, int height) =>
        Command(Protocol.RenderEmf, w => { w.Write(index); w.Write(width); w.Write(height); }, r => r.ReadBytes(r.ReadInt32()));

    /// <summary>Für den Druck: eine eigene Kopie des Dokuments – ohne die Drehung der Ansicht, bei einem Formular mit den aktuellen Werten
    /// der Seiten <paramref name="first"/> bis <paramref name="last"/> (0-basiert) fest auf der Seite. Gleiche Seiten, gleiche Reihenfolge;
    /// Druckdialog (Vorschau) und Druck arbeiten mit ihr. Der Aufrufer gibt sie frei.</summary>
    public Task<PdfiumDocument> CreatePrintCopyAsync(int first, int last) =>
        SandboxClient.RunAsync(generation, p => Register(Path, p.Call(Protocol.PrintCopy, w => { w.Write(id); w.Write(first); w.Write(last); }, ReadOpened)));

    /// <summary>Das Dokument samt eingetragener Werte als vollständige PDF-Datei (der Fokus wird vorher aus dem Feld genommen);
    /// removeSecurity: unverschlüsselt – entfernt die Einschränkungen eines Besitzerkennworts, ohne etwas vom Inhalt zu verlieren.</summary>
    public Task<byte[]> SaveAsync(bool removeSecurity = false) => Command(Protocol.Save, w => w.Write(removeSecurity), r => r.ReadBytes(r.ReadInt32()));

    /// <summary>Berechtigungsbits (/P, unverschlüsselt alle gesetzt) und Revision des Sicherheits-Handlers (-1 = nicht verschlüsselt).</summary>
    public Task<(uint Permissions, int Revision)> SecurityAsync() =>
        Command(Protocol.Security, _ => { }, r => (r.ReadUInt32(), r.ReadInt32()));

    /// <summary>Eingebettete Dateien: Name, Größe in Byte (-1 = unbekannt) und Änderungsdatum (null = keins in der Datei); Nummer =
    /// Position in der Liste.</summary>
    public Task<List<(string Name, long Size, DateTime? Modified)>> AttachmentsAsync() =>
        Command(Protocol.Attachments, _ => { }, r =>
        {
            var count = r.ReadInt32();
            var list = new List<(string, long, DateTime?)>(count);
            for (var i = 0; i < count; i++) { list.Add((r.ReadString(), r.ReadInt64(), ParsePdfDate(r.ReadString()))); }
            return list;
        });

    /// <summary>Anzahl der eingebetteten Dateien – billig, ohne sie zu entpacken (für die Statusleiste beim Öffnen).</summary>
    public Task<int> AttachmentCountAsync() => Command(Protocol.AttachmentCount, _ => { }, r => r.ReadInt32());

    /// <summary>PDF-Datum „D:JJJJMMTTHHmmSS+HH'mm'“ in Ortszeit; alles nach dem Jahr ist optional, ohne Zeitzone gilt die Angabe als
    /// Ortszeit. null, wenn die Zeichenfolge leer oder kein Datum ist (sie stammt vom Ersteller der PDF).</summary>
    internal static DateTime? ParsePdfDate(string text)
    {
        var s = text.Trim();
        if (s.StartsWith("D:", StringComparison.Ordinal)) { s = s[2..]; }
        int Part(int start, int length, int fallback) =>
            s.Length >= start + length && int.TryParse(s.AsSpan(start, length), System.Globalization.NumberStyles.None, null, out var value) ? value : fallback;
        var year = Part(0, 4, -1);
        if (year < 1) { return null; }
        var (month, day, hour, minute, second) = (Part(4, 2, 1), Part(6, 2, 1), Part(8, 2, 0), Part(10, 2, 0), Part(12, 2, 0));
        if (month is < 1 or > 12 || day < 1 || day > DateTime.DaysInMonth(year, month) || hour > 23 || minute > 59 || second > 59) { return null; }
        var local = new DateTime(year, month, day, hour, minute, second);
        var zone = s.Length > 14 ? s[14] : '\0';
        if (zone == 'Z') { return DateTime.SpecifyKind(local, DateTimeKind.Utc).ToLocalTime(); }
        if (zone is '+' or '-')
        {
            var offset = new TimeSpan(Part(15, 2, 0), s.Length >= 20 ? Part(18, 2, 0) : 0, 0);
            if (offset > TimeSpan.FromHours(14)) { return local; } // ungültige Zeitzone: DateTimeOffset würfe
            // am Rand des Wertebereichs (Jahr 1 mit positiver, 9999 mit negativer Zone) wirft der Konstruktor – die Zeichenfolge kommt aus
            // einer fremden PDF, und eine Ausnahme hier ließe den Rest der Antwort ungelesen in der Pipe (Review 06.10.2026)
            try { return new DateTimeOffset(local, zone == '-' ? -offset : offset).LocalDateTime; }
            catch (ArgumentOutOfRangeException) { return local; }
        }
        return local;
    }

    /// <summary>Inhalt einer eingebetteten Datei – zum Speichern, nie zum Ausführen.</summary>
    public Task<byte[]> AttachmentDataAsync(int index) => Command(Protocol.AttachmentData, w => w.Write(index), r => r.ReadBytes(r.ReadInt32()));

    /// <summary>PDF-Version (z. B. „1.7“, null = unbekannt) und ob das Dokument verschlüsselt ist.</summary>
    public Task<(string? Version, bool Encrypted)> InfoAsync() =>
        Query<(string?, bool)>((null, false), Protocol.Info, _ => { },
            r => { var (version, revision) = (r.ReadInt32(), r.ReadInt32()); return (version > 0 ? $"{version / 10}.{version % 10}" : null, revision >= 0); });

    // ================================================================== Text

    /// <summary>Anzahl der Zeichen auf der Seite (laut PDFiums Textextraktion).</summary>
    public Task<int> CharCountAsync(int index) => Query(0, Protocol.CharCount, w => w.Write(index), r => r.ReadInt32());

    /// <summary>Zeichen an einer Stelle der Seite (Seitenpixel); -1, wenn im Umkreis von <paramref name="tolerancePt"/> Punkt kein Text ist.</summary>
    public Task<int> CharAtAsync(int index, int pageWidth, int pageHeight, Point point, double tolerancePt = HitTolerancePt) =>
        Query(-1, Protocol.CharAt, w => { w.Write(index); w.Write(pageWidth); w.Write(pageHeight); w.Write(point.X); w.Write(point.Y); w.Write(tolerancePt); }, r => r.ReadInt32());

    /// <summary>Das Zeichen, das einer Stelle der Seite am nächsten liegt – auch neben dem Text; -1 auf Seiten ohne Text.</summary>
    public Task<int> NearestCharAsync(int index, int pageWidth, int pageHeight, Point point) =>
        Query(-1, Protocol.NearestChar, w => { w.Write(index); w.Write(pageWidth); w.Write(pageHeight); w.Write(point.X); w.Write(point.Y); }, r => r.ReadInt32());

    /// <summary>Das Wort um ein Zeichen: (erstes Zeichen, Anzahl) – begrenzt durch Leerraum.</summary>
    public Task<(int Start, int Count)> WordAtAsync(int index, int charIndex) =>
        Query((charIndex, 1), Protocol.WordAt, w => { w.Write(index); w.Write(charIndex); }, r => (r.ReadInt32(), r.ReadInt32()));

    /// <summary>Rechtecke (Seitenpixel) für die Markierung eines Zeichenbereichs – PDFium fasst die Zeichen zeilenweise zusammen.</summary>
    public Task<List<Rectangle>> SelectionRectsAsync(int index, int pageWidth, int pageHeight, int start, int count) =>
        Query([], Protocol.SelectionRects, w => { w.Write(index); w.Write(pageWidth); w.Write(pageHeight); w.Write(start); w.Write(count); }, r =>
        {
            var rects = new List<Rectangle>();
            for (var n = r.ReadInt32(); n > 0; n--) { rects.Add(new Rectangle(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32())); }
            return rects;
        });

    /// <summary>Alle Fundstellen eines Suchbegriffs auf einer Seite: (erstes Zeichen, Anzahl) in Lesereihenfolge.</summary>
    public Task<List<(int Start, int Count)>> SearchPageAsync(int index, string query, bool matchCase, bool wholeWord) =>
        Query([], Protocol.SearchPage, w => { w.Write(index); w.Write(query); w.Write(matchCase); w.Write(wholeWord); }, r =>
        {
            var hits = new List<(int, int)>();
            for (var n = r.ReadInt32(); n > 0; n--) { hits.Add((r.ReadInt32(), r.ReadInt32())); }
            return hits;
        });

    /// <summary>Text eines Zeichenbereichs der Seite.</summary>
    public Task<string> TextAsync(int index, int start, int count) =>
        Query(string.Empty, Protocol.Text, w => { w.Write(index); w.Write(start); w.Write(count); }, r => r.ReadString());

    // ================================================================== Links und Lesezeichen

    /// <summary>Link unter einer Stelle der Seite (Seitenpixel): zuerst Link-Anmerkungen, dann Webadressen, die nur als Text dastehen
    /// (wie Chromium). null, wenn dort kein Link ist oder sein Ziel nicht unterstützt wird (Dateien, Programme, andere PDFs).</summary>
    public Task<LinkTarget?> LinkAtAsync(int index, int pageWidth, int pageHeight, Point point) =>
        Query(null, Protocol.LinkAt, w => { w.Write(index); w.Write(pageWidth); w.Write(pageHeight); w.Write(point.X); w.Write(point.Y); },
            r => r.ReadBoolean() ? ReadTarget(r) : null);

    private static LinkTarget ReadTarget(BinaryReader reader)
    {
        var (page, top, uri) = Protocol.ReadTarget(reader);
        return new LinkTarget(page, top, uri);
    }

    /// <summary>Die Lesezeichen des Dokuments als Baum (leer ohne Lesezeichen).</summary>
    public Task<List<OutlineItem>> OutlineAsync() => Query([], Protocol.Outline, _ => { }, ReadOutline);

    private static List<OutlineItem> ReadOutline(BinaryReader reader)
    {
        var items = new List<OutlineItem>();
        for (var n = reader.ReadInt32(); n > 0; n--)
        {
            var title = reader.ReadString();
            var item = new OutlineItem(title, reader.ReadBoolean() ? ReadTarget(reader) : null);
            item.Children.AddRange(ReadOutline(reader));
            items.Add(item);
        }
        return items;
    }

    // ================================================================== Formulare

    private Task<int> FormMouse(byte kind, int index, int pageWidth, int pageHeight, Point point, int modifiers) =>
        Query(-1, Protocol.FormMouse, w =>
        {
            w.Write(CurrentPageIndex); w.Write(kind); w.Write(index); w.Write(pageWidth); w.Write(pageHeight);
            w.Write(point.X); w.Write(point.Y); w.Write(modifiers);
        }, r => r.ReadInt32());

    /// <summary>Maus bewegt (für Schaltflächen-Hervorhebung und Textauswahl im Feld); liefert die Feldart an der Stelle (-1 = keins).</summary>
    public Task<int> FormMouseMoveAsync(int index, int pageWidth, int pageHeight, Point point, int modifiers) =>
        HasForms ? FormMouse(Protocol.MouseMove, index, pageWidth, pageHeight, point, modifiers) : Task.FromResult(-1);

    /// <summary>Linke Maustaste gedrückt – geht immer an PDFium (ein Klick neben die Felder nimmt den Fokus weg); liefert die Feldart
    /// an der Stelle (-1 = keins, dann gehört der Klick der Ansicht).</summary>
    public Task<int> FormMouseDownAsync(int index, int pageWidth, int pageHeight, Point point, int modifiers) =>
        HasForms ? FormMouse(Protocol.MouseDown, index, pageWidth, pageHeight, point, modifiers) : Task.FromResult(-1);

    public Task FormMouseUpAsync(int index, int pageWidth, int pageHeight, Point point, int modifiers) =>
        HasForms ? FormMouse(Protocol.MouseUp, index, pageWidth, pageHeight, point, modifiers) : Task.CompletedTask;

    private Task<bool> FormKey(byte kind, int code, int modifiers) => !HasForms ? Task.FromResult(false) :
        Query(false, Protocol.FormKey, w => { w.Write(CurrentPageIndex); w.Write(kind); w.Write(code); w.Write(modifiers); }, r => r.ReadBoolean());

    /// <summary>Taste an das Feld mit dem Fokus; true, wenn PDFium sie verarbeitet hat (sonst darf die Ansicht sie nutzen).</summary>
    public Task<bool> FormKeyDownAsync(int keyCode, int modifiers) => FormKey(Protocol.KeyDown, keyCode, modifiers);

    public Task<bool> FormKeyUpAsync(int keyCode, int modifiers) => FormKey(Protocol.KeyUp, keyCode, modifiers);

    public Task<bool> FormCharAsync(char character, int modifiers) => FormKey(Protocol.KeyChar, character, modifiers);

    private Task<bool> FormEdit(byte kind) => !HasForms ? Task.FromResult(false) :
        Query(false, Protocol.FormEdit, w => { w.Write(CurrentPageIndex); w.Write(kind); }, r => r.ReadBoolean());

    public Task<bool> FormSelectAllAsync() => FormEdit(Protocol.EditSelectAll);

    public Task<bool> FormUndoAsync() => FormEdit(Protocol.EditUndo);

    public Task<bool> FormRedoAsync() => FormEdit(Protocol.EditRedo);

    /// <summary>Nimmt den Fokus aus dem Feld – PDFium übernimmt dabei den eingetippten Wert.</summary>
    public Task FormKillFocusAsync() => FormEdit(Protocol.EditKillFocus);

    /// <summary>Markierter Text im Feld mit dem Fokus (leer ohne Markierung).</summary>
    public Task<string> FormSelectedTextAsync() => !HasForms ? Task.FromResult(string.Empty) :
        Query(string.Empty, Protocol.FormSelectedText, w => w.Write(CurrentPageIndex), r => r.ReadString());

    /// <summary>Ersetzt die Markierung im Feld mit dem Fokus (Einfügen; leerer Text = Ausschneiden).</summary>
    public Task<bool> FormReplaceSelectionAsync(string text) => !HasForms ? Task.FromResult(false) :
        Query(false, Protocol.FormReplaceSelection, w => { w.Write(CurrentPageIndex); w.Write(text); }, r => r.ReadBoolean());

    // ================================================================== Hervorhebungen

    /// <summary>Gelb der Hervorhebungen in Acrobat (/C [1 0,757 0]); aufgetragen wird es mit 40 % Deckkraft.</summary>
    public const int AcrobatYellow = 0xFFC100;

    /// <summary>Hervorhebung (Farbe 0xRRGGBB, Vorgabe Acrobats Gelb) über einen Zeichenbereich einer Seite; true, wenn angelegt.</summary>
    /// <summary>Die Vierecke je Textzeile (links, oben, rechts, unten in PDF-Koordinaten), die eine Hervorhebung des Zeichenbereichs bekäme.</summary>
    public Task<List<(double Left, double Top, double Right, double Bottom)>> HighlightBoxesAsync(int index, int start, int count) =>
        Command(Protocol.HighlightBoxes, w => { w.Write(index); w.Write(start); w.Write(count); }, r =>
        {
            var boxes = new List<(double, double, double, double)>();
            for (var i = r.ReadInt32(); i > 0; i--) { boxes.Add((r.ReadDouble(), r.ReadDouble(), r.ReadDouble(), r.ReadDouble())); }
            return boxes;
        });

    public Task<bool> AddHighlightAsync(int index, int start, int count, int color = AcrobatYellow) =>
        Query(false, Protocol.AddHighlight, w => { w.Write(index); w.Write(start); w.Write(count); w.Write(color); }, r => r.ReadBoolean());

    /// <summary>Nummer der Textmarkierung an einer Stelle der Seite (Seitenpixel), -1 = keine.</summary>
    /// <summary>Eine Stelle der Seite (Seitenpixel) in PDF-Koordinaten der Seite (Punkt, Ursprung unten links); null nach einem Absturz.</summary>
    public Task<(double X, double Y)?> PagePointAsync(int index, int pageWidth, int pageHeight, Point point) =>
        Query<(double X, double Y)?>(null, Protocol.PagePoint, w => { w.Write(index); w.Write(pageWidth); w.Write(pageHeight); w.Write(point.X); w.Write(point.Y); },
            r => (r.ReadDouble(), r.ReadDouble()));

    /// <summary>Oberste Anmerkung einer der Arten (Bitmaske 1 &lt;&lt; FPDF_ANNOT_*) an einer Stelle der Seite (Seitenpixel): ihre Nummer
    /// im /Annots-Array, ihre Art und ihr Rechteck in Seitenpixeln; null, wenn dort keine ist.</summary>
    public Task<(int Number, int Subtype, Rectangle Rect)?> AnnotationAtAsync(int index, int pageWidth, int pageHeight, Point point, int subtypes) =>
        Query<(int Number, int Subtype, Rectangle Rect)?>(null, Protocol.AnnotAt, w => { w.Write(index); w.Write(pageWidth); w.Write(pageHeight); w.Write(point.X); w.Write(point.Y); w.Write(subtypes); },
            r =>
            {
                var (number, subtype) = (r.ReadInt32(), r.ReadInt32());
                var rect = new Rectangle(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32());
                return number < 0 ? null : (number, subtype, rect);
            });

    /// <summary>Freihand-Strich (Zeichnen): Punkte in Seitenpixeln, Farbe 0xRRGGBB, Strichstärke in Punkt; true, wenn angelegt.</summary>
    public Task<bool> AddInkAsync(int index, int pageWidth, int pageHeight, int color, float width, IReadOnlyList<Point> points) =>
        Query(false, Protocol.AddInk, w =>
        {
            w.Write(index); w.Write(pageWidth); w.Write(pageHeight); w.Write(color); w.Write(width); w.Write(points.Count);
            foreach (var p in points) { w.Write(p.X); w.Write(p.Y); }
        }, r => r.ReadBoolean());

    /// <summary>Blendet eine Anmerkung nur in der Anzeige aus bzw. wieder ein – die Datei bleibt unberührt.</summary>
    public Task<bool> SetAnnotationHiddenAsync(int index, int number, bool hidden) =>
        Query(false, Protocol.SetAnnotHidden, w => { w.Write(index); w.Write(number); w.Write(hidden); }, r => r.ReadBoolean());

    public Task<int> MarkupAtAsync(int index, int pageWidth, int pageHeight, Point point) =>
        Query(-1, Protocol.MarkupAt, w => { w.Write(index); w.Write(pageWidth); w.Write(pageHeight); w.Write(point.X); w.Write(point.Y); }, r => r.ReadInt32());

    /// <summary>Oberste Zeichnung (Ink), deren Linie die Stelle (Seitenpixel) trifft; -1 = keine.</summary>
    public Task<int> InkAtAsync(int index, int pageWidth, int pageHeight, Point point) =>
        Query(-1, Protocol.InkAt, w => { w.Write(index); w.Write(pageWidth); w.Write(pageHeight); w.Write(point.X); w.Write(point.Y); }, r => r.ReadInt32());

    /// <summary>Entfernt eine Textmarkierung oder Zeichnung (Nummer aus <see cref="MarkupAtAsync"/> bzw. <see cref="InkAtAsync"/>).</summary>
    public Task<bool> RemoveMarkupAsync(int index, int number) =>
        Query(false, Protocol.RemoveMarkup, w => { w.Write(index); w.Write(number); }, r => r.ReadBoolean());

    /// <summary>Vom Sandbox-Thread: PDFium meldet einen neu zu zeichnenden Bereich.</summary>
    internal void RaiseFormInvalidated(int page, RectangleF area)
    {
        try { FormInvalidated?.Invoke(this, new FormInvalidatedEventArgs(page, area)); }
        catch (Exception ex) { Debug.WriteLine(ex); }
    }

    /// <summary>Vom Sandbox-Thread: Das Dokument wurde geändert.</summary>
    internal void RaiseChanged()
    {
        try { Changed?.Invoke(this, EventArgs.Empty); }
        catch (Exception ex) { Debug.WriteLine(ex); }
    }

    public void Dispose()
    {
        // Schließen im Hilfsprogramm; war es inzwischen abgestürzt, gibt es dort nichts mehr zu schließen
        _ = SandboxClient.QueryAsync(generation, false, p =>
        {
            SandboxClient.Unregister(id);
            p.Call(Protocol.Close, w => w.Write(id), _ => true);
            return true;
        });
    }
}

/// <summary>Neu zu zeichnender Bereich einer Seite, als Anteil an ihrer Breite und Höhe (0…1) – unabhängig von der Zoomstufe.</summary>
internal sealed class FormInvalidatedEventArgs(int page, RectangleF area) : EventArgs
{
    public int Page { get; } = page;
    public RectangleF Area { get; } = area;
}

/// <summary>Die Datei ist kennwortgeschützt: Es fehlte ein Kennwort, oder (<see cref="WrongPassword"/>) das angegebene war falsch.</summary>
internal sealed class PasswordRequiredException(bool wrongPassword)
    : InvalidOperationException(wrongPassword ? "Das Kennwort ist falsch." : "Die Datei ist kennwortgeschützt.")
{
    public bool WrongPassword { get; } = wrongPassword;
}
