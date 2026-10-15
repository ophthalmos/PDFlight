using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using PDFLight.Classes;

namespace PDFLight.Viewer;

/// <summary>Eine Anmerkung der Seite <see cref="Page"/> (0-basiert) für die Liste der Seitenleiste: <see cref="Number"/> ist die
/// Position im /Annots-Array der Seite (dieselbe Nummer wie in PDFium und PDFsharp), <see cref="Subtype"/> die Art (FPDF_ANNOT_*),
/// <see cref="Bounds"/> die Lage als Anteil der angezeigten Seite (0…1).</summary>
public sealed record AnnotationEntry(int Page, int Number, int Subtype, string Author, DateTime? Modified, string Contents, Color? Color, RectangleF Bounds);

/// <summary>Eine Gruppe der Liste – je nach Sortierung eine Seite, ein Autor, ein Tag oder eine Art.</summary>
public sealed record AnnotationGroup(string Key, string Header, IReadOnlyList<AnnotationEntry> Entries);

/// <summary>Selbst gezeichnete Anmerkungsliste der Seitenleiste nach dem Vorbild von Acrobats Kommentarliste (Wunsch vom 15.10.2026,
/// ersetzt „Anmerkungen verwalten“): Gruppenköpfe mit Anzahl zum Auf- und Zuklappen, darunter je Anmerkung ein rundes Symbol in ihrer
/// Farbe, Autor, Datum und Text (umbrochen, höchstens <see cref="MaxTextLines"/> Zeilen) bzw. kursiv die Art, wenn sie keinen Text hat.
/// Klick oder Pfeiltasten wählen (<see cref="EntryActivated"/>), Doppelklick oder Enter bearbeiten, Entf löscht – was daraus folgt,
/// entscheidet das Hauptfenster. Aufbau wie <see cref="OutlineView"/>.</summary>
internal sealed class AnnotationListView : ScrollableControl
{
    private const int HeaderHeight = 30; // logische px
    private const int EntryPadding = 10;
    private const int LeftMargin = 12;
    private const int RightMargin = 10;
    private const int IconSize = 24;
    private const int IconGap = 8;
    private const int LineHeight = 20;
    private const int MaxTextLines = 6;

    private const TextFormatFlags TextFlags = TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

    /// <summary>Eine sichtbare Zeile: Gruppenkopf (Entry null) oder Anmerkung samt umbrochener Textzeilen.</summary>
    private sealed record Row(AnnotationGroup Group, AnnotationEntry? Entry, int Top, int Height, List<string> Lines, bool Placeholder);

    private readonly List<AnnotationGroup> groups = [];
    private readonly HashSet<string> collapsed = [];
    private readonly List<Row> rows = [];
    private AnnotationEntry? selected;
    private int hover = -1;
    private int layoutWidth = -1;
    private Font? boldFont, smallFont, italicFont;

    /// <summary>Auswahl per Klick oder Tastatur.</summary>
    public event EventHandler<AnnotationEntry>? EntryActivated;

    /// <summary>Doppelklick oder Enter.</summary>
    public event EventHandler<AnnotationEntry>? EditRequested;

    /// <summary>Entf.</summary>
    public event EventHandler<AnnotationEntry>? DeleteRequested;

    public AnnotationListView()
    {
        DoubleBuffered = true;
        AutoScroll = true;
        SetStyle(ControlStyles.Selectable | ControlStyles.ResizeRedraw, true);
    }

    [Category("Darstellung"), DefaultValue(typeof(Color), "204, 228, 247"), Description("Hintergrund des gewählten Eintrags.")]
    public Color SelectionBackColor { get; set { field = value; Invalidate(); } } = Color.FromArgb(204, 228, 247);

    [Category("Darstellung"), DefaultValue(typeof(Color), "229, 243, 255"), Description("Hintergrund des Eintrags unter der Maus.")]
    public Color HoverBackColor { get; set { field = value; Invalidate(); } } = Color.FromArgb(229, 243, 255);

    [Category("Darstellung"), DefaultValue(typeof(Color), "240, 240, 240"), Description("Hintergrund der Gruppenköpfe.")]
    public Color HeaderBackColor { get; set { field = value; Invalidate(); } } = Color.FromArgb(240, 240, 240);

    [Category("Darstellung"), DefaultValue(typeof(Color), "GrayText"), Description("Farbe für Datum, Anzahl und die Art ohne Text.")]
    public Color SecondaryForeColor { get; set { field = value; Invalidate(); } } = SystemColors.GrayText;

