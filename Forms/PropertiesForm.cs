using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Dokumenteigenschaften der aktuellen PDF-Datei nach dem Vorbild von Acrobat: Beschreibung (Metadaten bearbeiten, technische
/// Angaben), Sicherheit (Verschlüsselung, Berechtigungen), Schriften und Anhänge (eingebettete Dateien speichern).</summary>
internal partial class PropertiesForm : Form
{
    private readonly PdfInfo original;
    private readonly Func<int, Task<byte[]>>? loadAttachment;

    /// <summary>True, wenn „Metadaten entfernen“ gedrückt wurde: Beim Speichern werden alle Metadaten entfernt,
    /// auch die unsichtbaren (Anwendung, Produzent, Datumsangaben, XMP).</summary>
    public bool RemoveRequested { get; private set; }

    public string DocTitle => textBoxTitle.Text.Trim();
    public string DocAuthor => textBoxAuthor.Text.Trim();
    public string DocSubject => textBoxSubject.Text.Trim();
    public string DocKeywords => textBoxKeywords.Text.Trim();

    /// <summary>True, wenn der Benutzer mindestens ein Metadatum geändert hat.</summary>
    public bool InfoChanged =>
        DocTitle != (original.Title ?? string.Empty).Trim() ||
        DocAuthor != (original.Author ?? string.Empty).Trim() ||
        DocSubject != (original.Subject ?? string.Empty).Trim() ||
        DocKeywords != (original.Keywords ?? string.Empty).Trim();

    /// <param name="info">Metadaten, Seitenformat und Schriften (PDFsharp).</param>
    /// <param name="file">Die angezeigte Datei.</param>
    /// <param name="status">Version aus der Kopfzeile und PDF/A-Stufe, wie in der Statusleiste.</param>
    /// <param name="readOnly">Nur anzeigen (PDF/A ohne Bearbeitungsfreigabe, verschlüsselte Datei).</param>
    /// <param name="security">Berechtigungsbits und Revision des Sicherheits-Handlers aus PDFium; Revision -1 = unverschlüsselt.</param>
    /// <param name="openPassword">Die Datei verlangt ein Kennwort zum Öffnen.</param>
    /// <param name="attachments">Eingebettete Dateien (Name, Größe in Byte, -1 = unbekannt).</param>
    /// <param name="loadAttachment">Liest den Inhalt einer eingebetteten Datei über ihre Nummer.</param>
    public PropertiesForm(PdfInfo info, FileInfo file, PdfStatus? status, bool readOnly, (uint Permissions, int Revision) security, bool openPassword,
        IReadOnlyList<(string Name, long Size)> attachments, Func<int, Task<byte[]>>? loadAttachment)
    {
        InitializeComponent();
        Lng.Apply(this);
        TextBoxMargins.Apply(this);
        original = info;
        this.loadAttachment = loadAttachment;
        FillDescription(info, file, status, readOnly);
        FillSecurity(security, openPassword);
        FillFonts(info.Fonts ?? []);
        FillAttachments(attachments);
    }

    // ==== Beschreibung ====================================================================================================================

    private void FillDescription(PdfInfo info, FileInfo file, PdfStatus? status, bool readOnly)
    {
        // Bei schreibgeschützten PDF/A-Dateien und verschlüsselten Dateien nur anzeigen, nicht bearbeiten
        textBoxTitle.ReadOnly = textBoxAuthor.ReadOnly = textBoxSubject.ReadOnly = textBoxKeywords.ReadOnly = readOnly;
        buttonRemove.Enabled = !readOnly;
        if (readOnly)
        {
            buttonOK.Visible = false;
            buttonCancel.Text = Lng.T("Schließen");
        }
        textBoxTitle.Text = info.Title;
        textBoxAuthor.Text = info.Author;
        textBoxSubject.Text = info.Subject;
        textBoxKeywords.Text = info.Keywords;
        labelFileValue.Text = file.Name;
        labelCreatedValue.Text = info.Created?.ToString("G") ?? "–";
        labelModifiedValue.Text = info.Modified?.ToString("G") ?? "–";
        labelCreatorValue.Text = OrDash(info.Creator);
        labelProducerValue.Text = OrDash(info.Producer);

        var version = status?.Version ?? info.Version;
        var acrobat = version switch { "1.0" => "1.x", "1.1" => "2.x", "1.2" => "3.x", "1.3" => "4.x", "1.4" => "5.x", "1.5" => "6.x", "1.6" => "7.x", "1.7" => "8.x", _ => null };
        var versionText = acrobat != null ? $"{version} (Acrobat {acrobat})" : version;
        if (status?.PdfALevel is { } pdfA) { versionText += ", PDF/A-" + pdfA; }
        labelVersionValue.Text = versionText;

        labelLocationValue.Text = file.DirectoryName ?? "–";
        labelSizeValue.Text = file.Length < 1024 ? FormatSize(file.Length) : $"{FormatSize(file.Length)} ({file.Length:N0} Byte)";
        if (info.PageWidthPt > 0 && info.PageHeightPt > 0)
        {
            // „210 × 297 mm (DIN A4, Hochformat)“; ohne bekanntes Format nennt Describe selbst die Maße
            const double mmPerPt = 25.4 / 72;
            var size = $"{info.PageWidthPt * mmPerPt:0} × {info.PageHeightPt * mmPerPt:0} mm";
            var format = PaperFormat.Describe(info.PageWidthPt, info.PageHeightPt);
            labelFormatValue.Text = format.StartsWith(size, StringComparison.Ordinal) ? format : $"{size} ({format})";
        }
        labelPagesValue.Text = info.PageCount.ToString("N0");
        labelTaggedValue.Text = Lng.T(info.Tagged ? "Ja" : "Nein");
        labelFastWebValue.Text = Lng.T(IsLinearized(file) ? "Ja" : "Nein");
    }

    private static string OrDash(string? text) => string.IsNullOrWhiteSpace(text) ? "–" : text.Trim();

    /// <summary>„Schnelle Webanzeige“: Eine linearisierte Datei beginnt mit einem Wörterbuch, das /Linearized enthält.</summary>
    private static bool IsLinearized(FileInfo file)
    {
        try
        {
            using var stream = file.OpenRead();
            var buffer = new byte[1024];
            var read = stream.ReadAtLeast(buffer, buffer.Length, throwOnEndOfStream: false);
            return buffer.AsSpan(0, read).IndexOf("/Linearized"u8) >= 0;
        }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }

    internal static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes:N0} Byte",
        < 1024 * 1024 => $"{bytes / 1024.0:N2} KB",
        < 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024):N2} MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):N2} GB",
    };

    /// <summary>Leert die Felder und merkt sich, dass auch die unsichtbaren Metadaten weg sollen; gespeichert wird erst mit „Speichern“.</summary>
    private void ButtonRemove_Click(object? sender, EventArgs e)
    {
        RemoveRequested = true;
        textBoxTitle.Text = textBoxAuthor.Text = textBoxSubject.Text = textBoxKeywords.Text = string.Empty;
        labelCreatedValue.Text = labelModifiedValue.Text = labelCreatorValue.Text = labelProducerValue.Text = Lng.T("wird entfernt");        buttonRemove.Enabled = false;
        buttonOK.Focus();
    }

    // ==== Sicherheit ======================================================================================================================

    private void FillSecurity((uint Permissions, int Revision) security, bool openPassword)
    {
        var (bits, revision) = security;
        var encrypted = revision >= 0;
        labelSecurityMethodValue.Text = Lng.T(encrypted ? "Kennwortsicherheit" : "Keine Sicherheit");
        labelEncryptionValue.Text = !encrypted ? Lng.T("Keine") : revision switch
        {
            2 => "RC4, 40 Bit",
            3 => "RC4, 128 Bit",
            4 => Lng.T("128 Bit (RC4 oder AES)"),
            5 or 6 => "AES, 256 Bit",
            _ => "–",
        };
        labelOpenPasswordValue.Text = Lng.T(encrypted && openPassword ? "Ja" : "Nein");

        // Bitnummern nach der PDF-Norm (Tabelle 22), 1-basiert; Revision 2 kennt nur die Bits 3–6, die übrigen folgen dort aus ihnen
        bool Bit(int n) => !encrypted || (bits & (1u << (n - 1))) != 0;
        var modern = revision >= 3;
        var print = Bit(3);
        var rows = new (string Action, string State)[]
        {
            (Lng.T("Drucken"), !print ? Lng.T("Nicht zulässig") : modern && !Bit(12) ? Lng.T("Nur niedrige Auflösung") : Lng.T("Zulässig")),
            (Lng.T("Ändern des Dokuments"), Allowed(Bit(4))),
            (Lng.T("Dokumentzusammenstellung"), Allowed(modern ? Bit(11) : Bit(4))),
            (Lng.T("Kopieren von Inhalt"), Allowed(Bit(5))),
            (Lng.T("Kopieren von Inhalt für Barrierefreiheit"), Allowed(modern ? Bit(10) : Bit(5))),
            (Lng.T("Kommentieren"), Allowed(Bit(6))),
            (Lng.T("Ausfüllen von Formularfeldern"), Allowed(modern ? Bit(9) || Bit(6) : Bit(6))),
        };
        listPermissions.BeginUpdate();
        foreach (var (action, state) in rows) { listPermissions.Items.Add(new ListViewItem([action, state])); }
        listPermissions.EndUpdate();
    }

    private static string Allowed(bool allowed) => Lng.T(allowed ? "Zulässig" : "Nicht zulässig");

    // ==== Schriften =======================================================================================================================

    private void FillFonts(IReadOnlyList<FontInfo> fonts)
    {
        treeFonts.BeginUpdate();
        if (fonts.Count == 0) { treeFonts.Nodes.Add(Lng.T("Keine Schriften gefunden")); }
        foreach (var font in fonts)
        {
            var embedding = Lng.T(font.Subset ? "Eingebettete Untergruppe" : font.Embedded ? "Eingebettet" : "Nicht eingebettet");
            var node = treeFonts.Nodes.Add($"{font.Name} ({embedding})");
            node.Nodes.Add(Lng.T("Typ:") + " " + font.Type);
            var encoding = font.Encoding switch { "*" => Lng.T("Benutzerdefiniert"), "" => Lng.T("Integriert"), var other => other };
            node.Nodes.Add(Lng.T("Kodierung:") + " " + encoding);
        }
        treeFonts.ExpandAll();
        if (treeFonts.Nodes.Count > 0) { treeFonts.Nodes[0].EnsureVisible(); }
        treeFonts.EndUpdate();
    }

    // ==== Anhänge =========================================================================================================================

    private void FillAttachments(IReadOnlyList<(string Name, long Size)> attachments)
    {
        if (attachments.Count == 0 || loadAttachment == null)
        {
            tabControl.TabPages.Remove(tabAttachments);
            return;
        }
        listAttachments.BeginUpdate();
        for (var i = 0; i < attachments.Count; i++)
        {
            var (name, size) = attachments[i];
            listAttachments.Items.Add(new ListViewItem([name.Length > 0 ? name : "?", size >= 0 ? FormatSize(size) : "–"]) { Tag = i });
        }
        listAttachments.EndUpdate();
        tabAttachments.Text += $" ({attachments.Count})";
    }

    private void ListAttachments_SelectedIndexChanged(object? sender, EventArgs e) => buttonSaveAttachment.Enabled = listAttachments.SelectedItems.Count == 1;

    private void ListAttachments_DoubleClick(object? sender, EventArgs e) => SaveSelectedAttachment();

    private void ButtonSaveAttachment_Click(object? sender, EventArgs e) => SaveSelectedAttachment();

    /// <summary>Speichert die gewählte eingebettete Datei. Geöffnet wird sie bewusst nicht – sie könnte ausführbar sein.</summary>
    private async void SaveSelectedAttachment()
    {
        if (loadAttachment == null || listAttachments.SelectedItems.Count != 1 || listAttachments.SelectedItems[0].Tag is not int index) { return; }
        var name = listAttachments.SelectedItems[0].Text;
        saveAttachmentDialog.Title = Lng.T("Eingebettete Datei speichern");
        saveAttachmentDialog.Filter = Lng.T("Alle Dateien (*.*)|*.*");
        saveAttachmentDialog.FileName = SafeFileName(name);
        if (saveAttachmentDialog.ShowDialog(this) != DialogResult.OK) { return; }
        var target = saveAttachmentDialog.FileName;
        UseWaitCursor = true;
        buttonSaveAttachment.Enabled = false;
        try
        {
            var bytes = await loadAttachment(index);
            await File.WriteAllBytesAsync(target, bytes);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            TaskDlg.ErrTaskDlg(Handle, Lng.T("Die eingebettete Datei konnte nicht gespeichert werden."), ex);
        }
        finally
        {
            if (!IsDisposed)
            {
                UseWaitCursor = false;
                buttonSaveAttachment.Enabled = listAttachments.SelectedItems.Count == 1;
            }
        }
    }

    /// <summary>Nur der Dateiname, ohne Pfadanteile und unzulässige Zeichen – der Name in der PDF stammt vom Ersteller der Datei.</summary>
    private static string SafeFileName(string name)
    {
        var fileName = name.Replace('/', '\\');
        fileName = fileName[(fileName.LastIndexOf('\\') + 1)..];
        foreach (var c in Path.GetInvalidFileNameChars()) { fileName = fileName.Replace(c, '_'); }
        fileName = fileName.Trim().TrimEnd('.');
        return fileName.Length > 0 ? fileName : "attachment";
    }
}
