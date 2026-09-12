namespace PDFLight.Classes;

/// <summary>Ein Favorit: eine PDF-Datei, wahlweise mit eigenem Namen. Gespeichert in settings.json;
/// die Datei wird über ihren vollen Pfad identifiziert.</summary>
public sealed class Favorite
{
    public string File { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Menütext: der eigene Name, sonst der Dateiname.</summary>
    public string Label => string.IsNullOrWhiteSpace(Name) ? Path.GetFileName(File) : Name;

    public bool IsFor(string file) => string.Equals(File, file, StringComparison.OrdinalIgnoreCase);
}
