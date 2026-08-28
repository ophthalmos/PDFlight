namespace PDFLight.Forms
{
    partial class PageRangeForm
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
            textBoxPages = new System.Windows.Forms.TextBox();
            labelHint = new System.Windows.Forms.Label();
            groupRotation = new System.Windows.Forms.GroupBox();
            radioRight = new System.Windows.Forms.RadioButton();
            radioLeft = new System.Windows.Forms.RadioButton();
            radioTurn = new System.Windows.Forms.RadioButton();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            groupRotation.SuspendLayout();
            SuspendLayout();
            //
            // labelPrompt
            //
            labelPrompt.AutoSize = true;
            labelPrompt.Location = new System.Drawing.Point(12, 15);
            labelPrompt.Name = "labelPrompt";
            labelPrompt.Size = new System.Drawing.Size(80, 15);
            labelPrompt.TabIndex = 0;
            labelPrompt.Text = "&Seiten (1–99):";
            //
            // textBoxPages
            //
            textBoxPages.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxPages.Location = new System.Drawing.Point(12, 38);
            textBoxPages.Name = "textBoxPages";
            textBoxPages.Size = new System.Drawing.Size(360, 23);
            textBoxPages.TabIndex = 1;
            //
            // labelHint
            //
            labelHint.AutoSize = true;
            labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            labelHint.Location = new System.Drawing.Point(12, 66);
            labelHint.Name = "labelHint";
            labelHint.Size = new System.Drawing.Size(140, 15);
            labelHint.TabIndex = 2;
            labelHint.Text = "z.B.  3   oder   2-5, 8";
            //
            // groupRotation
            //
            groupRotation.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupRotation.Controls.Add(radioRight);
            groupRotation.Controls.Add(radioLeft);
            groupRotation.Controls.Add(radioTurn);
            groupRotation.Location = new System.Drawing.Point(12, 92);
            groupRotation.Name = "groupRotation";
            groupRotation.Size = new System.Drawing.Size(360, 56);
            groupRotation.TabIndex = 3;
            groupRotation.TabStop = false;
            groupRotation.Text = "Drehung";
            //
            // radioRight
            //
            radioRight.AutoSize = true;
            radioRight.Checked = true;
            radioRight.Location = new System.Drawing.Point(12, 22);
            radioRight.Name = "radioRight";
            radioRight.Size = new System.Drawing.Size(80, 19);
            radioRight.TabIndex = 0;
            radioRight.TabStop = true;
            radioRight.Text = "90° &rechts";
            radioRight.UseVisualStyleBackColor = true;
            //
            // radioLeft
            //
            radioLeft.AutoSize = true;
            radioLeft.Location = new System.Drawing.Point(120, 22);
            radioLeft.Name = "radioLeft";
            radioLeft.Size = new System.Drawing.Size(72, 19);
            radioLeft.TabIndex = 1;
            radioLeft.Text = "90° &links";
            radioLeft.UseVisualStyleBackColor = true;
            //
            // radioTurn
            //
            radioTurn.AutoSize = true;
            radioTurn.Location = new System.Drawing.Point(228, 22);
            radioTurn.Name = "radioTurn";
            radioTurn.Size = new System.Drawing.Size(50, 19);
            radioTurn.TabIndex = 2;
            radioTurn.Text = "&180°";
            radioTurn.UseVisualStyleBackColor = true;
            //
            // buttonOK
            //
            buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOK.Location = new System.Drawing.Point(176, 160);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(95, 27);
            buttonOK.TabIndex = 4;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(277, 160);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(95, 27);
            buttonCancel.TabIndex = 5;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // PageRangeForm
            //
            AcceptButton = buttonOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(384, 199);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(groupRotation);
            Controls.Add(labelHint);
            Controls.Add(textBoxPages);
            Controls.Add(labelPrompt);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PageRangeForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Seiten";
            groupRotation.ResumeLayout(false);
            groupRotation.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelPrompt;
        private System.Windows.Forms.TextBox textBoxPages;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.GroupBox groupRotation;
        private System.Windows.Forms.RadioButton radioRight;
        private System.Windows.Forms.RadioButton radioLeft;
        private System.Windows.Forms.RadioButton radioTurn;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
