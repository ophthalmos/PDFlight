using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Verwaltet die Stempelpalette: Liste links, rechts die Eigenschaften des markierten Stempels (Text, Größe,
/// Schriftfarbe, Hintergrund, Position). Gearbeitet wird auf Kopien; erst OK liefert die neue Liste.</summary>
public partial class StampManageForm : Form
{
    private readonly List<Stamp> stamps;
    private bool loading;  // beim Befüllen der Felder keine Änderungen zurückschreiben
    private bool renaming; // beim Nachziehen des Listeneintrags die Auswahl nicht neu laden

    /// <summary>Die bearbeitete Palette (nach OK).</summary>
    public List<Stamp> Stamps => stamps;

    public StampManageForm(IEnumerable<Stamp> source)
    {
        InitializeComponent();
        Lng.Apply(this);
        TextBoxMargins.Apply(this);
        stamps = [.. source.Select(s => s.Clone())];
        textBoxText.MaxLength = Stamp.MaxTextLength;
        textBoxInitials.MaxLength = Stamp.MaxInitialsLength;
        numOpacity.Minimum = Stamp.MinOpacity;
        numSize.Minimum = (decimal)Stamp.MinFontSize;
        numSize.Maximum = (decimal)Stamp.MaxFontSize;
        comboColor.Items.AddRange([.. ColorPalette.StampColors.Select(c => (object)Lng.T(c.Name))]);
        comboBackground.Items.AddRange([.. ColorPalette.StampBackgrounds.Select(b => (object)Lng.T(b.Name))]);
        comboPosition.Items.AddRange([.. Stamp.PositionNames.Select(p => (object)Lng.T(p))]);
        foreach (var stamp in stamps) { listStamps.Items.Add(stamp.Text); }
        if (listStamps.Items.Count > 0) { listStamps.SelectedIndex = 0; }
        ShowSelected();
    }

    private Stamp? Selected => listStamps.SelectedIndex >= 0 && listStamps.SelectedIndex < stamps.Count ? stamps[listStamps.SelectedIndex] : null;

    /// <summary>Felder mit dem markierten Stempel füllen (oder sperren, wenn keiner markiert ist).</summary>
    private void ShowSelected()
    {
        loading = true;
        var stamp = Selected;
        var enabled = stamp != null;
        textBoxText.Enabled = numSize.Enabled = comboColor.Enabled = comboBackground.Enabled = comboPosition.Enabled = buttonDelete.Enabled = enabled;
        cbDate.Enabled = cbBorder.Enabled = cbRounded.Enabled = textBoxInitials.Enabled = numOpacity.Enabled = enabled;
        numOpacity.Value = Math.Clamp(stamp?.Opacity ?? Stamp.DefaultOpacity, (int)numOpacity.Minimum, (int)numOpacity.Maximum);
        buttonDefaults.Enabled = stamps.Count == 0; // die Vorgaben gibt es nur in eine leere Palette
        textBoxInitials.Text = stamp?.Initials ?? string.Empty;
        cbDate.Checked = stamp?.WithDate ?? true;
        cbBorder.Checked = stamp?.Border ?? true;
        cbRounded.Checked = stamp?.Rounded ?? false;
        textBoxText.Text = stamp?.Text ?? string.Empty;
        numSize.Value = Math.Clamp((decimal)(stamp?.FontSize ?? 24), numSize.Minimum, numSize.Maximum);
        comboColor.SelectedIndex = ColorPalette.IndexOf(ColorPalette.StampColors, stamp?.Color ?? Color.Black);
        comboBackground.SelectedIndex = ColorPalette.IndexOf(ColorPalette.StampBackgrounds, stamp?.BackgroundColor);
        comboPosition.SelectedIndex = (int)(stamp?.Position ?? StampPosition.TopRight);
        loading = false;
    }

    /// <summary>Jede Feldänderung landet sofort im markierten Stempel; die Liste zeichnet den Eintrag neu.</summary>
    private void Field_Changed(object? sender, EventArgs e)
    {
        if (loading || Selected is not { } stamp) { return; }
        stamp.Text = textBoxText.Text.Trim();
        stamp.FontSize = (double)numSize.Value;
        stamp.TextColor = AnnotationStyle.ToHex(ColorPalette.StampColors[Math.Max(0, comboColor.SelectedIndex)].Color);
        stamp.Background = ColorPalette.StampBackgrounds[Math.Max(0, comboBackground.SelectedIndex)].Color is { } background ? AnnotationStyle.ToHex(background) : string.Empty;
        stamp.Position = (StampPosition)Math.Max(0, comboPosition.SelectedIndex);
        stamp.WithDate = cbDate.Checked;
        stamp.Border = cbBorder.Checked;
        stamp.Rounded = cbRounded.Checked;
        stamp.Initials = textBoxInitials.Text.Trim();
        stamp.Opacity = (int)numOpacity.Value;
        (sender as ComboBox)?.Invalidate(); // selbst gezeichnete Auswahlfelder zeigen nach Tastaturwahl sonst noch den alten Eintrag
        listStamps.Invalidate(); // die Vorschau zeichnet aus der Stempelliste; den Eintragstext selbst erst beim Verlassen des Feldes ändern
    }

    /// <summary>Den Listeneintrag (Name für Bildschirmleser) nachziehen – nicht bei jedem Tastendruck, weil das Ersetzen des
    /// Eintrags die Auswahl neu setzt und damit die Felder überschreibt.</summary>
    private void TextBoxText_Leave(object? sender, EventArgs e)
    {
        if (Selected is not { } stamp || listStamps.SelectedIndex < 0) { return; }
        renaming = true;
        listStamps.Items[listStamps.SelectedIndex] = stamp.Text;
        renaming = false;
    }

    private void ListStamps_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (!renaming) { ShowSelected(); }
    }

    private void ListStamps_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= stamps.Count) { return; }
        StampListPainter.Draw(e, stamps[e.Index], Font);
    }

    private void ComboColor_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index >= 0 && e.Index < ColorPalette.StampColors.Length) { ColorPalette.DrawItem(e, ColorPalette.StampColors[e.Index].Name, ColorPalette.StampColors[e.Index].Color, Font); }
    }

    private void ComboBackground_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index >= 0 && e.Index < ColorPalette.StampBackgrounds.Length) { ColorPalette.DrawItem(e, ColorPalette.StampBackgrounds[e.Index].Name, ColorPalette.StampBackgrounds[e.Index].Color, Font); }
    }

    private void ButtonNew_Click(object? sender, EventArgs e)
    {
        Stamp stamp = new() { Text = Lng.T("Neu") };
        if (Selected is { } template) { stamp = template.Clone(); stamp.Text = Lng.T("Neu"); } // Gestaltung des markierten übernehmen
        stamps.Add(stamp);
        listStamps.Items.Add(stamp.Text);
        listStamps.SelectedIndex = listStamps.Items.Count - 1;
        textBoxText.SelectAll();
        textBoxText.Focus();
    }

    /// <summary>Die drei Vorgabestempel (in der eingestellten Sprache) in die leere Palette setzen.</summary>
    private void ButtonDefaults_Click(object? sender, EventArgs e)
    {
        if (stamps.Count > 0) { return; }
        foreach (var stamp in Stamp.Defaults())
        {
            stamps.Add(stamp);
            listStamps.Items.Add(stamp.Text);
        }
        listStamps.SelectedIndex = 0;
        ShowSelected();
    }

    private void ButtonDelete_Click(object? sender, EventArgs e)
    {
        var index = listStamps.SelectedIndex;
        if (index < 0) { return; }
        stamps.RemoveAt(index);
        listStamps.Items.RemoveAt(index);
        if (listStamps.Items.Count > 0) { listStamps.SelectedIndex = Math.Min(index, listStamps.Items.Count - 1); }
        ShowSelected();
    }

    private void ButtonOK_Click(object? sender, EventArgs e)
    {
        var empty = stamps.FindIndex(s => s.Text.Length == 0);
        if (empty >= 0)
        {
            listStamps.SelectedIndex = empty;
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Bitte gib für jeden Stempel einen Text ein."), null, TaskDialogIcon.Warning);
            textBoxText.Focus();
            return;
        }
        DialogResult = DialogResult.OK;
    }
}
