using System.Text.Json.Serialization;

namespace PDFLight.Classes;

/// <summary>Feste Stempelpositionen auf der Seite (10 mm Abstand vom Rand).</summary>
public enum StampPosition { TopLeft, TopCenter, TopRight, Center }

/// <summary>Ein Textstempel der Palette: ein Wort in fetter Schrift mit Rahmen in der Schriftfarbe, wahlweise mit
/// Hintergrund. Gespeichert in settings.json; Farben als RRGGBB, leerer Hintergrund = transparent.</summary>
public sealed class Stamp
{
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
    public int Opacity { get; set; } = DefaultOpacity; // Deckkraft in Prozent (ExtGState /CA und /ca im Darstellungsstrom)

    public const int DefaultOpacity = 75; // mäßige Transparenz als Vorgabe – der Seiteninhalt bleibt unter dem Stempel lesbar
    public const int MinOpacity = 10;

    /// <summary>Deckkraft als Faktor 0…1, auf den erlaubten Bereich begrenzt.</summary>
    [JsonIgnore]
    public double Alpha => Math.Clamp(Opacity, MinOpacity, 100) / 100.0;


    /// <summary>Eckenradius in Punkt relativ zur Schriftgröße (0 = eckig).</summary>
    [JsonIgnore]
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
        var alpha = (int)Math.Round(Alpha * 255); // Deckkraft wie im PDF – GDI+-Text statt TextRenderer, weil der kein Alpha kennt
        var color = Color.FromArgb(alpha, Color);
        using Font font = new("Arial", Math.Max(6f, bounds.Height * 0.42f), FontStyle.Bold, GraphicsUnit.Pixel);
        using Font small = new("Arial", Math.Max(5f, bounds.Height * 0.42f * (float)DateFactor), FontStyle.Regular, GraphicsUnit.Pixel);
        var format = StringFormat.GenericTypographic;
        var textSize = g.MeasureString(text, font, int.MaxValue, format);
        var dateSize = withSecond ? g.MeasureString(dateLine, small, int.MaxValue, format) : SizeF.Empty;
        var padX = (int)(bounds.Height * 0.25);
        Rectangle box = new(bounds.X, bounds.Y, Math.Min(bounds.Width, (int)Math.Ceiling(Math.Max(textSize.Width, dateSize.Width)) + 2 * padX), bounds.Height);
        using var path = RoundedPath(new RectangleF(box.X + 1, box.Y + 1, box.Width - 2, box.Height - 2), Rounded ? box.Height * 0.2f : 0);
        var smoothing = g.SmoothingMode;
        var hint = g.TextRenderingHint;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        using var clip = g.Clip; // Kopie – nach dem Zurücksetzen freigeben, sonst bleibt je Zeichnen eine GDI-Region liegen
        g.SetClip(box, System.Drawing.Drawing2D.CombineMode.Intersect); // zu breite Texte enden am Kasten
        if (BackgroundColor is { } background)
        {
            using SolidBrush fill = new(Color.FromArgb(alpha, background));
            g.FillPath(fill, path);
        }
        if (Border)
        {
            using Pen pen = new(color, 2);
            g.DrawPath(pen, path);
        }
        using SolidBrush ink = new(color);
        var split = withSecond ? box.Y + box.Height * 0.62f : box.Bottom; // oben der Text, unten die Datumszeile
        g.DrawString(text, font, ink, box.X + (box.Width - textSize.Width) / 2, box.Y + 2 + (split - box.Y - 2 - textSize.Height) / 2, format);
        if (withSecond) { g.DrawString(dateLine, small, ink, box.X + (box.Width - dateSize.Width) / 2, split + (box.Bottom - 2 - split - dateSize.Height) / 2, format); }
        g.Clip = clip;
        g.SmoothingMode = smoothing;
        g.TextRenderingHint = hint;
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