    [Category("Darstellung"), DefaultValue(""), Description("Text, der ohne Einträge erscheint.")]
    public string EmptyText { get; set { field = value; Invalidate(); } } = string.Empty;

    /// <summary>Wie eine Anmerkung ohne Text heißt (ihre Art, übersetzt) – setzt das Bedienfeld.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<AnnotationEntry, string> TypeName { get; set; } = e => e.Subtype.ToString();

    [Browsable(false)]
    public AnnotationEntry? SelectedEntry => selected;

    /// <summary>Alle Einträge in Anzeigereihenfolge (auch die zugeklappter Gruppen).</summary>
    [Browsable(false)]
    public IEnumerable<AnnotationEntry> Entries => groups.SelectMany(g => g.Entries);

    [Browsable(false)]
    public bool AllExpanded => collapsed.Count == 0 || groups.All(g => !collapsed.Contains(g.Key));

    [Browsable(false)]
    public bool AllCollapsed => groups.Count > 0 && groups.All(g => collapsed.Contains(g.Key));

    /// <summary>Neue Gruppen setzen. Die Auswahl bleibt bei derselben Anmerkung (Seite, Nummer, Art, Text); fehlt sie – etwa weil sie
    /// gerade gelöscht wurde und die Nummern dahinter nachgerückt sind –, rückt mit <paramref name="keepPosition"/> der Eintrag an
    /// ihrer Stelle nach. Zugeklappte Gruppen bleiben zu.</summary>
    public void SetGroups(IReadOnlyList<AnnotationGroup> value, bool keepPosition = false)
    {
        var oldIndex = selected == null ? -1 : Entries.ToList().IndexOf(selected);
        var old = selected;
        groups.Clear();
        groups.AddRange(value);
        selected = null;
        if (old != null)
        {
            var all = Entries.ToList();
            selected = all.FirstOrDefault(e => e.Page == old.Page && e.Number == old.Number && e.Subtype == old.Subtype && e.Contents == old.Contents)
                ?? (keepPosition && all.Count > 0 && oldIndex >= 0 ? all[Math.Min(oldIndex, all.Count - 1)] : null);
        }
        hover = -1;
        Relayout();
        if (selected != null) { EnsureVisible(selected); }
    }

    public void ExpandAll()
    {
        collapsed.Clear();
        Relayout();
    }

    public void CollapseAll()
    {
        foreach (var group in groups) { collapsed.Add(group.Key); }
        Relayout();
    }

    /// <summary>Wählt die Anmerkung aus, ohne <see cref="EntryActivated"/> zu melden; ihre Gruppe klappt dafür auf.</summary>
    public void SelectEntry(AnnotationEntry? entry)
    {
        selected = entry;
        if (entry != null && groups.FirstOrDefault(g => g.Entries.Contains(entry)) is { } group && collapsed.Remove(group.Key)) { Relayout(); }
        if (entry != null) { EnsureVisible(entry); }
        Invalidate();
    }

    // ================================================================== Anordnung

    private int Scale(int logical) => LogicalToDeviceUnits(logical);

    private void Relayout()
    {
        layoutWidth = Width;
        var height = ArrangeRows(Width - Scale(RightMargin));
        if (height > Height) { height = ArrangeRows(Width - SystemInformation.VerticalScrollBarWidth - Scale(RightMargin)); }
        AutoScrollMinSize = new Size(0, height);
        Invalidate();
    }

    private int ArrangeRows(int right)
    {
        rows.Clear();
        var top = 0;
        var left = Scale(LeftMargin);
        var width = Math.Max(Scale(40), right - left);
        foreach (var group in groups)
        {
            rows.Add(new Row(group, null, top, Scale(HeaderHeight), [], false));
            top += Scale(HeaderHeight);
            if (collapsed.Contains(group.Key)) { continue; }
            foreach (var entry in group.Entries)
            {
                var placeholder = string.IsNullOrWhiteSpace(entry.Contents);
                var lines = placeholder ? [TypeName(entry)] : Wrap(entry.Contents, width, Font);
                var height = 2 * Scale(EntryPadding) + Scale(IconSize) + Scale(4) + lines.Count * Scale(LineHeight);
                rows.Add(new Row(group, entry, top, height, lines, placeholder));
                top += height;
            }
        }
        return top;
    }

