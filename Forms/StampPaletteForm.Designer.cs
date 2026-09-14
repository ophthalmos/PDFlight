namespace PDFLight.Forms
{
    partial class StampPaletteForm
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
            labelPage = new Label();
            listStamps = new ListBox();
            labelHint = new Label();
            buttonOK = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            // 
            // labelPage
            // 
            labelPage.AutoSize = true;
            labelPage.Location = new Point(12, 12);
            labelPage.Name = "labelPage";
            labelPage.Size = new Size(181, 15);
            labelPage.TabIndex = 0;
            labelPage.Text = "Der Stempel kommt auf Seite {0}.";
            // 
            // listStamps
            // 
            listStamps.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listStamps.DrawMode = DrawMode.OwnerDrawFixed;
            listStamps.IntegralHeight = false;
            listStamps.ItemHeight = 44;
            listStamps.Location = new Point(12, 34);
            listStamps.Name = "listStamps";
            listStamps.Size = new Size(360, 254);
            listStamps.TabIndex = 1;
            listStamps.DrawItem += ListStamps_DrawItem;
            listStamps.DoubleClick += ListStamps_DoubleClick;
            // 
            // labelHint
            // 
            labelHint.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelHint.AutoSize = true;
            labelHint.ForeColor = SystemColors.GrayText;
            labelHint.Location = new Point(12, 296);
            labelHint.Name = "labelHint";
            labelHint.Size = new Size(202, 15);
            labelHint.TabIndex = 2;
            labelHint.Text = "Doppelklick setzt den Stempel sofort.";
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(176, 322);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(95, 27);
            buttonOK.TabIndex = 3;
            buttonOK.Text = "Einfügen";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(277, 322);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(95, 27);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // StampPaletteForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(384, 361);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(labelHint);
            Controls.Add(listStamps);
            Controls.Add(labelPage);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "StampPaletteForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Stempel einfügen";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelPage;
        private System.Windows.Forms.ListBox listStamps;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
