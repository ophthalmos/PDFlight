using System.ComponentModel;
using PDFLight.Classes;

namespace PDFLight.Viewer;

/// <summary>Selbst gezeichnete Dokumentstruktur (Lesezeichen) für die Seitenleiste, nach dem Vorbild von Chrome (Wunsch vom 04.10.2026):
/// Pfeile statt Plus/Minus, keine Linien, lange Titel brechen um – jede Zeile ist so hoch wie ihr Text (der Windows-TreeView kennt nur
/// eine Zeilenhöhe für alle Knoten). Klick auf den Pfeil klappt auf/zu, Klick auf den Titel meldet <see cref="ItemActivated"/>; die
/// Tastatur arbeitet wie beim TreeView (↑↓ wählen und springen, → aufklappen bzw. zum ersten Unterpunkt, ← zuklappen bzw. zum
/// übergeordneten Punkt, Enter springt, Leertaste klappt um).</summary>
internal sealed class OutlineView : ScrollableControl
{
    // Maße wie in Chromes PDF-Seitenleiste (gemessen 04.10.2026 bei 100 %): Segoe UI 13 px (9,75 pt, im Designer), Zeilenabstand
    // innerhalb eines Eintrags 20 px, von Eintrag zu Eintrag 30 px – also 5 px Polster über und unter dem Text
    private const int LineHeight = 20;  // Abstand der Textzeilen eines umbrochenen Eintrags (logische px)
    private const int RowPadding = 5;   // Abstand über und unter dem Text eines Eintrags
    private const int LeftMargin = 6;   // Rand links vor dem Pfeil der obersten Ebene
    private const int GlyphWidth = 20;  // Platz für den Pfeil
    private const int Indent = 16;      // Einrückung je Ebene
    private const int RightMargin = 10; // Rand rechts

    private const TextFormatFlags TextFlags = TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

    /// <summary>Ein Eintrag mit Aufklappzustand; die Zeilen (<see cref="rows"/>) sind die gerade sichtbaren in Anzeigereihenfolge.</summary>
    private sealed class Node(OutlineItem item, Node? parent, int depth)
    {
        public OutlineItem Item { get; } = item;
        public Node? Parent { get; } = parent;
        public int Depth { get; } = depth;
        public List<Node> Children { get; } = [];
        public bool Expanded { get; set; }
    }

    /// <summary>Eine sichtbare Zeile: Eintrag, Lage, Textrahmen und die umbrochenen Textzeilen (selbst umbrochen – TextRenderer kennt
    /// keinen eigenen Zeilenabstand).</summary>
    private sealed record Row(Node Node, int Top, int Height, Rectangle Text, List<string> Lines);

    private readonly List<Node> roots = [];
    private readonly List<Row> rows = [];
    private Node? selected;
    private int hover = -1;
    private int layoutWidth = -1;

    /// <summary>Klick auf einen Titel, Enter oder Wahl mit den Pfeiltasten.</summary>
    public event EventHandler<OutlineItem>? ItemActivated;

    public OutlineView()
    {
        DoubleBuffered = true;
        AutoScroll = true;
        SetStyle(ControlStyles.Selectable | ControlStyles.ResizeRedraw, true);
    }

    /// <summary>Hintergrund des gewählten Eintrags.</summary>
    [Category("Darstellung"), DefaultValue(typeof(Color), "204, 228, 247"), Description("Hintergrund des gewählten Eintrags.")]
    public Color SelectionBackColor { get; set { field = value; Invalidate(); } } = Color.FromArgb(204, 228, 247);

    /// <summary>Hintergrund des Eintrags unter der Maus.</summary>
    [Category("Darstellung"), DefaultValue(typeof(Color), "229, 243, 255"), Description("Hintergrund des Eintrags unter der Maus.")]
    public Color HoverBackColor { get; set { field = value; Invalidate(); } } = Color.FromArgb(229, 243, 255);

    /// <summary>Text, der ohne Einträge erscheint (leer: nichts).</summary>
    [Category("Darstellung"), DefaultValue(""), Description("Text, der ohne Einträge erscheint.")]
    public string EmptyText { get; set { field = value; Invalidate(); } } = string.Empty;

    /// <summary>Die Gliederung neu setzen; alles zugeklappt, ein einziger Oberpunkt (oft der Titel) gleich aufgeklappt.</summary>
    public void SetItems(IReadOnlyList<OutlineItem> items)
    {
        roots.Clear();
        foreach (var item in items) { roots.Add(Build(item, null, 0)); }
        if (roots.Count == 1) { roots[0].Expanded = true; }
        selected = null;
        hover = -1;
        AutoScrollPosition = Point.Empty;
        Relayout();

        static Node Build(OutlineItem item, Node? parent, int depth)
        {
            Node node = new(item, parent, depth);
            foreach (var child in item.Children) { node.Children.Add(Build(child, node, depth + 1)); }
            return node;
        }
    }

    /// <summary>Anzahl der Ebenen (1 = nur Oberpunkte, 0 = keine Einträge).</summary>
    [Browsable(false)]
    public int Depth => roots.Count == 0 ? 0 : roots.Max(DepthOf);

