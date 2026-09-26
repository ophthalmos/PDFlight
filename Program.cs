namespace PDFLight;

internal static class Program
{
    /// <summary>Einstiegspunkt: optional eine PDF-Datei und Schalter (/help, /max, /page:N, /print – s. StartOptions).</summary>
    [STAThread]
    private static void Main(string[] args)
    {
        _ = Task.Run(Classes.PdfEditService.WarmUp); // PDFsharp vorbereiten, während Fenster und WebView2 starten
        ApplicationConfiguration.Initialize();
        Application.Run(new Forms.MainForm(Classes.StartOptions.Parse(args)));
    }
}
