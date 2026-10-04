namespace PDFLight.Viewer;

/// <summary>Zeichnet die Leisten der Anzeige (Viewer-Leiste, Kopf der Seitenleiste) im dunklen Anzeigehintergrund wie die
/// PDF-Leiste von Edge und Chrome: dunkelgraue Fläche, helle Schrift, Hervorhebungen in etwas hellerem Grau. Die Symbole liefert der
/// Aufrufer passend hell (<see cref="Classes.ToolbarIcons.Get(char, Size, Color)"/>).</summary>
internal sealed class ViewerStripRenderer() : ToolStripProfessionalRenderer(new DarkColors())
{
    public static readonly Color Background = Color.FromArgb(59, 59, 59);
    public static readonly Color Foreground = Color.FromArgb(232, 232, 232);
    public static readonly Color FieldBackground = Color.FromArgb(40, 40, 40);
    public static readonly Color ComboBackground = Color.FromArgb(56, 56, 56); // Fläche einer Auswahlliste im Windows-Design „DarkMode_CFD“ (gemessen)
    private static readonly Color Hover = Color.FromArgb(80, 80, 80);
    private static readonly Color Pressed = Color.FromArgb(100, 100, 100);
    private static readonly Color Separator = Color.FromArgb(95, 95, 95);

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = e.Item.Enabled ? Foreground : Color.FromArgb(140, 140, 140);
        base.OnRenderItemText(e);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { } // keine helle Kante unten

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        e.ArrowColor = e.Item?.Enabled != false ? Foreground : Color.FromArgb(140, 140, 140); // sonst schwarz auf Dunkelgrau (Dropdown „Farbe und Stärke“)
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
        public override Color MenuItemSelected => Hover;
        public override Color MenuItemBorder => Hover;
        public override Color MenuBorder => Separator;
    }
}
