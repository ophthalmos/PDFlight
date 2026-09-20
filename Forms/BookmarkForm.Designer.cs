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
            labelTitle = new Label();
            textBoxTitle = new TextBox();
            labelPage = new Label();
            numPage = new NumericUpDown();
            buttonNew = new Button();
            buttonNewChild = new Button();
            buttonDelete = new Button();
            buttonUp = new Button();
            buttonDown = new Button();
            buttonOutdent = new Button();
            buttonIndent = new Button();
            labelExpand = new Label();
            buttonLevel1 = new Button();
            buttonLevel2 = new Button();
            buttonLevel3 = new Button();
            buttonLevelAll = new Button();
            buttonOK = new Button();
            buttonCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)numPage).BeginInit();
            SuspendLayout();
            // 
            // treeView
            // 
            treeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeView.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            treeView.FullRowSelect = true;
            treeView.HideSelection = false;
            treeView.Location = new Point(12, 12);
            treeView.Name = "treeView";
            treeView.ShowLines = false;
            treeView.ShowNodeToolTips = true;
            treeView.Size = new Size(440, 447);
            treeView.TabIndex = 0;
            treeView.DrawNode += TreeView_DrawNode;
            treeView.AfterSelect += TreeView_AfterSelect;
            treeView.KeyDown += TreeView_KeyDown;
            // 
            // labelTitle
            // 
            labelTitle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelTitle.AutoSize = true;
            labelTitle.Location = new Point(464, 12);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(33, 15);
            labelTitle.TabIndex = 1;
            labelTitle.Text = "&Titel:";
            // 
            // textBoxTitle
            // 
            textBoxTitle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textBoxTitle.Location = new Point(464, 30);
            textBoxTitle.Name = "textBoxTitle";
            textBoxTitle.Size = new Size(188, 23);
            textBoxTitle.TabIndex = 2;
            textBoxTitle.TextChanged += TextBoxTitle_TextChanged;
            // 
            // labelPage
            // 
            labelPage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelPage.AutoSize = true;
            labelPage.Location = new Point(464, 62);
            labelPage.Name = "labelPage";
            labelPage.Size = new Size(53, 15);
            labelPage.TabIndex = 3;
            labelPage.Text = "&Zielseite:";
            // 
            // numPage
            // 
            numPage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            numPage.Location = new Point(464, 80);
            numPage.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPage.Name = "numPage";
            numPage.Size = new Size(80, 23);
            numPage.TabIndex = 4;
            numPage.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numPage.ValueChanged += NumPage_ValueChanged;
            // 
            // buttonNew
            // 
            buttonNew.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonNew.ImageAlign = ContentAlignment.MiddleLeft;
            buttonNew.Location = new Point(464, 118);
            buttonNew.Name = "buttonNew";
            buttonNew.Size = new Size(188, 27);
            buttonNew.TabIndex = 5;
            buttonNew.Text = "&Neu";
            buttonNew.UseVisualStyleBackColor = true;
            buttonNew.Click += ButtonNew_Click;
            // 
            // buttonNewChild
            // 
            buttonNewChild.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonNewChild.Location = new Point(464, 151);
            buttonNewChild.Name = "buttonNewChild";
            buttonNewChild.Size = new Size(188, 27);
            buttonNewChild.TabIndex = 6;
            buttonNewChild.Text = "Neuer &Unterpunkt";
            buttonNewChild.UseVisualStyleBackColor = true;
            buttonNewChild.Click += ButtonNewChild_Click;
            // 
            // buttonDelete
            // 
            buttonDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonDelete.ImageAlign = ContentAlignment.MiddleLeft;
            buttonDelete.Location = new Point(464, 184);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(188, 27);
            buttonDelete.TabIndex = 7;
            buttonDelete.Text = "&Löschen";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += ButtonDelete_Click;
            // 
            // buttonUp
            // 
            buttonUp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonUp.Location = new Point(464, 229);
            buttonUp.Name = "buttonUp";
            buttonUp.Size = new Size(188, 27);
            buttonUp.TabIndex = 8;
            buttonUp.Text = "Nach &oben";
            buttonUp.UseVisualStyleBackColor = true;
            buttonUp.Click += ButtonUp_Click;
            // 
            // buttonDown
            // 
            buttonDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonDown.Location = new Point(464, 262);
            buttonDown.Name = "buttonDown";
            buttonDown.Size = new Size(188, 27);
            buttonDown.TabIndex = 9;
            buttonDown.Text = "Nach &unten";
            buttonDown.UseVisualStyleBackColor = true;
            buttonDown.Click += ButtonDown_Click;
            // 
            // buttonOutdent
            // 
            buttonOutdent.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonOutdent.Location = new Point(464, 295);
            buttonOutdent.Name = "buttonOutdent";
            buttonOutdent.Size = new Size(188, 27);
            buttonOutdent.TabIndex = 10;
            buttonOutdent.Text = "Ebene &höher";
            buttonOutdent.UseVisualStyleBackColor = true;
            buttonOutdent.Click += ButtonOutdent_Click;
            // 
            // buttonIndent
            // 
            buttonIndent.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonIndent.Location = new Point(464, 328);
            buttonIndent.Name = "buttonIndent";
            buttonIndent.Size = new Size(188, 27);
            buttonIndent.TabIndex = 11;
            buttonIndent.Text = "Ebene &tiefer";
            buttonIndent.UseVisualStyleBackColor = true;
            buttonIndent.Click += ButtonIndent_Click;
            // 
            // labelExpand
            // 
            labelExpand.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelExpand.AutoSize = true;
            labelExpand.Location = new Point(464, 370);
            labelExpand.Name = "labelExpand";
            labelExpand.Size = new Size(124, 15);
            labelExpand.TabIndex = 12;
            labelExpand.Text = "Aufklappen bis Ebene:";
            // 
            // buttonLevel1
            // 
            buttonLevel1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLevel1.Location = new Point(464, 388);
            buttonLevel1.Name = "buttonLevel1";
            buttonLevel1.Size = new Size(44, 27);
            buttonLevel1.TabIndex = 13;
            buttonLevel1.Text = "&1";
            buttonLevel1.UseVisualStyleBackColor = true;
            buttonLevel1.Click += ButtonLevel1_Click;
            // 
            // buttonLevel2
            // 
            buttonLevel2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLevel2.Location = new Point(512, 388);
            buttonLevel2.Name = "buttonLevel2";
            buttonLevel2.Size = new Size(44, 27);
            buttonLevel2.TabIndex = 14;
            buttonLevel2.Text = "&2";
            buttonLevel2.UseVisualStyleBackColor = true;
            buttonLevel2.Click += ButtonLevel2_Click;
            // 
            // buttonLevel3
            // 
            buttonLevel3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLevel3.Location = new Point(560, 388);
            buttonLevel3.Name = "buttonLevel3";
            buttonLevel3.Size = new Size(44, 27);
            buttonLevel3.TabIndex = 15;
            buttonLevel3.Text = "&3";
            buttonLevel3.UseVisualStyleBackColor = true;
            buttonLevel3.Click += ButtonLevel3_Click;
            // 
            // buttonLevelAll
            // 
            buttonLevelAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLevelAll.Location = new Point(608, 388);
            buttonLevelAll.Name = "buttonLevelAll";
            buttonLevelAll.Size = new Size(44, 27);
            buttonLevelAll.TabIndex = 16;
            buttonLevelAll.Text = "&Alle";
            buttonLevelAll.UseVisualStyleBackColor = true;
            buttonLevelAll.Click += ButtonLevelAll_Click;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.Location = new Point(458, 432);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(96, 27);
            buttonOK.TabIndex = 18;
            buttonOK.Text = "&Speichern";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(560, 432);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(92, 27);
            buttonCancel.TabIndex = 19;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // BookmarkForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(664, 471);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
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
            MinimumSize = new Size(560, 400);
            Name = "BookmarkForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Lesezeichen bearbeiten (Entf löscht, Einfg legt einen Eintrag an, F2 springt zum Titel)";
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
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
