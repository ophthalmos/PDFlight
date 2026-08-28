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
            tabControl = new System.Windows.Forms.TabControl();
            tabTargets = new System.Windows.Forms.TabPage();
            listTargets = new System.Windows.Forms.ListBox();
            btnTargetAdd = new System.Windows.Forms.Button();
            btnTargetRemove = new System.Windows.Forms.Button();
            btnTargetUp = new System.Windows.Forms.Button();
            btnTargetDown = new System.Windows.Forms.Button();
            btnTargetRemoveMissing = new System.Windows.Forms.Button();
            btnTargetSort = new System.Windows.Forms.Button();
            labelTargetStatus = new System.Windows.Forms.Label();
            labelTargetHint = new System.Windows.Forms.Label();
            tabPrograms = new System.Windows.Forms.TabPage();
            listPrograms = new System.Windows.Forms.ListBox();
            btnProgramAdd = new System.Windows.Forms.Button();
            btnProgramRemove = new System.Windows.Forms.Button();
            btnProgramUp = new System.Windows.Forms.Button();
            btnProgramDown = new System.Windows.Forms.Button();
            btnProgramDetect = new System.Windows.Forms.Button();
            btnProgramSort = new System.Windows.Forms.Button();
            labelProgramStatus = new System.Windows.Forms.Label();
            labelProgramHint = new System.Windows.Forms.Label();
            tabGeneral = new System.Windows.Forms.TabPage();
            cbJumpLastUsed = new System.Windows.Forms.CheckBox();
            cbConfirmDelete = new System.Windows.Forms.CheckBox();
            cbShowProgramIcons = new System.Windows.Forms.CheckBox();
            cbToolbarIcons = new System.Windows.Forms.CheckBox();
            cbLargeIcons = new System.Windows.Forms.CheckBox();
            cbCloseOnEscape = new System.Windows.Forms.CheckBox();
            cbReopenLast = new System.Windows.Forms.CheckBox();
            btnClearRecent = new System.Windows.Forms.Button();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            tabControl.SuspendLayout();
            tabTargets.SuspendLayout();
            tabPrograms.SuspendLayout();
            tabGeneral.SuspendLayout();
            SuspendLayout();
            //
            // tabControl
            //
            tabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControl.Controls.Add(tabGeneral);
            tabControl.Controls.Add(tabTargets);
            tabControl.Controls.Add(tabPrograms);
            tabControl.Location = new System.Drawing.Point(12, 12);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(610, 350);
            tabControl.TabIndex = 0;
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
            tabTargets.Location = new System.Drawing.Point(4, 24);
            tabTargets.Name = "tabTargets";
            tabTargets.Padding = new System.Windows.Forms.Padding(3);
            tabTargets.Size = new System.Drawing.Size(602, 322);
            tabTargets.TabIndex = 0;
            tabTargets.Text = "Zielordner";
            tabTargets.UseVisualStyleBackColor = true;
            //
            // listTargets
            //
            listTargets.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            listTargets.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            listTargets.IntegralHeight = false;
            listTargets.ItemHeight = 18;
            listTargets.Location = new System.Drawing.Point(8, 8);
            listTargets.Name = "listTargets";
            listTargets.Size = new System.Drawing.Size(440, 250);
            listTargets.TabIndex = 0;
            listTargets.DrawItem += ListTargets_DrawItem;
            listTargets.SelectedIndexChanged += ListTargets_SelectedIndexChanged;
            //
            // btnTargetAdd
            //
            btnTargetAdd.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnTargetAdd.Location = new System.Drawing.Point(456, 8);
            btnTargetAdd.Name = "btnTargetAdd";
            btnTargetAdd.Size = new System.Drawing.Size(138, 27);
            btnTargetAdd.TabIndex = 1;
            btnTargetAdd.Text = "&Hinzufügen …";
            btnTargetAdd.UseVisualStyleBackColor = true;
            btnTargetAdd.Click += BtnTargetAdd_Click;
            //
            // btnTargetRemove
            //
            btnTargetRemove.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnTargetRemove.Location = new System.Drawing.Point(456, 41);
            btnTargetRemove.Name = "btnTargetRemove";
            btnTargetRemove.Size = new System.Drawing.Size(138, 27);
            btnTargetRemove.TabIndex = 2;
            btnTargetRemove.Text = "&Entfernen";
            btnTargetRemove.UseVisualStyleBackColor = true;
            btnTargetRemove.Click += BtnTargetRemove_Click;
            //
            // btnTargetUp
            //
            btnTargetUp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnTargetUp.Location = new System.Drawing.Point(456, 84);
            btnTargetUp.Name = "btnTargetUp";
            btnTargetUp.Size = new System.Drawing.Size(138, 27);
            btnTargetUp.TabIndex = 3;
            btnTargetUp.Text = "Nach &oben";
            btnTargetUp.UseVisualStyleBackColor = true;
            btnTargetUp.Click += BtnTargetUp_Click;
            //
            // btnTargetDown
            //
            btnTargetDown.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnTargetDown.Location = new System.Drawing.Point(456, 117);
            btnTargetDown.Name = "btnTargetDown";
            btnTargetDown.Size = new System.Drawing.Size(138, 27);
            btnTargetDown.TabIndex = 4;
            btnTargetDown.Text = "Nach &unten";
            btnTargetDown.UseVisualStyleBackColor = true;
            btnTargetDown.Click += BtnTargetDown_Click;
            //
            // btnTargetRemoveMissing
            //
            btnTargetRemoveMissing.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnTargetRemoveMissing.Location = new System.Drawing.Point(456, 160);
            btnTargetRemoveMissing.Name = "btnTargetRemoveMissing";
            btnTargetRemoveMissing.Size = new System.Drawing.Size(138, 27);
            btnTargetRemoveMissing.TabIndex = 5;
            btnTargetRemoveMissing.Text = "&Fehlende entfernen";
            btnTargetRemoveMissing.UseVisualStyleBackColor = true;
            btnTargetRemoveMissing.Click += BtnTargetRemoveMissing_Click;
            //
            // btnTargetSort
            //
            btnTargetSort.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnTargetSort.Location = new System.Drawing.Point(456, 193);
            btnTargetSort.Name = "btnTargetSort";
            btnTargetSort.Size = new System.Drawing.Size(138, 27);
            btnTargetSort.TabIndex = 8;
            btnTargetSort.Text = "&Alphabetisch sortieren";
            btnTargetSort.UseVisualStyleBackColor = true;
            btnTargetSort.Click += BtnTargetSort_Click;
            //
            // labelTargetStatus
            //
            labelTargetStatus.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            labelTargetStatus.ForeColor = System.Drawing.Color.Firebrick;
            labelTargetStatus.Location = new System.Drawing.Point(8, 264);
            labelTargetStatus.Name = "labelTargetStatus";
            labelTargetStatus.Size = new System.Drawing.Size(440, 17);
            labelTargetStatus.TabIndex = 6;
            //
            // labelTargetHint
            //
            labelTargetHint.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            labelTargetHint.ForeColor = System.Drawing.SystemColors.GrayText;
            labelTargetHint.Location = new System.Drawing.Point(8, 288);
            labelTargetHint.Name = "labelTargetHint";
            labelTargetHint.Size = new System.Drawing.Size(586, 30);
            labelTargetHint.TabIndex = 7;
            labelTargetHint.Text = "Der erste Ordner ist das Ziel für das Schnell-Verschieben (Strg+Klick auf \"Verschieben\").";
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
            tabPrograms.Location = new System.Drawing.Point(4, 24);
            tabPrograms.Name = "tabPrograms";
            tabPrograms.Padding = new System.Windows.Forms.Padding(3);
            tabPrograms.Size = new System.Drawing.Size(602, 322);
            tabPrograms.TabIndex = 1;
            tabPrograms.Text = "Programme";
            tabPrograms.UseVisualStyleBackColor = true;
            //
            // listPrograms
            //
            listPrograms.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            listPrograms.IntegralHeight = false;
            listPrograms.ItemHeight = 15;
            listPrograms.Location = new System.Drawing.Point(8, 8);
            listPrograms.Name = "listPrograms";
            listPrograms.Size = new System.Drawing.Size(440, 250);
            listPrograms.TabIndex = 0;
            listPrograms.SelectedIndexChanged += ListPrograms_SelectedIndexChanged;
            //
            // btnProgramAdd
            //
            btnProgramAdd.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnProgramAdd.Location = new System.Drawing.Point(456, 8);
            btnProgramAdd.Name = "btnProgramAdd";
            btnProgramAdd.Size = new System.Drawing.Size(138, 27);
            btnProgramAdd.TabIndex = 1;
            btnProgramAdd.Text = "&Hinzufügen …";
            btnProgramAdd.UseVisualStyleBackColor = true;
            btnProgramAdd.Click += BtnProgramAdd_Click;
            //
            // btnProgramRemove
            //
            btnProgramRemove.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnProgramRemove.Location = new System.Drawing.Point(456, 41);
            btnProgramRemove.Name = "btnProgramRemove";
            btnProgramRemove.Size = new System.Drawing.Size(138, 27);
            btnProgramRemove.TabIndex = 2;
            btnProgramRemove.Text = "&Entfernen";
            btnProgramRemove.UseVisualStyleBackColor = true;
            btnProgramRemove.Click += BtnProgramRemove_Click;
            //
            // btnProgramUp
            //
            btnProgramUp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnProgramUp.Location = new System.Drawing.Point(456, 84);
            btnProgramUp.Name = "btnProgramUp";
            btnProgramUp.Size = new System.Drawing.Size(138, 27);
            btnProgramUp.TabIndex = 3;
            btnProgramUp.Text = "Nach &oben";
            btnProgramUp.UseVisualStyleBackColor = true;
            btnProgramUp.Click += BtnProgramUp_Click;
            //
            // btnProgramDown
            //
            btnProgramDown.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnProgramDown.Location = new System.Drawing.Point(456, 117);
            btnProgramDown.Name = "btnProgramDown";
            btnProgramDown.Size = new System.Drawing.Size(138, 27);
            btnProgramDown.TabIndex = 4;
            btnProgramDown.Text = "Nach &unten";
            btnProgramDown.UseVisualStyleBackColor = true;
            btnProgramDown.Click += BtnProgramDown_Click;
            //
            // btnProgramDetect
            //
            btnProgramDetect.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnProgramDetect.Location = new System.Drawing.Point(456, 160);
            btnProgramDetect.Name = "btnProgramDetect";
            btnProgramDetect.Size = new System.Drawing.Size(138, 27);
            btnProgramDetect.TabIndex = 5;
            btnProgramDetect.Text = "Neu e&rkennen";
            btnProgramDetect.UseVisualStyleBackColor = true;
            btnProgramDetect.Click += BtnProgramDetect_Click;
            //
            // btnProgramSort
            //
            btnProgramSort.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnProgramSort.Location = new System.Drawing.Point(456, 193);
            btnProgramSort.Name = "btnProgramSort";
            btnProgramSort.Size = new System.Drawing.Size(138, 27);
            btnProgramSort.TabIndex = 8;
            btnProgramSort.Text = "&Alphabetisch sortieren";
            btnProgramSort.UseVisualStyleBackColor = true;
            btnProgramSort.Click += BtnProgramSort_Click;
            //
            // labelProgramStatus
            //
            labelProgramStatus.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            labelProgramStatus.Location = new System.Drawing.Point(8, 264);
            labelProgramStatus.Name = "labelProgramStatus";
            labelProgramStatus.Size = new System.Drawing.Size(440, 17);
            labelProgramStatus.TabIndex = 6;
            //
            // labelProgramHint
            //
            labelProgramHint.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            labelProgramHint.ForeColor = System.Drawing.SystemColors.GrayText;
            labelProgramHint.Location = new System.Drawing.Point(8, 288);
            labelProgramHint.Name = "labelProgramHint";
            labelProgramHint.Size = new System.Drawing.Size(586, 30);
            labelProgramHint.TabIndex = 7;
            labelProgramHint.Text = "Die Reihenfolge bestimmt die Tastenkürzel Strg+1 bis Strg+9 im Programme-Menü.";
            //
            // tabGeneral
            //
            tabGeneral.Controls.Add(cbJumpLastUsed);
            tabGeneral.Controls.Add(cbConfirmDelete);
            tabGeneral.Controls.Add(cbShowProgramIcons);
            tabGeneral.Controls.Add(cbToolbarIcons);
            tabGeneral.Controls.Add(cbLargeIcons);
            tabGeneral.Controls.Add(cbCloseOnEscape);
            tabGeneral.Controls.Add(cbReopenLast);
            tabGeneral.Controls.Add(btnClearRecent);
            tabGeneral.Location = new System.Drawing.Point(4, 24);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            tabGeneral.Size = new System.Drawing.Size(602, 322);
            tabGeneral.TabIndex = 2;
            tabGeneral.Text = "Allgemein";
            tabGeneral.UseVisualStyleBackColor = true;
            //
            // cbJumpLastUsed
            //
            cbJumpLastUsed.AutoSize = true;
            cbJumpLastUsed.Location = new System.Drawing.Point(16, 20);
            cbJumpLastUsed.Name = "cbJumpLastUsed";
            cbJumpLastUsed.Size = new System.Drawing.Size(320, 19);
            cbJumpLastUsed.TabIndex = 1;
            cbJumpLastUsed.Text = "Ordnerdialog springt zum zuletzt &verwendeten Ordner";
            cbJumpLastUsed.UseVisualStyleBackColor = true;
            //
            // cbConfirmDelete
            //
            cbConfirmDelete.AutoSize = true;
            cbConfirmDelete.Location = new System.Drawing.Point(16, 48);
            cbConfirmDelete.Name = "cbConfirmDelete";
            cbConfirmDelete.Size = new System.Drawing.Size(310, 19);
            cbConfirmDelete.TabIndex = 2;
            cbConfirmDelete.Text = "Vor dem Verschieben in den &Papierkorb nachfragen";
            cbConfirmDelete.UseVisualStyleBackColor = true;
            //
            // cbShowProgramIcons
            //
            cbShowProgramIcons.AutoSize = true;
            cbShowProgramIcons.Checked = true;
            cbShowProgramIcons.CheckState = System.Windows.Forms.CheckState.Checked;
            cbShowProgramIcons.Location = new System.Drawing.Point(16, 76);
            cbShowProgramIcons.Name = "cbShowProgramIcons";
            cbShowProgramIcons.Size = new System.Drawing.Size(330, 19);
            cbShowProgramIcons.TabIndex = 4;
            cbShowProgramIcons.Text = "Programm&symbole zusätzlich in der Symbolleiste anzeigen";
            cbShowProgramIcons.UseVisualStyleBackColor = true;
            //
            // cbToolbarIcons
            //
            cbToolbarIcons.AutoSize = true;
            cbToolbarIcons.Checked = true;
            cbToolbarIcons.CheckState = System.Windows.Forms.CheckState.Checked;
            cbToolbarIcons.Location = new System.Drawing.Point(16, 104);
            cbToolbarIcons.Name = "cbToolbarIcons";
            cbToolbarIcons.Size = new System.Drawing.Size(320, 19);
            cbToolbarIcons.TabIndex = 5;
            cbToolbarIcons.Text = "Symbole auf den Schal&tflächen der Symbolleiste anzeigen";
            cbToolbarIcons.UseVisualStyleBackColor = true;
            //
            // cbLargeIcons
            //
            cbLargeIcons.AutoSize = true;
            cbLargeIcons.Checked = true;
            cbLargeIcons.CheckState = System.Windows.Forms.CheckState.Checked;
            cbLargeIcons.Location = new System.Drawing.Point(16, 132);
            cbLargeIcons.Name = "cbLargeIcons";
            cbLargeIcons.Size = new System.Drawing.Size(280, 19);
            cbLargeIcons.TabIndex = 6;
            cbLargeIcons.Text = "&Große Symbole in der Symbolleiste (24 statt 16 Pixel)";
            cbLargeIcons.UseVisualStyleBackColor = true;
            //
            // cbCloseOnEscape
            //
            cbCloseOnEscape.AutoSize = true;
            cbCloseOnEscape.Location = new System.Drawing.Point(16, 160);
            cbCloseOnEscape.Name = "cbCloseOnEscape";
            cbCloseOnEscape.Size = new System.Drawing.Size(200, 19);
            cbCloseOnEscape.TabIndex = 7;
            cbCloseOnEscape.Text = "Programm mit &Esc beenden";
            cbCloseOnEscape.UseVisualStyleBackColor = true;
            //
            // cbReopenLast
            //
            cbReopenLast.AutoSize = true;
            cbReopenLast.Location = new System.Drawing.Point(16, 188);
            cbReopenLast.Name = "cbReopenLast";
            cbReopenLast.Size = new System.Drawing.Size(260, 19);
            cbReopenLast.TabIndex = 8;
            cbReopenLast.Text = "Zuletzt geöffnete &Datei beim Start laden";
            cbReopenLast.UseVisualStyleBackColor = true;
            //
            // btnClearRecent
            //
            btnClearRecent.Location = new System.Drawing.Point(16, 228);
            btnClearRecent.Name = "btnClearRecent";
            btnClearRecent.Size = new System.Drawing.Size(180, 27);
            btnClearRecent.TabIndex = 3;
            btnClearRecent.Text = "Zuletzt-Liste &leeren";
            btnClearRecent.UseVisualStyleBackColor = true;
            btnClearRecent.Click += BtnClearRecent_Click;
            //
            // buttonOK
            //
            buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOK.Location = new System.Drawing.Point(420, 372);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(95, 27);
            buttonOK.TabIndex = 1;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(527, 372);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(95, 27);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // SettingsForm
            //
            AcceptButton = buttonOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(634, 411);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(tabControl);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(560, 380);
            Name = "SettingsForm";
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Einstellungen";
            tabControl.ResumeLayout(false);
            tabTargets.ResumeLayout(false);
            tabPrograms.ResumeLayout(false);
            tabGeneral.ResumeLayout(false);
            tabGeneral.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
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
        private System.Windows.Forms.CheckBox cbShowProgramIcons;
        private System.Windows.Forms.CheckBox cbToolbarIcons;
        private System.Windows.Forms.CheckBox cbLargeIcons;
        private System.Windows.Forms.CheckBox cbCloseOnEscape;
        private System.Windows.Forms.CheckBox cbReopenLast;
        private System.Windows.Forms.Button btnClearRecent;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
