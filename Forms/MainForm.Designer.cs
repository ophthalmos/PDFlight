namespace PDFLight.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            splashTimer = new System.Windows.Forms.Timer(components);
            escHoldTimer = new System.Windows.Forms.Timer(components);
            searchTimer = new System.Windows.Forms.Timer(components);
            toolStrip = new ToolStrip();
            btnOpen = new ToolStripSplitButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnPrev = new ToolStripButton();
            btnNext = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            splitButtonMove = new ToolStripSplitButton();
            btnCopy = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            btnRename = new ToolStripButton();
            btnDelete = new ToolStripButton();
            btnShowInFolder = new ToolStripButton();
            toolStripSeparator8 = new ToolStripSeparator();
            btnPrint = new ToolStripButton();
            btnEmail = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            ddbEdit = new ToolStripDropDownButton();
            mnuDeletePages = new ToolStripMenuItem();
            mnuRotatePages = new ToolStripMenuItem();
            mnuMovePage = new ToolStripMenuItem();
            mnuInsertPage = new ToolStripMenuItem();
            toolStripSeparator15 = new ToolStripSeparator();
            mnuAppendPdf = new ToolStripMenuItem();
            mnuDuplex = new ToolStripMenuItem();
            mnuExtractPages = new ToolStripMenuItem();
            toolStripSeparator14 = new ToolStripSeparator();
            mnuManageStamps = new ToolStripMenuItem();
            btnManageAnnotations = new ToolStripButton();
            toolStripSeparator16 = new ToolStripSeparator();
            mnuRemoveBookmarks = new ToolStripMenuItem();
            mnuEditBookmarks = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            mnuSetPassword = new ToolStripMenuItem();
            mnuRemovePassword = new ToolStripMenuItem();
            mnuRemoveRestrictions = new ToolStripMenuItem();
            toolStripSeparator10 = new ToolStripSeparator();
            mnuProperties = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            ddbFavorites = new ToolStripDropDownButton();
            mnuFavoriteAdd = new ToolStripMenuItem();
            mnuFavoriteRemove = new ToolStripMenuItem();
            mnuFavoriteCleanup = new ToolStripMenuItem();
            toolStripSeparator13 = new ToolStripSeparator();
            toolStripSeparator12 = new ToolStripSeparator();
            ddbPrograms = new ToolStripDropDownButton();
            toolStripSeparator5 = new ToolStripSeparator();
            ddbInfo = new ToolStripDropDownButton();
            mnuShortcuts = new ToolStripMenuItem();
            mnuCheckUpdate = new ToolStripMenuItem();
            toolStripSeparator11 = new ToolStripSeparator();
            mnuAbout = new ToolStripMenuItem();
            btnSettings = new ToolStripButton();
            statusStrip = new StatusStrip();
            statusIndex = new ToolStripStatusLabel();
            statusPath = new ToolStripStatusLabel();
            statusOneClick = new ToolStripStatusLabel();
            statusFormat = new ToolStripStatusLabel();
            statusZoom = new ToolStripStatusLabel();
            statusInfo = new ToolStripStatusLabel();
            pnlPdfA = new Panel();
            btnPdfAEnable = new Button();
            lblPdfA = new Label();
            viewerStrip = new ToolStrip();
            btnSidebar = new ToolStripButton();
            toolStripSeparatorV1 = new ToolStripSeparator();
            btnUndo = new ToolStripButton();
            btnHighlight = new ToolStripButton();
            ddbHighlight = new ToolStripDropDownButton();
            btnDraw = new ToolStripButton();
            ddbInk = new ToolStripDropDownButton();
            btnErase = new ToolStripButton();
            btnFreeText = new ToolStripButton();
            btnStamp = new ToolStripButton();
            labelViewerSpacer = new ToolStripLabel();
            textPage = new ToolStripTextBox();
            labelPageCount = new ToolStripLabel();
            toolStripSeparatorV2 = new ToolStripSeparator();
            btnZoomOut = new ToolStripButton();
            comboZoom = new ToolStripComboBox();
            btnZoomIn = new ToolStripButton();
            btnFitWidth = new ToolStripButton();
            btnTwoPage = new ToolStripButton();
            toolStripSeparatorV3 = new ToolStripSeparator();
            btnRotateLeft = new ToolStripButton();
            btnRotateRight = new ToolStripButton();
            btnCloseDocument = new ToolStripButton();
            btnFullScreen = new ToolStripButton();
            toolStripSeparatorV4 = new ToolStripSeparator();
            toolStripSeparatorV5 = new ToolStripSeparator();
            toolStripSeparatorV6 = new ToolStripSeparator();
            btnSearch = new ToolStripButton();
            btnSearchClose = new ToolStripButton();
            btnSearchNext = new ToolStripButton();
            btnSearchPrev = new ToolStripButton();
            labelMatches = new ToolStripLabel();
            btnWholeWord = new ToolStripButton();
            btnMatchCase = new ToolStripButton();
            textSearch = new ToolStripTextBox();
            btnSaveDocument = new ToolStripButton();
            splitViewer = new SplitContainer();
            panelSidebarContent = new Panel();
            thumbnailGrid = new PDFLight.Viewer.ThumbnailGrid();
            outlineView = new PDFLight.Viewer.OutlineView();
            sidebarStrip = new ToolStrip();
            btnSideThumbnails = new ToolStripButton();
            btnSideBookmarks = new ToolStripButton();
            sidebarHeader = new ToolStrip();
            labelSidebarTitle = new ToolStripLabel();
            btnSidebarClose = new ToolStripButton();
            pageView = new PDFLight.Viewer.PageView();
            contextMenuPage = new ContextMenuStrip(components);
            mnuViewCopy = new ToolStripMenuItem();
            mnuViewSelectAll = new ToolStripMenuItem();
            toolStripSeparatorC1 = new ToolStripSeparator();
            mnuAddFreeTextHere = new ToolStripMenuItem();
            mnuHighlight = new ToolStripMenuItem();
            mnuEditAnnotation = new ToolStripMenuItem();
            mnuRemoveAnnotation = new ToolStripMenuItem();
            printDocument = new System.Drawing.Printing.PrintDocument();
            printDialog = new PrintDialog();
            toolTipLink = new ToolTip(components);
            toolStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            pnlPdfA.SuspendLayout();
            viewerStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitViewer).BeginInit();
            splitViewer.Panel1.SuspendLayout();
            splitViewer.Panel2.SuspendLayout();
            splitViewer.SuspendLayout();
            panelSidebarContent.SuspendLayout();
            sidebarStrip.SuspendLayout();
            sidebarHeader.SuspendLayout();
            contextMenuPage.SuspendLayout();
            SuspendLayout();
            // 
            // splashTimer
            // 
            splashTimer.Interval = 1000;
            splashTimer.Tick += SplashTimer_Tick;
            // 
            // escHoldTimer
            // 
            escHoldTimer.Tick += EscHoldTimer_Tick;
            // 
            // searchTimer
            // 
            searchTimer.Interval = 300;
            searchTimer.Tick += SearchTimer_Tick;
            // 
            // toolStrip
            // 
            toolStrip.AllowDrop = true;
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { btnOpen, toolStripSeparator1, btnPrev, btnNext, toolStripSeparator2, splitButtonMove, btnCopy, toolStripSeparator3, btnRename, btnDelete, btnShowInFolder, toolStripSeparator8, btnPrint, btnEmail, toolStripSeparator4, ddbEdit, toolStripSeparator9, ddbFavorites, toolStripSeparator12, ddbPrograms, toolStripSeparator5, ddbInfo, btnSettings });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(984, 25);
            toolStrip.TabIndex = 1;
            toolStrip.DragDrop += HandleDragDrop;
            toolStrip.DragEnter += HandleDragEnter;
            // 
            // btnOpen
            // 
            btnOpen.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(60, 22);
            btnOpen.Text = "Öffnen";
            btnOpen.ToolTipText = "PDF-Datei öffnen (Strg+O)\r\nPfeil: zuletzt geöffnete Dateien";
            btnOpen.ButtonClick += BtnOpen_Click;
            btnOpen.DropDownOpening += BtnOpen_DropDownOpening;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // btnPrev
            // 
            btnPrev.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnPrev.Enabled = false;
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new Size(23, 22);
            btnPrev.Text = "◀";
            btnPrev.ToolTipText = "Vorherige PDF-Datei im Ordner (Strg+Umschalt+←)";
            btnPrev.Click += BtnPrev_Click;
            // 
            // btnNext
            // 
            btnNext.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnNext.Enabled = false;
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(23, 22);
            btnNext.Text = "▶";
            btnNext.ToolTipText = "Nächste PDF-Datei im Ordner (Strg+Umschalt+→)";
            btnNext.Click += BtnNext_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // splitButtonMove
            // 
            splitButtonMove.DisplayStyle = ToolStripItemDisplayStyle.Text;
            splitButtonMove.Enabled = false;
            splitButtonMove.Name = "splitButtonMove";
            splitButtonMove.Size = new Size(86, 22);
            splitButtonMove.Text = "Verschieben";
            splitButtonMove.ToolTipText = "In einen Ordner verschieben (Strg+M)\r\nStrg+Klick: direkt in den 1-Klick-Ordner (siehe Statusleiste)\r\nPfeil: Zielliste";
            splitButtonMove.ButtonClick += SplitButtonMove_ButtonClick;
            splitButtonMove.DropDownOpening += SplitButtonMove_DropDownOpening;
            // 
            // btnCopy
            // 
            btnCopy.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnCopy.Enabled = false;
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new Size(58, 22);
            btnCopy.Text = "Kopieren";
            btnCopy.ToolTipText = "In einen Ordner kopieren (Strg+K)\r\nStrg+Klick: direkt in den 1-Klick-Ordner (siehe Statusleiste)";
            btnCopy.Click += BtnCopy_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // btnRename
            // 
            btnRename.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnRename.Enabled = false;
            btnRename.Name = "btnRename";
            btnRename.Size = new Size(83, 22);
            btnRename.Text = "Umbenennen";
            btnRename.ToolTipText = "Datei umbenennen (F2)";
            btnRename.Click += BtnRename_Click;
            // 
            // btnDelete
            // 
            btnDelete.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnDelete.Enabled = false;
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(55, 22);
            btnDelete.Text = "Löschen";
            btnDelete.ToolTipText = "In den Papierkorb verschieben (Strg+Umschalt+Entf)";
            btnDelete.Click += BtnDelete_Click;
            // 
            // btnShowInFolder
            // 
            btnShowInFolder.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnShowInFolder.Enabled = false;
            btnShowInFolder.Name = "btnShowInFolder";
            btnShowInFolder.Size = new Size(86, 22);
            btnShowInFolder.Text = "Ordner öffnen";
            btnShowInFolder.ToolTipText = "Datei im Dateimanager anzeigen";
            btnShowInFolder.Click += BtnShowInFolder_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(6, 25);
            // 
            // btnPrint
            // 
            btnPrint.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnPrint.Enabled = false;
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(55, 22);
            btnPrint.Text = "Drucken";
            btnPrint.ToolTipText = "Datei drucken (Strg+P)";
            btnPrint.Click += BtnPrint_Click;
            // 
            // btnEmail
            // 
            btnEmail.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnEmail.Enabled = false;
            btnEmail.Name = "btnEmail";
            btnEmail.Size = new Size(45, 22);
            btnEmail.Text = "E-Mail";
            btnEmail.ToolTipText = "Neue E-Mail mit dieser Datei als Anhang (Strg+E)";
            btnEmail.Click += BtnEmail_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 25);
            // 
            // ddbEdit
            // 
            ddbEdit.DisplayStyle = ToolStripItemDisplayStyle.Text;
            ddbEdit.DropDownItems.AddRange(new ToolStripItem[] { mnuDeletePages, mnuRotatePages, mnuMovePage, mnuInsertPage, toolStripSeparator15, mnuAppendPdf, mnuDuplex, mnuExtractPages, toolStripSeparator14, mnuManageStamps, toolStripSeparator16, mnuRemoveBookmarks, mnuEditBookmarks, toolStripSeparator6, mnuSetPassword, mnuRemovePassword, mnuRemoveRestrictions, toolStripSeparator10, mnuProperties });
            ddbEdit.Enabled = false;
            ddbEdit.Name = "ddbEdit";
            ddbEdit.Size = new Size(76, 22);
            ddbEdit.Text = "Bearbeiten";
            ddbEdit.ToolTipText = "Dokument bearbeiten: Seiten löschen/drehen, PDF anhängen,\r\nSeiten extrahieren, Eigenschaften (Tastenkürzel im Menü)";
            // 
            // mnuDeletePages
            // 
            mnuDeletePages.Name = "mnuDeletePages";
            mnuDeletePages.ShortcutKeyDisplayString = "Strg+Entf";
            mnuDeletePages.Size = new Size(314, 22);
            mnuDeletePages.Text = "Seiten löschen…";
            mnuDeletePages.Click += MnuDeletePages_Click;
            // 
            // mnuRotatePages
            // 
            mnuRotatePages.Name = "mnuRotatePages";
            mnuRotatePages.ShortcutKeyDisplayString = "Strg+R";
            mnuRotatePages.Size = new Size(314, 22);
            mnuRotatePages.Text = "Seiten drehen…";
            mnuRotatePages.Click += MnuRotatePages_Click;
            // 
            // mnuMovePage
            // 
            mnuMovePage.Name = "mnuMovePage";
            mnuMovePage.ShortcutKeyDisplayString = "Strg+Y";
            mnuMovePage.Size = new Size(314, 22);
            mnuMovePage.Text = "Aktuelle Seite verschieben…";
            mnuMovePage.Click += MnuMovePage_Click;
            // 
            // mnuInsertPage
            // 
            mnuInsertPage.Name = "mnuInsertPage";
            mnuInsertPage.ShortcutKeyDisplayString = "Strg+Einfg";
            mnuInsertPage.Size = new Size(314, 22);
            mnuInsertPage.Text = "Leere Seite einfügen…";
            mnuInsertPage.Click += MnuInsertPage_Click;
            // 
            // toolStripSeparator15
            // 
            toolStripSeparator15.Name = "toolStripSeparator15";
            toolStripSeparator15.Size = new Size(311, 6);
            // 
            // mnuAppendPdf
            // 
            mnuAppendPdf.Name = "mnuAppendPdf";
            mnuAppendPdf.ShortcutKeyDisplayString = "Strg+N";
            mnuAppendPdf.Size = new Size(314, 22);
            mnuAppendPdf.Text = "PDF-Datei anhängen…";
            mnuAppendPdf.Click += MnuAppendPdf_Click;
            // 
            // mnuDuplex
            // 
            mnuDuplex.Name = "mnuDuplex";
            mnuDuplex.Size = new Size(314, 22);
            mnuDuplex.Text = "Rückseiten-Scan einfügen…";
            mnuDuplex.Click += MnuDuplex_Click;
            // 
            // mnuExtractPages
            // 
            mnuExtractPages.Name = "mnuExtractPages";
            mnuExtractPages.ShortcutKeyDisplayString = "Strg+X";
            mnuExtractPages.Size = new Size(314, 22);
            mnuExtractPages.Text = "Seiten extrahieren…";
            mnuExtractPages.Click += MnuExtractPages_Click;
            // 
            // toolStripSeparator14
            // 
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new Size(311, 6);
            // 
            // mnuManageStamps
            // 
            mnuManageStamps.Name = "mnuManageStamps";
            mnuManageStamps.ShortcutKeyDisplayString = "Strg+Umschalt+H";
            mnuManageStamps.Size = new Size(314, 22);
            mnuManageStamps.Text = "Stempel verwalten…";
            mnuManageStamps.Click += MnuManageStamps_Click;
            // 
            // toolStripSeparator16
            // 
            toolStripSeparator16.Name = "toolStripSeparator16";
            toolStripSeparator16.Size = new Size(311, 6);
            // 
            // mnuRemoveBookmarks
            // 
            mnuRemoveBookmarks.Name = "mnuRemoveBookmarks";
            mnuRemoveBookmarks.Size = new Size(314, 22);
            mnuRemoveBookmarks.Text = "Lesezeichen entfernen…";
            mnuRemoveBookmarks.Click += MnuRemoveBookmarks_Click;
            // 
            // mnuEditBookmarks
            // 
            mnuEditBookmarks.Name = "mnuEditBookmarks";
            mnuEditBookmarks.ShortcutKeyDisplayString = "Strg+F2";
            mnuEditBookmarks.Size = new Size(314, 22);
            mnuEditBookmarks.Text = "Lesezeichen bearbeiten…";
            mnuEditBookmarks.Click += MnuEditBookmarks_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(311, 6);
            // 
            // mnuSetPassword
            // 
            mnuSetPassword.Name = "mnuSetPassword";
            mnuSetPassword.Size = new Size(314, 22);
            mnuSetPassword.Text = "Kennwort vergeben…";
            mnuSetPassword.Click += MnuSetPassword_Click;
            // 
            // mnuRemovePassword
            // 
            mnuRemovePassword.Name = "mnuRemovePassword";
            mnuRemovePassword.Size = new Size(314, 22);
            mnuRemovePassword.Text = "Kennwort entfernen…";
            mnuRemovePassword.Click += MnuRemovePassword_Click;
            // 
            // mnuRemoveRestrictions
            // 
            mnuRemoveRestrictions.Name = "mnuRemoveRestrictions";
            mnuRemoveRestrictions.Size = new Size(314, 22);
            mnuRemoveRestrictions.Text = "Einschränkungen entfernen…";
            mnuRemoveRestrictions.Click += MnuRemoveRestrictions_Click;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(311, 6);
            // 
            // mnuProperties
            // 
            mnuProperties.Name = "mnuProperties";
            mnuProperties.ShortcutKeyDisplayString = "Strg+I";
            mnuProperties.Size = new Size(314, 22);
            mnuProperties.Text = "Eigenschaften…";
            mnuProperties.Click += MnuProperties_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(6, 25);
            // 
            // ddbFavorites
            // 
            ddbFavorites.DisplayStyle = ToolStripItemDisplayStyle.Text;
            ddbFavorites.DropDownItems.AddRange(new ToolStripItem[] { mnuFavoriteAdd, mnuFavoriteRemove, mnuFavoriteCleanup, toolStripSeparator13 });
            ddbFavorites.Name = "ddbFavorites";
            ddbFavorites.Size = new Size(69, 22);
            ddbFavorites.Text = "Favoriten";
            ddbFavorites.ToolTipText = "Dateien als Favoriten merken und wiederfinden (Strg+D)";
            ddbFavorites.Visible = false;
            ddbFavorites.DropDownOpening += DdbFavorites_DropDownOpening;
            // 
            // mnuFavoriteAdd
            // 
            mnuFavoriteAdd.Name = "mnuFavoriteAdd";
            mnuFavoriteAdd.ShortcutKeyDisplayString = "Strg+D";
            mnuFavoriteAdd.Size = new Size(369, 22);
            mnuFavoriteAdd.Text = "Angezeigte Datei zu den Favoriten hinzufügen…";
            mnuFavoriteAdd.Click += MnuFavoriteAdd_Click;
            // 
            // mnuFavoriteRemove
            // 
            mnuFavoriteRemove.Name = "mnuFavoriteRemove";
            mnuFavoriteRemove.ShortcutKeyDisplayString = "Strg+D";
            mnuFavoriteRemove.Size = new Size(369, 22);
            mnuFavoriteRemove.Text = "Datei aus den Favoriten entfernen";
            mnuFavoriteRemove.Visible = false;
            mnuFavoriteRemove.Click += MnuFavoriteRemove_Click;
            // 
            // mnuFavoriteCleanup
            // 
            mnuFavoriteCleanup.Name = "mnuFavoriteCleanup";
            mnuFavoriteCleanup.Size = new Size(369, 22);
            mnuFavoriteCleanup.Text = "Nicht mehr vorhandene Favoriten entfernen";
            mnuFavoriteCleanup.Visible = false;
            mnuFavoriteCleanup.Click += MnuFavoriteCleanup_Click;
            // 
            // toolStripSeparator13
            // 
            toolStripSeparator13.Name = "toolStripSeparator13";
            toolStripSeparator13.Size = new Size(366, 6);
            // 
            // toolStripSeparator12
            // 
            toolStripSeparator12.Name = "toolStripSeparator12";
            toolStripSeparator12.Size = new Size(6, 25);
            toolStripSeparator12.Visible = false;
            // 
            // ddbPrograms
            // 
            ddbPrograms.DisplayStyle = ToolStripItemDisplayStyle.Text;
            ddbPrograms.Name = "ddbPrograms";
            ddbPrograms.Size = new Size(83, 22);
            ddbPrograms.Text = "Programme";
            ddbPrograms.ToolTipText = "Datei in einem anderen Programm öffnen (Strg+1 … Strg+9)";
            ddbPrograms.DropDownOpening += DdbPrograms_DropDownOpening;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(6, 25);
            // 
            // ddbInfo
            // 
            ddbInfo.Alignment = ToolStripItemAlignment.Right;
            ddbInfo.DisplayStyle = ToolStripItemDisplayStyle.Text;
            ddbInfo.DropDownItems.AddRange(new ToolStripItem[] { mnuShortcuts, mnuCheckUpdate, toolStripSeparator11, mnuAbout });
            ddbInfo.Name = "ddbInfo";
            ddbInfo.Size = new Size(45, 22);
            ddbInfo.Text = "Hilfe";
            // 
            // mnuShortcuts
            // 
            mnuShortcuts.Name = "mnuShortcuts";
            mnuShortcuts.ShortcutKeyDisplayString = "F1";
            mnuShortcuts.Size = new Size(198, 22);
            mnuShortcuts.Text = "Hilfedatei (PDF)…";
            mnuShortcuts.Click += MnuShortcuts_Click;
            // 
            // mnuCheckUpdate
            // 
            mnuCheckUpdate.Name = "mnuCheckUpdate";
            mnuCheckUpdate.Size = new Size(198, 22);
            mnuCheckUpdate.Text = "Nach Updates suchen…";
            mnuCheckUpdate.Click += MnuCheckUpdate_Click;
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new Size(195, 6);
            // 
            // mnuAbout
            // 
            mnuAbout.Name = "mnuAbout";
            mnuAbout.Size = new Size(198, 22);
            mnuAbout.Text = "Über PDFlight…";
            mnuAbout.Click += MnuAbout_Click;
            // 
            // btnSettings
            // 
            btnSettings.Alignment = ToolStripItemAlignment.Right;
            btnSettings.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(82, 22);
            btnSettings.Text = "Einstellungen";
            btnSettings.ToolTipText = "Zielordner, Programme und Optionen verwalten (Strg+,)";
            btnSettings.Click += BtnSettings_Click;
            // 
            // statusStrip
            // 
            statusStrip.AllowDrop = true;
            statusStrip.BackColor = SystemColors.Control;
            statusStrip.Items.AddRange(new ToolStripItem[] { statusIndex, statusPath, statusOneClick, statusFormat, statusZoom, statusInfo });
            statusStrip.Location = new Point(0, 725);
            statusStrip.Name = "statusStrip";
            statusStrip.ShowItemToolTips = true;
            statusStrip.Size = new Size(984, 24);
            statusStrip.TabIndex = 2;
            statusStrip.DragDrop += HandleDragDrop;
            statusStrip.DragEnter += HandleDragEnter;
            // 
            // statusIndex
            // 
            statusIndex.BorderSides = ToolStripStatusLabelBorderSides.Right;
            statusIndex.BorderStyle = Border3DStyle.Etched;
            statusIndex.Name = "statusIndex";
            statusIndex.Size = new Size(28, 19);
            statusIndex.Text = "0/0";
            statusIndex.ToolTipText = "Position der angezeigten Datei unter den PDF-Dateien des Ordners\r\n(mit Strg+Umschalt+← / → blättern)";
            // 
            // statusPath
            // 
            statusPath.Name = "statusPath";
            statusPath.Padding = new Padding(4, 0, 4, 0);
            statusPath.Size = new Size(937, 19);
            statusPath.Spring = true;
            statusPath.Text = "Keine Datei geöffnet";
            statusPath.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // statusOneClick
            // 
            statusOneClick.BorderSides = ToolStripStatusLabelBorderSides.Left;
            statusOneClick.BorderStyle = Border3DStyle.Etched;
            statusOneClick.Name = "statusOneClick";
            statusOneClick.Size = new Size(4, 19);
            statusOneClick.Visible = false;
            // 
            // statusFormat
            // 
            statusFormat.BorderSides = ToolStripStatusLabelBorderSides.Left;
            statusFormat.BorderStyle = Border3DStyle.Etched;
            statusFormat.Name = "statusFormat";
            statusFormat.Size = new Size(4, 19);
            statusFormat.Visible = false;
            // 
            // statusZoom
            // 
            statusZoom.BorderSides = ToolStripStatusLabelBorderSides.Left;
            statusZoom.BorderStyle = Border3DStyle.Etched;
            statusZoom.Name = "statusZoom";
            statusZoom.Size = new Size(4, 19);
            statusZoom.ToolTipText = "Zoomstufe der Anzeige";
            statusZoom.Visible = false;
            // 
            // statusInfo
            // 
            statusInfo.BorderSides = ToolStripStatusLabelBorderSides.Left;
            statusInfo.BorderStyle = Border3DStyle.Etched;
            statusInfo.Name = "statusInfo";
            statusInfo.Size = new Size(4, 19);
            statusInfo.ToolTipText = "Seitenzahl, Dateigröße und Änderungsdatum der angezeigten Datei";
            // 
            // pnlPdfA
            // 
            pnlPdfA.BackColor = SystemColors.Info;
            pnlPdfA.Controls.Add(btnPdfAEnable);
            pnlPdfA.Controls.Add(lblPdfA);
            pnlPdfA.Dock = DockStyle.Top;
            pnlPdfA.Location = new Point(0, 25);
            pnlPdfA.Name = "pnlPdfA";
            pnlPdfA.Size = new Size(984, 36);
            pnlPdfA.TabIndex = 3;
            pnlPdfA.Visible = false;
            // 
            // btnPdfAEnable
            // 
            btnPdfAEnable.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPdfAEnable.AutoSize = true;
            btnPdfAEnable.Location = new Point(812, 5);
            btnPdfAEnable.Name = "btnPdfAEnable";
            btnPdfAEnable.Size = new Size(164, 29);
            btnPdfAEnable.TabIndex = 1;
            btnPdfAEnable.Text = "Bearbeitung aktivieren";
            btnPdfAEnable.UseVisualStyleBackColor = true;
            btnPdfAEnable.Click += BtnPdfAEnable_Click;
            // 
            // lblPdfA
            // 
            lblPdfA.AutoSize = true;
            lblPdfA.ForeColor = SystemColors.InfoText;
            lblPdfA.Location = new Point(10, 9);
            lblPdfA.Name = "lblPdfA";
            lblPdfA.Size = new Size(685, 19);
            lblPdfA.TabIndex = 0;
            lblPdfA.Text = "Diese Datei entspricht dem PDF/A-Standard für die Langzeitarchivierung und wurde schreibgeschützt geöffnet.";
            // 
            // viewerStrip
            // 
            viewerStrip.AutoSize = false;
            viewerStrip.GripStyle = ToolStripGripStyle.Hidden;
            viewerStrip.Items.AddRange(new ToolStripItem[] { btnSidebar, toolStripSeparatorV1, btnUndo, btnHighlight, ddbHighlight, btnDraw, ddbInk, btnErase, toolStripSeparatorV5, btnFreeText, btnStamp, btnManageAnnotations, labelViewerSpacer, textPage, labelPageCount, toolStripSeparatorV2, btnZoomOut, comboZoom, btnZoomIn, btnFitWidth, btnTwoPage, toolStripSeparatorV3, btnRotateLeft, btnRotateRight, btnCloseDocument, toolStripSeparatorV6, btnFullScreen, toolStripSeparatorV4, btnSearch, btnSearchClose, btnSearchNext, btnSearchPrev, labelMatches, btnWholeWord, btnMatchCase, textSearch, btnSaveDocument });
            viewerStrip.Location = new Point(0, 61);
            viewerStrip.Name = "viewerStrip";
            viewerStrip.Padding = new Padding(0, 4, 1, 0);
            viewerStrip.Size = new Size(984, 40);
            viewerStrip.TabIndex = 4;
            viewerStrip.Resize += ViewerStrip_Resize;
            // 
            // btnSidebar
            // 
            btnSidebar.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSidebar.Name = "btnSidebar";
            btnSidebar.Size = new Size(104, 33);
            btnSidebar.Text = "Inhaltsverzeichnis";
            btnSidebar.ToolTipText = "Seitenleiste mit Miniaturen und Lesezeichen (F10)";
            btnSidebar.Click += BtnSidebar_Click;
            // 
            // toolStripSeparatorV1
            // 
            toolStripSeparatorV1.Name = "toolStripSeparatorV1";
            toolStripSeparatorV1.Size = new Size(6, 36);
            // 
            // btnUndo
            // 
            btnUndo.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnUndo.Enabled = false;
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new Size(74, 33);
            btnUndo.Text = "Rückgängig";
            btnUndo.ToolTipText = "Rückgängig (Strg+Z)";
            btnUndo.Click += BtnUndo_Click;
            // 
            // btnHighlight
            // 
            btnHighlight.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnHighlight.Enabled = false;
            btnHighlight.Name = "btnHighlight";
            btnHighlight.Size = new Size(80, 33);
            btnHighlight.Text = "Hervorheben";
            btnHighlight.ToolTipText = "Hervorheben: markierten Text sofort, sonst jede folgende Markierung (Esc beendet)";
            btnHighlight.Click += BtnHighlight_Click;
            // 
            // ddbHighlight
            // 
            ddbHighlight.DisplayStyle = ToolStripItemDisplayStyle.None;
            ddbHighlight.Enabled = false;
            ddbHighlight.Margin = new Padding(0, 1, 2, 2);
            ddbHighlight.Name = "ddbHighlight";
            ddbHighlight.AutoSize = false;
            ddbHighlight.Size = new Size(22, 33);
            ddbHighlight.ToolTipText = "Farbe zum Hervorheben";
            // 
            // btnDraw
            // 
            btnDraw.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnDraw.Enabled = false;
            btnDraw.Name = "btnDraw";
            btnDraw.Size = new Size(60, 33);
            btnDraw.Text = "Zeichnen";
            btnDraw.ToolTipText = "Zeichnen (Esc beendet)";
            btnDraw.Click += BtnDraw_Click;
            // 
            // ddbInk
            // 
            ddbInk.DisplayStyle = ToolStripItemDisplayStyle.None;
            ddbInk.Enabled = false;
            ddbInk.Margin = new Padding(0, 1, 2, 2);
            ddbInk.Name = "ddbInk";
            ddbInk.AutoSize = false;
            ddbInk.Size = new Size(22, 33);
            ddbInk.ToolTipText = "Farbe und Stärke";
            // 
            // btnErase
            // 
            btnErase.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnErase.Enabled = false;
            btnErase.Name = "btnErase";
            btnErase.Size = new Size(54, 33);
            btnErase.Text = "Radierer";
            btnErase.ToolTipText = "Radierer: Zeichnungen und Hervorhebungen durch Klicken oder Wischen entfernen (Esc beendet)";
            btnErase.Click += BtnErase_Click;
            // 
            // btnFreeText
            // 
            btnFreeText.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnFreeText.Enabled = false;
            btnFreeText.Name = "btnFreeText";
            btnFreeText.Size = new Size(49, 33);
            btnFreeText.Text = "Freitext";
            btnFreeText.ToolTipText = "Freitext hinzufügen (Strg+T)";
            btnFreeText.Click += BtnFreeText_Click;
            // 
            // btnStamp
            // 
            btnStamp.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnStamp.Enabled = false;
            btnStamp.Name = "btnStamp";
            btnStamp.Size = new Size(54, 33);
            btnStamp.Text = "Stempel";
            btnStamp.ToolTipText = "Stempel einfügen (Strg+H)";
            btnStamp.Click += BtnStamp_Click;
            //
            // btnManageAnnotations
            //
            btnManageAnnotations.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnManageAnnotations.Enabled = false;
            btnManageAnnotations.Name = "btnManageAnnotations";
            btnManageAnnotations.Size = new Size(64, 33);
            btnManageAnnotations.Text = "Verwalten";
            btnManageAnnotations.ToolTipText = "Anmerkungen verwalten (Strg+Umschalt+T)";
            btnManageAnnotations.Click += BtnManageAnnotations_Click;
            // 
            // labelViewerSpacer
            // 
            labelViewerSpacer.AutoSize = false;
            labelViewerSpacer.Name = "labelViewerSpacer";
            labelViewerSpacer.Size = new Size(0, 22);
            // 
            // textPage
            // 
            textPage.AutoSize = false;
            textPage.Enabled = false;
            textPage.Name = "textPage";
            textPage.Size = new Size(44, 25);
            textPage.TextBoxTextAlign = HorizontalAlignment.Right;
            textPage.ToolTipText = "Seite (Strg+G: Zahl tippen, Enter)";
            textPage.Leave += TextPage_Leave;
            textPage.KeyDown += TextPage_KeyDown;
            // 
            // labelPageCount
            // 
            labelPageCount.Name = "labelPageCount";
            labelPageCount.Size = new Size(0, 33);
            // 
            // toolStripSeparatorV2
            // 
            toolStripSeparatorV2.Name = "toolStripSeparatorV2";
            toolStripSeparatorV2.Size = new Size(6, 36);
            // 
            // btnZoomOut
            // 
            btnZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnZoomOut.Enabled = false;
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Size = new Size(23, 33);
            btnZoomOut.Text = "−";
            btnZoomOut.ToolTipText = "Verkleinern (Strg+Minus)";
            btnZoomOut.Click += BtnZoomOut_Click;
            // 
            // comboZoom
            // 
            comboZoom.AutoSize = false;
            comboZoom.DropDownWidth = 170;
            comboZoom.Enabled = false;
            comboZoom.Items.AddRange(new object[] { "An Seite anpassen", "An Höhe anpassen", "An Breite anpassen", "Originalgröße", "25 %", "50 %", "75 %", "100 %", "125 %", "150 %", "200 %", "300 %", "400 %" });
            comboZoom.Name = "comboZoom";
            comboZoom.Size = new Size(80, 23);
            comboZoom.ToolTipText = "Zoomstufe (Strg+Mausrad)";
            comboZoom.SelectedIndexChanged += ComboZoom_SelectedIndexChanged;
            comboZoom.KeyDown += ComboZoom_KeyDown;
            // 
            // btnZoomIn
            // 
            btnZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnZoomIn.Enabled = false;
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.Size = new Size(23, 33);
            btnZoomIn.Text = "+";
            btnZoomIn.ToolTipText = "Vergrößern (Strg+Plus)";
            btnZoomIn.Click += BtnZoomIn_Click;
            // 
            // btnFitWidth
            // 
            btnFitWidth.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnFitWidth.Enabled = false;
            btnFitWidth.Name = "btnFitWidth";
            btnFitWidth.Size = new Size(23, 33);
            btnFitWidth.Text = "↔";
            btnFitWidth.ToolTipText = "An Breite anpassen (Strg+Umschalt+B)";
            btnFitWidth.Click += BtnFitWidth_Click;
            // 
            // btnTwoPage
            // 
            btnTwoPage.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnTwoPage.Enabled = false;
            btnTwoPage.Name = "btnTwoPage";
            btnTwoPage.Size = new Size(35, 33);
            btnTwoPage.Text = "▯▯";
            btnTwoPage.ToolTipText = "Zweiseitige Ansicht ein/aus (Strg+Leertaste)";
            btnTwoPage.Click += BtnTwoPage_Click;
            // 
            // toolStripSeparatorV3
            // 
            toolStripSeparatorV3.Name = "toolStripSeparatorV3";
            toolStripSeparatorV3.Size = new Size(6, 36);
            // 
            // btnRotateLeft
            // 
            btnRotateLeft.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnRotateLeft.Enabled = false;
            btnRotateLeft.Name = "btnRotateLeft";
            btnRotateLeft.Size = new Size(23, 33);
            btnRotateLeft.Text = "↺";
            btnRotateLeft.ToolTipText = "Alle Seiten nach links drehen (Strg+Umschalt+L)";
            btnRotateLeft.Click += BtnRotateLeft_Click;
            // 
            // btnRotateRight
            // 
            btnRotateRight.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnRotateRight.Enabled = false;
            btnRotateRight.Name = "btnRotateRight";
            btnRotateRight.Size = new Size(23, 33);
            btnRotateRight.Text = "↻";
            btnRotateRight.ToolTipText = "Alle Seiten nach rechts drehen (Strg+Umschalt+R)";
            btnRotateRight.Click += BtnRotateRight_Click;
            // 
            // btnCloseDocument
            // 
            btnCloseDocument.Alignment = ToolStripItemAlignment.Right;
            btnCloseDocument.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnCloseDocument.Enabled = false;
            btnCloseDocument.Name = "btnCloseDocument";
            btnCloseDocument.Size = new Size(23, 33);
            btnCloseDocument.Text = "×";
            btnCloseDocument.ToolTipText = "Dokument schließen (Strg+W)";
            btnCloseDocument.Click += BtnCloseDocument_Click;
            // 
            // btnFullScreen
            // 
            btnFullScreen.Alignment = ToolStripItemAlignment.Right;
            btnFullScreen.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnFullScreen.Name = "btnFullScreen";
            btnFullScreen.Size = new Size(23, 33);
            btnFullScreen.Text = "⛶";
            btnFullScreen.ToolTipText = "PDF im Vollbildmodus anzeigen (F11)";
            btnFullScreen.Click += BtnFullScreen_Click;
            // 
            // toolStripSeparatorV4
            // 
            toolStripSeparatorV4.Alignment = ToolStripItemAlignment.Right;
            toolStripSeparatorV4.Name = "toolStripSeparatorV4";
            toolStripSeparatorV4.Size = new Size(6, 36);
            //
            // toolStripSeparatorV5
            //
            toolStripSeparatorV5.Name = "toolStripSeparatorV5";
            toolStripSeparatorV5.Size = new Size(6, 36);
            //
            // toolStripSeparatorV6
            //
            toolStripSeparatorV6.Alignment = ToolStripItemAlignment.Right;
            toolStripSeparatorV6.Name = "toolStripSeparatorV6";
            toolStripSeparatorV6.Size = new Size(6, 36);
            // 
            // btnSearch
            // 
            btnSearch.Alignment = ToolStripItemAlignment.Right;
            btnSearch.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSearch.Enabled = false;
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(50, 33);
            btnSearch.Text = "Suchen";
            btnSearch.ToolTipText = "Im Dokument suchen (Strg+F)";
            btnSearch.Click += BtnSearch_Click;
            // 
            // btnSearchClose
            // 
            btnSearchClose.Alignment = ToolStripItemAlignment.Right;
            btnSearchClose.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSearchClose.Name = "btnSearchClose";
            btnSearchClose.Size = new Size(23, 33);
            btnSearchClose.Text = "✕";
            btnSearchClose.ToolTipText = "Suche schließen (Esc)";
            btnSearchClose.Visible = false;
            btnSearchClose.Click += BtnSearchClose_Click;
            // 
            // btnSearchNext
            // 
            btnSearchNext.Alignment = ToolStripItemAlignment.Right;
            btnSearchNext.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSearchNext.Name = "btnSearchNext";
            btnSearchNext.Size = new Size(23, 33);
            btnSearchNext.Text = "▼";
            btnSearchNext.ToolTipText = "Nächster Treffer (F3, Enter)";
            btnSearchNext.Visible = false;
            btnSearchNext.Click += BtnSearchNext_Click;
            // 
            // btnSearchPrev
            // 
            btnSearchPrev.Alignment = ToolStripItemAlignment.Right;
            btnSearchPrev.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSearchPrev.Name = "btnSearchPrev";
            btnSearchPrev.Size = new Size(23, 19);
            btnSearchPrev.Text = "▲";
            btnSearchPrev.ToolTipText = "Vorheriger Treffer (Umschalt+F3)";
            btnSearchPrev.Visible = false;
            btnSearchPrev.Click += BtnSearchPrev_Click;
            // 
            // labelMatches
            // 
            labelMatches.Alignment = ToolStripItemAlignment.Right;
            labelMatches.Name = "labelMatches";
            labelMatches.Size = new Size(0, 0);
            labelMatches.Visible = false;
            // 
            // btnWholeWord
            // 
            btnWholeWord.Alignment = ToolStripItemAlignment.Right;
            btnWholeWord.CheckOnClick = true;
            btnWholeWord.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnWholeWord.Name = "btnWholeWord";
            btnWholeWord.Size = new Size(30, 19);
            btnWholeWord.Text = "|ab|";
            btnWholeWord.ToolTipText = "Nur ganze Wörter suchen";
            btnWholeWord.Visible = false;
            btnWholeWord.CheckedChanged += SearchOption_CheckedChanged;
            // 
            // btnMatchCase
            // 
            btnMatchCase.Alignment = ToolStripItemAlignment.Right;
            btnMatchCase.CheckOnClick = true;
            btnMatchCase.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnMatchCase.Name = "btnMatchCase";
            btnMatchCase.Size = new Size(25, 19);
            btnMatchCase.Text = "Aa";
            btnMatchCase.ToolTipText = "Groß-/Kleinschreibung beachten";
            btnMatchCase.Visible = false;
            btnMatchCase.CheckedChanged += SearchOption_CheckedChanged;
            // 
            // textSearch
            // 
            textSearch.Alignment = ToolStripItemAlignment.Right;
            textSearch.AutoSize = false;
            textSearch.Name = "textSearch";
            textSearch.Size = new Size(220, 25);
            textSearch.Visible = false;
            textSearch.KeyDown += TextSearch_KeyDown;
            textSearch.TextChanged += TextSearch_TextChanged;
            // 
            // btnSaveDocument
            // 
            btnSaveDocument.Alignment = ToolStripItemAlignment.Right;
            btnSaveDocument.Name = "btnSaveDocument";
            btnSaveDocument.Size = new Size(63, 19);
            btnSaveDocument.Text = "Speichern";
            btnSaveDocument.ToolTipText = "Formulareingaben in der Datei speichern (Strg+S)";
            btnSaveDocument.Visible = false;
            btnSaveDocument.Click += BtnSaveDocument_Click;
            // 
            // splitViewer
            // 
            splitViewer.Dock = DockStyle.Fill;
            splitViewer.FixedPanel = FixedPanel.Panel1;
            splitViewer.Location = new Point(0, 101);
            splitViewer.Name = "splitViewer";
            // 
            // splitViewer.Panel1
            // 
            splitViewer.Panel1.Controls.Add(panelSidebarContent);
            splitViewer.Panel1.Controls.Add(sidebarStrip);
            splitViewer.Panel1.Controls.Add(sidebarHeader);
            splitViewer.Panel1Collapsed = true;
            splitViewer.Panel1MinSize = 100;
            // 
            // splitViewer.Panel2
            // 
            splitViewer.Panel2.Controls.Add(pageView);
            splitViewer.Size = new Size(984, 624);
            splitViewer.SplitterDistance = 200;
            splitViewer.SplitterWidth = 5;
            splitViewer.TabIndex = 0;
            // 
            // panelSidebarContent
            // 
            panelSidebarContent.Controls.Add(thumbnailGrid);
            panelSidebarContent.Controls.Add(outlineView);
            panelSidebarContent.Dock = DockStyle.Fill;
            panelSidebarContent.Location = new Point(44, 40);
            panelSidebarContent.Name = "panelSidebarContent";
            panelSidebarContent.Size = new Size(156, 60);
            panelSidebarContent.TabIndex = 1;
            // 
            // thumbnailGrid
            // 
            thumbnailGrid.AutoScroll = true;
            thumbnailGrid.CurrentPageColor = Color.FromArgb(220, 238, 250);
            thumbnailGrid.Dock = DockStyle.Fill;
            thumbnailGrid.Location = new Point(0, 0);
            thumbnailGrid.Name = "thumbnailGrid";
            thumbnailGrid.Size = new Size(156, 60);
            thumbnailGrid.TabIndex = 1;
            thumbnailGrid.PageActivated += ThumbnailGrid_PageActivated;
            // 
            // outlineView
            // 
            outlineView.AccessibleName = "Dokumentstruktur";
            outlineView.AccessibleRole = AccessibleRole.Outline;
            outlineView.Dock = DockStyle.Fill;
            outlineView.Font = new Font("Segoe UI", 9.75F);
            outlineView.Location = new Point(0, 0);
            outlineView.Name = "outlineView";
            outlineView.Size = new Size(156, 60);
            outlineView.TabIndex = 2;
            outlineView.Visible = false;
            outlineView.ItemActivated += OutlineView_ItemActivated;
            // 
            // sidebarStrip
            // 
            sidebarStrip.AutoSize = false;
            sidebarStrip.Dock = DockStyle.Left;
            sidebarStrip.GripStyle = ToolStripGripStyle.Hidden;
            sidebarStrip.Items.AddRange(new ToolStripItem[] { btnSideThumbnails, btnSideBookmarks });
            sidebarStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            sidebarStrip.Location = new Point(0, 40);
            sidebarStrip.Name = "sidebarStrip";
            sidebarStrip.Padding = new Padding(0, 8, 0, 0);
            sidebarStrip.Size = new Size(44, 60);
            sidebarStrip.TabIndex = 0;
            // 
            // btnSideThumbnails
            // 
            btnSideThumbnails.AutoSize = false;
            btnSideThumbnails.Checked = true;
            btnSideThumbnails.CheckState = CheckState.Checked;
            btnSideThumbnails.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSideThumbnails.Margin = new Padding(0, 0, 0, 4);
            btnSideThumbnails.Name = "btnSideThumbnails";
            btnSideThumbnails.Size = new Size(44, 40);
            btnSideThumbnails.Text = "▦";
            btnSideThumbnails.ToolTipText = "Miniaturen";
            btnSideThumbnails.Click += BtnSideThumbnails_Click;
            // 
            // btnSideBookmarks
            // 
            btnSideBookmarks.AutoSize = false;
            btnSideBookmarks.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSideBookmarks.Margin = new Padding(0, 0, 0, 4);
            btnSideBookmarks.Name = "btnSideBookmarks";
            btnSideBookmarks.Size = new Size(44, 40);
            btnSideBookmarks.Text = "≡";
            btnSideBookmarks.ToolTipText = "Dokumentstruktur";
            btnSideBookmarks.Click += BtnSideBookmarks_Click;
            // 
            // sidebarHeader
            // 
            sidebarHeader.AutoSize = false;
            sidebarHeader.GripStyle = ToolStripGripStyle.Hidden;
            sidebarHeader.Items.AddRange(new ToolStripItem[] { labelSidebarTitle, btnSidebarClose });
            sidebarHeader.Location = new Point(0, 0);
            sidebarHeader.Name = "sidebarHeader";
            sidebarHeader.Padding = new Padding(10, 0, 6, 0);
            sidebarHeader.Size = new Size(200, 40);
            sidebarHeader.TabIndex = 3;
            // 
            // labelSidebarTitle
            // 
            labelSidebarTitle.Font = new Font("Segoe UI Semibold", 11F);
            labelSidebarTitle.Name = "labelSidebarTitle";
            labelSidebarTitle.Size = new Size(130, 37);
            labelSidebarTitle.Text = "Inhaltsverzeichnis";
            // 
            // btnSidebarClose
            // 
            btnSidebarClose.Alignment = ToolStripItemAlignment.Right;
            btnSidebarClose.AutoSize = false;
            btnSidebarClose.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSidebarClose.Name = "btnSidebarClose";
            btnSidebarClose.Size = new Size(32, 32);
            btnSidebarClose.Text = "✕";
            btnSidebarClose.ToolTipText = "Seitenleiste schließen";
            btnSidebarClose.Click += BtnSidebarClose_Click;
            // 
            // pageView
            // 
            pageView.AllowDrop = true;
            pageView.AutoScroll = true;
            pageView.BackColor = Color.FromArgb(243, 243, 243);
            pageView.ContextMenuStrip = contextMenuPage;
            pageView.CurrentPageHighlightColor = Color.FromArgb(220, 238, 250);
            pageView.Dock = DockStyle.Fill;
            pageView.Location = new Point(0, 0);
            pageView.Name = "pageView";
            pageView.ShowCurrentPageHighlight = true;
            pageView.Size = new Size(984, 624);
            pageView.TabIndex = 0;
            pageView.ZoomChanged += PageView_ZoomChanged;
            pageView.CurrentPageChanged += PageView_CurrentPageChanged;
            pageView.SelectionFinished += PageView_SelectionFinished;
            pageView.SearchChanged += PageView_SearchChanged;
            pageView.LinkHover += PageView_LinkHover;
            pageView.ExternalLinkClicked += PageView_ExternalLinkClicked;
            pageView.PageContentChanged += PageView_PageContentChanged;
            pageView.AnnotationMoved += PageView_AnnotationMoved;
            pageView.AnnotationDoubleClicked += PageView_AnnotationDoubleClicked;
            pageView.InkStarted += PageView_InkStarted;
            pageView.InkAdded += PageView_InkAdded;
            pageView.EraseStarted += PageView_EraseStarted;
            pageView.Erased += PageView_Erased;
            pageView.DragDrop += HandleDragDrop;
            pageView.DragEnter += HandleDragEnter;
            // 
            // contextMenuPage
            // 
            contextMenuPage.Items.AddRange(new ToolStripItem[] { mnuViewCopy, mnuViewSelectAll, toolStripSeparatorC1, mnuAddFreeTextHere, mnuHighlight, mnuEditAnnotation, mnuRemoveAnnotation });
            contextMenuPage.Name = "contextMenuPage";
            contextMenuPage.Size = new Size(206, 164);
            contextMenuPage.Opening += ContextMenuPage_Opening;
            // 
            // mnuViewCopy
            // 
            mnuViewCopy.Name = "mnuViewCopy";
            mnuViewCopy.ShortcutKeyDisplayString = "Strg+C";
            mnuViewCopy.Size = new Size(205, 22);
            mnuViewCopy.Text = "Kopieren";
            mnuViewCopy.Click += MnuViewCopy_Click;
            // 
            // mnuViewSelectAll
            // 
            mnuViewSelectAll.Name = "mnuViewSelectAll";
            mnuViewSelectAll.ShortcutKeyDisplayString = "Strg+A";
            mnuViewSelectAll.Size = new Size(205, 22);
            mnuViewSelectAll.Text = "Alles markieren";
            mnuViewSelectAll.Click += MnuViewSelectAll_Click;
            // 
            // toolStripSeparatorC1
            // 
            toolStripSeparatorC1.Name = "toolStripSeparatorC1";
            toolStripSeparatorC1.Size = new Size(202, 6);
            // 
            // mnuAddFreeTextHere
            // 
            mnuAddFreeTextHere.Name = "mnuAddFreeTextHere";
            mnuAddFreeTextHere.Size = new Size(205, 22);
            mnuAddFreeTextHere.Text = "Freitext hier einfügen…";
            mnuAddFreeTextHere.Click += MnuAddFreeTextHere_Click;
            // 
            // mnuHighlight
            // 
            mnuHighlight.Name = "mnuHighlight";
            mnuHighlight.Size = new Size(205, 22);
            mnuHighlight.Text = "Hervorheben";
            mnuHighlight.Click += MnuHighlight_Click;
            //
            // mnuEditAnnotation
            //
            mnuEditAnnotation.Name = "mnuEditAnnotation";
            mnuEditAnnotation.Size = new Size(205, 22);
            mnuEditAnnotation.Text = "Anmerkung bearbeiten…";
            mnuEditAnnotation.Click += MnuEditAnnotation_Click;
            //
            // mnuRemoveAnnotation
            // 
            mnuRemoveAnnotation.Name = "mnuRemoveAnnotation";
            mnuRemoveAnnotation.Size = new Size(205, 22);
            mnuRemoveAnnotation.Text = "Anmerkung entfernen";
            mnuRemoveAnnotation.Click += MnuRemoveAnnotation_Click;
            // 
            // printDocument
            // 
            printDocument.BeginPrint += PrintDocument_BeginPrint;
            printDocument.PrintPage += PrintDocument_PrintPage;
            printDocument.QueryPageSettings += PrintDocument_QueryPageSettings;
            // 
            // printDialog
            // 
            printDialog.AllowCurrentPage = true;
            printDialog.AllowSomePages = true;
            printDialog.Document = printDocument;
            printDialog.UseEXDialog = true;
            // 
            // MainForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 749);
            Controls.Add(splitViewer);
            Controls.Add(viewerStrip);
            Controls.Add(pnlPdfA);
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            Font = new Font("Segoe UI", 10F);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(600, 448);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PDFlight";
            Activated += MainForm_Activated;
            FormClosing += MainForm_FormClosing;
            FormClosed += MainForm_FormClosed;
            Shown += MainForm_Shown;
            DragDrop += HandleDragDrop;
            DragEnter += HandleDragEnter;
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            pnlPdfA.ResumeLayout(false);
            pnlPdfA.PerformLayout();
            viewerStrip.ResumeLayout(false);
            viewerStrip.PerformLayout();
            splitViewer.Panel1.ResumeLayout(false);
            splitViewer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitViewer).EndInit();
            splitViewer.ResumeLayout(false);
            panelSidebarContent.ResumeLayout(false);
            sidebarStrip.ResumeLayout(false);
            sidebarStrip.PerformLayout();
            sidebarHeader.ResumeLayout(false);
            sidebarHeader.PerformLayout();
            contextMenuPage.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Timer splashTimer;
        private System.Windows.Forms.Timer escHoldTimer;
        private System.Windows.Forms.Timer searchTimer;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripSplitButton btnOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnPrev;
        private System.Windows.Forms.ToolStripButton btnNext;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSplitButton splitButtonMove;
        private System.Windows.Forms.ToolStripButton btnCopy;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnRename;
        private System.Windows.Forms.ToolStripButton btnDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripButton btnEmail;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripDropDownButton ddbEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuDeletePages;
        private System.Windows.Forms.ToolStripMenuItem mnuRotatePages;
        private System.Windows.Forms.ToolStripMenuItem mnuMovePage;
        private System.Windows.Forms.ToolStripMenuItem mnuInsertPage;
        private System.Windows.Forms.ToolStripMenuItem mnuAppendPdf;
        private System.Windows.Forms.ToolStripMenuItem mnuDuplex;
        private System.Windows.Forms.ToolStripMenuItem mnuSetPassword;
        private System.Windows.Forms.ToolStripMenuItem mnuRemovePassword;
        private System.Windows.Forms.ToolStripMenuItem mnuRemoveRestrictions;
        private System.Windows.Forms.ToolStripMenuItem mnuExtractPages;
        private ToolStripButton btnManageAnnotations;
        private System.Windows.Forms.ToolStripMenuItem mnuRemoveBookmarks;
        private System.Windows.Forms.ToolStripMenuItem mnuEditBookmarks;
        private System.Windows.Forms.ToolStripMenuItem mnuManageStamps;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem mnuProperties;
        private System.Windows.Forms.ToolStripDropDownButton ddbPrograms;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripDropDownButton ddbFavorites;
        private System.Windows.Forms.ToolStripMenuItem mnuFavoriteAdd;
        private System.Windows.Forms.ToolStripMenuItem mnuFavoriteRemove;
        private System.Windows.Forms.ToolStripMenuItem mnuFavoriteCleanup;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripButton btnShowInFolder;
        private System.Windows.Forms.ToolStripButton btnPrint;
        private System.Windows.Forms.ToolStripButton btnSettings;
        private System.Windows.Forms.ToolStripDropDownButton ddbInfo;
        private System.Windows.Forms.ToolStripMenuItem mnuShortcuts;
        private System.Windows.Forms.ToolStripMenuItem mnuCheckUpdate;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem mnuAbout;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusIndex;
        private System.Windows.Forms.ToolStripStatusLabel statusPath;
        private System.Windows.Forms.ToolStripStatusLabel statusOneClick;
        private System.Windows.Forms.ToolStripStatusLabel statusFormat;
        private System.Windows.Forms.ToolStripStatusLabel statusZoom;
        private System.Windows.Forms.ToolStripStatusLabel statusInfo;
        private System.Windows.Forms.Panel pnlPdfA;
        private System.Windows.Forms.Label lblPdfA;
        private System.Windows.Forms.Button btnPdfAEnable;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripSeparator toolStripSeparator16;
        private ToolStrip viewerStrip;
        private ToolStripButton btnSidebar;
        private ToolStripSeparator toolStripSeparatorV1;
        private ToolStripButton btnUndo;
        private ToolStripButton btnHighlight;
        private ToolStripDropDownButton ddbHighlight;
        private ToolStripButton btnFreeText;
        private ToolStripButton btnStamp;
        private ToolStripButton btnDraw;
        private ToolStripDropDownButton ddbInk;
        private ToolStripButton btnErase;
        private ToolStripLabel labelViewerSpacer;
        private ToolStripTextBox textPage;
        private ToolStripLabel labelPageCount;
        private ToolStripSeparator toolStripSeparatorV2;
        private ToolStripButton btnZoomOut;
        private ToolStripComboBox comboZoom;
        private ToolStripButton btnZoomIn;
        private ToolStripButton btnFitWidth;
        private ToolStripButton btnTwoPage;
        private ToolStripSeparator toolStripSeparatorV3;
        private ToolStripButton btnRotateLeft;
        private ToolStripButton btnRotateRight;
        private ToolStripButton btnSearch;
        private ToolStripButton btnFullScreen;
        private ToolStripTextBox textSearch;
        private ToolStripButton btnSearchPrev;
        private ToolStripButton btnSearchNext;
        private ToolStripButton btnMatchCase;
        private ToolStripButton btnWholeWord;
        private ToolStripSeparator toolStripSeparatorV4;
        private ToolStripSeparator toolStripSeparatorV5;
        private ToolStripSeparator toolStripSeparatorV6;
        private ToolStripButton btnCloseDocument;
        private ToolStripButton btnSaveDocument;
        private ToolStripLabel labelMatches;
        private ToolStripButton btnSearchClose;
        private SplitContainer splitViewer;
        private PDFLight.Viewer.ThumbnailGrid thumbnailGrid;
        private PDFLight.Viewer.OutlineView outlineView;
        private ToolStrip sidebarStrip;
        private ToolStripButton btnSideThumbnails;
        private ToolStripButton btnSideBookmarks;
        private ToolStrip sidebarHeader;
        private Panel panelSidebarContent;
        private ToolStripLabel labelSidebarTitle;
        private ToolStripButton btnSidebarClose;
        private PDFLight.Viewer.PageView pageView;
        private ContextMenuStrip contextMenuPage;
        private ToolStripMenuItem mnuViewCopy;
        private ToolStripMenuItem mnuViewSelectAll;
        private ToolStripSeparator toolStripSeparatorC1;
        private ToolStripMenuItem mnuHighlight;
        private ToolStripMenuItem mnuEditAnnotation;
        private ToolStripMenuItem mnuRemoveAnnotation;
        private ToolStripMenuItem mnuAddFreeTextHere;
        private System.Drawing.Printing.PrintDocument printDocument;
        private PrintDialog printDialog;
        private ToolTip toolTipLink;
    }
}
