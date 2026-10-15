using System.ComponentModel;
using PDFLight.Classes;
using PDFLight.Controls;

namespace PDFLight.Viewer;

/// <summary>Eine eingebettete Datei: <see cref="Index"/> = Position in PDFiums Liste, Größe in Byte (-1 = unbekannt), Änderungsdatum
/// aus /Params /ModDate (null = keins).</summary>
public sealed record AttachmentEntry(int Index, string Name, long Size, DateTime? Modified);

/// <summary>Selbst gezeichnete Anhangliste der Seitenleiste (Wunsch vom 15.10.2026, vorher Reiter „Anhänge“ im Eigenschaften-Dialog):
/// je Anhang das Explorer-Icon seines Dateityps (nur aus der Endung – die Datei selbst wird dafür nicht ausgepackt), daneben der Name,
/// darunter grau Größe und Datum. Zeilen statt Spalten wie bei Acrobat – die Seitenleiste ist dafür zu schmal. Klick oder Pfeiltasten
/// wählen, Doppelklick oder Enter öffnen (<see cref="OpenRequested"/>). Aufbau wie <see cref="AnnotationListView"/>.</summary>
internal sealed class AttachmentListView : ScrollableControl
{
    private const int RowPadding = 8;      // logische px
    private const int LeftMargin = 12;
    private const int RightMargin = 10;
    private const int IconSize = 32;
    private const int IconGap = 10;

    private const TextFormatFlags TextFlags = TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

    private readonly List<AttachmentEntry> entries = [];
    private AttachmentEntry? selected;
    private int hover = -1;
    private Font? smallFont;

    /// <summary>Doppelklick oder Enter.</summary>
    public event EventHandler<AttachmentEntry>? OpenRequested;

    /// <summary>Die Auswahl hat sich geändert (Knöpfe des Bedienfelds nachführen).</summary>
    public event EventHandler? SelectionChanged;

    public AttachmentListView()
    {
        DoubleBuffered = true;
        AutoScroll = true;
        SetStyle(ControlStyles.Selectable | ControlStyles.ResizeRedraw, true);
    }

    [Category("Darstellung"), DefaultValue(typeof(Color), "204, 228, 247"), Description("Hintergrund des gewählten Eintrags.")]
    public Color SelectionBackColor { get; set { field = value; Invalidate(); } } = Color.FromArgb(204, 228, 247);

    [Category("Darstellung"), DefaultValue(typeof(Color), "229, 243, 255"), Description("Hintergrund des Eintrags unter der Maus.")]
    public Color HoverBackColor { get; set { field = value; Invalidate(); } } = Color.FromArgb(229, 243, 255);

    [Category("Darstellung"), DefaultValue(typeof(Color), "GrayText"), Description("Farbe für Größe und Datum.")]
    public Color SecondaryForeColor { get; set { field = value; Invalidate(); } } = SystemColors.GrayText;

    [Category("Darstellung"), DefaultValue(""), Description("Text, der ohne Einträge erscheint.")]
    public string EmptyText { get; set { field = value; Invalidate(); } } = string.Empty;

    [Browsable(false)]
    public AttachmentEntry? SelectedEntry => selected;

    [Browsable(false)]
    public IReadOnlyList<AttachmentEntry> Entries => entries;

