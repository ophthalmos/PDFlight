using System.Text.Json;

namespace PDFLight.Classes;

/// <summary>Programmeinstellungen, gespeichert als JSON unter %APPDATA%\PDFLight\settings.json.</summary>
public class AppSettings
{
    public const int MaxRecentFolders = 16;

    public List<string> TargetFolders { get; set; } = [];
    public List<string> RecentFolders { get; set; } = [];
    public List<string> ExternalPrograms { get; set; } = []; // wird beim ersten Start automatisch gefüllt (ProgramFinder)
    public bool AlphabeticSort { get; set; } = true;      // Zielliste alphabetisch sortiert anzeigen
    public bool JumpToLastUsed { get; set; } = true;      // Ordnerdialog springt zum zuletzt verwendeten Ordner
    public bool ConfirmDelete { get; set; } = true;       // vor dem Verschieben in den Papierkorb nachfragen
    public bool ShowProgramIcons { get; set; } = true;    // Symbole der externen Programme zusätzlich in der Symbolleiste
    public bool ShowToolbarIcons { get; set; } = true;    // Symbole auf den Schaltflächen der Symbolleiste
    public bool LargeToolbarIcons { get; set; } = true;   // 24 statt 16 Pixel (vor DPI-Skalierung)
    public int WindowX { get; set; } = -1;
    public int WindowY { get; set; } = -1;
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
    public bool WindowMaximized { get; set; }

    private static string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PDFLight", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath)) ?? new AppSettings();
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { } // defekte Datei → Standardwerte
        return new AppSettings();
    }

    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true }; // gecacht (CA1869)

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, SerializerOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { } // Speichern darf das Programm nie blockieren
    }

    /// <summary>Trägt einen Ordner vorne in die Zuletzt-Liste ein (ohne Duplikate, begrenzte Länge).</summary>
    public void AddRecentFolder(string path)
    {
        RecentFolders.RemoveAll(x => string.Equals(x, path, StringComparison.OrdinalIgnoreCase));
        RecentFolders.Insert(0, path);
        if (RecentFolders.Count > MaxRecentFolders) { RecentFolders.RemoveRange(MaxRecentFolders, RecentFolders.Count - MaxRecentFolders); }
    }
}
