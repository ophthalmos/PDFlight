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
            components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(AnnotationListForm));
            contextMenuList = new ContextMenuStrip(components);
            editMenuItem = new ToolStripMenuItem();
            deleteMenuItem = new ToolStripMenuItem();
            labelFileValue = new Label();
            listView = new ListView();
            colPage = new ColumnHeader();
            colPosition = new ColumnHeader();
            colText = new ColumnHeader();
            btnEdit = new Button();
            btnDelete = new Button();
            buttonClose = new Button();
            contextMenuList.SuspendLayout();
            SuspendLayout();
            // 
            // contextMenuList
            // 
            contextMenuList.Items.AddRange(new ToolStripItem[] { editMenuItem, deleteMenuItem });
            contextMenuList.Name = "contextMenuList";
            contextMenuList.Size = new Size(181, 70);
            contextMenuList.Opening += ContextMenuList_Opening;
            // 
            // editMenuItem
            // 
            editMenuItem.Name = "editMenuItem";
            editMenuItem.ShortcutKeyDisplayString = "Enter";
            editMenuItem.Size = new Size(180, 22);
            editMenuItem.Text = "&Bearbeiten …";
            editMenuItem.Click += BtnEdit_Click;
            // 
            // deleteMenuItem
            // 
            deleteMenuItem.Name = "deleteMenuItem";
            deleteMenuItem.ShortcutKeyDisplayString = "Entf";
            deleteMenuItem.Size = new Size(180, 22);
            deleteMenuItem.Text = "Löschen";
            deleteMenuItem.Click += BtnDelete_Click;
            // 
            // labelFileValue
            // 
            labelFileValue.AutoEllipsis = true;
            labelFileValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelFileValue.Location = new Point(12, 12);
            labelFileValue.Name = "labelFileValue";
            labelFileValue.Size = new Size(660, 15);
            labelFileValue.TabIndex = 0;
            labelFileValue.Text = "datei.pdf";
            // 
            // listView
            // 
            listView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listView.Columns.AddRange(new ColumnHeader[] { colPage, colPosition, colText });
            listView.ContextMenuStrip = contextMenuList;
            listView.FullRowSelect = true;
            listView.Location = new Point(12, 36);
            listView.MultiSelect = false;
            listView.Name = "listView";
            listView.Size = new Size(660, 300);
            listView.TabIndex = 1;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = View.Details;
            listView.SelectedIndexChanged += ListView_SelectedIndexChanged;
            listView.DoubleClick += ListView_DoubleClick;
            listView.KeyDown += ListView_KeyDown;
            // 
            // colPage
            // 
            colPage.Text = "Seitennr.";
            colPage.Width = 70;
            // 
            // colPosition
            // 
            colPosition.Text = "Position";
            colPosition.Width = 180;
            // 
            // colText
            // 
            colText.Text = "Text";
            colText.Width = 400;
            // 
            // btnEdit
            // 
            btnEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEdit.ImageAlign = ContentAlignment.MiddleLeft;
            btnEdit.Location = new Point(12, 346);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(130, 27);
            btnEdit.TabIndex = 2;
            btnEdit.Text = "&Bearbeiten …";
            btnEdit.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Click += BtnEdit_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.ImageAlign = ContentAlignment.MiddleLeft;
            btnDelete.Location = new Point(148, 346);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(130, 27);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "Löschen";
            btnDelete.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += BtnDelete_Click;
            // 
            // buttonClose
            // 
            buttonClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonClose.DialogResult = DialogResult.Cancel;
            buttonClose.Location = new Point(577, 346);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(95, 27);
            buttonClose.TabIndex = 4;
            buttonClose.Text = "Schließen";
            buttonClose.UseVisualStyleBackColor = true;
            // 
            // AnnotationListForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new Size(684, 385);
            Controls.Add(buttonClose);
            Controls.Add(btnDelete);
            Controls.Add(btnEdit);
            Controls.Add(listView);
            Controls.Add(labelFileValue);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(500, 296);
            Name = "AnnotationListForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Textanmerkungen";
            contextMenuList.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuList;
        private System.Windows.Forms.ToolStripMenuItem editMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteMenuItem;
        private System.Windows.Forms.Label labelFileValue;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader colPage;
        private System.Windows.Forms.ColumnHeader colPosition;
        private System.Windows.Forms.ColumnHeader colText;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button buttonClose;
    }
}
