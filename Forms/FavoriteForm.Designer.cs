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
            labelFileValue = new System.Windows.Forms.Label();
            labelPage = new System.Windows.Forms.Label();
            labelName = new System.Windows.Forms.Label();
            textBoxName = new System.Windows.Forms.TextBox();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
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
            // labelPage
            //
            labelPage.AutoSize = true;
            labelPage.Location = new System.Drawing.Point(12, 38);
            labelPage.Name = "labelPage";
            labelPage.Size = new System.Drawing.Size(0, 15);
            labelPage.TabIndex = 1;
            //
            // labelName
            //
            labelName.AutoSize = true;
            labelName.Location = new System.Drawing.Point(12, 66);
            labelName.Name = "labelName";
            labelName.Size = new System.Drawing.Size(95, 15);
            labelName.TabIndex = 2;
            labelName.Text = "&Name (optional):";
            //
            // textBoxName
            //
            textBoxName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxName.Location = new System.Drawing.Point(12, 84);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new System.Drawing.Size(400, 23);
            textBoxName.TabIndex = 3;
            //
            // buttonOK
            //
            buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOK.Location = new System.Drawing.Point(216, 125);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(95, 27);
            buttonOK.TabIndex = 4;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(317, 125);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(95, 27);
            buttonCancel.TabIndex = 5;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // FavoriteForm
            //
            AcceptButton = buttonOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(424, 164);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(textBoxName);
            Controls.Add(labelName);
            Controls.Add(labelPage);
            Controls.Add(labelFileValue);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FavoriteForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Favorit hinzufügen";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelFileValue;
        private System.Windows.Forms.Label labelPage;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
