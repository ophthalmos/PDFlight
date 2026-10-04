using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace PDFLight.Host;

/// <summary>Ein PDF-Dokument im Hilfsprogramm. Die Bytes kommen vom Hauptprogramm und bleiben fixiert, solange PDFium aus ihnen liest.
/// Die zuletzt benutzten Seiten bleiben samt Textdaten geladen. Alles läuft auf dem einen Thread des Hilfsprogramms.
/// „Seitenpixel“ meint die Seite als Bild der Größe Breite × Höhe (Ursprung links oben); die Umrechnung in PDF-Punkte übernimmt PDFium
/// (FPDF_DeviceToPage/PageToDevice, berücksichtigt /Rotate und den Seitenausschnitt).</summary>
internal sealed unsafe class PdfDocument : IDisposable
{
    private const int LoadedPageLimit = 16;
    private const int MaxPixels = 100_000_000;     // Obergrenze je Bild (BGRA ≈ 400 MB), schützt vor unsinnigen Aufträgen
    private const uint HighlightColor = 0xFFE4DD;  // Feldhervorhebung wie in Chromium, halb deckend
    private const byte HighlightAlpha = 100;
    private const int KeyDelete = 0x2E;            // FWL_VKEY_Delete; Rückschritt kommt als Zeichen

    private GCHandle pin;
    private nint document, form, formInfo;
    private readonly Dictionary<int, LoadedPage> loadedPages = [];
    private readonly Dictionary<int, (double Left, double Right, double Bottom, double Top)[]> charBoxes = [];
    private long useCounter;
    private int formPage = -1; // Seite des letzten Formular-Klicks: dorthin gehen die Tasten

    private sealed class LoadedPage(nint page)
    {
        public nint Page { get; } = page;
        public nint Text { get; set; }
        public nint WebLinks { get; set; }
        public long LastUse { get; set; }
    }

    public int Id { get; }
    public (float Width, float Height)[] PageSizes { get; }
    public bool HasForms => form != 0;

    /// <summary>Angezeigte Seite – PDFium fragt danach (FFI_GetCurrentPage). Kommt mit jedem Formularbefehl.</summary>
    public int CurrentPage { get; set; }

    /// <summary>Drehung der Ansicht in Vierteln im Uhrzeigersinn (0–3), zusätzlich zu /Rotate der Seite: gilt für Darstellung und alle
    /// Umrechnungen zwischen Seitenpixeln und Seite. Die Datei bleibt unverändert; der Druck (EMF) zeigt die Seite ungedreht.</summary>
    public int ViewRotation { get; set; }

    private const uint ErrorPassword = 4; // FPDF_ERR_PASSWORD: Kennwort nötig oder falsch

    /// <summary>Öffnet ein Dokument aus Bytes. <paramref name="password"/>: Benutzer-Kennwort (UTF-8 übergeben; leer = keins) –
    /// Dateien, die nur ein Besitzer-Kennwort haben, öffnet PDFium ohne. Fehlt das nötige Kennwort oder ist es falsch:
    /// <see cref="PasswordNeededException"/>.</summary>
    public PdfDocument(int id, byte[] bytes, string password = "")
    {
        Id = id;
        pin = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        var passwordBytes = password.Length == 0 ? null : Encoding.UTF8.GetBytes(password + "\0");
        fixed (byte* passwordPointer = passwordBytes)
        {
            document = Pdfium.LoadMemDocument(pin.AddrOfPinnedObject(), (nuint)bytes.Length, (nint)passwordPointer);
        }
        if (document == 0)
        {
            var error = Pdfium.GetLastError();
            pin.Free();
            if (error == ErrorPassword) { throw new PasswordNeededException(); }
            throw new InvalidOperationException(ErrorText(error));
        }
        var sizes = new (float, float)[Pdfium.GetPageCount(document)];
        for (var i = 0; i < sizes.Length; i++)
        {
            _ = Pdfium.GetPageSizeByIndex(document, i, out var width, out var height);
            sizes[i] = ((float)width, (float)height);
        }
        PageSizes = sizes;
        InitForms();
    }

    private static string ErrorText(uint error) => error switch
    {
        2 => "Die Datei wurde nicht gefunden oder konnte nicht geöffnet werden.",
        3 => "Die Datei ist kein gültiges PDF oder beschädigt.",
        4 => "Die Datei ist kennwortgeschützt.",
        5 => "Das Sicherheitsverfahren der Datei wird nicht unterstützt.",
        6 => "Die Seite wurde nicht gefunden oder ist fehlerhaft.",
        _ => $"PDFium konnte die Datei nicht öffnen (Fehler {error}).",
    };

    // ================================================================== Seiten

    private LoadedPage GetPage(int index)
    {
        if (index < 0 || index >= PageSizes.Length) { throw new ArgumentOutOfRangeException(nameof(index), $"Seite {index + 1} gibt es nicht."); }
        if (!loadedPages.TryGetValue(index, out var loaded))
        {
            if (loadedPages.Count >= LoadedPageLimit) // die am längsten unbenutzte Seite schließen – nicht die mit dem Eingabefokus
            {
                var oldest = loadedPages.Where(p => p.Key != formPage).MinBy(p => p.Value.LastUse);
                ClosePage(oldest.Value);
                loadedPages.Remove(oldest.Key);
            }
            var page = Pdfium.LoadPage(document, index);
            if (page == 0) { throw new InvalidOperationException($"Seite {index + 1} konnte nicht geladen werden."); }
            loaded = new LoadedPage(page);
            loadedPages[index] = loaded; // vor FORM_OnAfterLoadPage: PDFium fragt dabei schon nach der Seite
            if (form != 0) { Pdfium.FormOnAfterLoadPage(page, form); }
        }
        loaded.LastUse = ++useCounter;
        return loaded;
    }

    private nint TextPage(int index)
    {
        var loaded = GetPage(index);
        if (loaded.Text == 0) { loaded.Text = Pdfium.TextLoadPage(loaded.Page); }
        return loaded.Text;
    }

    private void ClosePage(LoadedPage loaded)
    {
        if (loaded.WebLinks != 0) { Pdfium.LinkCloseWebLinks(loaded.WebLinks); }
        if (loaded.Text != 0) { Pdfium.TextClosePage(loaded.Text); }
        if (form != 0) { Pdfium.FormOnBeforeClosePage(loaded.Page, form); }
        Pdfium.ClosePage(loaded.Page);
    }

    /// <summary>Für die Formular-Rückrufe: nur schon geladene Seiten – ein Laden mitten in einem FORM_-Aufruf löste weitere Rückrufe aus.</summary>
    public nint LoadedPageHandle(int index) => loadedPages.TryGetValue(index, out var loaded) ? loaded.Page : 0;

    private (nint Page, double X, double Y) ToPagePoint(int index, int pageWidth, int pageHeight, int x, int y)
    {
        var page = GetPage(index).Page;
        _ = Pdfium.DeviceToPage(page, 0, 0, pageWidth, pageHeight, ViewRotation, x, y, out var px, out var py);
        return (page, px, py);
    }

