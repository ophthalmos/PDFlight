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
            webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            splashTimer = new System.Windows.Forms.Timer(components);
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
            toolStripSeparator15 = new ToolStripSeparator();
            mnuAppendPdf = new ToolStripMenuItem();
            mnuDuplex = new ToolStripMenuItem();
            mnuExtractPages = new ToolStripMenuItem();
            toolStripSeparator14 = new ToolStripSeparator();
            mnuAddStamp = new ToolStripMenuItem();
            mnuManageStamps = new ToolStripMenuItem();
            mnuAddAnnotation = new ToolStripMenuItem();
            mnuManageAnnotations = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            mnuUndo = new ToolStripMenuItem();
            toolStripSeparator7 = new ToolStripSeparator();
            mnuSetPassword = new ToolStripMenuItem();
            mnuRemovePassword = new ToolStripMenuItem();
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
            ((System.ComponentModel.ISupportInitialize)webView).BeginInit();
            toolStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            pnlPdfA.SuspendLayout();
            SuspendLayout();
            // 
            // webView
            // 
            webView.AllowExternalDrop = true;
            webView.CreationProperties = null;
            webView.DefaultBackgroundColor = Color.White;
            webView.Dock = DockStyle.Fill;
            webView.Location = new Point(0, 61);
            webView.Name = "webView";
            webView.Size = new Size(984, 664);
            webView.TabIndex = 0;
            webView.ZoomFactor = 1D;
            // 
            // splashTimer
            // 
            splashTimer.Interval = 1000;
            splashTimer.Tick += SplashTimer_Tick;
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { btnOpen, toolStripSeparator1, btnPrev, btnNext, toolStripSeparator2, splitButtonMove, btnCopy, toolStripSeparator3, btnRename, btnDelete, btnShowInFolder, toolStripSeparator8, btnPrint, btnEmail, toolStripSeparator4, ddbEdit, toolStripSeparator9, ddbFavorites, toolStripSeparator12, ddbPrograms, toolStripSeparator5, ddbInfo, btnSettings });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(984, 25);
            toolStrip.TabIndex = 1;
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
            ddbEdit.DropDownItems.AddRange(new ToolStripItem[] { mnuDeletePages, mnuRotatePages, mnuMovePage, toolStripSeparator15, mnuAppendPdf, mnuDuplex, mnuExtractPages, toolStripSeparator14, mnuAddStamp, mnuManageStamps, mnuAddAnnotation, mnuManageAnnotations, toolStripSeparator6, mnuUndo, toolStripSeparator7, mnuSetPassword, mnuRemovePassword, toolStripSeparator10, mnuProperties });
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
            mnuDeletePages.Size = new Size(336, 22);
            mnuDeletePages.Text = "Seiten löschen …";
            mnuDeletePages.Click += MnuDeletePages_Click;
            // 
            // mnuRotatePages
            // 
            mnuRotatePages.Name = "mnuRotatePages";
            mnuRotatePages.ShortcutKeyDisplayString = "Strg+R";
            mnuRotatePages.Size = new Size(336, 22);
            mnuRotatePages.Text = "Seiten drehen …";
            mnuRotatePages.Click += MnuRotatePages_Click;
            // 
            // mnuMovePage
            // 
            mnuMovePage.Name = "mnuMovePage";
            mnuMovePage.ShortcutKeyDisplayString = "Strg+Y";
            mnuMovePage.Size = new Size(336, 22);
            mnuMovePage.Text = "Aktuelle Seite verschieben …";
            mnuMovePage.Click += MnuMovePage_Click;
            // 
            // toolStripSeparator15
            // 
            toolStripSeparator15.Name = "toolStripSeparator15";
            toolStripSeparator15.Size = new Size(333, 6);
            // 
            // mnuAppendPdf
            // 
            mnuAppendPdf.Name = "mnuAppendPdf";
            mnuAppendPdf.ShortcutKeyDisplayString = "Strg+N";
            mnuAppendPdf.Size = new Size(336, 22);
            mnuAppendPdf.Text = "PDF-Datei anhängen …";
            mnuAppendPdf.Click += MnuAppendPdf_Click;
            // 
            // mnuDuplex
            // 
            mnuDuplex.Name = "mnuDuplex";
            mnuDuplex.Size = new Size(336, 22);
            mnuDuplex.Text = "Rückseiten-Scan einfügen …";
            mnuDuplex.Click += MnuDuplex_Click;
            // 
            // mnuExtractPages
            // 
            mnuExtractPages.Name = "mnuExtractPages";
            mnuExtractPages.ShortcutKeyDisplayString = "Strg+X";
            mnuExtractPages.Size = new Size(336, 22);
            mnuExtractPages.Text = "Seiten extrahieren …";
            mnuExtractPages.Click += MnuExtractPages_Click;
            // 
            // toolStripSeparator14
            // 
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new Size(333, 6);
            // 
            // mnuAddStamp
            // 
            mnuAddStamp.Name = "mnuAddStamp";
            mnuAddStamp.ShortcutKeyDisplayString = "Strg+H";
            mnuAddStamp.Size = new Size(336, 22);
            mnuAddStamp.Text = "Stempel einfügen …";
            mnuAddStamp.Click += MnuAddStamp_Click;
            // 
            // mnuManageStamps
            // 
            mnuManageStamps.Name = "mnuManageStamps";
            mnuManageStamps.ShortcutKeyDisplayString = "Strg+Umschalt+H";
            mnuManageStamps.Size = new Size(336, 22);
            mnuManageStamps.Text = "Stempel verwalten …";
            mnuManageStamps.Click += MnuManageStamps_Click;
            // 
            // mnuAddAnnotation
            // 
            mnuAddAnnotation.Name = "mnuAddAnnotation";
            mnuAddAnnotation.ShortcutKeyDisplayString = "Strg+T";
            mnuAddAnnotation.Size = new Size(336, 22);
            mnuAddAnnotation.Text = "Freitext hinzufügen …";
            mnuAddAnnotation.Click += MnuAddAnnotation_Click;
            // 
            // mnuManageAnnotations
            // 
            mnuManageAnnotations.Name = "mnuManageAnnotations";
            mnuManageAnnotations.ShortcutKeyDisplayString = "Strg+Umschalt+T";
            mnuManageAnnotations.Size = new Size(336, 22);
            mnuManageAnnotations.Text = "Anmerkungen verwalten …";
            mnuManageAnnotations.Click += MnuManageAnnotations_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(333, 6);
            // 
            // mnuUndo
            // 
            mnuUndo.Enabled = false;
            mnuUndo.Name = "mnuUndo";
            mnuUndo.ShortcutKeyDisplayString = "Strg+Z";
            mnuUndo.Size = new Size(336, 22);
            mnuUndo.Text = "Änderung rückgängig";
            mnuUndo.Click += MnuUndo_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(333, 6);
            // 
            // mnuSetPassword
            // 
            mnuSetPassword.Name = "mnuSetPassword";
            mnuSetPassword.Size = new Size(336, 22);
            mnuSetPassword.Text = "Kennwort vergeben …";
            mnuSetPassword.Click += MnuSetPassword_Click;
            // 
            // mnuRemovePassword
            // 
            mnuRemovePassword.Name = "mnuRemovePassword";
            mnuRemovePassword.Size = new Size(336, 22);
            mnuRemovePassword.Text = "Kennwort entfernen …";
            mnuRemovePassword.Click += MnuRemovePassword_Click;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(333, 6);
            // 
            // mnuProperties
            // 
            mnuProperties.Name = "mnuProperties";
            mnuProperties.ShortcutKeyDisplayString = "Strg+I";
            mnuProperties.Size = new Size(336, 22);
            mnuProperties.Text = "Eigenschaften …";
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
            mnuFavoriteAdd.Size = new Size(372, 22);
            mnuFavoriteAdd.Text = "Angezeigte Datei zu den Favoriten hinzufügen …";
            mnuFavoriteAdd.Click += MnuFavoriteAdd_Click;
            // 
            // mnuFavoriteRemove
            // 
            mnuFavoriteRemove.Name = "mnuFavoriteRemove";
            mnuFavoriteRemove.ShortcutKeyDisplayString = "Strg+D";
            mnuFavoriteRemove.Size = new Size(372, 22);
            mnuFavoriteRemove.Text = "Datei aus den Favoriten entfernen";
            mnuFavoriteRemove.Visible = false;
            mnuFavoriteRemove.Click += MnuFavoriteRemove_Click;
            // 
            // mnuFavoriteCleanup
            // 
            mnuFavoriteCleanup.Name = "mnuFavoriteCleanup";
            mnuFavoriteCleanup.Size = new Size(372, 22);
            mnuFavoriteCleanup.Text = "Nicht mehr vorhandene Favoriten entfernen";
            mnuFavoriteCleanup.Visible = false;
            mnuFavoriteCleanup.Click += MnuFavoriteCleanup_Click;
            // 
            // toolStripSeparator13
            // 
            toolStripSeparator13.Name = "toolStripSeparator13";
            toolStripSeparator13.Size = new Size(369, 6);
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
            mnuShortcuts.Size = new Size(201, 22);
            mnuShortcuts.Text = "Hilfedatei (PDF) …";
            mnuShortcuts.Click += MnuShortcuts_Click;
            // 
            // mnuCheckUpdate
            // 
            mnuCheckUpdate.Name = "mnuCheckUpdate";
            mnuCheckUpdate.Size = new Size(201, 22);
            mnuCheckUpdate.Text = "Nach Updates suchen …";
            mnuCheckUpdate.Click += MnuCheckUpdate_Click;
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new Size(198, 6);
            // 
            // mnuAbout
            // 
            mnuAbout.Name = "mnuAbout";
            mnuAbout.Size = new Size(201, 22);
            mnuAbout.Text = "Über PDFlight …";
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
            statusStrip.Items.AddRange(new ToolStripItem[] { statusIndex, statusPath, statusOneClick, statusFormat, statusZoom, statusInfo });
            statusStrip.Location = new Point(0, 725);
            statusStrip.Name = "statusStrip";
            statusStrip.ShowItemToolTips = true;
            statusStrip.Size = new Size(984, 24);
            statusStrip.TabIndex = 2;
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
            statusZoom.ToolTipText = "Zoomstufe des Viewers";
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
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 749);
            Controls.Add(webView);
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
            Shown += MainForm_Shown;
            ((System.ComponentModel.ISupportInitialize)webView).EndInit();
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            pnlPdfA.ResumeLayout(false);
            pnlPdfA.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private System.Windows.Forms.Timer splashTimer;
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
        private System.Windows.Forms.ToolStripMenuItem mnuAppendPdf;
        private System.Windows.Forms.ToolStripMenuItem mnuDuplex;
        private System.Windows.Forms.ToolStripMenuItem mnuSetPassword;
        private System.Windows.Forms.ToolStripMenuItem mnuRemovePassword;
        private System.Windows.Forms.ToolStripMenuItem mnuExtractPages;
        private System.Windows.Forms.ToolStripMenuItem mnuAddAnnotation;
        private System.Windows.Forms.ToolStripMenuItem mnuManageAnnotations;
        private System.Windows.Forms.ToolStripMenuItem mnuAddStamp;
        private System.Windows.Forms.ToolStripMenuItem mnuManageStamps;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem mnuUndo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
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
    }
}
