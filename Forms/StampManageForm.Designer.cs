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
            cbDate = new CheckBox();
            cbBorder = new CheckBox();
            cbRounded = new CheckBox();
            buttonOK = new Button();
            buttonCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)numSize).BeginInit();
            SuspendLayout();
            // 
            // listStamps
            // 
            listStamps.BackColor = SystemColors.Control;
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
            textBoxText.Location = new Point(430, 12);
            textBoxText.Name = "textBoxText";
            textBoxText.Size = new Size(160, 23);
            textBoxText.TabIndex = 4;
            textBoxText.TextChanged += Field_Changed;
            textBoxText.Leave += TextBoxText_Leave;
            // 
            // labelSize
            // 
            labelSize.AutoSize = true;
            labelSize.Location = new Point(330, 47);
            labelSize.Name = "labelSize";
            labelSize.Size = new Size(97, 15);
            labelSize.TabIndex = 5;
            labelSize.Text = "Schrift&größe (pt):";
            // 
            // numSize
            // 
            numSize.Location = new Point(430, 45);
            numSize.Maximum = new decimal(new int[] { 72, 0, 0, 0 });
            numSize.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numSize.Name = "numSize";
            numSize.Size = new Size(80, 23);
            numSize.TabIndex = 6;
            numSize.Value = new decimal(new int[] { 24, 0, 0, 0 });
            numSize.ValueChanged += Field_Changed;
            // 
            // labelColor
            // 
            labelColor.AutoSize = true;
            labelColor.Location = new Point(330, 81);
            labelColor.Name = "labelColor";
            labelColor.Size = new Size(71, 15);
            labelColor.TabIndex = 7;
            labelColor.Text = "Schrift&farbe:";
            // 
            // comboColor
            // 
            comboColor.DrawMode = DrawMode.OwnerDrawFixed;
            comboColor.DropDownStyle = ComboBoxStyle.DropDownList;
            comboColor.Location = new Point(430, 78);
            comboColor.Name = "comboColor";
            comboColor.Size = new Size(160, 24);
            comboColor.TabIndex = 8;
            comboColor.DrawItem += ComboColor_DrawItem;
            comboColor.SelectedIndexChanged += Field_Changed;
            // 
            // labelBackground
            // 
            labelBackground.AutoSize = true;
            labelBackground.Location = new Point(330, 115);
            labelBackground.Name = "labelBackground";
            labelBackground.Size = new Size(75, 15);
            labelBackground.TabIndex = 9;
            labelBackground.Text = "&Hintergrund:";
            // 
            // comboBackground
            // 
            comboBackground.DrawMode = DrawMode.OwnerDrawFixed;
            comboBackground.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBackground.Location = new Point(430, 112);
            comboBackground.Name = "comboBackground";
            comboBackground.Size = new Size(160, 24);
            comboBackground.TabIndex = 10;
            comboBackground.DrawItem += ComboBackground_DrawItem;
            comboBackground.SelectedIndexChanged += Field_Changed;
            // 
            // labelPosition
            // 
            labelPosition.AutoSize = true;
            labelPosition.Location = new Point(330, 149);
            labelPosition.Name = "labelPosition";
            labelPosition.Size = new Size(53, 15);
            labelPosition.TabIndex = 11;
            labelPosition.Text = "&Position:";
            // 
            // comboPosition
            // 
            comboPosition.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPosition.Location = new Point(430, 146);
            comboPosition.Name = "comboPosition";
            comboPosition.Size = new Size(160, 23);
            comboPosition.TabIndex = 12;
            comboPosition.SelectedIndexChanged += Field_Changed;
            // 
            // cbDate
            // 
            cbDate.AutoSize = true;
            cbDate.Checked = true;
            cbDate.CheckState = CheckState.Checked;
            cbDate.Location = new Point(333, 182);
            cbDate.Name = "cbDate";
            cbDate.Size = new Size(173, 19);
            cbDate.TabIndex = 13;
            cbDate.Text = "&Datum und Uhrzeit anfügen";
            cbDate.UseVisualStyleBackColor = true;
            cbDate.CheckedChanged += Field_Changed;
            // 
            // cbBorder
            // 
            cbBorder.AutoSize = true;
            cbBorder.Checked = true;
            cbBorder.CheckState = CheckState.Checked;
            cbBorder.Location = new Point(333, 207);
            cbBorder.Name = "cbBorder";
            cbBorder.Size = new Size(70, 19);
            cbBorder.TabIndex = 14;
            cbBorder.Text = "&Rahmen";
            cbBorder.UseVisualStyleBackColor = true;
            cbBorder.CheckedChanged += Field_Changed;
            // 
            // cbRounded
            // 
            cbRounded.AutoSize = true;
            cbRounded.Location = new Point(430, 207);
            cbRounded.Name = "cbRounded";
            cbRounded.Size = new Size(129, 19);
            cbRounded.TabIndex = 15;
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
            buttonOK.TabIndex = 16;
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
            buttonCancel.TabIndex = 17;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // StampManageForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(602, 329);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(cbRounded);
            Controls.Add(cbBorder);
            Controls.Add(cbDate);
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
        private System.Windows.Forms.CheckBox cbDate;
        private System.Windows.Forms.CheckBox cbBorder;
        private System.Windows.Forms.CheckBox cbRounded;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
