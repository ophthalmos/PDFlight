namespace PDFLight.Forms
{
    partial class AnnotationListForm
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
            listView = new System.Windows.Forms.ListView();
            colPage = new System.Windows.Forms.ColumnHeader();
            colType = new System.Windows.Forms.ColumnHeader();
            colPosition = new System.Windows.Forms.ColumnHeader();
            colText = new System.Windows.Forms.ColumnHeader();
            btnEdit = new System.Windows.Forms.Button();
            btnDelete = new System.Windows.Forms.Button();
            buttonClose = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // labelFileValue
            //
            labelFileValue.AutoEllipsis = true;
            labelFileValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            labelFileValue.Location = new System.Drawing.Point(12, 12);
            labelFileValue.Name = "labelFileValue";
            labelFileValue.Size = new System.Drawing.Size(660, 15);
            labelFileValue.TabIndex = 0;
            labelFileValue.Text = "datei.pdf";
            //
            // listView
            //
            listView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colPage, colType, colPosition, colText });
            listView.FullRowSelect = true;
            listView.HideSelection = false;
            listView.Location = new System.Drawing.Point(12, 36);
            listView.MultiSelect = false;
            listView.Name = "listView";
            listView.Size = new System.Drawing.Size(660, 300);
            listView.TabIndex = 1;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = System.Windows.Forms.View.Details;
            listView.DoubleClick += ListView_DoubleClick;
            listView.SelectedIndexChanged += ListView_SelectedIndexChanged;
            //
            // colPage
            //
            colPage.Text = "Seite Nr.";
            colPage.Width = 70;
            //
            // colType
            //
            colType.Text = "Art";
            colType.Width = 120;
            //
            // colPosition
            //
            colPosition.Text = "Position";
            colPosition.Width = 180;
            //
            // colText
            //
            colText.Text = "Text";
            colText.Width = 280;
            //
            // btnEdit
            //
            btnEdit.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnEdit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btnEdit.Location = new System.Drawing.Point(12, 346);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new System.Drawing.Size(130, 27);
            btnEdit.TabIndex = 2;
            btnEdit.Text = "&Bearbeiten …";
            btnEdit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Click += BtnEdit_Click;
            //
            // btnDelete
            //
            btnDelete.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btnDelete.Location = new System.Drawing.Point(148, 346);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(130, 27);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "Löschen";
            btnDelete.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += BtnDelete_Click;
            //
            // buttonClose
            //
            buttonClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonClose.Location = new System.Drawing.Point(577, 346);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(95, 27);
            buttonClose.TabIndex = 4;
            buttonClose.Text = "Schließen";
            buttonClose.UseVisualStyleBackColor = true;
            //
            // AnnotationListForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new System.Drawing.Size(684, 385);
            Controls.Add(buttonClose);
            Controls.Add(btnDelete);
            Controls.Add(btnEdit);
            Controls.Add(listView);
            Controls.Add(labelFileValue);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(500, 300);
            Name = "AnnotationListForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Textanmerkungen";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label labelFileValue;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader colPage;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colPosition;
        private System.Windows.Forms.ColumnHeader colText;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button buttonClose;
    }
}
