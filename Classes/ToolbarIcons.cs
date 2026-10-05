using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
    public const char MoveToFolder = '\uE8DE';
    public const char Copy = '\uE8C8';
    public const char Rename = '\uE8AC';
    public const char Delete = '\uE74D';
    public const char Print = '\uE749';
    public const char Mail = '\uE715';
    public const char Edit = '\uE70F';
    public const char AllApps = '\uE71D';
    public const char FolderOpen = '\uE838';
    public const char Settings = '\uE713';
    public const char Info = '\uE946';
    public const char Eye = '\uE7B3';          // RedEye: Kennwort sichtbar machen
    public const char Rotate = '\uE7AD';       // Bearbeiten-Menü: Seiten drehen
    public const char MovePage = '\uE8CB';     // Sort: aktuelle Seite verschieben (Bearbeiten-Menü)
    public const char NewPage = '\uE8A5';      // Document: leere Seite einfügen (Bearbeiten-Menü)
    public const char Stamp = '\uE7C1';        // Flag: Stempel einfügen (Bearbeiten-Menü)
    public const char List = '\uE8FD';         // Stempel verwalten
    public const char Attach = '\uE723';       // PDF-Datei anhängen
    public const char Interleave = '\uE8AB';   // Switch: Rückseiten-Scan verzahnen
    public const char Page = '\uE7C3';         // Seiten extrahieren
    public const char Undo = '\uE7A7';
    public const char Lock = '\uE72E';         // Kennwort vergeben
    public const char Unlock = '\uE785';       // Kennwort entfernen
    public const char Clear = '\uE894';        // Liste leeren
    public const char Keyboard = '\uE765';     // Tastenkürzel-Menüpunkt
    public const char UpdateSearch = '\uE777'; // UpdateRestore: nach Updates suchen
    public const char OpenWith = '\uE7AC';     // Menüpunkt Öffnen mit
    public const char Help = '\uE9CE';         // Fragezeichen im Kreis: Hilfe-Menü
    public const char Add = '\uE710';          // Plus: runder „Neuer Ordner“-Knopf im Verschieben-Dialog
    public const char NewFolder = '\uE8F4';    // Kontextmenü des Ordnerbaums
    public const char Comment = '\uE90A';      // Textanmerkung einfügen (Bearbeiten-Menü)
    public const char Favorite = '\uE734';     // FavoriteStar: Favoriten-Menü und Hinzufügen-Eintrag
    public const char Bookmarks = '\uE8A4';    // Lesezeichen entfernen (Bearbeiten-Menü)
    public const char Sidebar = '\uE700';      // GlobalNavigationButton (☰): Seitenleiste der Anzeige ein/aus
    public const char ZoomIn = '\uE8A3';
    public const char ZoomOut = '\uE71F';
    public const char FitWidth = '\uE799';     // Seite zwischen senkrechten Strichen: an Breite anpassen
    public const char FitPage = '\uE9A6';      // FitPage (Ecken): an Seite anpassen
    public const char FullScreen = '\uE740';   // Vollbild ein
    public const char BackToWindow = '\uE73F'; // Vollbild aus
    public const char CloseDocument = '\uE8BB'; // ChromeClose: Dokument schließen
    public const char Save = '\uE74E';         // Formulareingaben speichern (Viewer-Leiste)
    public const char TwoPage = '\uE89A';    // TwoPage: zweiseitige Ansicht
    public const char Search = '\uE721';
    public const char ChevronUp = '\uE70E';    // Suche: vorheriger Treffer (nächster: ChevronDown)
    public const char Cancel = '\uE711';       // Suchleiste schließen
    public const char Draw = '\uE76D';         // InkingTool: Zeichnen in der Viewer-Leiste (wie Edge)
    public const char Erase = '\uE75C';        // EraseTool: Radierer in der Viewer-Leiste (wie Chrome)
    public const char ManageAnnotations = '\uF0E3'; // ClipboardList: \u201EAnmerkungen verwalten\u201C in der Viewer-Leiste (E8FD ist die Dokumentstruktur)
    public const char HighlightFill = '\uE891'; // F\u00FCllung zu Highlight: Kappe und Spitze des Markers in der gew\u00E4hlten Farbe
    public const char DrawFill = '\uE88F';     // InkingColorOutline: Kappe und Stiftspitze zu Draw in der Stiftfarbe
    public const char Thumbnails = '\uEB9F';   // Photo2 (Bild mit Bergen): Seitenleiste mit Miniaturen (wie Chrome)
    public const char Outline = '\uE8FD';      // BulletedList: Seitenleiste mit der Dokumentstruktur
    public const char SelectAll = '\uE8B3';    // Kontextmenü der Anzeige: alles markieren

    public const char Highlight = '';    // Textmarker: Kontextmenü „Hervorheben“ der Anzeige

    private const string FontName = "Segoe MDL2 Assets";
    private static readonly Dictionary<(char Glyph, int Size, int Color), Image> cache = [];

    /// <summary>False, falls die Symbolschrift fehlt — dann bleiben die Buttons reine Textbuttons.</summary>
    public static bool FontAvailable { get; } = CheckFontAvailable();

    /// <summary>Ob Menüeinträge Symbole bekommen (Einstellung „Symbole anzeigen“ und Schrift vorhanden) –
    /// setzt MainForm beim Anwenden der Einstellungen; die Dialoge fragen nur noch hier nach.</summary>
    public static bool MenuIconsEnabled { get; set; }

    /// <summary>16-px-Symbol für einen Menüeintrag in der DPI-Skalierung des Controls; null, wenn Symbole abgeschaltet sind.</summary>
    public static Image? MenuIcon(char glyph, Control control) =>
        MenuIconsEnabled ? Get(glyph, control.LogicalToDeviceUnits(new Size(16, 16))) : null;

    /// <summary>Menüsymbol für einen Textbutton (Anmerkungsliste, Lesezeichen-Editor): die Glyphe sitzt zentriert im Zeilenkasten
    /// und wirkt neben dem Text zu hoch – ein paar Pixel Luft oben rücken sie optisch auf die Textmitte.</summary>
    public static Bitmap? ButtonIcon(char glyph, Control control)
    {
        var icon = MenuIcon(glyph, control);
        if (icon == null) { return null; }
        var shift = control.LogicalToDeviceUnits(3);
        Bitmap padded = new(icon.Width, icon.Height + shift);
        using var g = Graphics.FromImage(padded);
        g.DrawImageUnscaled(icon, 0, shift);
        return padded;
    }

    public const char ChevronDown = '';  // Suche: nächster Treffer

    private static readonly Dictionary<int, Font> glyphFonts = []; // je Pixelgröße eine Schrift, lebt bis zum Programmende

    /// <summary>Die Symbolschrift in der gewünschten Pixelgröße für Glyphen, die als Text gezeichnet werden (Aufklapp-Pfeile);
    /// null, wenn „Segoe MDL2 Assets“ fehlt – der Aufrufer zeichnet dann einen Ersatz.</summary>
    public static Font? GlyphFont(int pixelSize)
    {
        if (!FontAvailable) { return null; }
        lock (glyphFonts)
        {
            if (!glyphFonts.TryGetValue(pixelSize, out var font)) { font = new Font(FontName, pixelSize, GraphicsUnit.Pixel); glyphFonts[pixelSize] = font; }
            return font;
        }
    }

    private static bool CheckFontAvailable()
    {
        using Font font = new(FontName, 10f);
        return string.Equals(font.Name, FontName, StringComparison.OrdinalIgnoreCase); // GDI fällt sonst stumm auf eine Standardschrift zurück
    }

    /// <summary>Graustufen-Kopie eines Bildes — für die Programm-Symbole in der Symbolleiste,
    /// damit sie sich den einfarbigen MDL2-Symbolen unterordnen (im Menü bleiben sie farbig).</summary>
    public static Image? ToGrayscale(Image? source)
    {
        if (source == null) { return null; }
        Bitmap result = new(source.Width, source.Height);
        using var g = Graphics.FromImage(result);
        using ImageAttributes attributes = new();
        // Luminanz auf 0,7 gestaucht und um 0,3 angehoben: Schwarz wird zu mittlerem Grau,
        // Weiß bleibt Weiß — sonst gerieten satte Farben (Acrobat-Rot) fast schwarz
        attributes.SetColorMatrix(new ColorMatrix(
        [
            [0.209f, 0.209f, 0.209f, 0f, 0f],
            [0.411f, 0.411f, 0.411f, 0f, 0f],
            [0.080f, 0.080f, 0.080f, 0f, 0f],
            [0f, 0f, 0f, 1f, 0f],
            [0.3f, 0.3f, 0.3f, 0f, 1f],
        ]));
        g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
        return result;
    }

    /// <summary>„T im Kasten“ wie Edges Schaltfläche „Text hinzufügen“ – die Symbolschrift hat dafür keine Glyphe.</summary>
    public static Image TextBox(Size size, Color color)
    {
        Bitmap bitmap = new(size.Width, size.Height);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        var line = Math.Max(1f, size.Height / 16f);
        var inset = size.Height * 0.16f;
        var box = new RectangleF(inset, inset, size.Width - 2 * inset, size.Height - 2 * inset);
        using (var pen = new Pen(color, line)) { g.DrawRectangle(pen, box.X, box.Y, box.Width, box.Height); }
        using Font font = new("Segoe UI", box.Height * 0.62f, FontStyle.Regular, GraphicsUnit.Pixel);
        using StringFormat format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using SolidBrush brush = new(color);
        g.DrawString("T", font, brush, new RectangleF(box.X, box.Y + line / 2, box.Width, box.Height), format);
        return bitmap;
    }

    /// <summary>Symbol mit farbig gefüllten Teilen wie bei Edge – „Hervorheben“ und „Zeichnen“ zeigen so die gewählte Farbe in Kappe und
    /// Spitze des Markers bzw. in der Stiftspitze: erst die Füllglyphe in <paramref name="fillColor"/> (erst ab <paramref name="fillTop"/>
    /// als Anteil der Höhe, um Teile oben abzuschneiden), darüber der Umriss. Neues Bild, nicht im Zwischenspeicher – der Aufrufer gibt es frei.</summary>
    public static Image WithFill(char outline, char fill, Size size, Color color, Color fillColor, float fillTop = 0)
    {
        Bitmap bitmap = new(size.Width, size.Height);
        using var g = Graphics.FromImage(bitmap);
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        using Font font = new(FontName, size.Height * 0.75f, GraphicsUnit.Pixel);
        using StringFormat format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        var bounds = new RectangleF(0, 0, size.Width, size.Height);
        g.SetClip(new RectangleF(0, size.Height * fillTop, size.Width, size.Height));
        using (SolidBrush brush = new(fillColor)) { g.DrawString(fill.ToString(), font, brush, bounds, format); }
        g.ResetClip();
        using (SolidBrush brush = new(color)) { g.DrawString(outline.ToString(), font, brush, bounds, format); }
        return bitmap;
    }

    /// <summary>Aufklappzeichen wie bei Edge, mittig in <paramref name="area"/>: gefülltes „▶“ für zu, „◢“ für auf (Wunsch vom
    /// 05.10.2026 – auffälliger als feine Pfeile, man muss ja darauf klicken). Dokumentstruktur und Lesezeichen-Editor teilen es sich;
    /// mit GDI+ geglättet gezeichnet, unabhängig von der Symbolschrift. <paramref name="scale"/> = Gerätepixel je logischem Pixel.</summary>
    public static void DrawExpander(Graphics g, Rectangle area, bool expanded, Color color, float scale)
    {
        var (cx, cy) = (area.X + area.Width / 2f, area.Y + area.Height / 2f);
        PointF[] points;
        if (expanded)
        {
            var half = 3 * scale; // Schenkel 6 px
            points = [new(cx + half, cy - half), new(cx + half, cy + half), new(cx - half, cy + half)];
        }
        else
        {
            var (half, depth) = (4 * scale, 5 * scale); // 8 px hoch, 5 px breit
            points = [new(cx - depth / 2, cy - half), new(cx + depth / 2, cy), new(cx - depth / 2, cy + half)];
        }
        var smoothing = g.SmoothingMode;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using SolidBrush brush = new(color);
        g.FillPolygon(brush, points);
        g.SmoothingMode = smoothing;
    }

    public static Image Get(char glyph, Size size) => Get(glyph, size, Color.FromArgb(64, 64, 64));

    /// <summary>Wie <see cref="Get(char, Size)"/> in einer anderen Farbe – etwa hell für die dunkle Viewer-Leiste.</summary>
    public static Image Get(char glyph, Size size, Color color)
    {
        if (!cache.TryGetValue((glyph, size.Width, color.ToArgb()), out var image))
        {
            Bitmap bitmap = new(size.Width, size.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                using Font font = new(FontName, size.Height * 0.75f, GraphicsUnit.Pixel);
                using StringFormat format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using SolidBrush brush = new(color);
                g.DrawString(glyph.ToString(), font, brush, new RectangleF(0, 0, size.Width, size.Height), format);
            }
            image = bitmap;
            cache[(glyph, size.Width, color.ToArgb())] = image;
        }
        return image;
    }
}
