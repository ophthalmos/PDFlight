namespace PDFLight.Forms
{
    partial class SettingsForm
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            tabControl = new TabControl();
            tabGeneral = new TabPage();
            cbJumpLastUsed = new CheckBox();
            cbConfirmDelete = new CheckBox();
            cbOpenNextAfterDelete = new CheckBox();
            labelToolbar = new Label();
            comboToolbar = new ComboBox();
            cbCloseOnEscape = new CheckBox();
            cbReopenLast = new CheckBox();
            cbFullPathTitle = new CheckBox();
            cbShowFavorites = new CheckBox();
            labelMaxRecentFiles = new Label();
            numMaxRecentFiles = new NumericUpDown();
            labelLanguage = new Label();
            comboLanguage = new ComboBox();
            tabTargets = new TabPage();
            listTargets = new ListBox();
            btnTargetAdd = new Button();
            btnTargetRemove = new Button();
            btnTargetUp = new Button();
            btnTargetDown = new Button();
            btnTargetRemoveMissing = new Button();
            btnTargetSort = new Button();
            labelTargetStatus = new Label();
            labelTargetHint = new Label();
            tabPrograms = new TabPage();
            listPrograms = new ListBox();
            btnProgramAdd = new Button();
            btnProgramRemove = new Button();
            btnProgramUp = new Button();
            btnProgramDown = new Button();
            btnProgramDetect = new Button();
            btnProgramSort = new Button();
            labelProgramStatus = new Label();
            labelProgramHint = new Label();
            buttonOK = new Button();
            buttonCancel = new Button();
            panelButtons = new Panel();
            tabControl.SuspendLayout();
            tabGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMaxRecentFiles).BeginInit();
            tabTargets.SuspendLayout();
            tabPrograms.SuspendLayout();
            panelButtons.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabGeneral);
            tabControl.Controls.Add(tabTargets);
            tabControl.Controls.Add(tabPrograms);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(488, 326);
            tabControl.TabIndex = 0;
            // 
            // tabGeneral
            // 
            tabGeneral.Controls.Add(cbJumpLastUsed);
            tabGeneral.Controls.Add(cbConfirmDelete);
            tabGeneral.Controls.Add(cbOpenNextAfterDelete);
            tabGeneral.Controls.Add(labelToolbar);
            tabGeneral.Controls.Add(comboToolbar);
            tabGeneral.Controls.Add(cbCloseOnEscape);
            tabGeneral.Controls.Add(cbReopenLast);
            tabGeneral.Controls.Add(cbFullPathTitle);
            tabGeneral.Controls.Add(cbShowFavorites);
            tabGeneral.Controls.Add(labelMaxRecentFiles);
            tabGeneral.Controls.Add(numMaxRecentFiles);
            tabGeneral.Controls.Add(labelLanguage);
            tabGeneral.Controls.Add(comboLanguage);
            tabGeneral.Location = new Point(4, 24);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new Padding(3);
            tabGeneral.Size = new Size(480, 298);
            tabGeneral.TabIndex = 2;
            tabGeneral.Text = "Allgemein";
            tabGeneral.UseVisualStyleBackColor = true;
            // 
            // cbJumpLastUsed
            // 
            cbJumpLastUsed.AutoSize = true;
            cbJumpLastUsed.Location = new Point(18, 47);
            cbJumpLastUsed.Name = "cbJumpLastUsed";
            cbJumpLastUsed.Size = new Size(310, 19);
            cbJumpLastUsed.TabIndex = 1;
            cbJumpLastUsed.Text = "Ordnerdialog springt zum zuletzt &verwendeten Ordner";
            cbJumpLastUsed.UseVisualStyleBackColor = true;
            // 
            // cbConfirmDelete
            // 
            cbConfirmDelete.AutoSize = true;
            cbConfirmDelete.Location = new Point(18, 72);
            cbConfirmDelete.Name = "cbConfirmDelete";
            cbConfirmDelete.Size = new Size(295, 19);
            cbConfirmDelete.TabIndex = 2;
            cbConfirmDelete.Text = "Vor dem Verschieben in den &Papierkorb nachfragen";
            cbConfirmDelete.UseVisualStyleBackColor = true;
            // 
            // cbOpenNextAfterDelete
            // 
            cbOpenNextAfterDelete.AutoSize = true;
            cbOpenNextAfterDelete.Checked = true;
            cbOpenNextAfterDelete.CheckState = CheckState.Checked;
            cbOpenNextAfterDelete.Location = new Point(18, 97);
            cbOpenNextAfterDelete.Name = "cbOpenNextAfterDelete";
            cbOpenNextAfterDelete.Size = new Size(337, 19);
            cbOpenNextAfterDelete.TabIndex = 3;
            cbOpenNextAfterDelete.Text = "Nach dem Löschen die &nächste Datei des Ordners anzeigen";
            cbOpenNextAfterDelete.UseVisualStyleBackColor = true;
            // 
            // labelToolbar
            // 
            labelToolbar.AutoSize = true;
            labelToolbar.Location = new Point(18, 21);
            labelToolbar.Name = "labelToolbar";
            labelToolbar.Size = new Size(160, 15);
            labelToolbar.TabIndex = 4;
            labelToolbar.Text = "Layout der Hauptmenüleiste:";
            // 
            // comboToolbar
            // 
            comboToolbar.DropDownStyle = ComboBoxStyle.DropDownList;
            comboToolbar.Location = new Point(203, 18);
            comboToolbar.Name = "comboToolbar";
            comboToolbar.Size = new Size(269, 23);
            comboToolbar.TabIndex = 5;
            // 
            // cbCloseOnEscape
            // 
            cbCloseOnEscape.AutoSize = true;
            cbCloseOnEscape.Location = new Point(18, 122);
            cbCloseOnEscape.Name = "cbCloseOnEscape";
            cbCloseOnEscape.Size = new Size(313, 19);
            cbCloseOnEscape.TabIndex = 6;
            cbCloseOnEscape.Text = "Programm mit 2× &Esc beenden (Umschalt+Esc: sofort)";
            cbCloseOnEscape.UseVisualStyleBackColor = true;
            // 
            // cbReopenLast
            // 
            cbReopenLast.AutoSize = true;
            cbReopenLast.Location = new Point(18, 147);
            cbReopenLast.Name = "cbReopenLast";
            cbReopenLast.Size = new Size(235, 19);
            cbReopenLast.TabIndex = 7;
            cbReopenLast.Text = "Zuletzt geöffnete &Datei beim Start laden";
            cbReopenLast.UseVisualStyleBackColor = true;
            // 
            // cbFullPathTitle
            // 
            cbFullPathTitle.AutoSize = true;
            cbFullPathTitle.Location = new Point(18, 172);
            cbFullPathTitle.Name = "cbFullPathTitle";
            cbFullPathTitle.Size = new Size(287, 19);
            cbFullPathTitle.TabIndex = 8;
            cbFullPathTitle.Text = "Vollständigen Dateipfad in der Titelleiste an&zeigen";
            cbFullPathTitle.UseVisualStyleBackColor = true;
            // 
            // cbShowFavorites
            // 
            cbShowFavorites.AutoSize = true;
            cbShowFavorites.Location = new Point(18, 197);
            cbShowFavorites.Name = "cbShowFavorites";
            cbShowFavorites.Size = new Size(353, 19);
            cbShowFavorites.TabIndex = 9;
            cbShowFavorites.Text = "&Favoriten-Menü anzeigen (Dateien merken, max. 200 Einträge)";
            cbShowFavorites.UseVisualStyleBackColor = true;
            // 
            // labelMaxRecentFiles
            // 
            labelMaxRecentFiles.AutoSize = true;
            labelMaxRecentFiles.Location = new Point(18, 229);
            labelMaxRecentFiles.Name = "labelMaxRecentFiles";
            labelMaxRecentFiles.Size = new Size(245, 15);
            labelMaxRecentFiles.TabIndex = 10;
            labelMaxRecentFiles.Text = "Zuletzt geöffnete Dateien &merken (0 = keine):";
            // 
            // numMaxRecentFiles
            // 
            numMaxRecentFiles.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numMaxRecentFiles.Location = new Point(334, 226);
            numMaxRecentFiles.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            numMaxRecentFiles.Name = "numMaxRecentFiles";
            numMaxRecentFiles.Size = new Size(60, 23);
            numMaxRecentFiles.TabIndex = 11;
            numMaxRecentFiles.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // labelLanguage
            // 
            labelLanguage.AutoSize = true;
            labelLanguage.Location = new Point(18, 258);
            labelLanguage.Name = "labelLanguage";
            labelLanguage.Size = new Size(115, 15);
            labelLanguage.TabIndex = 12;
            labelLanguage.Text = "Sprache / &Language:";
            // 
            // comboLanguage
            // 
            comboLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            comboLanguage.Location = new Point(203, 255);
            comboLanguage.Name = "comboLanguage";
            comboLanguage.Size = new Size(125, 23);
            comboLanguage.TabIndex = 13;
            // 
            // tabTargets
            // 
            tabTargets.Controls.Add(listTargets);
            tabTargets.Controls.Add(btnTargetAdd);
            tabTargets.Controls.Add(btnTargetRemove);
            tabTargets.Controls.Add(btnTargetUp);
            tabTargets.Controls.Add(btnTargetDown);
            tabTargets.Controls.Add(btnTargetRemoveMissing);
            tabTargets.Controls.Add(btnTargetSort);
            tabTargets.Controls.Add(labelTargetStatus);
            tabTargets.Controls.Add(labelTargetHint);
            tabTargets.Location = new Point(4, 24);
            tabTargets.Name = "tabTargets";
            tabTargets.Padding = new Padding(3);
            tabTargets.Size = new Size(480, 298);
            tabTargets.TabIndex = 0;
            tabTargets.Text = "Zielordner";
            tabTargets.UseVisualStyleBackColor = true;
            // 
            // listTargets
            // 
            listTargets.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listTargets.DrawMode = DrawMode.OwnerDrawFixed;
            listTargets.IntegralHeight = false;
            listTargets.ItemHeight = 18;
            listTargets.Location = new Point(8, 8);
            listTargets.Name = "listTargets";
            listTargets.Size = new Size(320, 265);
            listTargets.TabIndex = 0;
            listTargets.DrawItem += ListTargets_DrawItem;
            listTargets.SelectedIndexChanged += ListTargets_SelectedIndexChanged;
            // 
            // btnTargetAdd
            // 
            btnTargetAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTargetAdd.Location = new Point(334, 8);
            btnTargetAdd.Name = "btnTargetAdd";
            btnTargetAdd.Size = new Size(138, 27);
            btnTargetAdd.TabIndex = 1;
            btnTargetAdd.Text = "&Hinzufügen …";
            btnTargetAdd.UseVisualStyleBackColor = true;
            btnTargetAdd.Click += BtnTargetAdd_Click;
            // 
            // btnTargetRemove
            // 
            btnTargetRemove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTargetRemove.Location = new Point(334, 41);
            btnTargetRemove.Name = "btnTargetRemove";
            btnTargetRemove.Size = new Size(138, 27);
            btnTargetRemove.TabIndex = 2;
            btnTargetRemove.Text = "&Entfernen";
            btnTargetRemove.UseVisualStyleBackColor = true;
            btnTargetRemove.Click += BtnTargetRemove_Click;
            // 
            // btnTargetUp
            // 
            btnTargetUp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTargetUp.Location = new Point(334, 84);
            btnTargetUp.Name = "btnTargetUp";
            btnTargetUp.Size = new Size(138, 27);
            btnTargetUp.TabIndex = 3;
            btnTargetUp.Text = "Nach &oben";
            btnTargetUp.UseVisualStyleBackColor = true;
            btnTargetUp.Click += BtnTargetUp_Click;
            // 
            // btnTargetDown
            // 
            btnTargetDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTargetDown.Location = new Point(334, 117);
            btnTargetDown.Name = "btnTargetDown";
            btnTargetDown.Size = new Size(138, 27);
            btnTargetDown.TabIndex = 4;
            btnTargetDown.Text = "Nach &unten";
            btnTargetDown.UseVisualStyleBackColor = true;
            btnTargetDown.Click += BtnTargetDown_Click;
            // 
            // btnTargetRemoveMissing
            // 
            btnTargetRemoveMissing.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTargetRemoveMissing.Location = new Point(334, 160);
            btnTargetRemoveMissing.Name = "btnTargetRemoveMissing";
            btnTargetRemoveMissing.Size = new Size(138, 27);
            btnTargetRemoveMissing.TabIndex = 5;
            btnTargetRemoveMissing.Text = "&Fehlende entfernen";
            btnTargetRemoveMissing.UseVisualStyleBackColor = true;
            btnTargetRemoveMissing.Click += BtnTargetRemoveMissing_Click;
            // 
            // btnTargetSort
            // 
            btnTargetSort.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTargetSort.Location = new Point(334, 193);
            btnTargetSort.Name = "btnTargetSort";
            btnTargetSort.Size = new Size(138, 27);
            btnTargetSort.TabIndex = 8;
            btnTargetSort.Text = "&Alphabetisch sortieren";
            btnTargetSort.UseVisualStyleBackColor = true;
            btnTargetSort.Click += BtnTargetSort_Click;
            // 
            // labelTargetStatus
            // 
            labelTargetStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelTargetStatus.ForeColor = Color.Firebrick;
            labelTargetStatus.Location = new Point(8, 202);
            labelTargetStatus.Name = "labelTargetStatus";
            labelTargetStatus.Size = new Size(318, 17);
            labelTargetStatus.TabIndex = 6;
            // 
            // labelTargetHint
            // 
            labelTargetHint.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelTargetHint.ForeColor = SystemColors.GrayText;
            labelTargetHint.Location = new Point(6, 276);
            labelTargetHint.Name = "labelTargetHint";
            labelTargetHint.Size = new Size(472, 19);
            labelTargetHint.TabIndex = 7;
            labelTargetHint.Text = "Strg+Klick auf \"Verschieben\"/\"Kopieren\": sofort in den 1-Klick-Ordner (s. Statusleiste).";
            // 
            // tabPrograms
            // 
            tabPrograms.Controls.Add(listPrograms);
            tabPrograms.Controls.Add(btnProgramAdd);
            tabPrograms.Controls.Add(btnProgramRemove);
            tabPrograms.Controls.Add(btnProgramUp);
            tabPrograms.Controls.Add(btnProgramDown);
            tabPrograms.Controls.Add(btnProgramDetect);
            tabPrograms.Controls.Add(btnProgramSort);
            tabPrograms.Controls.Add(labelProgramStatus);
            tabPrograms.Controls.Add(labelProgramHint);
            tabPrograms.Location = new Point(4, 24);
            tabPrograms.Name = "tabPrograms";
            tabPrograms.Padding = new Padding(3);
            tabPrograms.Size = new Size(480, 298);
            tabPrograms.TabIndex = 1;
            tabPrograms.Text = "Programme";
            tabPrograms.UseVisualStyleBackColor = true;
            // 
            // listPrograms
            // 
            listPrograms.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listPrograms.IntegralHeight = false;
            listPrograms.Location = new Point(8, 8);
            listPrograms.Name = "listPrograms";
            listPrograms.Size = new Size(320, 265);
            listPrograms.TabIndex = 0;
            listPrograms.SelectedIndexChanged += ListPrograms_SelectedIndexChanged;
            // 
            // btnProgramAdd
            // 
            btnProgramAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProgramAdd.Location = new Point(334, 6);
            btnProgramAdd.Name = "btnProgramAdd";
            btnProgramAdd.Size = new Size(138, 27);
            btnProgramAdd.TabIndex = 1;
            btnProgramAdd.Text = "&Hinzufügen …";
            btnProgramAdd.UseVisualStyleBackColor = true;
            btnProgramAdd.Click += BtnProgramAdd_Click;
            // 
            // btnProgramRemove
            // 
            btnProgramRemove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProgramRemove.Location = new Point(334, 41);
            btnProgramRemove.Name = "btnProgramRemove";
            btnProgramRemove.Size = new Size(138, 27);
            btnProgramRemove.TabIndex = 2;
            btnProgramRemove.Text = "&Entfernen";
            btnProgramRemove.UseVisualStyleBackColor = true;
            btnProgramRemove.Click += BtnProgramRemove_Click;
            // 
            // btnProgramUp
            // 
            btnProgramUp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProgramUp.Location = new Point(334, 84);
            btnProgramUp.Name = "btnProgramUp";
            btnProgramUp.Size = new Size(138, 27);
            btnProgramUp.TabIndex = 3;
            btnProgramUp.Text = "Nach &oben";
            btnProgramUp.UseVisualStyleBackColor = true;
            btnProgramUp.Click += BtnProgramUp_Click;
            // 
            // btnProgramDown
            // 
            btnProgramDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProgramDown.Location = new Point(334, 117);
            btnProgramDown.Name = "btnProgramDown";
            btnProgramDown.Size = new Size(138, 27);
            btnProgramDown.TabIndex = 4;
            btnProgramDown.Text = "Nach &unten";
            btnProgramDown.UseVisualStyleBackColor = true;
            btnProgramDown.Click += BtnProgramDown_Click;
            // 
            // btnProgramDetect
            // 
            btnProgramDetect.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProgramDetect.Location = new Point(334, 160);
            btnProgramDetect.Name = "btnProgramDetect";
            btnProgramDetect.Size = new Size(138, 27);
            btnProgramDetect.TabIndex = 5;
            btnProgramDetect.Text = "Neu e&rkennen";
            btnProgramDetect.UseVisualStyleBackColor = true;
            btnProgramDetect.Click += BtnProgramDetect_Click;
            // 
            // btnProgramSort
            // 
            btnProgramSort.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProgramSort.Location = new Point(334, 193);
            btnProgramSort.Name = "btnProgramSort";
            btnProgramSort.Size = new Size(138, 27);
            btnProgramSort.TabIndex = 8;
            btnProgramSort.Text = "&Alphabetisch sortieren";
            btnProgramSort.UseVisualStyleBackColor = true;
            btnProgramSort.Click += BtnProgramSort_Click;
            // 
            // labelProgramStatus
            // 
            labelProgramStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelProgramStatus.Location = new Point(8, 202);
            labelProgramStatus.Name = "labelProgramStatus";
            labelProgramStatus.Size = new Size(318, 17);
            labelProgramStatus.TabIndex = 6;
            // 
            // labelProgramHint
            // 
            labelProgramHint.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelProgramHint.ForeColor = SystemColors.GrayText;
            labelProgramHint.Location = new Point(6, 276);
            labelProgramHint.Name = "labelProgramHint";
            labelProgramHint.Size = new Size(472, 19);
            labelProgramHint.TabIndex = 7;
            labelProgramHint.Text = "Die Reihenfolge bestimmt die Tastenkürzel Strg+1 bis Strg+9 im Programme-Menü.";
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(207, 6);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(125, 27);
            buttonOK.TabIndex = 1;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(338, 6);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(138, 27);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // panelButtons
            // 
            panelButtons.Controls.Add(buttonCancel);
            panelButtons.Controls.Add(buttonOK);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 326);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(488, 45);
            panelButtons.TabIndex = 1;
            // 
            // SettingsForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(488, 371);
            Controls.Add(tabControl);
            Controls.Add(panelButtons);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(1000, 600);
            MinimizeBox = false;
            MinimumSize = new Size(504, 400);
            Name = "SettingsForm";
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Show;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Einstellungen";
            tabControl.ResumeLayout(false);
            tabGeneral.ResumeLayout(false);
            tabGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMaxRecentFiles).EndInit();
            tabTargets.ResumeLayout(false);
            tabPrograms.ResumeLayout(false);
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.TabPage tabTargets;
        private System.Windows.Forms.ListBox listTargets;
        private System.Windows.Forms.Button btnTargetAdd;
        private System.Windows.Forms.Button btnTargetRemove;
        private System.Windows.Forms.Button btnTargetUp;
        private System.Windows.Forms.Button btnTargetDown;
        private System.Windows.Forms.Button btnTargetRemoveMissing;
        private System.Windows.Forms.Button btnTargetSort;
        private System.Windows.Forms.Label labelTargetStatus;
        private System.Windows.Forms.Label labelTargetHint;
        private System.Windows.Forms.TabPage tabPrograms;
        private System.Windows.Forms.ListBox listPrograms;
        private System.Windows.Forms.Button btnProgramAdd;
        private System.Windows.Forms.Button btnProgramRemove;
        private System.Windows.Forms.Button btnProgramUp;
        private System.Windows.Forms.Button btnProgramDown;
        private System.Windows.Forms.Button btnProgramDetect;
        private System.Windows.Forms.Button btnProgramSort;
        private System.Windows.Forms.Label labelProgramStatus;
        private System.Windows.Forms.Label labelProgramHint;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.CheckBox cbJumpLastUsed;
        private System.Windows.Forms.CheckBox cbConfirmDelete;
        private System.Windows.Forms.CheckBox cbOpenNextAfterDelete;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.ComboBox comboLanguage;
        private System.Windows.Forms.Label labelMaxRecentFiles;
        private System.Windows.Forms.NumericUpDown numMaxRecentFiles;
        private System.Windows.Forms.Label labelToolbar;
        private System.Windows.Forms.ComboBox comboToolbar;
        private System.Windows.Forms.CheckBox cbCloseOnEscape;
        private System.Windows.Forms.CheckBox cbReopenLast;
        private System.Windows.Forms.CheckBox cbFullPathTitle;
        private System.Windows.Forms.CheckBox cbShowFavorites;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
