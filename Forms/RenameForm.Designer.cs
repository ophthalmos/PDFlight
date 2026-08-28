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
            labelName = new System.Windows.Forms.Label();
            renameTextBox = new System.Windows.Forms.TextBox();
            btnTransform = new System.Windows.Forms.Button();
            btnDate = new System.Windows.Forms.Button();
            btnTransformMenu = new System.Windows.Forms.ContextMenuStrip(components);
            underscoreMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            hyphensMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            lowercaseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            firstLetterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeDiacriticMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            btnDateMenu = new System.Windows.Forms.ContextMenuStrip(components);
            directoryTextBox = new System.Windows.Forms.TextBox();
            folderOpenButton = new System.Windows.Forms.Button();
            otherFolderButton = new System.Windows.Forms.Button();
            labelList = new System.Windows.Forms.Label();
            toolStripSort = new System.Windows.Forms.ToolStrip();
            alphabeticSortButton = new System.Windows.Forms.ToolStripButton();
            dateSortButton = new System.Windows.Forms.ToolStripButton();
            listView = new System.Windows.Forms.ListView();
            columnName = new System.Windows.Forms.ColumnHeader();
            contextMenuListView = new System.Windows.Forms.ContextMenuStrip(components);
            acceptMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            renameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            deleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            propertiesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            labelHint = new System.Windows.Forms.Label();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            btnTransformMenu.SuspendLayout();
            toolStripSort.SuspendLayout();
            contextMenuListView.SuspendLayout();
            SuspendLayout();
            //
            // labelName
            //
            labelName.AutoSize = true;
            labelName.Location = new System.Drawing.Point(12, 12);
            labelName.Name = "labelName";
            labelName.Size = new System.Drawing.Size(103, 15);
            labelName.TabIndex = 0;
            labelName.Text = "&Neuer Dateiname:";
            //
            // renameTextBox
            //
            renameTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            renameTextBox.Location = new System.Drawing.Point(12, 33);
            renameTextBox.Name = "renameTextBox";
            renameTextBox.Size = new System.Drawing.Size(510, 23);
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
            btnTransform.Location = new System.Drawing.Point(12, 64);
            btnTransform.Name = "btnTransform";
            btnTransform.Size = new System.Drawing.Size(115, 27);
            btnTransform.TabIndex = 2;
            btnTransform.Text = "&Umwandeln  ▾";
            btnTransform.UseVisualStyleBackColor = true;
            btnTransform.Click += BtnTransform_Click;
            //
            // btnDate
            //
            btnDate.Location = new System.Drawing.Point(137, 64);
            btnDate.Name = "btnDate";
            btnDate.Size = new System.Drawing.Size(115, 27);
            btnDate.TabIndex = 3;
            btnDate.Text = "&Datum  ▾";
            btnDate.UseVisualStyleBackColor = true;
            btnDate.Click += BtnDate_Click;
            //
            // btnTransformMenu
            //
            btnTransformMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { underscoreMenuItem, hyphensMenuItem, lowercaseMenuItem, firstLetterMenuItem, removeDiacriticMenuItem });
            btnTransformMenu.Name = "btnTransformMenu";
            btnTransformMenu.Size = new System.Drawing.Size(230, 114);
            //
            // underscoreMenuItem
            //
            underscoreMenuItem.Name = "underscoreMenuItem";
            underscoreMenuItem.Size = new System.Drawing.Size(229, 22);
            underscoreMenuItem.Text = "Leerzeichen → Unterstriche";
            underscoreMenuItem.Click += UnderscoreMenuItem_Click;
            //
            // hyphensMenuItem
            //
            hyphensMenuItem.Name = "hyphensMenuItem";
            hyphensMenuItem.Size = new System.Drawing.Size(229, 22);
            hyphensMenuItem.Text = "Leerzeichen → Bindestriche";
            hyphensMenuItem.Click += HyphensMenuItem_Click;
            //
            // lowercaseMenuItem
            //
            lowercaseMenuItem.Name = "lowercaseMenuItem";
            lowercaseMenuItem.Size = new System.Drawing.Size(229, 22);
            lowercaseMenuItem.Text = "alles kleinschreiben";
            lowercaseMenuItem.Click += LowercaseMenuItem_Click;
            //
            // firstLetterMenuItem
            //
            firstLetterMenuItem.Name = "firstLetterMenuItem";
            firstLetterMenuItem.Size = new System.Drawing.Size(229, 22);
            firstLetterMenuItem.Text = "Wortanfänge großschreiben";
            firstLetterMenuItem.Click += FirstLetterMenuItem_Click;
            //
            // removeDiacriticMenuItem
            //
            removeDiacriticMenuItem.Name = "removeDiacriticMenuItem";
            removeDiacriticMenuItem.Size = new System.Drawing.Size(229, 22);
            removeDiacriticMenuItem.Text = "Umlaute && Akzente ersetzen";
            removeDiacriticMenuItem.Click += RemoveDiacriticMenuItem_Click;
            //
            // btnDateMenu
            //
            btnDateMenu.Name = "btnDateMenu";
            btnDateMenu.Size = new System.Drawing.Size(140, 4);
            btnDateMenu.ItemClicked += BtnDateMenu_ItemClicked;
            //
            // directoryTextBox
            //
            directoryTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            directoryTextBox.Location = new System.Drawing.Point(12, 103);
            directoryTextBox.Name = "directoryTextBox";
            directoryTextBox.ReadOnly = true;
            directoryTextBox.Size = new System.Drawing.Size(438, 23);
            directoryTextBox.TabIndex = 4;
            //
            // folderOpenButton
            //
            folderOpenButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            folderOpenButton.Location = new System.Drawing.Point(456, 102);
            folderOpenButton.Name = "folderOpenButton";
            folderOpenButton.Size = new System.Drawing.Size(30, 25);
            folderOpenButton.TabIndex = 5;
            folderOpenButton.Text = "📂";
            folderOpenButton.UseVisualStyleBackColor = true;
            folderOpenButton.Click += FolderButton_Click;
            //
            // otherFolderButton
            //
            otherFolderButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            otherFolderButton.Location = new System.Drawing.Point(492, 102);
            otherFolderButton.Name = "otherFolderButton";
            otherFolderButton.Size = new System.Drawing.Size(30, 25);
            otherFolderButton.TabIndex = 6;
            otherFolderButton.Text = "…";
            otherFolderButton.UseVisualStyleBackColor = true;
            otherFolderButton.Click += OtherFolderButton_Click;
            //
            // labelList
            //
            labelList.AutoSize = true;
            labelList.Location = new System.Drawing.Point(12, 140);
            labelList.Name = "labelList";
            labelList.Size = new System.Drawing.Size(133, 15);
            labelList.TabIndex = 7;
            labelList.Text = "PDF-Dateien im Ordner:";
            //
            // toolStripSort
            //
            toolStripSort.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            toolStripSort.Dock = System.Windows.Forms.DockStyle.None;
            toolStripSort.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripSort.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { alphabeticSortButton, dateSortButton });
            toolStripSort.Location = new System.Drawing.Point(410, 134);
            toolStripSort.Name = "toolStripSort";
            toolStripSort.Size = new System.Drawing.Size(112, 25);
            toolStripSort.TabIndex = 8;
            //
            // alphabeticSortButton
            //
            alphabeticSortButton.Checked = true;
            alphabeticSortButton.CheckState = System.Windows.Forms.CheckState.Checked;
            alphabeticSortButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            alphabeticSortButton.Name = "alphabeticSortButton";
            alphabeticSortButton.Size = new System.Drawing.Size(34, 22);
            alphabeticSortButton.Text = "A–Z";
            alphabeticSortButton.ToolTipText = "Alphabetisch sortieren (F5)";
            alphabeticSortButton.Click += AlphabeticSortButton_Click;
            //
            // dateSortButton
            //
            dateSortButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            dateSortButton.Name = "dateSortButton";
            dateSortButton.Size = new System.Drawing.Size(46, 22);
            dateSortButton.Text = "Datum";
            dateSortButton.ToolTipText = "Nach Änderungsdatum sortieren (F6)";
            dateSortButton.Click += DateSortButton_Click;
            //
            // listView
            //
            listView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnName });
            listView.ContextMenuStrip = contextMenuListView;
            listView.FullRowSelect = true;
            listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            listView.LabelEdit = true;
            listView.Location = new System.Drawing.Point(12, 162);
            listView.MultiSelect = false;
            listView.Name = "listView";
            listView.ShowItemToolTips = true;
            listView.Size = new System.Drawing.Size(510, 330);
            listView.TabIndex = 9;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = System.Windows.Forms.View.Details;
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
            contextMenuListView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { acceptMenuItem, renameMenuItem, openMenuItem, deleteMenuItem, propertiesMenuItem });
            contextMenuListView.Name = "contextMenuListView";
            contextMenuListView.Size = new System.Drawing.Size(240, 114);
            contextMenuListView.Opening += ContextMenuListView_Opening;
            //
            // acceptMenuItem
            //
            acceptMenuItem.Name = "acceptMenuItem";
            acceptMenuItem.ShortcutKeyDisplayString = "Eingabe";
            acceptMenuItem.Size = new System.Drawing.Size(239, 22);
            acceptMenuItem.Text = "Namen übernehmen";
            acceptMenuItem.Click += AcceptMenuItem_Click;
            //
            // renameMenuItem
            //
            renameMenuItem.Name = "renameMenuItem";
            renameMenuItem.ShortcutKeyDisplayString = "F2";
            renameMenuItem.Size = new System.Drawing.Size(239, 22);
            renameMenuItem.Text = "Umbenennen";
            renameMenuItem.Click += RenameMenuItem_Click;
            //
            // openMenuItem
            //
            openMenuItem.Name = "openMenuItem";
            openMenuItem.ShortcutKeyDisplayString = "Strg+Eingabe";
            openMenuItem.Size = new System.Drawing.Size(239, 22);
            openMenuItem.Text = "In neuem Fenster öffnen";
            openMenuItem.Click += OpenMenuItem_Click;
            //
            // deleteMenuItem
            //
            deleteMenuItem.Name = "deleteMenuItem";
            deleteMenuItem.ShortcutKeyDisplayString = "Entf";
            deleteMenuItem.Size = new System.Drawing.Size(239, 22);
            deleteMenuItem.Text = "In den Papierkorb";
            deleteMenuItem.Click += DeleteMenuItem_Click;
            //
            // propertiesMenuItem
            //
            propertiesMenuItem.Name = "propertiesMenuItem";
            propertiesMenuItem.Size = new System.Drawing.Size(239, 22);
            propertiesMenuItem.Text = "Eigenschaften";
            propertiesMenuItem.Click += PropertiesMenuItem_Click;
            //
            // labelHint
            //
            labelHint.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            labelHint.Location = new System.Drawing.Point(12, 500);
            labelHint.Name = "labelHint";
            labelHint.Size = new System.Drawing.Size(510, 32);
            labelHint.TabIndex = 10;
            labelHint.Text = "Doppelklick oder Eingabe übernimmt einen Namen aus der Liste  ·  F2: Listeneintrag umbenennen\r\nF4: anderer Zielordner  ·  F5/F6: Sortierung  ·  Strg+Eingabe: sofort umbenennen";
            //
            // btnOK
            //
            btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOK.Enabled = false;
            btnOK.Location = new System.Drawing.Point(296, 540);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(115, 27);
            btnOK.TabIndex = 11;
            btnOK.Text = "Umbenennen";
            btnOK.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(417, 540);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(105, 27);
            btnCancel.TabIndex = 12;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // RenameForm
            //
            AcceptButton = btnOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(534, 579);
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
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(480, 480);
            Name = "RenameForm";
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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
