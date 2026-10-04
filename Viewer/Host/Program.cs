namespace PDFLight.Host;

/// <summary>Hilfsprogramm: führt PDFium in einem abgeschotteten Prozess aus (AppContainer, s. SandboxProcess). Es liest keine Dateien –
/// die PDF-Bytes kommen über die Standard-Eingabe vom Hauptprogramm, Bilder, Texte und EMF-Daten gehen über die Standard-Ausgabe zurück
/// (Befehle s. Protocol). Ein Fehler in PDFium, den eine präparierte Datei ausnutzt, bleibt damit in einem Prozess ohne Zugriff auf
/// Dateien, Netz und Registry eingesperrt. Alles läuft auf einem Thread, nacheinander – PDFium ist ohnehin nicht thread-sicher.</summary>
internal static class Program
{
    private static readonly Dictionary<int, PdfDocument> Documents = [];
    private static int nextDocumentId = 1;

    private static int Main()
    {
        Pdfium.InitLibrary(); // lädt pdfium.dll – erst danach kennt der Absturz-Handler ihren Adressbereich
        CrashGuard.Install();
        using var input = new BinaryReader(Console.OpenStandardInput());
        using var output = new BinaryWriter(new BufferedStream(Console.OpenStandardOutput(), 1 << 16));
        while (true)
        {
            byte command;
            try { command = input.ReadByte(); }
            catch (EndOfStreamException) { return 0; } // Hauptprogramm beendet oder Pipe geschlossen
            if (command == Protocol.Quit) { return 0; }
            // Jeder Befehl liest zuerst alle Argumente, rechnet dann und liefert erst danach, was zu schreiben ist – ein Fehler lässt die
            // Antwort nie halb stehen. Ereignisse aus PDFium gehen vor der Antwort hinaus.
            Action<BinaryWriter> reply;
            try { reply = Execute(command, input); }
            catch (Exception ex) when (ex is not EndOfStreamException and not IOException)
            {
                Events.Flush(output);
                output.Write(Protocol.Error);
                output.Write(ex.Message);
                output.Flush();
                continue;
            }
            Events.Flush(output);
            output.Write(Protocol.Ok);
            reply(output);
            output.Flush();
        }
    }

    private static PdfDocument Document(int id) =>
        Documents.TryGetValue(id, out var document) ? document : throw new InvalidOperationException($"Dokument {id} ist nicht geöffnet.");

