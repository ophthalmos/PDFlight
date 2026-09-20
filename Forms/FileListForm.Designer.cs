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
            labelPrompt = new Label();
            listView = new ListView();
            colName = new ColumnHeader();
            colDate = new ColumnHeader();
            btnUp = new Button();
            btnDown = new Button();
            btnOK = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // labelPrompt
            // 
            labelPrompt.AutoSize = true;
            labelPrompt.Location = new Point(12, 12);
            labelPrompt.Name = "labelPrompt";
            labelPrompt.Size = new Size(297, 15);
            labelPrompt.TabIndex = 0;
            labelPrompt.Text = "Wähle Dateien (Strg+Leertaste), ändere die Reihenfolge";
            // 
            // listView
            // 
            listView.AllowDrop = true;
            listView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listView.Columns.AddRange(new ColumnHeader[] { colName, colDate });
            listView.FullRowSelect = true;
            listView.Location = new Point(12, 34);
            listView.Name = "listView";
            listView.ShowItemToolTips = true;
            listView.Size = new Size(460, 300);
            listView.TabIndex = 1;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = View.Details;
            listView.ItemDrag += ListView_ItemDrag;
            listView.SelectedIndexChanged += ListView_SelectedIndexChanged;
            listView.DragDrop += ListView_DragDrop;
            listView.DragEnter += ListView_DragEnter;
            listView.DragOver += ListView_DragOver;
            listView.DragLeave += ListView_DragLeave;
            listView.DoubleClick += ListView_DoubleClick;
            listView.KeyDown += ListView_KeyDown;
            listView.Resize += ListView_Resize;
            // 
            // colName
            // 
            colName.Text = "Name";
            colName.Width = 250;
            // 
            // colDate
            // 
            colDate.Text = "Datum";
            colDate.Width = 130;
            // 
            // btnUp
            // 
            btnUp.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUp.Location = new Point(12, 346);
            btnUp.Name = "btnUp";
            btnUp.Size = new Size(44, 27);
            btnUp.TabIndex = 2;
            btnUp.Text = "↑";
            btnUp.UseVisualStyleBackColor = true;
            btnUp.Click += BtnUp_Click;
            // 
            // btnDown
            // 
            btnDown.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDown.Location = new Point(62, 346);
            btnDown.Name = "btnDown";
            btnDown.Size = new Size(44, 27);
            btnDown.TabIndex = 3;
            btnDown.Text = "↓";
            btnDown.UseVisualStyleBackColor = true;
            btnDown.Click += BtnDown_Click;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.Location = new Point(226, 346);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(145, 27);
            btnOK.TabIndex = 4;
            btnOK.Text = "Dateien &hinzufügen";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(377, 346);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(95, 27);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // FileListForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(484, 385);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(btnDown);
            Controls.Add(btnUp);
            Controls.Add(listView);
            Controls.Add(labelPrompt);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimizeBox = false;
            MinimumSize = new Size(360, 260);
            Name = "FileListForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Geöffnete PDF-Dateien";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelPrompt;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colDate;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
