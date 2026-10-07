using System.Text;
using System.Text.RegularExpressions;

namespace PDFLight.Classes;

/// <summary>Was die Statusleiste und die Bearbeitungssperren über die angezeigte Datei wissen müssen. Seit 04.10.2026 ohne PDFsharp:
/// Seitenzahl, Version, Verschlüsselung und Lesezeichen kommen aus dem abgeschotteten Hilfsprogramm (PDFium), die PDF/A-Stufe aus einer
/// Textsuche in den Bytes – vorher las PDFsharp jede Datei beim Öffnen ungeschützt im Hauptprozess.</summary>
/// <param name="PageCount">Seitenzahl, -1 solange das Dokument noch nicht geladen ist.</param>
/// <param name="Version">PDF-Version, z. B. „1.7“.</param>
/// <param name="PdfALevel">Deklarierte PDF/A-Stufe, z. B. „2b“; null ohne Deklaration.</param>
/// <param name="OutlineCount">Lesezeichen, alle Ebenen.</param>
/// <param name="Encrypted">Verschlüsselt (Benutzer- oder nur Besitzerkennwort).</param>
/// <param name="AttachmentCount">Eingebettete Dateien (Anhänge des Dokuments, z. B. die XML einer E-Rechnung) – Hinweis in der Statusleiste.</param>
internal sealed partial record PdfStatus(int PageCount, string? Version, string? PdfALevel, int OutlineCount = 0, bool Encrypted = false, int AttachmentCount = 0)
{
    private static readonly byte[] PartKey = "pdfaid:part"u8.ToArray();

    /// <summary>Deklarierte PDF/A-Stufe aus den XMP-Metadaten, ohne die PDF zu parsen: PDF/A verbietet Filter auf dem Metadaten-Strom, er
    /// steht also lesbar in der Datei. Attribut- und Element-Schreibweise (pdfaid:part="2" bzw. &lt;pdfaid:part&gt;2&lt;/pdfaid:part&gt;);
    /// reine Deklarationsprüfung, keine Validierung. Gesucht wird das letzte Vorkommen – inkrementelle Updates hängen neue Metadaten an.</summary>
    public static string? DetectPdfALevel(byte[] bytes)
    {
        var index = bytes.AsSpan().LastIndexOf(PartKey);
        if (index < 0) { return null; }
        var start = Math.Max(0, index - 2048);
        var window = Encoding.Latin1.GetString(bytes, start, Math.Min(bytes.Length - start, 4096)); // die pdfaid-Angaben stehen beieinander
        // im ganzen Fenster suchen: in der Element-Schreibweise ist das letzte Vorkommen das schließende </pdfaid:part>
        var matches = PartRegex().Matches(window);
        if (matches.Count == 0) { return null; }
        var partMatch = matches[^1];
        // die Konformitätsangabe, die der gewählten Teilangabe am nächsten steht – beide gehören zum selben Metadatenblock; die erste im
        // Fenster könnte aus einem älteren Block einer früheren Speicherung stammen (Ultra-Review 04.10.2026)
        var conformance = ConformanceRegex().Matches(window).OrderBy(m => Math.Abs(m.Index - partMatch.Index)).FirstOrDefault()?.Groups[1].Value ?? string.Empty;
        return partMatch.Groups[1].Value + conformance.ToLowerInvariant();
    }

    [GeneratedRegex(@"pdfaid:part(?:\s*=\s*[""']|\s*>\s*)(\d+)")]
    private static partial Regex PartRegex();

    [GeneratedRegex(@"pdfaid:conformance(?:\s*=\s*[""']|\s*>\s*)([A-Za-z])")]
    private static partial Regex ConformanceRegex();
}