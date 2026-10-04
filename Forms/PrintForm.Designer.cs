namespace PDFLight.Forms
{
    partial class PrintForm
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
            panelSettings = new Panel();
            flowSettings = new FlowLayoutPanel();
            labelPrinter = new Label();
            comboPrinter = new ComboBox();
            labelCopies = new Label();
            numCopies = new NumericUpDown();
            labelLayout = new Label();
            panelLayout = new FlowLayoutPanel();
            rbAuto = new RadioButton();
            rbPortrait = new RadioButton();
            rbLandscape = new RadioButton();
            labelPages = new Label();
            panelPages = new FlowLayoutPanel();
            rbAll = new RadioButton();
            rbCurrent = new RadioButton();
            rbOdd = new RadioButton();
            rbEven = new RadioButton();
            rbCustom = new RadioButton();
            textRange = new TextBox();
            labelRangeError = new Label();
            labelDuplex = new Label();
            comboDuplex = new ComboBox();
            linkMore = new LinkLabel();
            panelMore = new FlowLayoutPanel();
            labelPaper = new Label();
            comboPaper = new ComboBox();
            labelScaling = new Label();
            comboScaling = new ComboBox();
            numScale = new NumericUpDown();
            labelColor = new Label();
            comboColor = new ComboBox();
            linkSystem = new LinkLabel();
            panelButtons = new Panel();
            buttonPrint = new Button();
            buttonCancel = new Button();
            panelHeader = new Panel();
            labelSheets = new Label();
            labelTitle = new Label();
            previewView = new PDFLight.Viewer.PrintPreviewView();
            panelSettings.SuspendLayout();
            flowSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCopies).BeginInit();
            panelLayout.SuspendLayout();
            panelPages.SuspendLayout();
            panelMore.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numScale).BeginInit();
            panelButtons.SuspendLayout();
            panelHeader.SuspendLayout();
            SuspendLayout();
            //
            // panelSettings
            //
            panelSettings.BackColor = SystemColors.Window;
            panelSettings.Controls.Add(flowSettings);
            panelSettings.Controls.Add(panelButtons);
            panelSettings.Controls.Add(panelHeader);
            panelSettings.Dock = DockStyle.Left;
            panelSettings.Location = new Point(0, 0);
            panelSettings.Name = "panelSettings";
            panelSettings.Size = new Size(300, 681);
            panelSettings.TabIndex = 0;
            //
            // flowSettings
            //
            flowSettings.AutoScroll = true;
            flowSettings.Controls.Add(labelPrinter);
            flowSettings.Controls.Add(comboPrinter);
            flowSettings.Controls.Add(labelCopies);
            flowSettings.Controls.Add(numCopies);
            flowSettings.Controls.Add(labelLayout);
            flowSettings.Controls.Add(panelLayout);
            flowSettings.Controls.Add(labelPages);
            flowSettings.Controls.Add(panelPages);
            flowSettings.Controls.Add(labelDuplex);
            flowSettings.Controls.Add(comboDuplex);
            flowSettings.Controls.Add(linkMore);
            flowSettings.Controls.Add(panelMore);
            flowSettings.Controls.Add(linkSystem);
            flowSettings.Dock = DockStyle.Fill;
            flowSettings.FlowDirection = FlowDirection.TopDown;
            flowSettings.Location = new Point(0, 64);
            flowSettings.Name = "flowSettings";
            flowSettings.Padding = new Padding(16, 0, 8, 8);
            flowSettings.Size = new Size(300, 561);
            flowSettings.TabIndex = 1;
            flowSettings.WrapContents = false;
            //
            // labelPrinter
            //
            labelPrinter.AutoSize = true;
            labelPrinter.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelPrinter.Location = new Point(16, 8);
            labelPrinter.Margin = new Padding(0, 8, 0, 4);
            labelPrinter.Name = "labelPrinter";
            labelPrinter.Size = new Size(51, 15);
            labelPrinter.TabIndex = 0;
            labelPrinter.Text = "Drucker";
            //
            // comboPrinter
            //
            comboPrinter.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPrinter.Location = new Point(16, 27);
            comboPrinter.Margin = new Padding(0);
            comboPrinter.Name = "comboPrinter";
            comboPrinter.Size = new Size(260, 23);
            comboPrinter.TabIndex = 1;
            comboPrinter.SelectedIndexChanged += ComboPrinter_SelectedIndexChanged;
            //
            // labelCopies
            //
            labelCopies.AutoSize = true;
            labelCopies.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelCopies.Location = new Point(16, 64);
            labelCopies.Margin = new Padding(0, 14, 0, 4);
            labelCopies.Name = "labelCopies";
            labelCopies.Size = new Size(46, 15);
            labelCopies.TabIndex = 2;
            labelCopies.Text = "Kopien";
            //
            // numCopies
            //
            numCopies.Location = new Point(16, 83);
            numCopies.Margin = new Padding(0);
            numCopies.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            numCopies.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCopies.Name = "numCopies";
            numCopies.Size = new Size(80, 23);
            numCopies.TabIndex = 3;
            numCopies.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numCopies.ValueChanged += Settings_Changed;
            //
            // labelLayout
            //
            labelLayout.AutoSize = true;
            labelLayout.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelLayout.Location = new Point(16, 120);
            labelLayout.Margin = new Padding(0, 14, 0, 2);
            labelLayout.Name = "labelLayout";
            labelLayout.Size = new Size(45, 15);
            labelLayout.TabIndex = 4;
            labelLayout.Text = "Layout";
            //
            // panelLayout
            //
            panelLayout.AutoSize = true;
            panelLayout.Controls.Add(rbAuto);
            panelLayout.Controls.Add(rbPortrait);
            panelLayout.Controls.Add(rbLandscape);
            panelLayout.FlowDirection = FlowDirection.TopDown;
            panelLayout.Location = new Point(16, 137);
            panelLayout.Margin = new Padding(0);
            panelLayout.Name = "panelLayout";
            panelLayout.Size = new Size(190, 75);
            panelLayout.TabIndex = 5;
            panelLayout.WrapContents = false;
            //
            // rbAuto
            //
            rbAuto.AutoSize = true;
            rbAuto.Checked = true;
            rbAuto.Location = new Point(0, 3);
            rbAuto.Margin = new Padding(0, 3, 0, 3);
            rbAuto.Name = "rbAuto";
            rbAuto.Size = new Size(190, 19);
            rbAuto.TabIndex = 0;
            rbAuto.TabStop = true;
            rbAuto.Text = "Wie die Seite (Hoch-/Querformat)";
            rbAuto.UseVisualStyleBackColor = true;
            rbAuto.CheckedChanged += Settings_Changed;
            //
            // rbPortrait
            //
            rbPortrait.AutoSize = true;
            rbPortrait.Location = new Point(0, 28);
            rbPortrait.Margin = new Padding(0, 3, 0, 3);
            rbPortrait.Name = "rbPortrait";
            rbPortrait.Size = new Size(83, 19);
            rbPortrait.TabIndex = 1;
            rbPortrait.Text = "Hochformat";
            rbPortrait.UseVisualStyleBackColor = true;
            rbPortrait.CheckedChanged += Settings_Changed;
            //
            // rbLandscape
            //
            rbLandscape.AutoSize = true;
            rbLandscape.Location = new Point(0, 53);
            rbLandscape.Margin = new Padding(0, 3, 0, 3);
            rbLandscape.Name = "rbLandscape";
            rbLandscape.Size = new Size(80, 19);
            rbLandscape.TabIndex = 2;
            rbLandscape.Text = "Querformat";
            rbLandscape.UseVisualStyleBackColor = true;
            rbLandscape.CheckedChanged += Settings_Changed;
            //
            // labelPages
            //
            labelPages.AutoSize = true;
            labelPages.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelPages.Location = new Point(16, 226);
            labelPages.Margin = new Padding(0, 14, 0, 2);
            labelPages.Name = "labelPages";
            labelPages.Size = new Size(41, 15);
            labelPages.TabIndex = 6;
            labelPages.Text = "Seiten";
            //
            // panelPages
            //
            panelPages.AutoSize = true;
            panelPages.Controls.Add(rbAll);
            panelPages.Controls.Add(rbCurrent);
            panelPages.Controls.Add(rbOdd);
            panelPages.Controls.Add(rbEven);
            panelPages.Controls.Add(rbCustom);
            panelPages.Controls.Add(textRange);
            panelPages.Controls.Add(labelRangeError);
            panelPages.FlowDirection = FlowDirection.TopDown;
            panelPages.Location = new Point(16, 243);
            panelPages.Margin = new Padding(0);
            panelPages.Name = "panelPages";
            panelPages.Size = new Size(220, 170);
            panelPages.TabIndex = 7;
            panelPages.WrapContents = false;
            //
            // rbAll
            //
            rbAll.AutoSize = true;
            rbAll.Checked = true;
            rbAll.Location = new Point(0, 3);
            rbAll.Margin = new Padding(0, 3, 0, 3);
            rbAll.Name = "rbAll";
            rbAll.Size = new Size(46, 19);
            rbAll.TabIndex = 0;
            rbAll.TabStop = true;
            rbAll.Text = "Alle";
            rbAll.UseVisualStyleBackColor = true;
            rbAll.CheckedChanged += Settings_Changed;
            //
            // rbCurrent
            //
            rbCurrent.AutoSize = true;
            rbCurrent.Location = new Point(0, 28);
            rbCurrent.Margin = new Padding(0, 3, 0, 3);
            rbCurrent.Name = "rbCurrent";
            rbCurrent.Size = new Size(97, 19);
            rbCurrent.TabIndex = 1;
            rbCurrent.Text = "Aktuelle Seite";
            rbCurrent.UseVisualStyleBackColor = true;
            rbCurrent.CheckedChanged += Settings_Changed;
            //
            // rbOdd
            //
            rbOdd.AutoSize = true;
            rbOdd.Location = new Point(0, 53);
            rbOdd.Margin = new Padding(0, 3, 0, 3);
            rbOdd.Name = "rbOdd";
            rbOdd.Size = new Size(141, 19);
            rbOdd.TabIndex = 2;
            rbOdd.Text = "Nur ungerade Seiten";
            rbOdd.UseVisualStyleBackColor = true;
            rbOdd.CheckedChanged += Settings_Changed;
            //
            // rbEven
            //
            rbEven.AutoSize = true;
            rbEven.Location = new Point(0, 78);
            rbEven.Margin = new Padding(0, 3, 0, 3);
            rbEven.Name = "rbEven";
            rbEven.Size = new Size(126, 19);
            rbEven.TabIndex = 3;
            rbEven.Text = "Nur gerade Seiten";
            rbEven.UseVisualStyleBackColor = true;
            rbEven.CheckedChanged += Settings_Changed;
            //
            // rbCustom
            //
            rbCustom.AutoSize = true;
            rbCustom.Location = new Point(0, 103);
            rbCustom.Margin = new Padding(0, 3, 0, 3);
            rbCustom.Name = "rbCustom";
            rbCustom.Size = new Size(118, 19);
            rbCustom.TabIndex = 4;
            rbCustom.Text = "Benutzerdefiniert:";
            rbCustom.UseVisualStyleBackColor = true;
            rbCustom.CheckedChanged += Settings_Changed;
            //
            // textRange
            //
            textRange.Location = new Point(18, 125);
            textRange.Margin = new Padding(18, 0, 0, 0);
            textRange.Name = "textRange";
            textRange.Size = new Size(200, 23);
            textRange.TabIndex = 5;
            textRange.TextChanged += TextRange_TextChanged;
            //
            // labelRangeError
            //
            labelRangeError.AutoSize = true;
            labelRangeError.ForeColor = Color.Firebrick;
            labelRangeError.Location = new Point(18, 150);
            labelRangeError.Margin = new Padding(18, 2, 0, 0);
            labelRangeError.Name = "labelRangeError";
            labelRangeError.Size = new Size(118, 15);
            labelRangeError.TabIndex = 6;
            labelRangeError.Text = "Ungültige Seitenangabe";
            labelRangeError.Visible = false;
            //
            // labelDuplex
            //
            labelDuplex.AutoSize = true;
            labelDuplex.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelDuplex.Location = new Point(16, 427);
            labelDuplex.Margin = new Padding(0, 14, 0, 4);
            labelDuplex.Name = "labelDuplex";
            labelDuplex.Size = new Size(110, 15);
            labelDuplex.TabIndex = 8;
            labelDuplex.Text = "Beidseitiger Druck";
            //
            // comboDuplex
            //
            comboDuplex.DropDownStyle = ComboBoxStyle.DropDownList;
            comboDuplex.Location = new Point(16, 446);
            comboDuplex.Margin = new Padding(0);
            comboDuplex.Name = "comboDuplex";
            comboDuplex.Size = new Size(260, 23);
            comboDuplex.TabIndex = 9;
            comboDuplex.SelectedIndexChanged += Settings_Changed;
            //
            // linkMore
            //
            linkMore.AutoSize = true;
            linkMore.Location = new Point(16, 483);
            linkMore.Margin = new Padding(0, 14, 0, 4);
            linkMore.Name = "linkMore";
            linkMore.Size = new Size(126, 15);
            linkMore.TabIndex = 10;
            linkMore.TabStop = true;
            linkMore.Text = "Weitere Einstellungen ▾";
            linkMore.LinkClicked += LinkMore_LinkClicked;
            //
            // panelMore
            //
            panelMore.AutoSize = true;
            panelMore.Controls.Add(labelPaper);
            panelMore.Controls.Add(comboPaper);
            panelMore.Controls.Add(labelScaling);
            panelMore.Controls.Add(comboScaling);
            panelMore.Controls.Add(numScale);
            panelMore.Controls.Add(labelColor);
            panelMore.Controls.Add(comboColor);
            panelMore.FlowDirection = FlowDirection.TopDown;
            panelMore.Location = new Point(16, 502);
            panelMore.Margin = new Padding(0);
            panelMore.Name = "panelMore";
            panelMore.Size = new Size(260, 180);
            panelMore.TabIndex = 11;
            panelMore.Visible = false;
            panelMore.WrapContents = false;
            //
            // labelPaper
            //
            labelPaper.AutoSize = true;
            labelPaper.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelPaper.Location = new Point(0, 6);
            labelPaper.Margin = new Padding(0, 6, 0, 4);
            labelPaper.Name = "labelPaper";
            labelPaper.Size = new Size(84, 15);
            labelPaper.TabIndex = 0;
            labelPaper.Text = "Papierformat";
            //
            // comboPaper
            //
            comboPaper.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPaper.Location = new Point(0, 25);
            comboPaper.Margin = new Padding(0);
            comboPaper.Name = "comboPaper";
            comboPaper.Size = new Size(260, 23);
            comboPaper.TabIndex = 1;
            comboPaper.SelectedIndexChanged += Settings_Changed;
            //
            // labelScaling
            //
            labelScaling.AutoSize = true;
            labelScaling.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelScaling.Location = new Point(0, 62);
            labelScaling.Margin = new Padding(0, 14, 0, 4);
            labelScaling.Name = "labelScaling";
            labelScaling.Size = new Size(70, 15);
            labelScaling.TabIndex = 2;
            labelScaling.Text = "Skalierung";
            //
            // comboScaling
            //
            comboScaling.DropDownStyle = ComboBoxStyle.DropDownList;
            comboScaling.Location = new Point(0, 81);
            comboScaling.Margin = new Padding(0);
            comboScaling.Name = "comboScaling";
            comboScaling.Size = new Size(260, 23);
            comboScaling.TabIndex = 3;
            comboScaling.SelectedIndexChanged += Settings_Changed;
            //
            // numScale
            //
            numScale.Enabled = false;
            numScale.Location = new Point(0, 108);
            numScale.Margin = new Padding(0, 4, 0, 0);
            numScale.Maximum = new decimal(new int[] { 400, 0, 0, 0 });
            numScale.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numScale.Name = "numScale";
            numScale.Size = new Size(80, 23);
            numScale.TabIndex = 4;
            numScale.Value = new decimal(new int[] { 100, 0, 0, 0 });
            numScale.ValueChanged += Settings_Changed;
            //
            // labelColor
            //
            labelColor.AutoSize = true;
            labelColor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelColor.Location = new Point(0, 145);
            labelColor.Margin = new Padding(0, 14, 0, 4);
            labelColor.Name = "labelColor";
            labelColor.Size = new Size(39, 15);
            labelColor.TabIndex = 5;
            labelColor.Text = "Farbe";
            //
            // comboColor
            //
            comboColor.DropDownStyle = ComboBoxStyle.DropDownList;
            comboColor.Location = new Point(0, 164);
            comboColor.Margin = new Padding(0);
            comboColor.Name = "comboColor";
            comboColor.Size = new Size(260, 23);
            comboColor.TabIndex = 6;
            comboColor.SelectedIndexChanged += Settings_Changed;
            //
            // linkSystem
            //
            linkSystem.AutoSize = true;
            linkSystem.Location = new Point(16, 696);
            linkSystem.Margin = new Padding(0, 14, 0, 4);
            linkSystem.MaximumSize = new Size(260, 0);
            linkSystem.Name = "linkSystem";
            linkSystem.Size = new Size(245, 30);
            linkSystem.TabIndex = 12;
            linkSystem.TabStop = true;
            linkSystem.Text = "Drucken mithilfe des Systemdialogfelds… (Strg+Umschalt+P)";
            linkSystem.LinkClicked += LinkSystem_LinkClicked;
            //
            // panelButtons
            //
            panelButtons.Controls.Add(buttonPrint);
            panelButtons.Controls.Add(buttonCancel);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 625);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(300, 56);
            panelButtons.TabIndex = 2;
            //
            // buttonPrint
            //
            buttonPrint.Location = new Point(16, 12);
            buttonPrint.Name = "buttonPrint";
            buttonPrint.Size = new Size(124, 32);
            buttonPrint.TabIndex = 0;
            buttonPrint.Text = "Drucken";
            buttonPrint.UseVisualStyleBackColor = true;
            buttonPrint.Click += ButtonPrint_Click;
            //
            // buttonCancel
            //
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(152, 12);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(124, 32);
            buttonCancel.TabIndex = 1;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // panelHeader
            //
            panelHeader.Controls.Add(labelSheets);
            panelHeader.Controls.Add(labelTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(300, 64);
            panelHeader.TabIndex = 0;
            //
            // labelSheets
            //
            labelSheets.AutoSize = true;
            labelSheets.Location = new Point(16, 40);
            labelSheets.Name = "labelSheets";
            labelSheets.Size = new Size(0, 15);
            labelSheets.TabIndex = 1;
            //
            // labelTitle
            //
            labelTitle.AutoSize = true;
            labelTitle.Font = new Font("Segoe UI", 14F);
            labelTitle.Location = new Point(14, 10);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(74, 25);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Drucken";
            //
            // previewView
            //
            previewView.Dock = DockStyle.Fill;
            previewView.Location = new Point(300, 0);
            previewView.Name = "previewView";
            previewView.Size = new Size(784, 681);
            previewView.TabIndex = 1;
            //
            // PrintForm
            //
            AcceptButton = buttonPrint;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(1084, 681);
            Controls.Add(previewView);
            Controls.Add(panelSettings);
            KeyPreview = true;
            MinimizeBox = false;
            MinimumSize = new Size(760, 520);
            Name = "PrintForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Drucken";
            Load += PrintForm_Load;
            panelSettings.ResumeLayout(false);
            flowSettings.ResumeLayout(false);
            flowSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCopies).EndInit();
            panelLayout.ResumeLayout(false);
            panelLayout.PerformLayout();
            panelPages.ResumeLayout(false);
            panelPages.PerformLayout();
            panelMore.ResumeLayout(false);
            panelMore.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numScale).EndInit();
            panelButtons.ResumeLayout(false);
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelSettings;
        private FlowLayoutPanel flowSettings;
        private Label labelPrinter;
        private ComboBox comboPrinter;
        private Label labelCopies;
        private NumericUpDown numCopies;
        private Label labelLayout;
        private FlowLayoutPanel panelLayout;
        private RadioButton rbAuto;
        private RadioButton rbPortrait;
        private RadioButton rbLandscape;
        private Label labelPages;
        private FlowLayoutPanel panelPages;
        private RadioButton rbAll;
        private RadioButton rbCurrent;
        private RadioButton rbOdd;
        private RadioButton rbEven;
        private RadioButton rbCustom;
        private TextBox textRange;
        private Label labelRangeError;
        private Label labelDuplex;
        private ComboBox comboDuplex;
        private LinkLabel linkMore;
        private FlowLayoutPanel panelMore;
        private Label labelPaper;
        private ComboBox comboPaper;
        private Label labelScaling;
        private ComboBox comboScaling;
        private NumericUpDown numScale;
        private Label labelColor;
        private ComboBox comboColor;
        private LinkLabel linkSystem;
        private Panel panelButtons;
        private Button buttonPrint;
        private Button buttonCancel;
        private Panel panelHeader;
        private Label labelSheets;
        private Label labelTitle;
        private PDFLight.Viewer.PrintPreviewView previewView;
    }
}
