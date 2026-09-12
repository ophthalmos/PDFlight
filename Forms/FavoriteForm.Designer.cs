namespace PDFLight.Forms
{
    partial class FavoriteForm
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
            labelInfo = new Label();
            labelName = new Label();
            textBoxName = new TextBox();
            buttonOK = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            // 
            // labelFileValue
            // 
            labelFileValue.AutoEllipsis = true;
            labelFileValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelFileValue.Location = new Point(12, 12);
            labelFileValue.Name = "labelFileValue";
            labelFileValue.Size = new Size(256, 15);
            labelFileValue.TabIndex = 0;
            labelFileValue.Text = "datei.pdf";
            // 
            // labelInfo
            // 
            labelInfo.AutoSize = true;
            labelInfo.Location = new Point(12, 38);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(212, 15);
            labelInfo.TabIndex = 1;
            labelInfo.Text = "Ohne Namen erscheint der Dateiname.";
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Location = new Point(12, 66);
            labelName.Name = "labelName";
            labelName.Size = new Size(178, 15);
            labelName.TabIndex = 2;
            labelName.Text = "&Name (optional, bis 30 Zeichen):";
            // 
            // textBoxName
            // 
            textBoxName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxName.Location = new Point(12, 84);
            textBoxName.MaxLength = 30;
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(256, 23);
            textBoxName.TabIndex = 3;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(72, 125);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(95, 27);
            buttonOK.TabIndex = 4;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(173, 125);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(95, 27);
            buttonCancel.TabIndex = 5;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // FavoriteForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(280, 164);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(textBoxName);
            Controls.Add(labelName);
            Controls.Add(labelInfo);
            Controls.Add(labelFileValue);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FavoriteForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Favorit hinzufügen";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelFileValue;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
