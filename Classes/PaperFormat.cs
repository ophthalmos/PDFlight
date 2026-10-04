namespace PDFLight.Classes;

/// <summary>Gängige Papierformate: benennt die Maße einer Seite (Dialog „Leere Seite einfügen“) und liefert die festen Formate für
/// neue Seiten. Maße in Punkt (1/72 Zoll), immer Hochformat (Breite ≤ Höhe).</summary>
internal static class PaperFormat
{
    private const double PtPerMm = 72 / 25.4;
    private const double ToleranceMm = 1;

    public static readonly (double Width, double Height) A4 = (210 * PtPerMm, 297 * PtPerMm);
    public static readonly (double Width, double Height) Letter = (612, 792); // 8,5 × 11 Zoll

    // Name, kurze und lange Seite in mm; die DIN-Namen laufen über Lng.T (englisch nur „A4“), die US-Namen sind überall gleich
    private static readonly (string Name, bool Translate, double ShortMm, double LongMm)[] Known =
    [
        ("DIN A3", true, 297, 420), ("DIN A4", true, 210, 297), ("DIN A5", true, 148, 210), ("DIN A6", true, 105, 148),
        ("US Letter", false, 215.9, 279.4), ("US Legal", false, 215.9, 355.6),
    ];

    /// <summary>Beschreibung wie „DIN A4, Hochformat“ oder „216 × 280 mm, Querformat“ (Toleranz 1 mm); leer bei unbekannten Maßen.</summary>
    public static string Describe(double widthPt, double heightPt)
    {
        if (widthPt <= 0 || heightPt <= 0) { return string.Empty; }
        var widthMm = widthPt / PtPerMm;
        var heightMm = heightPt / PtPerMm;
        var shortMm = Math.Min(widthMm, heightMm);
        var longMm = Math.Max(widthMm, heightMm);
        var (Name, Translate, ShortMm, LongMm) = Known.FirstOrDefault(k => Math.Abs(k.ShortMm - shortMm) <= ToleranceMm && Math.Abs(k.LongMm - longMm) <= ToleranceMm);
        var name = Name == null ? $"{widthMm:0} × {heightMm:0} mm" : Translate ? Lng.T(Name) : Name;
        return name + ", " + (widthMm > heightMm ? Lng.T("Querformat") : Lng.T("Hochformat"));
    }
}
