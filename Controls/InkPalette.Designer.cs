namespace PDFLight.Controls
{
    partial class InkPalette
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
            labelColors = new Label();
            panelColors = new Panel();
            panelPreview = new Panel();
            labelWidth = new Label();
            trackWidth = new TrackBar();
            labelThin = new Label();
            labelWide = new Label();
            ((System.ComponentModel.ISupportInitialize)trackWidth).BeginInit();
            SuspendLayout();
            // 
            // labelColors
            // 
            labelColors.AutoSize = true;
            labelColors.Font = new Font("Segoe UI Semibold", 10F);
            labelColors.Location = new Point(14, 12);
            labelColors.Name = "labelColors";
            labelColors.Size = new Size(48, 19);
            labelColors.TabIndex = 0;
            labelColors.Text = "Farben";
            // 
            // panelColors
            // 
            panelColors.Location = new Point(12, 38);
            panelColors.Name = "panelColors";
            panelColors.Size = new Size(222, 185);
            panelColors.TabIndex = 1;
            panelColors.Paint += PanelColors_Paint;
            panelColors.MouseClick += PanelColors_MouseClick;
            panelColors.MouseLeave += PanelColors_MouseLeave;
            panelColors.MouseMove += PanelColors_MouseMove;
            // 
            // panelPreview
            // 
            panelPreview.Location = new Point(14, 228);
            panelPreview.Name = "panelPreview";
            panelPreview.Size = new Size(218, 48);
            panelPreview.TabIndex = 2;
            panelPreview.Paint += PanelPreview_Paint;
            // 
            // labelWidth
            // 
            labelWidth.AutoSize = true;
            labelWidth.Font = new Font("Segoe UI Semibold", 10F);
            labelWidth.Location = new Point(14, 284);
            labelWidth.Name = "labelWidth";
            labelWidth.Size = new Size(45, 19);
            labelWidth.TabIndex = 3;
            labelWidth.Text = "Stärke";
            // 
            // trackWidth
            // 
            trackWidth.AutoSize = false;
            trackWidth.Location = new Point(8, 308);
            trackWidth.Maximum = 24;
            trackWidth.Minimum = 1;
            trackWidth.Name = "trackWidth";
            trackWidth.Size = new Size(230, 30);
            trackWidth.TabIndex = 4;
            trackWidth.TickStyle = TickStyle.None;
            trackWidth.Value = 4;
            trackWidth.ValueChanged += TrackWidth_ValueChanged;
            // 
            // labelThin
            // 
            labelThin.AutoSize = true;
            labelThin.Font = new Font("Segoe UI", 8F);
            labelThin.Location = new Point(12, 338);
            labelThin.Name = "labelThin";
            labelThin.Size = new Size(41, 13);
            labelThin.TabIndex = 5;
            labelThin.Text = "Schmal";
            // 
            // labelWide
            // 
            labelWide.Font = new Font("Segoe UI", 8F);
            labelWide.Location = new Point(162, 338);
            labelWide.Name = "labelWide";
            labelWide.Size = new Size(72, 13);
            labelWide.TabIndex = 6;
            labelWide.Text = "Breit";
            labelWide.TextAlign = ContentAlignment.TopRight;
            // 
            // InkPalette
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(labelWide);
            Controls.Add(labelThin);
            Controls.Add(trackWidth);
            Controls.Add(labelWidth);
            Controls.Add(panelPreview);
            Controls.Add(panelColors);
            Controls.Add(labelColors);
            Name = "InkPalette";
            Padding = new Padding(0, 0, 10, 12);
            Size = new Size(246, 362);
            ((System.ComponentModel.ISupportInitialize)trackWidth).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelColors;
        private Panel panelColors;
        private Panel panelPreview;
        private Label labelWidth;
        private TrackBar trackWidth;
        private Label labelThin;
        private Label labelWide;
    }
}