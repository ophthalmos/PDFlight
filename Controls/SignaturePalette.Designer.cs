namespace PDFLight.Controls
{
    partial class SignaturePalette
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
            panelRows = new Panel();
            toolTip = new ToolTip(components);
            SuspendLayout();
            //
            // panelRows
            //
            panelRows.Dock = DockStyle.Fill;
            panelRows.Location = new Point(0, 4);
            panelRows.Name = "panelRows";
            panelRows.Size = new Size(260, 112);
            panelRows.TabIndex = 0;
            panelRows.Paint += PanelRows_Paint;
            panelRows.MouseClick += PanelRows_MouseClick;
            panelRows.MouseLeave += PanelRows_MouseLeave;
            panelRows.MouseMove += PanelRows_MouseMove;
            //
            // SignaturePalette
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelRows);
            Name = "SignaturePalette";
            Padding = new Padding(0, 4, 0, 4);
            Size = new Size(260, 120);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelRows;
        private ToolTip toolTip;
    }
}