    /// <summary>Neue Einträge; die Auswahl bleibt beim gleichnamigen Anhang.</summary>
    public void SetEntries(IReadOnlyList<AttachmentEntry> value)
    {
        var old = selected?.Name;
        entries.Clear();
        entries.AddRange(value);
        selected = old == null ? null : entries.FirstOrDefault(e => e.Name == old);
        hover = -1;
        Relayout();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Wählt den Anhang mit diesem Namen aus (nach dem Hinzufügen).</summary>
    public void SelectByName(string name)
    {
        if (entries.FirstOrDefault(e => e.Name == name) is { } entry) { Select(entry); }
    }

    // ================================================================== Anordnung

    private int Scale(int logical) => LogicalToDeviceUnits(logical);

    private int RowHeight => 2 * Scale(RowPadding) + Math.Max(Scale(IconSize), Font.Height + Scale(4) + (smallFont ?? Font).Height);

    private void Relayout()
    {
        AutoScrollMinSize = new Size(0, entries.Count * RowHeight);
        Invalidate();
    }

    private Rectangle RowBounds(int index) => new(0, index * RowHeight + AutoScrollPosition.Y, ClientSize.Width, RowHeight);

    private int RowAt(Point point)
    {
        if (entries.Count == 0) { return -1; }
        var index = (point.Y - AutoScrollPosition.Y) / Math.Max(1, RowHeight);
        return point.Y >= AutoScrollPosition.Y && index < entries.Count ? index : -1;
    }

    private void UpdateFonts()
    {
        smallFont?.Dispose();
        smallFont = new Font(Font.FontFamily, Font.SizeInPoints * 0.9f);
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        if (smallFont == null) { UpdateFonts(); Relayout(); }
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
        if (disposing) { smallFont?.Dispose(); }
        base.Dispose(disposing);
    }

    // ================================================================== Zeichnen

    /// <summary>„217,20 KB · 12.03.2026 14:20“ – Größe und Datum, was davon bekannt ist.</summary>
    internal static string Details(AttachmentEntry entry)
    {
        var parts = new List<string>(2);
        if (entry.Size >= 0) { parts.Add(FileUtil.FormatSize(entry.Size)); }
        if (entry.Modified is { } modified) { parts.Add(modified.ToString("g", System.Globalization.CultureInfo.CurrentCulture)); }
        return string.Join(" · ", parts);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        if (smallFont == null) { UpdateFonts(); }
        if (entries.Count == 0)
        {
            if (EmptyText.Length > 0) { TextRenderer.DrawText(g, EmptyText, Font, new Point(Scale(LeftMargin), Scale(RowPadding)), SystemColors.GrayText, TextFlags); }
            return;
        }
        for (var i = 0; i < entries.Count; i++)
        {
            var bounds = RowBounds(i);
            if (bounds.Bottom < e.ClipRectangle.Top) { continue; }
            if (bounds.Top > e.ClipRectangle.Bottom) { break; }
            PaintEntry(g, entries[i], bounds, i == hover);
        }
    }

    private void PaintEntry(Graphics g, AttachmentEntry entry, Rectangle bounds, bool hot)
    {
        var back = entry == selected ? SelectionBackColor : hot ? HoverBackColor : Color.Empty;
        if (back != Color.Empty) { using SolidBrush brush = new(back); g.FillRectangle(brush, bounds); }
        var iconSize = Scale(IconSize);
        var icon = new Rectangle(Scale(LeftMargin), bounds.Y + (bounds.Height - iconSize) / 2, iconSize, iconSize);
        // Explorer-Icon des Dateityps, nur aus der Endung (ShellInfo – ohne Plattenzugriff, die Bilder sind geteilt)
        if (ShellInfo.GetTypeIcon(Path.GetExtension(entry.Name), iconSize) is { } image) { g.DrawImage(image, icon); }
        var x = icon.Right + Scale(IconGap);
        var width = Math.Max(Scale(20), bounds.Right - x - Scale(RightMargin));
        var small = smallFont!; // OnPaint hat ihn angelegt
        var textHeight = Font.Height + Scale(4) + small.Height;
        var y = bounds.Y + (bounds.Height - textHeight) / 2;
        TextRenderer.DrawText(g, Ellipsize(entry.Name.Length > 0 ? entry.Name : "?", Font, width), Font, new Point(x, y), ForeColor, TextFlags);
        TextRenderer.DrawText(g, Ellipsize(Details(entry), small, width), small, new Point(x, y + Font.Height + Scale(4)), SecondaryForeColor, TextFlags);
    }

    private static int TextWidth(string text, Font font) => TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue), TextFlags).Width;

    /// <summary>Längster Anfang, der samt „…“ passt (Intervallhalbierung – Namen können lang sein).</summary>
    private static string Ellipsize(string text, Font font, int width)
    {
        if (TextWidth(text, font) <= width) { return text; }
        var (low, high) = (0, text.Length - 1);
        while (low < high)
        {
            var middle = (low + high + 1) / 2;
            if (TextWidth(text[..middle] + "…", font) <= width) { low = middle; } else { high = middle - 1; }
        }
        return text[..low].TrimEnd() + "…";
    }

    // ================================================================== Maus

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var index = RowAt(e.Location);
        if (index == hover) { return; }
        hover = index;
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
        var entry = entries[index];
        if (e.Button == MouseButtons.Left && e.Clicks == 2) { OpenRequested?.Invoke(this, entry); return; }
        if (e.Button is MouseButtons.Left or MouseButtons.Right) { Select(entry); } // Rechtsklick wählt vor dem Kontextmenü
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
        keyData is Keys.Up or Keys.Down or Keys.Home or Keys.End or Keys.PageUp or Keys.PageDown or Keys.Enter || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (entries.Count == 0) { return; }
        var index = selected == null ? -1 : entries.IndexOf(selected);
        var page = Math.Max(1, ClientSize.Height / Math.Max(1, RowHeight));
        switch (e.KeyData)
        {
            case Keys.Up: Select(entries[Math.Max(0, index - 1)]); break;
            case Keys.Down: Select(entries[Math.Min(entries.Count - 1, index + 1)]); break;
            case Keys.PageUp: Select(entries[Math.Max(0, index - page)]); break;
            case Keys.PageDown: Select(entries[Math.Min(entries.Count - 1, Math.Max(0, index) + page)]); break;
            case Keys.Home: Select(entries[0]); break;
            case Keys.End: Select(entries[^1]); break;
            case Keys.Enter when selected != null: OpenRequested?.Invoke(this, selected); break;
            default: return;
        }
        e.Handled = e.SuppressKeyPress = true;
    }

    // ================================================================== Auswahl

    private void Select(AttachmentEntry entry)
    {
        var changed = selected != entry;
        selected = entry;
        var index = entries.IndexOf(entry);
        var top = index * RowHeight + AutoScrollPosition.Y;
        if (top < 0) { AutoScrollPosition = new Point(0, index * RowHeight); }
        else if (top + RowHeight > ClientSize.Height) { AutoScrollPosition = new Point(0, (index + 1) * RowHeight - ClientSize.Height); }
        Invalidate();
        if (changed) { SelectionChanged?.Invoke(this, EventArgs.Empty); }
    }
}
