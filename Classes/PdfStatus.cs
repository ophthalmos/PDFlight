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
internal sealed partial record PdfStatus(int PageCount, string? Version, string? PdfALevel, int OutlineCount = 0, bool Encrypted = false)
{
    private static readonly byte[] PartKey = "pdfaid:part"u8.ToArray();
    private const int HeaderSearchLength = 1024; // so weit sucht auch PDFium nach dem Dateikopf (davor darf Fremdes stehen)

    /// <summary>Steht „%PDF-“ in den ersten 1024 Bytes? Sonst ist es keine PDF (Foto, Word-Datei, eine als .pdf gespeicherte Webseite …) –
    /// PDFlight meldet das selbst, statt PDFiums Fehlertext zu zeigen (seit 10.10.2026).</summary>
    public static bool HasPdfHeader(ReadOnlySpan<byte> bytes) => bytes[..Math.Min(bytes.Length, HeaderSearchLength)].IndexOf("%PDF-"u8) >= 0;

    /// <summary>Wie <see cref="HasPdfHeader(ReadOnlySpan{byte})"/>, liest nur den Anfang der Datei; nicht lesbar = ja (den Fehler meldet dann das Laden).</summary>
    public static bool HasPdfHeader(string path) => ReadHead(path) is not { } head || HasPdfHeader(head);

    /// <summary>Ein Bild, aus dem PDFlight eine PDF machen kann (seit 10.10.2026, wie Acrobat)? Nach den ersten Bytes, nicht nach der
    /// Endung: JPEG, PNG, GIF, BMP und TIFF – was GDI+ lesen kann (WebP und HEIC nicht).</summary>
    public static bool IsImage(ReadOnlySpan<byte> bytes) =>
        bytes.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF]) || bytes.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47]) // JPEG, PNG
        || bytes.StartsWith("GIF8"u8) || IsBmp(bytes)                                                                               // GIF, BMP
        || bytes.StartsWith((ReadOnlySpan<byte>)[0x49, 0x49, 0x2A, 0x00]) || bytes.StartsWith((ReadOnlySpan<byte>)[0x4D, 0x4D, 0x00, 0x2A]); // TIFF (II*, MM*)

    /// <summary>BMP: „BM“ allein träfe auch Textdateien („BMW …“) – dazu die Größe des Bildkopfs an Stelle 14, die nur wenige Werte
    /// annehmen kann (BITMAPCOREHEADER 12, INFO 40, V2/V3 52/56, OS/2 64, V4 108, V5 124; Review 11.10.2026).</summary>
    private static bool IsBmp(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= 18 && bytes.StartsWith("BM"u8) && System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bytes[14..18]) is 12 or 40 or 52 or 56 or 64 or 108 or 124;

    /// <summary>Wie <see cref="IsImage(ReadOnlySpan{byte})"/>, liest nur den Anfang der Datei; nicht lesbar = nein.</summary>
    public static bool IsImage(string path) => ReadHead(path) is { } head && IsImage(head);

    private static byte[]? ReadHead(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            var head = new byte[HeaderSearchLength];
            return head[..stream.ReadAtLeast(head, HeaderSearchLength, throwOnEndOfStream: false)];
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return null; }
    }

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