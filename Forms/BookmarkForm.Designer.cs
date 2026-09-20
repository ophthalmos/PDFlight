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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(BookmarkForm));
            treeView = new PDFLight.Controls.BookmarkTreeView();
            labelTitle = new System.Windows.Forms.Label();
            textBoxTitle = new System.Windows.Forms.TextBox();
            labelPage = new System.Windows.Forms.Label();
            numPage = new System.Windows.Forms.NumericUpDown();
            buttonNew = new System.Windows.Forms.Button();
            buttonNewChild = new System.Windows.Forms.Button();
            buttonDelete = new System.Windows.Forms.Button();
            buttonUp = new System.Windows.Forms.Button();
            buttonDown = new System.Windows.Forms.Button();
            buttonOutdent = new System.Windows.Forms.Button();
            buttonIndent = new System.Windows.Forms.Button();
            labelExpand = new System.Windows.Forms.Label();
            buttonLevel1 = new System.Windows.Forms.Button();
            buttonLevel2 = new System.Windows.Forms.Button();
            buttonLevel3 = new System.Windows.Forms.Button();
            buttonLevelAll = new System.Windows.Forms.Button();
            labelHint = new System.Windows.Forms.Label();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)numPage).BeginInit();
            SuspendLayout();
            //
            // treeView
            //
            treeView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            treeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
            treeView.FullRowSelect = true;
            treeView.HideSelection = false;
            treeView.Location = new System.Drawing.Point(12, 12);
            treeView.Name = "treeView";
            treeView.ShowLines = false;
            treeView.ShowNodeToolTips = true;
            treeView.Size = new System.Drawing.Size(440, 400);
            treeView.TabIndex = 0;
            treeView.DrawNode += TreeView_DrawNode;
            treeView.AfterSelect += TreeView_AfterSelect;
            treeView.KeyDown += TreeView_KeyDown;
            //
            // labelTitle
            //
            labelTitle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            labelTitle.AutoSize = true;
            labelTitle.Location = new System.Drawing.Point(464, 12);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new System.Drawing.Size(34, 15);
            labelTitle.TabIndex = 1;
            labelTitle.Text = "&Titel:";
            //
            // textBoxTitle
            //
            textBoxTitle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            textBoxTitle.Location = new System.Drawing.Point(464, 30);
            textBoxTitle.Name = "textBoxTitle";
            textBoxTitle.Size = new System.Drawing.Size(188, 23);
            textBoxTitle.TabIndex = 2;
            textBoxTitle.TextChanged += TextBoxTitle_TextChanged;
            //
            // labelPage
            //
            labelPage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            labelPage.AutoSize = true;
            labelPage.Location = new System.Drawing.Point(464, 62);
            labelPage.Name = "labelPage";
            labelPage.Size = new System.Drawing.Size(58, 15);
            labelPage.TabIndex = 3;
            labelPage.Text = "&Zielseite:";
            //
            // numPage
            //
            numPage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            numPage.Location = new System.Drawing.Point(464, 80);
            numPage.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPage.Name = "numPage";
            numPage.Size = new System.Drawing.Size(80, 23);
            numPage.TabIndex = 4;
            numPage.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numPage.ValueChanged += NumPage_ValueChanged;
            //
            // buttonNew
            //
            buttonNew.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonNew.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            buttonNew.Location = new System.Drawing.Point(464, 118);
            buttonNew.Name = "buttonNew";
            buttonNew.Size = new System.Drawing.Size(188, 27);
            buttonNew.TabIndex = 5;
            buttonNew.Text = "&Neu";
            buttonNew.UseVisualStyleBackColor = true;
            buttonNew.Click += ButtonNew_Click;
            //
            // buttonNewChild
            //
            buttonNewChild.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonNewChild.Location = new System.Drawing.Point(464, 151);
            buttonNewChild.Name = "buttonNewChild";
            buttonNewChild.Size = new System.Drawing.Size(188, 27);
            buttonNewChild.TabIndex = 6;
            buttonNewChild.Text = "Neuer &Unterpunkt";
            buttonNewChild.UseVisualStyleBackColor = true;
            buttonNewChild.Click += ButtonNewChild_Click;
            //
            // buttonDelete
            //
            buttonDelete.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            buttonDelete.Location = new System.Drawing.Point(464, 184);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new System.Drawing.Size(188, 27);
            buttonDelete.TabIndex = 7;
            buttonDelete.Text = "&Löschen";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += ButtonDelete_Click;
            //
            // buttonUp
            //
            buttonUp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonUp.Location = new System.Drawing.Point(464, 229);
            buttonUp.Name = "buttonUp";
            buttonUp.Size = new System.Drawing.Size(188, 27);
            buttonUp.TabIndex = 8;
            buttonUp.Text = "Nach &oben";
            buttonUp.UseVisualStyleBackColor = true;
            buttonUp.Click += ButtonUp_Click;
            //
            // buttonDown
            //
            buttonDown.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonDown.Location = new System.Drawing.Point(464, 262);
            buttonDown.Name = "buttonDown";
            buttonDown.Size = new System.Drawing.Size(188, 27);
            buttonDown.TabIndex = 9;
            buttonDown.Text = "Nach &unten";
            buttonDown.UseVisualStyleBackColor = true;
            buttonDown.Click += ButtonDown_Click;
            //
            // buttonOutdent
            //
            buttonOutdent.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonOutdent.Location = new System.Drawing.Point(464, 295);
            buttonOutdent.Name = "buttonOutdent";
            buttonOutdent.Size = new System.Drawing.Size(188, 27);
            buttonOutdent.TabIndex = 10;
            buttonOutdent.Text = "Ebene &höher";
            buttonOutdent.UseVisualStyleBackColor = true;
            buttonOutdent.Click += ButtonOutdent_Click;
            //
            // buttonIndent
            //
            buttonIndent.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonIndent.Location = new System.Drawing.Point(464, 328);
            buttonIndent.Name = "buttonIndent";
            buttonIndent.Size = new System.Drawing.Size(188, 27);
            buttonIndent.TabIndex = 11;
            buttonIndent.Text = "Ebene &tiefer";
            buttonIndent.UseVisualStyleBackColor = true;
            buttonIndent.Click += ButtonIndent_Click;
            //
            // labelExpand
            //
            labelExpand.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            labelExpand.AutoSize = true;
            labelExpand.Location = new System.Drawing.Point(464, 370);
            labelExpand.Name = "labelExpand";
            labelExpand.Size = new System.Drawing.Size(125, 15);
            labelExpand.TabIndex = 12;
            labelExpand.Text = "Aufklappen bis Ebene:";
            //
            // buttonLevel1
            //
            buttonLevel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonLevel1.Location = new System.Drawing.Point(464, 388);
            buttonLevel1.Name = "buttonLevel1";
            buttonLevel1.Size = new System.Drawing.Size(44, 27);
            buttonLevel1.TabIndex = 13;
            buttonLevel1.Text = "&1";
            buttonLevel1.UseVisualStyleBackColor = true;
            buttonLevel1.Click += ButtonLevel1_Click;
            //
            // buttonLevel2
            //
            buttonLevel2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonLevel2.Location = new System.Drawing.Point(512, 388);
            buttonLevel2.Name = "buttonLevel2";
            buttonLevel2.Size = new System.Drawing.Size(44, 27);
            buttonLevel2.TabIndex = 14;
            buttonLevel2.Text = "&2";
            buttonLevel2.UseVisualStyleBackColor = true;
            buttonLevel2.Click += ButtonLevel2_Click;
            //
            // buttonLevel3
            //
            buttonLevel3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonLevel3.Location = new System.Drawing.Point(560, 388);
            buttonLevel3.Name = "buttonLevel3";
            buttonLevel3.Size = new System.Drawing.Size(44, 27);
            buttonLevel3.TabIndex = 15;
            buttonLevel3.Text = "&3";
            buttonLevel3.UseVisualStyleBackColor = true;
            buttonLevel3.Click += ButtonLevel3_Click;
            //
            // buttonLevelAll
            //
            buttonLevelAll.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonLevelAll.Location = new System.Drawing.Point(608, 388);
            buttonLevelAll.Name = "buttonLevelAll";
            buttonLevelAll.Size = new System.Drawing.Size(44, 27);
            buttonLevelAll.TabIndex = 16;
            buttonLevelAll.Text = "&Alle";
            buttonLevelAll.UseVisualStyleBackColor = true;
            buttonLevelAll.Click += ButtonLevelAll_Click;
            //
            // labelHint
            //
            labelHint.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            labelHint.AutoSize = true;
            labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            labelHint.Location = new System.Drawing.Point(12, 420);
            labelHint.Name = "labelHint";
            labelHint.Size = new System.Drawing.Size(450, 15);
            labelHint.TabIndex = 17;
            labelHint.Text = "Entf löscht, Einfg legt einen Eintrag an, F2 springt zum Titel; Strg+Z macht das Speichern rückgängig.";
            //
            // buttonOK
            //
            buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonOK.Location = new System.Drawing.Point(456, 442);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(95, 27);
            buttonOK.TabIndex = 18;
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
            buttonCancel.TabIndex = 19;
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
            Controls.Add(buttonLevelAll);
            Controls.Add(buttonLevel3);
            Controls.Add(buttonLevel2);
            Controls.Add(buttonLevel1);
            Controls.Add(labelExpand);
            Controls.Add(buttonIndent);
            Controls.Add(buttonOutdent);
            Controls.Add(buttonDown);
            Controls.Add(buttonUp);
            Controls.Add(buttonDelete);
            Controls.Add(buttonNewChild);
            Controls.Add(buttonNew);
            Controls.Add(numPage);
            Controls.Add(labelPage);
            Controls.Add(textBoxTitle);
            Controls.Add(labelTitle);
            Controls.Add(treeView);
            Icon = (Icon)resources.GetObject("$this.Icon");
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

        private PDFLight.Controls.BookmarkTreeView treeView;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.TextBox textBoxTitle;
        private System.Windows.Forms.Label labelPage;
        private System.Windows.Forms.NumericUpDown numPage;
        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.Button buttonNewChild;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonUp;
        private System.Windows.Forms.Button buttonDown;
        private System.Windows.Forms.Button buttonOutdent;
        private System.Windows.Forms.Button buttonIndent;
        private System.Windows.Forms.Label labelExpand;
        private System.Windows.Forms.Button buttonLevel1;
        private System.Windows.Forms.Button buttonLevel2;
        private System.Windows.Forms.Button buttonLevel3;
        private System.Windows.Forms.Button buttonLevelAll;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
