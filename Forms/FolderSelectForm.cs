using System.Drawing;
using PDFLight.Classes;
using PDFLight.Controls;

namespace PDFLight.Forms;

/// <summary>Zielordner-Dialog: Ordnerbaum mit Zuletzt-/Zielliste, Verlauf, Pfadfeld und Neuer-Ordner-Funktion (portiert aus PDFMover).</summary>
public partial class FolderSelectForm : Form
{
    public ComboBox TargetComboBox => comboBoxTarget;
    public ComboBox RecentComboBox => comboBoxRecent;
    public CheckBox Add2Folderlist => cbAdd2Folderlist;

    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string ShellTreePath
    {
        get => shellTreeView.SelectedPath;
        set => shellTreeView.SelectedPath = value;
    }

    private const string MsgDirectoryNotExist = "Der angegebene Pfad existiert nicht.";
    private const string NewFolderName = "Neuer Ordner";
    private readonly bool copyMode;
    private readonly bool jumpToLastUsed;
    private readonly RoundButton btnNewFolder = new();
    private bool selectAllDone;

    public FolderSelectForm(string startFolder, bool copyMode, bool jumpToLastUsed)
    {
        InitializeComponent();
        this.copyMode = copyMode;
        this.jumpToLastUsed = jumpToLastUsed;
        shellTreeView.ItemHeight = 20;

        if (!jumpToLastUsed) { shellTreeView.SelectedPath = string.IsNullOrEmpty(startFolder) ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop) : startFolder; }

