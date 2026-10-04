namespace PDFLight.Forms
{
    partial class AnnotationForm
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
            labelFileValue = new Label();
            labelPage = new Label();
            labelText = new Label();
            textBoxText = new TextBox();
            labelInfo = new Label();
            labelLeft = new Label();
            numLeft = new NumericUpDown();
            labelTop = new Label();
            numTop = new NumericUpDown();
            labelSize = new Label();
            numSize = new NumericUpDown();
            labelBackground = new Label();
            comboBackground = new ComboBox();
            labelBorder = new Label();
            comboBorder = new ComboBox();
            labelTextColor = new Label();
            comboTextColor = new ComboBox();
            buttonOK = new Button();
            buttonCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)numLeft).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numTop).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSize).BeginInit();
            SuspendLayout();
            // 
            // labelFileValue
            // 
            labelFileValue.AutoEllipsis = true;
            labelFileValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelFileValue.Location = new Point(12, 9);
            labelFileValue.Name = "labelFileValue";
            labelFileValue.Size = new Size(269, 15);
            labelFileValue.TabIndex = 0;
            labelFileValue.Text = "datei.pdf";
            // 
            // labelPage
            // 
            labelPage.Location = new Point(12, 34);
            labelPage.Name = "labelPage";
            labelPage.Size = new Size(269, 15);
            labelPage.TabIndex = 1;
            // 
            // labelText
            // 
            labelText.AutoSize = true;
            labelText.Location = new Point(12, 62);
            labelText.Name = "labelText";
            labelText.Size = new Size(31, 15);
            labelText.TabIndex = 2;
            labelText.Text = "&Text:";
            // 
            // textBoxText
            // 
            textBoxText.AcceptsReturn = true;
            textBoxText.Location = new Point(12, 80);
            textBoxText.Multiline = true;
            textBoxText.Name = "textBoxText";
            textBoxText.ScrollBars = ScrollBars.Vertical;
            textBoxText.Size = new Size(269, 115);
            textBoxText.TabIndex = 3;
            // 
            // labelInfo
            // 
            labelInfo.ForeColor = SystemColors.GrayText;
            labelInfo.Location = new Point(12, 198);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(269, 18);
            labelInfo.TabIndex = 4;
            labelInfo.Text = "Maße ab der linken oberen Ecke der Seite.";
            // 
            // labelLeft
            // 
            labelLeft.AutoSize = true;
            labelLeft.Location = new Point(12, 238);
            labelLeft.Name = "labelLeft";
            labelLeft.Size = new Size(137, 15);
            labelLeft.TabIndex = 5;
            labelLeft.Text = "Abstand von &links (mm):";
            // 
            // numLeft
            // 
            numLeft.DecimalPlaces = 1;
            numLeft.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numLeft.Location = new Point(201, 236);
            numLeft.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numLeft.Name = "numLeft";
            numLeft.Size = new Size(80, 23);
            numLeft.TabIndex = 6;
            numLeft.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // labelTop
            // 
            labelTop.AutoSize = true;
            labelTop.Location = new Point(12, 267);
            labelTop.Name = "labelTop";
            labelTop.Size = new Size(140, 15);
            labelTop.TabIndex = 7;
            labelTop.Text = "Abstand von &oben (mm):";
            // 
            // numTop
            // 
            numTop.DecimalPlaces = 1;
            numTop.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numTop.Location = new Point(201, 265);
            numTop.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numTop.Name = "numTop";
            numTop.Size = new Size(80, 23);
            numTop.TabIndex = 8;
            numTop.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // labelSize
            // 
            labelSize.AutoSize = true;
            labelSize.Location = new Point(12, 296);
            labelSize.Name = "labelSize";
            labelSize.Size = new Size(97, 15);
            labelSize.TabIndex = 9;
            labelSize.Text = "Schrift&größe (pt):";
            // 
            // numSize
            // 
            numSize.Location = new Point(201, 294);
            numSize.Maximum = new decimal(new int[] { 72, 0, 0, 0 });
            numSize.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            numSize.Name = "numSize";
            numSize.Size = new Size(80, 23);
            numSize.TabIndex = 10;
            numSize.Value = new decimal(new int[] { 12, 0, 0, 0 });
            // 
            // labelBackground
            // 
            labelBackground.AutoSize = true;
            labelBackground.Location = new Point(12, 329);
            labelBackground.Name = "labelBackground";
            labelBackground.Size = new Size(75, 15);
            labelBackground.TabIndex = 11;
            labelBackground.Text = "&Hintergrund:";
            // 
            // comboBackground
            // 
            comboBackground.DrawMode = DrawMode.OwnerDrawFixed;
            comboBackground.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBackground.Location = new Point(160, 326);
            comboBackground.Name = "comboBackground";
            comboBackground.Size = new Size(121, 24);
            comboBackground.TabIndex = 12;
            comboBackground.DrawItem += ComboBackground_DrawItem;
            comboBackground.SelectedIndexChanged += ColorCombo_SelectedIndexChanged;
            // 
            // labelBorder
            //
            labelBorder.AutoSize = true;
            labelBorder.Location = new Point(12, 389);
            labelBorder.Name = "labelBorder";
            labelBorder.Size = new Size(52, 15);
            labelBorder.TabIndex = 15;
            labelBorder.Text = "&Rahmen:";
            //
            // comboBorder
            //
            comboBorder.DrawMode = DrawMode.OwnerDrawFixed;
            comboBorder.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBorder.Location = new Point(160, 386);
            comboBorder.Name = "comboBorder";
            comboBorder.Size = new Size(121, 24);
            comboBorder.TabIndex = 16;
            comboBorder.DrawItem += ComboBorder_DrawItem;
            comboBorder.SelectedIndexChanged += ColorCombo_SelectedIndexChanged;
            // 
            // labelTextColor
            // 
            labelTextColor.AutoSize = true;
            labelTextColor.Location = new Point(12, 359);
            labelTextColor.Name = "labelTextColor";
            labelTextColor.Size = new Size(71, 15);
            labelTextColor.TabIndex = 13;
            labelTextColor.Text = "Schrift&farbe:";
            // 
            // comboTextColor
            // 
            comboTextColor.DrawMode = DrawMode.OwnerDrawFixed;
            comboTextColor.DropDownStyle = ComboBoxStyle.DropDownList;
            comboTextColor.Location = new Point(161, 356);
            comboTextColor.Name = "comboTextColor";
            comboTextColor.Size = new Size(120, 24);
            comboTextColor.TabIndex = 14;
            comboTextColor.DrawItem += ComboTextColor_DrawItem;
            comboTextColor.SelectedIndexChanged += ColorCombo_SelectedIndexChanged;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonOK.Location = new Point(12, 424);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(142, 27);
            buttonOK.TabIndex = 17;
            buttonOK.Text = "Hinzufügen";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(160, 424);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(121, 27);
            buttonCancel.TabIndex = 18;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // AnnotationForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(293, 463);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(comboTextColor);
            Controls.Add(labelTextColor);
            Controls.Add(comboBorder);
            Controls.Add(labelBorder);
            Controls.Add(comboBackground);
            Controls.Add(labelBackground);
            Controls.Add(numSize);
            Controls.Add(labelSize);
            Controls.Add(numTop);
            Controls.Add(labelTop);
            Controls.Add(numLeft);
            Controls.Add(labelLeft);
            Controls.Add(labelInfo);
            Controls.Add(textBoxText);
            Controls.Add(labelText);
            Controls.Add(labelPage);
            Controls.Add(labelFileValue);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AnnotationForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Textanmerkung hinzufügen";
            ((System.ComponentModel.ISupportInitialize)numLeft).EndInit();
            ((System.ComponentModel.ISupportInitialize)numTop).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSize).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelFileValue;
        private System.Windows.Forms.Label labelPage;
        private System.Windows.Forms.Label labelText;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelLeft;
        private System.Windows.Forms.NumericUpDown numLeft;
        private System.Windows.Forms.Label labelTop;
        private System.Windows.Forms.NumericUpDown numTop;
        private System.Windows.Forms.Label labelSize;
        private System.Windows.Forms.NumericUpDown numSize;
        private System.Windows.Forms.Label labelBackground;
        private System.Windows.Forms.ComboBox comboBackground;
        private System.Windows.Forms.Label labelBorder;
        private System.Windows.Forms.ComboBox comboBorder;
        private System.Windows.Forms.Label labelTextColor;
        private System.Windows.Forms.ComboBox comboTextColor;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
