namespace PDFLight.Forms
{
    partial class FolderSelectForm
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(FolderSelectForm));
            comboBoxRecent = new ComboBox();
            labelTarget = new Label();
            buttonCancel = new Button();
            buttonOK = new Button();
            comboBoxTarget = new ComboBox();
            shellTreeView = new PDFLight.Controls.FolderTreeView();
            pathEdit = new PDFLight.Controls.PathEditBox();
            shellHistory = new PDFLight.Controls.FolderHistoryToolBar();
            cbAdd2Folderlist = new CheckBox();
            toolTip = new ToolTip(components);
            linkLabelRecent = new LinkLabel();
            contextMenuTree = new ContextMenuStrip(components);
            newFolderMenuItem = new ToolStripMenuItem();
            renameFolderMenuItem = new ToolStripMenuItem();
            deleteFolderMenuItem = new ToolStripMenuItem();
            contextMenuTree.SuspendLayout();
            SuspendLayout();
            // 
            // comboBoxRecent
            // 
            comboBoxRecent.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxRecent.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxRecent.FormattingEnabled = true;
            comboBoxRecent.Location = new Point(90, 12);
            comboBoxRecent.Name = "comboBoxRecent";
            comboBoxRecent.Size = new Size(381, 23);
            comboBoxRecent.TabIndex = 0;
            comboBoxRecent.SelectedIndexChanged += ComboBoxRecent_SelectedIndexChanged;
            // 
            // labelTarget
            // 
            labelTarget.AutoSize = true;
            labelTarget.Location = new Point(12, 44);
            labelTarget.Name = "labelTarget";
            labelTarget.Size = new Size(50, 15);
            labelTarget.TabIndex = 8;
            labelTarget.Text = "&Zielliste:";
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(356, 610);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(115, 25);
            buttonCancel.TabIndex = 8;
            buttonCancel.Text = "&Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(235, 610);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(115, 25);
            buttonOK.TabIndex = 7;
            buttonOK.Text = "Verschieben";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // comboBoxTarget
            // 
            comboBoxTarget.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxTarget.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxTarget.FormattingEnabled = true;
            comboBoxTarget.Location = new Point(90, 41);
            comboBoxTarget.Name = "comboBoxTarget";
            comboBoxTarget.Size = new Size(381, 23);
            comboBoxTarget.TabIndex = 1;
            comboBoxTarget.SelectedIndexChanged += ComboBoxTarget_SelectedIndexChanged;
            // 
            // shellTreeView
            // 
            shellTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            shellTreeView.ContextMenuStrip = contextMenuTree;
            shellTreeView.HideSelection = false;
            shellTreeView.ImageIndex = 0;
            shellTreeView.Indent = 19;
            shellTreeView.ItemHeight = 26;
            shellTreeView.Location = new Point(11, 70);
            shellTreeView.Name = "shellTreeView";
            shellTreeView.SelectedImageIndex = 0;
            shellTreeView.Size = new Size(460, 499);
            shellTreeView.TabIndex = 2;
            shellTreeView.AfterLabelEdit += ShellTreeView_AfterLabelEdit;
            shellTreeView.AfterSelect += ShellTreeView_AfterSelect;
            shellTreeView.DoubleClick += ShellTreeView_DoubleClick;
            shellTreeView.KeyDown += ShellTreeView_KeyDown;
            shellTreeView.NodeMouseClick += ShellTreeView_NodeMouseClick;
            shellTreeView.PreviewKeyDown += ShellTreeView_PreviewKeyDown;
            shellTreeView.Resize += ShellTreeView_Resize;
            // 
            // pathEdit
            // 
            pathEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pathEdit.Location = new Point(90, 577);
            pathEdit.Name = "pathEdit";
            pathEdit.Size = new Size(381, 23);
            pathEdit.TabIndex = 3;
            pathEdit.ButtonClick += PathEdit_ButtonClick;
            pathEdit.EditFieldEnter += PathEdit_EditFieldEnter;
            pathEdit.EditFieldLeave += PathEdit_EditFieldLeave;
            pathEdit.EditFieldClick += PathEdit_EditFieldClick;
            // 
            // shellHistory
            // 
            shellHistory.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            shellHistory.Dock = DockStyle.None;
            shellHistory.GripStyle = ToolStripGripStyle.Hidden;
            shellHistory.LayoutStyle = ToolStripLayoutStyle.Flow;
            shellHistory.Location = new Point(11, 578);
            shellHistory.MaximumSize = new Size(72, 28);
            shellHistory.Name = "shellHistory";
            shellHistory.Size = new Size(70, 22);
            shellHistory.TabIndex = 5;
            shellHistory.Tree = shellTreeView;
            // 
            // cbAdd2Folderlist
            // 
            cbAdd2Folderlist.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cbAdd2Folderlist.AutoSize = true;
            cbAdd2Folderlist.Enabled = false;
            cbAdd2Folderlist.Location = new Point(12, 614);
            cbAdd2Folderlist.Name = "cbAdd2Folderlist";
            cbAdd2Folderlist.Size = new Size(188, 19);
            cbAdd2Folderlist.TabIndex = 9;
            cbAdd2Folderlist.Text = "Ordner zur Zielliste hinzufügen";
            // 
            // linkLabelRecent
            // 
            linkLabelRecent.AutoSize = true;
            linkLabelRecent.Location = new Point(12, 15);
            linkLabelRecent.Name = "linkLabelRecent";
            linkLabelRecent.Size = new Size(46, 15);
            linkLabelRecent.TabIndex = 10;
            linkLabelRecent.TabStop = true;
            linkLabelRecent.Text = "Zuletzt:";
            linkLabelRecent.LinkClicked += LinkLabelRecent_LinkClicked;
            // 
            // FolderSelectForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(477, 644);
            Controls.Add(linkLabelRecent);
            Controls.Add(cbAdd2Folderlist);
            Controls.Add(shellHistory);
            Controls.Add(pathEdit);
            Controls.Add(shellTreeView);
            Controls.Add(comboBoxTarget);
            Controls.Add(buttonOK);
            Controls.Add(buttonCancel);
            Controls.Add(labelTarget);
            Controls.Add(comboBoxRecent);
            HelpButton = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(490, 376);
            Name = "FolderSelectForm";
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Show;
            StartPosition = FormStartPosition.CenterParent;
            Text = "VERSCHIEBEN: Wähle einen Ordner ...";
            HelpButtonClicked += FolderSelectForm_HelpButtonClicked;
            FormClosing += FolderSelectForm_FormClosing;
            Load += FolderSelectForm_Load;
            Shown += FolderSelectForm_Shown;
            HelpRequested += FolderSelectForm_HelpRequested;
            //
            // contextMenuTree
            //
            contextMenuTree.Items.AddRange(new ToolStripItem[] { newFolderMenuItem, renameFolderMenuItem, deleteFolderMenuItem });
            contextMenuTree.Name = "contextMenuTree";
            contextMenuTree.Size = new Size(220, 70);
            contextMenuTree.Opening += ContextMenuTree_Opening;
            //
            // newFolderMenuItem
            //
            newFolderMenuItem.Name = "newFolderMenuItem";
            newFolderMenuItem.ShortcutKeyDisplayString = "Strg+N";
            newFolderMenuItem.Size = new Size(219, 22);
            newFolderMenuItem.Text = "Neuer Ordner";
            newFolderMenuItem.Click += ButtonNewFolder_Clicked;
            //
            // renameFolderMenuItem
            //
            renameFolderMenuItem.Name = "renameFolderMenuItem";
            renameFolderMenuItem.ShortcutKeyDisplayString = "F2";
            renameFolderMenuItem.Size = new Size(219, 22);
            renameFolderMenuItem.Text = "Umbenennen";
            renameFolderMenuItem.Click += RenameFolderMenuItem_Click;
            //
            // deleteFolderMenuItem
            //
            deleteFolderMenuItem.Name = "deleteFolderMenuItem";
            deleteFolderMenuItem.ShortcutKeyDisplayString = "Entf";
            deleteFolderMenuItem.Size = new Size(219, 22);
            deleteFolderMenuItem.Text = "In den Papierkorb";
            deleteFolderMenuItem.Click += DeleteFolderMenuItem_Click;
            contextMenuTree.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxRecent;
        private System.Windows.Forms.Label labelTarget;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ComboBox comboBoxTarget;
        private PDFLight.Controls.FolderTreeView shellTreeView;
        private PDFLight.Controls.PathEditBox pathEdit;
        private PDFLight.Controls.FolderHistoryToolBar shellHistory;
        private System.Windows.Forms.CheckBox cbAdd2Folderlist;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.LinkLabel linkLabelRecent;
        private System.Windows.Forms.ContextMenuStrip contextMenuTree;
        private System.Windows.Forms.ToolStripMenuItem newFolderMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameFolderMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteFolderMenuItem;
    }
}
