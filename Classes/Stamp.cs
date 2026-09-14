using System.Text.Json.Serialization;

namespace PDFLight.Classes;

/// <summary>Feste Stempelpositionen auf der Seite (10 mm Abstand vom Rand).</summary>
public enum StampPosition { TopLeft, TopCenter, TopRight, Center }

/// <summary>Ein Textstempel der Palette: ein Wort in fetter Schrift mit Rahmen in der Schriftfarbe, wahlweise mit
/// Hintergrund. Gespeichert in settings.json; Farben als RRGGBB, leerer Hintergrund = transparent.</summary>
public sealed class Stamp
{
    public const int MaxTextLength = 20;
    public const double MinFontSize = 8;
    public const double MaxFontSize = 72;
    public const double DateFactor = 0.4;    // Datumszeile relativ zur Schriftgröße
    public const double PaddingFactor = 0.35; // Innenabstand relativ zur Schriftgröße

    /// <summary>Die Datumszeile, die jeder Stempel automatisch bekommt: Datum und Uhrzeit im Format der Sprache.</summary>
    public static string DateLine(DateTime when) => when.ToString("g", System.Globalization.CultureInfo.CurrentCulture);

    public string Text { get; set; } = string.Empty;
    public double FontSize { get; set; } = 24;
    public string TextColor { get; set; } = "C00000";
    public string Background { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StampPosition Position { get; set; } = StampPosition.TopRight;

    /// <summary>Die Namen der Positionen in der Reihenfolge des Enums (Lng-Schlüssel).</summary>
    public static readonly string[] PositionNames = ["oben links", "oben mittig", "oben rechts", "Seitenmitte"];

    /// <summary>Die Vorgaben beim ersten Start (in der eingestellten Sprache) – Text und Farbe frei änderbar.</summary>
    public static List<Stamp> Defaults() =>
    [
        new() { Text = Lng.T("Erledigt"), TextColor = "008000" },
        new() { Text = Lng.T("Bezahlt"), TextColor = "C00000" },
        new() { Text = Lng.T("Archiviert"), TextColor = "000000" },
    ];

    [JsonIgnore]
    public Color Color => AnnotationStyle.ParseHex(TextColor) ?? Color.Black;

    [JsonIgnore]
    public Color? BackgroundColor => AnnotationStyle.ParseHex(Background);

    public Stamp Clone() => (Stamp)MemberwiseClone();

    /// <summary>Zeichnet den Stempel verkleinert in ein Rechteck (Listen der Dialoge): Kasten mit Rahmen in der Schriftfarbe,
    /// fetter Text, alles im Maßstab der Höhe – so sieht man Farbe und Hintergrund, die echte Größe zeigt die Zahl daneben.</summary>
    public void DrawPreview(Graphics g, Rectangle bounds)
    {
        var text = Text.Length > 0 ? Text : "…";
        var dateLine = DateLine(DateTime.Now);
        using Font font = new("Arial", Math.Max(6f, bounds.Height * 0.42f), FontStyle.Bold, GraphicsUnit.Pixel);
        using Font small = new("Arial", Math.Max(5f, bounds.Height * 0.42f * (float)DateFactor), FontStyle.Regular, GraphicsUnit.Pixel);
        var textSize = TextRenderer.MeasureText(g, text, font, Size.Empty, TextFormatFlags.NoPadding);
        var dateSize = TextRenderer.MeasureText(g, dateLine, small, Size.Empty, TextFormatFlags.NoPadding);
        var padX = (int)(bounds.Height * 0.25);
        Rectangle box = new(bounds.X, bounds.Y, Math.Min(bounds.Width, Math.Max(textSize.Width, dateSize.Width) + 2 * padX), bounds.Height);
        if (BackgroundColor is { } background)
        {
            using SolidBrush fill = new(background);
            g.FillRectangle(fill, box);
        }
        using Pen border = new(Color, 2);
        g.DrawRectangle(border, box.X + 1, box.Y + 1, box.Width - 2, box.Height - 2);
        var split = box.Y + (int)(box.Height * 0.62); // oben der Text, unten die Datumszeile
        TextRenderer.DrawText(g, text, font, new Rectangle(box.X, box.Y + 2, box.Width, split - box.Y - 2), Color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g, dateLine, small, new Rectangle(box.X, split, box.Width, box.Bottom - split - 2), Color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
    }
}
