using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Web.WebView2.Core;
using PDFLight.Classes;

namespace PDFLight.Forms;

public partial class MainForm : Form
{
    private readonly AppSettings settings;
    private readonly PdfViewHost viewHost;
    private readonly string startFile;
    private readonly Dictionary<string, Image> programIcons = new(StringComparer.OrdinalIgnoreCase);
    private FileInfo currentFile;
    private int currentPageCount = -1;      // -1 = nicht bestimmbar (z.B. verschlüsselt)
    private DateTime loadedWriteTimeUtc;    // erkennt externe Änderungen an der angezeigten Datei
    private string undoBackupFile;          // Sicherungskopie für einstufiges Rückgängig
    private string undoTargetFile;          // Datei, für die die Sicherung gilt
    private bool isFullScreen;              // F11-Vollbild (randlos, ohne Tool-/Statusleiste)
    private FormWindowState fullScreenPreviousState;

    public MainForm(string startFile)
    {
        InitializeComponent();
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } // Fenstersymbol = Programmicon der EXE
        catch (Exception ex) when (ex is ArgumentException or IOException) { }
        this.startFile = startFile;
        settings = AppSettings.Load();
        viewHost = new PdfViewHost(webView);
        RestoreWindowBounds();
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        try { await viewHost.InitializeAsync(); }
        catch (WebView2RuntimeNotFoundException)
        {
            TaskDlg.MsgTaskDlg(Handle, "Die WebView2-Runtime ist nicht installiert.", "Bitte installieren Sie sie über:" + Environment.NewLine + "https://developer.microsoft.com/microsoft-edge/webview2/", TaskDialogIcon.Error);
            Close();
            return;
        }
        catch (Exception ex) when (ex is DllNotFoundException or InvalidOperationException or System.Runtime.InteropServices.COMException or IOException or UnauthorizedAccessException)
        {
            // z.B. beschädigte Installation oder gesperrter Datenordner — sauberer Dialog statt WinForms-Absturzfenster
            TaskDlg.ErrTaskDlg(Handle, "Die PDF-Anzeige (WebView2) konnte nicht gestartet werden.", ex);
            Close();
            return;
        }
        webView.KeyDown += WebView_KeyDown; // Tastenkürzel funktionieren auch, wenn der Viewer den Fokus hat
        viewHost.PdfFileDropped += (s, path) => LoadPdf(path, addToRecent: true); // Drop auf das Viewer-Areal
        InitDropDownClickShield();
        EnableClassicDragDrop();
        EnsureProgramList();
        RebuildProgramIconButtons();
        ApplyToolbarIcons();
        toolStrip.Resize += (s, args) => UpdateProgramIconVisibility();