        btnNewFolder.Text = " ➕";
        btnNewFolder.BackColor = SystemColors.ButtonFace;
        btnNewFolder.Font = new Font(btnNewFolder.Font.Name, 12F);
        pathEdit.TextBox.PreviewKeyDown += (sender, e) => e.IsInputKey = e.KeyCode is Keys.Return or Keys.Enter; // verhindert, dass Enter das Formular schließt
        pathEdit.TextBox.KeyDown += PathEditTextBox_KeyDown;
        shellHistory.DropDownAnchor = pathEdit.HistoryButton; // Verlaufsmenü klappt unter dem ▼-Button des Pfadfelds auf
        pathEdit.HistoryButtonClick += (sender, e) => shellHistory.ShowDropDown();
        btnNewFolder.Click += ButtonNewFolder_Clicked;
        toolTip.SetToolTip(btnNewFolder, "Neuer Ordner (Strg+N)");
    }

    private void FolderSelectForm_Load(object sender, EventArgs e)
    {
        if (copyMode) { Text = "KOPIEREN: Wählen Sie einen Ordner ..."; buttonOK.Text = "Kopieren"; }
        else { Text = "VERSCHIEBEN: Wählen Sie einen Ordner ..."; buttonOK.Text = "Verschieben"; }

        cbAdd2Folderlist.Checked = comboBoxTarget.SelectedIndex != -1;
        cbAdd2Folderlist.Enabled = comboBoxTarget.SelectedIndex == -1;
    }

    private void FolderSelectForm_Shown(object sender, EventArgs e)
    {
        Cursor.Current = Cursors.Default;
        btnNewFolder.Size = new Size(48, 48);
        btnNewFolder.Location = new Point(shellTreeView.Width - btnNewFolder.Width - SystemInformation.VerticalScrollBarWidth * 2, SystemInformation.VerticalScrollBarWidth);
        shellTreeView.Controls.Add(btnNewFolder);
        pathEdit.TextBox.Focus();
        if (jumpToLastUsed) { LinkLabelRecent_LinkClicked(null, null); }
    }

    private void ShellTreeView_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) { e.IsInputKey = e.KeyCode is Keys.Return or Keys.Enter; }

    private void ShellTreeView_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.Control && !string.IsNullOrEmpty(shellTreeView.SelectedPath)) { DialogResult = DialogResult.OK; }
    }

    private void ShellTreeView_DoubleClick(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(shellTreeView.SelectedPath))
        {
            Application.DoEvents(); // führt sonst zu Thread-Problemen
            DialogResult = DialogResult.OK;
        }
    }

    private void ShellTreeView_AfterSelect(object sender, TreeViewEventArgs e)
    {
        if (!string.IsNullOrEmpty(shellTreeView.SelectedPath))
        {
            pathEdit.Text = shellTreeView.SelectedPath;
            comboBoxTarget.SelectedIndex = comboBoxTarget.FindStringExact(shellTreeView.SelectedPath);
            comboBoxRecent.SelectedIndex = comboBoxRecent.FindStringExact(shellTreeView.SelectedPath);
            if (shellTreeView.SelectedNode.Nodes.Count > 0) { shellTreeView.SelectedNode.Expand(); }
            cbAdd2Folderlist.Checked = comboBoxTarget.SelectedIndex != -1;
            cbAdd2Folderlist.Enabled = comboBoxTarget.SelectedIndex == -1;
        }
        else
        {
            pathEdit.Text = string.Empty;
            comboBoxTarget.SelectedIndex = comboBoxRecent.SelectedIndex = -1;
            cbAdd2Folderlist.Checked = cbAdd2Folderlist.Enabled = false;
        }
        btnNewFolder.Visible = shellTreeView.SelectedNode != null && FileUtil.HasFolderWritePermission(shellTreeView.SelectedPath);
    }

    private void ShellTreeView_Resize(object sender, EventArgs e)
    {
        btnNewFolder.Location = new Point(shellTreeView.Width - btnNewFolder.Width - SystemInformation.VerticalScrollBarWidth * 2, SystemInformation.VerticalScrollBarWidth);
    }

    private void ShowHiddenFolders()
    {
        var dir = shellTreeView.SelectedPath;
        shellTreeView.ShowHidden = !shellTreeView.ShowHidden;
        if (!string.IsNullOrEmpty(dir))
        {
            if ((new DirectoryInfo(dir).Attributes & FileAttributes.Hidden) == 0) { shellTreeView.SelectedPath = dir; }
        }
    }

    private void ButtonNewFolder_Clicked(object sender, EventArgs e)
    {
        try { shellTreeView.CreateDir(NewFolderName, true); }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException or IOException)
        {
            TaskDlg.ErrTaskDlg(Handle, "Der Ordner konnte nicht erstellt werden.", ex);
        }
    }

    private void ComboBoxTarget_SelectedIndexChanged(object sender, EventArgs e) { SelectFolderPath(comboBoxTarget, (string)comboBoxTarget.SelectedItem); }

    private void ComboBoxRecent_SelectedIndexChanged(object sender, EventArgs e) { SelectFolderPath(comboBoxRecent, (string)comboBoxRecent.SelectedItem); }

    private void SelectFolderPath(ComboBox comboBox, string path)
    {
        if (string.IsNullOrEmpty(path) || path == shellTreeView.SelectedPath) { return; }
        if (Directory.Exists(path)) { shellTreeView.SelectedPath = path; }
        else
        {
            TaskDlg.MsgTaskDlg(Handle, MsgDirectoryNotExist, path, TaskDialogIcon.Warning);
            comboBox.Items.Remove(path); // nur für diesen Dialog; die gespeicherten Listen bereinigt das Hauptfenster
        }
    }

    private void ShellTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
    {
        if (shellTreeView.SelectedNode != null)
        {
            shellTreeView.SelectedNode.EnsureVisible();
            shellTreeView.TopNode = shellTreeView.SelectedNode;
        }
    }

    private void PathEdit_ButtonClick(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(pathEdit.Text))
        {
            if (pathEdit.IsValidPath) { shellTreeView.SelectedPath = pathEdit.Text; }
            else if (Path.HasExtension(pathEdit.Text) && Directory.Exists(Path.GetDirectoryName(pathEdit.Text))) { shellTreeView.SelectedPath = Path.GetDirectoryName(pathEdit.Text); }
            else
            {
                // schrittweise auf existierende übergeordnete Ordner zurückfallen
                var baseDirectory = pathEdit.Text.Contains(Path.DirectorySeparatorChar.ToString()) ? pathEdit.Text[..pathEdit.Text.LastIndexOf(Path.DirectorySeparatorChar)] : string.Empty;
                var partDirectory = !string.IsNullOrEmpty(baseDirectory) && baseDirectory.Contains(Path.DirectorySeparatorChar.ToString()) ? baseDirectory[..baseDirectory.LastIndexOf(Path.DirectorySeparatorChar)] : string.Empty;
                var rootDirectory = !string.IsNullOrEmpty(baseDirectory) ? baseDirectory.Split(Path.DirectorySeparatorChar)[0] : string.Empty;
                if (!string.IsNullOrEmpty(baseDirectory) && Directory.Exists(baseDirectory)) { shellTreeView.SelectedPath = baseDirectory; }
                else if (!string.IsNullOrEmpty(partDirectory) && Directory.Exists(partDirectory)) { shellTreeView.SelectedPath = partDirectory; }
                else if (!string.IsNullOrEmpty(rootDirectory) && Directory.Exists(rootDirectory)) { shellTreeView.SelectedPath = rootDirectory; }
                else { pathEdit.TextBox.Clear(); }
            }
            pathEdit.TextBox.SelectAll();
            pathEdit.TextBox.Focus();
        }
    }

    private void PathEditTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(pathEdit.Text))
        {
            pathEdit.Button.PerformClick();
            if (e.Modifiers == Keys.Control)
            {
                Application.DoEvents(); // führt sonst zu Thread-Problemen
                DialogResult = DialogResult.OK;
            }
        }
    }

    private void PathEdit_EditFieldEnter(object sender, EventArgs e)
    {
        if (MouseButtons == MouseButtons.None)
        {
            selectAllDone = true;
            pathEdit.TextBox.SelectAll();
        }
    }

    private void PathEdit_EditFieldLeave(object sender, EventArgs e) { selectAllDone = false; }

    private void PathEdit_EditFieldClick(object sender, EventArgs e)
    {
        if (!selectAllDone && pathEdit.TextBox.SelectionLength == 0)
        {
            selectAllDone = true;
            pathEdit.TextBox.SelectAll();
        }
    }

    private void FolderSelectForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (DialogResult != DialogResult.OK) { return; } // nur beim Übernehmen den Pfad aus dem Textfeld verarbeiten
        if (shellTreeView.SelectedPath != pathEdit.Text && pathEdit.IsValidPath)
        {
            pathEdit.Button.PerformClick();
            Application.DoEvents(); // ansonsten Fehlermeldung!
        }
        else if (pathEdit.Text.Length > 0 && !pathEdit.IsValidPath)
        {
            try
            {
                var directory = Directory.CreateDirectory(pathEdit.Text.Replace("\"", ""));
                if (directory.Exists)
                {
                    shellTreeView.SelectedPath = directory.FullName;
                    Application.DoEvents(); // ansonsten Fehlermeldung!
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException or IOException or ArgumentException or NotSupportedException)
            {
                TaskDlg.ErrTaskDlg(Handle, "Der Ordner konnte nicht erstellt werden.", ex);
                pathEdit.TextBox.Clear();
                e.Cancel = true;
            }
        }
    }

    private void FolderSelectForm_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        ShowHelpMsgBox();
    }

    private void FolderSelectForm_HelpRequested(object sender, HelpEventArgs hlpevent)
    {
        hlpevent.Handled = true;
        ShowHelpMsgBox();
    }

    private void ShowHelpMsgBox()
    {
        var text = TaskDlg.AlignShortcutColumns( // bündige Spalten wie in der Kürzel-Übersicht des Über-Dialogs
        [
            ("Strg+Links", "Einen Schritt zurück im Verlauf."),
            ("Strg+Rechts", "Einen Schritt vor im Verlauf."),
            ("Strg+Unten", "Verlaufsliste anzeigen."),
            ("Strg+Oben", "In den übergeordneten Ordner wechseln."),
            ("Strg+H", "Versteckte Ordner ein-/ausblenden."),
            ("Strg+L", "Zum zuletzt verwendeten Ordner springen."),
            ("Strg+N", "Neuen Ordner anlegen."),
            ("Strg+Eingabe", "Auswahl übernehmen."),
        ]);
        TaskDlg.MsgTaskDlg(Handle, "Tastenkürzel", text);
    }

    private void PasteFromClipboard()
    {
        var clipboard = Clipboard.GetText();
        if (!string.IsNullOrEmpty(clipboard.Trim()))
        {
            pathEdit.Text = clipboard;
            pathEdit.Focus();
            if (Directory.Exists(clipboard)) { pathEdit.Button.PerformClick(); }
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Enter or Keys.Return:
                {
                    if (shellTreeView.Focused || pathEdit.TextBox.Focused) { return false; } // weitere Verarbeitung ermöglichen
                    else { DialogResult = DialogResult.OK; return true; }
                }
            case (Keys.V | Keys.Control) or (Keys.Insert | Keys.Shift):
                {
                    if (shellTreeView.SelectedNode != null && shellTreeView.SelectedNode.IsEditing) { return false; }
                    else if (pathEdit.TextBox.Focused) { return false; }
                    else { PasteFromClipboard(); return true; }
                }
            case Keys.H | Keys.Control: { ShowHiddenFolders(); return true; }
            case Keys.L | Keys.Control: { LinkLabelRecent_LinkClicked(null, null); return true; }
            case Keys.N | Keys.Control: { ButtonNewFolder_Clicked(null, null); return true; }
            case Keys.Add | Keys.Control: { ButtonNewFolder_Clicked(null, null); return true; }
            case Keys.Oemplus | Keys.Control: { ButtonNewFolder_Clicked(null, null); return true; }
            case Keys.Right | Keys.Control: { shellHistory.MoveForward(); return true; }
            case Keys.Left | Keys.Control: { shellHistory.MoveBackward(); return true; }
            case Keys.Down | Keys.Control: { shellHistory.ShowDropDown(); return true; }
            case Keys.Up | Keys.Control: { shellHistory.MoveUpward(); return true; }
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void LinkLabelRecent_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        if (comboBoxRecent.Items.Count > 0) { SelectFolderPath(comboBoxRecent, comboBoxRecent.Items[0].ToString()); }
    }
}
