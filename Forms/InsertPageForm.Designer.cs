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
            groupFormat = new GroupBox();
            radioLikePage = new RadioButton();
            radioA4 = new RadioButton();
            radioLetter = new RadioButton();
            comboOrientation = new ComboBox();
            radioFromImage = new RadioButton();
            groupPosition = new GroupBox();
            radioAfter = new RadioButton();
            radioBefore = new RadioButton();
            radioFirst = new RadioButton();
            radioLast = new RadioButton();
            groupImage = new GroupBox();
            textBoxImage = new TextBox();
            buttonBrowse = new Button();
            radioOriginal = new RadioButton();
            comboDpi = new ComboBox();
            labelDpi = new Label();
            radioFit = new RadioButton();
            labelImageSize = new Label();
            labelDpiHint = new Label();
            labelDpiSource = new Label();
            pictureBoxPreview = new PictureBox();
            labelPreviewPage = new Label();
            buttonOK = new Button();
            buttonCancel = new Button();
            groupFormat.SuspendLayout();
            groupPosition.SuspendLayout();
            groupImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            SuspendLayout();
            // 
            // groupFormat
            // 
            groupFormat.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupFormat.Controls.Add(radioLikePage);
            groupFormat.Controls.Add(radioA4);
            groupFormat.Controls.Add(radioLetter);
            groupFormat.Controls.Add(comboOrientation);
            groupFormat.Controls.Add(radioFromImage);
            groupFormat.Location = new Point(12, 12);
            groupFormat.Name = "groupFormat";
            groupFormat.Size = new Size(281, 104);
            groupFormat.TabIndex = 0;
            groupFormat.TabStop = false;
            groupFormat.Text = "Format";
            // 
            // radioLikePage
            // 
            radioLikePage.AutoSize = true;
            radioLikePage.Checked = true;
            radioLikePage.Location = new Point(12, 22);
            radioLikePage.Name = "radioLikePage";
            radioLikePage.Size = new Size(110, 19);
            radioLikePage.TabIndex = 0;
            radioLikePage.TabStop = true;
            radioLikePage.Text = "&Wie Seite {0}: {1}";
            radioLikePage.UseVisualStyleBackColor = true;
            radioLikePage.CheckedChanged += Option_CheckedChanged;
            // 
            // radioA4
            // 
            radioA4.AutoSize = true;
            radioA4.Location = new Point(12, 48);
            radioA4.Name = "radioA4";
            radioA4.Size = new Size(62, 19);
            radioA4.TabIndex = 1;
            radioA4.Text = "DIN A&4";
            radioA4.UseVisualStyleBackColor = true;
            radioA4.CheckedChanged += Option_CheckedChanged;
            // 
            // radioLetter
            // 
            radioLetter.AutoSize = true;
            radioLetter.Location = new Point(85, 48);
            radioLetter.Name = "radioLetter";
            radioLetter.Size = new Size(72, 19);
            radioLetter.TabIndex = 2;
            radioLetter.Text = "US &Letter";
            radioLetter.UseVisualStyleBackColor = true;
            radioLetter.CheckedChanged += Option_CheckedChanged;
            // 
            // comboOrientation
            // 
            comboOrientation.DropDownStyle = ComboBoxStyle.DropDownList;
            comboOrientation.Enabled = false;
            comboOrientation.Location = new Point(170, 47);
            comboOrientation.Name = "comboOrientation";
            comboOrientation.Size = new Size(105, 23);
            comboOrientation.TabIndex = 3;
            comboOrientation.SelectedIndexChanged += ComboOrientation_SelectedIndexChanged;
            // 
            // radioFromImage
            // 
            radioFromImage.AutoSize = true;
            radioFromImage.Enabled = false;
            radioFromImage.Location = new Point(12, 74);
            radioFromImage.Name = "radioFromImage";
            radioFromImage.Size = new Size(157, 19);
            radioFromImage.TabIndex = 4;
            radioFromImage.Text = "Wie das &Bild (ohne Rand)";
            radioFromImage.UseVisualStyleBackColor = true;
            radioFromImage.CheckedChanged += Option_CheckedChanged;
            // 
            // groupPosition
            // 
            groupPosition.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupPosition.Controls.Add(radioAfter);
            groupPosition.Controls.Add(radioBefore);
            groupPosition.Controls.Add(radioFirst);
            groupPosition.Controls.Add(radioLast);
            groupPosition.Location = new Point(12, 124);
            groupPosition.Name = "groupPosition";
            groupPosition.Size = new Size(281, 78);
            groupPosition.TabIndex = 1;
            groupPosition.TabStop = false;
            groupPosition.Text = "Position";
            // 
            // radioAfter
            // 
            radioAfter.AutoSize = true;
            radioAfter.Checked = true;
            radioAfter.Location = new Point(12, 22);
            radioAfter.Name = "radioAfter";
            radioAfter.Size = new Size(98, 19);
            radioAfter.TabIndex = 0;
            radioAfter.TabStop = true;
            radioAfter.Text = "&Nach Seite {0}";
            radioAfter.UseVisualStyleBackColor = true;
            // 
            // radioBefore
            // 
            radioBefore.AutoSize = true;
            radioBefore.Location = new Point(170, 22);
            radioBefore.Name = "radioBefore";
            radioBefore.Size = new Size(87, 19);
            radioBefore.TabIndex = 1;
            radioBefore.Text = "&Vor Seite {0}";
            radioBefore.UseVisualStyleBackColor = true;
            // 
            // radioFirst
            // 
            radioFirst.AutoSize = true;
            radioFirst.Location = new Point(12, 48);
            radioFirst.Name = "radioFirst";
            radioFirst.Size = new Size(86, 19);
            radioFirst.TabIndex = 2;
            radioFirst.Text = "Am &Anfang";
            radioFirst.UseVisualStyleBackColor = true;
            // 
            // radioLast
            // 
            radioLast.AutoSize = true;
            radioLast.Location = new Point(170, 48);
            radioLast.Name = "radioLast";
            radioLast.Size = new Size(73, 19);
            radioLast.TabIndex = 3;
            radioLast.Text = "Am &Ende";
            radioLast.UseVisualStyleBackColor = true;
            // 
            // groupImage
            // 
            groupImage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupImage.Controls.Add(textBoxImage);
            groupImage.Controls.Add(buttonBrowse);
            groupImage.Controls.Add(radioOriginal);
            groupImage.Controls.Add(comboDpi);
            groupImage.Controls.Add(labelDpi);
            groupImage.Controls.Add(radioFit);
            groupImage.Controls.Add(labelImageSize);
            groupImage.Controls.Add(labelDpiHint);
            groupImage.Controls.Add(labelDpiSource);
            groupImage.Location = new Point(12, 210);
            groupImage.Name = "groupImage";
            groupImage.Size = new Size(434, 150);
            groupImage.TabIndex = 2;
            groupImage.TabStop = false;
            groupImage.Text = "Bild (optional)";
            // 
            // textBoxImage
            // 
            textBoxImage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxImage.Location = new Point(12, 24);
            textBoxImage.Name = "textBoxImage";
            textBoxImage.Size = new Size(269, 23);
            textBoxImage.TabIndex = 0;
            textBoxImage.TextChanged += TextBoxImage_TextChanged;
            // 
            // buttonBrowse
            // 
            buttonBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonBrowse.Location = new Point(287, 22);
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.Size = new Size(139, 27);
            buttonBrowse.TabIndex = 1;
            buttonBrowse.Text = "&Durchsuchen…";
            buttonBrowse.UseVisualStyleBackColor = true;
            buttonBrowse.Click += ButtonBrowse_Click;
            // 
            // radioOriginal
            // 
            radioOriginal.AutoSize = true;
            radioOriginal.Checked = true;
            radioOriginal.Enabled = false;
            radioOriginal.Location = new Point(12, 58);
            radioOriginal.Name = "radioOriginal";
            radioOriginal.Size = new Size(117, 19);
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
            comboDpi.Location = new Point(150, 56);
            comboDpi.Name = "comboDpi";
            comboDpi.Size = new Size(64, 23);
            comboDpi.TabIndex = 3;
            comboDpi.Text = "300";
            comboDpi.TextChanged += ComboDpi_TextChanged;
            // 
            // labelDpi
            // 
            labelDpi.AutoSize = true;
            labelDpi.Enabled = false;
            labelDpi.Location = new Point(220, 60);
            labelDpi.Name = "labelDpi";
            labelDpi.Size = new Size(24, 15);
            labelDpi.TabIndex = 4;
            labelDpi.Text = "dpi";
            // 
            // radioFit
            // 
            radioFit.AutoSize = true;
            radioFit.Enabled = false;
            radioFit.Location = new Point(12, 84);
            radioFit.Name = "radioFit";
            radioFit.Size = new Size(217, 19);
            radioFit.TabIndex = 5;
            radioFit.Text = "An die Seite an&passen (10 mm Rand)";
            radioFit.UseVisualStyleBackColor = true;
            radioFit.CheckedChanged += Option_CheckedChanged;
            // 
            // labelImageSize
            // 
            labelImageSize.AutoSize = true;
            labelImageSize.ForeColor = SystemColors.GrayText;
            labelImageSize.Location = new Point(12, 108);
            labelImageSize.Name = "labelImageSize";
            labelImageSize.Size = new Size(0, 15);
            labelImageSize.TabIndex = 6;
            // 
            // labelDpiHint
            // 
            labelDpiHint.AutoSize = true;
            labelDpiHint.Enabled = false;
            labelDpiHint.ForeColor = SystemColors.GrayText;
            labelDpiHint.Location = new Point(12, 126);
            labelDpiHint.Name = "labelDpiHint";
            labelDpiHint.Size = new Size(354, 15);
            labelDpiHint.TabIndex = 7;
            labelDpiHint.Text = "Mehr dpi = kleineres Bild. Scans meist 300 dpi, Screenshots 96 dpi.";
            // 
            // labelDpiSource
            // 
            labelDpiSource.AutoSize = true;
            labelDpiSource.ForeColor = SystemColors.GrayText;
            labelDpiSource.Location = new Point(250, 60);
            labelDpiSource.Name = "labelDpiSource";
            labelDpiSource.Size = new Size(0, 15);
            labelDpiSource.TabIndex = 8;
            // 
            // pictureBoxPreview
            // 
            pictureBoxPreview.BackColor = Color.FromArgb(210, 210, 210);
            pictureBoxPreview.Location = new Point(299, 19);
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.Size = new Size(147, 183);
            pictureBoxPreview.TabIndex = 3;
            pictureBoxPreview.TabStop = false;
            pictureBoxPreview.Paint += PictureBoxPreview_Paint;
            // 
            // labelPreviewPage
            // 
            labelPreviewPage.ForeColor = SystemColors.GrayText;
            labelPreviewPage.Location = new Point(299, 200);
            labelPreviewPage.Name = "labelPreviewPage";
            labelPreviewPage.Size = new Size(147, 15);
            labelPreviewPage.TabIndex = 6;
            labelPreviewPage.TextAlign = ContentAlignment.TopCenter;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.Location = new Point(250, 372);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(95, 27);
            buttonOK.TabIndex = 4;
            buttonOK.Text = "Einfügen";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(351, 372);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(95, 27);
            buttonCancel.TabIndex = 5;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // InsertPageForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(458, 411);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(pictureBoxPreview);
            Controls.Add(labelPreviewPage);
            Controls.Add(groupImage);
            Controls.Add(groupPosition);
            Controls.Add(groupFormat);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InsertPageForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Leere Seite einfügen";
            FormClosed += InsertPageForm_FormClosed;
            groupFormat.ResumeLayout(false);
            groupFormat.PerformLayout();
            groupPosition.ResumeLayout(false);
            groupPosition.PerformLayout();
            groupImage.ResumeLayout(false);
            groupImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
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
        private System.Windows.Forms.Label labelDpiSource;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Label labelPreviewPage;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
