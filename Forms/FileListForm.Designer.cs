namespace PDFLight.Forms
{
    partial class FileListForm
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(FileListForm));
            labelPrompt = new System.Windows.Forms.Label();
            listView = new System.Windows.Forms.ListView();
            colName = new System.Windows.Forms.ColumnHeader();
            colDate = new System.Windows.Forms.ColumnHeader();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // labelPrompt
            //
            labelPrompt.AutoSize = true;
            labelPrompt.Location = new System.Drawing.Point(12, 12);
            labelPrompt.Name = "labelPrompt";
            labelPrompt.Size = new System.Drawing.Size(126, 15);
            labelPrompt.TabIndex = 0;
            labelPrompt.Text = "Geöffnete PDF-Dateien:";
            //
            // listView
            //
            listView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colName, colDate });
            listView.FullRowSelect = true;
            listView.HideSelection = false;
            listView.Location = new System.Drawing.Point(12, 34);
            listView.Name = "listView";
            listView.ShowItemToolTips = true;
            listView.Size = new System.Drawing.Size(460, 300);
            listView.TabIndex = 1;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = System.Windows.Forms.View.Details;
            listView.SelectedIndexChanged += ListView_SelectedIndexChanged;
            listView.Resize += ListView_Resize;
            listView.DoubleClick += ListView_DoubleClick;
            listView.KeyDown += ListView_KeyDown;
            //
            // colName
            //
            colName.Text = "Name";
            colName.Width = 300;
            //
            // colDate
            //
            colDate.Text = "Datum";
            colDate.Width = 130;
            //
            // btnOK
            //
            btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnOK.Location = new System.Drawing.Point(226, 346);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(145, 27);
            btnOK.TabIndex = 2;
            btnOK.Text = "Dateien &hinzufügen";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            //
            // btnCancel
            //
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(377, 346);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(95, 27);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // FileListForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(484, 385);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(listView);
            Controls.Add(labelPrompt);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(360, 260);
            Name = "FileListForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Geöffnete PDF-Dateien";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelPrompt;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colDate;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
