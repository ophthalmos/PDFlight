namespace PDFLight.Controls
{
    partial class AttachmentPanel
    {
        /// <summary>Erforderliche Designervariable.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Verwendete Ressourcen bereinigen.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) { components.Dispose(); }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            toolStripTop = new ToolStrip();
            labelCount = new ToolStripLabel();
            btnSaveAll = new ToolStripButton();
            btnSaveAs = new ToolStripButton();
            btnOpen = new ToolStripButton();
            listView = new PDFLight.Viewer.AttachmentListView();
            contextMenuEntry = new ContextMenuStrip(components);
            mnuOpen = new ToolStripMenuItem();
            mnuSaveAs = new ToolStripMenuItem();
            toolStripSeparatorC1 = new ToolStripSeparator();
            mnuCtxSaveAll = new ToolStripMenuItem();
            toolStripTop.SuspendLayout();
            contextMenuEntry.SuspendLayout();
            SuspendLayout();
            //
            // toolStripTop
            //
            toolStripTop.AutoSize = false;
            toolStripTop.GripStyle = ToolStripGripStyle.Hidden;
            toolStripTop.Items.AddRange(new ToolStripItem[] { labelCount, btnSaveAll, btnSaveAs, btnOpen });
            toolStripTop.Location = new Point(0, 0);
            toolStripTop.Name = "toolStripTop";
            toolStripTop.Padding = new Padding(8, 0, 4, 0);
            toolStripTop.Size = new Size(240, 36);
            toolStripTop.TabIndex = 0;
            //
            // labelCount
            //
            labelCount.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelCount.Name = "labelCount";
            labelCount.Size = new Size(54, 33);
            labelCount.Text = "Anhänge";
            //
            // btnSaveAll
            //
            btnSaveAll.Alignment = ToolStripItemAlignment.Right;
            btnSaveAll.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSaveAll.Enabled = false;
            btnSaveAll.Name = "btnSaveAll";
            btnSaveAll.Size = new Size(23, 33);
            btnSaveAll.Text = "Alle speichern…";
            btnSaveAll.ToolTipText = "Alle speichern…";
            btnSaveAll.Click += MnuSaveAll_Click;
            //
            // btnSaveAs
            //
            btnSaveAs.Alignment = ToolStripItemAlignment.Right;
            btnSaveAs.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSaveAs.Enabled = false;
            btnSaveAs.Name = "btnSaveAs";
            btnSaveAs.Size = new Size(23, 33);
            btnSaveAs.Text = "Speichern unter…";
            btnSaveAs.ToolTipText = "Speichern unter…";
            btnSaveAs.Click += MnuSaveAs_Click;
            //
            // btnOpen
            //
            btnOpen.Alignment = ToolStripItemAlignment.Right;
            btnOpen.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnOpen.Enabled = false;
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(23, 33);
            btnOpen.Text = "Öffnen";
            btnOpen.ToolTipText = "Öffnen";
            btnOpen.Click += MnuOpen_Click;
            //
            // listView
            //
            listView.AccessibleName = "Anhänge";
            listView.AccessibleRole = AccessibleRole.List;
            listView.AutoScroll = true;
            listView.ContextMenuStrip = contextMenuEntry;
            listView.Dock = DockStyle.Fill;
            listView.Font = new Font("Segoe UI", 9.75F);
            listView.Location = new Point(0, 36);
            listView.Name = "listView";
            listView.Size = new Size(240, 264);
            listView.TabIndex = 1;
            listView.OpenRequested += ListView_OpenRequested;
            listView.SelectionChanged += ListView_SelectionChanged;
            //
            // contextMenuEntry
            //
            contextMenuEntry.Items.AddRange(new ToolStripItem[] { mnuOpen, mnuSaveAs, toolStripSeparatorC1, mnuCtxSaveAll });
            contextMenuEntry.Name = "contextMenuEntry";
            contextMenuEntry.Size = new Size(181, 98);
            contextMenuEntry.Opening += ContextMenuEntry_Opening;
            //
            // mnuOpen
            //
            mnuOpen.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            mnuOpen.Name = "mnuOpen";
            mnuOpen.ShortcutKeyDisplayString = "Eingabe";
            mnuOpen.Size = new Size(180, 22);
            mnuOpen.Text = "Ö&ffnen";
            mnuOpen.Click += MnuOpen_Click;
            //
            // mnuSaveAs
            //
            mnuSaveAs.Name = "mnuSaveAs";
            mnuSaveAs.Size = new Size(180, 22);
            mnuSaveAs.Text = "Speichern &unter…";
            mnuSaveAs.Click += MnuSaveAs_Click;
            //
            // toolStripSeparatorC1
            //
            toolStripSeparatorC1.Name = "toolStripSeparatorC1";
            toolStripSeparatorC1.Size = new Size(177, 6);
            //
            // mnuCtxSaveAll
            //
            mnuCtxSaveAll.Name = "mnuCtxSaveAll";
            mnuCtxSaveAll.Size = new Size(180, 22);
            mnuCtxSaveAll.Text = "Alle speichern…";
            mnuCtxSaveAll.Click += MnuSaveAll_Click;
            //
            // AttachmentPanel
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(listView);
            Controls.Add(toolStripTop);
            Name = "AttachmentPanel";
            Size = new Size(240, 300);
            toolStripTop.ResumeLayout(false);
            toolStripTop.PerformLayout();
            contextMenuEntry.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ToolStrip toolStripTop;
        private ToolStripLabel labelCount;
        private ToolStripButton btnSaveAll;
        private ToolStripButton btnSaveAs;
        private ToolStripButton btnOpen;
        private PDFLight.Viewer.AttachmentListView listView;
        private ContextMenuStrip contextMenuEntry;
        private ToolStripMenuItem mnuOpen;
        private ToolStripMenuItem mnuSaveAs;
        private ToolStripSeparator toolStripSeparatorC1;
        private ToolStripMenuItem mnuCtxSaveAll;
    }
}
