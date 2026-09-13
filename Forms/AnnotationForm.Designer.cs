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
            labelFileValue = new System.Windows.Forms.Label();
            labelText = new System.Windows.Forms.Label();
            textBoxText = new System.Windows.Forms.TextBox();
            labelInfo = new System.Windows.Forms.Label();
            labelPage = new System.Windows.Forms.Label();
            numPage = new System.Windows.Forms.NumericUpDown();
            labelLeft = new System.Windows.Forms.Label();
            numLeft = new System.Windows.Forms.NumericUpDown();
            labelTop = new System.Windows.Forms.Label();
            numTop = new System.Windows.Forms.NumericUpDown();
            labelSize = new System.Windows.Forms.Label();
            numSize = new System.Windows.Forms.NumericUpDown();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)numPage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numLeft).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numTop).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSize).BeginInit();
            SuspendLayout();
            //
            // labelFileValue
            //
            labelFileValue.AutoEllipsis = true;
            labelFileValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            labelFileValue.Location = new System.Drawing.Point(12, 12);
            labelFileValue.Name = "labelFileValue";
            labelFileValue.Size = new System.Drawing.Size(400, 15);
            labelFileValue.TabIndex = 0;
            labelFileValue.Text = "datei.pdf";
            //
            // labelText
            //
            labelText.AutoSize = true;
            labelText.Location = new System.Drawing.Point(12, 38);
            labelText.Name = "labelText";
            labelText.Size = new System.Drawing.Size(34, 15);
            labelText.TabIndex = 1;
            labelText.Text = "&Text:";
            //
            // textBoxText
            //
            textBoxText.AcceptsReturn = true;
            textBoxText.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxText.Location = new System.Drawing.Point(12, 56);
            textBoxText.Multiline = true;
            textBoxText.Name = "textBoxText";
            textBoxText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBoxText.Size = new System.Drawing.Size(400, 62);
            textBoxText.TabIndex = 2;
            //
            // labelInfo
            //
            labelInfo.ForeColor = System.Drawing.SystemColors.GrayText;
            labelInfo.Location = new System.Drawing.Point(12, 124);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new System.Drawing.Size(400, 30);
            labelInfo.TabIndex = 3;
            labelInfo.Text = "Der Text erscheint als gelber Kasten; Maße ab der linken oberen Ecke der Seite.";
            //
            // labelPage
            //
            labelPage.AutoSize = true;
            labelPage.Location = new System.Drawing.Point(12, 170);
            labelPage.Name = "labelPage";
            labelPage.Size = new System.Drawing.Size(38, 15);
            labelPage.TabIndex = 4;
            labelPage.Text = "&Seite:";
            //
            // numPage
            //
            numPage.Location = new System.Drawing.Point(190, 167);
            numPage.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numPage.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPage.Name = "numPage";
            numPage.Size = new System.Drawing.Size(80, 23);
            numPage.TabIndex = 5;
            numPage.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // labelLeft
            //
            labelLeft.AutoSize = true;
            labelLeft.Location = new System.Drawing.Point(12, 199);
            labelLeft.Name = "labelLeft";
            labelLeft.Size = new System.Drawing.Size(140, 15);
            labelLeft.TabIndex = 6;
            labelLeft.Text = "Abstand von &links (mm):";
            //
            // numLeft
            //
            numLeft.DecimalPlaces = 1;
            numLeft.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numLeft.Location = new System.Drawing.Point(190, 196);
            numLeft.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numLeft.Name = "numLeft";
            numLeft.Size = new System.Drawing.Size(80, 23);
            numLeft.TabIndex = 7;
            numLeft.Value = new decimal(new int[] { 20, 0, 0, 0 });
            //
            // labelTop
            //
            labelTop.AutoSize = true;
            labelTop.Location = new System.Drawing.Point(12, 228);
            labelTop.Name = "labelTop";
            labelTop.Size = new System.Drawing.Size(140, 15);
            labelTop.TabIndex = 8;
            labelTop.Text = "Abstand von &oben (mm):";
            //
            // numTop
            //
            numTop.DecimalPlaces = 1;
            numTop.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numTop.Location = new System.Drawing.Point(190, 225);
            numTop.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numTop.Name = "numTop";
            numTop.Size = new System.Drawing.Size(80, 23);
            numTop.TabIndex = 9;
            numTop.Value = new decimal(new int[] { 20, 0, 0, 0 });
            //
            // labelSize
            //
            labelSize.AutoSize = true;
            labelSize.Location = new System.Drawing.Point(12, 257);
            labelSize.Name = "labelSize";
            labelSize.Size = new System.Drawing.Size(110, 15);
            labelSize.TabIndex = 10;
            labelSize.Text = "Schrift&größe (pt):";
            //
            // numSize
            //
            numSize.Location = new System.Drawing.Point(190, 254);
            numSize.Maximum = new decimal(new int[] { 72, 0, 0, 0 });
            numSize.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            numSize.Name = "numSize";
            numSize.Size = new System.Drawing.Size(80, 23);
            numSize.TabIndex = 11;
            numSize.Value = new decimal(new int[] { 12, 0, 0, 0 });
            //
            // buttonOK
            //
            buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonOK.Location = new System.Drawing.Point(216, 295);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(95, 27);
            buttonOK.TabIndex = 12;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(317, 295);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(95, 27);
            buttonCancel.TabIndex = 13;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // AnnotationForm
            //
            AcceptButton = buttonOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(424, 334);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(numSize);
            Controls.Add(labelSize);
            Controls.Add(numTop);
            Controls.Add(labelTop);
            Controls.Add(numLeft);
            Controls.Add(labelLeft);
            Controls.Add(numPage);
            Controls.Add(labelPage);
            Controls.Add(labelInfo);
            Controls.Add(textBoxText);
            Controls.Add(labelText);
            Controls.Add(labelFileValue);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AnnotationForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Textanmerkung hinzufügen";
            ((System.ComponentModel.ISupportInitialize)numPage).EndInit();
            ((System.ComponentModel.ISupportInitialize)numLeft).EndInit();
            ((System.ComponentModel.ISupportInitialize)numTop).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSize).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelFileValue;
        private System.Windows.Forms.Label labelText;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelPage;
        private System.Windows.Forms.NumericUpDown numPage;
        private System.Windows.Forms.Label labelLeft;
        private System.Windows.Forms.NumericUpDown numLeft;
        private System.Windows.Forms.Label labelTop;
        private System.Windows.Forms.NumericUpDown numTop;
        private System.Windows.Forms.Label labelSize;
        private System.Windows.Forms.NumericUpDown numSize;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
