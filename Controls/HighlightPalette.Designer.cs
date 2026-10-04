namespace PDFLight.Controls
{
    partial class HighlightPalette
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
            labelColors = new Label();
            panelColors = new Panel();
            toolTip = new ToolTip(components);
            SuspendLayout();
            //
            // labelColors
            //
            labelColors.AutoSize = true;
            labelColors.Font = new Font("Segoe UI Semibold", 10F);
            labelColors.Location = new Point(14, 12);
            labelColors.Name = "labelColors";
            labelColors.Size = new Size(128, 19);
            labelColors.TabIndex = 0;
            labelColors.Text = "Hervorhebungsfarbe";
            //
            // panelColors
            //
            panelColors.Location = new Point(12, 40);
            panelColors.Name = "panelColors";
            panelColors.Size = new Size(231, 99);
            panelColors.TabIndex = 1;
            panelColors.Paint += PanelColors_Paint;
            panelColors.MouseClick += PanelColors_MouseClick;
            panelColors.MouseLeave += PanelColors_MouseLeave;
            panelColors.MouseMove += PanelColors_MouseMove;
            //
            // HighlightPalette
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelColors);
            Controls.Add(labelColors);
            Name = "HighlightPalette";
            Padding = new Padding(0, 0, 10, 10);
            Size = new Size(255, 151);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelColors;
        private Panel panelColors;
        private ToolTip toolTip;
    }
}
