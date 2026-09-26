using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Zentraler Einstellungsdialog: Zielordnerliste, externe Programme und allgemeine Optionen.
/// Der Dialog arbeitet auf Kopien; erst OK übernimmt die Werte (über die öffentlichen Eigenschaften).</summary>
public partial class SettingsForm : Form
{
    public const int TabGeneral = 0;
    public const int TabTargets = 1;
    public const int TabPrograms = 2;
    public const int TabAdobe = 3;

    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public List<string> TargetFolders => [.. listTargets.Items.Cast<string>()];

    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public List<string> ExternalPrograms => [.. listPrograms.Items.Cast<string>()];

    [System.ComponentModel.Browsable(false)]
    public bool RememberLastPage => cbRememberPage.Checked;

    [System.ComponentModel.Browsable(false)]
    public bool ConfirmDelete => cbConfirmDelete.Checked;

    [System.ComponentModel.Browsable(false)]
    public bool OpenNextAfterDelete => cbOpenNextAfterDelete.Checked;

    /// <summary>Die Einträge der Symbolleisten-Auswahl (Reihenfolge wie im Dialog) und die vier Einstellungen dahinter:
    /// große Symbole, Programm-Icons, Symbole überhaupt, Beschriftung. Die Installer-Vorgabe (setup.default) kennt vier Stufen davon.</summary>
    private static readonly (string Text, bool Large, bool Program, bool Icons, bool Labels)[] ToolbarLayouts =
    [
        ("Große Symbole und Programm-Icons", true, true, true, true),
        ("Große Symbole, ohne Programm-Icons", true, false, true, true),
        ("Große Symbole und Icons, ohne Text", true, true, true, false),
        ("Kleine Symbole und Programm-Icons", false, true, true, true),
        ("Kleine Symbole, ohne Programm-Icons", false, false, true, true),
        ("Kleine Symbole und Icons, ohne Text", false, true, true, false),
        ("Nur Text", false, false, false, true),
    ];

    [System.ComponentModel.Browsable(false)]
    public bool LargeToolbarIcons => ToolbarLayouts[comboToolbar.SelectedIndex].Large;

    [System.ComponentModel.Browsable(false)]
    public bool ShowProgramIcons => ToolbarLayouts[comboToolbar.SelectedIndex].Program;

    [System.ComponentModel.Browsable(false)]
    public bool ShowToolbarIcons => ToolbarLayouts[comboToolbar.SelectedIndex].Icons;
    [System.ComponentModel.Browsable(false)]
    public bool ShowToolbarText => ToolbarLayouts[comboToolbar.SelectedIndex].Labels;

    [System.ComponentModel.Browsable(false)]
    public bool CloseOnEscape => cbCloseOnEscape.Checked;

    [System.ComponentModel.Browsable(false)]
    public bool ReopenLastFile => cbReopenLast.Checked;

    [System.ComponentModel.Browsable(false)]
    public bool ShowFullPathInTitle => cbFullPathTitle.Checked;

    /// <summary>Favoriten-Menü in der Symbolleiste anzeigen (Strg+D merkt die Datei); Standard aus.</summary>
    public bool ShowFavorites => cbShowFavorites.Checked;

    /// <summary>Anzeigehintergrund dunkel (Fläche und Leiste des PDF-Viewers).</summary>
    public bool DarkViewer => rbBackgroundDark.Checked;

    /// <summary>Zustimmung zur optionalen Adobe-Ansicht (Adobe PDF Embed API, Seite „Adobe PDF Embed API“); Standard aus.</summary>
    [System.ComponentModel.Browsable(false)]
    public bool AdobeEmbedEnabled => cbAdobeEnabled.Checked;

    /// <summary>Schaltfläche „Adobe“ in der Symbolleiste (nur mit Zustimmung wirksam; F8 geht auch ohne).</summary>
    [System.ComponentModel.Browsable(false)]
    public bool AdobeEmbedButton => cbAdobeButton.Checked;

    [System.ComponentModel.Browsable(false)]
    public int MaxRecentFiles => (int)numMaxRecentFiles.Value; // 0 = kein Verlauf im Öffnen-Menü

