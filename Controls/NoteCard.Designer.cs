namespace PDFLight.Controls
{
    partial class NoteCard
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
            textNote = new TextBox();
            labelMore = new Label();
            labelClose = new Label();
            buttonCommit = new RoundButton();
            contextMenuNote = new ContextMenuStrip(components);
            mnuDelete = new ToolStripMenuItem();
            mnuEdit = new ToolStripMenuItem();
            toolTip = new ToolTip(components);
            contextMenuNote.SuspendLayout();
            SuspendLayout();
            // 
            // textNote
            // 
            textNote.BackColor = Color.FromArgb(255, 249, 199);
            textNote.BorderStyle = BorderStyle.None;
            textNote.Font = new Font("Segoe UI", 9F);
            textNote.ForeColor = Color.FromArgb(40, 40, 40);
            textNote.Location = new Point(22, 36);
            textNote.MaxLength = 10000;
            textNote.Multiline = true;
            textNote.Name = "textNote";
            textNote.ReadOnly = true;
            textNote.Size = new Size(190, 84);
            textNote.TabIndex = 0;
            textNote.KeyDown += TextNote_KeyDown;
            textNote.Leave += TextNote_Leave;
            // 
            // labelMore
            // 
            labelMore.BackColor = Color.FromArgb(255, 249, 199);
            labelMore.Cursor = Cursors.Hand;
            labelMore.Font = new Font("Segoe MDL2 Assets", 7.5F, FontStyle.Bold);
            labelMore.ForeColor = Color.FromArgb(40, 40, 40);
            labelMore.Location = new Point(166, 12);
            labelMore.Name = "labelMore";
            labelMore.Size = new Size(22, 20);
            labelMore.TabIndex = 1;
            labelMore.Text = "";
            labelMore.TextAlign = ContentAlignment.MiddleCenter;
            toolTip.SetToolTip(labelMore, "Weitere Optionen");
            labelMore.Click += LabelMore_Click;
            // 
            // labelClose
            // 
            labelClose.BackColor = Color.FromArgb(255, 249, 199);
            labelClose.Cursor = Cursors.Hand;
            labelClose.Font = new Font("Segoe MDL2 Assets", 7.5F, FontStyle.Bold);
            labelClose.ForeColor = Color.FromArgb(40, 40, 40);
            labelClose.Location = new Point(190, 12);
            labelClose.Name = "labelClose";
            labelClose.Size = new Size(22, 20);
            labelClose.TabIndex = 2;
            labelClose.Text = "";
            labelClose.TextAlign = ContentAlignment.MiddleCenter;
            toolTip.SetToolTip(labelClose, "Schließen");
            labelClose.Click += LabelClose_Click;
            // 
            // buttonCommit
            // 
            buttonCommit.BackColor = Color.FromArgb(255, 249, 199);
            buttonCommit.Cursor = Cursors.Hand;
            buttonCommit.Font = new Font("Segoe MDL2 Assets", 9F);
            buttonCommit.ForeColor = Color.FromArgb(0, 120, 212);
            buttonCommit.GlyphColor = Color.FromArgb(0, 120, 212);
            buttonCommit.HoverBackColor = Color.FromArgb(0, 120, 212);
            buttonCommit.HoverGlyphColor = Color.White;
            buttonCommit.Location = new Point(188, 96);
            buttonCommit.Name = "buttonCommit";
            buttonCommit.Size = new Size(24, 24);
            buttonCommit.SurroundColor = Color.FromArgb(255, 249, 199);
            buttonCommit.TabIndex = 3;
            buttonCommit.Text = "";
            toolTip.SetToolTip(buttonCommit, "Übernehmen (Strg+Eingabe)");
            buttonCommit.UseVisualStyleBackColor = false;
            buttonCommit.Visible = false;
            buttonCommit.Click += ButtonCommit_Click;
            // 
            // contextMenuNote
            // 
            contextMenuNote.Items.AddRange(new ToolStripItem[] { mnuDelete, mnuEdit });
            contextMenuNote.Name = "contextMenuNote";
            contextMenuNote.Size = new Size(131, 48);
            // 
            // mnuDelete
            // 
            mnuDelete.Name = "mnuDelete";
            mnuDelete.Size = new Size(130, 22);
            mnuDelete.Text = "Löschen";
            mnuDelete.Click += MnuDelete_Click;
            // 
            // mnuEdit
            // 
            mnuEdit.Name = "mnuEdit";
            mnuEdit.Size = new Size(130, 22);
            mnuEdit.Text = "Bearbeiten";
            mnuEdit.Click += MnuEdit_Click;
            // 
            // NoteCard
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(buttonCommit);
            Controls.Add(labelClose);
            Controls.Add(labelMore);
            Controls.Add(textNote);
            Name = "NoteCard";
            Size = new Size(231, 136);
            contextMenuNote.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textNote;
        private Label labelMore;
        private Label labelClose;
        private RoundButton buttonCommit;
        private ContextMenuStrip contextMenuNote;
        private ToolStripMenuItem mnuDelete;
        private ToolStripMenuItem mnuEdit;
        private ToolTip toolTip;
    }
}