    private static int TextWidth(string text, Font font) => TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue), TextFlags).Width;

    /// <summary>Absätze (Zeilenumbrüche, auch Acrobats reine CR) und darin wortweise auf <paramref name="width"/> umbrechen; überlange
    /// Wörter zeichenweise. Höchstens <see cref="MaxTextLines"/> Zeilen, die letzte dann mit „…“.</summary>
    private static List<string> Wrap(string text, int width, Font font)
    {
        List<string> lines = [];
        foreach (var paragraph in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            var line = string.Empty;
            foreach (var word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var candidate = line.Length == 0 ? word : line + " " + word;
                if (TextWidth(candidate, font) <= width) { line = candidate; continue; }
                if (line.Length > 0) { lines.Add(line); }
                line = word;
                while (line.Length > 1 && TextWidth(line, font) > width)
                {
                    var fit = 1;
                    while (fit < line.Length && TextWidth(line[..(fit + 1)], font) <= width) { fit++; }
                    lines.Add(line[..fit]);
                    line = line[fit..];
                }
            }
            lines.Add(line);
            if (lines.Count > MaxTextLines) { break; }
        }
        while (lines.Count > 1 && lines[^1].Length == 0) { lines.RemoveAt(lines.Count - 1); } // Leerzeilen am Ende
        if (lines.Count > MaxTextLines)
        {
            lines.RemoveRange(MaxTextLines, lines.Count - MaxTextLines);
            var last = lines[^1];
            while (last.Length > 0 && TextWidth(last + "…", font) > width) { last = last[..^1]; }
            lines[^1] = last.TrimEnd() + "…";
        }
        return lines;
    }

    private Rectangle RowBounds(Row row) => new(0, row.Top + AutoScrollPosition.Y, ClientSize.Width, row.Height);

    private int RowAt(Point point)
    {
        for (var i = 0; i < rows.Count; i++)
        {
            if (RowBounds(rows[i]).Contains(point)) { return i; }
        }
        return -1;
    }

    private void UpdateFonts()
    {
        boldFont?.Dispose();
        smallFont?.Dispose();
        italicFont?.Dispose();
        boldFont = new Font(Font, FontStyle.Bold);
        smallFont = new Font(Font.FontFamily, Font.SizeInPoints * 0.9f);
        italicFont = new Font(Font, FontStyle.Italic);
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        if (boldFont == null) { UpdateFonts(); Relayout(); }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (Width != layoutWidth) { Relayout(); }
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        UpdateFonts();
        Relayout();
    }

    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);
        Relayout();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { boldFont?.Dispose(); smallFont?.Dispose(); italicFont?.Dispose(); }
        base.Dispose(disposing);
    }

    // ================================================================== Zeichnen

    /// <summary>Symbol je Art aus der Symbolschrift.</summary>
    private static char GlyphOf(int subtype) => subtype switch
    {
        1 => ToolbarIcons.Comment,                       // Notiz
        3 => 'T',                                         // Freitext (wie das „T im Kasten“ der Viewer-Leiste)
        >= 9 and <= 12 => ToolbarIcons.Highlight,         // Textmarkierungen
        13 => ToolbarIcons.Stamp,
        15 => ToolbarIcons.Draw,                          // Zeichnung
        17 => ToolbarIcons.Attach,                        // Dateianhang
        _ => ToolbarIcons.Edit,                           // Formen und alles Übrige
    };

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        if (boldFont == null) { UpdateFonts(); }
        if (rows.Count == 0)
        {
            if (EmptyText.Length > 0) { TextRenderer.DrawText(g, EmptyText, Font, new Point(Scale(LeftMargin), Scale(EntryPadding)), SystemColors.GrayText, TextFlags); }
            return;
        }
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var bounds = RowBounds(row);
            if (bounds.Bottom < e.ClipRectangle.Top) { continue; }
            if (bounds.Top > e.ClipRectangle.Bottom) { break; }
            if (row.Entry == null) { PaintHeader(g, row, bounds, i == hover); }
            else { PaintEntry(g, row, row.Entry, bounds, i == hover); }
        }
    }

    private void PaintHeader(Graphics g, Row row, Rectangle bounds, bool hot)
    {
        using (SolidBrush brush = new(hot ? HoverBackColor : HeaderBackColor)) { g.FillRectangle(brush, bounds); }
        var y = bounds.Y + (bounds.Height - boldFont!.Height) / 2; // boldFont: OnPaint hat ihn angelegt
        TextRenderer.DrawText(g, row.Group.Header, boldFont, new Point(Scale(LeftMargin), y), ForeColor, TextFlags);
        var glyph = new Rectangle(bounds.Right - Scale(RightMargin) - Scale(16), bounds.Y, Scale(16), bounds.Height);
        var count = row.Group.Entries.Count.ToString(System.Globalization.CultureInfo.CurrentCulture);
        var countWidth = TextWidth(count, Font);
        TextRenderer.DrawText(g, count, Font, new Point(glyph.Left - Scale(6) - countWidth, bounds.Y + (bounds.Height - Font.Height) / 2), SecondaryForeColor, TextFlags);
        ToolbarIcons.DrawExpander(g, glyph, !collapsed.Contains(row.Group.Key), ForeColor, DeviceDpi / 96f);
    }

    private void PaintEntry(Graphics g, Row row, AnnotationEntry entry, Rectangle bounds, bool hot)
    {
        var back = entry == selected ? SelectionBackColor : hot ? HoverBackColor : Color.Empty;
        if (back != Color.Empty) { using SolidBrush brush = new(back); g.FillRectangle(brush, bounds); }
        var left = Scale(LeftMargin);
        var top = bounds.Y + Scale(EntryPadding);
        var icon = new Rectangle(left, top, Scale(IconSize), Scale(IconSize));
        PaintIcon(g, icon, entry);
        // Kopfzeile neben dem Symbol: Autor fett, Datum grau
        var x = icon.Right + Scale(IconGap);
        if (entry.Author.Length > 0)
        {
            var author = Ellipsize(entry.Author, boldFont!, Math.Max(Scale(20), bounds.Width - x - Scale(RightMargin) - Scale(90)));
            TextRenderer.DrawText(g, author, boldFont, new Point(x, icon.Y + (icon.Height - boldFont!.Height) / 2), ForeColor, TextFlags);
            x += TextWidth(author, boldFont) + Scale(8);
        }
        if (entry.Modified is { } modified)
        {
            var date = modified.ToString("g", System.Globalization.CultureInfo.CurrentCulture);
            TextRenderer.DrawText(g, Ellipsize(date, smallFont!, Math.Max(0, bounds.Width - x - Scale(RightMargin))), smallFont, new Point(x, icon.Y + (icon.Height - smallFont!.Height) / 2), SecondaryForeColor, TextFlags);
        }
        // Text unter dem Symbol, jede Zeile mittig in ihrer festen Zeilenhöhe
        var font = row.Placeholder ? italicFont! : Font;
        var color = row.Placeholder ? SecondaryForeColor : ForeColor;
        var y = icon.Bottom + Scale(4) + (Scale(LineHeight) - font.Height) / 2;
        foreach (var line in row.Lines)
        {
            TextRenderer.DrawText(g, line, font, new Point(left, y), color, TextFlags);
            y += Scale(LineHeight);
        }
    }

    private static string Ellipsize(string text, Font font, int width)
    {
        if (TextWidth(text, font) <= width) { return text; }
        while (text.Length > 1 && TextWidth(text + "…", font) > width) { text = text[..^1]; }
        return text + "…";
    }

    /// <summary>Rundes Symbol in der Farbe der Anmerkung (ohne Farbe grau), die Glyphe hell oder dunkel – je nachdem, was sich abhebt.</summary>
    private void PaintIcon(Graphics g, Rectangle icon, AnnotationEntry entry)
    {
        var fill = entry.Color ?? Color.FromArgb(110, 110, 110);
        var luminance = (0.299 * fill.R + 0.587 * fill.G + 0.114 * fill.B) / 255;
        var glyphColor = luminance > 0.6 ? Color.FromArgb(40, 40, 40) : Color.White;
        var smoothing = g.SmoothingMode;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (SolidBrush brush = new(fill)) { g.FillEllipse(brush, icon); }
        if (luminance > 0.92) { using Pen pen = new(Color.FromArgb(180, 180, 180)); g.DrawEllipse(pen, icon); } // Weiß (Umkehren) auf weißem Grund
        g.SmoothingMode = smoothing;
        var glyph = GlyphOf(entry.Subtype);
        var hint = g.TextRenderingHint;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        using StringFormat format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using SolidBrush text = new(glyphColor);
        if (glyph == 'T' || !ToolbarIcons.FontAvailable)
        {
            using Font letter = new("Segoe UI", icon.Height * 0.5f, FontStyle.Bold, GraphicsUnit.Pixel);
            g.DrawString(glyph == 'T' ? "T" : "•", letter, text, icon, format); // ohne Symbolschrift ein Punkt
        }
        else if (ToolbarIcons.GlyphFont((int)(icon.Height * 0.55f)) is { } symbols)
        {
            g.DrawString(glyph.ToString(), symbols, text, icon, format);
        }
        g.TextRenderingHint = hint;
    }

    // ================================================================== Maus

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var index = RowAt(e.Location);
        if (index == hover) { return; }
        hover = index;
        Cursor = index >= 0 ? Cursors.Hand : Cursors.Default;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        hover = -1;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus(); // für Tastatur und Mausrad
        if (RowAt(e.Location) is not (>= 0 and var index)) { return; }
        var row = rows[index];
        if (row.Entry == null)
        {
            if (e.Button == MouseButtons.Left) { Toggle(row.Group); }
            return;
        }
        if (e.Button == MouseButtons.Left && e.Clicks == 2) { EditRequested?.Invoke(this, row.Entry); return; }
        if (e.Button is MouseButtons.Left or MouseButtons.Right) { Select(row.Entry, activate: true); } // Rechtsklick wählt vor dem Kontextmenü
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        Invalidate();
    }

    protected override void OnScroll(ScrollEventArgs se)
    {
        base.OnScroll(se);
        Invalidate();
    }

    // ================================================================== Tastatur

    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Up or Keys.Down or Keys.Left or Keys.Right or Keys.Home or Keys.End or Keys.PageUp or Keys.PageDown or Keys.Enter or Keys.Delete || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var entries = rows.Where(r => r.Entry != null).Select(r => r.Entry!).ToList(); // nur sichtbare Einträge
        var index = selected == null ? -1 : entries.IndexOf(selected);
        var page = Math.Max(1, ClientSize.Height / Math.Max(1, Scale(80)));
        switch (e.KeyData)
        {
            case Keys.Up when entries.Count > 0: Select(entries[Math.Max(0, index - 1)], activate: true); break;
            case Keys.Down when entries.Count > 0: Select(entries[Math.Min(entries.Count - 1, index + 1)], activate: true); break;
            case Keys.PageUp when entries.Count > 0: Select(entries[Math.Max(0, index - page)], activate: true); break;
            case Keys.PageDown when entries.Count > 0: Select(entries[Math.Min(entries.Count - 1, Math.Max(0, index) + page)], activate: true); break;
            case Keys.Home when entries.Count > 0: Select(entries[0], activate: true); break;
            case Keys.End when entries.Count > 0: Select(entries[^1], activate: true); break;
            case Keys.Left when GroupOf(selected) is { } group && !collapsed.Contains(group.Key): Toggle(group); break;
            case Keys.Right when GroupOf(selected) is { } group && collapsed.Contains(group.Key): Toggle(group); break;
            case Keys.Enter when selected != null: EditRequested?.Invoke(this, selected); break;
            case Keys.Delete when selected != null: DeleteRequested?.Invoke(this, selected); break;
            default: return;
        }
        e.Handled = e.SuppressKeyPress = true;
    }

    private AnnotationGroup? GroupOf(AnnotationEntry? entry) => entry == null ? null : groups.FirstOrDefault(g => g.Entries.Contains(entry));

    // ================================================================== Auswahl und Aufklappen

    private void Select(AnnotationEntry entry, bool activate)
    {
        selected = entry;
        EnsureVisible(entry);
        Invalidate();
        if (activate) { EntryActivated?.Invoke(this, entry); }
    }

    private void Toggle(AnnotationGroup group)
    {
        if (!collapsed.Remove(group.Key)) { collapsed.Add(group.Key); }
        Relayout();
    }

    private void EnsureVisible(AnnotationEntry entry)
    {
        if (rows.FirstOrDefault(r => r.Entry == entry) is not { } row) { return; }
        var top = row.Top + AutoScrollPosition.Y;
        if (top < 0) { AutoScrollPosition = new Point(0, row.Top); }
        else if (top + row.Height > ClientSize.Height) { AutoScrollPosition = new Point(0, row.Top + row.Height - ClientSize.Height); }
        Invalidate();
    }
}