    private static void CheckSize(int width, int height)
    {
        if (width <= 0 || height <= 0 || (long)width * height > MaxPixels) { throw new ArgumentOutOfRangeException(nameof(width), $"Ungültige Bildgröße {width} × {height}."); }
    }

    // ================================================================== Rendern

    /// <summary>Ein Ausschnitt der Seite als BGRA-Bytes: Die ganze Seite hätte pageWidth × pageHeight Pixel, geliefert wird nur das
    /// Rechteck (x, y, width, height) daraus – PDFium zeichnet mit negativem Versatz und schneidet am Bitmap-Rand ab.</summary>
    public byte[] Render(int index, int pageWidth, int pageHeight, int x, int y, int width, int height)
    {
        CheckSize(width, height);
        CheckSize(pageWidth, pageHeight);
        var page = GetPage(index).Page;
        var pixels = new byte[width * height * 4];
        fixed (byte* buffer = pixels)
        {
            var bitmap = Pdfium.BitmapCreateEx(width, height, Pdfium.BitmapBgra, (nint)buffer, width * 4);
            _ = Pdfium.BitmapFillRect(bitmap, 0, 0, width, height, 0xFFFFFFFF); // weißes Papier, deckend
            Pdfium.RenderPageBitmap(bitmap, page, -x, -y, pageWidth, pageHeight, ViewRotation, Pdfium.RenderAnnotations);
            if (form != 0) { Pdfium.FflDraw(form, bitmap, page, -x, -y, pageWidth, pageHeight, ViewRotation, Pdfium.RenderAnnotations); } // Felder samt Werten
            Pdfium.BitmapDestroy(bitmap);
        }
        return pixels;
    }

    /// <summary>Die Seite als EMF (Vektor) in width × height Druckerpixeln. Formularfelder zeichnet FPDF_RenderPage nicht – für den Druck
    /// eines Formulars erst eine <see cref="PrintCopy"/> anlegen.</summary>
    public byte[] RenderEmf(int index, int width, int height)
    {
        CheckSize(width, height);
        var page = GetPage(index).Page;
        var dc = CreatePageMetafile(width, height);
        Pdfium.RenderPage(dc, page, 0, 0, width, height, 0, Pdfium.RenderAnnotations | Pdfium.RenderForPrinting);
        var metafile = Native.CloseEnhMetaFile(dc);
        if (metafile == 0) { throw new Win32Exception(); }
        try
        {
            var bytes = new byte[Native.GetEnhMetaFileBits(metafile, 0, 0)];
            fixed (byte* buffer = bytes) { _ = Native.GetEnhMetaFileBits(metafile, (uint)bytes.Length, (nint)buffer); }
            return bytes;
        }
        finally { _ = Native.DeleteEnhMetaFile(metafile); }
    }

    /// <summary>EMF im Speicher für eine Seite von width × height Einheiten (Druckerpixel). Zwei Fallen:
    /// – Bezugsgerät einer EMF ist der Bildschirm (z. B. 2560 × 1440). PDFium nimmt dessen Fläche als Zeichenfläche und schneidet alles
    ///   ab, was darüber hinausgeht – bei 300 dpi fehlte das Logo in Seitenmitte, bei 600 dpi die ganze Seite. Deshalb verkleinert eine
    ///   Welttransformation die Seite auf die Gerätefläche; PDFium zeichnet weiter in voller Auflösung, die EMF speichert die vollen
    ///   Koordinaten samt Transformation, und beim Abspielen wird der Rahmen wieder auf die Zielgröße gebracht.
    /// – Ohne Rahmen nähme die EMF nur die Fläche des Gezeichneten als Bildgröße und streckte sie beim Abspielen auf die ganze Seite.
    ///   Der Rahmen ist in 0,01 mm des Bezugsgeräts anzugeben – dessen Maße liefert ein Probe-DC.</summary>
    private static nint CreatePageMetafile(int width, int height)
    {
        var probe = Native.CreateEnhMetaFile(0, null, 0, null);
        if (probe == 0) { throw new Win32Exception(); }
        var (deviceWidth, deviceHeight) = (Native.GetDeviceCaps(probe, Native.HorzRes), Native.GetDeviceCaps(probe, Native.VertRes));
        var (deviceMmX, deviceMmY) = (Native.GetDeviceCaps(probe, Native.HorzSize), Native.GetDeviceCaps(probe, Native.VertSize));
        _ = Native.DeleteEnhMetaFile(Native.CloseEnhMetaFile(probe));

        var scale = Math.Min(1.0, Math.Min((double)deviceWidth / width, (double)deviceHeight / height)); // Seite → Gerätefläche
        var frame = stackalloc int[4];
        frame[0] = 0;
        frame[1] = 0;
        frame[2] = (int)(width * scale * deviceMmX * 100 / deviceWidth);
        frame[3] = (int)(height * scale * deviceMmY * 100 / deviceHeight);
        var dc = Native.CreateEnhMetaFile(0, null, (nint)frame, null);
        if (dc == 0) { throw new Win32Exception(); }
        var transform = new Native.XForm { M11 = (float)scale, M22 = (float)scale };
        if (Native.SetGraphicsMode(dc, Native.GraphicsModeAdvanced) == 0 || !Native.SetWorldTransform(dc, &transform))
        {
            _ = Native.DeleteEnhMetaFile(Native.CloseEnhMetaFile(dc));
            throw new Win32Exception();
        }
        return dc;
    }

    // ================================================================== Text

    public int CharCount(int index) => Pdfium.TextCountChars(TextPage(index));

    /// <summary>Zeichen an einer Stelle (Seitenpixel); -1, wenn im Umkreis von <paramref name="tolerancePt"/> Punkt kein Text ist.</summary>
    public int CharAt(int index, int pageWidth, int pageHeight, int x, int y, double tolerancePt)
    {
        var (_, px, py) = ToPagePoint(index, pageWidth, pageHeight, x, y);
        return Pdfium.TextGetCharIndexAtPos(TextPage(index), px, py, tolerancePt, tolerancePt);
    }

    /// <summary>Das Zeichen, das einer Stelle am nächsten liegt – auch neben dem Text (Ziehen ab leerer Fläche, über das Zeilenende
    /// hinaus). Der senkrechte Abstand zählt vierfach, damit die Zeile unter der Maus gewinnt. -1 auf Seiten ohne Text.
    /// FPDFText_GetCharIndexAtPos taugt dafür nicht: mit großer Toleranz liefert es irgendein Zeichen im Umkreis.</summary>
    public int NearestChar(int index, int pageWidth, int pageHeight, int x, int y)
    {
        var (_, px, py) = ToPagePoint(index, pageWidth, pageHeight, x, y);
        var boxes = CharBoxes(index);
        var best = -1;
        var bestScore = double.MaxValue;
        for (var i = 0; i < boxes.Length; i++)
        {
            var (left, right, bottom, top) = boxes[i];
            if (right <= left) { continue; } // Zeichen ohne Fläche (Zeilenumbrüche, erzeugte Leerzeichen)
            var dx = px < left ? left - px : px > right ? px - right : 0;
            var dy = py < bottom ? bottom - py : py > top ? py - top : 0;
            var score = dx + 4 * dy;
            if (score < bestScore) { bestScore = score; best = i; }
        }
        return best;
    }

