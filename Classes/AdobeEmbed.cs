using System.Text.RegularExpressions;

namespace PDFLight.Classes;

/// <summary>Adobe PDF Embed API – die optionale zweite Ansicht zum Hervorheben und Kommentieren (Einstellungen, Seite „Adobe PDF Embed API“).
/// Bewusst die Ausnahme: Adobes Viewer läuft als Webdienst (SDK aus dem Netz, Lizenzprüfung, Cookies), deshalb erst nach ausdrücklicher
/// Zustimmung und nur auf Knopfdruck (F8 bzw. Schaltfläche „Adobe“); jede andere Programmfunktion wechselt stillschweigend zurück zum
/// Chromium-Viewer. Die Seite adobe\index.html neben der EXE bettet das SDK ein; <see cref="PdfViewHost.ShowAdobe"/> zeigt sie im
/// selben WebView2. Erfahrungen aus dem Testprojekt PdfEmbed (E:\Code\CSharp\PdfEmbed, September 2026).</summary>
internal static partial class AdobeEmbed
{
    /// <summary>Seitenursprung der Adobe-Seite (virtueller Host des WebView2). Client-ID und Domain gehören zusammen: Die ID gilt nur
    /// für die in der Adobe Developer Console eingetragene Anwendungsdomäne – für PDFlight ist das „localhost“.</summary>
    public const string Host = "localhost";

    /// <summary>Die Client-ID steht nicht im Quelltext, sondern in dieser Datei neben der EXE (im Projektordner git-ignoriert, vom Build
    /// mitkopiert, vom Installer mitgeliefert; Vorlage: adobe-clientid.example.txt). Wunsch vom 24.09.2026: die ID nicht auf GitHub zeigen.</summary>
    public const string ClientIdFile = "adobe-clientid.txt";

    /// <summary>Adobes Nutzungsprotokoll: Anfragen dorthin beantwortet PDFlight selbst mit „204 No Content“ (s. PdfViewHost) – sie gehen nie ins Netz.</summary>
    public const string BlockedLogUrl = "https://dc-api.adobe.io/system/log";

    /// <summary>Ordner mit der Seite, die das SDK einbettet (adobe\index.html neben der EXE).</summary>
    public static string PageFolder => Path.Combine(AppContext.BaseDirectory, "adobe");

    /// <summary>Die Client-ID aus <see cref="ClientIdFile"/>: die erste Zeile, die kein Kommentar (#) ist, 32 Hexziffern; null = fehlt oder ungültig.</summary>
    public static string? ClientId { get; } = ReadClientId();

    /// <summary>Die Ansicht ist technisch möglich: Client-ID und Seite vorhanden (die Zustimmung des Nutzers kommt aus den Einstellungen).</summary>
    public static bool IsAvailable => ClientId != null && File.Exists(Path.Combine(PageFolder, "index.html"));

    /// <summary>Sprache der Adobe-Oberfläche passend zur Programmsprache.</summary>
    public static string Locale => Lng.CultureCode switch { "en" => "en-US", "fr" => "fr-FR", "es" => "es-ES", _ => "de-DE" };

    /// <summary>Adobes Datenschutzerklärung in der Programmsprache (Link auf der Einstellungsseite).</summary>
    public static string PrivacyUrl => Lng.CultureCode switch
    {
        "de" => "https://www.adobe.com/de/privacy/policy.html",
        "fr" => "https://www.adobe.com/fr/privacy/policy.html",
        "es" => "https://www.adobe.com/es/privacy/policy.html",
        _ => "https://www.adobe.com/privacy/policy.html",
    };

    private static string? ReadClientId()
    {
        try
        {
            var line = File.ReadLines(Path.Combine(AppContext.BaseDirectory, ClientIdFile))
                .Select(l => l.Trim()).FirstOrDefault(l => l.Length > 0 && !l.StartsWith('#'));
            return line != null && ClientIdRegex().IsMatch(line) ? line : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return null; }
    }

    [GeneratedRegex("^[0-9a-fA-F]{32}$")]
    private static partial Regex ClientIdRegex();
}
