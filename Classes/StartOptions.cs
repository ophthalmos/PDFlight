namespace PDFLight.Classes;

/// <summary>Die Kommandozeile von PDFlight: optional eine PDF-Datei und Schalter, wahlweise mit „/“ oder „--“:
/// help (Hilfedatei erzeugen und anzeigen – so ruft der Installer nach „PDFlight starten“ auf), max (maximiert),
/// page:N (auf Seite N öffnen) und print bzw. print:Druckername (ohne Dialog drucken und beenden – dahinter
/// steckt auch das Explorer-Kontextmenü „Drucken“, s. ShellUtil.RegisterFileType). Seit 19.10.2026 (Wunsch Wilhelms, für den
/// automatischen Zeitungsabruf in KNEpaper) außerdem twopage (zweiseitige Ansicht), zoom:N (Zoomstufe in Prozent, 100 = Originalgröße)
/// und nosidebar (Seitenleiste zu, auch bei Lesezeichen oder Anmerkungen) – wie max nur für diesen Start, die gemerkten Einstellungen
/// bleiben. Unbekannte Schalter werden übergangen.</summary>
internal sealed record StartOptions(string? File, bool Help, bool Maximized, int Page, bool Print, string? Printer, bool TwoPage = false, int ZoomPercent = 0, bool NoSidebar = false)
{
    public static StartOptions Parse(string[] args)
    {
        string? file = null, printer = null;
        bool help = false, maximized = false, print = false, twoPage = false, noSidebar = false;
        var page = 0;
        var zoom = 0;
        foreach (var arg in args)
        {
            if (arg.StartsWith("--", StringComparison.Ordinal) || (arg.Length > 1 && arg[0] == '/'))
            {
                var body = arg.TrimStart('-', '/');
                var separator = body.IndexOfAny([':', '=']);
                var name = (separator < 0 ? body : body[..separator]).ToLowerInvariant();
                var value = separator < 0 ? string.Empty : body[(separator + 1)..];
                switch (name)
                {
                    case "help": help = true; break;
                    case "max": maximized = true; break;
                    case "print": print = true; printer = value.Length > 0 ? value.Trim('"') : null; break;
                    case "page": if (int.TryParse(value, out var n) && n > 0) { page = n; } break;
                    case "twopage": twoPage = true; break;
                    case "zoom": if (int.TryParse(value.TrimEnd('%', ' '), out var z) && z > 0) { zoom = Math.Clamp(z, 10, 500); } break; // wie das Zoomfeld
                    case "nosidebar": noSidebar = true; break;
                }
            }
            else { file ??= arg; } // das erste Nicht-Schalter-Argument ist die Datei
        }
        return new StartOptions(file, help, maximized, page, print, printer, twoPage, zoom, noSidebar);
    }
}
