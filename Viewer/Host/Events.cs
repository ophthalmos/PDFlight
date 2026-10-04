namespace PDFLight.Host;

/// <summary>Ereignisse, die PDFium während eines Befehls auslöst (Formular: Bereich neu zeichnen, Wert geändert). Sie werden gesammelt
/// und vor der Antwort auf den Befehl geschrieben – das Hilfsprogramm schreibt nie von sich aus, nur als Antwort.</summary>
internal static class Events
{
    private static readonly List<Action<BinaryWriter>> Pending = [];
    private static readonly HashSet<int> ChangedDocuments = [];

    public static void Invalidate(int document, int page, float left, float top, float right, float bottom) => Pending.Add(w =>
    {
        w.Write(Protocol.EventInvalidate);
        w.Write(document);
        w.Write(page);
        w.Write(left);
        w.Write(top);
        w.Write(right);
        w.Write(bottom);
    });

    public static void Changed(int document)
    {
        if (!ChangedDocuments.Add(document)) { return; } // je Befehl einmal genügt
        Pending.Add(w =>
        {
            w.Write(Protocol.EventChanged);
            w.Write(document);
        });
    }

    public static void Flush(BinaryWriter writer)
    {
        foreach (var write in Pending) { write(writer); }
        Clear();
    }

    public static void Clear()
    {
        Pending.Clear();
        ChangedDocuments.Clear();
    }
}
