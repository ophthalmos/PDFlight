namespace PDFLight.Classes;

/// <summary>Ein Favorit: eine Seite einer PDF-Datei, wahlweise mit eigenem Namen (wie in SumatraPDF und PDFMover).
/// Gespeichert in settings.json; die Datei wird über ihren vollen Pfad identifiziert.</summary>
public sealed class Favorite
{
    public string File { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public string Name { get; set; } = string.Empty;

    /// <summary>Menütext: „Seite 5“ oder „Name (Seite 5)“.</summary>
    public string Label
    {
        get
        {
            var page = string.Format(Lng.T("Seite {0}"), Page);
            return string.IsNullOrWhiteSpace(Name) ? page : $"{Name} ({page})";
        }
    }

    public bool IsFor(string file) => string.Equals(File, file, StringComparison.OrdinalIgnoreCase);
}
