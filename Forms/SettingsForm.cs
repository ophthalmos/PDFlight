using System.Drawing;
using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Zentraler Einstellungsdialog: Zielordnerliste, externe Programme und allgemeine Optionen.
/// Der Dialog arbeitet auf Kopien; erst OK übernimmt die Werte (über die öffentlichen Eigenschaften).</summary>
public partial class SettingsForm : Form
{
    public const int TabTargets = 0;
    public const int TabPrograms = 1;
    public const int TabGeneral = 2;

    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public List<string> TargetFolders => [.. listTargets.Items.Cast<string>()];

    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public List<string> ExternalPrograms => [.. listPrograms.Items.Cast<string>()];

    [System.ComponentModel.Browsable(false)]
    public bool AlphabeticSort => cbAlphabetic.Checked;

    [System.ComponentModel.Browsable(false)]
    public bool JumpToLastUsed => cbJumpLastUsed.Checked;

    [System.ComponentModel.Browsable(false)]
    public bool ConfirmDelete => cbConfirmDelete.Checked;

    [System.ComponentModel.Browsable(false)]
    public bool ShowProgramIcons => cbShowProgramIcons.Checked;

    [System.ComponentModel.Browsable(false)]
    public bool ShowToolbarIcons => cbToolbarIcons.Checked;

    [System.ComponentModel.Browsable(false)]
    public bool LargeToolbarIcons => cbLargeIcons.Checked;

    [System.ComponentModel.Browsable(false)]
    public bool CloseOnEscape => cbCloseOnEscape.Checked;

    [System.ComponentModel.Browsable(false)]
    public bool ReopenLastFile => cbReopenLast.Checked;

    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ClearRecentRequested { get; private set; }

    public SettingsForm(AppSettings source, int initialTab)
    {
        InitializeComponent();
        listTargets.Items.AddRange([.. source.TargetFolders.Where(f => !string.IsNullOrEmpty(f))]);
        listPrograms.Items.AddRange([.. source.ExternalPrograms.Where(f => !string.IsNullOrEmpty(f))]);
        cbAlphabetic.Checked = source.AlphabeticSort;
        cbJumpLastUsed.Checked = source.JumpToLastUsed;
        cbConfirmDelete.Checked = source.ConfirmDelete;
        cbShowProgramIcons.Checked = source.ShowProgramIcons;
        cbToolbarIcons.Checked = source.ShowToolbarIcons;
        cbLargeIcons.Checked = source.LargeToolbarIcons;
        cbCloseOnEscape.Checked = source.CloseOnEscape;
        cbReopenLast.Checked = source.ReopenLastFile;
        if (listTargets.Items.Count > 0) { listTargets.SelectedIndex = 0; }
        if (listPrograms.Items.Count > 0) { listPrograms.SelectedIndex = 0; }
        tabControl.SelectedIndex = Math.Clamp(initialTab, 0, tabControl.TabCount - 1);
        UpdateTargetButtons();
        UpdateProgramButtons();
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

    private void ListTargets_SelectedIndexChanged(object sender, EventArgs e) { UpdateTargetButtons(); }

    private void UpdateTargetButtons()
    {
        var index = listTargets.SelectedIndex;
        btnTargetRemove.Enabled = index >= 0;
        btnTargetUp.Enabled = index > 0;
        btnTargetDown.Enabled = index >= 0 && index < listTargets.Items.Count - 1;
        btnTargetRemoveMissing.Enabled = listTargets.Items.Cast<string>().Any(f => !Directory.Exists(f));
        labelTargetStatus.Text = index >= 0 && !Directory.Exists((string)listTargets.Items[index]) ? "Der markierte Ordner existiert nicht mehr." : string.Empty;
    }

    /// <summary>Nicht mehr existierende Ordner werden rot dargestellt.</summary>
    private void ListTargets_DrawItem(object sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) { return; }
        e.DrawBackground();
        var path = (string)listTargets.Items[e.Index];
        var color = Directory.Exists(path) ? e.ForeColor : Color.Firebrick;
        using SolidBrush brush = new(color);
        e.Graphics.DrawString(path, e.Font, brush, e.Bounds.Left + 2, e.Bounds.Top + 1);
        e.DrawFocusRectangle();
    }

