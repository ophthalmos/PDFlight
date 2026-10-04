namespace PDFLight.Viewer;

/// <summary>Zeichnet die Leisten der Anzeige (Viewer-Leiste, Kopf der Seitenleiste) im dunklen Anzeigehintergrund wie die
/// PDF-Leiste von Edge und Chrome: dunkelgraue Fläche, helle Schrift, Hervorhebungen in etwas hellerem Grau. Die Symbole liefert der
/// Aufrufer passend hell (<see cref="Classes.ToolbarIcons.Get(char, Size, Color)"/>). Hell zeichnet er wie der Standard von WinForms
/// (ProfessionalColorTable) – in beiden Modi aber aktive Anmerkungsmodi wie Edge (<see cref="IsActiveMode"/>).</summary>
internal sealed class ViewerStripRenderer(bool dark = true) : ToolStripProfessionalRenderer(dark ? new DarkColors() : new ProfessionalColorTable())
{
    /// <summary>Gehört das Element zu einem aktiven Modus (Hervorheben, Zeichnen, Radieren samt Dropdown)? Dann zeichnet der Renderer es
    /// wie Edge: graue Fläche über Knopf und Pfeil, darunter eine durchgehende Linie in der Akzentfarbe (Wunsch vom 04.10.2026).</summary>
    public Func<ToolStripItem, bool>? IsActiveMode { get; init; }

    private static readonly Color ActiveLight = Color.FromArgb(229, 229, 229);  // Fläche bei Edge (gemessen)
    private static readonly Color AccentLight = Color.FromArgb(0, 114, 201);    // Linie bei Edge (gemessen 0072C9)

    protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
    {
        if (!RenderActiveMode(e)) { base.OnRenderButtonBackground(e); }
    }

    protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
    {
        if (!RenderActiveMode(e)) { base.OnRenderDropDownButtonBackground(e); }
    }

    private bool RenderActiveMode(ToolStripItemRenderEventArgs e)
    {
        if (IsActiveMode?.Invoke(e.Item) != true) { return false; }
        var bounds = new Rectangle(Point.Empty, e.Item.Size);
        // einheitlich über Knopf und Pfeil – eine Abstufung beim Überfahren erschien auch ohne Maus, wenn die Leiste ein Element als
        // ausgewählt führte (nach Tastatur- oder UIA-Bedienung), und teilte die Fläche sichtbar
        var fill = dark ? Hover : ActiveLight;
        using (var brush = new SolidBrush(fill)) { e.Graphics.FillRectangle(brush, bounds); }
        var line = Math.Max(2, e.Item.Height / 16); // 2 px bei 100 %, wie Edge
        using (var brush = new SolidBrush(dark ? SidebarRailRenderer.Accent(true) : AccentLight)) { e.Graphics.FillRectangle(brush, 0, bounds.Bottom - line, bounds.Width, line); }
        return true;
    }

    public static readonly Color Background = Color.FromArgb(59, 59, 59);
    public static readonly Color Foreground = Color.FromArgb(232, 232, 232);
    public static readonly Color FieldBackground = Color.FromArgb(40, 40, 40);
    public static readonly Color ComboBackground = Color.FromArgb(56, 56, 56); // Fläche einer Auswahlliste im Windows-Design „DarkMode_CFD“ (gemessen)
    private static readonly Color Hover = Color.FromArgb(80, 80, 80);
    private static readonly Color Pressed = Color.FromArgb(100, 100, 100);
    private static readonly Color Separator = Color.FromArgb(95, 95, 95);

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        if (dark) { e.TextColor = e.Item.Enabled ? Foreground : Color.FromArgb(140, 140, 140); }
        base.OnRenderItemText(e);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        if (!dark) { base.OnRenderToolStripBorder(e); } // dunkel: keine helle Kante unten
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        // reine Pfeil-Knöpfe (Farbwahl neben Hervorheben und Zeichnen) sind breiter als ihr Pfeil, damit man sie leichter trifft (Wunsch
        // vom 04.10.2026): den Pfeil in der Mitte zeichnen, nicht am rechten Rand
        if (e.Item is ToolStripDropDownButton { DisplayStyle: ToolStripItemDisplayStyle.None } button) { e.ArrowRectangle = new Rectangle(Point.Empty, button.Size); }
        if (dark) { e.ArrowColor = e.Item?.Enabled != false ? Foreground : Color.FromArgb(140, 140, 140); } // sonst schwarz auf Dunkelgrau (Dropdown „Farbe und Stärke“)
        base.OnRenderArrow(e);
    }

    private sealed class DarkColors : ProfessionalColorTable
    {
        public DarkColors() { UseSystemColors = false; }
        public override Color ToolStripGradientBegin => Background;
        public override Color ToolStripGradientMiddle => Background;
        public override Color ToolStripGradientEnd => Background;
        public override Color ToolStripBorder => Background;
        public override Color ButtonSelectedHighlight => Hover;
        public override Color ButtonSelectedGradientBegin => Hover;
        public override Color ButtonSelectedGradientMiddle => Hover;
        public override Color ButtonSelectedGradientEnd => Hover;
        public override Color ButtonSelectedBorder => Hover;
        public override Color ButtonPressedGradientBegin => Pressed;
        public override Color ButtonPressedGradientMiddle => Pressed;
        public override Color ButtonPressedGradientEnd => Pressed;
        public override Color ButtonPressedBorder => Pressed;
        public override Color ButtonCheckedGradientBegin => Pressed;
        public override Color ButtonCheckedGradientMiddle => Pressed;
        public override Color ButtonCheckedGradientEnd => Pressed;
        public override Color ButtonCheckedHighlight => Pressed;
        public override Color ButtonCheckedHighlightBorder => Pressed;
        public override Color CheckBackground => Pressed;
        public override Color CheckSelectedBackground => Hover;
        public override Color CheckPressedBackground => Pressed;
        public override Color SeparatorDark => Separator;
        public override Color SeparatorLight => Background;
        public override Color OverflowButtonGradientBegin => Background;
        public override Color OverflowButtonGradientMiddle => Background;
        public override Color OverflowButtonGradientEnd => Background;
        public override Color ToolStripDropDownBackground => Background;
        public override Color ImageMarginGradientBegin => Background;
        public override Color ImageMarginGradientMiddle => Background;
        public override Color ImageMarginGradientEnd => Background;
        // ein geöffneter Pfeil-Knopf (Farbwahl) nimmt die Farben eines gedrückten Menüeintrags – ohne sie zeichnete WinForms ihn im
        // Dunkelmodus hell (Fehlerbericht 04.10.2026; im aktiven Modus übermalte RenderActiveMode das, deshalb trat es nur so auf)
        public override Color MenuItemPressedGradientBegin => Pressed;
        public override Color MenuItemPressedGradientMiddle => Pressed;
        public override Color MenuItemPressedGradientEnd => Pressed;
        public override Color MenuItemSelectedGradientBegin => Hover;
        public override Color MenuItemSelectedGradientEnd => Hover;
        public override Color MenuItemSelected => Hover;
        public override Color MenuItemBorder => Hover;
        public override Color MenuBorder => Separator;
    }
}