    private static int DepthOf(Node node) => 1 + (node.Children.Count == 0 ? 0 : node.Children.Max(DepthOf));

    /// <summary>Entspricht der Aufklappzustand schon <see cref="ExpandToLevel"/> mit dieser Ebene? Dann wäre der Befehl wirkungslos.
    /// Es zählt nur, was zu sehen ist: Einträge ohne Unterpunkte und das Innere zugeklappter Zweige bleiben außen vor.</summary>
    public bool IsAtLevel(int level)
    {
        static bool Check(IEnumerable<Node> nodes, int level) =>
            nodes.All(n => n.Children.Count == 0 || (n.Expanded == n.Depth < level - 1 && (!n.Expanded || Check(n.Children, level))));
        return Check(roots, level);
    }

    /// <summary>Kontextmenü: alles zuklappen und bis zur Ebene <paramref name="level"/> wieder öffnen (1 = nur die Oberpunkte sichtbar,
    /// <see cref="int.MaxValue"/> = alles auf). Eine Auswahl im zugeklappten Zweig wandert zum sichtbaren Vorfahren.</summary>
    public void ExpandToLevel(int level)
    {
        void Set(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes) { node.Expanded = node.Depth < level - 1; Set(node.Children); }
        }
        Set(roots);
        if (selected != null)
        {
            for (var current = selected.Parent; current != null; current = current.Parent)
            {
                if (!current.Expanded) { selected = current; }
            }
        }
        Relayout();
        if (selected != null) { EnsureVisible(selected); }
    }

    // ================================================================== Anordnung

    private int Scale(int logical) => LogicalToDeviceUnits(logical);

    /// <summary>Zeilen der sichtbaren Einträge neu berechnen: Text auf die verfügbare Breite umbrochen. Erst ohne Platz für die
    /// Bildlaufleiste; wird der Inhalt höher als das Steuerelement, ein zweites Mal mit – so entscheidet allein die Höhe, und das
    /// Erscheinen der Leiste kann den Umbruch nicht hin- und herschalten.</summary>
    private void Relayout()
    {
        layoutWidth = Width;
        var height = ArrangeRows(Width - Scale(RightMargin));
        if (height > Height) { height = ArrangeRows(Width - SystemInformation.VerticalScrollBarWidth - Scale(RightMargin)); }
        AutoScrollMinSize = new Size(0, height);
        Invalidate();
    }

    /// <summary>Zeilen bis zum rechten Textrand <paramref name="right"/> anordnen; liefert die Gesamthöhe.</summary>
    private int ArrangeRows(int right)
    {
        rows.Clear();
        var top = Scale(RowPadding);
        void Add(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                var left = Scale(LeftMargin) + node.Depth * Scale(Indent) + Scale(GlyphWidth);
                var width = Math.Max(Scale(40), right - left);
                var lines = Wrap(Title(node), width);
                var textHeight = lines.Count * Scale(LineHeight);
                var height = textHeight + 2 * Scale(RowPadding);
                rows.Add(new Row(node, top, height, new Rectangle(left, top + Scale(RowPadding), width, textHeight), lines));
                top += height;
                if (node.Expanded) { Add(node.Children); }
            }
        }
        Add(roots);
        return top + Scale(RowPadding);
    }

    private static string Title(Node node) => node.Item.Title.Length > 0 ? node.Item.Title : "–";

    private int TextWidth(string text) => TextRenderer.MeasureText(text, Font, new Size(int.MaxValue, int.MaxValue), TextFlags).Width;

    /// <summary>Wortweise auf <paramref name="width"/> umbrechen; ein Wort, das allein nicht passt, wird zeichenweise geteilt.</summary>
    private List<string> Wrap(string text, int width)
    {
        List<string> lines = [];
        var line = string.Empty;
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = line.Length == 0 ? word : line + " " + word;
            if (TextWidth(candidate) <= width) { line = candidate; continue; }
            if (line.Length > 0) { lines.Add(line); }
            line = word;
            while (line.Length > 1 && TextWidth(line) > width) // überlanges Wort: so viele Zeichen wie passen
            {
                var fit = 1;
                while (fit < line.Length && TextWidth(line[..(fit + 1)]) <= width) { fit++; }
                lines.Add(line[..fit]);
                line = line[fit..];
            }
        }
        if (line.Length > 0 || lines.Count == 0) { lines.Add(line); }
        return lines;
    }

    private Rectangle RowBounds(Row row) => new(0, row.Top + AutoScrollPosition.Y, ClientSize.Width, row.Height);

    private Rectangle GlyphBounds(Row row) => new(Scale(LeftMargin) + row.Node.Depth * Scale(Indent), row.Top + AutoScrollPosition.Y, Scale(GlyphWidth), Scale(LineHeight) + 2 * Scale(RowPadding)); // auf Höhe der ersten Textzeile

    private int RowAt(Point point)
    {
        for (var i = 0; i < rows.Count; i++)
        {
            if (RowBounds(rows[i]).Contains(point)) { return i; }
        }
        return -1;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (Width != layoutWidth) { Relayout(); }
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        Relayout();
    }

    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);
        Relayout();
    }

    // ================================================================== Zeichnen

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        if (rows.Count == 0)
        {
            if (EmptyText.Length > 0)
            {
                TextRenderer.DrawText(g, EmptyText, Font, new Point(Scale(LeftMargin + GlyphWidth), Scale(RowPadding) + (Scale(LineHeight) - Font.Height) / 2), SystemColors.GrayText, TextFlags);
            }
            return;
        }
        var arrow = ForeColor; // Dreiecke in der Textfarbe wie bei Chrome und Edge (Wunsch vom 04.10.2026 – abgemischt waren sie im Dunkelmodus schlecht zu sehen)
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var bounds = RowBounds(row);
            if (bounds.Bottom < e.ClipRectangle.Top) { continue; }
            if (bounds.Top > e.ClipRectangle.Bottom) { break; }
            var back = row.Node == selected ? SelectionBackColor : i == hover ? HoverBackColor : Color.Empty;
            if (back != Color.Empty) { using SolidBrush brush = new(back); g.FillRectangle(brush, bounds); }
            if (row.Node.Children.Count > 0) { ToolbarIcons.DrawExpander(g, GlyphBounds(row), row.Node.Expanded, arrow, DeviceDpi / 96f); }
            // jede Textzeile mittig in ihrer festen Zeilenhöhe
            var color = row.Node.Item.Target == null ? SystemColors.GrayText : ForeColor;
            var y = row.Text.Y + AutoScrollPosition.Y + (Scale(LineHeight) - Font.Height) / 2;
            foreach (var line in row.Lines)
            {
                TextRenderer.DrawText(g, line, Font, new Point(row.Text.X, y), color, TextFlags);
                y += Scale(LineHeight);
            }
        }
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
        if (e.Button != MouseButtons.Left || RowAt(e.Location) is not (>= 0 and var index)) { return; }
        var row = rows[index];
        if (row.Node.Children.Count > 0 && (GlyphBounds(row).Contains(e.Location) || e.Clicks == 2))
        {
            Toggle(row.Node); // Pfeil (oder Doppelklick auf die Zeile) klappt um
            return;
        }
        Select(row.Node, activate: true);
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
        keyData is Keys.Up or Keys.Down or Keys.Left or Keys.Right or Keys.Home or Keys.End or Keys.PageUp or Keys.PageDown or Keys.Enter or Keys.Space || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (rows.Count == 0) { return; }
        var index = selected == null ? -1 : rows.FindIndex(r => r.Node == selected);
        var page = Math.Max(1, ClientSize.Height / Math.Max(1, rows[0].Height));
        switch (e.KeyData)
        {
            case Keys.Up: SelectRow(index < 0 ? 0 : index - 1); break;
            case Keys.Down: SelectRow(index + 1); break;
            case Keys.PageUp: SelectRow(Math.Max(0, index - page)); break;
            case Keys.PageDown: SelectRow(Math.Min(rows.Count - 1, Math.Max(0, index) + page)); break;
            case Keys.Home: SelectRow(0); break;
            case Keys.End: SelectRow(rows.Count - 1); break;
            case Keys.Right when selected != null && selected.Children.Count > 0:
                if (!selected.Expanded) { Toggle(selected); } else { Select(selected.Children[0], activate: true); }
                break;
            case Keys.Left when selected != null:
                if (selected.Expanded) { Toggle(selected); } else if (selected.Parent != null) { Select(selected.Parent, activate: true); }
                break;
            case Keys.Space when selected?.Children.Count > 0: Toggle(selected); break;
            case Keys.Enter when selected != null: ItemActivated?.Invoke(this, selected.Item); break;
            default: return;
        }
        e.Handled = e.SuppressKeyPress = true;
    }

    private void SelectRow(int index)
    {
        if (rows.Count == 0) { return; }
        Select(rows[Math.Clamp(index, 0, rows.Count - 1)].Node, activate: true);
    }

    // ================================================================== Auswahl und Aufklappen

    private void Select(Node node, bool activate)
    {
        selected = node;
        EnsureVisible(node);
        Invalidate();
        if (activate) { ItemActivated?.Invoke(this, node.Item); }
    }

    private void Toggle(Node node)
    {
        node.Expanded = !node.Expanded;
        if (!node.Expanded && selected != null && IsDescendant(selected, node)) { selected = node; } // die Auswahl verschwände sonst im zugeklappten Zweig
        Relayout();
        EnsureVisible(node);
    }

    private static bool IsDescendant(Node node, Node ancestor)
    {
        for (var current = node.Parent; current != null; current = current.Parent)
        {
            if (current == ancestor) { return true; }
        }
        return false;
    }

    private void EnsureVisible(Node node)
    {
        if (rows.FirstOrDefault(r => r.Node == node) is not { } row) { return; }
        var top = row.Top + AutoScrollPosition.Y;
        if (top < 0) { AutoScrollPosition = new Point(0, row.Top); }
        else if (top + row.Height > ClientSize.Height) { AutoScrollPosition = new Point(0, row.Top + row.Height - ClientSize.Height); }
        Invalidate();
    }
}
