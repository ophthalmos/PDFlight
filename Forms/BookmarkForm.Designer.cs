namespace PDFLight.Forms
{
    partial class BookmarkForm
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
            treeView = new System.Windows.Forms.TreeView();
            labelPage = new System.Windows.Forms.Label();
            numPage = new System.Windows.Forms.NumericUpDown();
            buttonNew = new System.Windows.Forms.Button();
            buttonNewChild = new System.Windows.Forms.Button();
            buttonRename = new System.Windows.Forms.Button();
            buttonDelete = new System.Windows.Forms.Button();
            buttonUp = new System.Windows.Forms.Button();
            buttonDown = new System.Windows.Forms.Button();
            buttonOutdent = new System.Windows.Forms.Button();
            buttonIndent = new System.Windows.Forms.Button();
            labelHint = new System.Windows.Forms.Label();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)numPage).BeginInit();
            SuspendLayout();
            //
            // treeView
            //
            treeView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            treeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            treeView.HideSelection = false;
            treeView.LabelEdit = true;
            treeView.Location = new System.Drawing.Point(12, 12);
            treeView.Name = "treeView";
            treeView.ShowNodeToolTips = true;
            treeView.Size = new System.Drawing.Size(440, 400);
            treeView.TabIndex = 0;
            treeView.AfterLabelEdit += TreeView_AfterLabelEdit;
            treeView.BeforeLabelEdit += TreeView_BeforeLabelEdit;
            treeView.DrawNode += TreeView_DrawNode;
            treeView.AfterSelect += TreeView_AfterSelect;
            treeView.KeyDown += TreeView_KeyDown;
            //
            // labelPage
            //
            labelPage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            labelPage.AutoSize = true;
            labelPage.Location = new System.Drawing.Point(464, 12);
            labelPage.Name = "labelPage";
            labelPage.Size = new System.Drawing.Size(58, 15);
            labelPage.TabIndex = 1;
            labelPage.Text = "&Zielseite:";
            //
            // numPage
            //
            numPage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            numPage.Location = new System.Drawing.Point(464, 30);
            numPage.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPage.Name = "numPage";
            numPage.Size = new System.Drawing.Size(80, 23);
            numPage.TabIndex = 2;
            numPage.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numPage.ValueChanged += NumPage_ValueChanged;
            //
            // buttonNew
            //
            buttonNew.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonNew.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            buttonNew.Location = new System.Drawing.Point(464, 72);
            buttonNew.Name = "buttonNew";
            buttonNew.Size = new System.Drawing.Size(188, 27);
            buttonNew.TabIndex = 3;
            buttonNew.Text = "&Neu";
            buttonNew.UseVisualStyleBackColor = true;
            buttonNew.Click += ButtonNew_Click;
            //
            // buttonNewChild
            //
            buttonNewChild.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonNewChild.Location = new System.Drawing.Point(464, 105);
            buttonNewChild.Name = "buttonNewChild";
            buttonNewChild.Size = new System.Drawing.Size(188, 27);
            buttonNewChild.TabIndex = 4;
            buttonNewChild.Text = "Neuer &Unterpunkt";
            buttonNewChild.UseVisualStyleBackColor = true;
            buttonNewChild.Click += ButtonNewChild_Click;
            //
            // buttonRename
            //
            buttonRename.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonRename.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            buttonRename.Location = new System.Drawing.Point(464, 138);
            buttonRename.Name = "buttonRename";
            buttonRename.Size = new System.Drawing.Size(188, 27);
            buttonRename.TabIndex = 5;
            buttonRename.Text = "&Umbenennen";
            buttonRename.UseVisualStyleBackColor = true;
            buttonRename.Click += ButtonRename_Click;
            //
            // buttonDelete
            //
            buttonDelete.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            buttonDelete.Location = new System.Drawing.Point(464, 171);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new System.Drawing.Size(188, 27);
            buttonDelete.TabIndex = 6;
            buttonDelete.Text = "&Löschen";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += ButtonDelete_Click;
            //
            // buttonUp
            //
            buttonUp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonUp.Location = new System.Drawing.Point(464, 216);
            buttonUp.Name = "buttonUp";
            buttonUp.Size = new System.Drawing.Size(188, 27);
            buttonUp.TabIndex = 7;
            buttonUp.Text = "Nach &oben";
            buttonUp.UseVisualStyleBackColor = true;
            buttonUp.Click += ButtonUp_Click;
            //
            // buttonDown
            //
            buttonDown.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonDown.Location = new System.Drawing.Point(464, 249);
            buttonDown.Name = "buttonDown";
            buttonDown.Size = new System.Drawing.Size(188, 27);
            buttonDown.TabIndex = 8;
            buttonDown.Text = "Nach &unten";
            buttonDown.UseVisualStyleBackColor = true;
            buttonDown.Click += ButtonDown_Click;
            //
            // buttonOutdent
            //
            buttonOutdent.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonOutdent.Location = new System.Drawing.Point(464, 282);
            buttonOutdent.Name = "buttonOutdent";
            buttonOutdent.Size = new System.Drawing.Size(188, 27);
            buttonOutdent.TabIndex = 9;
            buttonOutdent.Text = "Ebene &höher";
            buttonOutdent.UseVisualStyleBackColor = true;
            buttonOutdent.Click += ButtonOutdent_Click;
            //
            // buttonIndent
            //
            buttonIndent.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonIndent.Location = new System.Drawing.Point(464, 315);
            buttonIndent.Name = "buttonIndent";
            buttonIndent.Size = new System.Drawing.Size(188, 27);
            buttonIndent.TabIndex = 10;
            buttonIndent.Text = "Ebene &tiefer";
            buttonIndent.UseVisualStyleBackColor = true;
            buttonIndent.Click += ButtonIndent_Click;
            //
            // labelHint
            //
            labelHint.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            labelHint.AutoSize = true;
            labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            labelHint.Location = new System.Drawing.Point(12, 420);
            labelHint.Name = "labelHint";
            labelHint.Size = new System.Drawing.Size(500, 15);
            labelHint.TabIndex = 11;
            labelHint.Text = "Experimentelle Funktion – F2 benennt um, Entf löscht, Einfg legt einen Eintrag an; Strg+Z macht das Speichern rückgängig.";
            //
            // buttonOK
            //
            buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonOK.Location = new System.Drawing.Point(456, 442);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(95, 27);
            buttonOK.TabIndex = 12;
            buttonOK.Text = "&Speichern";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(557, 442);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(95, 27);
            buttonCancel.TabIndex = 13;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // BookmarkForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(664, 481);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(labelHint);
            Controls.Add(buttonIndent);
            Controls.Add(buttonOutdent);
            Controls.Add(buttonDown);
            Controls.Add(buttonUp);
            Controls.Add(buttonDelete);
            Controls.Add(buttonRename);
            Controls.Add(buttonNewChild);
            Controls.Add(buttonNew);
            Controls.Add(numPage);
            Controls.Add(labelPage);
            Controls.Add(treeView);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(560, 400);
            Name = "BookmarkForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Lesezeichen bearbeiten";
            ((System.ComponentModel.ISupportInitialize)numPage).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.Label labelPage;
        private System.Windows.Forms.NumericUpDown numPage;
        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.Button buttonNewChild;
        private System.Windows.Forms.Button buttonRename;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonUp;
        private System.Windows.Forms.Button buttonDown;
        private System.Windows.Forms.Button buttonOutdent;
        private System.Windows.Forms.Button buttonIndent;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
