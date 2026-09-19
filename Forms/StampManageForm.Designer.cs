namespace PDFLight.Forms
{
    partial class StampManageForm
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
            listStamps = new ListBox();
            buttonNew = new Button();
            buttonDelete = new Button();
            labelText = new Label();
            textBoxText = new TextBox();
            labelSize = new Label();
            numSize = new NumericUpDown();
            labelColor = new Label();
            comboColor = new ComboBox();
            labelBackground = new Label();
            comboBackground = new ComboBox();
            labelPosition = new Label();
            comboPosition = new ComboBox();
            labelInitials = new Label();
            textBoxInitials = new TextBox();
            cbDate = new CheckBox();
            cbBorder = new CheckBox();
            cbRounded = new CheckBox();
            buttonOK = new Button();
            buttonCancel = new Button();
            labelMaxInitials = new Label();
            buttonDefaults = new Button();
            ((System.ComponentModel.ISupportInitialize)numSize).BeginInit();
            SuspendLayout();
            // 
            // listStamps
            // 
            listStamps.BackColor = Color.FromArgb(228, 228, 228);
            listStamps.DrawMode = DrawMode.OwnerDrawFixed;
            listStamps.IntegralHeight = false;
            listStamps.ItemHeight = 44;
            listStamps.Location = new Point(12, 12);
            listStamps.Name = "listStamps";
            listStamps.Size = new Size(300, 270);
            listStamps.TabIndex = 0;
            listStamps.DrawItem += ListStamps_DrawItem;
            listStamps.SelectedIndexChanged += ListStamps_SelectedIndexChanged;
            // 
            // buttonNew
            // 
            buttonNew.Location = new Point(12, 290);
            buttonNew.Name = "buttonNew";
            buttonNew.Size = new Size(95, 27);
            buttonNew.TabIndex = 1;
            buttonNew.Text = "&Neu";
            buttonNew.UseVisualStyleBackColor = true;
            buttonNew.Click += ButtonNew_Click;
            // 
            // buttonDelete
            // 
            buttonDelete.Location = new Point(113, 290);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(95, 27);
            buttonDelete.TabIndex = 2;
            buttonDelete.Text = "&Löschen";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += ButtonDelete_Click;
            // 
            // labelText
            // 
            labelText.AutoSize = true;
            labelText.Location = new Point(330, 15);
            labelText.Name = "labelText";
            labelText.Size = new Size(31, 15);
            labelText.TabIndex = 3;
            labelText.Text = "&Text:";
            // 
            // textBoxText
            // 
            textBoxText.Location = new Point(435, 12);
            textBoxText.Name = "textBoxText";
            textBoxText.Size = new Size(155, 23);
            textBoxText.TabIndex = 4;
            textBoxText.TextChanged += Field_Changed;
            textBoxText.Leave += TextBoxText_Leave;
            // 
            // labelSize
            // 
            labelSize.AutoSize = true;
            labelSize.Location = new Point(330, 43);
            labelSize.Name = "labelSize";
            labelSize.Size = new Size(97, 15);
            labelSize.TabIndex = 5;
            labelSize.Text = "Schrift&größe (pt):";
            // 
            // numSize
            // 
            numSize.Location = new Point(435, 41);
            numSize.Maximum = new decimal(new int[] { 72, 0, 0, 0 });
            numSize.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numSize.Name = "numSize";
            numSize.Size = new Size(66, 23);
            numSize.TabIndex = 6;
            numSize.Value = new decimal(new int[] { 24, 0, 0, 0 });
            numSize.ValueChanged += Field_Changed;
            // 
            // labelColor
            // 
            labelColor.AutoSize = true;
            labelColor.Location = new Point(330, 73);
            labelColor.Name = "labelColor";
            labelColor.Size = new Size(71, 15);
            labelColor.TabIndex = 7;
            labelColor.Text = "Schrift&farbe:";
            // 
            // comboColor
            // 
            comboColor.DrawMode = DrawMode.OwnerDrawFixed;
            comboColor.DropDownStyle = ComboBoxStyle.DropDownList;
            comboColor.Location = new Point(435, 70);
            comboColor.Name = "comboColor";
            comboColor.Size = new Size(155, 24);
            comboColor.TabIndex = 8;
            comboColor.DrawItem += ComboColor_DrawItem;
            comboColor.SelectedIndexChanged += Field_Changed;
            // 
            // labelBackground
            // 
            labelBackground.AutoSize = true;
            labelBackground.Location = new Point(330, 103);
            labelBackground.Name = "labelBackground";
            labelBackground.Size = new Size(75, 15);
            labelBackground.TabIndex = 9;
            labelBackground.Text = "&Hintergrund:";
            // 
            // comboBackground
            // 
            comboBackground.DrawMode = DrawMode.OwnerDrawFixed;
            comboBackground.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBackground.Location = new Point(435, 100);
            comboBackground.Name = "comboBackground";
            comboBackground.Size = new Size(155, 24);
            comboBackground.TabIndex = 10;
            comboBackground.DrawItem += ComboBackground_DrawItem;
            comboBackground.SelectedIndexChanged += Field_Changed;
            // 
            // labelPosition
            // 
            labelPosition.AutoSize = true;
            labelPosition.Location = new Point(330, 133);
            labelPosition.Name = "labelPosition";
            labelPosition.Size = new Size(53, 15);
            labelPosition.TabIndex = 11;
            labelPosition.Text = "&Position:";
            // 
            // comboPosition
            // 
            comboPosition.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPosition.Location = new Point(435, 130);
            comboPosition.Name = "comboPosition";
            comboPosition.Size = new Size(155, 23);
            comboPosition.TabIndex = 12;
            comboPosition.SelectedIndexChanged += Field_Changed;
            // 
            // labelInitials
            // 
            labelInitials.AutoSize = true;
            labelInitials.Location = new Point(330, 162);
            labelInitials.Name = "labelInitials";
            labelInitials.Size = new Size(94, 15);
            labelInitials.TabIndex = 13;
            labelInitials.Text = "Bearbeiter&kürzel:";
            // 
            // textBoxInitials
            // 
            textBoxInitials.Location = new Point(435, 159);
            textBoxInitials.MaxLength = 3;
            textBoxInitials.Name = "textBoxInitials";
            textBoxInitials.Size = new Size(55, 23);
            textBoxInitials.TabIndex = 14;
            textBoxInitials.TextChanged += Field_Changed;
            // 
            // cbDate
            // 
            cbDate.AutoSize = true;
            cbDate.Checked = true;
            cbDate.CheckState = CheckState.Checked;
            cbDate.Location = new Point(333, 191);
            cbDate.Name = "cbDate";
            cbDate.Size = new Size(173, 19);
            cbDate.TabIndex = 15;
            cbDate.Text = "&Datum und Uhrzeit anfügen";
            cbDate.UseVisualStyleBackColor = true;
            cbDate.CheckedChanged += Field_Changed;
            // 
            // cbBorder
            // 
            cbBorder.AutoSize = true;
            cbBorder.Checked = true;
            cbBorder.CheckState = CheckState.Checked;
            cbBorder.Location = new Point(333, 219);
            cbBorder.Name = "cbBorder";
            cbBorder.Size = new Size(120, 19);
            cbBorder.TabIndex = 16;
            cbBorder.Text = "&Rahmen zeichnen";
            cbBorder.UseVisualStyleBackColor = true;
            cbBorder.CheckedChanged += Field_Changed;
            // 
            // cbRounded
            // 
            cbRounded.AutoSize = true;
            cbRounded.Location = new Point(333, 247);
            cbRounded.Name = "cbRounded";
            cbRounded.Size = new Size(129, 19);
            cbRounded.TabIndex = 17;
            cbRounded.Text = "Abgerundete &Ecken";
            cbRounded.UseVisualStyleBackColor = true;
            cbRounded.CheckedChanged += Field_Changed;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.Location = new Point(330, 290);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(159, 27);
            buttonOK.TabIndex = 18;
            buttonOK.Text = "Palette &speichern";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(495, 290);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(95, 27);
            buttonCancel.TabIndex = 19;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelMaxInitials
            // 
            labelMaxInitials.AutoSize = true;
            labelMaxInitials.Location = new Point(496, 162);
            labelMaxInitials.Name = "labelMaxInitials";
            labelMaxInitials.Size = new Size(86, 15);
            labelMaxInitials.TabIndex = 20;
            labelMaxInitials.Text = "max. 3 Zeichen";
            // 
            // buttonDefaults
            // 
            buttonDefaults.Location = new Point(214, 288);
            buttonDefaults.Name = "buttonDefaults";
            buttonDefaults.Size = new Size(98, 27);
            buttonDefaults.TabIndex = 21;
            buttonDefaults.Text = "&Defaults";
            buttonDefaults.UseVisualStyleBackColor = true;
            buttonDefaults.Click += ButtonDefaults_Click;
            // 
            // StampManageForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(602, 329);
            Controls.Add(buttonDefaults);
            Controls.Add(labelMaxInitials);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(cbRounded);
            Controls.Add(cbBorder);
            Controls.Add(cbDate);
            Controls.Add(textBoxInitials);
            Controls.Add(labelInitials);
            Controls.Add(comboPosition);
            Controls.Add(labelPosition);
            Controls.Add(comboBackground);
            Controls.Add(labelBackground);
            Controls.Add(comboColor);
            Controls.Add(labelColor);
            Controls.Add(numSize);
            Controls.Add(labelSize);
            Controls.Add(textBoxText);
            Controls.Add(labelText);
            Controls.Add(buttonDelete);
            Controls.Add(buttonNew);
            Controls.Add(listStamps);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "StampManageForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Stempel verwalten";
            ((System.ComponentModel.ISupportInitialize)numSize).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox listStamps;
        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Label labelText;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.Label labelSize;
        private System.Windows.Forms.NumericUpDown numSize;
        private System.Windows.Forms.Label labelColor;
        private System.Windows.Forms.ComboBox comboColor;
        private System.Windows.Forms.Label labelBackground;
        private System.Windows.Forms.ComboBox comboBackground;
        private System.Windows.Forms.Label labelPosition;
        private System.Windows.Forms.ComboBox comboPosition;
        private System.Windows.Forms.Label labelInitials;
        private System.Windows.Forms.TextBox textBoxInitials;
        private System.Windows.Forms.CheckBox cbDate;
        private System.Windows.Forms.CheckBox cbBorder;
        private System.Windows.Forms.CheckBox cbRounded;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private Label labelMaxInitials;
        private Button buttonDefaults;
    }
}
