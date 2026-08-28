namespace PDFLight.Forms
{
    partial class PropertiesForm
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
            labelFile = new System.Windows.Forms.Label();
            labelFileValue = new System.Windows.Forms.Label();
            labelInfoValue = new System.Windows.Forms.Label();
            labelTitle = new System.Windows.Forms.Label();
            textBoxTitle = new System.Windows.Forms.TextBox();
            labelAuthor = new System.Windows.Forms.Label();
            textBoxAuthor = new System.Windows.Forms.TextBox();
            labelSubject = new System.Windows.Forms.Label();
            textBoxSubject = new System.Windows.Forms.TextBox();
            labelKeywords = new System.Windows.Forms.Label();
            textBoxKeywords = new System.Windows.Forms.TextBox();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // labelFile
            //
            labelFile.AutoSize = true;
            labelFile.Location = new System.Drawing.Point(12, 12);
            labelFile.Name = "labelFile";
            labelFile.Size = new System.Drawing.Size(37, 15);
            labelFile.TabIndex = 0;
            labelFile.Text = "Datei:";
            //
            // labelFileValue
            //
            labelFileValue.AutoEllipsis = true;
            labelFileValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            labelFileValue.Location = new System.Drawing.Point(90, 12);
            labelFileValue.Name = "labelFileValue";
            labelFileValue.Size = new System.Drawing.Size(392, 15);
            labelFileValue.TabIndex = 1;
            labelFileValue.Text = "datei.pdf";
            //
            // labelInfoValue
            //
            labelInfoValue.AutoEllipsis = true;
            labelInfoValue.ForeColor = System.Drawing.SystemColors.GrayText;
            labelInfoValue.Location = new System.Drawing.Point(90, 32);
            labelInfoValue.Name = "labelInfoValue";
            labelInfoValue.Size = new System.Drawing.Size(392, 15);
            labelInfoValue.TabIndex = 2;
            labelInfoValue.Text = "…";
            //
            // labelTitle
            //
            labelTitle.AutoSize = true;
            labelTitle.Location = new System.Drawing.Point(12, 68);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new System.Drawing.Size(33, 15);
            labelTitle.TabIndex = 3;
            labelTitle.Text = "&Titel:";
            //
            // textBoxTitle
            //
            textBoxTitle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxTitle.Location = new System.Drawing.Point(90, 65);
            textBoxTitle.Name = "textBoxTitle";
            textBoxTitle.Size = new System.Drawing.Size(392, 23);
            textBoxTitle.TabIndex = 4;
            //
            // labelAuthor
            //
            labelAuthor.AutoSize = true;
            labelAuthor.Location = new System.Drawing.Point(12, 97);
            labelAuthor.Name = "labelAuthor";
            labelAuthor.Size = new System.Drawing.Size(46, 15);
            labelAuthor.TabIndex = 5;
            labelAuthor.Text = "&Autor:";
            //
            // textBoxAuthor
            //
            textBoxAuthor.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxAuthor.Location = new System.Drawing.Point(90, 94);
            textBoxAuthor.Name = "textBoxAuthor";
            textBoxAuthor.Size = new System.Drawing.Size(392, 23);
            textBoxAuthor.TabIndex = 6;
            //
            // labelSubject
            //
            labelSubject.AutoSize = true;
            labelSubject.Location = new System.Drawing.Point(12, 126);
            labelSubject.Name = "labelSubject";
            labelSubject.Size = new System.Drawing.Size(48, 15);
            labelSubject.TabIndex = 7;
            labelSubject.Text = "&Betreff:";
            //
            // textBoxSubject
            //
            textBoxSubject.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxSubject.Location = new System.Drawing.Point(90, 123);
            textBoxSubject.Name = "textBoxSubject";
            textBoxSubject.Size = new System.Drawing.Size(392, 23);
            textBoxSubject.TabIndex = 8;
            //
            // labelKeywords
            //
            labelKeywords.AutoSize = true;
            labelKeywords.Location = new System.Drawing.Point(12, 155);
            labelKeywords.Name = "labelKeywords";
            labelKeywords.Size = new System.Drawing.Size(72, 15);
            labelKeywords.TabIndex = 9;
            labelKeywords.Text = "&Stichwörter:";
            //
            // textBoxKeywords
            //
            textBoxKeywords.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxKeywords.Location = new System.Drawing.Point(90, 152);
            textBoxKeywords.Name = "textBoxKeywords";
            textBoxKeywords.Size = new System.Drawing.Size(392, 23);
            textBoxKeywords.TabIndex = 10;
            //
            // buttonOK
            //
            buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOK.Location = new System.Drawing.Point(286, 196);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(95, 27);
            buttonOK.TabIndex = 11;
            buttonOK.Text = "Speichern";
            buttonOK.UseVisualStyleBackColor = true;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(387, 196);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(95, 27);
            buttonCancel.TabIndex = 12;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // PropertiesForm
            //
            AcceptButton = buttonOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(494, 235);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(textBoxKeywords);
            Controls.Add(labelKeywords);
            Controls.Add(textBoxSubject);
            Controls.Add(labelSubject);
            Controls.Add(textBoxAuthor);
            Controls.Add(labelAuthor);
            Controls.Add(textBoxTitle);
            Controls.Add(labelTitle);
            Controls.Add(labelInfoValue);
            Controls.Add(labelFileValue);
            Controls.Add(labelFile);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PropertiesForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Dokumenteigenschaften";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelFile;
        private System.Windows.Forms.Label labelFileValue;
        private System.Windows.Forms.Label labelInfoValue;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.TextBox textBoxTitle;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.TextBox textBoxAuthor;
        private System.Windows.Forms.Label labelSubject;
        private System.Windows.Forms.TextBox textBoxSubject;
        private System.Windows.Forms.Label labelKeywords;
        private System.Windows.Forms.TextBox textBoxKeywords;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
