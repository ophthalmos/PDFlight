namespace PDFLight.Forms
{
    partial class SignatureForm
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
            labelHint = new Label();
            signaturePad = new PDFLight.Controls.SignaturePad();
            labelStorage = new Label();
            buttonLoadImage = new Button();
            buttonClear = new Button();
            buttonOK = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            // 
            // labelHint
            // 
            labelHint.AutoSize = true;
            labelHint.Location = new Point(12, 12);
            labelHint.Name = "labelHint";
            labelHint.Size = new Size(518, 15);
            labelHint.TabIndex = 0;
            labelHint.Text = "Hier kannst du die Unterschrift direkt zeichnen oder eine Bilddatei deiner Unterschrift importieren";
            // 
            // signaturePad
            // 
            signaturePad.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            signaturePad.BackColor = Color.White;
            signaturePad.Cursor = Cursors.Cross;
            signaturePad.EmptyText = "Hier unterschreiben";
            signaturePad.Location = new Point(12, 36);
            signaturePad.Name = "signaturePad";
            signaturePad.Size = new Size(536, 200);
            signaturePad.TabIndex = 1;
            signaturePad.ContentChanged += SignaturePad_ContentChanged;
            // 
            // labelStorage
            // 
            labelStorage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelStorage.ForeColor = SystemColors.GrayText;
            labelStorage.Location = new Point(12, 242);
            labelStorage.Name = "labelStorage";
            labelStorage.Size = new Size(536, 32);
            labelStorage.TabIndex = 6;
            labelStorage.Text = "Speicherort: {0} (verschlüsselt über Windows-Datenschutz-API)\r\nsignatures.dat ist ausschließlich mit dem aktuellen Windows-Benutzerkonto auf diesem PC lesbar.";
            // 
            // buttonLoadImage
            // 
            buttonLoadImage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonLoadImage.Location = new Point(12, 282);
            buttonLoadImage.Name = "buttonLoadImage";
            buttonLoadImage.Size = new Size(140, 27);
            buttonLoadImage.TabIndex = 2;
            buttonLoadImage.Text = "&Bild laden…";
            buttonLoadImage.UseVisualStyleBackColor = true;
            buttonLoadImage.Click += ButtonLoadImage_Click;
            // 
            // buttonClear
            // 
            buttonClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonClear.Enabled = false;
            buttonClear.Location = new Point(158, 282);
            buttonClear.Name = "buttonClear";
            buttonClear.Size = new Size(95, 27);
            buttonClear.TabIndex = 3;
            buttonClear.Text = "&Leeren";
            buttonClear.UseVisualStyleBackColor = true;
            buttonClear.Click += ButtonClear_Click;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Enabled = false;
            buttonOK.Location = new Point(352, 282);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(95, 27);
            buttonOK.TabIndex = 4;
            buttonOK.Text = "Übernehmen";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(453, 282);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(95, 27);
            buttonCancel.TabIndex = 5;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // SignatureForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(560, 321);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(buttonClear);
            Controls.Add(buttonLoadImage);
            Controls.Add(labelStorage);
            Controls.Add(signaturePad);
            Controls.Add(labelHint);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SignatureForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Unterschrift hinzufügen";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelHint;
        private PDFLight.Controls.SignaturePad signaturePad;
        private System.Windows.Forms.Label labelStorage;
        private System.Windows.Forms.Button buttonLoadImage;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
