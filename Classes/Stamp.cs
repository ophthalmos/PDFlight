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

    /// <summary>Datum und Uhrzeit im Kurzformat der Sprache.</summary>
    public static string DateText(DateTime when) => when.ToString("g", System.Globalization.CultureInfo.CurrentCulture);

    /// <summary>Die zweite Zeile unter dem Text: Datum (wenn gewünscht) und das Bearbeiterkürzel in Klammern; leer, wenn beides fehlt.</summary>
    public string SecondLine(string? dateText)
    {
        List<string> parts = [];
        if (WithDate && !string.IsNullOrEmpty(dateText)) { parts.Add(dateText); }
        if (Initials.Trim().Length > 0) { parts.Add("(" + Initials.Trim() + ")"); }
        return string.Join(" ", parts);
    }

    public string SecondLine(DateTime when) => SecondLine(DateText(when));

    public string Text { get; set; } = string.Empty;
    public double FontSize { get; set; } = 24;
    public string TextColor { get; set; } = "C00000";
    public string Background { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StampPosition Position { get; set; } = StampPosition.TopRight;
    public bool WithDate { get; set; } = true;  // Datum und Uhrzeit als zweite Zeile
    public bool Border { get; set; } = true;    // Rahmen in der Schriftfarbe
    public bool Rounded { get; set; }           // abgerundete Ecken (Rahmen und Hintergrund)
    public string Initials { get; set; } = string.Empty; // Bearbeiterkürzel (max. 3 Zeichen), in Klammern hinter dem Datum

    public const int MaxInitialsLength = 3;

    /// <summary>Eckenradius in Punkt relativ zur Schriftgröße (0 = eckig).</summary>
    public double CornerRadius => Rounded ? FontSize * 0.35 : 0;

    /// <summary>Die Namen der Positionen in der Reihenfolge des Enums (Lng-Schlüssel).</summary>
    public static readonly string[] PositionNames = ["oben links", "oben mittig", "oben rechts", "Seitenmitte"];

    /// <summary>Die Vorgaben beim ersten Start (in der eingestellten Sprache) – Text und Farbe frei änderbar.</summary>
    public static List<Stamp> Defaults() =>
    [
        new() { Text = Lng.T("Erledigt"), TextColor = "008000", Position = StampPosition.TopLeft },
        new() { Text = Lng.T("Bezahlt"), TextColor = "C00000", Position = StampPosition.TopRight },
        new() { Text = Lng.T("Archiviert"), TextColor = "000000", Position = StampPosition.TopCenter },
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
        var dateLine = SecondLine(DateTime.Now);
        var withSecond = dateLine.Length > 0;
        using Font font = new("Arial", Math.Max(6f, bounds.Height * 0.42f), FontStyle.Bold, GraphicsUnit.Pixel);
        using Font small = new("Arial", Math.Max(5f, bounds.Height * 0.42f * (float)DateFactor), FontStyle.Regular, GraphicsUnit.Pixel);
        var textSize = TextRenderer.MeasureText(g, text, font, Size.Empty, TextFormatFlags.NoPadding);
        var dateSize = withSecond ? TextRenderer.MeasureText(g, dateLine, small, Size.Empty, TextFormatFlags.NoPadding) : Size.Empty;
        var padX = (int)(bounds.Height * 0.25);
        Rectangle box = new(bounds.X, bounds.Y, Math.Min(bounds.Width, Math.Max(textSize.Width, dateSize.Width) + 2 * padX), bounds.Height);
        using var path = RoundedPath(new RectangleF(box.X + 1, box.Y + 1, box.Width - 2, box.Height - 2), Rounded ? box.Height * 0.2f : 0);
        var smoothing = g.SmoothingMode;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        if (BackgroundColor is { } background)
        {
            using SolidBrush fill = new(background);
            g.FillPath(fill, path);
        }
        if (Border)
        {
            using Pen pen = new(Color, 2);
            g.DrawPath(pen, path);
        }
        g.SmoothingMode = smoothing;
        var split = withSecond ? box.Y + (int)(box.Height * 0.62) : box.Bottom; // oben der Text, unten die Datumszeile
        TextRenderer.DrawText(g, text, font, new Rectangle(box.X, box.Y + 2, box.Width, split - box.Y - 2), Color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
        if (withSecond) { TextRenderer.DrawText(g, dateLine, small, new Rectangle(box.X, split, box.Width, box.Bottom - split - 2), Color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis); }
    }

    /// <summary>Rechteck mit wahlweise abgerundeten Ecken als GDI+-Pfad.</summary>
    public static System.Drawing.Drawing2D.GraphicsPath RoundedPath(RectangleF rect, float radius)
    {
        System.Drawing.Drawing2D.GraphicsPath path = new();
        if (radius <= 0) { path.AddRectangle(rect); return path; }
        var d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
