namespace PDFLight.Viewer;

/// <summary>Ziel eines Links oder Lesezeichens: eine Seite (0-basiert) mit optionaler Höhe in PDF-Punkten (von unten gemessen, wie
/// in /XYZ) – oder eine Webadresse (dann Page = -1).</summary>
internal sealed record LinkTarget(int Page, float? TopPt, string? Uri)
{
    /// <summary>Kurzbeschreibung für die Statusleiste.</summary>
    public string Description => Uri ?? $"Seite {Page + 1}";
}

/// <summary>Ein Lesezeichen mit Titel, Ziel (kann fehlen) und Unterpunkten.</summary>
internal sealed class OutlineItem(string title, LinkTarget? target)
{
    public string Title { get; } = title;
    public LinkTarget? Target { get; } = target;
    public List<OutlineItem> Children { get; } = [];
}
