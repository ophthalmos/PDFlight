using System.Text.Json;

namespace PDFLight.Classes;

/// <summary>Programmeinstellungen, gespeichert als JSON unter %APPDATA%\PDFlight\settings.json.</summary>
public class AppSettings
{
    public const int RecentListLimit = 50; // Obergrenze der einstellbaren Zuletzt-Listen; 0 schaltet die jeweilige Liste ab
    public const int FavoritesLimit = 200;  // Obergrenze der Favoriten (Menü bleibt handhabbar)
    private int maxRecentFolders = 10;
    private int maxRecentFiles = 20;

    /// <summary>Höchstzahl der Einträge der Zuletzt-Liste im Ordnerdialog (dort einstellbar; 0 = nichts merken).</summary>
    public int MaxRecentFolders
    {
        get => maxRecentFolders;
        set => maxRecentFolders = Math.Clamp(value, 0, RecentListLimit);
    }

    /// <summary>Höchstzahl der zuletzt geöffneten Dateien im Öffnen-Menü (Einstellungen; 0 = nichts merken).</summary>
    public int MaxRecentFiles
    {
        get => maxRecentFiles;
        set => maxRecentFiles = Math.Clamp(value, 0, RecentListLimit);
    }

    /// <summary>Kürzt beide Zuletzt-Listen auf ihre Höchstzahl – nach dem Laden (die Reihenfolge der JSON-Felder ist
    /// nicht garantiert) und nachdem eine Höchstzahl gesenkt wurde.</summary>
    public void TrimRecentLists()
    {
        Trim(RecentFolders, MaxRecentFolders);
        Trim(RecentFiles, MaxRecentFiles);
    }

    private static void Trim(List<string> list, int max)
    {
        if (list.Count > max) { list.RemoveRange(max, list.Count - max); }
    }

    public List<string> TargetFolders { get; set; } = [];
    public List<string> RecentFolders { get; set; } = [];
    public List<string> RecentFiles { get; set; } = [];      // zuletzt geöffnete PDF-Dateien (Öffnen-Dropdown)
    public List<string> ExternalPrograms { get; set; } = []; // wird beim ersten Start automatisch gefüllt (ProgramFinder)
    public List<Favorite> Favorites { get; set; } = [];        // gemerkte Dateien (Favoriten-Menü, Option ShowFavorites)
    public List<Stamp> Stamps { get; set; } = [];              // Stempelpalette (Bearbeiten → Stempel einfügen / verwalten)
    public bool StampsInitialized { get; set; }                // Vorgabestempel wurden einmal eingetragen (auch wenn später alle gelöscht sind)
    public bool RememberLastPage { get; set; } = true;    // Dokumente mit der zuletzt angezeigten Seite öffnen (Merker in LastPages)
    public Dictionary<string, int> LastPages { get; set; } = []; // zuletzt angezeigte Seite je Datei – nur für Dateien der Zuletzt-Liste und der Favoriten
    public bool ConfirmDelete { get; set; } = true;       // vor dem Verschieben in den Papierkorb nachfragen
    public bool OpenNextAfterDelete { get; set; } = true; // nach dem Löschen die nächste Datei des Ordners anzeigen (wie in PDFMover optional)
    public bool ShowProgramIcons { get; set; } = true;    // Symbole der externen Programme zusätzlich in der Symbolleiste
    public bool ShowToolbarIcons { get; set; } = true;    // Symbole auf den Schaltflächen der Symbolleiste
    public bool LargeToolbarIcons { get; set; } = true;   // 24 statt 16 Pixel (vor DPI-Skalierung)
    public bool ShowToolbarText { get; set; } = true;     // Beschriftung der Schaltflächen; aus = nur Symbole (Layout „ohne Text“)
    public bool CloseOnEscape { get; set; }               // Programm mit 2× Esc beenden (Shift+Esc sofort)
    public bool ReopenLastFile { get; set; }              // zuletzt geöffnete Datei beim Start laden
    public bool ShowFullPathInTitle { get; set; }         // vollständigen Dateipfad statt nur des Dateinamens in der Titelleiste
    public bool ShowFavorites { get; set; }               // Favoriten-Menü in der Symbolleiste (Strg+D merkt die Datei); Standard aus
    public bool ExperimentalFeatures { get; set; }        // experimentelle Funktionen (Lesezeichen-Editor) – bewusst nur von Hand in settings.json einschaltbar
    public string AnnotationBorderColor { get; set; } = "808080"; // Textanmerkung: zuletzt gewählte Rahmenfarbe als RRGGBB, leer = kein Rahmen
    public string AnnotationBackground { get; set; } = "FFFFCC"; // Textanmerkung: zuletzt gewählter Hintergrund als RRGGBB, leer = transparent
    public string AnnotationTextColor { get; set; } = "000000";  // Textanmerkung: zuletzt gewählte Schriftfarbe als RRGGBB
    public string Language { get; set; } = "de";          // Kultur-Code; Sprachen liegen als Languages\lng.<code>.resx bereit
    public string InstallerLanguage { get; set; } = string.Empty; // zuletzt übernommene Setup-Sprachwahl (s. ApplyInstallerDefaults)
    public int InstallerToolbarLevel { get; set; } = -1;          // zuletzt übernommene Symbolleisten-Abstufung des Setups (-1 = noch keine)
    public string LastFile { get; set; } = string.Empty;  // Datei, die beim Beenden geöffnet war
    public int WindowX { get; set; } = -1;
    public int WindowY { get; set; } = -1;
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
    public bool WindowMaximized { get; set; }
    public int BookmarkWindowX { get; set; } = -1;   // Lesezeichen-Editor: zuletzt genutzte Lage und Größe (Breite 0 = noch nie gemerkt)
    public int BookmarkWindowY { get; set; } = -1;
    public int BookmarkWindowWidth { get; set; }
    public int BookmarkWindowHeight { get; set; }

