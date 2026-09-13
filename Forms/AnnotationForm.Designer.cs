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
            labelFileValue = new System.Windows.Forms.Label();
            labelPage = new System.Windows.Forms.Label();
            labelText = new System.Windows.Forms.Label();
            textBoxText = new System.Windows.Forms.TextBox();
            labelInfo = new System.Windows.Forms.Label();
            labelLeft = new System.Windows.Forms.Label();
            numLeft = new System.Windows.Forms.NumericUpDown();
            labelTop = new System.Windows.Forms.Label();
            numTop = new System.Windows.Forms.NumericUpDown();
            labelSize = new System.Windows.Forms.Label();
            numSize = new System.Windows.Forms.NumericUpDown();
            previewWebView = new Microsoft.Web.WebView2.WinForms.WebView2();
            picturePreview = new System.Windows.Forms.PictureBox();
            labelPreviewState = new System.Windows.Forms.Label();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
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
            labelFileValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            labelFileValue.Location = new System.Drawing.Point(12, 12);
            labelFileValue.Name = "labelFileValue";
            labelFileValue.Size = new System.Drawing.Size(300, 15);
            labelFileValue.TabIndex = 0;
            labelFileValue.Text = "datei.pdf";
            //
            // labelPage
            //
            labelPage.AutoSize = true;
            labelPage.Location = new System.Drawing.Point(12, 32);
            labelPage.Name = "labelPage";
            labelPage.Size = new System.Drawing.Size(80, 15);
            labelPage.TabIndex = 1;
            //
            // labelText
            //
            labelText.AutoSize = true;
            labelText.Location = new System.Drawing.Point(12, 62);
            labelText.Name = "labelText";
            labelText.Size = new System.Drawing.Size(34, 15);
            labelText.TabIndex = 2;
            labelText.Text = "&Text:";
            //
            // textBoxText
            //
            textBoxText.AcceptsReturn = true;
            textBoxText.Location = new System.Drawing.Point(12, 80);
            textBoxText.Multiline = true;
            textBoxText.Name = "textBoxText";
            textBoxText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBoxText.Size = new System.Drawing.Size(360, 84);
            textBoxText.TabIndex = 3;
            textBoxText.TextChanged += Preview_Changed;
            //
            // labelInfo
            //
            labelInfo.ForeColor = System.Drawing.SystemColors.GrayText;
            labelInfo.Location = new System.Drawing.Point(12, 170);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new System.Drawing.Size(360, 30);
            labelInfo.TabIndex = 4;
            labelInfo.Text = "Der Text erscheint als gelber Kasten; Maße ab der linken oberen Ecke der Seite.";
            //
            // labelLeft
            //
            labelLeft.AutoSize = true;
            labelLeft.Location = new System.Drawing.Point(12, 215);
            labelLeft.Name = "labelLeft";
            labelLeft.Size = new System.Drawing.Size(140, 15);
            labelLeft.TabIndex = 5;
            labelLeft.Text = "Abstand von &links (mm):";
            //
            // numLeft
            //
            numLeft.DecimalPlaces = 1;
            numLeft.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numLeft.Location = new System.Drawing.Point(190, 212);
            numLeft.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numLeft.Name = "numLeft";
            numLeft.Size = new System.Drawing.Size(80, 23);
            numLeft.TabIndex = 6;
            numLeft.Value = new decimal(new int[] { 20, 0, 0, 0 });
            numLeft.ValueChanged += Preview_Changed;
            //
            // labelTop
            //
            labelTop.AutoSize = true;
            labelTop.Location = new System.Drawing.Point(12, 244);
            labelTop.Name = "labelTop";
            labelTop.Size = new System.Drawing.Size(140, 15);
            labelTop.TabIndex = 7;
            labelTop.Text = "Abstand von &oben (mm):";
            //
            // numTop
            //
            numTop.DecimalPlaces = 1;
            numTop.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numTop.Location = new System.Drawing.Point(190, 241);
            numTop.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numTop.Name = "numTop";
            numTop.Size = new System.Drawing.Size(80, 23);
            numTop.TabIndex = 8;
            numTop.Value = new decimal(new int[] { 20, 0, 0, 0 });
            numTop.ValueChanged += Preview_Changed;
            //
            // labelSize
            //
            labelSize.AutoSize = true;
            labelSize.Location = new System.Drawing.Point(12, 273);
            labelSize.Name = "labelSize";
            labelSize.Size = new System.Drawing.Size(110, 15);
            labelSize.TabIndex = 9;
            labelSize.Text = "Schrift&größe (pt):";
            //
            // numSize
            //
            numSize.Location = new System.Drawing.Point(190, 270);
            numSize.Maximum = new decimal(new int[] { 72, 0, 0, 0 });
            numSize.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            numSize.Name = "numSize";
            numSize.Size = new System.Drawing.Size(80, 23);
            numSize.TabIndex = 10;
            numSize.Value = new decimal(new int[] { 12, 0, 0, 0 });
            numSize.ValueChanged += Preview_Changed;
            //
            // previewWebView
            //
            previewWebView.AllowExternalDrop = false;
            previewWebView.CreationProperties = null;
            previewWebView.DefaultBackgroundColor = System.Drawing.Color.White;
            previewWebView.Location = new System.Drawing.Point(390, 12);
            previewWebView.Name = "previewWebView";
            previewWebView.Size = new System.Drawing.Size(340, 420);
            previewWebView.TabIndex = 11;
            previewWebView.TabStop = false;
            previewWebView.ZoomFactor = 1D;
            //
            // picturePreview
            //
            picturePreview.BackColor = System.Drawing.SystemColors.ControlLight;
            picturePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picturePreview.Cursor = System.Windows.Forms.Cursors.Cross;
            picturePreview.Location = new System.Drawing.Point(390, 12);
            picturePreview.Name = "picturePreview";
            picturePreview.Size = new System.Drawing.Size(340, 420);
            picturePreview.TabIndex = 12;
            picturePreview.TabStop = false;
            picturePreview.MouseClick += PicturePreview_MouseClick;
            picturePreview.Paint += PicturePreview_Paint;
            //
            // labelPreviewState
            //
            labelPreviewState.ForeColor = System.Drawing.SystemColors.GrayText;
            labelPreviewState.Location = new System.Drawing.Point(390, 436);
            labelPreviewState.Name = "labelPreviewState";
            labelPreviewState.Size = new System.Drawing.Size(340, 15);
            labelPreviewState.TabIndex = 13;
            labelPreviewState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // buttonOK
            //
            buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            buttonOK.Location = new System.Drawing.Point(176, 424);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(95, 27);
            buttonOK.TabIndex = 14;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(277, 424);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(95, 27);
            buttonCancel.TabIndex = 15;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // AnnotationForm
            //
            AcceptButton = buttonOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(742, 463);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(labelPreviewState);
            Controls.Add(picturePreview);
            Controls.Add(previewWebView);
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
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AnnotationForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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
        private Microsoft.Web.WebView2.WinForms.WebView2 previewWebView;
        private System.Windows.Forms.PictureBox picturePreview;
        private System.Windows.Forms.Label labelPreviewState;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
