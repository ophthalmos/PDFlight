namespace PDFLight;

internal static class Program
{
    /// <summary>Einstiegspunkt; ein optionales Argument ist der Pfad der zu öffnenden PDF-Datei.
    /// „--help“ (der Installer nach „PDFlight starten“) erzeugt die Hilfedatei und zeigt sie an.</summary>
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        var showHelp = args.Any(a => string.Equals(a, "--help", StringComparison.OrdinalIgnoreCase));
        var startFile = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal));
        Application.Run(new Forms.MainForm(startFile, showHelp));
    }
}
