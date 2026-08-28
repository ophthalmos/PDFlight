namespace PDFLight;

internal static class Program
{
    /// <summary>Einstiegspunkt; ein optionales Argument ist der Pfad der zu öffnenden PDF-Datei.</summary>
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Forms.MainForm(args.Length > 0 ? args[0] : null));
    }
}
