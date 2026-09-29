namespace PDFLight.Forms
{
    partial class InsertPageForm
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
            groupFormat = new System.Windows.Forms.GroupBox();
            radioLikePage = new System.Windows.Forms.RadioButton();
            radioA4 = new System.Windows.Forms.RadioButton();
            radioLetter = new System.Windows.Forms.RadioButton();
            comboOrientation = new System.Windows.Forms.ComboBox();
            radioFromImage = new System.Windows.Forms.RadioButton();
            groupPosition = new System.Windows.Forms.GroupBox();
            radioAfter = new System.Windows.Forms.RadioButton();
            radioBefore = new System.Windows.Forms.RadioButton();
            radioFirst = new System.Windows.Forms.RadioButton();
            radioLast = new System.Windows.Forms.RadioButton();
            groupImage = new System.Windows.Forms.GroupBox();
            textBoxImage = new System.Windows.Forms.TextBox();
            buttonBrowse = new System.Windows.Forms.Button();
            radioOriginal = new System.Windows.Forms.RadioButton();
            comboDpi = new System.Windows.Forms.ComboBox();
            labelDpi = new System.Windows.Forms.Label();
            radioFit = new System.Windows.Forms.RadioButton();
            labelImageSize = new System.Windows.Forms.Label();
            labelDpiHint = new System.Windows.Forms.Label();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            groupFormat.SuspendLayout();
            groupPosition.SuspendLayout();
            groupImage.SuspendLayout();
            SuspendLayout();
            //
            // groupFormat
            //
            groupFormat.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupFormat.Controls.Add(radioLikePage);
            groupFormat.Controls.Add(radioA4);
            groupFormat.Controls.Add(radioLetter);
            groupFormat.Controls.Add(comboOrientation);
            groupFormat.Controls.Add(radioFromImage);
            groupFormat.Location = new System.Drawing.Point(12, 12);
            groupFormat.Name = "groupFormat";
            groupFormat.Size = new System.Drawing.Size(416, 104);
            groupFormat.TabIndex = 0;
            groupFormat.TabStop = false;
            groupFormat.Text = "Format";
            //
            // radioLikePage
            //
            radioLikePage.AutoSize = true;
            radioLikePage.Checked = true;
            radioLikePage.Location = new System.Drawing.Point(12, 22);
            radioLikePage.Name = "radioLikePage";
            radioLikePage.Size = new System.Drawing.Size(250, 19);
            radioLikePage.TabIndex = 0;
            radioLikePage.TabStop = true;
            radioLikePage.Text = "&Wie Seite {0}: {1}";
            radioLikePage.UseVisualStyleBackColor = true;
            radioLikePage.CheckedChanged += Option_CheckedChanged;
            //
            // radioA4
            //
            radioA4.AutoSize = true;
            radioA4.Location = new System.Drawing.Point(12, 48);
            radioA4.Name = "radioA4";
            radioA4.Size = new System.Drawing.Size(65, 19);
            radioA4.TabIndex = 1;
            radioA4.Text = "DIN A&4";
            radioA4.UseVisualStyleBackColor = true;
            radioA4.CheckedChanged += Option_CheckedChanged;
            //
            // radioLetter
            //
            radioLetter.AutoSize = true;
            radioLetter.Location = new System.Drawing.Point(110, 48);
            radioLetter.Name = "radioLetter";
            radioLetter.Size = new System.Drawing.Size(80, 19);
            radioLetter.TabIndex = 2;
            radioLetter.Text = "US &Letter";
            radioLetter.UseVisualStyleBackColor = true;
            radioLetter.CheckedChanged += Option_CheckedChanged;
            //
            // comboOrientation
            //
            comboOrientation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboOrientation.Enabled = false;
            comboOrientation.Location = new System.Drawing.Point(220, 46);
            comboOrientation.Name = "comboOrientation";
            comboOrientation.Size = new System.Drawing.Size(140, 23);
            comboOrientation.TabIndex = 3;
            //
            // radioFromImage
            //
            radioFromImage.AutoSize = true;
            radioFromImage.Enabled = false;
            radioFromImage.Location = new System.Drawing.Point(12, 74);
            radioFromImage.Name = "radioFromImage";
            radioFromImage.Size = new System.Drawing.Size(170, 19);
            radioFromImage.TabIndex = 4;
            radioFromImage.Text = "Wie das &Bild (ohne Rand)";
            radioFromImage.UseVisualStyleBackColor = true;
            radioFromImage.CheckedChanged += Option_CheckedChanged;
            //
            // groupPosition
            //
            groupPosition.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupPosition.Controls.Add(radioAfter);
            groupPosition.Controls.Add(radioBefore);
            groupPosition.Controls.Add(radioFirst);
            groupPosition.Controls.Add(radioLast);
            groupPosition.Location = new System.Drawing.Point(12, 124);
            groupPosition.Name = "groupPosition";
            groupPosition.Size = new System.Drawing.Size(416, 78);
            groupPosition.TabIndex = 1;
            groupPosition.TabStop = false;
            groupPosition.Text = "Position";
            //
            // radioAfter
            //
            radioAfter.AutoSize = true;
            radioAfter.Checked = true;
            radioAfter.Location = new System.Drawing.Point(12, 22);
            radioAfter.Name = "radioAfter";
            radioAfter.Size = new System.Drawing.Size(110, 19);
            radioAfter.TabIndex = 0;
            radioAfter.TabStop = true;
            radioAfter.Text = "&Nach Seite {0}";
            radioAfter.UseVisualStyleBackColor = true;
            //
            // radioBefore
            //
            radioBefore.AutoSize = true;
            radioBefore.Location = new System.Drawing.Point(220, 22);
            radioBefore.Name = "radioBefore";
            radioBefore.Size = new System.Drawing.Size(100, 19);
            radioBefore.TabIndex = 1;
            radioBefore.Text = "&Vor Seite {0}";
            radioBefore.UseVisualStyleBackColor = true;
            //
            // radioFirst
            //
            radioFirst.AutoSize = true;
            radioFirst.Location = new System.Drawing.Point(12, 48);
            radioFirst.Name = "radioFirst";
            radioFirst.Size = new System.Drawing.Size(85, 19);
            radioFirst.TabIndex = 2;
            radioFirst.Text = "Am &Anfang";
            radioFirst.UseVisualStyleBackColor = true;
            //
            // radioLast
            //
            radioLast.AutoSize = true;
            radioLast.Location = new System.Drawing.Point(220, 48);
            radioLast.Name = "radioLast";
            radioLast.Size = new System.Drawing.Size(70, 19);
            radioLast.TabIndex = 3;
            radioLast.Text = "Am &Ende";
            radioLast.UseVisualStyleBackColor = true;
            //
            // groupImage
            //
            groupImage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupImage.Controls.Add(textBoxImage);
            groupImage.Controls.Add(buttonBrowse);
            groupImage.Controls.Add(radioOriginal);
            groupImage.Controls.Add(comboDpi);
            groupImage.Controls.Add(labelDpi);
            groupImage.Controls.Add(radioFit);
            groupImage.Controls.Add(labelImageSize);
            groupImage.Controls.Add(labelDpiHint);
            groupImage.Location = new System.Drawing.Point(12, 210);
            groupImage.Name = "groupImage";
            groupImage.Size = new System.Drawing.Size(416, 150);
            groupImage.TabIndex = 2;
            groupImage.TabStop = false;
            groupImage.Text = "Bild (optional)";
            //
            // textBoxImage
            //
            textBoxImage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxImage.Location = new System.Drawing.Point(12, 24);
            textBoxImage.Name = "textBoxImage";
            textBoxImage.Size = new System.Drawing.Size(270, 23);
            textBoxImage.TabIndex = 0;
            textBoxImage.TextChanged += TextBoxImage_TextChanged;
            //
            // buttonBrowse
            //
            buttonBrowse.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonBrowse.Location = new System.Drawing.Point(288, 22);
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.Size = new System.Drawing.Size(116, 27);
            buttonBrowse.TabIndex = 1;
            buttonBrowse.Text = "&Durchsuchen …";
            buttonBrowse.UseVisualStyleBackColor = true;
            buttonBrowse.Click += ButtonBrowse_Click;
            //
            // radioOriginal
            //
            radioOriginal.AutoSize = true;
            radioOriginal.Checked = true;
            radioOriginal.Enabled = false;
            radioOriginal.Location = new System.Drawing.Point(12, 58);
            radioOriginal.Name = "radioOriginal";
            radioOriginal.Size = new System.Drawing.Size(120, 19);
            radioOriginal.TabIndex = 2;
            radioOriginal.TabStop = true;
            radioOriginal.Text = "&Originalgröße bei";
            radioOriginal.UseVisualStyleBackColor = true;
            radioOriginal.CheckedChanged += Option_CheckedChanged;
            //
            // comboDpi
            //
            comboDpi.Enabled = false;
            comboDpi.Items.AddRange(new object[] { "72", "96", "150", "200", "300", "600" });
            comboDpi.Location = new System.Drawing.Point(150, 56);
            comboDpi.Name = "comboDpi";
            comboDpi.Size = new System.Drawing.Size(64, 23);
            comboDpi.TabIndex = 3;
            comboDpi.Text = "300";
            comboDpi.TextChanged += ComboDpi_TextChanged;
            //
            // labelDpi
            //
            labelDpi.AutoSize = true;
            labelDpi.Enabled = false;
            labelDpi.Location = new System.Drawing.Point(220, 60);
            labelDpi.Name = "labelDpi";
            labelDpi.Size = new System.Drawing.Size(24, 15);
            labelDpi.TabIndex = 4;
            labelDpi.Text = "dpi";
            //
            // radioFit
            //
            radioFit.AutoSize = true;
            radioFit.Enabled = false;
            radioFit.Location = new System.Drawing.Point(12, 84);
            radioFit.Name = "radioFit";
            radioFit.Size = new System.Drawing.Size(230, 19);
            radioFit.TabIndex = 5;
            radioFit.Text = "An die Seite an&passen (10 mm Rand)";
            radioFit.UseVisualStyleBackColor = true;
            radioFit.CheckedChanged += Option_CheckedChanged;
            //
            // labelImageSize
            //
            labelImageSize.AutoSize = true;
            labelImageSize.ForeColor = System.Drawing.SystemColors.GrayText;
            labelImageSize.Location = new System.Drawing.Point(12, 108);
            labelImageSize.Name = "labelImageSize";
            labelImageSize.Size = new System.Drawing.Size(0, 15);
            labelImageSize.TabIndex = 6;
            //
            // labelDpiHint
            //
            labelDpiHint.AutoSize = true;
            labelDpiHint.Enabled = false;
            labelDpiHint.ForeColor = System.Drawing.SystemColors.GrayText;
            labelDpiHint.Location = new System.Drawing.Point(12, 126);
            labelDpiHint.Name = "labelDpiHint";
            labelDpiHint.Size = new System.Drawing.Size(360, 15);
            labelDpiHint.TabIndex = 7;
            labelDpiHint.Text = "Mehr dpi = kleineres Bild. Scans meist 300 dpi, Screenshots 96 dpi.";
            //
            // buttonOK
            //
            buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonOK.Location = new System.Drawing.Point(232, 372);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(95, 27);
            buttonOK.TabIndex = 3;
            buttonOK.Text = "Einfügen";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(333, 372);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(95, 27);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // InsertPageForm
            //
            AcceptButton = buttonOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(440, 411);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(groupImage);
            Controls.Add(groupPosition);
            Controls.Add(groupFormat);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InsertPageForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Leere Seite einfügen";
            groupFormat.ResumeLayout(false);
            groupFormat.PerformLayout();
            groupPosition.ResumeLayout(false);
            groupPosition.PerformLayout();
            groupImage.ResumeLayout(false);
            groupImage.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupFormat;
        private System.Windows.Forms.RadioButton radioLikePage;
        private System.Windows.Forms.RadioButton radioA4;
        private System.Windows.Forms.RadioButton radioLetter;
        private System.Windows.Forms.ComboBox comboOrientation;
        private System.Windows.Forms.RadioButton radioFromImage;
        private System.Windows.Forms.GroupBox groupPosition;
        private System.Windows.Forms.RadioButton radioAfter;
        private System.Windows.Forms.RadioButton radioBefore;
        private System.Windows.Forms.RadioButton radioFirst;
        private System.Windows.Forms.RadioButton radioLast;
        private System.Windows.Forms.GroupBox groupImage;
        private System.Windows.Forms.TextBox textBoxImage;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.RadioButton radioOriginal;
        private System.Windows.Forms.ComboBox comboDpi;
        private System.Windows.Forms.Label labelDpi;
        private System.Windows.Forms.RadioButton radioFit;
        private System.Windows.Forms.Label labelImageSize;
        private System.Windows.Forms.Label labelDpiHint;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