    private (double Left, double Right, double Bottom, double Top)[] CharBoxes(int index)
    {
        if (charBoxes.TryGetValue(index, out var boxes)) { return boxes; }
        var text = TextPage(index);
        boxes = new (double, double, double, double)[Pdfium.TextCountChars(text)];
        for (var i = 0; i < boxes.Length; i++)
        {
            _ = Pdfium.TextGetCharBox(text, i, out var left, out var right, out var bottom, out var top);
            boxes[i] = (left, right, bottom, top);
        }
        charBoxes[index] = boxes; // in PDF-Punkten – unabhängig von der Zoomstufe
        return boxes;
    }

    /// <summary>Das Wort um ein Zeichen: (erstes Zeichen, Anzahl) – begrenzt durch Leerraum.</summary>
    public (int Start, int Count) WordAt(int index, int charIndex)
    {
        var text = TextPage(index);
        var count = Pdfium.TextCountChars(text);
        bool IsWordChar(int i) => i >= 0 && i < count && !char.IsWhiteSpace((char)Pdfium.TextGetUnicode(text, i));
        if (!IsWordChar(charIndex)) { return (charIndex, 1); }
        var start = charIndex;
        var end = charIndex;
        while (IsWordChar(start - 1)) { start--; }
        while (IsWordChar(end + 1)) { end++; }
        return (start, end - start + 1);
    }