    private static readonly (string Name, string Code)[] Languages = [("Deutsch", "de"), ("English", "en"), ("Français", "fr"), ("Español", "es")];

    [System.ComponentModel.Browsable(false)]
    public string Language => Languages[Math.Max(0, comboLanguage.SelectedIndex)].Code;

    public SettingsForm(AppSettings source, int initialTab)
    {
        InitializeComponent();
        Lng.Apply(this); // übersetzt alle Designer-Texte, falls nicht Deutsch eingestellt ist
        comboLanguage.Items.AddRange([.. Languages.Select(l => (object)l.Name)]);
        comboLanguage.SelectedIndex = Math.Max(0, Array.FindIndex(Languages, l => l.Code == source.Language));
        listTargets.Items.AddRange([.. source.TargetFolders.Where(f => !string.IsNullOrEmpty(f))]);
        listPrograms.Items.AddRange([.. source.ExternalPrograms.Where(f => !string.IsNullOrEmpty(f))]);
        cbRememberPage.Checked = source.RememberLastPage;
        cbConfirmDelete.Checked = source.ConfirmDelete;
        cbOpenNextAfterDelete.Checked = source.OpenNextAfterDelete;
        comboToolbar.Items.AddRange([.. ToolbarLayouts.Select(l => (object)Lng.T(l.Text))]);
        var layout = Array.FindIndex(ToolbarLayouts, l => l.Large == source.LargeToolbarIcons && l.Program == source.ShowProgramIcons && l.Icons == source.ShowToolbarIcons && l.Labels == source.ShowToolbarText);
        comboToolbar.SelectedIndex = layout >= 0 ? layout : ToolbarLayouts.Length - 1; // Kombinationen ohne Listeneintrag (z.B. nur Text mit Programm-Icons) landen bei „Nur Text“
        cbCloseOnEscape.Checked = source.CloseOnEscape;
        cbReopenLast.Checked = source.ReopenLastFile;
        cbFullPathTitle.Checked = source.ShowFullPathInTitle;
        cbShowFavorites.Checked = source.ShowFavorites;
        rbBackgroundDark.Checked = source.DarkViewer;
        rbBackgroundLight.Checked = !source.DarkViewer;
        labelAdobeText.Text = Lng.T("Adobe.Consent", labelAdobeText.Text); // mehrzeilig → expliziter Schlüssel (wie die Tooltips des Hauptfensters)
        cbAdobeEnabled.Checked = source.AdobeEmbedEnabled;
        cbAdobeButton.Checked = source.AdobeEmbedButton;
        cbAdobeButton.Enabled = cbAdobeEnabled.Checked;
        labelAdobeClientId.Text = AdobeEmbed.IsAvailable
            ? Lng.T("Adobe-Client-ID gefunden (adobe-clientid.txt).")
            : Lng.T("Adobe-Client-ID fehlt: Datei adobe-clientid.txt neben PDFlight.exe anlegen – bis dahin bleibt die Ansicht gesperrt.");
        LayoutAdobeTab();
        numMaxRecentFiles.Value = Math.Clamp(source.MaxRecentFiles, (int)numMaxRecentFiles.Minimum, (int)numMaxRecentFiles.Maximum);
        if (listTargets.Items.Count > 0) { listTargets.SelectedIndex = 0; }
        if (listPrograms.Items.Count > 0) { listPrograms.SelectedIndex = 0; }
        tabControl.SelectedIndex = Math.Clamp(initialTab, 0, tabControl.TabCount - 1);
        UpdateTargetButtons();
        UpdateProgramButtons();
    }

    // ------------------------------------------------------------------ Adobe PDF Embed API

    /// <summary>Der Zustimmungstext ist je nach Sprache und Dialogbreite unterschiedlich hoch – die Höhe wird gemessen, Link,
    /// Häkchen und Client-ID-Zeile rücken nach (das kann der Designer nicht; er hält nur die Vorgabe für Deutsch bei 488 px).</summary>
    private void LayoutAdobeTab()
    {
        var height = TextRenderer.MeasureText(labelAdobeText.Text, labelAdobeText.Font, new Size(labelAdobeText.Width, 0), TextFormatFlags.WordBreak).Height;
        labelAdobeText.Height = height + LogicalToDeviceUnits(4);
        var gap = LogicalToDeviceUnits(6);
        linkAdobePrivacy.Top = labelAdobeText.Bottom + gap;
        cbAdobeEnabled.Top = linkAdobePrivacy.Bottom + gap;
        cbAdobeButton.Top = cbAdobeEnabled.Bottom + gap;
        labelAdobeClientId.Top = cbAdobeButton.Bottom + gap;
    }

