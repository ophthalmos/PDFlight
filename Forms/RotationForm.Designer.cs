namespace PDFLight.Forms
{
    partial class RotationForm
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
            labelPages = new Label();
            panelPages = new Panel();
            radioAllPages = new RadioButton();
            radioCurrentPage = new RadioButton();
            radioRange = new RadioButton();
            numFrom = new NumericUpDown();
            labelTo = new Label();
            numTo = new NumericUpDown();
            labelRotation = new Label();
            panelRotation = new Panel();
            radioRight = new RadioButton();
            radioLeft = new RadioButton();
            radioTurn = new RadioButton();
            buttonOK = new Button();
            buttonCancel = new Button();
            panelPages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numFrom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numTo).BeginInit();
            panelRotation.SuspendLayout();
            SuspendLayout();
            //
            // labelPages
            //
            labelPages.AutoSize = true;
            labelPages.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelPages.Location = new Point(12, 10);
            labelPages.Name = "labelPages";
            labelPages.Size = new Size(41, 15);
            labelPages.TabIndex = 0;
            labelPages.Text = "Seiten";
            //
            // panelPages
            //
            panelPages.Controls.Add(radioAllPages);
            panelPages.Controls.Add(radioCurrentPage);
            panelPages.Controls.Add(radioRange);
            panelPages.Controls.Add(numFrom);
            panelPages.Controls.Add(labelTo);
            panelPages.Controls.Add(numTo);
            panelPages.Location = new Point(12, 28);
            panelPages.Name = "panelPages";
            panelPages.Size = new Size(210, 81);
            panelPages.TabIndex = 1;
            //
            // radioAllPages
            //
            radioAllPages.AutoSize = true;
            radioAllPages.Location = new Point(2, 4);
            radioAllPages.Name = "radioAllPages";
            radioAllPages.Size = new Size(83, 19);
            radioAllPages.TabIndex = 0;
            radioAllPages.Text = "Alle Seiten";
            radioAllPages.UseVisualStyleBackColor = true;
            //
            // radioCurrentPage
            //
            radioCurrentPage.AutoSize = true;
            radioCurrentPage.Checked = true;
            radioCurrentPage.Location = new Point(2, 29);
            radioCurrentPage.Name = "radioCurrentPage";
            radioCurrentPage.Size = new Size(101, 19);
            radioCurrentPage.TabIndex = 1;
            radioCurrentPage.TabStop = true;
            radioCurrentPage.Text = "Aktuelle Seite";
            radioCurrentPage.UseVisualStyleBackColor = true;
            //
            // radioRange
            //
            radioRange.AutoSize = true;
            radioRange.Location = new Point(2, 56);
            radioRange.Name = "radioRange";
            radioRange.Size = new Size(46, 19);
            radioRange.TabIndex = 2;
            radioRange.Text = "Von";
            radioRange.UseVisualStyleBackColor = true;
            //
            // numFrom
            //
            numFrom.Location = new Point(62, 54);
            numFrom.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numFrom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numFrom.Name = "numFrom";
            numFrom.Size = new Size(58, 23);
            numFrom.TabIndex = 3;
            numFrom.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numFrom.ValueChanged += NumFrom_ValueChanged;
            numFrom.Enter += NumRange_Enter;
            //
            // labelTo
            //
            labelTo.AutoSize = true;
            labelTo.Location = new Point(124, 57);
            labelTo.Name = "labelTo";
            labelTo.Size = new Size(20, 15);
            labelTo.TabIndex = 4;
            labelTo.Text = "bis";
            //
            // numTo
            //
            numTo.Location = new Point(150, 54);
            numTo.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numTo.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numTo.Name = "numTo";
            numTo.Size = new Size(58, 23);
            numTo.TabIndex = 5;
            numTo.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numTo.ValueChanged += NumTo_ValueChanged;
            numTo.Enter += NumRange_Enter;
            //
            // labelRotation
            //
            labelRotation.AutoSize = true;
            labelRotation.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelRotation.Location = new Point(12, 119);
            labelRotation.Name = "labelRotation";
            labelRotation.Size = new Size(54, 15);
            labelRotation.TabIndex = 2;
            labelRotation.Text = "Drehung";
            //
            // panelRotation
            //
            panelRotation.Controls.Add(radioRight);
            panelRotation.Controls.Add(radioLeft);
            panelRotation.Controls.Add(radioTurn);
            panelRotation.Location = new Point(12, 137);
            panelRotation.Name = "panelRotation";
            panelRotation.Size = new Size(210, 78);
            panelRotation.TabIndex = 3;
            //
            // radioRight
            //
            radioRight.AutoSize = true;
            radioRight.Checked = true;
            radioRight.Location = new Point(2, 4);
            radioRight.Name = "radioRight";
            radioRight.Size = new Size(77, 19);
            radioRight.TabIndex = 0;
            radioRight.TabStop = true;
            radioRight.Text = "90° &rechts";
            radioRight.UseVisualStyleBackColor = true;
            radioRight.MouseDoubleClick += RadioRotation_MouseDoubleClick;
            //
            // radioLeft
            //
            radioLeft.AutoSize = true;
            radioLeft.Location = new Point(2, 29);
            radioLeft.Name = "radioLeft";
            radioLeft.Size = new Size(69, 19);
            radioLeft.TabIndex = 1;
            radioLeft.Text = "90° &links";
            radioLeft.UseVisualStyleBackColor = true;
            radioLeft.MouseDoubleClick += RadioRotation_MouseDoubleClick;
            //
            // radioTurn
            //
            radioTurn.AutoSize = true;
            radioTurn.Location = new Point(2, 54);
            radioTurn.Name = "radioTurn";
            radioTurn.Size = new Size(48, 19);
            radioTurn.TabIndex = 2;
            radioTurn.Text = "&180°";
            radioTurn.UseVisualStyleBackColor = true;
            radioTurn.MouseDoubleClick += RadioRotation_MouseDoubleClick;
            //
            // buttonOK
            //
            buttonOK.Location = new Point(12, 225);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(102, 27);
            buttonOK.TabIndex = 4;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            //
            // buttonCancel
            //
            buttonCancel.Location = new Point(120, 225);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(102, 27);
            buttonCancel.TabIndex = 5;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += ButtonCancel_Click;
            //
            // RotationForm
            //
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(234, 264);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(panelRotation);
            Controls.Add(labelRotation);
            Controls.Add(panelPages);
            Controls.Add(labelPages);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RotationForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "Seiten drehen";
            panelPages.ResumeLayout(false);
            panelPages.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numFrom).EndInit();
            ((System.ComponentModel.ISupportInitialize)numTo).EndInit();
            panelRotation.ResumeLayout(false);
            panelRotation.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelPages;
        private Panel panelPages;
        private RadioButton radioAllPages;
        private RadioButton radioCurrentPage;
        private RadioButton radioRange;
        private NumericUpDown numFrom;
        private Label labelTo;
        private NumericUpDown numTo;
        private Label labelRotation;
        private Panel panelRotation;
        private RadioButton radioRight;
        private RadioButton radioLeft;
        private RadioButton radioTurn;
        private Button buttonOK;
        private Button buttonCancel;
    }
}
