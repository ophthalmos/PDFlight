namespace PDFLight.Classes;

/// <summary>Gibt dem Text in TextBoxen links und rechts etwas Luft (EM_SETMARGINS) —
/// einmal je Form nach InitializeComponent aufrufen.</summary>
internal static class TextBoxMargins
{
    private const uint EM_SETMARGINS = 0xD3;
    private const int EC_LEFTMARGIN = 1, EC_RIGHTMARGIN = 2;

    /// <summary>Setzt den Innenabstand (2 logische Pixel) für alle TextBoxen unterhalb von root.</summary>
    public static void Apply(Control root)
    {
        if (root is TextBox box)
        {
            var margin = box.LogicalToDeviceUnits(2);
            NativeMethods.SendMessage(box.Handle, EM_SETMARGINS, EC_LEFTMARGIN | EC_RIGHTMARGIN, margin | (margin << 16));
            if (box.PlaceholderText.Length > 0 && box.PlaceholderText[0] != ' ') { box.PlaceholderText = Placeholder(box.PlaceholderText); } // erst nach Lng aufrufen
            return;
        }
        foreach (Control child in root.Controls) { Apply(child); }
    }

    /// <summary>Innenabstand einzelner Seiten (logische Pixel, null = unverändert) für ein Edit-Fenster – auch das Textfeld einer
    /// ComboBox. Ein neuer Font oder ein neues Fensterhandle setzt ihn zurück: danach erneut aufrufen.</summary>
    public static void Set(nint edit, Control dpiSource, int? left, int? right)
    {
        var flags = (left != null ? EC_LEFTMARGIN : 0) | (right != null ? EC_RIGHTMARGIN : 0);
        if (edit == 0 || flags == 0) { return; }
        var (l, r) = (dpiSource.LogicalToDeviceUnits(left ?? 0), dpiSource.LogicalToDeviceUnits(right ?? 0));
        NativeMethods.SendMessage(edit, EM_SETMARGINS, flags, l | (r << 16));
    }

    /// <summary>Wie <see cref="Set"/> links und rechts, aber dauerhaft: auch nach einem neuen Fensterhandle (etwa weil der Rahmen im
    /// Dunkelmodus wechselt) und einer neuen Schrift – für die Suchfelder (Startseite, Anmerkungsliste, Viewer-Leiste; Wunsch vom 23.10.2026).</summary>
    public static void Keep(TextBox box, int margin)
    {
        keptMargins.AddOrUpdate(box, margin);
        box.HandleCreated -= KeptBox_Changed;
        box.HandleCreated += KeptBox_Changed;
        box.FontChanged -= KeptBox_Changed;
        box.FontChanged += KeptBox_Changed;
        if (box.IsHandleCreated) { Set(box.Handle, box, margin, margin); }
    }

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<TextBox, object> keptMargins = [];

    private static void KeptBox_Changed(object? sender, EventArgs e)
    {
        if (sender is TextBox box && box.IsHandleCreated && keptMargins.TryGetValue(box, out var margin)) { Set(box.Handle, box, (int)margin, (int)margin); }
    }

    /// <summary>Platzhaltertext mit demselben Abstand: WinForms zeichnet ihn selbst und ohne die Ränder aus EM_SETMARGINS – ein
    /// vorangestelltes schmales Leerzeichen (U+2009) rückt ihn etwa so weit ein wie den getippten Text.</summary>
    public static string Placeholder(string text) => " " + text;
}
