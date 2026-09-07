namespace PDFLight;

internal static class Program
{
    /// <summary>Einstiegspunkt: optional eine PDF-Datei und Schalter (/help, /max, /page:N, /print – s. StartOptions).</summary>
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Forms.MainForm(Classes.StartOptions.Parse(args)));
    }
}
