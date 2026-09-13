namespace PDFLight.Forms
{
    partial class AnnotationForm
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
            labelPage = new Label();
            labelText = new Label();
            textBoxText = new TextBox();
            labelInfo = new Label();
            labelLeft = new Label();
            numLeft = new NumericUpDown();
            labelTop = new Label();
            numTop = new NumericUpDown();
            labelSize = new Label();
            numSize = new NumericUpDown();
            labelBackground = new Label();
            comboBackground = new ComboBox();
            cbBorder = new CheckBox();
            previewWebView = new Microsoft.Web.WebView2.WinForms.WebView2();
            picturePreview = new PictureBox();
            labelPreviewState = new Label();
            buttonOK = new Button();
            buttonCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)numLeft).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numTop).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)previewWebView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picturePreview).BeginInit();
            SuspendLayout();
            // 
            // labelFileValue
            // 
            labelFileValue.AutoEllipsis = true;
            labelFileValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelFileValue.Location = new Point(358, 9);
            labelFileValue.Name = "labelFileValue";
            labelFileValue.Size = new Size(269, 15);
            labelFileValue.TabIndex = 0;
            labelFileValue.Text = "datei.pdf";
            // 
            // labelPage
            // 
            labelPage.AutoSize = true;
            labelPage.Location = new Point(357, 32);
            labelPage.Name = "labelPage";
            labelPage.Size = new Size(0, 15);
            labelPage.TabIndex = 1;
            // 
            // labelText
            // 
            labelText.AutoSize = true;
            labelText.Location = new Point(358, 62);
            labelText.Name = "labelText";
            labelText.Size = new Size(31, 15);
            labelText.TabIndex = 2;
            labelText.Text = "&Text:";
            // 
            // textBoxText
            // 
            textBoxText.AcceptsReturn = true;
            textBoxText.Location = new Point(358, 80);
            textBoxText.Multiline = true;
            textBoxText.Name = "textBoxText";
            textBoxText.ScrollBars = ScrollBars.Vertical;
            textBoxText.Size = new Size(257, 84);
            textBoxText.TabIndex = 3;
            textBoxText.TextChanged += Preview_Changed;
            // 
            // labelInfo
            // 
            labelInfo.ForeColor = SystemColors.GrayText;
            labelInfo.Location = new Point(358, 167);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(258, 30);
            labelInfo.TabIndex = 4;
            labelInfo.Text = "Maße ab der linken oberen Ecke der Seite.";
            // 
            // labelLeft
            // 
            labelLeft.AutoSize = true;
            labelLeft.Location = new Point(358, 214);
            labelLeft.Name = "labelLeft";
            labelLeft.Size = new Size(137, 15);
            labelLeft.TabIndex = 5;
            labelLeft.Text = "Abstand von &links (mm):";
            // 
            // numLeft
            // 
            numLeft.DecimalPlaces = 1;
            numLeft.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numLeft.Location = new Point(535, 212);
            numLeft.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numLeft.Name = "numLeft";
            numLeft.Size = new Size(80, 23);
            numLeft.TabIndex = 6;
            numLeft.Value = new decimal(new int[] { 20, 0, 0, 0 });
            numLeft.ValueChanged += Preview_Changed;
            // 
            // labelTop
            // 
            labelTop.AutoSize = true;
            labelTop.Location = new Point(358, 243);
            labelTop.Name = "labelTop";
            labelTop.Size = new Size(140, 15);
            labelTop.TabIndex = 7;
            labelTop.Text = "Abstand von &oben (mm):";
            // 
            // numTop
            // 
            numTop.DecimalPlaces = 1;
            numTop.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numTop.Location = new Point(535, 241);
            numTop.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numTop.Name = "numTop";
            numTop.Size = new Size(80, 23);
            numTop.TabIndex = 8;
            numTop.Value = new decimal(new int[] { 20, 0, 0, 0 });
            numTop.ValueChanged += Preview_Changed;
            // 
            // labelSize
            // 
            labelSize.AutoSize = true;
            labelSize.Location = new Point(358, 272);
            labelSize.Name = "labelSize";
            labelSize.Size = new Size(97, 15);
            labelSize.TabIndex = 9;
            labelSize.Text = "Schrift&größe (pt):";
            // 
            // numSize
            // 
            numSize.Location = new Point(535, 270);
            numSize.Maximum = new decimal(new int[] { 72, 0, 0, 0 });
            numSize.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            numSize.Name = "numSize";
            numSize.Size = new Size(80, 23);
            numSize.TabIndex = 10;
            numSize.Value = new decimal(new int[] { 12, 0, 0, 0 });
            numSize.ValueChanged += Preview_Changed;
            // 
            // labelBackground
            // 
            labelBackground.AutoSize = true;
            labelBackground.Location = new Point(358, 302);
            labelBackground.Name = "labelBackground";
            labelBackground.Size = new Size(75, 15);
            labelBackground.TabIndex = 11;
            labelBackground.Text = "&Hintergrund:";
            // 
            // comboBackground
            // 
            comboBackground.DrawMode = DrawMode.OwnerDrawFixed;
            comboBackground.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBackground.Location = new Point(495, 299);
            comboBackground.Name = "comboBackground";
            comboBackground.Size = new Size(120, 23);
            comboBackground.TabIndex = 12;
            comboBackground.DrawItem += ComboBackground_DrawItem;
            comboBackground.SelectedIndexChanged += Preview_Changed;
            // 
            // cbBorder
            // 
            cbBorder.AutoSize = true;
            cbBorder.Checked = true;
            cbBorder.CheckState = CheckState.Checked;
            cbBorder.Location = new Point(358, 332);
            cbBorder.Name = "cbBorder";
            cbBorder.Size = new Size(70, 19);
            cbBorder.TabIndex = 13;
            cbBorder.Text = "&Rahmen";
            cbBorder.UseVisualStyleBackColor = true;
            cbBorder.CheckedChanged += Preview_Changed;
            // 
            // previewWebView
            // 
            previewWebView.AllowExternalDrop = false;
            previewWebView.CreationProperties = null;
            previewWebView.DefaultBackgroundColor = Color.White;
            previewWebView.Location = new Point(12, 12);
            previewWebView.Name = "previewWebView";
            previewWebView.Size = new Size(340, 415);
            previewWebView.TabIndex = 14;
            previewWebView.TabStop = false;
            previewWebView.ZoomFactor = 1D;
            // 
            // picturePreview
            // 
            picturePreview.BackColor = SystemColors.ControlLight;
            picturePreview.BorderStyle = BorderStyle.FixedSingle;
            picturePreview.Cursor = Cursors.Cross;
            picturePreview.Location = new Point(12, 12);
            picturePreview.Name = "picturePreview";
            picturePreview.Size = new Size(340, 415);
            picturePreview.TabIndex = 15;
            picturePreview.TabStop = false;
            picturePreview.Paint += PicturePreview_Paint;
            picturePreview.MouseClick += PicturePreview_MouseClick;
            // 
            // labelPreviewState
            // 
            labelPreviewState.ForeColor = SystemColors.GrayText;
            labelPreviewState.Location = new Point(12, 430);
            labelPreviewState.Name = "labelPreviewState";
            labelPreviewState.Size = new Size(340, 15);
            labelPreviewState.TabIndex = 16;
            labelPreviewState.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonOK.Location = new Point(420, 424);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(95, 27);
            buttonOK.TabIndex = 17;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(521, 424);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(95, 27);
            buttonCancel.TabIndex = 18;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // AnnotationForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(627, 463);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(labelPreviewState);
            Controls.Add(picturePreview);
            Controls.Add(previewWebView);
            Controls.Add(cbBorder);
            Controls.Add(comboBackground);
            Controls.Add(labelBackground);
            Controls.Add(numSize);
            Controls.Add(labelSize);
            Controls.Add(numTop);
            Controls.Add(labelTop);
            Controls.Add(numLeft);
            Controls.Add(labelLeft);
            Controls.Add(labelInfo);
            Controls.Add(textBoxText);
            Controls.Add(labelText);
            Controls.Add(labelPage);
            Controls.Add(labelFileValue);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AnnotationForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Textanmerkung hinzufügen";
            FormClosed += AnnotationForm_FormClosed;
            Shown += AnnotationForm_Shown;
            ((System.ComponentModel.ISupportInitialize)numLeft).EndInit();
            ((System.ComponentModel.ISupportInitialize)numTop).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)previewWebView).EndInit();
            ((System.ComponentModel.ISupportInitialize)picturePreview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelFileValue;
        private System.Windows.Forms.Label labelPage;
        private System.Windows.Forms.Label labelText;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelLeft;
        private System.Windows.Forms.NumericUpDown numLeft;
        private System.Windows.Forms.Label labelTop;
        private System.Windows.Forms.NumericUpDown numTop;
        private System.Windows.Forms.Label labelSize;
        private System.Windows.Forms.NumericUpDown numSize;
        private System.Windows.Forms.Label labelBackground;
        private System.Windows.Forms.ComboBox comboBackground;
        private System.Windows.Forms.CheckBox cbBorder;
        private Microsoft.Web.WebView2.WinForms.WebView2 previewWebView;
        private System.Windows.Forms.PictureBox picturePreview;
        private System.Windows.Forms.Label labelPreviewState;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
