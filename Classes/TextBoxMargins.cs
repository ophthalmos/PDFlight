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
}
