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
            listStamps = new System.Windows.Forms.ListBox();
            buttonNew = new System.Windows.Forms.Button();
            buttonDelete = new System.Windows.Forms.Button();
            labelText = new System.Windows.Forms.Label();
            textBoxText = new System.Windows.Forms.TextBox();
            labelSize = new System.Windows.Forms.Label();
            numSize = new System.Windows.Forms.NumericUpDown();
            labelColor = new System.Windows.Forms.Label();
            comboColor = new System.Windows.Forms.ComboBox();
            labelBackground = new System.Windows.Forms.Label();
            comboBackground = new System.Windows.Forms.ComboBox();
            labelPosition = new System.Windows.Forms.Label();
            comboPosition = new System.Windows.Forms.ComboBox();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)numSize).BeginInit();
            SuspendLayout();
            //
            // listStamps
            //
            listStamps.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            listStamps.IntegralHeight = false;
            listStamps.ItemHeight = 44;
            listStamps.Location = new System.Drawing.Point(12, 12);
            listStamps.Name = "listStamps";
            listStamps.Size = new System.Drawing.Size(300, 226);
            listStamps.TabIndex = 0;
            listStamps.DrawItem += ListStamps_DrawItem;
            listStamps.SelectedIndexChanged += ListStamps_SelectedIndexChanged;
            //
            // buttonNew
            //
            buttonNew.Location = new System.Drawing.Point(12, 246);
            buttonNew.Name = "buttonNew";
            buttonNew.Size = new System.Drawing.Size(95, 27);
            buttonNew.TabIndex = 1;
            buttonNew.Text = "&Neu";
            buttonNew.UseVisualStyleBackColor = true;
            buttonNew.Click += ButtonNew_Click;
            //
            // buttonDelete
            //
            buttonDelete.Location = new System.Drawing.Point(113, 246);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new System.Drawing.Size(95, 27);
            buttonDelete.TabIndex = 2;
            buttonDelete.Text = "&Löschen";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += ButtonDelete_Click;
            //
            // labelText
            //
            labelText.AutoSize = true;
            labelText.Location = new System.Drawing.Point(330, 15);
            labelText.Name = "labelText";
            labelText.Size = new System.Drawing.Size(31, 15);
            labelText.TabIndex = 3;
            labelText.Text = "&Text:";
            //
            // textBoxText
            //
            textBoxText.Location = new System.Drawing.Point(430, 12);
            textBoxText.Name = "textBoxText";
            textBoxText.Size = new System.Drawing.Size(160, 23);
            textBoxText.TabIndex = 4;
            textBoxText.TextChanged += Field_Changed;
            textBoxText.Leave += TextBoxText_Leave;
            //
            // labelSize
            //
            labelSize.AutoSize = true;
            labelSize.Location = new System.Drawing.Point(330, 47);
            labelSize.Name = "labelSize";
            labelSize.Size = new System.Drawing.Size(95, 15);
            labelSize.TabIndex = 5;
            labelSize.Text = "Schrift&größe (pt):";
            //
            // numSize
            //
            numSize.Location = new System.Drawing.Point(430, 45);
            numSize.Maximum = new decimal(new int[] { 72, 0, 0, 0 });
            numSize.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numSize.Name = "numSize";
            numSize.Size = new System.Drawing.Size(80, 23);
            numSize.TabIndex = 6;
            numSize.Value = new decimal(new int[] { 24, 0, 0, 0 });
            numSize.ValueChanged += Field_Changed;
            //
            // labelColor
            //
            labelColor.AutoSize = true;
            labelColor.Location = new System.Drawing.Point(330, 81);
            labelColor.Name = "labelColor";
            labelColor.Size = new System.Drawing.Size(71, 15);
            labelColor.TabIndex = 7;
            labelColor.Text = "Schrift&farbe:";
            //
            // comboColor
            //
            comboColor.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            comboColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboColor.Location = new System.Drawing.Point(430, 78);
            comboColor.Name = "comboColor";
            comboColor.Size = new System.Drawing.Size(160, 24);
            comboColor.TabIndex = 8;
            comboColor.DrawItem += ComboColor_DrawItem;
            comboColor.SelectedIndexChanged += Field_Changed;
            //
            // labelBackground
            //
            labelBackground.AutoSize = true;
            labelBackground.Location = new System.Drawing.Point(330, 115);
            labelBackground.Name = "labelBackground";
            labelBackground.Size = new System.Drawing.Size(75, 15);
            labelBackground.TabIndex = 9;
            labelBackground.Text = "&Hintergrund:";
            //
            // comboBackground
            //
            comboBackground.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            comboBackground.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBackground.Location = new System.Drawing.Point(430, 112);
            comboBackground.Name = "comboBackground";
            comboBackground.Size = new System.Drawing.Size(160, 24);
            comboBackground.TabIndex = 10;
            comboBackground.DrawItem += ComboBackground_DrawItem;
            comboBackground.SelectedIndexChanged += Field_Changed;
            //
            // labelPosition
            //
            labelPosition.AutoSize = true;
            labelPosition.Location = new System.Drawing.Point(330, 149);
            labelPosition.Name = "labelPosition";
            labelPosition.Size = new System.Drawing.Size(53, 15);
            labelPosition.TabIndex = 11;
            labelPosition.Text = "&Position:";
            //
            // comboPosition
            //
            comboPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboPosition.Location = new System.Drawing.Point(430, 146);
            comboPosition.Name = "comboPosition";
            comboPosition.Size = new System.Drawing.Size(160, 23);
            comboPosition.TabIndex = 12;
            comboPosition.SelectedIndexChanged += Field_Changed;
            //
            // buttonOK
            //
            buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonOK.Location = new System.Drawing.Point(394, 246);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(95, 27);
            buttonOK.TabIndex = 13;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(495, 246);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(95, 27);
            buttonCancel.TabIndex = 14;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // StampManageForm
            //
            AcceptButton = buttonOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(602, 285);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
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
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "StampManageForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
