using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;
using PDFLight.Classes;
using PDFLight.Controls;

namespace PDFLight.Forms;

/// <summary>
/// Umbenennen-Dialog (portiert aus PDFMover): Namenseingabe mit Umwandeln- und Datums-Menü,
/// dazu die Liste aller PDF-Dateien des Ordners als Namensvorlage (Doppelklick/Eingabe übernimmt,
/// F2 benennt Listeneinträge direkt um, Entf löscht in den Papierkorb) sowie optionaler Ordnerwechsel.
/// </summary>
public partial class RenameForm : Form
{
    public string NewName => renameTextBox.Text;
    public string NewFolder => directoryTextBox.Text;

    private FileInfo fileInfo;
    private bool selectAllDone;
    private bool dateOrderDescending = true;
    private bool nameOrderDescending;
    private string filenameBeforeListEdit;

    public RenameForm(FileInfo currentFile)
    {
        InitializeComponent();
        fileInfo = currentFile;
        directoryTextBox.Text = fileInfo.DirectoryName;
        listView.PreviewKeyDown += (sender, e) => { if (e.KeyCode == Keys.Enter) { e.IsInputKey = true; } }; // Enter in der Liste übernimmt den Namen statt OK

        // Datums-Menü: obere Hälfte = Präfixe, untere Hälfte = Suffixe (wie in PDFMover)
        btnDateMenu.Items.Add(DateTime.Now.ToString("yyyyMMdd_"));
        btnDateMenu.Items.Add(DateTime.Now.AddDays(-1).ToString("yyyyMMdd_"));
        btnDateMenu.Items.Add(DateTime.Now.AddDays(-2).ToString("yyyyMMdd_"));
        btnDateMenu.Items.Add("-");
        btnDateMenu.Items.Add(DateTime.Now.ToString("yyyy-MM_"));
        btnDateMenu.Items.Add(DateTime.Now.AddMonths(-1).ToString("yyyy-MM_"));
        btnDateMenu.Items.Add("-");
        btnDateMenu.Items.Add(DateTime.Now.ToString("yyyy_"));
        btnDateMenu.Items.Add(DateTime.Now.AddYears(-1).ToString("yyyy_"));
        btnDateMenu.Items.Add(new ExtendedToolStripSeparator());
        btnDateMenu.Items.Add(DateTime.Now.ToString("       _ddMMyyyy"));
        btnDateMenu.Items.Add(DateTime.Now.AddDays(-1).ToString("       _ddMMyyyy"));
        btnDateMenu.Items.Add(DateTime.Now.AddDays(-2).ToString("       _ddMMyyyy"));
        btnDateMenu.Items.Add("-");
        btnDateMenu.Items.Add(DateTime.Now.ToString("         _MM-yyyy"));
        btnDateMenu.Items.Add(DateTime.Now.AddMonths(-1).ToString("         _MM-yyyy"));
        btnDateMenu.Items.Add("-");
        btnDateMenu.Items.Add(DateTime.Now.ToString("               _yyyy"));
        btnDateMenu.Items.Add(DateTime.Now.AddYears(-1).ToString("               _yyyy"));
    }

    private void RenameForm_Load(object sender, EventArgs e)
    {
        renameTextBox.Text = Path.GetFileNameWithoutExtension(fileInfo.Name);
        FillList(SortedFiles());
        listView.Columns[0].Width = listView.ClientSize.Width - 4;
    }

    private void RenameForm_Shown(object sender, EventArgs e) { renameTextBox.Focus(); }

    private IEnumerable<string> SortedFiles()
    {
        var files = FileUtil.GetPdfFilesInFolder(fileInfo.DirectoryName); // natürliche Sortierung
        if (dateSortButton.Checked)
        {
            return dateOrderDescending ? files.OrderBy(f => File.GetLastWriteTime(f)) : files.OrderByDescending(f => File.GetLastWriteTime(f));
        }
        if (nameOrderDescending) { files.Reverse(); }
        return files;
    }

    private void FillList(IEnumerable<string> files)
    {
        listView.BeginUpdate();
        listView.Items.Clear();
        foreach (var file in files) { listView.Items.Add(new ListViewItem(Path.GetFileName(file)) { Name = file }); }
        var current = listView.Items.Cast<ListViewItem>().FirstOrDefault(x => string.Equals(x.Name, fileInfo.FullName, StringComparison.OrdinalIgnoreCase));
        if (current != null)
        {
            current.ForeColor = Color.White;
            current.BackColor = Color.LightGray; // die aktuelle Datei ist nur Referenz, nicht Vorlage
            current.EnsureVisible();
        }
        listView.EndUpdate();
    }

