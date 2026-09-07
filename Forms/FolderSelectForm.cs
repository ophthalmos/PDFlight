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
        Lng.Apply(this);
        Lng.Apply(contextMenuTree); // Kontextmenüs hängen nicht im Control-Baum
        newFolderMenuItem.Image = ToolbarIcons.MenuIcon(ToolbarIcons.NewFolder, this); // Symbole wie in den Hauptmenüs (Einstellung „Symbole anzeigen“)
        renameFolderMenuItem.Image = ToolbarIcons.MenuIcon(ToolbarIcons.Rename, this);
        deleteFolderMenuItem.Image = ToolbarIcons.MenuIcon(ToolbarIcons.Delete, this);
        TextBoxMargins.Apply(this);
        this.copyMode = copyMode;
        this.jumpToLastUsed = jumpToLastUsed;
        shellTreeView.ItemHeight = 20;

        if (!jumpToLastUsed) { shellTreeView.SelectedPath = string.IsNullOrEmpty(startFolder) ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop) : startFolder; }

        btnNewFolder.Glyph = ToolbarIcons.Add;
        btnNewFolder.Text = Lng.T(NewFolderName); // nicht gezeichnet — Name für UI Automation
        btnNewFolder.BackColor = SystemColors.ButtonFace;
        pathEdit.TextBox.PreviewKeyDown += (sender, e) => e.IsInputKey = e.KeyCode is Keys.Return or Keys.Enter; // verhindert, dass Enter das Formular schließt
        pathEdit.TextBox.KeyDown += PathEditTextBox_KeyDown;
        shellHistory.DropDownAnchor = pathEdit.HistoryButton; // Verlaufsmenü klappt unter dem ▼-Button des Pfadfelds auf
        pathEdit.HistoryButtonClick += (sender, e) => shellHistory.ShowDropDown();
        btnNewFolder.Click += ButtonNewFolder_Clicked;
        toolTip.SetToolTip(btnNewFolder, Lng.T("Neuer Ordner (Strg+N)"));
    }

    private void FolderSelectForm_Load(object? sender, EventArgs e)
    {
        if (copyMode) { Text = Lng.T("KOPIEREN: Wähle einen Ordner ..."); buttonOK.Text = Lng.T("Kopieren"); }
        else { Text = Lng.T("VERSCHIEBEN: Wähle einen Ordner ..."); buttonOK.Text = Lng.T("Verschieben"); }

        cbAdd2Folderlist.Checked = comboBoxTarget.SelectedIndex != -1;
        cbAdd2Folderlist.Enabled = comboBoxTarget.SelectedIndex == -1;
    }

    private void FolderSelectForm_Shown(object? sender, EventArgs e)
    {
        Cursor.Current = Cursors.Default;
        btnNewFolder.Size = new Size(48, 48);
        btnNewFolder.Location = new Point(shellTreeView.Width - btnNewFolder.Width - SystemInformation.VerticalScrollBarWidth * 2, SystemInformation.VerticalScrollBarWidth);
        shellTreeView.Controls.Add(btnNewFolder);
        pathEdit.TextBox.Focus();
        if (jumpToLastUsed) { LinkLabelRecent_LinkClicked(null, null); }
    }

    private void ShellTreeView_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e) { e.IsInputKey = e.KeyCode is Keys.Return or Keys.Enter; }

    private void ShellTreeView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.Control && !string.IsNullOrEmpty(shellTreeView.SelectedPath)) { DialogResult = DialogResult.OK; }
        else if (e.KeyData == Keys.F2) { RenameFolderMenuItem_Click(null, null); e.Handled = true; }
        else if (e.KeyData == Keys.Delete) { DeleteFolderMenuItem_Click(null, null); e.Handled = true; }
    }

    /// <summary>Rechtsklick wählt den Knoten unter der Maus, damit das Kontextmenü den richtigen Ordner meint.</summary>
    private void ShellTreeView_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Button == MouseButtons.Right) { shellTreeView.SelectedNode = e.Node; }
    }

    private void ContextMenuTree_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        newFolderMenuItem.Enabled = btnNewFolder.Visible; // ein beschreibbarer Ordner ist ausgewählt
        renameFolderMenuItem.Enabled = deleteFolderMenuItem.Enabled = shellTreeView.SelectedNode?.Parent != null; // Wurzeln (Laufwerke, Desktop …) nicht
    }

    private void RenameFolderMenuItem_Click(object? sender, EventArgs? e) => shellTreeView.RenameSelected();

    private const int CountLimit = 5000; // mehr wird nicht gezählt – die Aussage „sehr viele“ genügt, und das Zählen bleibt kurz

    /// <summary>Für die Lösch-Rückfrage: wie viele Dateien und Unterordner (rekursiv) im Ordner stecken – sie
    /// wandern mit in den Papierkorb; null, wenn der Ordner leer ist. Unzugängliche Teile werden übersprungen,
    /// ab CountLimit wird abgebrochen.</summary>
    private static string? DescribeFolderContents(string path)
    {
        var files = 0;
        var folders = 0;
        try
        {
            EnumerationOptions options = new() { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = 0 };
            foreach (var entry in new DirectoryInfo(path).EnumerateFileSystemInfos("*", options))
            {
                if ((entry.Attributes & FileAttributes.Directory) != 0) { folders++; } else { files++; }
                if (files + folders >= CountLimit) { break; }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { } // dann gilt das bisher Gezählte
        if (files == 0 && folders == 0) { return null; }
        // Ein-/Mehrzahl an drei Stellen: Dateien, Unterordner und das Pronomen des zweiten Satzes
        var fileText = files == 0 ? null
            : files + folders >= CountLimit ? string.Format(Lng.T("mehr als {0} Dateien"), CountLimit - 1)
            : files == 1 ? Lng.T("eine Datei") : string.Format(Lng.T("{0} Dateien"), files);
        var folderText = folders == 0 ? null : folders == 1 ? Lng.T("einen Unterordner") : string.Format(Lng.T("{0} Unterordner"), folders);
        var contains = fileText != null && folderText != null
            ? string.Format(Lng.T("Der Ordner enthält {0} und {1}."), fileText, folderText)
            : string.Format(Lng.T("Der Ordner enthält {0}."), fileText ?? folderText);
        var moved = files + folders == 1 ? Lng.T("Sie wird mit in den Papierkorb verschoben.") : Lng.T("Sie werden mit in den Papierkorb verschoben.");
        return contains + "\n" + moved;
    }

    private void DeleteFolderMenuItem_Click(object? sender, EventArgs? e)
    {
        if (shellTreeView.SelectedNode?.Parent == null) { return; }
        var path = shellTreeView.SelectedPath;
        Cursor.Current = Cursors.WaitCursor;
        var contents = DescribeFolderContents(path);
        Cursor.Current = Cursors.Default;
        var icon = contents == null ? TaskDialogIcon.None : TaskDialogIcon.Warning; // Warnsymbol nur, wenn Inhalt mitgeht
        if (!TaskDlg.ConfirmTaskDlg(Handle, Lng.T("In den Papierkorb verschieben?"), path + "\n\n" + (contents ?? Lng.T("Der Ordner ist leer.")), icon)) { return; }
        try { shellTreeView.DeleteSelected(); }
        catch (OperationCanceledException) { } // im Systemdialog abgebrochen
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            TaskDlg.ErrTaskDlg(Handle, Lng.T("Der Ordner konnte nicht gelöscht werden."), ex);
        }
    }

    private void ShellTreeView_DoubleClick(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(shellTreeView.SelectedPath))
        {
            Application.DoEvents(); // führt sonst zu Thread-Problemen
            DialogResult = DialogResult.OK;
        }
    }

    private void ShellTreeView_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (!string.IsNullOrEmpty(shellTreeView.SelectedPath))
        {
            pathEdit.Text = shellTreeView.SelectedPath;
            comboBoxTarget.SelectedIndex = comboBoxTarget.FindStringExact(shellTreeView.SelectedPath);
            comboBoxRecent.SelectedIndex = comboBoxRecent.FindStringExact(shellTreeView.SelectedPath);
            if (shellTreeView.SelectedNode is { } selected && selected.Nodes.Count > 0) { selected.Expand(); }
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

    private void ShellTreeView_Resize(object? sender, EventArgs e)
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

    private void ButtonNewFolder_Clicked(object? sender, EventArgs? e)
    {
        try { shellTreeView.CreateDir(Lng.T(NewFolderName), true); }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException or IOException)
        {
            TaskDlg.ErrTaskDlg(Handle, Lng.T("Der Ordner konnte nicht erstellt werden."), ex);
        }
    }

    private void ComboBoxTarget_SelectedIndexChanged(object? sender, EventArgs e) { if (comboBoxTarget.SelectedItem is string path) { SelectFolderPath(comboBoxTarget, path); } }

    private void ComboBoxRecent_SelectedIndexChanged(object? sender, EventArgs e) { if (comboBoxRecent.SelectedItem is string path) { SelectFolderPath(comboBoxRecent, path); } }

    private void SelectFolderPath(ComboBox comboBox, string path)
    {
        if (string.IsNullOrEmpty(path) || path == shellTreeView.SelectedPath) { return; }
        if (Directory.Exists(path)) { shellTreeView.SelectedPath = path; }
        else
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T(MsgDirectoryNotExist), path, TaskDialogIcon.Warning);
            comboBox.Items.Remove(path); // nur für diesen Dialog; die gespeicherten Listen bereinigt das Hauptfenster
        }
    }

    private void ShellTreeView_AfterLabelEdit(object? sender, NodeLabelEditEventArgs e)
    {
        if (shellTreeView.SelectedNode != null)
        {
            shellTreeView.SelectedNode.EnsureVisible();
            shellTreeView.TopNode = shellTreeView.SelectedNode;
            // Der umbenannte Ordner hat einen neuen Pfad — Pfadfeld und Listen nachziehen, sonst legt FormClosing
            // beim OK den alten, nicht mehr existierenden Pfad aus dem Textfeld neu an („Neuer Ordner“ neben „Bücher“)
            BeginInvoke(new Action(() => ShellTreeView_AfterSelect(shellTreeView, new TreeViewEventArgs(shellTreeView.SelectedNode))));
        }
    }

    private void PathEdit_ButtonClick(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(pathEdit.Text))
        {
            if (pathEdit.IsValidPath) { shellTreeView.SelectedPath = pathEdit.Text; }
            else if (Path.HasExtension(pathEdit.Text) && Directory.Exists(Path.GetDirectoryName(pathEdit.Text))) { shellTreeView.SelectedPath = Path.GetDirectoryName(pathEdit.Text)!; }
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

    private void PathEditTextBox_KeyDown(object? sender, KeyEventArgs e)
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

    private void PathEdit_EditFieldEnter(object? sender, EventArgs e)
    {
        if (MouseButtons == MouseButtons.None)
        {
            selectAllDone = true;
            pathEdit.TextBox.SelectAll();
        }
    }

    private void PathEdit_EditFieldLeave(object? sender, EventArgs e) { selectAllDone = false; }

    private void PathEdit_EditFieldClick(object? sender, EventArgs e)
    {
        if (!selectAllDone && pathEdit.TextBox.SelectionLength == 0)
        {
            selectAllDone = true;
            pathEdit.TextBox.SelectAll();
        }
    }

    private void FolderSelectForm_FormClosing(object? sender, FormClosingEventArgs e)
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
                TaskDlg.ErrTaskDlg(Handle, Lng.T("Der Ordner konnte nicht erstellt werden."), ex);
                pathEdit.TextBox.Clear();
                e.Cancel = true;
            }
        }
    }

    private void FolderSelectForm_HelpButtonClicked(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        ShowHelpMsgBox();
    }

    private void FolderSelectForm_HelpRequested(object? sender, HelpEventArgs hlpevent)
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
            ("F2", "Ausgewählten Ordner umbenennen."),
            ("Entf", "Ausgewählten Ordner in den Papierkorb verschieben."),
            ("Strg+Eingabe", "Auswahl übernehmen."),
        ]);
        TaskDlg.MsgTaskDlg(Handle, Lng.T("Tastenkürzel"), text);
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

    private void LinkLabelRecent_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs? e)
    {
        if (comboBoxRecent.Items.Count > 0 && comboBoxRecent.Items[0] is string first) { SelectFolderPath(comboBoxRecent, first); }
    }
}
