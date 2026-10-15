namespace PDFLight.Forms
{
    partial class AnnotationFilterForm
    {
        /// <summary>Erforderliche Designervariable.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Verwendete Ressourcen bereinigen.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) { components.Dispose(); }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        private void InitializeComponent()
        {
            tableLayout = new TableLayoutPanel();
            labelAuthor = new Label();
            flowAuthors = new FlowLayoutPanel();
            labelType = new Label();
            flowTypes = new FlowLayoutPanel();
            labelColor = new Label();
            flowColors = new FlowLayoutPanel();
            tableButtons = new TableLayoutPanel();
            linkShowAll = new LinkLabel();
            buttonApply = new Button();
            tableLayout.SuspendLayout();
            tableButtons.SuspendLayout();
            SuspendLayout();
            //
            // tableLayout
            //
            tableLayout.AutoSize = true;
            tableLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayout.ColumnCount = 1;
            tableLayout.ColumnStyles.Add(new ColumnStyle());
            tableLayout.Controls.Add(labelAuthor, 0, 0);
            tableLayout.Controls.Add(flowAuthors, 0, 1);
            tableLayout.Controls.Add(labelType, 0, 2);
            tableLayout.Controls.Add(flowTypes, 0, 3);
            tableLayout.Controls.Add(labelColor, 0, 4);
            tableLayout.Controls.Add(flowColors, 0, 5);
            tableLayout.Controls.Add(tableButtons, 0, 6);
            tableLayout.Location = new Point(14, 12);
            tableLayout.Name = "tableLayout";
            tableLayout.RowCount = 7;
            tableLayout.RowStyles.Add(new RowStyle());
            tableLayout.RowStyles.Add(new RowStyle());
            tableLayout.RowStyles.Add(new RowStyle());
            tableLayout.RowStyles.Add(new RowStyle());
            tableLayout.RowStyles.Add(new RowStyle());
            tableLayout.RowStyles.Add(new RowStyle());
            tableLayout.RowStyles.Add(new RowStyle());
            tableLayout.Size = new Size(280, 190);
            tableLayout.TabIndex = 0;
            //
            // labelAuthor
            //
            labelAuthor.AutoSize = true;
            labelAuthor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelAuthor.Margin = new Padding(0, 0, 0, 6);
            labelAuthor.Name = "labelAuthor";
            labelAuthor.Size = new Size(40, 15);
            labelAuthor.TabIndex = 0;
            labelAuthor.Text = "Autor";
            //
            // flowAuthors
            //
            flowAuthors.AutoSize = true;
            flowAuthors.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowAuthors.Margin = new Padding(0, 0, 0, 10);
            flowAuthors.MaximumSize = new Size(340, 0);
            flowAuthors.MinimumSize = new Size(280, 0);
            flowAuthors.Name = "flowAuthors";
            flowAuthors.Size = new Size(280, 0);
            flowAuthors.TabIndex = 1;
            //
            // labelType
            //
            labelType.AutoSize = true;
            labelType.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelType.Margin = new Padding(0, 0, 0, 6);
            labelType.Name = "labelType";
            labelType.Size = new Size(26, 15);
            labelType.TabIndex = 2;
            labelType.Text = "Art";
            //
            // flowTypes
            //
            flowTypes.AutoSize = true;
            flowTypes.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowTypes.Margin = new Padding(0, 0, 0, 10);
            flowTypes.MaximumSize = new Size(340, 0);
            flowTypes.MinimumSize = new Size(280, 0);
            flowTypes.Name = "flowTypes";
            flowTypes.Size = new Size(280, 0);
            flowTypes.TabIndex = 3;
            //
            // labelColor
            //
            labelColor.AutoSize = true;
            labelColor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelColor.Margin = new Padding(0, 0, 0, 6);
            labelColor.Name = "labelColor";
            labelColor.Size = new Size(38, 15);
            labelColor.TabIndex = 4;
            labelColor.Text = "Farbe";
            //
            // flowColors
            //
            flowColors.AutoSize = true;
            flowColors.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowColors.Margin = new Padding(0, 0, 0, 10);
            flowColors.MaximumSize = new Size(340, 0);
            flowColors.MinimumSize = new Size(280, 0);
            flowColors.Name = "flowColors";
            flowColors.Size = new Size(280, 0);
            flowColors.TabIndex = 5;
            //
            // tableButtons
            //
            tableButtons.AutoSize = true;
            tableButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableButtons.ColumnCount = 2;
            tableButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableButtons.ColumnStyles.Add(new ColumnStyle());
            tableButtons.Controls.Add(linkShowAll, 0, 0);
            tableButtons.Controls.Add(buttonApply, 1, 0);
            tableButtons.Dock = DockStyle.Fill;
            tableButtons.Margin = new Padding(0, 4, 0, 0);
            tableButtons.Name = "tableButtons";
            tableButtons.RowCount = 1;
            tableButtons.RowStyles.Add(new RowStyle());
            tableButtons.Size = new Size(280, 31);
            tableButtons.TabIndex = 6;
            //
            // linkShowAll
            //
            linkShowAll.Anchor = AnchorStyles.Left;
            linkShowAll.AutoSize = true;
            linkShowAll.LinkBehavior = LinkBehavior.HoverUnderline;
            linkShowAll.Margin = new Padding(0);
            linkShowAll.Name = "linkShowAll";
            linkShowAll.Size = new Size(76, 15);
            linkShowAll.TabIndex = 0;
            linkShowAll.TabStop = true;
            linkShowAll.Text = "Alle anzeigen";
            linkShowAll.LinkClicked += LinkShowAll_LinkClicked;
            //
            // buttonApply
            //
            buttonApply.AutoSize = true;
            buttonApply.Margin = new Padding(0);
            buttonApply.MinimumSize = new Size(96, 27);
            buttonApply.Name = "buttonApply";
            buttonApply.Size = new Size(96, 27);
            buttonApply.TabIndex = 1;
            buttonApply.Text = "Anwenden";
            buttonApply.UseVisualStyleBackColor = true;
            buttonApply.Click += ButtonApply_Click;
            //
            // AnnotationFilterForm
            //
            AcceptButton = buttonApply;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(308, 214);
            Controls.Add(tableLayout);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AnnotationFilterForm";
            Padding = new Padding(14, 12, 14, 12);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "Anmerkungen filtern";
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            tableButtons.ResumeLayout(false);
            tableButtons.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel tableLayout;
        private Label labelAuthor;
        private FlowLayoutPanel flowAuthors;
        private Label labelType;
        private FlowLayoutPanel flowTypes;
        private Label labelColor;
        private FlowLayoutPanel flowColors;
        private TableLayoutPanel tableButtons;
        private LinkLabel linkShowAll;
        private Button buttonApply;
    }
}