    // ------------------------------------------------------------------ Namensfeld

    private void RenameTextBox_Enter(object sender, EventArgs e)
    {
        if (MouseButtons == MouseButtons.None) { selectAllDone = true; renameTextBox.SelectAll(); }
    }

    private void RenameTextBox_Leave(object sender, EventArgs e) { selectAllDone = false; renameTextBox.SelectionStart = renameTextBox.Text.Length; }

    private void RenameTextBox_Click(object sender, EventArgs e)
    {
        if (!selectAllDone && renameTextBox.SelectionLength == 0) { selectAllDone = true; renameTextBox.SelectAll(); }
    }

    private void RenameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (((e.Control && e.KeyCode == Keys.V) || (e.Shift && e.KeyCode == Keys.Insert)) && Clipboard.ContainsText())
        {
            renameTextBox.Paste(Clipboard.GetText().Replace(Environment.NewLine, " ").Replace("  ", " ").Trim()); // Zeilenumbrüche beim Einfügen glätten
            e.SuppressKeyPress = true;
        }
        else if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.Control)
        {
            e.SuppressKeyPress = true;
            DialogResult = DialogResult.OK;
        }
    }

    private void RenameTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Back || (ModifierKeys & Keys.Control) == Keys.Control) { return; }
        if (Path.GetInvalidFileNameChars().Contains(e.KeyChar)) { e.Handled = true; Console.Beep(); }
    }

    private void RenameTextBox_TextChanged(object sender, EventArgs e)
    {
        var text = renameTextBox.Text;
        btnOK.Enabled = !(string.IsNullOrWhiteSpace(text) || text == fileInfo.Name || text == Path.GetFileNameWithoutExtension(fileInfo.Name));
        AcceptButton = btnOK.Enabled ? btnOK : btnCancel;
        if (text.Length > 0) // Liste zum passenden Eintrag scrollen
        {
            var found = listView.Items.Cast<ListViewItem>().FirstOrDefault(x => Path.GetFileNameWithoutExtension(x.Text).StartsWith(text, StringComparison.OrdinalIgnoreCase));
            if (found != null) { listView.TopItem = found; }
        }
    }

    // ------------------------------------------------------------------ Umwandeln- und Datums-Menü

    private string NameWithoutPdf()
    {
        var text = renameTextBox.Text.Trim();
        return text.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? text[..^4] : text;
    }

    private void SetName(string text)
    {
        renameTextBox.Text = text;
        btnOK.Focus();
        renameTextBox.Focus();
    }

    private void BtnTransform_Click(object sender, EventArgs e) { btnTransformMenu.Show(btnTransform.PointToScreen(new Point(0, btnTransform.Height))); }

    private void UnderscoreMenuItem_Click(object sender, EventArgs e) { SetName(NameWithoutPdf().Replace(", ", "_").Replace(" – ", "-").Replace(" ", "_")); }

    private void HyphensMenuItem_Click(object sender, EventArgs e) { SetName(NameWithoutPdf().Replace(", ", "-").Replace(" – ", "_").Replace(" ", "-")); }

    private void LowercaseMenuItem_Click(object sender, EventArgs e) { SetName(NameWithoutPdf().ToLowerInvariant()); }

    private void FirstLetterMenuItem_Click(object sender, EventArgs e) { SetName(System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(NameWithoutPdf().ToLower())); }

    private void RemoveDiacriticMenuItem_Click(object sender, EventArgs e) { SetName(FileUtil.RemoveDiacritics(NameWithoutPdf())); }

    private void BtnDate_Click(object sender, EventArgs e) { btnDateMenu.Show(btnDate.PointToScreen(new Point(0, btnDate.Height))); }

    [GeneratedRegex(@"^\d{4}-?((0[1-9])|(1[012]))?((0[1-9]|[12]\d)|3[01])?(_|-)")]
    private static partial Regex DatePrefixRegex();

    [GeneratedRegex(@"(_|-)((0[1-9]|[12]\d)|3[01])?((0[1-9])|(1[012]))?-?\d{4}$")]
    private static partial Regex DateSuffixRegex();

    private void BtnDateMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
        var index = btnDateMenu.Items.IndexOf(e.ClickedItem);
        var split = btnDateMenu.Items.Count / 2;
        var date = e.ClickedItem.Text.Trim();
        var name = NameWithoutPdf();
        name = DatePrefixRegex().Replace(name, ""); // vorhandenes Datums-Präfix ersetzen
        name = DateSuffixRegex().Replace(name, ""); // vorhandenes Datums-Suffix ersetzen
        if (index < split) { SetName(date + name); }
        else if (index > split) { SetName(name + date); }
    }

    // ------------------------------------------------------------------ Dateiliste

    private void ListView_KeyDown(object sender, KeyEventArgs e)
    {
        if (listView.SelectedItems.Count == 0) { return; }
        if (e.KeyData == Keys.F2) { listView.SelectedItems[0].BeginEdit(); }
        else if (e.KeyData == Keys.Delete) { DeleteMenuItem_Click(null, null); }
        else if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.Control) { OpenMenuItem_Click(null, null); e.Handled = e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.Enter)
        {
            renameTextBox.Text = Path.GetFileNameWithoutExtension(listView.SelectedItems[0].Text);
            e.Handled = e.SuppressKeyPress = true;
        }
    }

    private void ListView_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        var clicked = listView.HitTest(e.Location).Item;
        if (clicked != null) { renameTextBox.Text = Path.GetFileNameWithoutExtension(clicked.Text); }
    }

    private void ListView_BeforeLabelEdit(object sender, LabelEditEventArgs e)
    {
        if (string.Equals(listView.Items[e.Item].Name, fileInfo.FullName, StringComparison.OrdinalIgnoreCase)) { e.CancelEdit = true; } // aktuelle Datei nur übers Namensfeld
        else { filenameBeforeListEdit = listView.Items[e.Item].Text; }
    }

    private void ListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
        if (e.Label == null) { return; }
        try
        {
            var destPath = Path.Combine(fileInfo.DirectoryName, e.Label);
            File.Move(Path.Combine(fileInfo.DirectoryName, filenameBeforeListEdit), destPath);
            listView.Items[e.Item].Name = destPath;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            TaskDlg.ErrTaskDlg(Handle, "Umbenennen fehlgeschlagen.", ex);
            e.CancelEdit = true;
        }
    }

    private void ContextMenuListView_Opening(object sender, CancelEventArgs e)
    {
        if (listView.SelectedItems.Count == 0) { e.Cancel = true; return; }
        var isCurrent = string.Equals(listView.SelectedItems[0].Name, fileInfo.FullName, StringComparison.OrdinalIgnoreCase);
        deleteMenuItem.Enabled = renameMenuItem.Enabled = openMenuItem.Enabled = !isCurrent;
    }

    private void AcceptMenuItem_Click(object sender, EventArgs e)
    {
        if (listView.SelectedItems.Count > 0) { renameTextBox.Text = Path.GetFileNameWithoutExtension(listView.SelectedItems[0].Text); }
    }

    private void RenameMenuItem_Click(object sender, EventArgs e)
    {
        if (listView.SelectedItems.Count > 0) { listView.SelectedItems[0].BeginEdit(); }
    }

    private void OpenMenuItem_Click(object sender, EventArgs e)
    {
        if (listView.SelectedItems.Count == 0) { return; }
        try { Process.Start(new ProcessStartInfo(Application.ExecutablePath, $"\"{listView.SelectedItems[0].Name}\"") { UseShellExecute = false }); } // neue PDFlight-Instanz
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException) { TaskDlg.ErrTaskDlg(Handle, "PDFlight konnte nicht gestartet werden.", ex); }
    }

    private void DeleteMenuItem_Click(object sender, EventArgs e)
    {
        if (listView.SelectedItems.Count == 0
            || string.Equals(listView.SelectedItems[0].Name, fileInfo.FullName, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!TaskDlg.ConfirmTaskDlg(Handle, "In den Papierkorb verschieben?", listView.SelectedItems[0].Text)) { return; }
        try
        {
            FileSystem.DeleteFile(listView.SelectedItems[0].Name, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            listView.SelectedItems[0].Remove();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { TaskDlg.ErrTaskDlg(Handle, "Löschen fehlgeschlagen.", ex); }
    }

    private void PropertiesMenuItem_Click(object sender, EventArgs e)
    {
        if (listView.SelectedItems.Count > 0) { ShellUtil.ShowFileProperties(listView.SelectedItems[0].Name); }
    }

    // ------------------------------------------------------------------ Sortierung (F5/F6)

    private void AlphabeticSortButton_Click(object sender, EventArgs e)
    {
        if (alphabeticSortButton.Checked) { nameOrderDescending = !nameOrderDescending; }
        alphabeticSortButton.Checked = true;
        dateSortButton.Checked = false;
        alphabeticSortButton.Text = nameOrderDescending ? "Z–A" : "A–Z";
        FillList(SortedFiles());
    }

    private void DateSortButton_Click(object sender, EventArgs e)
    {
        if (dateSortButton.Checked) { dateOrderDescending = !dateOrderDescending; }
        dateSortButton.Checked = true;
        alphabeticSortButton.Checked = false;
        dateSortButton.ToolTipText = dateOrderDescending ? "Änderungsdatum (neu → alt)" : "Änderungsdatum (alt → neu)";
        FillList(SortedFiles());
    }

    // ------------------------------------------------------------------ Ordner

    private void FolderButton_Click(object sender, EventArgs e)
    {
        if (Directory.Exists(directoryTextBox.Text))
        {
            try { Process.Start(new ProcessStartInfo("explorer.exe", $"/e, /select,\"{Path.Combine(directoryTextBox.Text, fileInfo.Name)}\"")); }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException) { TaskDlg.ErrTaskDlg(Handle, "Der Ordner konnte nicht geöffnet werden.", ex); }
        }
        else { TaskDlg.MsgTaskDlg(Handle, "Der angegebene Pfad existiert nicht.", null, TaskDialogIcon.Warning); }
    }

    private void OtherFolderButton_Click(object sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new() { Description = "Zielordner für die umbenannte Datei", UseDescriptionForTitle = true, InitialDirectory = directoryTextBox.Text };
        if (dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(dialog.SelectedPath) && !dialog.SelectedPath.Equals(fileInfo.DirectoryName, StringComparison.OrdinalIgnoreCase))
        {
            directoryTextBox.Text = dialog.SelectedPath;
            FillList(FileUtil.GetPdfFilesInFolder(dialog.SelectedPath)); // Liste zeigt jetzt den Zielordner
            btnOK.Enabled = true; // Ordnerwechsel allein ist bereits eine Änderung
        }
    }

    // ------------------------------------------------------------------ Abschluss

    private void RenameForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (DialogResult != DialogResult.OK) { return; }
        var fileName = renameTextBox.Text.Trim();
        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            TaskDlg.MsgTaskDlg(Handle, "Der Name enthält ungültige Zeichen.", "Diese werden entfernt …", TaskDialogIcon.Information);
            renameTextBox.Text = Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
            e.Cancel = true;
            return;
        }
        var sameFolder = directoryTextBox.Text.Equals(fileInfo.DirectoryName, StringComparison.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(fileName) || ((fileName == Path.GetFileNameWithoutExtension(fileInfo.Name) || fileName == fileInfo.Name) && sameFolder))
        {
            DialogResult = DialogResult.Cancel; // nichts geändert
            return;
        }
        var newPath = Path.Combine(directoryTextBox.Text, fileName);
        if (!Path.GetExtension(newPath).Equals(".pdf", StringComparison.OrdinalIgnoreCase)) { newPath += ".pdf"; }
        renameTextBox.Text = Path.GetFileName(newPath); // ab hier inklusive Erweiterung (NewName)
        if (!newPath.Equals(fileInfo.FullName, StringComparison.OrdinalIgnoreCase) && File.Exists(newPath))
        {
            if (TaskDlg.ConfirmTaskDlg(Handle, "Vorhandene Datei ersetzen?", "Eine Datei gleichen Namens ist bereits vorhanden. Sie wird in den Papierkorb verschoben.", TaskDialogIcon.Warning, defaultNo: true))
            {
                try { FileSystem.DeleteFile(newPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin); }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or OperationCanceledException)
                {
                    TaskDlg.ErrTaskDlg(Handle, "Die vorhandene Datei konnte nicht ersetzt werden.", ex);
                    e.Cancel = true;
                }
            }
            else { e.Cancel = true; }
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.F2: { if (ActiveControl != listView) { renameTextBox.SelectAll(); renameTextBox.Focus(); return true; } return false; }
            case Keys.F4: { OtherFolderButton_Click(null, null); return true; }
            case Keys.F5: { AlphabeticSortButton_Click(null, null); return true; }
            case Keys.F6: { DateSortButton_Click(null, null); return true; }
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
