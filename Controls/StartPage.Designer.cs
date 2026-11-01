namespace PDFLight.Controls
{
    partial class StartPage
    {
        /// <summary>Erforderliche Designervariable.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Verwendete Ressourcen bereinigen.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) { components.Dispose(); }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            panelTop = new Panel();
            linkOpen = new LinkLabel();
            textSearch = new TextBox();
            listView = new PDFLight.Viewer.RecentFilesView();
            contextMenuEntry = new ContextMenuStrip(components);
            mnuOpen = new ToolStripMenuItem();
            mnuShowInFolder = new ToolStripMenuItem();
            toolStripSeparatorC1 = new ToolStripSeparator();
            mnuAddFavorite = new ToolStripMenuItem();
            mnuRemoveFavorite = new ToolStripMenuItem();
            mnuRemoveRecent = new ToolStripMenuItem();
            panelBottom = new Panel();
            linkClear = new LinkLabel();
            toolStripActions = new ToolStrip();
            labelChecked = new ToolStripLabel();
            btnOpenChecked = new ToolStripButton();
            btnMergeChecked = new ToolStripButton();
            btnRemoveChecked = new ToolStripButton();
            btnClearChecked = new ToolStripButton();
            panelTop.SuspendLayout();
            contextMenuEntry.SuspendLayout();
            panelBottom.SuspendLayout();
            toolStripActions.SuspendLayout();
            SuspendLayout();
            //
            // panelTop
            //
            panelTop.Controls.Add(linkOpen);
            panelTop.Controls.Add(textSearch);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(24, 8);
            panelTop.Name = "panelTop";
            panelTop.Padding = new Padding(0, 14, 8, 0);
            panelTop.Size = new Size(712, 48);
            panelTop.TabIndex = 0;
            //
            // linkOpen
            //
            linkOpen.ActiveLinkColor = Color.FromArgb(0, 90, 158);
            linkOpen.AutoSize = true;
            linkOpen.Dock = DockStyle.Right;
            linkOpen.Font = new Font("Segoe UI", 10F);
            linkOpen.LinkBehavior = LinkBehavior.HoverUnderline;
            linkOpen.LinkColor = Color.FromArgb(0, 102, 204);
            linkOpen.Location = new Point(593, 14);
            linkOpen.Name = "linkOpen";
            linkOpen.Size = new Size(111, 17);
            linkOpen.TabIndex = 1;
            linkOpen.TabStop = true;
            linkOpen.Text = "Dokument öffnen…";
            linkOpen.LinkClicked += LinkOpen_LinkClicked;
            //
            // textSearch
            //
            textSearch.Font = new Font("Segoe UI", 10F);
            textSearch.Location = new Point(8, 11);
            textSearch.Name = "textSearch";
            textSearch.PlaceholderText = "Suchen";
            textSearch.Size = new Size(280, 25);
            textSearch.TabIndex = 0;
            textSearch.TextChanged += TextSearch_TextChanged;
            textSearch.KeyDown += TextSearch_KeyDown;
            //
            // listView
            //
            listView.AccessibleName = "Zuletzt geöffnet";
            listView.AccessibleRole = AccessibleRole.List;
            listView.AllowDrop = true;
            listView.AutoScroll = true;
            listView.ContextMenuStrip = contextMenuEntry;
            listView.Dock = DockStyle.Fill;
            listView.Font = new Font("Segoe UI", 10.5F);
            listView.Location = new Point(24, 56);
            listView.Name = "listView";
            listView.Size = new Size(712, 400);
            listView.TabIndex = 1;
            listView.OpenRequested += ListView_OpenRequested;
            listView.RemoveRequested += ListView_RemoveRequested;
            listView.LeaveUpRequested += ListView_LeaveUpRequested;
            listView.TypeAheadRequested += ListView_TypeAheadRequested;
            listView.CheckedChanged += ListView_CheckedChanged;
            listView.CollapsedChanged += ListView_CollapsedChanged;
            listView.DragDrop += ListView_DragDrop;
            listView.DragEnter += ListView_DragEnter;
            //
            // contextMenuEntry
            //
            contextMenuEntry.Items.AddRange(new ToolStripItem[] { mnuOpen, mnuShowInFolder, toolStripSeparatorC1, mnuAddFavorite, mnuRemoveFavorite, mnuRemoveRecent });
            contextMenuEntry.Name = "contextMenuEntry";
            contextMenuEntry.Size = new Size(246, 120);
            contextMenuEntry.Opening += ContextMenuEntry_Opening;
            //
            // mnuOpen
            //
            mnuOpen.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            mnuOpen.Name = "mnuOpen";
            mnuOpen.ShortcutKeyDisplayString = "Eingabe";
            mnuOpen.Size = new Size(245, 22);
            mnuOpen.Text = "Ö&ffnen";
            mnuOpen.Click += MnuOpen_Click;
            //
            // mnuShowInFolder
            //
            mnuShowInFolder.Name = "mnuShowInFolder";
            mnuShowInFolder.Size = new Size(245, 22);
            mnuShowInFolder.Text = "&Im Ordner anzeigen";
            mnuShowInFolder.Click += MnuShowInFolder_Click;
            //
            // toolStripSeparatorC1
            //
            toolStripSeparatorC1.Name = "toolStripSeparatorC1";
            toolStripSeparatorC1.Size = new Size(242, 6);
            //
            // mnuAddFavorite
            //
            mnuAddFavorite.Name = "mnuAddFavorite";
            mnuAddFavorite.Size = new Size(245, 22);
            mnuAddFavorite.Text = "Zu den Fa&voriten hinzufügen…";
            mnuAddFavorite.Click += MnuAddFavorite_Click;
            //
            // mnuRemoveFavorite
            //
            mnuRemoveFavorite.Name = "mnuRemoveFavorite";
            mnuRemoveFavorite.Size = new Size(245, 22);
            mnuRemoveFavorite.Text = "Aus den Favoriten &entfernen";
            mnuRemoveFavorite.Click += MnuRemoveFavorite_Click;
            //
            // mnuRemoveRecent
            //
            mnuRemoveRecent.Name = "mnuRemoveRecent";
            mnuRemoveRecent.ShortcutKeyDisplayString = "Entf";
            mnuRemoveRecent.Size = new Size(245, 22);
            mnuRemoveRecent.Text = "Aus der &Liste entfernen";
            mnuRemoveRecent.Click += MnuRemoveRecent_Click;
            //
            // panelBottom
            //
            panelBottom.Controls.Add(linkClear);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(24, 456);
            panelBottom.Name = "panelBottom";
            panelBottom.Padding = new Padding(0, 10, 8, 0);
            panelBottom.Size = new Size(712, 40);
            panelBottom.TabIndex = 2;
            //
            // linkClear
            //
            linkClear.ActiveLinkColor = Color.FromArgb(0, 90, 158);
            linkClear.AutoSize = true;
            linkClear.Dock = DockStyle.Right;
            linkClear.Font = new Font("Segoe UI", 10F);
            linkClear.LinkBehavior = LinkBehavior.HoverUnderline;
            linkClear.LinkColor = Color.FromArgb(0, 102, 204);
            linkClear.Location = new Point(620, 10);
            linkClear.Name = "linkClear";
            linkClear.Size = new Size(84, 17);
            linkClear.TabIndex = 0;
            linkClear.TabStop = true;
            linkClear.Text = "Liste leeren";
            linkClear.LinkClicked += LinkClear_LinkClicked;
            //
            // toolStripActions
            //
            toolStripActions.AutoSize = false;
            toolStripActions.Font = new Font("Segoe UI", 9.75F);
            toolStripActions.GripStyle = ToolStripGripStyle.Hidden;
            toolStripActions.Items.AddRange(new ToolStripItem[] { labelChecked, btnOpenChecked, btnMergeChecked, btnRemoveChecked, btnClearChecked });
            toolStripActions.Location = new Point(0, 56);
            toolStripActions.Name = "toolStripActions";
            toolStripActions.Padding = new Padding(24, 0, 24, 0);
            toolStripActions.Size = new Size(760, 36);
            toolStripActions.TabIndex = 3;
            //
            // labelChecked
            //
            labelChecked.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            labelChecked.Margin = new Padding(0, 1, 12, 2);
            labelChecked.Name = "labelChecked";
            labelChecked.Size = new Size(76, 33);
            labelChecked.Text = "{0} ausgewählt";
            labelChecked.Visible = false;
            //
            // btnOpenChecked
            //
            btnOpenChecked.Name = "btnOpenChecked";
            btnOpenChecked.Size = new Size(48, 33);
            btnOpenChecked.Text = "Öffnen";
            btnOpenChecked.Visible = false;
            btnOpenChecked.ToolTipText = "Jede Datei in einem eigenen Fenster öffnen";
            btnOpenChecked.Click += BtnOpenChecked_Click;
            //
            // btnMergeChecked
            //
            btnMergeChecked.Name = "btnMergeChecked";
            btnMergeChecked.Size = new Size(110, 33);
            btnMergeChecked.Text = "Zusammenführen…";
            btnMergeChecked.Visible = false;
            btnMergeChecked.ToolTipText = "Die Dateien zu einer neuen Sammelmappe zusammenführen";
            btnMergeChecked.Click += BtnMergeChecked_Click;
            //
            // btnRemoveChecked
            //
            btnRemoveChecked.Name = "btnRemoveChecked";
            btnRemoveChecked.Size = new Size(200, 33);
            btnRemoveChecked.Text = "Aus „Zuletzt geöffnet“ entfernen";
            btnRemoveChecked.Visible = false;
            btnRemoveChecked.Click += BtnRemoveChecked_Click;
            //
            // btnClearChecked
            //
            btnClearChecked.Alignment = ToolStripItemAlignment.Right;
            btnClearChecked.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnClearChecked.Name = "btnClearChecked";
            btnClearChecked.Size = new Size(23, 33);
            btnClearChecked.Text = "✕";
            btnClearChecked.Visible = false;
            btnClearChecked.ToolTipText = "Auswahl aufheben";
            btnClearChecked.Click += BtnClearChecked_Click;
            //
            // StartPage
            //
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(232, 232, 232);
            Controls.Add(listView);
            Controls.Add(toolStripActions);
            Controls.Add(panelBottom);
            Controls.Add(panelTop);
            Name = "StartPage";
            Padding = new Padding(0, 8, 0, 0);
            Size = new Size(760, 496);
            Resize += StartPage_Resize;
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            contextMenuEntry.ResumeLayout(false);
            panelBottom.ResumeLayout(false);
            panelBottom.PerformLayout();
            toolStripActions.ResumeLayout(false);
            toolStripActions.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelTop;
        private LinkLabel linkOpen;
        private TextBox textSearch;
        private PDFLight.Viewer.RecentFilesView listView;
        private ContextMenuStrip contextMenuEntry;
        private ToolStripMenuItem mnuOpen;
        private ToolStripMenuItem mnuShowInFolder;
        private ToolStripSeparator toolStripSeparatorC1;
        private ToolStripMenuItem mnuAddFavorite;
        private ToolStripMenuItem mnuRemoveFavorite;
        private ToolStripMenuItem mnuRemoveRecent;
        private Panel panelBottom;
        private LinkLabel linkClear;
        private ToolStrip toolStripActions;
        private ToolStripLabel labelChecked;
        private ToolStripButton btnOpenChecked;
        private ToolStripButton btnMergeChecked;
        private ToolStripButton btnRemoveChecked;
        private ToolStripButton btnClearChecked;
    }
}
