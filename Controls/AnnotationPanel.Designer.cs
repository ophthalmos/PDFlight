namespace PDFLight.Controls
{
    partial class AnnotationPanel
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
            btnMore = new ToolStripDropDownButton();
            mnuExpandAll = new ToolStripMenuItem();
            mnuCollapseAll = new ToolStripMenuItem();
            toolStripSeparatorM1 = new ToolStripSeparator();
            mnuDeleteAll = new ToolStripMenuItem();
            btnFilter = new ToolStripDropDownButton();
            mnuFilterType = new ToolStripMenuItem();
            mnuFilterAuthor = new ToolStripMenuItem();
            mnuFilterColor = new ToolStripMenuItem();
            toolStripSeparatorF1 = new ToolStripSeparator();
            mnuFilterReset = new ToolStripMenuItem();
            btnSort = new ToolStripDropDownButton();
            mnuSortPage = new ToolStripMenuItem();
            mnuSortDate = new ToolStripMenuItem();
            mnuSortAuthor = new ToolStripMenuItem();
            mnuSortType = new ToolStripMenuItem();
            btnSearch = new ToolStripButton();
            panelSearch = new Panel();
            textSearch = new TextBox();
            listView = new PDFLight.Viewer.AnnotationListView();
            contextMenuEntry = new ContextMenuStrip(components);
            mnuEdit = new ToolStripMenuItem();
            mnuDelete = new ToolStripMenuItem();
            toolStripSeparatorC1 = new ToolStripSeparator();
            mnuCtxExpandAll = new ToolStripMenuItem();
            mnuCtxCollapseAll = new ToolStripMenuItem();
            toolStripTop.SuspendLayout();
            panelSearch.SuspendLayout();
            contextMenuEntry.SuspendLayout();
            SuspendLayout();
            //
            // toolStripTop
            //
            toolStripTop.AutoSize = false;
            toolStripTop.GripStyle = ToolStripGripStyle.Hidden;
            toolStripTop.Items.AddRange(new ToolStripItem[] { labelCount, btnMore, btnFilter, btnSort, btnSearch });
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
            labelCount.Size = new Size(88, 33);
            labelCount.Text = "Anmerkungen";
            //
            // btnMore
            //
            btnMore.Alignment = ToolStripItemAlignment.Right;
            btnMore.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnMore.DropDownItems.AddRange(new ToolStripItem[] { mnuExpandAll, mnuCollapseAll, toolStripSeparatorM1, mnuDeleteAll });
            btnMore.Name = "btnMore";
            btnMore.ShowDropDownArrow = false;
            btnMore.Size = new Size(20, 33);
            btnMore.Text = "…";
            btnMore.ToolTipText = "Weitere Befehle";
            btnMore.DropDownOpening += BtnMore_DropDownOpening;
            //
            // mnuExpandAll
            //
            mnuExpandAll.Name = "mnuExpandAll";
            mnuExpandAll.Size = new Size(180, 22);
            mnuExpandAll.Text = "Alles aufklappen";
            mnuExpandAll.Click += MnuExpandAll_Click;
            //
            // mnuCollapseAll
            //
            mnuCollapseAll.Name = "mnuCollapseAll";
            mnuCollapseAll.Size = new Size(180, 22);
            mnuCollapseAll.Text = "Alles zuklappen";
            mnuCollapseAll.Click += MnuCollapseAll_Click;
            //
            // toolStripSeparatorM1
            //
            toolStripSeparatorM1.Name = "toolStripSeparatorM1";
            toolStripSeparatorM1.Size = new Size(177, 6);
            //
            // mnuDeleteAll
            //
            mnuDeleteAll.Name = "mnuDeleteAll";
            mnuDeleteAll.Size = new Size(180, 22);
            mnuDeleteAll.Text = "Alle löschen…";
            mnuDeleteAll.Click += MnuDeleteAll_Click;
            //
            // btnFilter
            //
            btnFilter.Alignment = ToolStripItemAlignment.Right;
            btnFilter.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnFilter.DropDownItems.AddRange(new ToolStripItem[] { mnuFilterType, mnuFilterAuthor, mnuFilterColor, toolStripSeparatorF1, mnuFilterReset });
            btnFilter.Name = "btnFilter";
            btnFilter.ShowDropDownArrow = false;
            btnFilter.Size = new Size(36, 33);
            btnFilter.Text = "Filter";
            btnFilter.ToolTipText = "Filtern";
            btnFilter.DropDownOpening += BtnFilter_DropDownOpening;
            //
            // mnuFilterType
            //
            mnuFilterType.Name = "mnuFilterType";
            mnuFilterType.Size = new Size(180, 22);
            mnuFilterType.Text = "Art";
            //
            // mnuFilterAuthor
            //
            mnuFilterAuthor.Name = "mnuFilterAuthor";
            mnuFilterAuthor.Size = new Size(180, 22);
            mnuFilterAuthor.Text = "Autor";
            //
            // mnuFilterColor
            //
            mnuFilterColor.Name = "mnuFilterColor";
            mnuFilterColor.Size = new Size(180, 22);
            mnuFilterColor.Text = "Farbe";
            //
            // toolStripSeparatorF1
            //
            toolStripSeparatorF1.Name = "toolStripSeparatorF1";
            toolStripSeparatorF1.Size = new Size(177, 6);
            //
            // mnuFilterReset
            //
            mnuFilterReset.Name = "mnuFilterReset";
            mnuFilterReset.Size = new Size(180, 22);
            mnuFilterReset.Text = "Alle anzeigen";
            mnuFilterReset.Click += MnuFilterReset_Click;
            //
            // btnSort
            //
            btnSort.Alignment = ToolStripItemAlignment.Right;
            btnSort.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSort.DropDownItems.AddRange(new ToolStripItem[] { mnuSortPage, mnuSortDate, mnuSortAuthor, mnuSortType });
            btnSort.Name = "btnSort";
            btnSort.ShowDropDownArrow = false;
            btnSort.Size = new Size(23, 33);
            btnSort.Text = "A–Z";
            btnSort.ToolTipText = "Sortieren";
            //
            // mnuSortPage
            //
            mnuSortPage.Checked = true;
            mnuSortPage.CheckState = CheckState.Checked;
            mnuSortPage.Name = "mnuSortPage";
            mnuSortPage.Size = new Size(180, 22);
            mnuSortPage.Text = "Nach Seite";
            mnuSortPage.Click += MnuSort_Click;
            //
            // mnuSortDate
            //
            mnuSortDate.Name = "mnuSortDate";
            mnuSortDate.Size = new Size(180, 22);
            mnuSortDate.Text = "Nach Datum";
            mnuSortDate.Click += MnuSort_Click;
            //
            // mnuSortAuthor
            //
            mnuSortAuthor.Name = "mnuSortAuthor";
            mnuSortAuthor.Size = new Size(180, 22);
            mnuSortAuthor.Text = "Nach Autor";
            mnuSortAuthor.Click += MnuSort_Click;
            //
            // mnuSortType
            //
            mnuSortType.Name = "mnuSortType";
            mnuSortType.Size = new Size(180, 22);
            mnuSortType.Text = "Nach Art";
            mnuSortType.Click += MnuSort_Click;
            //
            // btnSearch
            //
            btnSearch.Alignment = ToolStripItemAlignment.Right;
            btnSearch.CheckOnClick = true;
            btnSearch.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(23, 33);
            btnSearch.Text = "⌕";
            btnSearch.ToolTipText = "Anmerkungen durchsuchen";
            btnSearch.CheckedChanged += BtnSearch_CheckedChanged;
            //
            // panelSearch
            //
            panelSearch.Controls.Add(textSearch);
            panelSearch.Dock = DockStyle.Top;
            panelSearch.Location = new Point(0, 36);
            panelSearch.Name = "panelSearch";
            panelSearch.Padding = new Padding(8, 4, 8, 6);
            panelSearch.Size = new Size(240, 33);
            panelSearch.TabIndex = 1;
            panelSearch.Visible = false;
            //
            // textSearch
            //
            textSearch.Dock = DockStyle.Fill;
            textSearch.Location = new Point(8, 4);
            textSearch.Name = "textSearch";
            textSearch.PlaceholderText = "Text oder Autor";
            textSearch.Size = new Size(224, 23);
            textSearch.TabIndex = 0;
            textSearch.TextChanged += TextSearch_TextChanged;
            textSearch.KeyDown += TextSearch_KeyDown;
            //
            // listView
            //
            listView.AccessibleName = "Anmerkungen";
            listView.AccessibleRole = AccessibleRole.List;
            listView.AutoScroll = true;
            listView.ContextMenuStrip = contextMenuEntry;
            listView.Dock = DockStyle.Fill;
            listView.Font = new Font("Segoe UI", 9.75F);
            listView.Location = new Point(0, 69);
            listView.Name = "listView";
            listView.Size = new Size(240, 231);
            listView.TabIndex = 2;
            listView.EntryActivated += ListView_EntryActivated;
            listView.EditRequested += ListView_EditRequested;
            listView.DeleteRequested += ListView_DeleteRequested;
            //
            // contextMenuEntry
            //
            contextMenuEntry.Items.AddRange(new ToolStripItem[] { mnuEdit, mnuDelete, toolStripSeparatorC1, mnuCtxExpandAll, mnuCtxCollapseAll });
            contextMenuEntry.Name = "contextMenuEntry";
            contextMenuEntry.Size = new Size(181, 98);
            contextMenuEntry.Opening += ContextMenuEntry_Opening;
            //
            // mnuEdit
            //
            mnuEdit.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            mnuEdit.Name = "mnuEdit";
            mnuEdit.ShortcutKeyDisplayString = "Eingabe";
            mnuEdit.Size = new Size(180, 22);
            mnuEdit.Text = "Bearbeiten…";
            mnuEdit.Click += MnuEdit_Click;
            //
            // mnuDelete
            //
            mnuDelete.Name = "mnuDelete";
            mnuDelete.ShortcutKeyDisplayString = "Entf";
            mnuDelete.Size = new Size(180, 22);
            mnuDelete.Text = "Löschen";
            mnuDelete.Click += MnuDelete_Click;
            //
            // toolStripSeparatorC1
            //
            toolStripSeparatorC1.Name = "toolStripSeparatorC1";
            toolStripSeparatorC1.Size = new Size(177, 6);
            //
            // mnuCtxExpandAll
            //
            mnuCtxExpandAll.Name = "mnuCtxExpandAll";
            mnuCtxExpandAll.Size = new Size(180, 22);
            mnuCtxExpandAll.Text = "Alles aufklappen";
            mnuCtxExpandAll.Click += MnuExpandAll_Click;
            //
            // mnuCtxCollapseAll
            //
            mnuCtxCollapseAll.Name = "mnuCtxCollapseAll";
            mnuCtxCollapseAll.Size = new Size(180, 22);
            mnuCtxCollapseAll.Text = "Alles zuklappen";
            mnuCtxCollapseAll.Click += MnuCollapseAll_Click;
            //
            // AnnotationPanel
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(listView);
            Controls.Add(panelSearch);
            Controls.Add(toolStripTop);
            Name = "AnnotationPanel";
            Size = new Size(240, 300);
            toolStripTop.ResumeLayout(false);
            toolStripTop.PerformLayout();
            panelSearch.ResumeLayout(false);
            panelSearch.PerformLayout();
            contextMenuEntry.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ToolStrip toolStripTop;
        private ToolStripLabel labelCount;
        private ToolStripDropDownButton btnMore;
        private ToolStripMenuItem mnuExpandAll;
        private ToolStripMenuItem mnuCollapseAll;
        private ToolStripSeparator toolStripSeparatorM1;
        private ToolStripMenuItem mnuDeleteAll;
        private ToolStripDropDownButton btnFilter;
        private ToolStripMenuItem mnuFilterType;
        private ToolStripMenuItem mnuFilterAuthor;
        private ToolStripMenuItem mnuFilterColor;
        private ToolStripSeparator toolStripSeparatorF1;
        private ToolStripMenuItem mnuFilterReset;
        private ToolStripDropDownButton btnSort;
        private ToolStripMenuItem mnuSortPage;
        private ToolStripMenuItem mnuSortDate;
        private ToolStripMenuItem mnuSortAuthor;
        private ToolStripMenuItem mnuSortType;
        private ToolStripButton btnSearch;
        private Panel panelSearch;
        private TextBox textSearch;
        private PDFLight.Viewer.AnnotationListView listView;
        private ContextMenuStrip contextMenuEntry;
        private ToolStripMenuItem mnuEdit;
        private ToolStripMenuItem mnuDelete;
        private ToolStripSeparator toolStripSeparatorC1;
        private ToolStripMenuItem mnuCtxExpandAll;
        private ToolStripMenuItem mnuCtxCollapseAll;
    }
}
