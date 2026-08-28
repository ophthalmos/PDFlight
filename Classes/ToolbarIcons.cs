using System.Drawing;
using System.Drawing.Text;

namespace PDFLight.Classes;

/// <summary>
/// Rendert Symbole für die Toolbar aus der Windows-Symbolschrift "Segoe MDL2 Assets"
/// (ab Windows 10 vorinstalliert) — DPI-scharf und ohne eingebettete Bilddateien.
/// Die Glyphen-Codes: https://learn.microsoft.com/windows/apps/design/style/segoe-ui-symbol-font
/// </summary>
internal static class ToolbarIcons
{
    public const char OpenFile = '\uE8E5';
    public const char Previous = '\uE76B';     // ChevronLeft
    public const char Next = '\uE76C';         // ChevronRight
    public const char PageUp = '\uE70E';       // ChevronUp
    public const char PageDown = '\uE70D';     // ChevronDown
    public const char MoveToFolder = '\uE8DE';
    public const char Copy = '\uE8C8';
    public const char Rename = '\uE8AC';
    public const char Delete = '\uE74D';
    public const char Mail = '\uE715';
    public const char Edit = '\uE70F';
    public const char AllApps = '\uE71D';
    public const char FolderOpen = '\uE838';
    public const char Settings = '\uE713';
    public const char Info = '\uE946';

    private const string FontName = "Segoe MDL2 Assets";
    private static readonly Dictionary<(char Glyph, int Size), Image> cache = [];

    /// <summary>False, falls die Symbolschrift fehlt — dann bleiben die Buttons reine Textbuttons.</summary>
    public static bool FontAvailable { get; } = CheckFontAvailable();

    private static bool CheckFontAvailable()
    {
        using Font font = new(FontName, 10f);
        return string.Equals(font.Name, FontName, StringComparison.OrdinalIgnoreCase); // GDI fällt sonst stumm auf eine Standardschrift zurück
    }

    public static Image Get(char glyph, Size size)
    {
        if (!cache.TryGetValue((glyph, size.Width), out var image))
        {
            Bitmap bitmap = new(size.Width, size.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                using Font font = new(FontName, size.Height * 0.75f, GraphicsUnit.Pixel);
                using StringFormat format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using SolidBrush brush = new(Color.FromArgb(64, 64, 64));
                g.DrawString(glyph.ToString(), font, brush, new RectangleF(0, 0, size.Width, size.Height), format);
            }
            image = bitmap;
            cache[(glyph, size.Width)] = image;
        }
        return image;
    }
}
