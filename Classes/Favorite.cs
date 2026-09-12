namespace PDFLight.Classes;

/// <summary>Ein Favorit: eine PDF-Datei, wahlweise mit eigenem Namen. Gespeichert in settings.json;
/// die Datei wird über ihren vollen Pfad identifiziert.</summary>
public sealed class Favorite
{
    /// <summary>Längere Pfade werden im Menü in der Mitte gekürzt; Namen bleiben vollständig.</summary>
    public const int MaxPathLength = 60;

    public string File { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Der eigene Name, sonst der Dateiname – zum Sortieren und für die Rückfragen.</summary>
    public string Label => string.IsNullOrWhiteSpace(Name) ? Path.GetFileName(File) : Name;

    /// <summary>Menütext: „Name (Pfad)“ – bei eigenem Namen mit dem vollen Dateipfad, sonst Dateiname mit Ordner.</summary>
    public string MenuText
    {
        get
        {
            var named = !string.IsNullOrWhiteSpace(Name);
            var location = named ? File : Path.GetDirectoryName(File) ?? File;
            return $"{Label} ({FileUtil.ShortenMiddle(location, MaxPathLength)})";
        }
    }

    public bool IsFor(string file) => string.Equals(File, file, StringComparison.OrdinalIgnoreCase);
}
