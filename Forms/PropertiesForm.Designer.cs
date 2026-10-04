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
            listPermissions = new ListView();
            columnPermission = new ColumnHeader();
            columnPermissionValue = new ColumnHeader();
            groupSecurity = new GroupBox();
            labelOpenPasswordValue = new Label();
            labelOpenPassword = new Label();
            labelEncryptionValue = new Label();
            labelEncryption = new Label();
            labelSecurityMethodValue = new Label();
            labelSecurityMethod = new Label();
            tabFonts = new TabPage();
            groupFonts = new GroupBox();
            treeFonts = new TreeView();
            tabAttachments = new TabPage();
            labelAttachmentHint = new Label();
            buttonSaveAttachment = new Button();
            listAttachments = new ListView();
            columnAttachmentName = new ColumnHeader();
            columnAttachmentSize = new ColumnHeader();
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
            labelFastWebValue.Location = new Point(156, 168);
            labelFastWebValue.Name = "labelFastWebValue";
            labelFastWebValue.Size = new Size(12, 15);
            labelFastWebValue.TabIndex = 15;
            labelFastWebValue.Text = "–";
            //
            // labelFastWeb
            //
            labelFastWeb.Location = new Point(10, 168);
            labelFastWeb.Name = "labelFastWeb";
            labelFastWeb.Size = new Size(140, 15);
            labelFastWeb.TabIndex = 14;
            labelFastWeb.Text = "Schnelle Webanzeige:";
            labelFastWeb.TextAlign = ContentAlignment.TopRight;
            //
            // labelTaggedValue
            //
            labelTaggedValue.AutoSize = true;
            labelTaggedValue.Location = new Point(498, 144);
            labelTaggedValue.Name = "labelTaggedValue";
            labelTaggedValue.Size = new Size(12, 15);
            labelTaggedValue.TabIndex = 13;
            labelTaggedValue.Text = "–";
            //
            // labelTagged
            //
            labelTagged.Location = new Point(316, 144);
            labelTagged.Name = "labelTagged";
            labelTagged.Size = new Size(176, 15);
            labelTagged.TabIndex = 12;
            labelTagged.Text = "PDF mit Tags:";
            labelTagged.TextAlign = ContentAlignment.TopRight;
            //
            // labelPagesValue
            //
            labelPagesValue.AutoSize = true;
            labelPagesValue.Location = new Point(156, 144);
            labelPagesValue.Name = "labelPagesValue";
            labelPagesValue.Size = new Size(12, 15);
            labelPagesValue.TabIndex = 11;
            labelPagesValue.Text = "–";
            //
            // labelPages
            //
            labelPages.Location = new Point(10, 144);
            labelPages.Name = "labelPages";
            labelPages.Size = new Size(140, 15);
            labelPages.TabIndex = 10;
            labelPages.Text = "Seitenanzahl:";
            labelPages.TextAlign = ContentAlignment.TopRight;
            //
            // labelFormatValue
            //
            labelFormatValue.AutoEllipsis = true;
            labelFormatValue.Location = new Point(156, 120);
            labelFormatValue.Name = "labelFormatValue";
            labelFormatValue.Size = new Size(420, 15);
            labelFormatValue.TabIndex = 9;
            labelFormatValue.Text = "–";
            //
            // labelFormat
            //
            labelFormat.Location = new Point(10, 120);
            labelFormat.Name = "labelFormat";
            labelFormat.Size = new Size(140, 15);
            labelFormat.TabIndex = 8;
            labelFormat.Text = "Seitenformat:";
            labelFormat.TextAlign = ContentAlignment.TopRight;
            //
            // labelSizeValue
            //
            labelSizeValue.AutoEllipsis = true;
            labelSizeValue.Location = new Point(156, 96);
            labelSizeValue.Name = "labelSizeValue";
            labelSizeValue.Size = new Size(420, 15);
            labelSizeValue.TabIndex = 7;
            labelSizeValue.Text = "–";
            //
            // labelSize
            //
            labelSize.Location = new Point(10, 96);
            labelSize.Name = "labelSize";
            labelSize.Size = new Size(140, 15);
            labelSize.TabIndex = 6;
            labelSize.Text = "Dateigröße:";
            labelSize.TextAlign = ContentAlignment.TopRight;
            //
            // labelLocationValue
            //
            labelLocationValue.AutoEllipsis = true;
            labelLocationValue.Location = new Point(156, 72);
            labelLocationValue.Name = "labelLocationValue";
            labelLocationValue.Size = new Size(420, 15);
            labelLocationValue.TabIndex = 5;
            labelLocationValue.Text = "–";
            //
            // labelLocation
            //
            labelLocation.Location = new Point(10, 72);
            labelLocation.Name = "labelLocation";
            labelLocation.Size = new Size(140, 15);
            labelLocation.TabIndex = 4;
            labelLocation.Text = "Speicherort:";
            labelLocation.TextAlign = ContentAlignment.TopRight;
            //
            // labelVersionValue
            //
            labelVersionValue.AutoEllipsis = true;
            labelVersionValue.Location = new Point(156, 48);
            labelVersionValue.Name = "labelVersionValue";
            labelVersionValue.Size = new Size(420, 15);
            labelVersionValue.TabIndex = 3;
            labelVersionValue.Text = "–";
            //
            // labelVersion
            //
            labelVersion.Location = new Point(10, 48);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new Size(140, 15);
            labelVersion.TabIndex = 2;
            labelVersion.Text = "PDF-Version:";
            labelVersion.TextAlign = ContentAlignment.TopRight;
            //
            // labelProducerValue
            //
            labelProducerValue.AutoEllipsis = true;
            labelProducerValue.Location = new Point(156, 24);
            labelProducerValue.Name = "labelProducerValue";
            labelProducerValue.Size = new Size(420, 15);
            labelProducerValue.TabIndex = 1;
            labelProducerValue.Text = "–";
            //
            // labelProducer
            //
            labelProducer.Location = new Point(10, 24);
            labelProducer.Name = "labelProducer";
            labelProducer.Size = new Size(140, 15);
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
            labelCreatorValue.Location = new Point(126, 226);
            labelCreatorValue.Name = "labelCreatorValue";
            labelCreatorValue.Size = new Size(284, 15);
            labelCreatorValue.TabIndex = 15;
            labelCreatorValue.Text = "–";
            //
            // labelCreator
            //
            labelCreator.Location = new Point(10, 226);
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
            labelModifiedValue.Size = new Size(12, 15);
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
            labelCreatedValue.Location = new Point(126, 202);
            labelCreatedValue.Name = "labelCreatedValue";
            labelCreatedValue.Size = new Size(12, 15);
            labelCreatedValue.TabIndex = 11;
            labelCreatedValue.Text = "–";
            //
            // labelCreated
            //
            labelCreated.Location = new Point(10, 202);
            labelCreated.Name = "labelCreated";
            labelCreated.Size = new Size(110, 15);
            labelCreated.TabIndex = 10;
            labelCreated.Text = "Erstellt am:";
            labelCreated.TextAlign = ContentAlignment.TopRight;
            //
            // textBoxKeywords
            //
            textBoxKeywords.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxKeywords.Location = new Point(126, 137);
            textBoxKeywords.Multiline = true;
            textBoxKeywords.Name = "textBoxKeywords";
            textBoxKeywords.ScrollBars = ScrollBars.Vertical;
            textBoxKeywords.Size = new Size(450, 54);
            textBoxKeywords.TabIndex = 9;
            //
            // labelKeywords
            //
            labelKeywords.Location = new Point(10, 140);
            labelKeywords.Name = "labelKeywords";
            labelKeywords.Size = new Size(110, 15);
            labelKeywords.TabIndex = 8;
            labelKeywords.Text = "&Stichwörter:";
            labelKeywords.TextAlign = ContentAlignment.TopRight;
            //
            // textBoxSubject
            //
            textBoxSubject.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxSubject.Location = new Point(126, 108);
            textBoxSubject.Name = "textBoxSubject";
            textBoxSubject.Size = new Size(450, 23);
            textBoxSubject.TabIndex = 7;
            //
            // labelSubject
            //
            labelSubject.Location = new Point(10, 111);
            labelSubject.Name = "labelSubject";
            labelSubject.Size = new Size(110, 15);
            labelSubject.TabIndex = 6;
            labelSubject.Text = "&Betreff:";
            labelSubject.TextAlign = ContentAlignment.TopRight;
            //
            // textBoxAuthor
            //
            textBoxAuthor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxAuthor.Location = new Point(126, 79);
            textBoxAuthor.Name = "textBoxAuthor";
            textBoxAuthor.Size = new Size(450, 23);
            textBoxAuthor.TabIndex = 5;
            //
            // labelAuthor
            //
            labelAuthor.Location = new Point(10, 82);
            labelAuthor.Name = "labelAuthor";
            labelAuthor.Size = new Size(110, 15);
            labelAuthor.TabIndex = 4;
            labelAuthor.Text = "&Autor:";
            labelAuthor.TextAlign = ContentAlignment.TopRight;
            //
            // textBoxTitle
            //
            textBoxTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxTitle.Location = new Point(126, 50);
            textBoxTitle.Name = "textBoxTitle";
            textBoxTitle.Size = new Size(450, 23);
            textBoxTitle.TabIndex = 3;
            //
            // labelTitle
            //
            labelTitle.Location = new Point(10, 53);
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
            labelFileValue.Location = new Point(126, 26);
            labelFileValue.Name = "labelFileValue";
            labelFileValue.Size = new Size(450, 15);
            labelFileValue.TabIndex = 1;
            labelFileValue.Text = "datei.pdf";
            //
            // labelFile
            //
            labelFile.Location = new Point(10, 26);
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
            groupPermissions.Controls.Add(listPermissions);
            groupPermissions.Location = new Point(8, 116);
            groupPermissions.Name = "groupPermissions";
            groupPermissions.Size = new Size(592, 344);
            groupPermissions.TabIndex = 1;
            groupPermissions.TabStop = false;
            groupPermissions.Text = "Dokumenteinschränkungen – Zusammenfassung";
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
            listPermissions.Size = new Size(572, 310);
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
            groupSecurity.Controls.Add(labelOpenPasswordValue);
            groupSecurity.Controls.Add(labelOpenPassword);
            groupSecurity.Controls.Add(labelEncryptionValue);
            groupSecurity.Controls.Add(labelEncryption);
            groupSecurity.Controls.Add(labelSecurityMethodValue);
            groupSecurity.Controls.Add(labelSecurityMethod);
            groupSecurity.Location = new Point(8, 8);
            groupSecurity.Name = "groupSecurity";
            groupSecurity.Size = new Size(592, 100);
            groupSecurity.TabIndex = 0;
            groupSecurity.TabStop = false;
            groupSecurity.Text = "Dokumentsicherheit";
            //
            // labelOpenPasswordValue
            //
            labelOpenPasswordValue.AutoSize = true;
            labelOpenPasswordValue.Location = new Point(186, 72);
            labelOpenPasswordValue.Name = "labelOpenPasswordValue";
            labelOpenPasswordValue.Size = new Size(12, 15);
            labelOpenPasswordValue.TabIndex = 5;
            labelOpenPasswordValue.Text = "–";
            //
            // labelOpenPassword
            //
            labelOpenPassword.Location = new Point(10, 72);
            labelOpenPassword.Name = "labelOpenPassword";
            labelOpenPassword.Size = new Size(170, 15);
            labelOpenPassword.TabIndex = 4;
            labelOpenPassword.Text = "Kennwort zum Öffnen:";
            labelOpenPassword.TextAlign = ContentAlignment.TopRight;
            //
            // labelEncryptionValue
            //
            labelEncryptionValue.AutoSize = true;
            labelEncryptionValue.Location = new Point(186, 48);
            labelEncryptionValue.Name = "labelEncryptionValue";
            labelEncryptionValue.Size = new Size(12, 15);
            labelEncryptionValue.TabIndex = 3;
            labelEncryptionValue.Text = "–";
            //
            // labelEncryption
            //
            labelEncryption.Location = new Point(10, 48);
            labelEncryption.Name = "labelEncryption";
            labelEncryption.Size = new Size(170, 15);
            labelEncryption.TabIndex = 2;
            labelEncryption.Text = "Verschlüsselung:";
            labelEncryption.TextAlign = ContentAlignment.TopRight;
            //
            // labelSecurityMethodValue
            //
            labelSecurityMethodValue.AutoSize = true;
            labelSecurityMethodValue.Location = new Point(186, 24);
            labelSecurityMethodValue.Name = "labelSecurityMethodValue";
            labelSecurityMethodValue.Size = new Size(12, 15);
            labelSecurityMethodValue.TabIndex = 1;
            labelSecurityMethodValue.Text = "–";
            //
            // labelSecurityMethod
            //
            labelSecurityMethod.Location = new Point(10, 24);
            labelSecurityMethod.Name = "labelSecurityMethod";
            labelSecurityMethod.Size = new Size(170, 15);
            labelSecurityMethod.TabIndex = 0;
            labelSecurityMethod.Text = "Sicherheitssystem:";
            labelSecurityMethod.TextAlign = ContentAlignment.TopRight;
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
            labelAttachmentHint.Size = new Size(420, 15);
            labelAttachmentHint.TabIndex = 2;
            labelAttachmentHint.Text = "Eingebettete Dateien werden nur gespeichert, nicht geöffnet.";
            //
            // buttonSaveAttachment
            //
            buttonSaveAttachment.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonSaveAttachment.Enabled = false;
            buttonSaveAttachment.Location = new Point(440, 432);
            buttonSaveAttachment.Name = "buttonSaveAttachment";
            buttonSaveAttachment.Size = new Size(160, 27);
            buttonSaveAttachment.TabIndex = 1;
            buttonSaveAttachment.Text = "Speichern &unter…";
            buttonSaveAttachment.UseVisualStyleBackColor = true;
            buttonSaveAttachment.Click += ButtonSaveAttachment_Click;
            //
            // listAttachments
            //
            listAttachments.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listAttachments.Columns.AddRange(new ColumnHeader[] { columnAttachmentName, columnAttachmentSize });
            listAttachments.FullRowSelect = true;
            listAttachments.Location = new Point(8, 8);
            listAttachments.MultiSelect = false;
            listAttachments.Name = "listAttachments";
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
            columnAttachmentName.Width = 440;
            //
            // columnAttachmentSize
            //
            columnAttachmentSize.Text = "Größe";
            columnAttachmentSize.TextAlign = HorizontalAlignment.Right;
            columnAttachmentSize.Width = 120;
            //
            // buttonOK
            //
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(432, 520);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(95, 27);
            buttonOK.TabIndex = 1;
            buttonOK.Text = "Speichern";
            buttonOK.UseVisualStyleBackColor = true;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(533, 520);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(95, 27);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // saveAttachmentDialog
            //
            saveAttachmentDialog.OverwritePrompt = true;
            //
            // PropertiesForm
            //
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(640, 559);
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
            groupSecurity.ResumeLayout(false);
            groupSecurity.PerformLayout();
            tabFonts.ResumeLayout(false);
            groupFonts.ResumeLayout(false);
            tabAttachments.ResumeLayout(false);
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
        private Label labelSecurityMethod;
        private Label labelSecurityMethodValue;
        private Label labelEncryption;
        private Label labelEncryptionValue;
        private Label labelOpenPassword;
        private Label labelOpenPasswordValue;
        private GroupBox groupPermissions;
        private ListView listPermissions;
        private ColumnHeader columnPermission;
        private ColumnHeader columnPermissionValue;
        private TabPage tabFonts;
        private GroupBox groupFonts;
        private TreeView treeFonts;
        private TabPage tabAttachments;
        private ListView listAttachments;
        private ColumnHeader columnAttachmentName;
        private ColumnHeader columnAttachmentSize;
        private Button buttonSaveAttachment;
        private Label labelAttachmentHint;
        private Button buttonOK;
        private Button buttonCancel;
        private SaveFileDialog saveAttachmentDialog;
    }
}
