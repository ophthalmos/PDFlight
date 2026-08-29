using System.Globalization;
using System.Resources;

namespace PDFLight.Classes;

/// <summary>Mehrsprachigkeit (Deutsch/Englisch, erweiterbar): Deutsch ist einkompiliert und bleibt der
/// Rückfall für jeden fehlenden Eintrag; andere Sprachen liegen als Languages\lng.&lt;kultur&gt;.resx daneben,
/// wobei der SCHLÜSSEL jedes Eintrags der deutsche Text selbst ist. Ändert sich ein deutscher Text,
/// verfällt seine Übersetzung dadurch automatisch, bis die resx nachgezogen ist — bis dahin erscheint
/// der Text auf Deutsch, es bricht also nichts.</summary>
internal static class Lng
{
    private static readonly ResourceManager resources = new("PDFLight.Languages.lng", typeof(Lng).Assembly);
    private static CultureInfo culture; // null = Deutsch (keine Übersetzung nötig)

    /// <summary>Der gewählte Kultur-Code ("de", "en", …), z.B. für die Sprache des WebView2-Viewers.</summary>
    public static string CultureCode { get; private set; } = "de";

    public static void Initialize(string cultureCode)
    {
        CultureCode = string.IsNullOrEmpty(cultureCode) ? "de" : cultureCode;
        try { culture = CultureCode == "de" ? null : CultureInfo.GetCultureInfo(CultureCode); }
        catch (CultureNotFoundException) { culture = null; CultureCode = "de"; }
    }

    /// <summary>Übersetzt einen deutschen Text; ohne Eintrag (oder auf Deutsch) kommt er unverändert zurück.</summary>
    public static string T(string german)
    {
        if (culture == null || string.IsNullOrEmpty(german)) { return german; }
        try { return resources.GetString(german, culture) ?? german; }
        catch (MissingManifestResourceException) { return german; }
    }

    /// <summary>Übersetzt alle Texte eines Formulars samt Menüs und Tooltips —
    /// einmal direkt nach InitializeComponent aufrufen.</summary>
    public static void Apply(Control root)
    {
        if (culture == null) { return; }
        root.Text = T(root.Text);
        TranslateChildren(root);
    }

    private static void TranslateChildren(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            child.Text = T(child.Text);
            if (child is ToolStrip strip) // deckt auch StatusStrip ab
            {
                foreach (ToolStripItem item in strip.Items) { TranslateItem(item); }
            }
            TranslateChildren(child);
        }
    }

    private static void TranslateItem(ToolStripItem item)
    {
        item.Text = T(item.Text);
        item.ToolTipText = T(item.ToolTipText);
        if (item is ToolStripDropDownItem dropDown)
        {
            foreach (ToolStripItem child in dropDown.DropDownItems) { TranslateItem(child); }
        }
    }
}