    /// <summary>Rechtecke (Seitenpixel) für einen Zeichenbereich – PDFium fasst die Zeichen zeilenweise zusammen.</summary>
    public List<(int X, int Y, int Width, int Height)> SelectionRects(int index, int pageWidth, int pageHeight, int start, int count)
    {
        var result = new List<(int, int, int, int)>();
        var page = GetPage(index).Page;
        foreach (var (left, top, right, bottom) in LineBoxes(index, start, count))
        {
            _ = Pdfium.PageToDevice(page, 0, 0, pageWidth, pageHeight, ViewRotation, left, top, out var x1, out var y1);
            _ = Pdfium.PageToDevice(page, 0, 0, pageWidth, pageHeight, ViewRotation, right, bottom, out var x2, out var y2);
            result.Add((Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1)));
        }
        return result;
    }

    /// <summary>Je Textzeile ein durchgehendes Rechteck (PDF-Punkte, oben &gt; unten) über einen Zeichenbereich – wie Acrobat es für
    /// Hervorhebungen anlegt. Grundlage sind die „lockeren“ Zeichenrahmen (FPDFText_GetLooseCharBox): ihre Höhe kommt aus den
    /// Schriftmaßen statt aus der einzelnen Glyphe, so ist eine Zeile gleichmäßig hoch. PDFiums FPDFText_GetRect liefert dagegen ein
    /// Rechteck je Textstück in Glyphenhöhe – die Markierung wurde stufig und hatte Lücken zwischen den Wörtern.
    /// Eine neue Zeile beginnt, wenn ein Zeichen sich senkrecht kaum mit der laufenden Zeile überlappt, nach links zurückspringt oder
    /// weit rechts davon steht (nächste Spalte).</summary>
    private List<(double Left, double Top, double Right, double Bottom)> LineBoxes(int index, int start, int count)
    {
        var lines = new List<(double Left, double Top, double Right, double Bottom)>();
        if (count <= 0) { return lines; }
        var text = TextPage(index);
        var end = Math.Min(start + count, Pdfium.TextCountChars(text));
        (double Left, double Top, double Right, double Bottom)? line = null;
        for (var i = Math.Max(0, start); i < end; i++)
        {
            if (Pdfium.TextGetLooseCharBox(text, i, out var box) == 0 || box.Right <= box.Left || box.Top <= box.Bottom) { continue; } // Zeilenumbrüche, erzeugte Leerzeichen
            if (line is { } current)
            {
                var height = Math.Min(current.Top - current.Bottom, box.Top - box.Bottom);
                var overlap = Math.Min(current.Top, box.Top) - Math.Max(current.Bottom, box.Bottom);
                var sameLine = overlap > height / 2 && box.Left > current.Left - height && box.Left < current.Right + 3 * height;
                if (sameLine)
                {
                    line = (Math.Min(current.Left, box.Left), Math.Max(current.Top, box.Top), Math.Max(current.Right, box.Right), Math.Min(current.Bottom, box.Bottom));
                    continue;
                }
                lines.Add(current);
            }
            line = (box.Left, box.Top, box.Right, box.Bottom);
        }
        if (line is { } last) { lines.Add(last); }
        // Wie Acrobat: etwas Luft über und unter der Schrift, zwischen den Zeilen bleibt ein schmaler Spalt. Gemessen 02.10.2026 an einer
        // Acrobat-Hervorhebung (10-pt-Schrift, 12,25 pt Zeilenabstand, gerendert bei 150 dpi): 3 px über der Schrift, 2 px unter den
        // Unterlängen, 2 px Spalt. Das ergeben die lockeren Zeichenrahmen, oben um 6 %, unten um 2 % ihrer Höhe erweitert. Nie in die
        // Nachbarzeile hinein.
        const double PaddingTop = 0.06, PaddingBottom = 0.02;
        var padded = lines.ConvertAll(l =>
        {
            var height = l.Top - l.Bottom;
            return l with { Top = l.Top + height * PaddingTop, Bottom = l.Bottom - height * PaddingBottom };
        });
        for (var i = 0; i + 1 < padded.Count; i++)
        {
            var (upper, lower) = (padded[i], padded[i + 1]); // die obere Zeile hat die größeren y-Werte
            if (upper.Bottom >= lower.Top) { continue; }      // kein Überlappen
            var middle = (lines[i].Bottom + lines[i + 1].Top) / 2; // in der Mitte des ursprünglichen Abstands trennen
            if (middle < lines[i + 1].Top || middle > lines[i].Bottom) { continue; } // Zeilen überlappen schon ohne Erweiterung (z. B. Hochstellung)
            padded[i] = upper with { Bottom = middle };
            padded[i + 1] = lower with { Top = middle };
        }
        return padded;
    }

    public string Text(int index, int start, int count)
    {
        if (count <= 0) { return string.Empty; }
        var chars = new ushort[count + 1]; // UTF-16 samt abschließender Null
        int written;
        fixed (ushort* buffer = chars) { written = Pdfium.TextGetText(TextPage(index), start, count, buffer); }
        var builder = new StringBuilder(Math.Max(0, written - 1));
        for (var i = 0; i < written - 1; i++) { builder.Append((char)chars[i]); }
        return builder.ToString();
    }

    /// <summary>Alle Fundstellen eines Suchbegriffs auf einer Seite: (erstes Zeichen, Anzahl) in Lesereihenfolge.</summary>
    public List<(int Start, int Count)> SearchPage(int index, string query, bool matchCase, bool wholeWord)
    {
        var result = new List<(int, int)>();
        if (query.Length == 0) { return result; }
        fixed (char* what = query) // nullterminiert
        {
            var flags = (matchCase ? Pdfium.SearchMatchCase : 0) | (wholeWord ? Pdfium.SearchWholeWord : 0);
            var search = Pdfium.TextFindStart(TextPage(index), (ushort*)what, flags, 0);
            try
            {
                while (Pdfium.TextFindNext(search) != 0) { result.Add((Pdfium.TextGetSchResultIndex(search), Pdfium.TextGetSchCount(search))); }
            }
            finally { Pdfium.TextFindClose(search); }
        }
        return result;
    }

    // ================================================================== Links und Lesezeichen

    public sealed record Target(int Page, float? Top, string? Uri);

    public sealed class OutlineNode(string title, Target? target)
    {
        public string Title { get; } = title;
        public Target? Target { get; } = target;
        public List<OutlineNode> Children { get; } = [];
    }

    /// <summary>Link unter einer Stelle der Seite (Seitenpixel): zuerst Link-Anmerkungen, dann Webadressen, die nur als Text dastehen
    /// (wie Chromium). null, wenn dort kein Link ist oder sein Ziel nicht unterstützt wird.</summary>
    public Target? LinkAt(int index, int pageWidth, int pageHeight, int x, int y)
    {
        var (page, px, py) = ToPagePoint(index, pageWidth, pageHeight, x, y);
        var link = Pdfium.LinkGetLinkAtPoint(page, px, py);
        if (link != 0) { return DestTarget(Pdfium.LinkGetDest(document, link)) ?? ActionTarget(Pdfium.LinkGetAction(link)); }
        var loaded = GetPage(index);
        if (loaded.WebLinks == 0) { loaded.WebLinks = Pdfium.LinkLoadWebLinks(TextPage(index)); }
        var web = loaded.WebLinks;
        for (var i = 0; i < Pdfium.LinkCountWebLinks(web); i++)
        {
            for (var r = 0; r < Pdfium.LinkCountRects(web, i); r++)
            {
                _ = Pdfium.LinkGetRect(web, i, r, out var left, out var top, out var right, out var bottom);
                if (px < left || px > right || py < bottom || py > top) { continue; }
                var length = Pdfium.LinkGetUrl(web, i, null, 0); // samt abschließender Null
                if (length <= 1) { return null; }
                var chars = new ushort[length];
                fixed (ushort* buffer = chars) { _ = Pdfium.LinkGetUrl(web, i, buffer, length); }
                return new Target(-1, null, new string([.. chars.Take(length - 1).Select(c => (char)c)]));
            }
        }
        return null;
    }

    /// <summary>Die Lesezeichen als Baum (leer ohne Lesezeichen). Mit Schutz gegen Zyklen und zu tiefe Verschachtelung.</summary>
    public List<OutlineNode> Outline()
    {
        var result = new List<OutlineNode>();
        var visited = new HashSet<nint>();
        void Read(nint parent, List<OutlineNode> into, int depth)
        {
            if (depth > 32) { return; }
            var child = Pdfium.BookmarkGetFirstChild(document, parent);
            while (child != 0 && visited.Add(child))
            {
                var target = DestTarget(Pdfium.BookmarkGetDest(document, child)) ?? ActionTarget(Pdfium.BookmarkGetAction(child));
                var node = new OutlineNode(BookmarkTitle(child), target);
                into.Add(node);
                Read(child, node.Children, depth + 1);
                child = Pdfium.BookmarkGetNextSibling(document, child);
            }
        }
        Read(0, result, 0);
        return result;
    }

    private static string BookmarkTitle(nint bookmark)
    {
        var bytes = Pdfium.BookmarkGetTitle(bookmark, 0, 0); // UTF-16LE samt abschließender Null
        if (bytes <= 2) { return string.Empty; }
        var buffer = Marshal.AllocHGlobal((int)bytes);
        try
        {
            _ = Pdfium.BookmarkGetTitle(bookmark, buffer, bytes);
            return Marshal.PtrToStringUni(buffer) ?? string.Empty;
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    /// <summary>Seitenziel mit optionaler Höhe (PDF-Punkte von unten, wie in /XYZ); null ohne gültige Seite.</summary>
    private Target? DestTarget(nint dest)
    {
        if (dest == 0) { return null; }
        var page = Pdfium.DestGetPageIndex(document, dest);
        if (page < 0) { return null; }
        var located = Pdfium.DestGetLocationInPage(dest, out _, out var hasY, out _, out _, out var y, out _) != 0;
        return new Target(page, located && hasY != 0 ? y : null, null);
    }

    /// <summary>GoTo-Aktion (Seite im Dokument) oder URI-Aktion (Webadresse); andere Aktionen werden nicht ausgeführt.</summary>
    private Target? ActionTarget(nint action)
    {
        if (action == 0) { return null; }
        var type = Pdfium.ActionGetType(action);
        if (type == Pdfium.ActionGoTo) { return DestTarget(Pdfium.ActionGetDest(document, action)); }
        if (type != Pdfium.ActionUri) { return null; }
        var bytes = Pdfium.ActionGetUriPath(document, action, 0, 0); // 7-Bit-ASCII samt abschließender Null
        if (bytes <= 1) { return null; }
        var buffer = Marshal.AllocHGlobal((int)bytes);
        try
        {
            _ = Pdfium.ActionGetUriPath(document, action, buffer, bytes);
            return new Target(-1, null, Marshal.PtrToStringAnsi(buffer));
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    // ================================================================== Formulare

    private void InitForms()
    {
        if (Pdfium.GetFormType(document) == 0) { return; } // FORMTYPE_NONE
        formInfo = FormFillInfo.Create(this);
        form = Pdfium.InitFormFillEnvironment(document, formInfo);
        if (form == 0) { FormFillInfo.Free(formInfo); formInfo = 0; return; }
        Pdfium.SetFormFieldHighlightColor(form, 0, HighlightColor); // 0 = alle Feldarten
        Pdfium.SetFormFieldHighlightAlpha(form, HighlightAlpha);
    }

    /// <summary>Rückruf FFI_Invalidate: Rechteck in PDF-Punkten → Anteil an Breite und Höhe der angezeigten Seite.</summary>
    public void OnFormInvalidate(nint pageHandle, double left, double top, double right, double bottom)
    {
        const int Scale = 100_000; // FPDF_PageToDevice liefert ganze Zahlen – auf eine große „Seite“ abbilden, dann teilen
        foreach (var (index, loaded) in loadedPages)
        {
            if (loaded.Page != pageHandle) { continue; }
            _ = Pdfium.PageToDevice(loaded.Page, 0, 0, Scale, Scale, ViewRotation, left, top, out var x1, out var y1);
            _ = Pdfium.PageToDevice(loaded.Page, 0, 0, Scale, Scale, ViewRotation, right, bottom, out var x2, out var y2);
            Events.Invalidate(Id, index, Math.Min(x1, x2) / (float)Scale, Math.Min(y1, y2) / (float)Scale, Math.Max(x1, x2) / (float)Scale, Math.Max(y1, y2) / (float)Scale);
            return;
        }
    }

    /// <summary>Maus an das Formular; liefert die Feldart an der Stelle (-1 = keins) bzw. bei Loslassen 1/0 (verarbeitet).
    /// Ein Klick geht immer an PDFium (neben die Felder nimmt den Fokus weg); vor dem Drücken kommt eine Bewegung, denn PDFium legt das
    /// Feld-Objekt erst beim Überfahren an („mouse enter“) – ohne sie ginge ein Klick ins Leere.</summary>
    public int FormMouse(byte kind, int index, int pageWidth, int pageHeight, int x, int y, int modifiers)
    {
        if (form == 0) { return -1; }
        var (page, px, py) = ToPagePoint(index, pageWidth, pageHeight, x, y);
        switch (kind)
        {
            case Protocol.MouseMove:
                _ = Pdfium.FormOnMouseMove(form, page, modifiers, px, py);
                return Pdfium.HasFormFieldAtPoint(form, page, px, py);
            case Protocol.MouseDown:
                var field = Pdfium.HasFormFieldAtPoint(form, page, px, py);
                _ = Pdfium.FormOnMouseMove(form, page, modifiers & ~Pdfium.ModLeftButton, px, py);
                _ = Pdfium.FormOnLButtonDown(form, page, modifiers, px, py);
                if (field >= 0) { formPage = index; }
                return field;
            default:
                var handled = Pdfium.FormOnLButtonUp(form, page, modifiers, px, py) != 0;
                if (handled) { InvalidateFocusedField(); }
                return handled ? 1 : 0;
        }
    }

    /// <summary>Taste oder Zeichen an das Feld mit dem Fokus; true, wenn PDFium es verarbeitet hat. FFI_OnChange kommt erst, wenn
    /// PDFium den Wert übernimmt (Fokuswechsel) – Eingaben im Feld zählen deshalb gleich als Änderung.</summary>
    public bool FormKey(byte kind, int code, int modifiers) => OnFormPage(page => kind switch
    {
        Protocol.KeyDown => Pdfium.FormOnKeyDown(form, page, code, modifiers) != 0,
        Protocol.KeyUp => Pdfium.FormOnKeyUp(form, page, code, modifiers) != 0,
        _ => Pdfium.FormOnChar(form, page, code, modifiers) != 0,
    }, changes: kind == Protocol.KeyChar || (kind == Protocol.KeyDown && code == KeyDelete));

    public bool FormEdit(byte kind)
    {
        if (kind == Protocol.EditKillFocus)
        {
            if (form != 0) { _ = Pdfium.FormForceToKillFocus(form); } // PDFium übernimmt dabei den eingetippten Wert
            return true;
        }
        return OnFormPage(page => kind switch
        {
            Protocol.EditSelectAll => Pdfium.FormSelectAllText(form, page) != 0,
            Protocol.EditUndo => Pdfium.FormUndo(form, page) != 0,
            _ => Pdfium.FormRedo(form, page) != 0,
        }, changes: kind != Protocol.EditSelectAll);
    }

    public string FormSelectedText()
    {
        if (form == 0 || !loadedPages.TryGetValue(formPage, out var loaded)) { return string.Empty; }
        var bytes = Pdfium.FormGetSelectedText(form, loaded.Page, 0, 0); // UTF-16LE samt abschließender Null
        if (bytes <= 2) { return string.Empty; }
        var buffer = Marshal.AllocHGlobal((int)bytes);
        try
        {
            _ = Pdfium.FormGetSelectedText(form, loaded.Page, buffer, bytes);
            return Marshal.PtrToStringUni(buffer) ?? string.Empty;
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    public bool FormReplaceSelection(string text) => OnFormPage(page =>
    {
        fixed (char* chars = text) { Pdfium.FormReplaceSelection(form, page, (ushort*)chars); } // nullterminiert
        return true;
    }, changes: true);

    private bool OnFormPage(Func<nint, bool> action, bool changes)
    {
        var handled = form != 0 && loadedPages.TryGetValue(formPage, out var loaded) && action(loaded.Page);
        if (handled) { InvalidateFocusedField(); }
        if (handled && changes) { Events.Changed(Id); }
        return handled;
    }

    /// <summary>Meldet das Feld mit dem Fokus als neu zu zeichnen. PDFium ruft FFI_Invalidate nicht bei jeder Eingabe auf (bei Entf
    /// fehlte es) – die Änderung erschiene sonst erst beim nächsten Neuzeichnen.</summary>
    private void InvalidateFocusedField()
    {
        if (Pdfium.FormGetFocusedAnnot(form, out var pageIndex, out var annot) == 0 || annot == 0) { return; }
        try
        {
            if (Pdfium.AnnotGetRect(annot, out var rect) != 0 && loadedPages.TryGetValue(pageIndex, out var loaded))
            {
                OnFormInvalidate(loaded.Page, rect.Left, rect.Top, rect.Right, rect.Bottom);
            }
        }
        finally { Pdfium.PageCloseAnnot(annot); }
    }

    // ================================================================== Hervorhebungen

    private const uint HighlightOpacity = 102; // 40 % von 255 – Acrobats /CA 0,4

    /// <summary>Legt eine Hervorhebung (Highlight-Anmerkung) über einen Zeichenbereich der Seite: je Textzeile ein durchgehendes
    /// Viereck (<see cref="LineBoxes"/>), Farbe 0xRRGGBB mit 40 % Deckkraft wie Acrobat, druckbar. Die Darstellung (/AP, Multiplizieren
    /// statt Überdecken) erzeugt PDFium beim nächsten Rendern selbst und schreibt sie in die Anmerkung – gespeichert sehen andere
    /// Programme dieselbe Markierung.</summary>
    public bool AddHighlight(int index, int start, int count, uint color)
    {
        var lines = LineBoxes(index, start, count);
        if (lines.Count == 0) { return false; }
        var page = GetPage(index).Page;
        var annot = Pdfium.PageCreateAnnot(page, Pdfium.AnnotHighlight);
        if (annot == 0) { throw new InvalidOperationException("PDFium konnte die Hervorhebung nicht anlegen."); }
        try
        {
            // Deckkraft 40 % (/CA 0,4) wie Acrobat; PDFium trägt die Farbe in seiner Darstellung multiplizierend auf, die Schrift bleibt lesbar
            _ = Pdfium.AnnotSetColor(annot, Pdfium.AnnotColor, (color >> 16) & 0xFF, (color >> 8) & 0xFF, color & 0xFF, HighlightOpacity);
            _ = Pdfium.AnnotSetFlags(annot, Pdfium.AnnotFlagPrint);
            var bounds = new Pdfium.RectF { Left = float.MaxValue, Bottom = float.MaxValue, Right = float.MinValue, Top = float.MinValue };
            foreach (var (left, top, right, bottom) in lines) // je Zeile ein Viereck
            {
                var quad = new Pdfium.QuadPointsF
                {
                    X1 = (float)left, Y1 = (float)top, X2 = (float)right, Y2 = (float)top,
                    X3 = (float)left, Y3 = (float)bottom, X4 = (float)right, Y4 = (float)bottom,
                };
                _ = Pdfium.AnnotAppendAttachmentPoints(annot, &quad);
                bounds = new Pdfium.RectF
                {
                    Left = Math.Min(bounds.Left, (float)left), Right = Math.Max(bounds.Right, (float)right),
                    Bottom = Math.Min(bounds.Bottom, (float)bottom), Top = Math.Max(bounds.Top, (float)top),
                };
            }
            _ = Pdfium.AnnotSetRect(annot, &bounds);
        }
        finally { Pdfium.PageCloseAnnot(annot); }
        Events.Invalidate(Id, index, 0, 0, 1, 1); // die ganze Seite neu zeichnen
        Events.Changed(Id);
        return true;
    }

    /// <summary>Die Vierecke je Textzeile, die <see cref="AddHighlight"/> für den Zeichenbereich anlegen würde (PDF-Koordinaten) – für
    /// Hervorhebungen, deren Darstellung PDFlight selbst schreibt (weißer Marker: Umkehren statt Multiplizieren).</summary>
    public List<(double Left, double Top, double Right, double Bottom)> HighlightBoxes(int index, int start, int count) => LineBoxes(index, start, count);

    /// <summary>Nummer der obersten Textmarkierung (Hervorhebung, Unterstreichung, Wellenlinie, Durchstreichung) an einer Stelle
    /// (Seitenpixel); -1, wenn dort keine ist. Getroffen wird ein Viereck der Markierung, nicht ihr umschließendes Rechteck.</summary>
    /// <summary>Seitenpixel → PDF-Koordinaten der Seite (für das Einfügen von Anmerkungen an einer angeklickten Stelle).</summary>
    public (double X, double Y) PagePoint(int index, int pageWidth, int pageHeight, int x, int y)
    {
        var (_, px, py) = ToPagePoint(index, pageWidth, pageHeight, x, y);
        return (px, py);
    }

    /// <summary>Oberste Anmerkung einer der Arten in <paramref name="subtypes"/> (Bitmaske 1 &lt;&lt; FPDF_ANNOT_*) an einer Stelle
    /// (Seitenpixel) samt Art und Rechteck in Seitenpixeln; Nummer -1, wenn dort keine ist.</summary>
    public (int Number, int Subtype, (int X, int Y, int Width, int Height) Rect) AnnotAt(int index, int pageWidth, int pageHeight, int x, int y, int subtypes)
    {
        var (page, px, py) = ToPagePoint(index, pageWidth, pageHeight, x, y);
        for (var i = Pdfium.PageGetAnnotCount(page) - 1; i >= 0; i--) // zuletzt angelegte liegen oben
        {
            var annot = Pdfium.PageGetAnnot(page, i);
            if (annot == 0) { continue; }
            try
            {
                var subtype = Pdfium.AnnotGetSubtype(annot);
                if (subtype is < 0 or > 30 || (subtypes & (1 << subtype)) == 0 || Pdfium.AnnotGetRect(annot, out var r) == 0) { continue; }
                if ((Pdfium.AnnotGetFlags(annot) & Pdfium.AnnotFlagHidden) != 0 && !hiddenAnnots.ContainsKey((index, i))) { continue; } // unsichtbar
                var (left, right) = (Math.Min(r.Left, r.Right), Math.Max(r.Left, r.Right));
                var (bottom, top) = (Math.Min(r.Bottom, r.Top), Math.Max(r.Bottom, r.Top));
                if (px < left || px > right || py < bottom || py > top) { continue; }
                _ = Pdfium.PageToDevice(page, 0, 0, pageWidth, pageHeight, ViewRotation, left, top, out var x1, out var y1);
                _ = Pdfium.PageToDevice(page, 0, 0, pageWidth, pageHeight, ViewRotation, right, bottom, out var x2, out var y2);
                return (i, subtype, (Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1)));
            }
            finally { Pdfium.PageCloseAnnot(annot); }
        }
        return (-1, 0, (0, 0, 0, 0));
    }

    /// <summary>Freihand-Strich (Zeichnen) als Ink-Anmerkung: Punkte in Seitenpixeln (Ansichtsdrehung berücksichtigt), Farbe 0xRRGGBB,
    /// Strichstärke in Punkt (/Border). Die Darstellung erzeugt PDFium beim nächsten Rendern selbst, wie bei den Hervorhebungen.</summary>
    public bool AddInk(int index, int pageWidth, int pageHeight, uint color, float width, (int X, int Y)[] points)
    {
        var page = GetPage(index).Page;
        var pdf = new Pdfium.PointF[points.Length == 1 ? 2 : points.Length]; // ein einzelner Punkt wird ein Tupfer: zweimal derselbe
        for (var i = 0; i < points.Length; i++)
        {
            _ = Pdfium.DeviceToPage(page, 0, 0, pageWidth, pageHeight, ViewRotation, points[i].X, points[i].Y, out var x, out var y);
            pdf[i] = new Pdfium.PointF { X = (float)x, Y = (float)y };
        }
        if (points.Length == 1) { pdf[1] = pdf[0]; }
        var number = Pdfium.PageGetAnnotCount(page); // die neue Anmerkung kommt ans Ende von /Annots
        var annot = Pdfium.PageCreateAnnot(page, Pdfium.AnnotInk);
        if (annot == 0) { throw new InvalidOperationException("PDFium konnte den Strich nicht anlegen."); }
        var stroked = false;
        try
        {
            _ = Pdfium.AnnotSetColor(annot, Pdfium.AnnotColor, (color >> 16) & 0xFF, (color >> 8) & 0xFF, color & 0xFF, 255);
            _ = Pdfium.AnnotSetFlags(annot, Pdfium.AnnotFlagPrint);
            _ = Pdfium.AnnotSetBorder(annot, 0, 0, width);
            fixed (Pdfium.PointF* first = pdf) { stroked = Pdfium.AnnotAddInkStroke(annot, first, (nuint)pdf.Length) >= 0; }
            if (!stroked) { return false; }
            var half = width / 2 + 1;
            var bounds = new Pdfium.RectF
            {
                Left = pdf.Min(p => p.X) - half, Right = pdf.Max(p => p.X) + half,
                Bottom = pdf.Min(p => p.Y) - half, Top = pdf.Max(p => p.Y) + half,
            };
            _ = Pdfium.AnnotSetRect(annot, &bounds);
        }
        finally
        {
            Pdfium.PageCloseAnnot(annot);
            if (!stroked) { _ = Pdfium.PageRemoveAnnot(page, number); } // keine leere Ink-Anmerkung zurücklassen (Review 04.10.2026)
        }
        Events.Invalidate(Id, index, 0, 0, 1, 1);
        Events.Changed(Id);
        return true;
    }

    private readonly Dictionary<(int Page, int Number), int> hiddenAnnots = []; // nur in der Anzeige ausgeblendet: ursprüngliche Flags

    /// <summary>Blendet eine Anmerkung in der Anzeige aus bzw. wieder ein (Hidden-Flag; PDFium liest die Flags bei jedem Rendern neu).</summary>
    public bool SetAnnotHidden(int index, int number, bool hidden)
    {
        var page = GetPage(index).Page;
        if (number < 0 || number >= Pdfium.PageGetAnnotCount(page)) { return false; }
        var annot = Pdfium.PageGetAnnot(page, number);
        if (annot == 0) { return false; }
        try
        {
            if (hidden && !hiddenAnnots.ContainsKey((index, number)))
            {
                var flags = Pdfium.AnnotGetFlags(annot);
                hiddenAnnots[(index, number)] = flags;
                return Pdfium.AnnotSetFlags(annot, flags | Pdfium.AnnotFlagHidden) != 0;
            }
            if (!hidden && hiddenAnnots.Remove((index, number), out var original)) { return Pdfium.AnnotSetFlags(annot, original) != 0; }
            return true;
        }
        finally { Pdfium.PageCloseAnnot(annot); }
    }

    public int MarkupAt(int index, int pageWidth, int pageHeight, int x, int y)
    {
        var (page, px, py) = ToPagePoint(index, pageWidth, pageHeight, x, y);
        for (var i = Pdfium.PageGetAnnotCount(page) - 1; i >= 0; i--) // zuletzt angelegte liegen oben
        {
            var annot = Pdfium.PageGetAnnot(page, i);
            if (annot == 0) { continue; }
            try
            {
                if (Pdfium.AnnotGetSubtype(annot) is < Pdfium.AnnotHighlight or > Pdfium.AnnotStrikeOut) { continue; }
                var count = Pdfium.AnnotCountAttachmentPoints(annot);
                for (nuint j = 0; j < count; j++)
                {
                    Pdfium.QuadPointsF quad;
                    if (Pdfium.AnnotGetAttachmentPoints(annot, j, &quad) == 0) { continue; }
                    var (left, right) = (Math.Min(Math.Min(quad.X1, quad.X2), Math.Min(quad.X3, quad.X4)), Math.Max(Math.Max(quad.X1, quad.X2), Math.Max(quad.X3, quad.X4)));
                    var (bottom, top) = (Math.Min(Math.Min(quad.Y1, quad.Y2), Math.Min(quad.Y3, quad.Y4)), Math.Max(Math.Max(quad.Y1, quad.Y2), Math.Max(quad.Y3, quad.Y4)));
                    if (px >= left && px <= right && py >= bottom && py <= top) { return i; }
                }
            }
            finally { Pdfium.PageCloseAnnot(annot); }
        }
        return -1;
    }

    /// <summary>Oberste Zeichnung (Ink), deren Linie die Stelle (Seitenpixel) trifft: Abstand zum nächsten Streckenstück höchstens halbe
    /// Strichstärke plus 4 Pixel – ein Klick in das Rechteck neben die Linie zählt nicht. -1 = keine.</summary>
    public int InkAt(int index, int pageWidth, int pageHeight, int x, int y)
    {
        var (page, px, py) = ToPagePoint(index, pageWidth, pageHeight, x, y);
        var pixelsPerPoint = Math.Max(pageWidth, pageHeight) / Math.Max(1f, Math.Max(Pdfium.GetPageWidthF(page), Pdfium.GetPageHeightF(page)));
        var slack = 4 / Math.Max(0.01, pixelsPerPoint); // 4 Pixel Spielraum, in Punkt
        for (var i = Pdfium.PageGetAnnotCount(page) - 1; i >= 0; i--) // zuletzt angelegte liegen oben
        {
            var annot = Pdfium.PageGetAnnot(page, i);
            if (annot == 0) { continue; }
            try
            {
                if (Pdfium.AnnotGetSubtype(annot) != Pdfium.AnnotInk) { continue; }
                var width = Pdfium.AnnotGetBorder(annot, out _, out _, out var w) != 0 && w > 0 ? w : 1f;
                var limit = width / 2 + slack;
                for (uint path = 0; path < Pdfium.AnnotGetInkListCount(annot); path++)
                {
                    var count = Pdfium.AnnotGetInkListPath(annot, path, null, 0);
                    if (count == 0 || count > 1_000_000) { continue; }
                    var points = new Pdfium.PointF[count];
                    fixed (Pdfium.PointF* first = points) { _ = Pdfium.AnnotGetInkListPath(annot, path, first, count); }
                    for (var k = 0; k < points.Length; k++)
                    {
                        var (a, b) = (points[k], points[Math.Min(k + 1, points.Length - 1)]);
                        if (SegmentDistance(px, py, a.X, a.Y, b.X, b.Y) <= limit) { return i; }
                    }
                }
            }
            finally { Pdfium.PageCloseAnnot(annot); }
        }
        return -1;
    }

    /// <summary>Abstand eines Punkts zur Strecke a–b.</summary>
    private static double SegmentDistance(double px, double py, double ax, double ay, double bx, double by)
    {
        var (dx, dy) = (bx - ax, by - ay);
        var lengthSquared = dx * dx + dy * dy;
        var t = lengthSquared < 1e-9 ? 0 : Math.Clamp(((px - ax) * dx + (py - ay) * dy) / lengthSquared, 0, 1);
        var (cx, cy) = (ax + t * dx - px, ay + t * dy - py);
        return Math.Sqrt(cx * cx + cy * cy);
    }

    /// <summary>Entfernt eine Textmarkierung, Zeichnung, einen Stempel oder Freitext (Nummer aus <see cref="MarkupAt"/>, <see cref="InkAt"/> bzw.
    /// <see cref="AnnotAt"/>) samt angehängtem Popup – das bliebe sonst verwaist auf der Seite; andere Anmerkungsarten bleiben unberührt.</summary>
    public bool RemoveMarkup(int index, int number)
    {
        var page = GetPage(index).Page;
        if (number < 0 || number >= Pdfium.PageGetAnnotCount(page)) { return false; }
        var annot = Pdfium.PageGetAnnot(page, number);
        if (annot == 0) { return false; }
        int subtype, popup = -1;
        try
        {
            subtype = Pdfium.AnnotGetSubtype(annot);
            var linked = Pdfium.AnnotGetLinkedAnnot(annot, "Popup");
            if (linked != 0)
            {
                try { popup = Pdfium.PageGetAnnotIndex(page, linked); }
                finally { Pdfium.PageCloseAnnot(linked); }
            }
        }
        finally { Pdfium.PageCloseAnnot(annot); }
        if (subtype is not ((>= Pdfium.AnnotHighlight and <= Pdfium.AnnotStrikeOut) or Pdfium.AnnotInk or Pdfium.AnnotStamp or Pdfium.AnnotFreeText)) { return false; }
        if (popup > number && Pdfium.PageRemoveAnnot(page, popup) == 0) { popup = -1; } // die höhere Nummer zuerst – sonst rückte die andere nach vorn
        if (Pdfium.PageRemoveAnnot(page, number) == 0) { return false; }
        if (popup >= 0 && popup < number) { _ = Pdfium.PageRemoveAnnot(page, popup); }
        Events.Invalidate(Id, index, 0, 0, 1, 1);
        Events.Changed(Id);
        return true;
    }

    // ================================================================== Speichern und Druckkopie

    /// <summary>Das Dokument samt eingetragener Werte als vollständige PDF-Datei (der Fokus wird vorher aus dem Feld genommen).</summary>
    public byte[] Save(bool removeSecurity)
    {
        if (form != 0) { _ = Pdfium.FormForceToKillFocus(form); }
        return FormFillInfo.SaveToBytes(document, removeSecurity ? Pdfium.SaveRemoveSecurity : Pdfium.SaveNoIncremental);
    }

    /// <summary>Berechtigungsbits (/P) und Revision des Sicherheits-Handlers – für den Eigenschaften-Dialog. Die Benutzer-Berechtigungen, also /P der
    /// Datei, auch wenn sie mit dem Besitzerkennwort geöffnet wurde (FPDF_GetDocPermissions meldete dann alles erlaubt).</summary>
    public (uint Permissions, int Revision) Security() => (Pdfium.GetDocUserPermissions(document), Pdfium.GetSecurityHandlerRevision(document));

    /// <summary>Eingebettete Dateien (Namensbaum /EmbeddedFiles): Name und Größe (-1, wenn PDFium sie nicht liefern kann).</summary>
    public List<(string Name, long Size)> Attachments()
    {
        var result = new List<(string, long)>();
        var count = Pdfium.DocGetAttachmentCount(document);
        for (var i = 0; i < count; i++)
        {
            var attachment = Pdfium.DocGetAttachment(document, i);
            if (attachment == 0) { result.Add(("?", -1)); continue; }
            result.Add((AttachmentName(attachment), Pdfium.AttachmentGetFile(attachment, null, 0, out var size) != 0 ? size : -1));
        }
        return result;
    }

    /// <summary>Inhalt einer eingebetteten Datei.</summary>
    public byte[] AttachmentData(int index)
    {
        if (index < 0 || index >= Pdfium.DocGetAttachmentCount(document)) { throw new InvalidOperationException("Diese eingebettete Datei gibt es nicht."); }
        var attachment = Pdfium.DocGetAttachment(document, index);
        if (attachment == 0 || Pdfium.AttachmentGetFile(attachment, null, 0, out var size) == 0) { throw new InvalidOperationException("PDFium konnte die eingebettete Datei nicht lesen."); }
        var data = new byte[size];
        fixed (byte* buffer = data)
        {
            if (size > 0 && Pdfium.AttachmentGetFile(attachment, buffer, size, out _) == 0) { throw new InvalidOperationException("PDFium konnte die eingebettete Datei nicht lesen."); }
        }
        return data;
    }

    private static string AttachmentName(nint attachment)
    {
        var length = Pdfium.AttachmentGetName(attachment, null, 0); // Bytes in UTF-16LE samt abschließender Null
        if (length <= 2) { return string.Empty; }
        var buffer = new byte[length];
        fixed (byte* first = buffer) { _ = Pdfium.AttachmentGetName(attachment, first, length); }
        return System.Text.Encoding.Unicode.GetString(buffer, 0, (int)length - 2);
    }

    /// <summary>PDF-Version aus der Kopfzeile (z. B. 17; 0 = unbekannt) und Revision des Sicherheits-Handlers (-1 = nicht verschlüsselt).</summary>
    public (int Version, int SecurityRevision) Info() =>
        (Pdfium.GetFileVersion(document, out var version) != 0 ? version : 0, Pdfium.GetSecurityHandlerRevision(document));

    /// <summary>Für den Druck eines Formulars: eine Kopie, in der die Felder der Seiten first bis last mit ihren aktuellen Werten fest
    /// auf der Seite stehen (wie Chromium: Seiten importieren, FPDFPage_Flatten, speichern, neu laden). Gleiche Seiten, gleiche Reihenfolge.</summary>
    public PdfDocument PrintCopy(int newId, int first, int last)
    {
        if (form != 0) { _ = Pdfium.FormForceToKillFocus(form); }
        var copy = Pdfium.CreateNewDocument();
        try
        {
            if (Pdfium.ImportPages(copy, document, 0, 0) == 0) { throw new InvalidOperationException("PDFium konnte die Seiten nicht kopieren."); }
            for (var i = Math.Max(0, first); form != 0 && i <= Math.Min(last, PageSizes.Length - 1); i++) // ohne Formular nichts einzubrennen
            {
                // scheitert es, nicht still weiterdrucken – die Seite käme sonst ohne Formularwerte aufs Papier
                var page = Pdfium.LoadPage(copy, i);
                if (page == 0) { throw new InvalidOperationException($"PDFium konnte Seite {i + 1} nicht für den Druck laden."); }
                var result = Pdfium.FlattenPage(page, Pdfium.FlattenForPrint);
                Pdfium.ClosePage(page);
                if (result == Pdfium.FlattenFailed) { throw new InvalidOperationException($"PDFium konnte die Formularfelder auf Seite {i + 1} nicht für den Druck übernehmen."); }
            }
            return new PdfDocument(newId, FormFillInfo.SaveToBytes(copy)); // eingebrannt gilt erst nach dem Neuladen
        }
        finally { Pdfium.CloseDocument(copy); }
    }

    public void Dispose()
    {
        foreach (var loaded in loadedPages.Values) { ClosePage(loaded); }
        loadedPages.Clear();
        if (form != 0) { Pdfium.ExitFormFillEnvironment(form); form = 0; } // nach den Seiten, vor dem Dokument
        if (formInfo != 0) { FormFillInfo.Free(formInfo); formInfo = 0; }
        if (document != 0) { Pdfium.CloseDocument(document); document = 0; }
        if (pin.IsAllocated) { pin.Free(); }
    }
}

/// <summary>Die Datei ist kennwortgeschützt, und das Kennwort fehlte oder war falsch.</summary>
internal sealed class PasswordNeededException() : InvalidOperationException("Die Datei ist kennwortgeschützt.");
