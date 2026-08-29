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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            webView = new Microsoft.Web.WebView2.WinForms.WebView2();
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
            btnEmail = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            ddbEdit = new ToolStripDropDownButton();
            mnuDeletePages = new ToolStripMenuItem();
            mnuRotatePages = new ToolStripMenuItem();
            mnuAppendPdf = new ToolStripMenuItem();
            mnuDuplex = new ToolStripMenuItem();
            mnuRemovePassword = new ToolStripMenuItem();
            mnuExtractPages = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            mnuUndo = new ToolStripMenuItem();
            toolStripSeparator7 = new ToolStripSeparator();
            mnuProperties = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            ddbPrograms = new ToolStripDropDownButton();
            toolStripSeparator5 = new ToolStripSeparator();
            btnSettings = new ToolStripButton();
            btnInfo = new ToolStripButton();
            statusStrip = new StatusStrip();
            statusIndex = new ToolStripStatusLabel();
            statusPath = new ToolStripStatusLabel();
            statusInfo = new ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)webView).BeginInit();
            toolStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // webView
            // 
            webView.AllowExternalDrop = true;
            webView.CreationProperties = null;
            webView.DefaultBackgroundColor = Color.White;
            webView.Dock = DockStyle.Fill;
            webView.Location = new Point(0, 25);
            webView.Name = "webView";
            webView.Size = new Size(984, 700);
            webView.TabIndex = 0;
            webView.ZoomFactor = 1D;
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { btnOpen, toolStripSeparator1, btnPrev, btnNext, toolStripSeparator2, splitButtonMove, btnCopy, toolStripSeparator3, btnRename, btnDelete, btnShowInFolder, toolStripSeparator8, btnEmail, toolStripSeparator4, ddbEdit, toolStripSeparator9, ddbPrograms, toolStripSeparator5, btnSettings, btnInfo });
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
            splitButtonMove.ToolTipText = "In einen Ordner verschieben (Strg+M)\r\nStrg+Klick: direkt in den ersten Zielordner\r\nPfeil: Zielliste";
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
            btnCopy.ToolTipText = "In einen Ordner kopieren (Strg+K)";
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
            ddbEdit.DropDownItems.AddRange(new ToolStripItem[] { mnuDeletePages, mnuRotatePages, mnuAppendPdf, mnuDuplex, mnuExtractPages, toolStripSeparator6, mnuUndo, toolStripSeparator7, mnuProperties, mnuRemovePassword });
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
            mnuDeletePages.Size = new Size(233, 22);
            mnuDeletePages.Text = "Seiten löschen …";
            mnuDeletePages.Click += MnuDeletePages_Click;
            // 
            // mnuRotatePages
            // 
            mnuRotatePages.Name = "mnuRotatePages";
            mnuRotatePages.ShortcutKeyDisplayString = "Strg+R";
            mnuRotatePages.Size = new Size(233, 22);
            mnuRotatePages.Text = "Seiten drehen …";
            mnuRotatePages.Click += MnuRotatePages_Click;
            // 
            // mnuAppendPdf
            // 
            mnuAppendPdf.Name = "mnuAppendPdf";
            mnuAppendPdf.Size = new Size(233, 22);
            mnuAppendPdf.Text = "PDF-Datei anhängen …";
            mnuAppendPdf.Click += MnuAppendPdf_Click;
            //
            // mnuDuplex
            //
            mnuDuplex.Name = "mnuDuplex";
            mnuDuplex.Size = new Size(267, 22);
            mnuDuplex.Text = "Duplex zusammenführen …";
            mnuDuplex.Click += MnuDuplex_Click;
            //
            // mnuRemovePassword
            //
            mnuRemovePassword.Name = "mnuRemovePassword";
            mnuRemovePassword.Size = new Size(267, 22);
            mnuRemovePassword.Text = "Kennwort entfernen …";
            mnuRemovePassword.Click += MnuRemovePassword_Click;
            // 
            // mnuExtractPages
            // 
            mnuExtractPages.Name = "mnuExtractPages";
            mnuExtractPages.ShortcutKeyDisplayString = "Strg+X";
            mnuExtractPages.Size = new Size(233, 22);
            mnuExtractPages.Text = "Seiten extrahieren …";
            mnuExtractPages.Click += MnuExtractPages_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(230, 6);
            // 
            // mnuUndo
            // 
            mnuUndo.Enabled = false;
            mnuUndo.Name = "mnuUndo";
            mnuUndo.ShortcutKeyDisplayString = "Strg+Z";
            mnuUndo.Size = new Size(233, 22);
            mnuUndo.Text = "Änderung rückgängig";
            mnuUndo.Click += MnuUndo_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(230, 6);
            // 
            // mnuProperties
            // 
            mnuProperties.Name = "mnuProperties";
            mnuProperties.ShortcutKeyDisplayString = "Strg+I";
            mnuProperties.Size = new Size(233, 22);
            mnuProperties.Text = "Eigenschaften …";
            mnuProperties.Click += MnuProperties_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(6, 25);
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
            // btnSettings
            // 
            btnSettings.Alignment = ToolStripItemAlignment.Right;
            btnSettings.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(82, 22);
            btnSettings.Text = "Einstellungen";
            btnSettings.ToolTipText = "Zielordner, Programme und Optionen verwalten";
            btnSettings.Click += BtnSettings_Click;
            // 
            // btnInfo
            // 
            btnInfo.Alignment = ToolStripItemAlignment.Right;
            btnInfo.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnInfo.Name = "btnInfo";
            btnInfo.Size = new Size(32, 22);
            btnInfo.Text = "Info";
            btnInfo.ToolTipText = "Über PDFlight und Spenden (F1)";
            btnInfo.Click += BtnInfo_Click;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { statusIndex, statusPath, statusInfo });
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
            // statusInfo
            // 
            statusInfo.BorderSides = ToolStripStatusLabelBorderSides.Left;
            statusInfo.BorderStyle = Border3DStyle.Etched;
            statusInfo.Name = "statusInfo";
            statusInfo.Size = new Size(4, 19);
            statusInfo.ToolTipText = "Seitenzahl, Dateigröße und Änderungsdatum der angezeigten Datei";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 749);
            Controls.Add(webView);
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
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
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
        private System.Windows.Forms.ToolStripMenuItem mnuAppendPdf;
        private System.Windows.Forms.ToolStripMenuItem mnuDuplex;
        private System.Windows.Forms.ToolStripMenuItem mnuRemovePassword;
        private System.Windows.Forms.ToolStripMenuItem mnuExtractPages;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem mnuUndo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem mnuProperties;
        private System.Windows.Forms.ToolStripDropDownButton ddbPrograms;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton btnShowInFolder;
        private System.Windows.Forms.ToolStripButton btnSettings;
        private System.Windows.Forms.ToolStripButton btnInfo;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusIndex;
        private System.Windows.Forms.ToolStripStatusLabel statusPath;
        private System.Windows.Forms.ToolStripStatusLabel statusInfo;
        private ToolStripSeparator toolStripSeparator9;
    }
}
