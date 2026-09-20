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
            labelInfo = new Label();
            labelPrompt = new Label();
            textBoxPages = new TextBox();
            labelHint = new Label();
            groupRotation = new GroupBox();
            radioRight = new RadioButton();
            radioLeft = new RadioButton();
            radioTurn = new RadioButton();
            buttonOK = new Button();
            buttonCancel = new Button();
            groupRotation.SuspendLayout();
            SuspendLayout();
            // 
            // labelInfo
            // 
            labelInfo.AutoSize = true;
            labelInfo.Location = new Point(12, 12);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(0, 15);
            labelInfo.TabIndex = 6;
            labelInfo.Visible = false;
            // 
            // labelPrompt
            // 
            labelPrompt.AutoSize = true;
            labelPrompt.Location = new Point(12, 15);
            labelPrompt.Name = "labelPrompt";
            labelPrompt.Size = new Size(77, 15);
            labelPrompt.TabIndex = 0;
            labelPrompt.Text = "&Seiten (1–99):";
            // 
            // textBoxPages
            // 
            textBoxPages.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxPages.Location = new Point(12, 38);
            textBoxPages.Name = "textBoxPages";
            textBoxPages.Size = new Size(269, 23);
            textBoxPages.TabIndex = 1;
            // 
            // labelHint
            // 
            labelHint.AutoSize = true;
            labelHint.ForeColor = SystemColors.GrayText;
            labelHint.Location = new Point(12, 66);
            labelHint.Name = "labelHint";
            labelHint.Size = new Size(108, 15);
            labelHint.TabIndex = 2;
            labelHint.Text = "z.B.  3   oder   2-5, 8";
            // 
            // groupRotation
            // 
            groupRotation.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupRotation.Controls.Add(radioRight);
            groupRotation.Controls.Add(radioLeft);
            groupRotation.Controls.Add(radioTurn);
            groupRotation.Location = new Point(12, 92);
            groupRotation.Name = "groupRotation";
            groupRotation.Size = new Size(269, 56);
            groupRotation.TabIndex = 3;
            groupRotation.TabStop = false;
            groupRotation.Text = "Drehung";
            // 
            // radioRight
            // 
            radioRight.AutoSize = true;
            radioRight.Checked = true;
            radioRight.Location = new Point(12, 22);
            radioRight.Name = "radioRight";
            radioRight.Size = new Size(77, 19);
            radioRight.TabIndex = 0;
            radioRight.TabStop = true;
            radioRight.Text = "90° &rechts";
            radioRight.UseVisualStyleBackColor = true;
            // 
            // radioLeft
            // 
            radioLeft.AutoSize = true;
            radioLeft.Location = new Point(110, 22);
            radioLeft.Name = "radioLeft";
            radioLeft.Size = new Size(69, 19);
            radioLeft.TabIndex = 1;
            radioLeft.Text = "90° &links";
            radioLeft.UseVisualStyleBackColor = true;
            // 
            // radioTurn
            // 
            radioTurn.AutoSize = true;
            radioTurn.Location = new Point(208, 22);
            radioTurn.Name = "radioTurn";
            radioTurn.Size = new Size(48, 19);
            radioTurn.TabIndex = 2;
            radioTurn.Text = "&180°";
            radioTurn.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(85, 160);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(95, 27);
            buttonOK.TabIndex = 4;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(186, 160);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(95, 27);
            buttonCancel.TabIndex = 5;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // PageRangeForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(293, 199);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(groupRotation);
            Controls.Add(labelHint);
            Controls.Add(textBoxPages);
            Controls.Add(labelPrompt);
            Controls.Add(labelInfo);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PageRangeForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Seiten";
            groupRotation.ResumeLayout(false);
            groupRotation.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelInfo;
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