    private static Action<BinaryWriter> Execute(byte command, BinaryReader input)
    {
        switch (command)
        {
            case Protocol.Hello:
                {
                    var report = Diagnostics.Report();
                    return w => w.Write(report);
                }
            case Protocol.Probe:
                {
                    var (kind, target) = (input.ReadString(), input.ReadString());
                    var result = Diagnostics.Probe(kind, target);
                    return w => w.Write(result);
                }
            case Protocol.Crash:
                {
                    Diagnostics.Crash(input.ReadByte());
                    return w => { };
                }

            // ================================================================== Dokument

            case Protocol.Open:
                {
                    var (bytes, password) = (input.ReadBytes(input.ReadInt32()), input.ReadString());
                    try { return Register(new PdfDocument(nextDocumentId, bytes, password)); }
                    catch (PasswordNeededException) { return w => w.Write(Protocol.PasswordNeeded); } // kein Fehler: das Hauptprogramm fragt nach
                }
            case Protocol.Close:
                {
                    var id = input.ReadInt32();
                    if (Documents.Remove(id, out var document)) { document.Dispose(); }
                    return w => { };
                }
            case Protocol.Save:
                {
                    var (id, removeSecurity) = (input.ReadInt32(), input.ReadBoolean());
                    var bytes = Document(id).Save(removeSecurity);
                    return w => { w.Write(bytes.Length); w.Write(bytes); };
                }
            case Protocol.Security:
                {
                    var (permissions, revision) = Document(input.ReadInt32()).Security();
                    return w => { w.Write(permissions); w.Write(revision); };
                }
            case Protocol.Attachments:
                {
                    var attachments = Document(input.ReadInt32()).Attachments();
                    return w => { w.Write(attachments.Count); foreach (var (name, size) in attachments) { w.Write(name); w.Write(size); } };
                }
            case Protocol.AttachmentData:
                {
                    var (id, index) = (input.ReadInt32(), input.ReadInt32());
                    var data = Document(id).AttachmentData(index);
                    return w => { w.Write(data.Length); w.Write(data); };
                }
            case Protocol.HighlightBoxes:
                {
                    var (id, page, start, count) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var boxes = Document(id).HighlightBoxes(page, start, count);
                    return w => { w.Write(boxes.Count); foreach (var (left, top, right, bottom) in boxes) { w.Write(left); w.Write(top); w.Write(right); w.Write(bottom); } };
                }
            case Protocol.Info:
                {
                    var (version, securityRevision) = Document(input.ReadInt32()).Info();
                    return w => { w.Write(version); w.Write(securityRevision); };
                }
            case Protocol.PrintCopy:
                {
                    var (id, first, last) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    return Register(Document(id).PrintCopy(nextDocumentId, first, last));
                }

            // ================================================================== Darstellung

            case Protocol.Render:
                {
                    var (id, page, pageWidth, pageHeight) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var (x, y, width, height) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var pixels = Document(id).Render(page, pageWidth, pageHeight, x, y, width, height);
                    return w => w.Write(pixels);
                }
            case Protocol.SetRotation:
                {
                    var (id, rotation) = (input.ReadInt32(), input.ReadInt32());
                    Document(id).ViewRotation = rotation & 3;
                    return w => { };
                }
            case Protocol.RenderEmf:
                {
                    var (id, page, width, height) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var emf = Document(id).RenderEmf(page, width, height);
                    return w => { w.Write(emf.Length); w.Write(emf); };
                }

            // ================================================================== Text

            case Protocol.CharCount:
                {
                    var (id, page) = (input.ReadInt32(), input.ReadInt32());
                    var count = Document(id).CharCount(page);
                    return w => w.Write(count);
                }
            case Protocol.CharAt:
                {
                    var (id, page, pageWidth, pageHeight) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var (x, y, tolerance) = (input.ReadInt32(), input.ReadInt32(), input.ReadDouble());
                    var index = Document(id).CharAt(page, pageWidth, pageHeight, x, y, tolerance);
                    return w => w.Write(index);
                }
            case Protocol.NearestChar:
                {
                    var (id, page, pageWidth, pageHeight) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var (x, y) = (input.ReadInt32(), input.ReadInt32());
                    var index = Document(id).NearestChar(page, pageWidth, pageHeight, x, y);
                    return w => w.Write(index);
                }
            case Protocol.WordAt:
                {
                    var (id, page, charIndex) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var (start, count) = Document(id).WordAt(page, charIndex);
                    return w => { w.Write(start); w.Write(count); };
                }
            case Protocol.SelectionRects:
                {
                    var (id, page, pageWidth, pageHeight) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var (start, count) = (input.ReadInt32(), input.ReadInt32());
                    var rects = Document(id).SelectionRects(page, pageWidth, pageHeight, start, count);
                    return w =>
                    {
                        w.Write(rects.Count);
                        foreach (var (x, y, width, height) in rects) { w.Write(x); w.Write(y); w.Write(width); w.Write(height); }
                    };
                }
            case Protocol.Text:
                {
                    var (id, page, start, count) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var text = Document(id).Text(page, start, count);
                    return w => w.Write(text);
                }
            case Protocol.SearchPage:
                {
                    var (id, page, query, matchCase, wholeWord) = (input.ReadInt32(), input.ReadInt32(), input.ReadString(), input.ReadBoolean(), input.ReadBoolean());
                    var hits = Document(id).SearchPage(page, query, matchCase, wholeWord);
                    return w =>
                    {
                        w.Write(hits.Count);
                        foreach (var (start, count) in hits) { w.Write(start); w.Write(count); }
                    };
                }

            // ================================================================== Links und Lesezeichen

            case Protocol.LinkAt:
                {
                    var (id, page, pageWidth, pageHeight) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var (x, y) = (input.ReadInt32(), input.ReadInt32());
                    var target = Document(id).LinkAt(page, pageWidth, pageHeight, x, y);
                    return w =>
                    {
                        w.Write(target != null);
                        if (target != null) { Protocol.WriteTarget(w, target.Page, target.Top, target.Uri); }
                    };
                }
            case Protocol.Outline:
                {
                    var outline = Document(input.ReadInt32()).Outline();
                    return w => WriteOutline(w, outline);
                }

            // ================================================================== Formulare

            case Protocol.FormMouse:
                {
                    var (id, current, kind) = (input.ReadInt32(), input.ReadInt32(), input.ReadByte());
                    var (page, pageWidth, pageHeight) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var (x, y, modifiers) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var document = Document(id);
                    document.CurrentPage = current;
                    var result = document.FormMouse(kind, page, pageWidth, pageHeight, x, y, modifiers);
                    return w => w.Write(result);
                }
            case Protocol.FormKey:
                {
                    var (id, current, kind, code, modifiers) = (input.ReadInt32(), input.ReadInt32(), input.ReadByte(), input.ReadInt32(), input.ReadInt32());
                    var document = Document(id);
                    document.CurrentPage = current;
                    var handled = document.FormKey(kind, code, modifiers);
                    return w => w.Write(handled);
                }
            case Protocol.FormEdit:
                {
                    var (id, current, kind) = (input.ReadInt32(), input.ReadInt32(), input.ReadByte());
                    var document = Document(id);
                    document.CurrentPage = current;
                    var handled = document.FormEdit(kind);
                    return w => w.Write(handled);
                }
            case Protocol.FormSelectedText:
                {
                    var (id, current) = (input.ReadInt32(), input.ReadInt32());
                    var document = Document(id);
                    document.CurrentPage = current;
                    var text = document.FormSelectedText();
                    return w => w.Write(text);
                }
            case Protocol.FormReplaceSelection:
                {
                    var (id, current, text) = (input.ReadInt32(), input.ReadInt32(), input.ReadString());
                    var document = Document(id);
                    document.CurrentPage = current;
                    var handled = document.FormReplaceSelection(text);
                    return w => w.Write(handled);
                }

            // ================================================================== Hervorhebungen

            case Protocol.AddHighlight:
                {
                    var (id, page, start, count, color) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var added = Document(id).AddHighlight(page, start, count, (uint)color);
                    return w => w.Write(added);
                }
            case Protocol.PagePoint:
                {
                    var (id, page, pageWidth, pageHeight) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var (x, y) = (input.ReadInt32(), input.ReadInt32());
                    var (px, py) = Document(id).PagePoint(page, pageWidth, pageHeight, x, y);
                    return w => { w.Write(px); w.Write(py); };
                }
            case Protocol.AnnotAt:
                {
                    var (id, page, pageWidth, pageHeight) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var (x, y, subtypes) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var (number, subtype, rect) = Document(id).AnnotAt(page, pageWidth, pageHeight, x, y, subtypes);
                    return w => { w.Write(number); w.Write(subtype); w.Write(rect.X); w.Write(rect.Y); w.Write(rect.Width); w.Write(rect.Height); };
                }
            case Protocol.AddInk:
                {
                    var (id, page, pageWidth, pageHeight) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var (color, width, count) = (input.ReadInt32(), input.ReadSingle(), input.ReadInt32());
                    if (count is < 1 or > 100_000) { throw new InvalidOperationException("Ungültige Punktzahl."); }
                    var points = new (int X, int Y)[count];
                    for (var i = 0; i < count; i++) { points[i] = (input.ReadInt32(), input.ReadInt32()); }
                    var added = Document(id).AddInk(page, pageWidth, pageHeight, (uint)color, width, points);
                    return w => w.Write(added);
                }
            case Protocol.SetAnnotHidden:
                {
                    var (id, page, number, hidden) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadBoolean());
                    var done = Document(id).SetAnnotHidden(page, number, hidden);
                    return w => w.Write(done);
                }
            case Protocol.MarkupAt:
                {
                    var (id, page, pageWidth, pageHeight) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var (x, y) = (input.ReadInt32(), input.ReadInt32());
                    var number = Document(id).MarkupAt(page, pageWidth, pageHeight, x, y);
                    return w => w.Write(number);
                }
            case Protocol.InkAt:
                {
                    var (id, page, pageWidth, pageHeight) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var (x, y) = (input.ReadInt32(), input.ReadInt32());
                    var number = Document(id).InkAt(page, pageWidth, pageHeight, x, y);
                    return w => w.Write(number);
                }
            case Protocol.RemoveMarkup:
                {
                    var (id, page, number) = (input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                    var removed = Document(id).RemoveMarkup(page, number);
                    return w => w.Write(removed);
                }
            default:
                throw new InvalidOperationException($"Unbekannter Befehl {command}.");
        }
    }

    /// <summary>Nimmt ein frisch geöffnetes Dokument auf; Antwort wie bei <see cref="Protocol.Open"/>.</summary>
    private static Action<BinaryWriter> Register(PdfDocument document)
    {
        Documents[document.Id] = document;
        nextDocumentId++;
        return w =>
        {
            w.Write(document.Id);
            w.Write(document.HasForms);
            w.Write(document.PageSizes.Length);
            foreach (var (width, height) in document.PageSizes) { w.Write(width); w.Write(height); }
        };
    }

    private static void WriteOutline(BinaryWriter writer, List<PdfDocument.OutlineNode> nodes)
    {
        writer.Write(nodes.Count);
        foreach (var node in nodes)
        {
            writer.Write(node.Title);
            writer.Write(node.Target != null);
            if (node.Target is { } target) { Protocol.WriteTarget(writer, target.Page, target.Top, target.Uri); }
            WriteOutline(writer, node.Children);
        }
    }
}