    private void TabAdobe_Resize(object? sender, EventArgs e) { LayoutAdobeTab(); }

    private void CbAdobeEnabled_CheckedChanged(object? sender, EventArgs e) { cbAdobeButton.Enabled = cbAdobeEnabled.Checked; }

    private void LinkAdobePrivacy_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(AdobeEmbed.PrivacyUrl) { UseShellExecute = true }); }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            TaskDlg.ErrTaskDlg(Handle, Lng.T("Der Browser konnte nicht geöffnet werden."), ex);
        }
    }

    // ------------------------------------------------------------------ gemeinsame Listenhelfer

    private static void MoveSelected(ListBox list, int direction)
    {
        var index = list.SelectedIndex;
        var target = index + direction;
        if (index < 0 || target < 0 || target >= list.Items.Count) { return; }
        (list.Items[target], list.Items[index]) = (list.Items[index], list.Items[target]);
        list.SelectedIndex = target;
    }

    private static void RemoveSelected(ListBox list)
    {
        var index = list.SelectedIndex;
        if (index < 0) { return; }
        list.Items.RemoveAt(index);
        if (list.Items.Count > 0) { list.SelectedIndex = Math.Min(index, list.Items.Count - 1); }
    }

    // ------------------------------------------------------------------ Zielordner

    private void ListTargets_SelectedIndexChanged(object? sender, EventArgs e) { UpdateTargetButtons(); }

    private void UpdateTargetButtons()
    {
        var index = listTargets.SelectedIndex;
        btnTargetRemove.Enabled = index >= 0;
        btnTargetUp.Enabled = index > 0;
        btnTargetDown.Enabled = index >= 0 && index < listTargets.Items.Count - 1;
        btnTargetRemoveMissing.Enabled = listTargets.Items.Cast<string>().Any(f => !Directory.Exists(f));
        labelTargetStatus.Text = index >= 0 && !Directory.Exists((string)listTargets.Items[index]) ? Lng.T("Der markierte Ordner existiert nicht mehr.") : string.Empty;
    }

    /// <summary>Nicht mehr existierende Ordner werden rot dargestellt. TextRenderer statt
    /// Graphics.DrawString: gleiches (klares) GDI-Rendering wie bei einer normalen ListBox.</summary>
    private void ListTargets_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) { return; }
        e.DrawBackground();
        var path = (string)listTargets.Items[e.Index];
        var color = Directory.Exists(path) ? e.ForeColor : Color.Firebrick;
        TextRenderer.DrawText(e.Graphics, path, e.Font, new Point(e.Bounds.Left + 2, e.Bounds.Top + 1), color);
        e.DrawFocusRectangle();
    }

    private void BtnTargetAdd_Click(object? sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new() { Description = Lng.T("Ordner zur Zielliste hinzufügen"), UseDescriptionForTitle = true, ShowNewFolderButton = true };
        if (listTargets.SelectedIndex >= 0) { dialog.InitialDirectory = (string)listTargets.Items[listTargets.SelectedIndex]; }
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        var existing = listTargets.Items.Cast<string>().ToList().FindIndex(f => string.Equals(f, dialog.SelectedPath, StringComparison.OrdinalIgnoreCase));
        listTargets.SelectedIndex = existing >= 0 ? existing : listTargets.Items.Add(dialog.SelectedPath);
        UpdateTargetButtons();
    }

    private void BtnTargetRemove_Click(object? sender, EventArgs e) { RemoveSelected(listTargets); UpdateTargetButtons(); }

    private void BtnTargetUp_Click(object? sender, EventArgs e) { MoveSelected(listTargets, -1); }

    private void BtnTargetDown_Click(object? sender, EventArgs e) { MoveSelected(listTargets, 1); }

    private void BtnTargetRemoveMissing_Click(object? sender, EventArgs e)
    {
        for (var i = listTargets.Items.Count - 1; i >= 0; i--)
        {
            if (!Directory.Exists((string)listTargets.Items[i])) { listTargets.Items.RemoveAt(i); }
        }
        UpdateTargetButtons();
    }

    private void BtnTargetSort_Click(object? sender, EventArgs e)
    {
        var selected = listTargets.SelectedIndex >= 0 ? (string)listTargets.Items[listTargets.SelectedIndex] : null;
        var sorted = listTargets.Items.Cast<string>().OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray();
        listTargets.Items.Clear();
        listTargets.Items.AddRange(sorted);
        if (selected != null) { listTargets.SelectedIndex = Array.IndexOf(sorted, selected); }
        UpdateTargetButtons();
    }

    // ------------------------------------------------------------------ Programme

    private void ListPrograms_SelectedIndexChanged(object? sender, EventArgs e) { UpdateProgramButtons(); }

    private void UpdateProgramButtons()
    {
        var index = listPrograms.SelectedIndex;
        btnProgramRemove.Enabled = index >= 0;
        btnProgramUp.Enabled = index > 0;
        btnProgramDown.Enabled = index >= 0 && index < listPrograms.Items.Count - 1;
        labelProgramStatus.Text = index >= 0 ? ProgramFinder.GetDisplayName((string)listPrograms.Items[index]) : string.Empty;
    }

    private void BtnProgramAdd_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new() { Filter = Lng.T("Programme (*.exe)|*.exe"), Title = Lng.T("Programm hinzufügen") };
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        var existing = listPrograms.Items.Cast<string>().ToList().FindIndex(f => string.Equals(f, dialog.FileName, StringComparison.OrdinalIgnoreCase));
        listPrograms.SelectedIndex = existing >= 0 ? existing : listPrograms.Items.Add(dialog.FileName);
        UpdateProgramButtons();
    }

    private void BtnProgramRemove_Click(object? sender, EventArgs e) { RemoveSelected(listPrograms); UpdateProgramButtons(); }

    private void BtnProgramUp_Click(object? sender, EventArgs e) { MoveSelected(listPrograms, -1); }

    private void BtnProgramDown_Click(object? sender, EventArgs e) { MoveSelected(listPrograms, 1); }

    private void BtnProgramDetect_Click(object? sender, EventArgs e)
    {
        var detected = ProgramFinder.DetectPrograms();
        // von Hand hinzugefügte Einträge nicht ungefragt verwerfen
        var custom = listPrograms.Items.Cast<string>()
            .Where(p => !detected.Any(d => string.Equals(d, p, StringComparison.OrdinalIgnoreCase))).ToList();
        if (custom.Count > 0)
        {
            var names = string.Join(Environment.NewLine, custom.Select(p => "• " + ProgramFinder.GetDisplayName(p)));
            var remove = TaskDlg.ConfirmTaskDlg(Handle, Lng.T("Individuelle Programmeinträge gefunden"),
                Lng.T("Diese Programme kennt die automatische Erkennung nicht:") + Environment.NewLine + names
                + Environment.NewLine + Environment.NewLine + Lng.T("Sollen sie aus der Liste entfernt werden?"),
                TaskDialogIcon.Warning, defaultNo: true); // Esc/Nein behält sie
            if (!remove) { detected.AddRange(custom); }
        }
        listPrograms.Items.Clear();
        listPrograms.Items.AddRange([.. detected]);
        if (listPrograms.Items.Count > 0) { listPrograms.SelectedIndex = 0; }
        UpdateProgramButtons();
    }

    private void BtnProgramSort_Click(object? sender, EventArgs e)
    {
        var selected = listPrograms.SelectedIndex >= 0 ? (string)listPrograms.Items[listPrograms.SelectedIndex] : null;
        var sorted = listPrograms.Items.Cast<string>().OrderBy(ProgramFinder.GetDisplayName, StringComparer.OrdinalIgnoreCase).ToArray(); // nach Anzeigename, nicht nach Pfad
        listPrograms.Items.Clear();
        listPrograms.Items.AddRange(sorted);
        if (selected != null) { listPrograms.SelectedIndex = Array.IndexOf(sorted, selected); }
        UpdateProgramButtons();
    }
}
