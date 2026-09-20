namespace PDFLight.Classes;

/// <summary>Ein Lesezeichen (Gliederungseintrag) für den experimentellen Editor: Titel, Zielseite (1-basiert; 0 = Ziel nicht
/// auflösbar), Unterpunkte und ob der Eintrag aufgeklappt ist. <see cref="Id"/> ist die Position im Vorordnungs-Durchlauf der
/// gelesenen Gliederung (-1 = neu angelegt) – darüber findet <c>PdfEditService.WriteOutlines</c> das ursprüngliche Ziel samt
/// Farbe und Schriftstil wieder, solange die Zielseite (<see cref="OriginalPage"/>) unverändert bleibt.</summary>
public sealed class Bookmark
{
    public string Title { get; set; } = string.Empty;
    public int Page { get; set; }
    public bool Open { get; set; }
    public List<Bookmark> Children { get; } = [];
    public int Id { get; set; } = -1;
    public int OriginalPage { get; set; }
}
