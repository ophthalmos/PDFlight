namespace PDFLight.Controls
{
    partial class SummaryPanel
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
            ddPreset = new ToolStripDropDownButton();
            btnRun = new ToolStripButton();
            panelBody = new Panel();
            richText = new RichTextBox();
            toolStripActions = new ToolStrip();
            btnCopy = new ToolStripButton();
            btnSave = new ToolStripButton();
            btnClear = new ToolStripButton();
            panelAsk = new Panel();
            textQuestion = new TextBox();
            btnAsk = new Button();
            linkDisclaimer = new LinkLabel();
            toolTip = new ToolTip(components);
            toolStripTop.SuspendLayout();
            panelBody.SuspendLayout();
            toolStripActions.SuspendLayout();
            panelAsk.SuspendLayout();
            SuspendLayout();
            //
            // toolStripTop
            //
            toolStripTop.AutoSize = false;
            toolStripTop.GripStyle = ToolStripGripStyle.Hidden;
            toolStripTop.Items.AddRange(new ToolStripItem[] { ddPreset, btnRun });
            toolStripTop.Location = new Point(0, 0);
            toolStripTop.Name = "toolStripTop";
            toolStripTop.Padding = new Padding(8, 0, 4, 0);
            toolStripTop.Size = new Size(240, 36);
            toolStripTop.TabIndex = 0;
            //
            // ddPreset
            //
            ddPreset.DisplayStyle = ToolStripItemDisplayStyle.Text;
            ddPreset.Name = "ddPreset";
            ddPreset.Size = new Size(110, 33);
            ddPreset.Text = "Zusammenfassung";
            ddPreset.ToolTipText = "Art der Zusammenfassung";
            //
            // btnRun
            //
            btnRun.Alignment = ToolStripItemAlignment.Right;
            btnRun.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(23, 33);
            btnRun.Text = "Erstellen";
            btnRun.ToolTipText = "Zusammenfassung erstellen";
            btnRun.Click += BtnRun_Click;
            //
            // panelBody
            //
            panelBody.Controls.Add(richText);
            panelBody.Dock = DockStyle.Fill;
            panelBody.Location = new Point(0, 66);
            panelBody.Name = "panelBody";
            panelBody.Padding = new Padding(12, 6, 4, 4);
            panelBody.Size = new Size(240, 156);
            panelBody.TabIndex = 2;
            //
            // richText
            //
            richText.AccessibleName = "Zusammenfassung";
            richText.BorderStyle = BorderStyle.None;
            richText.DetectUrls = false;
            richText.Dock = DockStyle.Fill;
            richText.Font = new Font("Segoe UI", 9.75F);
            richText.Location = new Point(12, 6);
            richText.Name = "richText";
            richText.ReadOnly = true;
            richText.ScrollBars = RichTextBoxScrollBars.Vertical;
            richText.Size = new Size(224, 146);
            richText.TabIndex = 0;
            richText.Text = "";
            richText.MouseMove += RichText_MouseMove;
            richText.MouseUp += RichText_MouseUp;
            //
            // toolStripActions
            //
            toolStripActions.AutoSize = false;
            toolStripActions.Dock = DockStyle.Top;
            toolStripActions.GripStyle = ToolStripGripStyle.Hidden;
            toolStripActions.Items.AddRange(new ToolStripItem[] { btnCopy, btnSave, btnClear });
            toolStripActions.Location = new Point(0, 36);
            toolStripActions.Name = "toolStripActions";
            toolStripActions.Padding = new Padding(8, 0, 4, 0);
            toolStripActions.Size = new Size(240, 30);
            toolStripActions.TabIndex = 1;
            //
            // btnCopy
            //
            btnCopy.Enabled = false;
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new Size(57, 27);
            btnCopy.Text = "Kopieren";
            btnCopy.ToolTipText = "Zusammenfassung kopieren";
            btnCopy.Click += BtnCopy_Click;
            //
            // btnSave
            //
            btnSave.Enabled = false;
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(67, 27);
            btnSave.Text = "Speichern…";
            btnSave.ToolTipText = "Als Word-Datei (RTF) speichern";
            btnSave.Click += BtnSave_Click;
            //
            // btnClear
            //
            btnClear.Alignment = ToolStripItemAlignment.Right;
            btnClear.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnClear.Enabled = false;
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(23, 27);
            btnClear.Text = "Neu beginnen";
            btnClear.ToolTipText = "Neu beginnen";
            btnClear.Click += BtnClear_Click;
            //
            // panelAsk
            //
            panelAsk.Controls.Add(textQuestion);
            panelAsk.Controls.Add(btnAsk);
            panelAsk.Dock = DockStyle.Bottom;
            panelAsk.Location = new Point(0, 222);
            panelAsk.Name = "panelAsk";
            panelAsk.Padding = new Padding(10, 4, 6, 4);
            panelAsk.Size = new Size(240, 34);
            panelAsk.TabIndex = 3;
            //
            // textQuestion
            //
            textQuestion.Dock = DockStyle.Fill;
            textQuestion.Location = new Point(10, 4);
            textQuestion.Name = "textQuestion";
            textQuestion.PlaceholderText = "Frage zum Dokument…";
            textQuestion.Size = new Size(194, 23);
            textQuestion.TabIndex = 0;
            textQuestion.TextChanged += TextQuestion_TextChanged;
            textQuestion.KeyDown += TextQuestion_KeyDown;
            //
            // btnAsk
            //
            btnAsk.Dock = DockStyle.Right;
            btnAsk.Enabled = false;
            btnAsk.FlatAppearance.BorderSize = 0;
            btnAsk.FlatStyle = FlatStyle.Flat;
            btnAsk.Location = new Point(204, 4);
            btnAsk.Name = "btnAsk";
            btnAsk.Size = new Size(30, 26);
            btnAsk.TabIndex = 1;
            btnAsk.Text = "➤";
            toolTip.SetToolTip(btnAsk, "Frage senden (Eingabe)");
            btnAsk.UseVisualStyleBackColor = true;
            btnAsk.Click += BtnAsk_Click;
            //
            // linkDisclaimer
            //
            linkDisclaimer.Dock = DockStyle.Bottom;
            linkDisclaimer.Location = new Point(0, 256);
            linkDisclaimer.Name = "linkDisclaimer";
            linkDisclaimer.Padding = new Padding(10, 4, 4, 6);
            linkDisclaimer.Size = new Size(240, 44);
            linkDisclaimer.TabIndex = 4;
            linkDisclaimer.TabStop = true;
            linkDisclaimer.Text = "KI-Antworten können unrichtig sein.";
            linkDisclaimer.LinkClicked += LinkDisclaimer_LinkClicked;
            //
            // SummaryPanel
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelBody);
            Controls.Add(panelAsk);
            Controls.Add(linkDisclaimer);
            Controls.Add(toolStripActions);
            Controls.Add(toolStripTop);
            Name = "SummaryPanel";
            Size = new Size(240, 300);
            toolStripTop.ResumeLayout(false);
            toolStripTop.PerformLayout();
            panelBody.ResumeLayout(false);
            toolStripActions.ResumeLayout(false);
            toolStripActions.PerformLayout();
            panelAsk.ResumeLayout(false);
            panelAsk.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private ToolStrip toolStripTop;
        private ToolStripDropDownButton ddPreset;
        private ToolStripButton btnRun;
        private Panel panelBody;
        private RichTextBox richText;
        private ToolStrip toolStripActions;
        private ToolStripButton btnCopy;
        private ToolStripButton btnSave;
        private ToolStripButton btnClear;
        private Panel panelAsk;
        private TextBox textQuestion;
        private Button btnAsk;
        private LinkLabel linkDisclaimer;
        private ToolTip toolTip;
    }
}
