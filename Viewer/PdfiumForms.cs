namespace PDFLight.Viewer;

/// <summary>Werte aus PDFiums Formular-Schnittstelle (fpdf_formfill.h), die die Ansicht braucht. Die Formularumgebung selbst läuft im
/// Hilfsprogramm (pdfhost).</summary>
internal static class PdfiumForms
{
    public const int FieldTextField = 6, FieldComboBox = 4; // FPDF_FORMFIELD_*: Textfelder bekommen den Textcursor, alles andere die Hand
    public const int ModShift = 1, ModControl = 2, ModAlt = 4, ModLeftButton = 1 << 6; // FWL_EVENTFLAG_*
}