    /// <summary>Voller Pfad der settings.json (Strg+Umschalt+F2 öffnet sie im zugeordneten Editor).</summary>
    public static string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PDFlight", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var loaded = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath)) ?? new AppSettings();
                loaded.TrimRecentLists();
                return ApplyInstallerDefaults(loaded);
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { } // defekte Datei → Standardwerte
        return ApplyInstallerDefaults(new AppSettings());
    }

    /// <summary>Beim ersten Start die Vorgabestempel eintragen – genau einmal, damit gelöschte nicht wiederkommen.
    /// Erst nach Lng.Initialize aufrufen, die Vorgaben sind übersetzt. True = es gab etwas zu speichern.</summary>
    public bool EnsureDefaultStamps()
    {
        if (StampsInitialized) { return false; }
        Stamps = Stamp.Defaults();
        StampsInitialized = true;
        return true;
    }

    /// <summary>Übernimmt die Vorgaben des Installers (Datei setup.default neben der EXE, Zeilen „language=de“ und
    /// „toolbar=0…3“) genau einmal pro Installation: Erst wenn eine (Neu-)Installation einen anderen Wert hinterlegt,
    /// überschreibt er die Einstellung — eine spätere Umstellung im Einstellungsdialog bleibt bis dahin erhalten.
    /// Die Symbolleisten-Abstufung richtet sich nach der Bildschirmbreite bei der Installation: 0 = alles an,
    /// 1 = kleine Symbole, 2 = zusätzlich ohne Programm-Icons, 3 = nur Text.</summary>
    private static AppSettings ApplyInstallerDefaults(AppSettings settings)
    {
        try
        {
            var marker = Path.Combine(AppContext.BaseDirectory, "setup.default");
            if (!File.Exists(marker)) { return settings; }
            foreach (var line in File.ReadAllLines(marker))
            {
                var separator = line.IndexOf('=');
                if (separator <= 0) { continue; }
                var key = line[..separator].Trim();
                var value = line[(separator + 1)..].Trim();
                if (key == "language" && value.Length == 2 && value != settings.InstallerLanguage)
                {
                    settings.InstallerLanguage = value; // wird beim nächsten Save festgehalten
                    settings.Language = value;
                }
                else if (key == "toolbar" && int.TryParse(value, out var level) && level is >= 0 and <= 3 && level != settings.InstallerToolbarLevel)
                {
                    settings.InstallerToolbarLevel = level;
                    settings.LargeToolbarIcons = level < 1;
                    settings.ShowProgramIcons = level < 2;
                    settings.ShowToolbarIcons = level < 3;
                    settings.ShowToolbarText = true;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        return settings;
    }

    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true }; // gecacht (CA1869)

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!); // SettingsPath ist immer ein voller Dateipfad
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, SerializerOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { } // Speichern darf das Programm nie blockieren
    }

    /// <summary>Trägt einen Ordner vorne in die Zuletzt-Liste ein (ohne Duplikate, begrenzte Länge).</summary>
    public void AddRecentFolder(string path)
    {
        if (MaxRecentFolders == 0) { return; } // Zuletzt-Liste abgeschaltet: keine Spuren
        RecentFolders.RemoveAll(x => string.Equals(x, path, StringComparison.OrdinalIgnoreCase));
        RecentFolders.Insert(0, path);
        Trim(RecentFolders, MaxRecentFolders);
    }

    /// <summary>Trägt eine Datei vorne in die Liste der zuletzt geöffneten Dateien ein.</summary>
    public void AddRecentFile(string path)
    {
        if (MaxRecentFiles == 0) { return; } // Verlauf abgeschaltet: keine Spuren
        RecentFiles.RemoveAll(x => string.Equals(x, path, StringComparison.OrdinalIgnoreCase));
        RecentFiles.Insert(0, path);
        Trim(RecentFiles, MaxRecentFiles);
    }

    // ------------------------------------------------------------------ zuletzt angezeigte Seiten

    /// <summary>Zuletzt angezeigte Seite einer Datei; 0, wenn keine gemerkt ist.</summary>
    public int GetLastPage(string path) => LastPageKey(path) is { } key ? LastPages[key] : 0;

    /// <summary>Merkt die Seite – nur für Dateien der Zuletzt-Liste oder der Favoriten, damit das Wörterbuch klein bleibt
    /// (Seite 1 wird nicht gemerkt). Einträge zu Dateien, die aus beiden Listen verschwunden sind, fliegen dabei raus.</summary>
    public void SetLastPage(string path, int page)
    {
        if (LastPageKey(path) is { } old) { LastPages.Remove(old); }
        if (page > 1 && IsTracked(path)) { LastPages[path] = page; }
        foreach (var stale in LastPages.Keys.Where(k => !IsTracked(k)).ToList()) { LastPages.Remove(stale); }
    }

    /// <summary>Nach Umbenennen/Verschieben: der Eintrag folgt der Datei.</summary>
    public void MoveLastPage(string oldFile, string newFile)
    {
        if (LastPageKey(oldFile) is { } key && LastPages.Remove(key, out var page)) { LastPages[newFile] = page; }
    }

    private bool IsTracked(string path) =>
        RecentFiles.Any(f => string.Equals(f, path, StringComparison.OrdinalIgnoreCase)) || Favorites.Any(f => f.IsFor(path));

    /// <summary>Der gespeicherte Schlüssel zur Datei (Pfadvergleich ohne Groß-/Kleinschreibung); null, wenn keiner da ist.</summary>
    private string? LastPageKey(string path) => LastPages.Keys.FirstOrDefault(k => string.Equals(k, path, StringComparison.OrdinalIgnoreCase));

    // ------------------------------------------------------------------ Favoriten

    /// <summary>Der Favorit dieser Datei; null, wenn es keinen gibt.</summary>
    public Favorite? FindFavorite(string file) => Favorites.Find(f => f.IsFor(file));

    /// <summary>Trägt eine Datei als Favorit ein (ersetzt einen vorhandenen Eintrag derselben Datei).
    /// False, wenn die Höchstzahl erreicht ist.</summary>
    public bool AddFavorite(string file, string name)
    {
        Favorites.RemoveAll(f => f.IsFor(file));
        if (Favorites.Count >= FavoritesLimit) { return false; }
        Favorites.Add(new Favorite { File = file, Name = name.Trim() });
        return true;
    }

    /// <summary>Zieht den Favoriten einer umbenannten oder verschobenen Datei auf ihren neuen Pfad um.</summary>
    public void MoveFavorites(string oldFile, string newFile)
    {
        foreach (var favorite in Favorites.Where(f => f.IsFor(oldFile))) { favorite.File = newFile; }
    }

    /// <summary>Übernimmt die gemeinsamen Listen frisch von der Platte, damit mehrere gleichzeitig laufende
    /// Instanzen die Ziel-, Zuletzt- und Programmlisten der jeweils anderen sehen (Aufruf vor jeder Verwendung
    /// und vor dem Speichern; die Fenster-/Anzeigeoptionen bleiben instanzlokal).</summary>
    public void ReloadSharedLists()
    {
        var fresh = Load();
        TargetFolders = fresh.TargetFolders;
        RecentFolders = fresh.RecentFolders;
        LastPages = fresh.LastPages;
        RecentFiles = fresh.RecentFiles;
        ExternalPrograms = fresh.ExternalPrograms;
        Favorites = fresh.Favorites;
        Stamps = fresh.Stamps;
        ExperimentalFeatures = fresh.ExperimentalFeatures; // von Hand in der Datei geändert (Strg+Umschalt+F2) – beim Beenden nicht überschreiben
    }
}
