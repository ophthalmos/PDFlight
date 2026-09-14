namespace PDFLight.Classes;

/// <summary>Die festen Farbauswahlen der Anmerkungs- und Stempeldialoge (Namen sind Lng-Schlüssel) und das gemeinsame
/// Zeichnen der Listeneinträge mit Farbfeld. null steht für „transparent“ bzw. „kein Rahmen“.</summary>
internal static class ColorPalette
{
    /// <summary>Hintergründe des Kastens; null = transparent.</summary>
    public static readonly (string Name, Color? Color)[] Backgrounds =
    [
        ("Gelb", Color.FromArgb(255, 255, 204)), ("Weiß", Color.White), ("Hellblau", Color.FromArgb(221, 238, 255)),
        ("Hellgrün", Color.FromArgb(221, 255, 221)), ("Rosa", Color.FromArgb(255, 221, 238)), ("Transparent", null),
    ];

    /// <summary>Rahmenfarben der Textanmerkung; null = kein Rahmen.</summary>
    public static readonly (string Name, Color? Color)[] Borders =
    [
        ("Grau", Color.FromArgb(128, 128, 128)), ("Schwarz", Color.Black), ("Rot", Color.FromArgb(192, 0, 0)), ("Blau", Color.FromArgb(0, 0, 192)), ("Kein Rahmen", null),
    ];

    /// <summary>Schriftfarben der Textanmerkung.</summary>
    public static readonly (string Name, Color Color)[] TextColors =
    [
        ("Schwarz", Color.Black), ("Rot", Color.FromArgb(192, 0, 0)), ("Grün", Color.FromArgb(0, 128, 0)), ("Blau", Color.FromArgb(0, 0, 192)),
    ];

    /// <summary>Schrift- und Rahmenfarben der Stempel (bewusst nur drei).</summary>
    public static readonly (string Name, Color Color)[] StampColors =
    [
        ("Schwarz", Color.Black), ("Rot", Color.FromArgb(192, 0, 0)), ("Grün", Color.FromArgb(0, 128, 0)),
    ];

    /// <summary>Index des Eintrags mit dieser Farbe, sonst 0 (erster Eintrag als Rückfall).</summary>
    public static int IndexOf((string Name, Color? Color)[] palette, Color? color)
    {
        var index = Array.FindIndex(palette, p => p.Color?.ToArgb() == color?.ToArgb());
        return index >= 0 ? index : 0;
    }

    public static int IndexOf((string Name, Color Color)[] palette, Color color)
    {
        var index = Array.FindIndex(palette, p => p.Color.ToArgb() == color.ToArgb());
        return index >= 0 ? index : 0;
    }

    /// <summary>Listeneintrag mit Farbfeld vor dem Namen; null (transparent / kein Rahmen) bekommt ein weißes Feld mit Diagonale.</summary>
    public static void DrawItem(DrawItemEventArgs e, string name, Color? color, Font fallbackFont)
    {
        e.DrawBackground();
        var edge = e.Bounds.Height - 4;
        Rectangle swatch = new(e.Bounds.X + 2, e.Bounds.Y + 2, edge, edge);
        using SolidBrush fill = new(color ?? Color.White);
        e.Graphics.FillRectangle(fill, swatch);
        if (color == null) { e.Graphics.DrawLine(Pens.Gray, swatch.Left, swatch.Bottom, swatch.Right, swatch.Top); }
        e.Graphics.DrawRectangle(Pens.Gray, swatch);
        var textColor = (e.State & DrawItemState.Selected) != 0 ? SystemColors.HighlightText : e.ForeColor;
        TextRenderer.DrawText(e.Graphics, Lng.T(name), e.Font ?? fallbackFont, new Rectangle(swatch.Right + 6, e.Bounds.Y, e.Bounds.Width - edge - 8, e.Bounds.Height), textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        e.DrawFocusRectangle();
    }
}
