namespace PDFLight.Forms
{
    partial class PasswordForm
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
            labelInfo = new System.Windows.Forms.Label();
            labelPassword = new System.Windows.Forms.Label();
            textBoxPassword = new System.Windows.Forms.TextBox();
            labelRepeat = new System.Windows.Forms.Label();
            textBoxRepeat = new System.Windows.Forms.TextBox();
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
            // labelInfo
            //
            labelInfo.AutoSize = true;
            labelInfo.Location = new System.Drawing.Point(12, 38);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new System.Drawing.Size(400, 45);
            labelInfo.TabIndex = 7;
            labelInfo.Visible = false;
            //
            // labelPassword
            //
            labelPassword.AutoSize = true;
            labelPassword.Location = new System.Drawing.Point(12, 50);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new System.Drawing.Size(63, 15);
            labelPassword.TabIndex = 1;
            labelPassword.Text = "&Kennwort:";
            //
            // textBoxPassword
            //
            textBoxPassword.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxPassword.Location = new System.Drawing.Point(100, 47);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.Size = new System.Drawing.Size(312, 23);
            textBoxPassword.TabIndex = 2;
            textBoxPassword.UseSystemPasswordChar = true;
            //
            // labelRepeat
            //
            labelRepeat.AutoSize = true;
            labelRepeat.Location = new System.Drawing.Point(12, 83);
            labelRepeat.Name = "labelRepeat";
            labelRepeat.Size = new System.Drawing.Size(79, 15);
            labelRepeat.TabIndex = 3;
            labelRepeat.Text = "&Wiederholen:";
            //
            // textBoxRepeat
            //
            textBoxRepeat.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxRepeat.Location = new System.Drawing.Point(100, 80);
            textBoxRepeat.Name = "textBoxRepeat";
            textBoxRepeat.Size = new System.Drawing.Size(312, 23);
            textBoxRepeat.TabIndex = 4;
            textBoxRepeat.UseSystemPasswordChar = true;
            //
            // buttonOK
            //
            buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOK.Location = new System.Drawing.Point(216, 90);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(95, 27);
            buttonOK.TabIndex = 5;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(317, 90);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(95, 27);
            buttonCancel.TabIndex = 6;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // PasswordForm
            //
            AcceptButton = buttonOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(424, 129);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(textBoxRepeat);
            Controls.Add(labelRepeat);
            Controls.Add(textBoxPassword);
            Controls.Add(labelPassword);
            Controls.Add(labelInfo);
            Controls.Add(labelFileValue);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PasswordForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Benutzer-Kennwort entfernen";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelFileValue;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelRepeat;
        private System.Windows.Forms.TextBox textBoxRepeat;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
