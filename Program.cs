namespace PDFLight;

internal static class Program
{
    /// <summary>Einstiegspunkt: optional eine PDF-Datei und Schalter (/help, /max, /page:N, /print, /twopage, /zoom:N, /nosidebar – s. StartOptions).</summary>
    [STAThread]
    private static void Main(string[] args)
    {
        // PDFsharp steht seit 6.5 unter AGPL-3.0 oder einer kommerziellen Lizenz; ohne Wahl wirft jedes new PdfDocument() eine
        // PdfSharpLicenseException (Fehlerbericht 14.10.2026: Hilfedatei beim Start). Entscheidung Wilhelms: AGPL.
        PdfSharp.PdfSharpLicense.SetAgplLicense();
        ApplicationConfiguration.Initialize();
        Application.Run(new Forms.MainForm(Classes.StartOptions.Parse(args)));
    }
}
