namespace PDFLight.Forms
{
    partial class PropertiesForm
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
            toolTipReveal = new ToolTip(components);
            tabControl = new TabControl();
            tabDescription = new TabPage();
            groupAdvanced = new GroupBox();
            labelFastWebValue = new Label();
            labelFastWeb = new Label();
            labelTaggedValue = new Label();
            labelTagged = new Label();
            labelPagesValue = new Label();
            labelPages = new Label();
            labelFormatValue = new Label();
            labelFormat = new Label();
            labelSizeValue = new Label();
            labelSize = new Label();
            labelLocationValue = new Label();
            labelLocation = new Label();
            labelVersionValue = new Label();
            labelVersion = new Label();
            labelProducerValue = new Label();
            labelProducer = new Label();
            groupDescription = new GroupBox();
            buttonRemove = new Button();
            labelCreatorValue = new Label();
            labelCreator = new Label();
            labelModifiedValue = new Label();
            labelModified = new Label();
            labelCreatedValue = new Label();
            labelCreated = new Label();
            textBoxKeywords = new TextBox();
            labelKeywords = new Label();
            textBoxSubject = new TextBox();
            labelSubject = new Label();
            textBoxAuthor = new TextBox();
            labelAuthor = new Label();
            textBoxTitle = new TextBox();
            labelTitle = new Label();
            labelFileValue = new Label();
            labelFile = new Label();
            tabSecurity = new TabPage();
            groupPermissions = new GroupBox();
            checkRemoveRestrictions = new CheckBox();
            listPermissions = new ListView();
            columnPermission = new ColumnHeader();
            columnPermissionValue = new ColumnHeader();
            groupSecurity = new GroupBox();
            labelPasswordHint = new Label();
            textPasswordRepeat = new TextBox();
            labelPasswordRepeat = new Label();
            textPassword = new TextBox();
            labelPassword = new Label();
            checkPassword = new CheckBox();
            labelSecurityStatus = new Label();
            tabFonts = new TabPage();
            groupFonts = new GroupBox();
            treeFonts = new TreeView();
            tabAttachments = new TabPage();
            labelAttachmentHint = new Label();
            buttonOpenAttachment = new Button();
            buttonSaveAttachment = new Button();
            listAttachments = new PDFLight.Controls.StaticHeaderListView();
            columnAttachmentName = new ColumnHeader();
            columnAttachmentModified = new ColumnHeader();
            columnAttachmentSize = new ColumnHeader();
            contextMenuAttachments = new ContextMenuStrip(components);
            mnuOpenAttachment = new ToolStripMenuItem();
            mnuSaveAttachment = new ToolStripMenuItem();
            buttonOK = new Button();
            buttonCancel = new Button();
            saveAttachmentDialog = new SaveFileDialog();
            tabControl.SuspendLayout();
            tabDescription.SuspendLayout();
            groupAdvanced.SuspendLayout();
            groupDescription.SuspendLayout();
            tabSecurity.SuspendLayout();
            groupPermissions.SuspendLayout();
            groupSecurity.SuspendLayout();
            tabFonts.SuspendLayout();
            groupFonts.SuspendLayout();
            tabAttachments.SuspendLayout();
            contextMenuAttachments.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Controls.Add(tabDescription);
            tabControl.Controls.Add(tabSecurity);
            tabControl.Controls.Add(tabFonts);
            tabControl.Controls.Add(tabAttachments);
            tabControl.Location = new Point(12, 12);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(616, 498);
            tabControl.TabIndex = 0;
            // 
            // tabDescription
            // 
            tabDescription.Controls.Add(groupAdvanced);
            tabDescription.Controls.Add(groupDescription);
            tabDescription.Location = new Point(4, 24);
            tabDescription.Name = "tabDescription";
            tabDescription.Padding = new Padding(3);
            tabDescription.Size = new Size(608, 470);
            tabDescription.TabIndex = 0;
            tabDescription.Text = "Beschreibung";
            tabDescription.UseVisualStyleBackColor = true;
            // 
            // groupAdvanced
            // 
            groupAdvanced.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupAdvanced.Controls.Add(labelFastWebValue);
            groupAdvanced.Controls.Add(labelFastWeb);
            groupAdvanced.Controls.Add(labelTaggedValue);
            groupAdvanced.Controls.Add(labelTagged);
            groupAdvanced.Controls.Add(labelPagesValue);
            groupAdvanced.Controls.Add(labelPages);
            groupAdvanced.Controls.Add(labelFormatValue);
            groupAdvanced.Controls.Add(labelFormat);
            groupAdvanced.Controls.Add(labelSizeValue);
            groupAdvanced.Controls.Add(labelSize);
            groupAdvanced.Controls.Add(labelLocationValue);
            groupAdvanced.Controls.Add(labelLocation);
            groupAdvanced.Controls.Add(labelVersionValue);
            groupAdvanced.Controls.Add(labelVersion);
            groupAdvanced.Controls.Add(labelProducerValue);
            groupAdvanced.Controls.Add(labelProducer);
            groupAdvanced.Location = new Point(8, 272);
            groupAdvanced.Name = "groupAdvanced";
            groupAdvanced.Size = new Size(592, 192);
            groupAdvanced.TabIndex = 1;
            groupAdvanced.TabStop = false;
            groupAdvanced.Text = "Erweitert";
            // 
            // labelFastWebValue
            // 
            labelFastWebValue.AutoSize = true;
            labelFastWebValue.Location = new Point(135, 168);
            labelFastWebValue.Name = "labelFastWebValue";
            labelFastWebValue.Size = new Size(13, 15);
            labelFastWebValue.TabIndex = 15;
            labelFastWebValue.Text = "–";
            // 
            // labelFastWeb
            // 
            labelFastWeb.Location = new Point(3, 168);
            labelFastWeb.Name = "labelFastWeb";
            labelFastWeb.Size = new Size(126, 15);
            labelFastWeb.TabIndex = 14;
            labelFastWeb.Text = "Schnelle Webanzeige:";
            labelFastWeb.TextAlign = ContentAlignment.TopRight;
            // 
            // labelTaggedValue
            // 
            labelTaggedValue.AutoSize = true;
            labelTaggedValue.Location = new Point(416, 144);
            labelTaggedValue.Name = "labelTaggedValue";
            labelTaggedValue.Size = new Size(13, 15);
            labelTaggedValue.TabIndex = 13;
            labelTaggedValue.Text = "–";
            // 
            // labelTagged
            // 
            labelTagged.Location = new Point(300, 144);
            labelTagged.Name = "labelTagged";
            labelTagged.Size = new Size(110, 15);
            labelTagged.TabIndex = 12;
            labelTagged.Text = "PDF mit Tags:";
            labelTagged.TextAlign = ContentAlignment.TopRight;
            // 
            // labelPagesValue
            // 
            labelPagesValue.AutoSize = true;
            labelPagesValue.Location = new Point(135, 144);
            labelPagesValue.Name = "labelPagesValue";
            labelPagesValue.Size = new Size(13, 15);
            labelPagesValue.TabIndex = 11;
            labelPagesValue.Text = "–";
            // 
            // labelPages
            // 
            labelPages.Location = new Point(3, 144);
            labelPages.Name = "labelPages";
            labelPages.Size = new Size(126, 15);
            labelPages.TabIndex = 10;
            labelPages.Text = "Seitenanzahl:";
            labelPages.TextAlign = ContentAlignment.TopRight;
            // 
            // labelFormatValue
            // 
            labelFormatValue.AutoEllipsis = true;
            labelFormatValue.Location = new Point(135, 120);
            labelFormatValue.Name = "labelFormatValue";
            labelFormatValue.Size = new Size(441, 15);
            labelFormatValue.TabIndex = 9;
            labelFormatValue.Text = "–";
            // 
            // labelFormat
            // 
            labelFormat.Location = new Point(6, 120);
            labelFormat.Name = "labelFormat";
            labelFormat.Size = new Size(123, 15);
            labelFormat.TabIndex = 8;
            labelFormat.Text = "Seitenformat:";
            labelFormat.TextAlign = ContentAlignment.TopRight;
            // 
            // labelSizeValue
            // 
            labelSizeValue.AutoEllipsis = true;
            labelSizeValue.Location = new Point(135, 96);
            labelSizeValue.Name = "labelSizeValue";
            labelSizeValue.Size = new Size(441, 15);
            labelSizeValue.TabIndex = 7;
            labelSizeValue.Text = "–";
            // 
            // labelSize
            // 
            labelSize.Location = new Point(3, 96);
            labelSize.Name = "labelSize";
            labelSize.Size = new Size(126, 15);
            labelSize.TabIndex = 6;
            labelSize.Text = "Dateigröße:";
            labelSize.TextAlign = ContentAlignment.TopRight;
            // 
            // labelLocationValue
            // 
            labelLocationValue.AutoEllipsis = true;
            labelLocationValue.Location = new Point(135, 72);
            labelLocationValue.Name = "labelLocationValue";
            labelLocationValue.Size = new Size(441, 15);
            labelLocationValue.TabIndex = 5;
            labelLocationValue.Text = "–";
            // 
            // labelLocation
            // 
            labelLocation.Location = new Point(3, 72);
            labelLocation.Name = "labelLocation";
            labelLocation.Size = new Size(126, 15);
            labelLocation.TabIndex = 4;
            labelLocation.Text = "Speicherort:";
            labelLocation.TextAlign = ContentAlignment.TopRight;
            // 
            // labelVersionValue
            // 
            labelVersionValue.AutoEllipsis = true;
            labelVersionValue.Location = new Point(135, 48);
            labelVersionValue.Name = "labelVersionValue";
            labelVersionValue.Size = new Size(441, 15);
            labelVersionValue.TabIndex = 3;
            labelVersionValue.Text = "–";
            // 
            // labelVersion
            // 
            labelVersion.Location = new Point(3, 48);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new Size(126, 15);
            labelVersion.TabIndex = 2;
            labelVersion.Text = "PDF-Version:";
            labelVersion.TextAlign = ContentAlignment.TopRight;
            // 
            // labelProducerValue
            // 
            labelProducerValue.AutoEllipsis = true;
            labelProducerValue.Location = new Point(135, 24);
            labelProducerValue.Name = "labelProducerValue";
            labelProducerValue.Size = new Size(441, 15);
            labelProducerValue.TabIndex = 1;
            labelProducerValue.Text = "–";
            // 
            // labelProducer
            // 
            labelProducer.Location = new Point(3, 24);
            labelProducer.Name = "labelProducer";
            labelProducer.Size = new Size(126, 15);
            labelProducer.TabIndex = 0;
            labelProducer.Text = "PDF erstellt mit:";
            labelProducer.TextAlign = ContentAlignment.TopRight;
            // 
            // groupDescription
            // 
            groupDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupDescription.Controls.Add(buttonRemove);
            groupDescription.Controls.Add(labelCreatorValue);
            groupDescription.Controls.Add(labelCreator);
            groupDescription.Controls.Add(labelModifiedValue);
            groupDescription.Controls.Add(labelModified);
            groupDescription.Controls.Add(labelCreatedValue);
            groupDescription.Controls.Add(labelCreated);
            groupDescription.Controls.Add(textBoxKeywords);
            groupDescription.Controls.Add(labelKeywords);
            groupDescription.Controls.Add(textBoxSubject);
            groupDescription.Controls.Add(labelSubject);
            groupDescription.Controls.Add(textBoxAuthor);
            groupDescription.Controls.Add(labelAuthor);
            groupDescription.Controls.Add(textBoxTitle);
            groupDescription.Controls.Add(labelTitle);
            groupDescription.Controls.Add(labelFileValue);
            groupDescription.Controls.Add(labelFile);
            groupDescription.Location = new Point(8, 8);
            groupDescription.Name = "groupDescription";
            groupDescription.Size = new Size(592, 258);
            groupDescription.TabIndex = 0;
            groupDescription.TabStop = false;
            groupDescription.Text = "Beschreibung";
            // 
            // buttonRemove
            // 
            buttonRemove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonRemove.Location = new Point(416, 222);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new Size(160, 27);
            buttonRemove.TabIndex = 16;
            buttonRemove.Text = "Metadaten &entfernen";
            buttonRemove.UseVisualStyleBackColor = true;
            buttonRemove.Click += ButtonRemove_Click;
            // 
            // labelCreatorValue
            // 
            labelCreatorValue.AutoEllipsis = true;
            labelCreatorValue.Location = new Point(135, 226);
            labelCreatorValue.Name = "labelCreatorValue";
            labelCreatorValue.Size = new Size(275, 15);
            labelCreatorValue.TabIndex = 15;
            labelCreatorValue.Text = "–";
            // 
            // labelCreator
            // 
            labelCreator.Location = new Point(19, 226);
            labelCreator.Name = "labelCreator";
            labelCreator.Size = new Size(110, 15);
            labelCreator.TabIndex = 14;
            labelCreator.Text = "Anwendung:";
            labelCreator.TextAlign = ContentAlignment.TopRight;
            // 
            // labelModifiedValue
            // 
            labelModifiedValue.AutoSize = true;
            labelModifiedValue.Location = new Point(416, 202);
            labelModifiedValue.Name = "labelModifiedValue";
            labelModifiedValue.Size = new Size(13, 15);
            labelModifiedValue.TabIndex = 13;
            labelModifiedValue.Text = "–";
            // 
            // labelModified
            // 
            labelModified.Location = new Point(300, 202);
            labelModified.Name = "labelModified";
            labelModified.Size = new Size(110, 15);
            labelModified.TabIndex = 12;
            labelModified.Text = "Geändert am:";
            labelModified.TextAlign = ContentAlignment.TopRight;
            // 
            // labelCreatedValue
            // 
            labelCreatedValue.AutoSize = true;
            labelCreatedValue.Location = new Point(135, 202);
            labelCreatedValue.Name = "labelCreatedValue";
            labelCreatedValue.Size = new Size(13, 15);
            labelCreatedValue.TabIndex = 11;
            labelCreatedValue.Text = "–";
            // 
            // labelCreated
            // 
            labelCreated.Location = new Point(19, 202);
            labelCreated.Name = "labelCreated";
            labelCreated.Size = new Size(110, 15);
            labelCreated.TabIndex = 10;
            labelCreated.Text = "Erstellt am:";
            labelCreated.TextAlign = ContentAlignment.TopRight;
            // 
            // textBoxKeywords
            // 
            textBoxKeywords.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxKeywords.Location = new Point(135, 137);
            textBoxKeywords.Multiline = true;
            textBoxKeywords.Name = "textBoxKeywords";
            textBoxKeywords.ScrollBars = ScrollBars.Vertical;
            textBoxKeywords.Size = new Size(441, 54);
            textBoxKeywords.TabIndex = 9;
            // 
            // labelKeywords
            // 
            labelKeywords.Location = new Point(19, 140);
            labelKeywords.Name = "labelKeywords";
            labelKeywords.Size = new Size(110, 15);
            labelKeywords.TabIndex = 8;
            labelKeywords.Text = "&Stichwörter:";
            labelKeywords.TextAlign = ContentAlignment.TopRight;
            // 
            // textBoxSubject
            // 
            textBoxSubject.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxSubject.Location = new Point(135, 108);
            textBoxSubject.Name = "textBoxSubject";
            textBoxSubject.Size = new Size(441, 23);
            textBoxSubject.TabIndex = 7;
            // 
            // labelSubject
            // 
            labelSubject.Location = new Point(19, 111);
            labelSubject.Name = "labelSubject";
            labelSubject.Size = new Size(110, 15);
            labelSubject.TabIndex = 6;
            labelSubject.Text = "&Betreff:";
            labelSubject.TextAlign = ContentAlignment.TopRight;
            // 
            // textBoxAuthor
            // 
            textBoxAuthor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxAuthor.Location = new Point(135, 79);
            textBoxAuthor.Name = "textBoxAuthor";
            textBoxAuthor.Size = new Size(441, 23);
            textBoxAuthor.TabIndex = 5;
            // 
            // labelAuthor
            // 
            labelAuthor.Location = new Point(19, 82);
            labelAuthor.Name = "labelAuthor";
            labelAuthor.Size = new Size(110, 15);
            labelAuthor.TabIndex = 4;
            labelAuthor.Text = "&Autor:";
            labelAuthor.TextAlign = ContentAlignment.TopRight;
            // 
            // textBoxTitle
            // 
            textBoxTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxTitle.Location = new Point(135, 50);
            textBoxTitle.Name = "textBoxTitle";
            textBoxTitle.Size = new Size(441, 23);
            textBoxTitle.TabIndex = 3;
            // 
            // labelTitle
            // 
            labelTitle.Location = new Point(19, 53);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(110, 15);
            labelTitle.TabIndex = 2;
            labelTitle.Text = "&Titel:";
            labelTitle.TextAlign = ContentAlignment.TopRight;
            // 
            // labelFileValue
            // 
            labelFileValue.AutoEllipsis = true;
            labelFileValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelFileValue.Location = new Point(135, 26);
            labelFileValue.Name = "labelFileValue";
            labelFileValue.Size = new Size(441, 15);
            labelFileValue.TabIndex = 1;
            labelFileValue.Text = "datei.pdf";
            // 
            // labelFile
            // 
            labelFile.Location = new Point(19, 26);
            labelFile.Name = "labelFile";
            labelFile.Size = new Size(110, 15);
            labelFile.TabIndex = 0;
            labelFile.Text = "Datei:";
            labelFile.TextAlign = ContentAlignment.TopRight;
            // 
            // tabSecurity
            // 
            tabSecurity.Controls.Add(groupPermissions);
            tabSecurity.Controls.Add(groupSecurity);
            tabSecurity.Location = new Point(4, 24);
            tabSecurity.Name = "tabSecurity";
            tabSecurity.Padding = new Padding(3);
            tabSecurity.Size = new Size(608, 470);
            tabSecurity.TabIndex = 1;
            tabSecurity.Text = "Sicherheit";
            tabSecurity.UseVisualStyleBackColor = true;
            // 
            // groupPermissions
            // 
            groupPermissions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupPermissions.Controls.Add(checkRemoveRestrictions);
            groupPermissions.Controls.Add(listPermissions);
            groupPermissions.Location = new Point(8, 272);
            groupPermissions.Name = "groupPermissions";
            groupPermissions.Size = new Size(592, 192);
            groupPermissions.TabIndex = 1;
            groupPermissions.TabStop = false;
            groupPermissions.Text = "Dokumenteinschränkungen – Zusammenfassung";
            // 
            // checkRemoveRestrictions
            // 
            checkRemoveRestrictions.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            checkRemoveRestrictions.AutoSize = true;
            checkRemoveRestrictions.Location = new Point(13, 167);
            checkRemoveRestrictions.Name = "checkRemoveRestrictions";
            checkRemoveRestrictions.Size = new Size(170, 19);
            checkRemoveRestrictions.TabIndex = 1;
            checkRemoveRestrictions.Text = "Einschränkungen &aufheben";
            checkRemoveRestrictions.UseVisualStyleBackColor = true;
            checkRemoveRestrictions.Visible = false;
            checkRemoveRestrictions.CheckedChanged += CheckRemoveRestrictions_CheckedChanged;
            // 
            // listPermissions
            // 
            listPermissions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listPermissions.BorderStyle = BorderStyle.None;
            listPermissions.Columns.AddRange(new ColumnHeader[] { columnPermission, columnPermissionValue });
            listPermissions.HeaderStyle = ColumnHeaderStyle.None;
            listPermissions.Location = new Point(10, 24);
            listPermissions.MultiSelect = false;
            listPermissions.Name = "listPermissions";
            listPermissions.Size = new Size(572, 137);
            listPermissions.TabIndex = 0;
            listPermissions.UseCompatibleStateImageBehavior = false;
            listPermissions.View = View.Details;
            // 
            // columnPermission
            // 
            columnPermission.Text = "Vorgang";
            columnPermission.Width = 300;
            // 
            // columnPermissionValue
            // 
            columnPermissionValue.Text = "Status";
            columnPermissionValue.Width = 240;
            // 
            // groupSecurity
            // 
            groupSecurity.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupSecurity.Controls.Add(labelPasswordHint);
            groupSecurity.Controls.Add(textPasswordRepeat);
            groupSecurity.Controls.Add(labelPasswordRepeat);
            groupSecurity.Controls.Add(textPassword);
            groupSecurity.Controls.Add(labelPassword);
            groupSecurity.Controls.Add(checkPassword);
            groupSecurity.Controls.Add(labelSecurityStatus);
            groupSecurity.Location = new Point(8, 8);
            groupSecurity.Name = "groupSecurity";
            groupSecurity.Size = new Size(592, 258);
            groupSecurity.TabIndex = 0;
            groupSecurity.TabStop = false;
            groupSecurity.Text = "Kennwortschutz";
            // 
            // labelPasswordHint
            // 
            labelPasswordHint.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelPasswordHint.ForeColor = SystemColors.GrayText;
            labelPasswordHint.Location = new Point(146, 150);
            labelPasswordHint.Name = "labelPasswordHint";
            labelPasswordHint.Size = new Size(436, 45);
            labelPasswordHint.TabIndex = 6;
            labelPasswordHint.Text = "Die Datei wird mit AES-256 (PDF 2.0) verschlüsselt.\r\nDas Kennwort wird künftig bei jedem Öffnen abgefragt.\r\nOhne Kennwort lässt sich die Datei nicht mehr anzeigen.";
            // 
            // textPasswordRepeat
            // 
            textPasswordRepeat.Location = new Point(146, 119);
            textPasswordRepeat.Name = "textPasswordRepeat";
            textPasswordRepeat.Size = new Size(353, 23);
            textPasswordRepeat.TabIndex = 5;
            textPasswordRepeat.UseSystemPasswordChar = true;
            // 
            // labelPasswordRepeat
            // 
            labelPasswordRepeat.Location = new Point(30, 122);
            labelPasswordRepeat.Name = "labelPasswordRepeat";
            labelPasswordRepeat.Size = new Size(110, 15);
            labelPasswordRepeat.TabIndex = 4;
            labelPasswordRepeat.Text = "&Wiederholen:";
            labelPasswordRepeat.TextAlign = ContentAlignment.TopRight;
            // 
            // textPassword
            // 
            textPassword.Location = new Point(146, 90);
            textPassword.Name = "textPassword";
            textPassword.Size = new Size(353, 23);
            textPassword.TabIndex = 3;
            textPassword.UseSystemPasswordChar = true;
            // 
            // labelPassword
            // 
            labelPassword.Location = new Point(30, 93);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(110, 15);
            labelPassword.TabIndex = 2;
            labelPassword.Text = "&Kennwort:";
            labelPassword.TextAlign = ContentAlignment.TopRight;
            // 
            // checkPassword
            // 
            checkPassword.AutoSize = true;
            checkPassword.Location = new Point(13, 62);
            checkPassword.Name = "checkPassword";
            checkPassword.Size = new Size(198, 19);
            checkPassword.TabIndex = 1;
            checkPassword.Text = "Kennwort zum &Öffnen verlangen";
            checkPassword.UseVisualStyleBackColor = true;
            checkPassword.CheckedChanged += CheckPassword_CheckedChanged;
            // 
            // labelSecurityStatus
            // 
            labelSecurityStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelSecurityStatus.Location = new Point(10, 24);
            labelSecurityStatus.Name = "labelSecurityStatus";
            labelSecurityStatus.Size = new Size(572, 34);
            labelSecurityStatus.TabIndex = 0;
            labelSecurityStatus.Text = "–";
            // 
            // tabFonts
            // 
            tabFonts.Controls.Add(groupFonts);
            tabFonts.Location = new Point(4, 24);
            tabFonts.Name = "tabFonts";
            tabFonts.Padding = new Padding(3);
            tabFonts.Size = new Size(608, 470);
            tabFonts.TabIndex = 2;
            tabFonts.Text = "Schriften";
            tabFonts.UseVisualStyleBackColor = true;
            // 
            // groupFonts
            // 
            groupFonts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupFonts.Controls.Add(treeFonts);
            groupFonts.Location = new Point(8, 8);
            groupFonts.Name = "groupFonts";
            groupFonts.Size = new Size(592, 452);
            groupFonts.TabIndex = 0;
            groupFonts.TabStop = false;
            groupFonts.Text = "In diesem Dokument verwendete Schriften";
            // 
            // treeFonts
            // 
            treeFonts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeFonts.ItemHeight = 22;
            treeFonts.Location = new Point(10, 24);
            treeFonts.Name = "treeFonts";
            treeFonts.ShowRootLines = false;
            treeFonts.Size = new Size(572, 418);
            treeFonts.TabIndex = 0;
            // 
            // tabAttachments
            // 
            tabAttachments.Controls.Add(labelAttachmentHint);
            tabAttachments.Controls.Add(buttonOpenAttachment);
            tabAttachments.Controls.Add(buttonSaveAttachment);
            tabAttachments.Controls.Add(listAttachments);
            tabAttachments.Location = new Point(4, 24);
            tabAttachments.Name = "tabAttachments";
            tabAttachments.Padding = new Padding(3);
            tabAttachments.Size = new Size(608, 470);
            tabAttachments.TabIndex = 3;
            tabAttachments.Text = "Anhänge";
            tabAttachments.UseVisualStyleBackColor = true;
            // 
            // labelAttachmentHint
            // 
            labelAttachmentHint.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelAttachmentHint.ForeColor = SystemColors.GrayText;
            labelAttachmentHint.Location = new Point(8, 438);
            labelAttachmentHint.Name = "labelAttachmentHint";
            labelAttachmentHint.Size = new Size(312, 15);
            labelAttachmentHint.TabIndex = 3;
            labelAttachmentHint.Text = "Programme und Skripte lassen sich nur speichern.";
            // 
            // buttonOpenAttachment
            // 
            buttonOpenAttachment.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOpenAttachment.Enabled = false;
            buttonOpenAttachment.Location = new Point(326, 432);
            buttonOpenAttachment.Name = "buttonOpenAttachment";
            buttonOpenAttachment.Size = new Size(108, 27);
            buttonOpenAttachment.TabIndex = 1;
            buttonOpenAttachment.Text = "Ö&ffnen";
            buttonOpenAttachment.UseVisualStyleBackColor = true;
            buttonOpenAttachment.Click += ButtonOpenAttachment_Click;
            // 
            // buttonSaveAttachment
            // 
            buttonSaveAttachment.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonSaveAttachment.Enabled = false;
            buttonSaveAttachment.Location = new Point(440, 432);
            buttonSaveAttachment.Name = "buttonSaveAttachment";
            buttonSaveAttachment.Size = new Size(160, 27);
            buttonSaveAttachment.TabIndex = 2;
            buttonSaveAttachment.Text = "Speichern &unter…";
            buttonSaveAttachment.UseVisualStyleBackColor = true;
            buttonSaveAttachment.Click += ButtonSaveAttachment_Click;
            // 
            // listAttachments
            // 
            listAttachments.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listAttachments.Columns.AddRange(new ColumnHeader[] { columnAttachmentName, columnAttachmentModified, columnAttachmentSize });
            listAttachments.ContextMenuStrip = contextMenuAttachments;
            listAttachments.FullRowSelect = true;
            listAttachments.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listAttachments.Location = new Point(8, 8);
            listAttachments.MultiSelect = false;
            listAttachments.Name = "listAttachments";
            listAttachments.OwnerDraw = true;
            listAttachments.Size = new Size(592, 416);
            listAttachments.TabIndex = 0;
            listAttachments.UseCompatibleStateImageBehavior = false;
            listAttachments.View = View.Details;
            listAttachments.SelectedIndexChanged += ListAttachments_SelectedIndexChanged;
            listAttachments.DoubleClick += ListAttachments_DoubleClick;
            // 
            // columnAttachmentName
            // 
            columnAttachmentName.Text = "Name";
            columnAttachmentName.Width = 300;
            // 
            // columnAttachmentModified
            // 
            columnAttachmentModified.Text = "Geändert am";
            columnAttachmentModified.Width = 140;
            // 
            // columnAttachmentSize
            // 
            columnAttachmentSize.Text = "Größe";
            columnAttachmentSize.TextAlign = HorizontalAlignment.Right;
            columnAttachmentSize.Width = 120;
            // 
            // contextMenuAttachments
            // 
            contextMenuAttachments.Items.AddRange(new ToolStripItem[] { mnuOpenAttachment, mnuSaveAttachment });
            contextMenuAttachments.Name = "contextMenuAttachments";
            contextMenuAttachments.Size = new Size(167, 48);
            contextMenuAttachments.Opening += ContextMenuAttachments_Opening;
            // 
            // mnuOpenAttachment
            // 
            mnuOpenAttachment.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            mnuOpenAttachment.Name = "mnuOpenAttachment";
            mnuOpenAttachment.Size = new Size(166, 22);
            mnuOpenAttachment.Text = "Ö&ffnen";
            mnuOpenAttachment.Click += ButtonOpenAttachment_Click;
            // 
            // mnuSaveAttachment
            // 
            mnuSaveAttachment.Name = "mnuSaveAttachment";
            mnuSaveAttachment.Size = new Size(166, 22);
            mnuSaveAttachment.Text = "Speichern &unter…";
            mnuSaveAttachment.Click += ButtonSaveAttachment_Click;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(428, 516);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(95, 27);
            buttonOK.TabIndex = 1;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(529, 516);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(95, 27);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // PropertiesForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(640, 555);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(tabControl);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PropertiesForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Dokumenteigenschaften";
            tabControl.ResumeLayout(false);
            tabDescription.ResumeLayout(false);
            groupAdvanced.ResumeLayout(false);
            groupAdvanced.PerformLayout();
            groupDescription.ResumeLayout(false);
            groupDescription.PerformLayout();
            tabSecurity.ResumeLayout(false);
            groupPermissions.ResumeLayout(false);
            groupPermissions.PerformLayout();
            groupSecurity.ResumeLayout(false);
            groupSecurity.PerformLayout();
            tabFonts.ResumeLayout(false);
            groupFonts.ResumeLayout(false);
            tabAttachments.ResumeLayout(false);
            contextMenuAttachments.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl;
        private TabPage tabDescription;
        private GroupBox groupDescription;
        private Label labelFile;
        private Label labelFileValue;
        private Label labelTitle;
        private TextBox textBoxTitle;
        private Label labelAuthor;
        private TextBox textBoxAuthor;
        private Label labelSubject;
        private TextBox textBoxSubject;
        private Label labelKeywords;
        private TextBox textBoxKeywords;
        private Label labelCreated;
        private Label labelCreatedValue;
        private Label labelModified;
        private Label labelModifiedValue;
        private Label labelCreator;
        private Label labelCreatorValue;
        private Button buttonRemove;
        private GroupBox groupAdvanced;
        private Label labelProducer;
        private Label labelProducerValue;
        private Label labelVersion;
        private Label labelVersionValue;
        private Label labelLocation;
        private Label labelLocationValue;
        private Label labelSize;
        private Label labelSizeValue;
        private Label labelFormat;
        private Label labelFormatValue;
        private Label labelPages;
        private Label labelPagesValue;
        private Label labelTagged;
        private Label labelTaggedValue;
        private Label labelFastWeb;
        private Label labelFastWebValue;
        private TabPage tabSecurity;
        private GroupBox groupSecurity;
        private Label labelSecurityStatus;
        private CheckBox checkPassword;
        private Label labelPassword;
        private TextBox textPassword;
        private Label labelPasswordRepeat;
        private TextBox textPasswordRepeat;
        private Label labelPasswordHint;
        private CheckBox checkRemoveRestrictions;
        private ToolTip toolTipReveal;
        private GroupBox groupPermissions;
        private ListView listPermissions;
        private ColumnHeader columnPermission;
        private ColumnHeader columnPermissionValue;
        private TabPage tabFonts;
        private GroupBox groupFonts;
        private TreeView treeFonts;
        private TabPage tabAttachments;
        private PDFLight.Controls.StaticHeaderListView listAttachments;
        private ColumnHeader columnAttachmentName;
        private ColumnHeader columnAttachmentModified;
        private ColumnHeader columnAttachmentSize;
        private Button buttonSaveAttachment;
        private Label labelAttachmentHint;
        private Button buttonOK;
        private Button buttonCancel;
        private SaveFileDialog saveAttachmentDialog;
        private Button buttonOpenAttachment;
        private ContextMenuStrip contextMenuAttachments;
        private ToolStripMenuItem mnuOpenAttachment;
        private ToolStripMenuItem mnuSaveAttachment;
    }
}
