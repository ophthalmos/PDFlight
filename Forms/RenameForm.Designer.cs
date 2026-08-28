namespace PDFLight.Forms
{
    partial class RenameForm
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
            labelName = new Label();
            renameTextBox = new TextBox();
            btnTransform = new Button();
            btnDate = new Button();
            btnTransformMenu = new ContextMenuStrip(components);
            underscoreMenuItem = new ToolStripMenuItem();
            hyphensMenuItem = new ToolStripMenuItem();
            lowercaseMenuItem = new ToolStripMenuItem();
            firstLetterMenuItem = new ToolStripMenuItem();
            removeDiacriticMenuItem = new ToolStripMenuItem();
            btnDateMenu = new ContextMenuStrip(components);
            directoryTextBox = new TextBox();
            folderOpenButton = new Button();
            otherFolderButton = new Button();
            labelList = new Label();
            toolStripSort = new ToolStrip();
            alphabeticSortButton = new ToolStripButton();
            dateSortButton = new ToolStripButton();
            listView = new ListView();
            columnName = new ColumnHeader();
            contextMenuListView = new ContextMenuStrip(components);
            acceptMenuItem = new ToolStripMenuItem();
            renameMenuItem = new ToolStripMenuItem();
            openMenuItem = new ToolStripMenuItem();
            deleteMenuItem = new ToolStripMenuItem();
            propertiesMenuItem = new ToolStripMenuItem();
            labelHint = new Label();
            btnOK = new Button();
            btnCancel = new Button();
            btnTransformMenu.SuspendLayout();
            toolStripSort.SuspendLayout();
            contextMenuListView.SuspendLayout();
            SuspendLayout();
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Location = new Point(12, 12);
            labelName.Name = "labelName";
            labelName.Size = new Size(102, 15);
            labelName.TabIndex = 0;
            labelName.Text = "&Neuer Dateiname:";
            // 
            // renameTextBox
            // 
            renameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            renameTextBox.Location = new Point(12, 39);
            renameTextBox.Name = "renameTextBox";
            renameTextBox.Size = new Size(360, 23);
            renameTextBox.TabIndex = 1;
            renameTextBox.Click += RenameTextBox_Click;
            renameTextBox.TextChanged += RenameTextBox_TextChanged;
            renameTextBox.Enter += RenameTextBox_Enter;
            renameTextBox.KeyDown += RenameTextBox_KeyDown;
            renameTextBox.KeyPress += RenameTextBox_KeyPress;
            renameTextBox.Leave += RenameTextBox_Leave;
            // 
            // btnTransform
            // 
            btnTransform.Location = new Point(257, 6);
            btnTransform.Name = "btnTransform";
            btnTransform.Size = new Size(115, 27);
            btnTransform.TabIndex = 2;
            btnTransform.Text = "&Umwandeln  ▾";
            btnTransform.UseVisualStyleBackColor = true;
            btnTransform.Click += BtnTransform_Click;
            // 
            // btnDate
            // 
            btnDate.Location = new Point(136, 6);
            btnDate.Name = "btnDate";
            btnDate.Size = new Size(115, 27);
            btnDate.TabIndex = 3;
            btnDate.Text = "&Datum  ▾";
            btnDate.UseVisualStyleBackColor = true;
            btnDate.Click += BtnDate_Click;
            // 
            // btnTransformMenu
            // 
            btnTransformMenu.Items.AddRange(new ToolStripItem[] { underscoreMenuItem, hyphensMenuItem, lowercaseMenuItem, firstLetterMenuItem, removeDiacriticMenuItem });
            btnTransformMenu.Name = "btnTransformMenu";
            btnTransformMenu.Size = new Size(224, 114);
            // 
            // underscoreMenuItem
            // 
            underscoreMenuItem.Name = "underscoreMenuItem";
            underscoreMenuItem.Size = new Size(223, 22);
            underscoreMenuItem.Text = "Leerzeichen → Unterstriche";
            underscoreMenuItem.Click += UnderscoreMenuItem_Click;
            // 
            // hyphensMenuItem
            // 
            hyphensMenuItem.Name = "hyphensMenuItem";
            hyphensMenuItem.Size = new Size(223, 22);
            hyphensMenuItem.Text = "Leerzeichen → Bindestriche";
            hyphensMenuItem.Click += HyphensMenuItem_Click;
            // 
            // lowercaseMenuItem
            // 
            lowercaseMenuItem.Name = "lowercaseMenuItem";
            lowercaseMenuItem.Size = new Size(223, 22);
            lowercaseMenuItem.Text = "alles kleinschreiben";
            lowercaseMenuItem.Click += LowercaseMenuItem_Click;
            // 
            // firstLetterMenuItem
            // 
            firstLetterMenuItem.Name = "firstLetterMenuItem";
            firstLetterMenuItem.Size = new Size(223, 22);
            firstLetterMenuItem.Text = "Wortanfänge großschreiben";
            firstLetterMenuItem.Click += FirstLetterMenuItem_Click;
            // 
            // removeDiacriticMenuItem
            // 
            removeDiacriticMenuItem.Name = "removeDiacriticMenuItem";
            removeDiacriticMenuItem.Size = new Size(223, 22);
            removeDiacriticMenuItem.Text = "Umlaute && Akzente ersetzen";
            removeDiacriticMenuItem.Click += RemoveDiacriticMenuItem_Click;
            // 
            // btnDateMenu
            // 
            btnDateMenu.Name = "btnDateMenu";
            btnDateMenu.Size = new Size(61, 4);
            btnDateMenu.ItemClicked += BtnDateMenu_ItemClicked;
            // 
            // directoryTextBox
            // 
            directoryTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            directoryTextBox.Location = new Point(12, 73);
            directoryTextBox.Name = "directoryTextBox";
            directoryTextBox.ReadOnly = true;
            directoryTextBox.Size = new Size(304, 23);
            directoryTextBox.TabIndex = 4;
            // 
            // folderOpenButton
            // 
            folderOpenButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            folderOpenButton.Location = new Point(316, 73);
            folderOpenButton.Name = "folderOpenButton";
            folderOpenButton.Size = new Size(30, 25);
            folderOpenButton.TabIndex = 5;
            folderOpenButton.Text = "📂";
            folderOpenButton.UseVisualStyleBackColor = true;
            folderOpenButton.Click += FolderButton_Click;
            // 
            // otherFolderButton
            // 
            otherFolderButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            otherFolderButton.Location = new Point(342, 73);
            otherFolderButton.Name = "otherFolderButton";
            otherFolderButton.Size = new Size(30, 25);
            otherFolderButton.TabIndex = 6;
            otherFolderButton.Text = "…";
            otherFolderButton.UseVisualStyleBackColor = true;
            otherFolderButton.Click += OtherFolderButton_Click;
            // 
            // labelList
            // 
            labelList.AutoSize = true;
            labelList.Location = new Point(12, 117);
            labelList.Name = "labelList";
            labelList.Size = new Size(133, 15);
            labelList.TabIndex = 7;
            labelList.Text = "PDF-Dateien im Ordner:";
            // 
            // toolStripSort
            // 
            toolStripSort.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            toolStripSort.Dock = DockStyle.None;
            toolStripSort.GripStyle = ToolStripGripStyle.Hidden;
            toolStripSort.Items.AddRange(new ToolStripItem[] { alphabeticSortButton, dateSortButton });
            toolStripSort.Location = new Point(290, 111);
            toolStripSort.Name = "toolStripSort";
            toolStripSort.Size = new Size(82, 25);
            toolStripSort.TabIndex = 8;
            // 
            // alphabeticSortButton
            // 
            alphabeticSortButton.Checked = true;
            alphabeticSortButton.CheckState = CheckState.Checked;
            alphabeticSortButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            alphabeticSortButton.Name = "alphabeticSortButton";
            alphabeticSortButton.Size = new Size(32, 22);
            alphabeticSortButton.Text = "A–Z";
            alphabeticSortButton.ToolTipText = "Alphabetisch sortieren (F5)";
            alphabeticSortButton.Click += AlphabeticSortButton_Click;
            // 
            // dateSortButton
            // 
            dateSortButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            dateSortButton.Name = "dateSortButton";
            dateSortButton.Size = new Size(47, 22);
            dateSortButton.Text = "Datum";
            dateSortButton.ToolTipText = "Nach Änderungsdatum sortieren (F6)";
            dateSortButton.Click += DateSortButton_Click;
            // 
            // listView
            // 
            listView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listView.Columns.AddRange(new ColumnHeader[] { columnName });
            listView.ContextMenuStrip = contextMenuListView;
            listView.FullRowSelect = true;
            listView.HeaderStyle = ColumnHeaderStyle.None;
            listView.LabelEdit = true;
            listView.Location = new Point(12, 139);
            listView.MultiSelect = false;
            listView.Name = "listView";
            listView.ShowItemToolTips = true;
            listView.Size = new Size(360, 253);
            listView.TabIndex = 9;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = View.Details;
            listView.AfterLabelEdit += ListView_AfterLabelEdit;
            listView.BeforeLabelEdit += ListView_BeforeLabelEdit;
            listView.KeyDown += ListView_KeyDown;
            listView.MouseDoubleClick += ListView_MouseDoubleClick;
            // 
            // columnName
            // 
            columnName.Text = "";
            columnName.Width = 480;
            // 
            // contextMenuListView
            // 
            contextMenuListView.Items.AddRange(new ToolStripItem[] { acceptMenuItem, renameMenuItem, openMenuItem, deleteMenuItem, propertiesMenuItem });
            contextMenuListView.Name = "contextMenuListView";
            contextMenuListView.Size = new Size(282, 114);
            contextMenuListView.Opening += ContextMenuListView_Opening;
            // 
            // acceptMenuItem
            // 
            acceptMenuItem.Name = "acceptMenuItem";
            acceptMenuItem.ShortcutKeyDisplayString = "Eingabe";
            acceptMenuItem.Size = new Size(281, 22);
            acceptMenuItem.Text = "Namen übernehmen";
            acceptMenuItem.Click += AcceptMenuItem_Click;
            // 
            // renameMenuItem
            // 
            renameMenuItem.Name = "renameMenuItem";
            renameMenuItem.ShortcutKeyDisplayString = "F2";
            renameMenuItem.Size = new Size(281, 22);
            renameMenuItem.Text = "Umbenennen";
            renameMenuItem.Click += RenameMenuItem_Click;
            // 
            // openMenuItem
            // 
            openMenuItem.Name = "openMenuItem";
            openMenuItem.ShortcutKeyDisplayString = "Strg+Eingabe";
            openMenuItem.Size = new Size(281, 22);
            openMenuItem.Text = "In neuem Fenster öffnen";
            openMenuItem.Click += OpenMenuItem_Click;
            // 
            // deleteMenuItem
            // 
            deleteMenuItem.Name = "deleteMenuItem";
            deleteMenuItem.ShortcutKeyDisplayString = "Entf";
            deleteMenuItem.Size = new Size(281, 22);
            deleteMenuItem.Text = "In den Papierkorb";
            deleteMenuItem.Click += DeleteMenuItem_Click;
            // 
            // propertiesMenuItem
            // 
            propertiesMenuItem.Name = "propertiesMenuItem";
            propertiesMenuItem.Size = new Size(281, 22);
            propertiesMenuItem.Text = "Eigenschaften";
            propertiesMenuItem.Click += PropertiesMenuItem_Click;
            // 
            // labelHint
            // 
            labelHint.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelHint.AutoSize = true;
            labelHint.ForeColor = SystemColors.GrayText;
            labelHint.Location = new Point(12, 395);
            labelHint.Name = "labelHint";
            labelHint.Size = new Size(349, 15);
            labelHint.TabIndex = 10;
            labelHint.Text = "Doppelklick oder Eingabe übernimmt einen Namen aus der Liste.";
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Enabled = false;
            btnOK.Location = new Point(136, 422);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(115, 27);
            btnOK.TabIndex = 11;
            btnOK.Text = "Umbenennen";
            btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(257, 422);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(115, 27);
            btnCancel.TabIndex = 12;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // RenameForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(384, 461);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(labelHint);
            Controls.Add(listView);
            Controls.Add(toolStripSort);
            Controls.Add(labelList);
            Controls.Add(otherFolderButton);
            Controls.Add(folderOpenButton);
            Controls.Add(directoryTextBox);
            Controls.Add(btnDate);
            Controls.Add(btnTransform);
            Controls.Add(renameTextBox);
            Controls.Add(labelName);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            Name = "RenameForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Datei umbenennen";
            FormClosing += RenameForm_FormClosing;
            Load += RenameForm_Load;
            Shown += RenameForm_Shown;
            btnTransformMenu.ResumeLayout(false);
            toolStripSort.ResumeLayout(false);
            toolStripSort.PerformLayout();
            contextMenuListView.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox renameTextBox;
        private System.Windows.Forms.Button btnTransform;
        private System.Windows.Forms.Button btnDate;
        private System.Windows.Forms.ContextMenuStrip btnTransformMenu;
        private System.Windows.Forms.ToolStripMenuItem underscoreMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hyphensMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lowercaseMenuItem;
        private System.Windows.Forms.ToolStripMenuItem firstLetterMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeDiacriticMenuItem;
        private System.Windows.Forms.ContextMenuStrip btnDateMenu;
        private System.Windows.Forms.TextBox directoryTextBox;
        private System.Windows.Forms.Button folderOpenButton;
        private System.Windows.Forms.Button otherFolderButton;
        private System.Windows.Forms.Label labelList;
        private System.Windows.Forms.ToolStrip toolStripSort;
        private System.Windows.Forms.ToolStripButton alphabeticSortButton;
        private System.Windows.Forms.ToolStripButton dateSortButton;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ContextMenuStrip contextMenuListView;
        private System.Windows.Forms.ToolStripMenuItem acceptMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem propertiesMenuItem;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