    private void BtnTargetAdd_Click(object sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new() { Description = "Ordner zur Zielliste hinzufügen", UseDescriptionForTitle = true, ShowNewFolderButton = true };
        if (listTargets.SelectedIndex >= 0) { dialog.InitialDirectory = (string)listTargets.Items[listTargets.SelectedIndex]; }
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        var existing = listTargets.Items.Cast<string>().ToList().FindIndex(f => string.Equals(f, dialog.SelectedPath, StringComparison.OrdinalIgnoreCase));
        listTargets.SelectedIndex = existing >= 0 ? existing : listTargets.Items.Add(dialog.SelectedPath);
        UpdateTargetButtons();
    }

    private void BtnTargetRemove_Click(object sender, EventArgs e) { RemoveSelected(listTargets); UpdateTargetButtons(); }

    private void BtnTargetUp_Click(object sender, EventArgs e) { MoveSelected(listTargets, -1); }

    private void BtnTargetDown_Click(object sender, EventArgs e) { MoveSelected(listTargets, 1); }

    private void BtnTargetRemoveMissing_Click(object sender, EventArgs e)
    {
        for (var i = listTargets.Items.Count - 1; i >= 0; i--)
        {
            if (!Directory.Exists((string)listTargets.Items[i])) { listTargets.Items.RemoveAt(i); }
        }
        UpdateTargetButtons();
    }

    // ------------------------------------------------------------------ Programme

    private void ListPrograms_SelectedIndexChanged(object sender, EventArgs e) { UpdateProgramButtons(); }

    private void UpdateProgramButtons()
    {
        var index = listPrograms.SelectedIndex;
        btnProgramRemove.Enabled = index >= 0;
        btnProgramUp.Enabled = index > 0;
        btnProgramDown.Enabled = index >= 0 && index < listPrograms.Items.Count - 1;
        labelProgramStatus.Text = index >= 0 ? ProgramFinder.GetDisplayName((string)listPrograms.Items[index]) : string.Empty;
    }

    private void BtnProgramAdd_Click(object sender, EventArgs e)
    {
        using OpenFileDialog dialog = new() { Filter = "Programme (*.exe)|*.exe", Title = "Programm hinzufügen" };
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        var existing = listPrograms.Items.Cast<string>().ToList().FindIndex(f => string.Equals(f, dialog.FileName, StringComparison.OrdinalIgnoreCase));
        listPrograms.SelectedIndex = existing >= 0 ? existing : listPrograms.Items.Add(dialog.FileName);
        UpdateProgramButtons();
    }

    private void BtnProgramRemove_Click(object sender, EventArgs e) { RemoveSelected(listPrograms); UpdateProgramButtons(); }

    private void BtnProgramUp_Click(object sender, EventArgs e) { MoveSelected(listPrograms, -1); }

    private void BtnProgramDown_Click(object sender, EventArgs e) { MoveSelected(listPrograms, 1); }

    private void BtnProgramDetect_Click(object sender, EventArgs e)
    {
        listPrograms.Items.Clear();
        listPrograms.Items.AddRange([.. ProgramFinder.DetectPrograms()]);
        if (listPrograms.Items.Count > 0) { listPrograms.SelectedIndex = 0; }
        UpdateProgramButtons();
    }

    // ------------------------------------------------------------------ Allgemein

    private void BtnClearRecent_Click(object sender, EventArgs e)
    {
        if (TaskDlg.ConfirmTaskDlg(Handle, "Die Liste der zuletzt verwendeten Ordner leeren?", null))
        {
            ClearRecentRequested = true;
            btnClearRecent.Enabled = false;
            btnClearRecent.Text = "Zuletzt-Liste wird geleert";
        }
    }
}