        if (!string.IsNullOrEmpty(startFile) && File.Exists(startFile)) { LoadPdf(startFile, addToRecent: true); }
        else if (settings.ReopenLastFile && !string.IsNullOrEmpty(settings.LastFile) && File.Exists(settings.LastFile)) { LoadPdf(settings.LastFile); }
        else { UpdateUiState(); }
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        SetFullScreen(false); // sonst würden randlose Vollbild-Maße gespeichert
        settings.ReloadSharedLists(); // Listenänderungen anderer Instanzen nicht überschreiben
        settings.LastFile = currentFile?.FullName ?? string.Empty;
        var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        settings.WindowX = bounds.X;
        settings.WindowY = bounds.Y;
        settings.WindowWidth = bounds.Width;
        settings.WindowHeight = bounds.Height;
        settings.WindowMaximized = WindowState == FormWindowState.Maximized;
        settings.Save();
    }

    private void MainForm_Activated(object sender, EventArgs e)
    {
        if (currentFile == null) { return; }
        try
        {
            // Datei wurde extern geändert (z.B. in einem anderen Programm gespeichert) → Anzeige aktualisieren
            if (File.Exists(currentFile.FullName) && File.GetLastWriteTimeUtc(currentFile.FullName) != loadedWriteTimeUtc)
            {
                LoadPdf(currentFile.FullName);
            }
        }
        catch (IOException) { }
    }

    private void RestoreWindowBounds()
    {
        if (settings.WindowWidth > 200 && settings.WindowHeight > 200)
        {
            Rectangle bounds = new(settings.WindowX, settings.WindowY, settings.WindowWidth, settings.WindowHeight);
            if (Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(bounds))) // nicht auf einem abgesteckten Monitor wiederherstellen
            {
                StartPosition = FormStartPosition.Manual;
                Bounds = bounds;
            }
        }
        if (settings.WindowMaximized) { WindowState = FormWindowState.Maximized; }
    }

    // ------------------------------------------------------------------ Menü-Schließen über dem WebView

    /// <summary>Praktisch unsichtbares Fenster über dem Viewer: Chromium läuft in einem eigenen Prozess,
    /// darum bekommt WinForms Klicks ins WebView nicht mit und offene Toolbar-Menüs blieben stehen.
    /// Solange ein Menü offen ist, fängt dieses Schild den ersten Klick ab und schließt das Menü
    /// (der Klick wird dabei wie bei Menüs üblich verschluckt).</summary>
    private sealed class ClickShieldForm : Form
    {
        public ClickShieldForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Opacity = 0.01; // bei 0 wären Klicks durchlässig
        }

        protected override bool ShowWithoutActivation => true; // darf dem Menü nicht den Fokus stehlen
    }

    private ClickShieldForm clickShield;
    private ToolStripDropDownItem[] dropDownButtons;

    private void InitDropDownClickShield()
    {
        clickShield = new ClickShieldForm();
        clickShield.MouseDown += (s, e) => CloseToolStripDropDowns();
        dropDownButtons = [btnOpen, splitButtonMove, ddbEdit, ddbPrograms];
        foreach (var item in dropDownButtons)
        {
            item.DropDownOpened += (s, e) => ShowClickShield();
            item.DropDownClosed += (s, e) => clickShield.Hide();
        }
    }

    private void ShowClickShield()
    {
        clickShield.Bounds = new Rectangle(webView.PointToScreen(Point.Empty), webView.Size);
        clickShield.Show(this); // liegt über dem WebView, aber unter dem (Topmost-)Menü
    }

    private void CloseToolStripDropDowns()
    {
        foreach (var item in dropDownButtons) { item.HideDropDown(); }
        clickShield.Hide();
    }

    /// <summary>F11-Vollbild wie im Browser: randlos maximiert, Tool- und Statusleiste ausgeblendet; Esc oder F11 beendet.</summary>
    private void SetFullScreen(bool enable)
    {
        if (isFullScreen == enable) { return; }
        isFullScreen = enable;
        SuspendLayout();
        if (enable)
        {
            fullScreenPreviousState = WindowState;
            toolStrip.Visible = statusStrip.Visible = false;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal;    // erzwingt die Neuberechnung, falls das Fenster bereits maximiert war
            WindowState = FormWindowState.Maximized; // randlos maximiert = echtes Vollbild inkl. Taskleiste
        }
        else
        {
            toolStrip.Visible = statusStrip.Visible = true;
            FormBorderStyle = FormBorderStyle.Sizable;
            WindowState = fullScreenPreviousState;
        }
        ResumeLayout();
    }

    // ------------------------------------------------------------------ Laden & Navigation

    private void LoadPdf(string path, int page = 0, bool addToRecent = false)
    {
        try { viewHost.Load(path, page); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            TaskDlg.ErrTaskDlg(Handle, "Die Datei konnte nicht geladen werden.", ex);
            return;
        }
        currentFile = new FileInfo(path);
        loadedWriteTimeUtc = currentFile.LastWriteTimeUtc;
        currentPageCount = PdfEditService.TryGetPageCount(path);
        if (addToRecent) // nur bei bewusstem Öffnen — nicht beim Blättern durch den Ordner
        {
            settings.ReloadSharedLists();
            settings.AddRecentFile(currentFile.FullName);
            settings.Save();
        }
        UpdateUiState();
    }

    private void UpdateUiState()
    {
        var hasFile = currentFile != null;
        Text = hasFile ? currentFile.Name + " – PDFlight" : "PDFlight";
        splitButtonMove.Enabled = btnCopy.Enabled = btnRename.Enabled = btnDelete.Enabled = btnShowInFolder.Enabled = ddbEdit.Enabled = btnEmail.Enabled = hasFile;
        foreach (var button in programIconButtons) { button.Enabled = hasFile; }
        if (hasFile)
        {
            var files = FileUtil.GetPdfFilesInFolder(currentFile.DirectoryName);
            var index = files.FindIndex(f => string.Equals(f, currentFile.FullName, StringComparison.OrdinalIgnoreCase));
            statusIndex.Text = (index >= 0 ? (index + 1).ToString() : "–") + "/" + files.Count;
            statusPath.Text = currentFile.FullName;
            var pages = currentPageCount > 0 ? currentPageCount + (currentPageCount == 1 ? " Seite   " : " Seiten   ") : string.Empty;
            statusInfo.Text = $"{pages}{currentFile.Length / 1024.0:N0} KB   {currentFile.LastWriteTime:g}";
            btnPrev.Enabled = btnNext.Enabled = files.Count > 1;
            btnPageUp.Enabled = btnPageDown.Enabled = true;
        }
        else
        {
            statusIndex.Text = "0/0";
            statusPath.Text = "Keine Datei geöffnet";
            statusInfo.Text = string.Empty;
            btnPrev.Enabled = btnNext.Enabled = btnPageUp.Enabled = btnPageDown.Enabled = false;
        }
    }

    private void OpenFile()
    {
        using OpenFileDialog dialog = new() { Filter = "PDF-Dateien (*.pdf)|*.pdf", Title = "PDF-Datei öffnen" };
        if (currentFile != null) { dialog.InitialDirectory = currentFile.DirectoryName; }
        if (dialog.ShowDialog(this) == DialogResult.OK) { LoadPdf(dialog.FileName, addToRecent: true); }
    }

    /// <summary>Dropdown des Öffnen-Buttons: die zuletzt geöffneten Dateien.</summary>
    private void BtnOpen_DropDownOpening(object sender, EventArgs e)
    {
        settings.ReloadSharedLists();
        btnOpen.DropDownItems.Clear();
        foreach (var file in settings.RecentFiles)
        {
            ToolStripMenuItem item = new(Path.GetFileName(file).Replace("&", "&&"))
            {
                Tag = file,
                ToolTipText = file,
                Enabled = File.Exists(file),
            };
            item.Click += (s, args) => LoadPdf((string)((ToolStripMenuItem)s).Tag, addToRecent: true);
            btnOpen.DropDownItems.Add(item);
        }
        if (btnOpen.DropDownItems.Count == 0)
        {
            btnOpen.DropDownItems.Add(new ToolStripMenuItem("(keine zuletzt geöffneten Dateien)") { Enabled = false });
        }
        else
        {
            btnOpen.DropDownItems.Add(new ToolStripSeparator());
            ToolStripMenuItem clear = new("Liste leeren");
            clear.Click += (s, args) => { settings.RecentFiles.Clear(); settings.Save(); };
            btnOpen.DropDownItems.Add(clear);
        }
    }

    /// <summary>Klassisches Drag &amp; Drop für Formular, Tool- und Statusleiste;
    /// Drops auf das Viewer-Areal fängt PdfViewHost über die file://-Navigation ab.</summary>
    private void EnableClassicDragDrop()
    {
        AllowDrop = true;
        DragEnter += HandleDragEnter;
        DragDrop += HandleDragDrop;
        toolStrip.AllowDrop = true;
        toolStrip.DragEnter += HandleDragEnter;
        toolStrip.DragDrop += HandleDragDrop;
        statusStrip.AllowDrop = true;
        statusStrip.DragEnter += HandleDragEnter;
        statusStrip.DragDrop += HandleDragDrop;
    }

    private void HandleDragEnter(object sender, DragEventArgs e) { e.Effect = GetDroppedPdf(e) != null ? DragDropEffects.Copy : DragDropEffects.None; }

    private void HandleDragDrop(object sender, DragEventArgs e)
    {
        var file = GetDroppedPdf(e);
        if (file != null)
        {
            LoadPdf(file);
            Activate();
        }
    }

    private static string GetDroppedPdf(DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true && e.Data.GetData(DataFormats.FileDrop) is string[] files)
        {
            return files.FirstOrDefault(f => f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) && File.Exists(f));
        }
        return null;
    }

    /// <summary>Blättert zur nächsten/vorherigen PDF-Datei im Ordner (mit Umlauf, wie in PDFMover).</summary>
    private void StepFile(int step)
    {
        if (currentFile == null) { return; }
        var files = FileUtil.GetPdfFilesInFolder(currentFile.DirectoryName);
        if (files.Count == 0) { return; }
        var index = files.FindIndex(f => string.Equals(f, currentFile.FullName, StringComparison.OrdinalIgnoreCase));
        index = index < 0 ? 0 : (index + step + files.Count) % files.Count;
        if (!string.Equals(files[index], currentFile.FullName, StringComparison.OrdinalIgnoreCase)) { LoadPdf(files[index]); }
    }

    /// <summary>Nach Verschieben/Löschen: nächste Datei an gleicher Position laden oder Anzeige leeren.</summary>
    private void LoadNextAfterRemoval(List<string> files, int removedIndex)
    {
        if (removedIndex >= 0) { files.RemoveAt(removedIndex); } else { removedIndex = 0; }
        if (files.Count == 0)
        {
            currentFile = null;
            viewHost.CloseDocument();
            UpdateUiState();
            statusPath.Text = "Der Ordner enthält keine weiteren PDF-Dateien.";
        }
        else { LoadPdf(files[removedIndex % files.Count]); }
    }

    // ------------------------------------------------------------------ Verschieben / Kopieren

    private void MoveCopyDialog(bool copy)
    {
        if (currentFile == null) { return; }
        Cursor.Current = Cursors.WaitCursor;
        settings.ReloadSharedLists(); // Ziel-/Zuletzt-Listen anderer Instanzen übernehmen
        var startFolder = settings.TargetFolders.FirstOrDefault(f => !string.IsNullOrEmpty(f) && Directory.Exists(f)) ?? string.Empty;
        var jumpToLastUsed = settings.JumpToLastUsed && settings.RecentFolders.Count > 0
            && !string.Equals(startFolder, settings.RecentFolders[0], StringComparison.OrdinalIgnoreCase);

        using FolderSelectForm dialog = new(startFolder, copy, jumpToLastUsed);
        if (settings.RecentFolders.Count > 0) { dialog.RecentComboBox.Items.AddRange([.. settings.RecentFolders]); }
        dialog.TargetComboBox.Items.AddRange([.. settings.TargetFolders.Where(f => !string.IsNullOrEmpty(f))]);
        dialog.ShellTreePath = startFolder;

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var folder = dialog.ShellTreePath;
            if (string.IsNullOrEmpty(folder)) { return; }
            if (dialog.Add2Folderlist.Checked && !settings.TargetFolders.Any(f => string.Equals(f, folder, StringComparison.OrdinalIgnoreCase)))
            {
                settings.TargetFolders.Insert(0, folder);
                settings.Save();
            }
            MoveOrCopyTo(folder, copy);
        }
        Cursor.Current = Cursors.Default;
    }

    private void MoveOrCopyTo(string folder, bool copy)
    {
        if (currentFile == null) { return; }
        if (!Directory.Exists(folder))
        {
            TaskDlg.MsgTaskDlg(Handle, "Der Zielordner existiert nicht.", folder, TaskDialogIcon.Warning);
            return;
        }
        if (string.Equals(Path.TrimEndingDirectorySeparator(folder), currentFile.DirectoryName, StringComparison.OrdinalIgnoreCase))
        {
            TaskDlg.MsgTaskDlg(Handle, "Die Datei befindet sich bereits in diesem Ordner.", null, TaskDialogIcon.Information);
            return;
        }
        var destination = Path.Combine(folder, currentFile.Name);
        if (File.Exists(destination))
        {
            destination = AskReplaceOrRename(destination); // Ersetzen / name_n anlegen / Abbrechen (wie in PDFMover)
            if (destination == null) { return; }
        }
        try
        {
            settings.ReloadSharedLists(); // parallel laufende Instanzen nicht überschreiben
            if (copy)
            {
                File.Copy(currentFile.FullName, destination, true);
                statusPath.Text = "Kopiert nach: " + destination;
                AskWhichFileToShow(destination);
            }
            else
            {
                File.Move(currentFile.FullName, destination, true);
                LoadPdf(destination); // die verschobene Datei bleibt angezeigt — nun vom neuen Ort (wie in PDFMover)
                CheckForDuplicate(new FileInfo(destination));
            }
            settings.AddRecentFolder(folder);
            settings.Save();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            TaskDlg.ErrTaskDlg(Handle, (copy ? "Kopieren" : "Verschieben") + " fehlgeschlagen.", ex);
        }
    }

    /// <summary>Zieldatei existiert bereits: Ersetzen, unter freiem name_n-Namen anlegen oder abbrechen (Dialog wie in PDFMover).
    /// Liefert den zu verwendenden Zielpfad oder null bei Abbruch.</summary>
    private string AskReplaceOrRename(string destination)
    {
        FileInfo destInfo = new(destination);
        var suggestion = FileUtil.SuggestAdditionalFileName(destInfo);
        TaskDialogButton btnReplace = new TaskDialogCommandLinkButton("&Ersetzen",
            $"Die vorhandene Datei ersetzen\nin {destInfo.DirectoryName}: {destInfo.Length / 1024} KB, {destInfo.LastWriteTime.ToShortDateString()}");
        TaskDialogButton btnRename = new TaskDialogCommandLinkButton("&Umbenennen", "Eine neue Datei erstellen:\n" + suggestion?.Name);
        var page = new TaskDialogPage()
        {
            Caption = currentFile.DirectoryName,
            Heading = "Im Ziel ist bereits eine Datei mit diesem Namen vorhanden.",
            Text = destInfo.Name,
            AllowCancel = true,
            SizeToContent = true,
            Buttons = { btnReplace, TaskDialogButton.Cancel },
            DefaultButton = btnReplace
        };
        if (suggestion != null) { page.Buttons.Insert(1, btnRename); }
        var result = TaskDialog.ShowDialog(Handle, page);
        if (result == btnReplace) { return destination; }
        if (result == btnRename) { return suggestion.FullName; }
        return null;
    }

    /// <summary>Nach dem Kopieren: Dateikopie oder Originaldatei weiter anzeigen? (Dialog wie in PDFMover)</summary>
    private void AskWhichFileToShow(string copiedFile)
    {
        TaskDialogButton btnCopy = new TaskDialogCommandLinkButton("Dateikopie:", copiedFile);
        TaskDialogButton btnSource = new TaskDialogCommandLinkButton("Originaldatei:", currentFile.FullName);
        var page = new TaskDialogPage()
        {
            Caption = Application.ProductName,
            Heading = "Welche Datei soll geöffnet werden?",
            AllowCancel = true,
            SizeToContent = true,
            Buttons = { btnCopy, btnSource, TaskDialogButton.Cancel },
            DefaultButton = btnCopy
        };
        var result = TaskDialog.ShowDialog(Handle, page);
        if (result == btnCopy) { LoadPdf(copiedFile, addToRecent: true); }
        else if (result == btnSource) { settings.AddRecentFile(copiedFile); } // Kopie in die Zuletzt-Liste; das Original bleibt angezeigt
    }

    /// <summary>Nach dem Verschieben: liegt im Zielordner bereits eine inhaltsgleiche Datei? (wie in PDFMover;
    /// statt einer neuen Instanz wird die vorhandene Datei im selben Fenster angezeigt)</summary>
    private void CheckForDuplicate(FileInfo movedFile)
    {
        var duplicate = FileUtil.FindDuplicateInFolder(movedFile, movedFile.DirectoryName);
        if (duplicate == null) { return; }
        TaskDialogButton btnOpenExisting = new TaskDialogCommandLinkButton("Vorhandene Datei öffnen", duplicate.Name);
        TaskDialogButton btnDeleteCurrent = new TaskDialogCommandLinkButton("Aktuelle Datei löschen", movedFile.Name);
        var page = new TaskDialogPage()
        {
            Icon = TaskDialogIcon.Warning,
            Caption = movedFile.DirectoryName,
            Heading = "Folgende Datei scheint identisch zu sein.",
            Text = $"{duplicate.Name} ({duplicate.Length / 1024} KB, {duplicate.LastWriteTime.ToShortDateString()})",
            AllowCancel = true,
            SizeToContent = true,
            Buttons = { btnOpenExisting, btnDeleteCurrent, TaskDialogButton.Ignore },
            DefaultButton = btnOpenExisting
        };
        var result = TaskDialog.ShowDialog(Handle, page);
        if (result == btnOpenExisting) { LoadPdf(duplicate.FullName); }
        else if (result == btnDeleteCurrent)
        {
            try
            {
                FileSystem.DeleteFile(movedFile.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                LoadPdf(duplicate.FullName); // das Duplikat bleibt übrig und wird angezeigt
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { TaskDlg.ErrTaskDlg(Handle, "Löschen fehlgeschlagen.", ex); }
        }
    }

    private void SplitButtonMove_ButtonClick(object sender, EventArgs e)
    {
        // Strg+Klick: direkt in den ersten Zielordner verschieben (wie in PDFMover)
        if ((ModifierKeys & Keys.Control) == Keys.Control && settings.TargetFolders.Count > 0 && Directory.Exists(settings.TargetFolders[0]))
        {
            MoveOrCopyTo(settings.TargetFolders[0], copy: false);
        }
        else { MoveCopyDialog(copy: false); }
    }

    private void SplitButtonMove_DropDownOpening(object sender, EventArgs e)
    {
        settings.ReloadSharedLists(); // Zielliste anderer Instanzen übernehmen
        splitButtonMove.DropDownItems.Clear();
        var targets = settings.TargetFolders.Where(f => !string.IsNullOrEmpty(f)); // Reihenfolge = Zielliste (sortierbar in den Einstellungen)
        foreach (var folder in targets)
        {
            ToolStripMenuItem item = new(folder.Replace("&", "&&")) { Enabled = Directory.Exists(folder), Tag = folder };
            item.Click += (s, args) => MoveOrCopyTo((string)((ToolStripMenuItem)s).Tag, copy: false);
            splitButtonMove.DropDownItems.Add(item);
        }
        if (splitButtonMove.DropDownItems.Count == 0)
        {
            splitButtonMove.DropDownItems.Add(new ToolStripMenuItem("(Zielliste ist leer)") { Enabled = false });
        }
        splitButtonMove.DropDownItems.Add(new ToolStripSeparator());
        ToolStripMenuItem editList = new("Zielliste bearbeiten …");
        editList.Click += (s, args) => OpenSettings(SettingsForm.TabTargets);
        splitButtonMove.DropDownItems.Add(editList);
    }

    private void OpenSettings(int tabIndex)
    {
        settings.ReloadSharedLists();
        using SettingsForm dialog = new(settings, tabIndex);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            settings.TargetFolders = dialog.TargetFolders;
            settings.ExternalPrograms = dialog.ExternalPrograms;
            settings.JumpToLastUsed = dialog.JumpToLastUsed;
            settings.ConfirmDelete = dialog.ConfirmDelete;
            settings.ShowProgramIcons = dialog.ShowProgramIcons;
            settings.ShowToolbarIcons = dialog.ShowToolbarIcons;
            settings.LargeToolbarIcons = dialog.LargeToolbarIcons;
            settings.CloseOnEscape = dialog.CloseOnEscape;
            settings.ReopenLastFile = dialog.ReopenLastFile;
            if (dialog.ClearRecentRequested) { settings.RecentFolders.Clear(); }
            settings.Save();
            RebuildProgramIconButtons();
            ApplyToolbarIcons();
        }
    }

    /// <summary>Versieht die Toolbar-Buttons mit Symbolen aus der Windows-Symbolschrift (abschaltbar in den Einstellungen).</summary>
    private void ApplyToolbarIcons()
    {
        var edge = LogicalToDeviceUnits(settings.LargeToolbarIcons ? 24 : 16); // DPI-gerecht; gilt auch für die Programm-Icons
        toolStrip.ImageScalingSize = new Size(edge, edge);
        var fontSize = settings.LargeToolbarIcons ? 10f : 9f; // größere Symbole → größere Schrift in der Symbolleiste
        if (Math.Abs(toolStrip.Font.Size - fontSize) > 0.1f) { toolStrip.Font = new Font(toolStrip.Font.FontFamily, fontSize); } // ToolStrip erbt die Form-Schrift nicht → direkt setzen
        var showIcons = settings.ShowToolbarIcons && ToolbarIcons.FontAvailable;
        var size = toolStrip.ImageScalingSize;
        void Set(ToolStripItem item, char glyph, bool imageOnly = false)
        {
            item.Image = showIcons ? ToolbarIcons.Get(glyph, size) : null;
            item.DisplayStyle = showIcons
                ? (imageOnly ? ToolStripItemDisplayStyle.Image : ToolStripItemDisplayStyle.ImageAndText)
                : ToolStripItemDisplayStyle.Text;
        }
        Set(btnOpen, ToolbarIcons.OpenFile);
        Set(btnPrev, ToolbarIcons.Previous, imageOnly: true);
        Set(btnNext, ToolbarIcons.Next, imageOnly: true);
        Set(btnPageUp, ToolbarIcons.PageUp, imageOnly: true);
        Set(btnPageDown, ToolbarIcons.PageDown, imageOnly: true);
        Set(splitButtonMove, ToolbarIcons.MoveToFolder);
        Set(btnCopy, ToolbarIcons.Copy);
        Set(btnRename, ToolbarIcons.Rename);
        Set(btnDelete, ToolbarIcons.Delete);
        Set(btnEmail, ToolbarIcons.Mail);
        Set(ddbEdit, ToolbarIcons.Edit);
        Set(ddbPrograms, ToolbarIcons.AllApps);
        Set(btnShowInFolder, ToolbarIcons.FolderOpen);
        Set(btnSettings, ToolbarIcons.Settings);
        Set(btnInfo, ToolbarIcons.Info, imageOnly: true);
        UpdateProgramIconVisibility(); // die Buttonbreiten haben sich geändert
    }

    // ------------------------------------------------------------------ Umbenennen / Löschen / Explorer

    private void RenameCurrent()
    {
        if (currentFile == null) { return; }
        using RenameForm dialog = new(currentFile);
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        var newPath = Path.Combine(dialog.NewFolder, dialog.NewName);
        if (string.Equals(newPath, currentFile.FullName, StringComparison.OrdinalIgnoreCase)) { UpdateUiState(); return; }
        try
        {
            File.Move(currentFile.FullName, newPath);
            if (!string.Equals(dialog.NewFolder, currentFile.DirectoryName, StringComparison.OrdinalIgnoreCase))
            {
                settings.AddRecentFolder(dialog.NewFolder); // Umbenennen mit Ordnerwechsel zählt wie ein Verschieben
                settings.Save();
            }
            LoadPdf(newPath); // aktualisiert Titel, Statusleiste und den im Viewer angezeigten Dateinamen
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            TaskDlg.ErrTaskDlg(Handle, "Umbenennen fehlgeschlagen.", ex);
        }
    }

    private void DeleteCurrent()
    {
        if (currentFile == null) { return; }
        if (settings.ConfirmDelete
            && !TaskDlg.ConfirmTaskDlg(Handle, "In den Papierkorb verschieben?", currentFile.Name)) { return; }
        var files = FileUtil.GetPdfFilesInFolder(currentFile.DirectoryName);
        var index = files.FindIndex(f => string.Equals(f, currentFile.FullName, StringComparison.OrdinalIgnoreCase));
        try { FileSystem.DeleteFile(currentFile.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin); }
        catch (OperationCanceledException) { return; } // im Systemdialog abgebrochen
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            TaskDlg.ErrTaskDlg(Handle, "Löschen fehlgeschlagen.", ex);
            return;
        }
        LoadNextAfterRemoval(files, index);
    }

    private void ShowInFolder()
    {
        if (currentFile == null) { return; }
        ShellUtil.ShowInFileManager(currentFile.FullName);
    }

    private bool emailInProgress;

    /// <summary>Erstellt eine neue E-Mail mit der aktuellen Datei als Anhang.
    /// Primär über das Shell-SendMail-DropTarget (respektiert die .mapimail-Zuordnung, z.B. eM Client);
    /// schlägt das fehl, als Fallback über Simple MAPI auf einem Hintergrund-Thread.</summary>
    private async void EmailCurrent()
    {
        if (currentFile == null || emailInProgress) { return; }
        emailInProgress = true;
        var path = currentFile.FullName;
        var subject = Path.GetFileNameWithoutExtension(currentFile.Name);
        try
        {
            try
            {
                MailSender.SendViaDropTarget(path); // muss auf dem UI-Thread (STA) laufen
            }
            catch (Exception ex) when (MailSender.IsComFailure(ex))
            {
                statusPath.Text = "E-Mail wird erstellt (MAPI) …";
                var error = await Task.Run(() => MapiMailer.SendWithAttachment(path, subject));
                if (error != null)
                {
                    TaskDlg.MsgTaskDlg(Handle, "Die E-Mail konnte nicht erstellt werden.", error, TaskDialogIcon.Warning);
                }
            }
        }
        finally
        {
            emailInProgress = false;
            if (currentFile != null) { statusPath.Text = currentFile.FullName; }
        }
    }

    /// <summary>Blättert im Dokument (der Chromium-Viewer reagiert auf Bild↑/Bild↓; eigene Buttons hat seine Leiste nicht).</summary>
    private void SendPageKey(string key)
    {
        if (currentFile == null) { return; }
        webView.Focus();
        SendKeys.Send(key);
    }

    // ------------------------------------------------------------------ Seitenoperationen (PDFsharp)

    /// <summary>Führt eine Dokumentänderung mit vorheriger Undo-Sicherung aus; false bei Fehler.</summary>
    private bool RunPdfEdit(Action edit, string actionName)
    {
        Cursor.Current = Cursors.WaitCursor;
        try
        {
            BackupForUndo();
            edit();
            return true;
        }
        catch (Exception ex) when (PdfEditService.IsPdfReadError(ex))
        {
            TaskDlg.ErrTaskDlg(Handle, actionName + " fehlgeschlagen.", ex);
            return false;
        }
        finally { Cursor.Current = Cursors.Default; }
    }

    private void ShowNotEditableMessage()
    {
        TaskDlg.MsgTaskDlg(Handle, "Die Datei kann nicht bearbeitet werden.", "Möglicherweise ist sie verschlüsselt oder beschädigt.", TaskDialogIcon.Warning);
    }

    private void DeletePagesDialog()
    {
        if (currentFile == null) { return; }
        if (currentPageCount <= 0) { ShowNotEditableMessage(); return; }
        if (currentPageCount == 1)
        {
            TaskDlg.MsgTaskDlg(Handle, "Die Datei hat nur eine Seite.", "Zum Löschen der ganzen Datei benutzen Sie den Papierkorb (Entf).", TaskDialogIcon.Information);
            return;
        }
        using PageRangeForm dialog = new("Seiten löschen", currentPageCount, emptyMeansAll: false, showRotation: false);
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        var pages = dialog.SelectedPages;
        if (pages.Count >= currentPageCount)
        {
            TaskDlg.MsgTaskDlg(Handle, "Mindestens eine Seite muss erhalten bleiben.", null, TaskDialogIcon.Warning);
            return;
        }
        var remaining = currentPageCount - pages.Count;
        if (RunPdfEdit(() => PdfEditService.DeletePages(currentFile.FullName, pages), "Seiten löschen"))
        {
            LoadPdf(currentFile.FullName, Math.Min(pages[0], remaining));
            statusPath.Text = pages.Count == 1 ? $"Seite {pages[0]} wurde gelöscht." : $"{pages.Count} Seiten wurden gelöscht.";
        }
    }

    private void RotatePagesDialog()
    {
        if (currentFile == null) { return; }
        if (currentPageCount <= 0) { ShowNotEditableMessage(); return; }
        using PageRangeForm dialog = new("Seiten drehen", currentPageCount, emptyMeansAll: true, showRotation: true);
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        var pages = dialog.SelectedPages;
        if (RunPdfEdit(() => PdfEditService.RotatePages(currentFile.FullName, pages, dialog.RotationDelta), "Seiten drehen"))
        {
            LoadPdf(currentFile.FullName, pages.Count == currentPageCount ? 0 : pages[0]);
            statusPath.Text = pages.Count == currentPageCount ? "Alle Seiten wurden gedreht." : (pages.Count == 1 ? $"Seite {pages[0]} wurde gedreht." : $"{pages.Count} Seiten wurden gedreht.");
        }
    }

    private void AppendPdfDialog()
    {
        if (currentFile == null) { return; }
        if (currentPageCount <= 0) { ShowNotEditableMessage(); return; }
        using OpenFileDialog dialog = new() { Filter = "PDF-Dateien (*.pdf)|*.pdf", Title = "PDF-Datei anhängen", InitialDirectory = currentFile.DirectoryName };
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        if (string.Equals(dialog.FileName, currentFile.FullName, StringComparison.OrdinalIgnoreCase))
        {
            TaskDlg.MsgTaskDlg(Handle, "Die Datei kann nicht an sich selbst angehängt werden.", null, TaskDialogIcon.Warning);
            return;
        }
        var firstNewPage = currentPageCount + 1;
        if (RunPdfEdit(() => PdfEditService.AppendPdf(currentFile.FullName, dialog.FileName), "Anhängen"))
        {
            LoadPdf(currentFile.FullName, firstNewPage);
            statusPath.Text = $"\"{Path.GetFileName(dialog.FileName)}\" wurde angehängt.";
        }
    }

    private void ExtractPagesDialog()
    {
        if (currentFile == null) { return; }
        if (currentPageCount <= 0) { ShowNotEditableMessage(); return; }
        using PageRangeForm rangeDialog = new("Seiten extrahieren", currentPageCount, emptyMeansAll: false, showRotation: false);
        if (rangeDialog.ShowDialog(this) != DialogResult.OK) { return; }
        using SaveFileDialog saveDialog = new()
        {
            Filter = "PDF-Dateien (*.pdf)|*.pdf",
            Title = "Auszug speichern",
            InitialDirectory = currentFile.DirectoryName,
            FileName = Path.GetFileNameWithoutExtension(currentFile.Name) + " – Auszug.pdf",
        };
        if (saveDialog.ShowDialog(this) != DialogResult.OK) { return; }
        if (string.Equals(saveDialog.FileName, currentFile.FullName, StringComparison.OrdinalIgnoreCase))
        {
            TaskDlg.MsgTaskDlg(Handle, "Der Auszug kann die geöffnete Datei nicht überschreiben.", null, TaskDialogIcon.Warning);
            return;
        }
        try
        {
            Cursor.Current = Cursors.WaitCursor;
            PdfEditService.ExtractPages(currentFile.FullName, saveDialog.FileName, rangeDialog.SelectedPages);
            statusPath.Text = "Auszug gespeichert: " + saveDialog.FileName;
        }
        catch (Exception ex) when (PdfEditService.IsPdfReadError(ex))
        {
            TaskDlg.ErrTaskDlg(Handle, "Extrahieren fehlgeschlagen.", ex);
        }
        finally { Cursor.Current = Cursors.Default; }
    }

    private void ShowProperties()
    {
        if (currentFile == null) { return; }
        PdfInfo info;
        try { info = PdfEditService.ReadInfo(currentFile.FullName); }
        catch (Exception ex) when (PdfEditService.IsPdfReadError(ex)) { ShowNotEditableMessage(); return; }
        currentFile.Refresh();
        using PropertiesForm dialog = new(info, currentFile);
        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.InfoChanged)
        {
            if (RunPdfEdit(() => PdfEditService.WriteInfo(currentFile.FullName, dialog.DocTitle, dialog.DocAuthor, dialog.DocSubject, dialog.DocKeywords), "Speichern der Eigenschaften"))
            {
                LoadPdf(currentFile.FullName);
                statusPath.Text = "Die Dokumenteigenschaften wurden gespeichert.";
            }
        }
    }

    private void BackupForUndo()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PDFlight");
        Directory.CreateDirectory(folder);
        undoBackupFile = Path.Combine(folder, "undo.pdf");
        File.Copy(currentFile.FullName, undoBackupFile, true);
        undoTargetFile = currentFile.FullName;
        mnuUndo.Enabled = true;
    }

    private void UndoLastChange()
    {
        if (undoTargetFile == null || !File.Exists(undoBackupFile)) { return; }
        try { File.Copy(undoBackupFile, undoTargetFile, true); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            TaskDlg.ErrTaskDlg(Handle, "Rückgängig fehlgeschlagen.", ex);
            return;
        }
        var file = undoTargetFile;
        undoTargetFile = null;
        mnuUndo.Enabled = false;
        LoadPdf(file);
        statusPath.Text = "Die letzte Änderung wurde rückgängig gemacht.";
    }

    // ------------------------------------------------------------------ Externe Programme

    private void EnsureProgramList()
    {
        settings.ExternalPrograms.RemoveAll(p => string.IsNullOrEmpty(p) || !File.Exists(p));
        if (settings.ExternalPrograms.Count == 0)
        {
            settings.ExternalPrograms = ProgramFinder.DetectPrograms();
            settings.Save();
        }
    }

    private void DdbPrograms_DropDownOpening(object sender, EventArgs e)
    {
        settings.ReloadSharedLists();
        ddbPrograms.DropDownItems.Clear();
        var number = 1;
        foreach (var exe in settings.ExternalPrograms.Take(ProgramFinder.MaxPrograms))
        {
            ToolStripMenuItem item = new(ProgramFinder.GetDisplayName(exe))
            {
                Tag = exe,
                Image = GetProgramIcon(exe),
                ShortcutKeyDisplayString = "Strg+" + number,
                ToolTipText = exe,
                Enabled = currentFile != null,
            };
            item.Click += (s, args) => LaunchExternalProgram((string)((ToolStripMenuItem)s).Tag);
            ddbPrograms.DropDownItems.Add(item);
            number++;
        }
        if (ddbPrograms.DropDownItems.Count == 0)
        {
            ddbPrograms.DropDownItems.Add(new ToolStripMenuItem("(keine Programme gefunden)") { Enabled = false });
        }
        ddbPrograms.DropDownItems.Add(new ToolStripSeparator());
        ToolStripMenuItem openWith = new("Öffnen mit …") { Enabled = currentFile != null };
        openWith.Click += (s, args) => OpenWithDialog();
        ddbPrograms.DropDownItems.Add(openWith);
        ddbPrograms.DropDownItems.Add(new ToolStripSeparator());
        ToolStripMenuItem managePrograms = new("Programme verwalten …");
        managePrograms.Click += (s, args) => OpenSettings(SettingsForm.TabPrograms);
        ddbPrograms.DropDownItems.Add(managePrograms);
    }

    private void LaunchExternalProgram(string exePath)
    {
        if (currentFile == null || !File.Exists(exePath)) { return; }
        try { Process.Start(new ProcessStartInfo(exePath, $"\"{currentFile.FullName}\"") { UseShellExecute = false }); }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or FileNotFoundException)
        {
            TaskDlg.ErrTaskDlg(Handle, "Das Programm konnte nicht gestartet werden.", ex);
        }
    }

    private void LaunchProgramByIndex(int index)
    {
        if (index < settings.ExternalPrograms.Count) { LaunchExternalProgram(settings.ExternalPrograms[index]); }
    }

    private readonly List<ToolStripButton> programIconButtons = [];

    /// <summary>Zeigt die Symbole der externen Programme zusätzlich als Toolbar-Buttons.
    /// Dank Overflow=Never verschwinden sie automatisch, wenn die Fensterbreite nicht reicht.</summary>
    private void RebuildProgramIconButtons()
    {
        foreach (var button in programIconButtons)
        {
            toolStrip.Items.Remove(button);
            button.Dispose();
        }
        programIconButtons.Clear();
        if (!settings.ShowProgramIcons) { return; }

        var insertIndex = toolStrip.Items.IndexOf(toolStripSeparator5); // direkt hinter dem Programme-Menü
        var number = 1;
        foreach (var exe in settings.ExternalPrograms.Take(ProgramFinder.MaxPrograms))
        {
            var icon = GetProgramIcon(exe);
            if (icon != null)
            {
                ToolStripButton button = new()
                {
                    DisplayStyle = ToolStripItemDisplayStyle.Image,
                    Image = icon,
                    Overflow = ToolStripItemOverflow.Never, // bei schmalem Fenster ausblenden statt ins Überlaufmenü
                    Tag = exe,
                    ToolTipText = ProgramFinder.GetDisplayName(exe) + " (Strg+" + number + ")",
                    Enabled = currentFile != null,
                };
                button.Click += (s, e) => LaunchExternalProgram((string)((ToolStripItem)s).Tag);
                toolStrip.Items.Insert(insertIndex++, button);
                programIconButtons.Add(button);
            }
            number++;
        }
        UpdateProgramIconVisibility();
    }

    /// <summary>Blendet die Programm-Icons aus, sobald sie nicht mehr neben alle übrigen Buttons passen —
    /// die Kernfunktionen haben Vorrang und sollen nicht ins Überlaufmenü rutschen.</summary>
    private void UpdateProgramIconVisibility()
    {
        if (programIconButtons.Count == 0) { return; }
        var requiredWidth = 0;
        foreach (ToolStripItem item in toolStrip.Items)
        {
            if (item is not ToolStripButton button || !programIconButtons.Contains(button))
            {
                requiredWidth += item.GetPreferredSize(Size.Empty).Width + item.Margin.Horizontal; // unabhängig von der aktuellen Sichtbarkeit
            }
        }
        var iconsWidth = programIconButtons.Sum(b => b.GetPreferredSize(Size.Empty).Width + b.Margin.Horizontal);
        var fits = requiredWidth + iconsWidth + 8 <= toolStrip.DisplayRectangle.Width;
        foreach (var button in programIconButtons) { button.Visible = fits; }
    }

    private void OpenWithDialog()
    {
        if (currentFile == null) { return; }
        // Windows-Dialog "Öffnen mit" (Pfad hier bewusst ohne Anführungszeichen — OpenAs_RunDLL erwartet das so)
        Process.Start(new ProcessStartInfo("rundll32.exe", "shell32.dll,OpenAs_RunDLL " + currentFile.FullName) { UseShellExecute = false });
    }

    private Image GetProgramIcon(string exePath)
    {
        if (!programIcons.TryGetValue(exePath, out var image))
        {
            try
            {
                using var icon = Icon.ExtractAssociatedIcon(exePath);
                image = icon?.ToBitmap();
            }
            catch (Exception ex) when (ex is ArgumentException or IOException) { image = null; }
            programIcons[exePath] = image;
        }
        return image;
    }

    // ------------------------------------------------------------------ Tastenkürzel

    private bool HandleShortcut(Keys keyData)
    {
        switch (keyData)
        {
            case Keys.O | Keys.Control: OpenFile(); return true;
            case Keys.M | Keys.Control: MoveCopyDialog(copy: false); return true;
            case Keys.K | Keys.Control: MoveCopyDialog(copy: true); return true;
            case Keys.F2: RenameCurrent(); return true;
            case Keys.Delete | Keys.Control: DeletePagesDialog(); return true;
            case Keys.Delete when currentFile != null: DeleteCurrent(); return true;
            case Keys.R | Keys.Control: RotatePagesDialog(); return true;
            case Keys.Z | Keys.Control when undoTargetFile != null: UndoLastChange(); return true;
            case Keys.I | Keys.Control: ShowProperties(); return true;
            case Keys.E | Keys.Control: EmailCurrent(); return true;
            case Keys.Right | Keys.Alt: StepFile(1); return true;   // Strg+Pfeile/±/0 gehören dem Viewer (Zoom & Co.)
            case Keys.Left | Keys.Alt: StepFile(-1); return true;
            case Keys.F1: TaskDlg.AboutTaskDlg(Handle, Icon); return true;
            case Keys.F11: SetFullScreen(!isFullScreen); return true;
            case Keys.Escape when isFullScreen: SetFullScreen(false); return true;
            case Keys.Escape when settings.CloseOnEscape: Close(); return true; // wie in PDFMover (Option)
        }
        if ((keyData & (Keys.Control | Keys.Alt | Keys.Shift)) == Keys.Control)
        {
            var key = keyData & Keys.KeyCode;
            if (key is >= Keys.D1 and <= Keys.D9) { LaunchProgramByIndex(key - Keys.D1); return true; } // Strg+1 … Strg+9: externe Programme
        }
        return false;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData) { return HandleShortcut(keyData) || base.ProcessCmdKey(ref msg, keyData); }

    private void WebView_KeyDown(object sender, KeyEventArgs e) { if (HandleShortcut(e.KeyData)) { e.Handled = true; } }

    // ------------------------------------------------------------------ Toolbar-Klicks

    private void BtnOpen_Click(object sender, EventArgs e) { OpenFile(); }
    private void BtnPrev_Click(object sender, EventArgs e) { StepFile(-1); }
    private void BtnNext_Click(object sender, EventArgs e) { StepFile(1); }
    private void BtnCopy_Click(object sender, EventArgs e) { MoveCopyDialog(copy: true); }
    private void BtnRename_Click(object sender, EventArgs e) { RenameCurrent(); }
    private void BtnDelete_Click(object sender, EventArgs e) { DeleteCurrent(); }
    private void BtnShowInFolder_Click(object sender, EventArgs e) { ShowInFolder(); }
    private void BtnSettings_Click(object sender, EventArgs e) { OpenSettings(SettingsForm.TabGeneral); }
    private void BtnInfo_Click(object sender, EventArgs e) { TaskDlg.AboutTaskDlg(Handle, Icon); }
    private void BtnPageUp_Click(object sender, EventArgs e) { SendPageKey("{PGUP}"); }
    private void BtnPageDown_Click(object sender, EventArgs e) { SendPageKey("{PGDN}"); }
    private void BtnEmail_Click(object sender, EventArgs e) { EmailCurrent(); }
    private void MnuDeletePages_Click(object sender, EventArgs e) { DeletePagesDialog(); }
    private void MnuRotatePages_Click(object sender, EventArgs e) { RotatePagesDialog(); }
    private void MnuAppendPdf_Click(object sender, EventArgs e) { AppendPdfDialog(); }
    private void MnuExtractPages_Click(object sender, EventArgs e) { ExtractPagesDialog(); }
    private void MnuUndo_Click(object sender, EventArgs e) { UndoLastChange(); }
    private void MnuProperties_Click(object sender, EventArgs e) { ShowProperties(); }
}
