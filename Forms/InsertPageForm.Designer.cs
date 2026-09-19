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
            labelPrompt = new System.Windows.Forms.Label();
            radioBefore = new System.Windows.Forms.RadioButton();
            radioAfter = new System.Windows.Forms.RadioButton();
            labelImage = new System.Windows.Forms.Label();
            textBoxImage = new System.Windows.Forms.TextBox();
            buttonBrowse = new System.Windows.Forms.Button();
            labelHint = new System.Windows.Forms.Label();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // labelPrompt
            //
            labelPrompt.AutoSize = true;
            labelPrompt.Location = new System.Drawing.Point(12, 12);
            labelPrompt.Name = "labelPrompt";
            labelPrompt.Size = new System.Drawing.Size(280, 15);
            labelPrompt.TabIndex = 0;
            labelPrompt.Text = "Die neue Seite übernimmt das Format von Seite {0}.";
            //
            // radioBefore
            //
            radioBefore.AutoSize = true;
            radioBefore.Location = new System.Drawing.Point(12, 40);
            radioBefore.Name = "radioBefore";
            radioBefore.Size = new System.Drawing.Size(160, 19);
            radioBefore.TabIndex = 1;
            radioBefore.Text = "&Vor der aktuellen Seite";
            radioBefore.UseVisualStyleBackColor = true;
            //
            // radioAfter
            //
            radioAfter.AutoSize = true;
            radioAfter.Checked = true;
            radioAfter.Location = new System.Drawing.Point(200, 40);
            radioAfter.Name = "radioAfter";
            radioAfter.Size = new System.Drawing.Size(170, 19);
            radioAfter.TabIndex = 2;
            radioAfter.TabStop = true;
            radioAfter.Text = "&Nach der aktuellen Seite";
            radioAfter.UseVisualStyleBackColor = true;
            //
            // labelImage
            //
            labelImage.AutoSize = true;
            labelImage.Location = new System.Drawing.Point(12, 76);
            labelImage.Name = "labelImage";
            labelImage.Size = new System.Drawing.Size(90, 15);
            labelImage.TabIndex = 3;
            labelImage.Text = "&Bild (optional):";
            //
            // textBoxImage
            //
            textBoxImage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxImage.Location = new System.Drawing.Point(12, 96);
            textBoxImage.Name = "textBoxImage";
            textBoxImage.Size = new System.Drawing.Size(268, 23);
            textBoxImage.TabIndex = 4;
            //
            // buttonBrowse
            //
            buttonBrowse.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonBrowse.Location = new System.Drawing.Point(286, 94);
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.Size = new System.Drawing.Size(120, 27);
            buttonBrowse.TabIndex = 5;
            buttonBrowse.Text = "&Durchsuchen …";
            buttonBrowse.UseVisualStyleBackColor = true;
            buttonBrowse.Click += ButtonBrowse_Click;
            //
            // labelHint
            //
            labelHint.AutoSize = true;
            labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            labelHint.Location = new System.Drawing.Point(12, 124);
            labelHint.Name = "labelHint";
            labelHint.Size = new System.Drawing.Size(330, 15);
            labelHint.TabIndex = 6;
            labelHint.Text = "Das Bild wird mit 10 mm Rand eingepasst; ohne Bild bleibt die Seite leer.";
            //
            // buttonOK
            //
            buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonOK.Location = new System.Drawing.Point(210, 156);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(95, 27);
            buttonOK.TabIndex = 7;
            buttonOK.Text = "Einfügen";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(311, 156);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(95, 27);
            buttonCancel.TabIndex = 8;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // InsertPageForm
            //
            AcceptButton = buttonOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(418, 195);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(labelHint);
            Controls.Add(buttonBrowse);
            Controls.Add(textBoxImage);
            Controls.Add(labelImage);
            Controls.Add(radioAfter);
            Controls.Add(radioBefore);
            Controls.Add(labelPrompt);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InsertPageForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Leere Seite einfügen";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelPrompt;
        private System.Windows.Forms.RadioButton radioBefore;
        private System.Windows.Forms.RadioButton radioAfter;
        private System.Windows.Forms.Label labelImage;
        private System.Windows.Forms.TextBox textBoxImage;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
