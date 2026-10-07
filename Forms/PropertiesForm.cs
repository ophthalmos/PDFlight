using System.Diagnostics;
using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Dokumenteigenschaften der aktuellen PDF-Datei nach dem Vorbild von Acrobat: Beschreibung (Metadaten bearbeiten, technische
/// Angaben), Sicherheit (Verschlüsselung, Berechtigungen), Schriften und Anhänge (eingebettete Dateien öffnen und speichern).</summary>
internal partial class PropertiesForm : Form
{
    private readonly PdfInfo original;
    private readonly Func<int, Task<byte[]>>? loadAttachment;
    private readonly bool pdfALocked;   // PDF/A ohne Bearbeitungsfreigabe: nichts ändern, auch nicht verschlüsseln (PDF/A verbietet es)
    private readonly bool encrypted;    // die Datei ist verschlüsselt (Kennwort zum Öffnen oder nur Einschränkungen)
    private readonly bool openPassword; // … und verlangt ein Kennwort zum Öffnen

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

    /// <summary>Was mit dem Kennwortschutz geschehen soll: beibehalten, neu setzen (<see cref="NewPassword"/>) oder entfernen.</summary>
    public PasswordChange PasswordAction
    {
        get
        {
            if (!checkPassword.Enabled) { return PasswordChange.Keep; }
            // eine Eingabe in einem der beiden Felder gilt als Änderung – sonst ginge ein nur ins Wiederholfeld getipptes Kennwort still verloren
            // (Review 06.10.2026); ButtonOK_Click prüft dann beide Felder
            if (openPassword) { return !checkPassword.Checked ? PasswordChange.Remove : textPassword.Text.Length + textPasswordRepeat.Text.Length > 0 ? PasswordChange.Set : PasswordChange.Keep; }
            return checkPassword.Checked ? PasswordChange.Set : PasswordChange.Keep;
        }
    }

    public string NewPassword => textPassword.Text;

    /// <summary>Einschränkungen einer Datei, die sich ohne Kennwort öffnen lässt, aufheben (auch Folge eines neuen Kennworts). Über das Feld,
    /// nicht über Visible – nach dem Schließen des Dialogs meldet jedes Steuerelement Visible = false.</summary>
    public bool RemoveRestrictions => RestrictionsOnly && checkRemoveRestrictions.Checked;

    /// <summary>Verschlüsselt, aber ohne Kennwort zum Öffnen: nur Einschränkungen.</summary>
    private bool RestrictionsOnly => encrypted && !openPassword;

    /// <summary>Gibt es bei „OK“ etwas zu schreiben?</summary>
    public bool HasChanges => InfoChanged || RemoveRequested || PasswordAction != PasswordChange.Keep || RemoveRestrictions;

    /// <param name="info">Metadaten, Seitenformat und Schriften (PDFsharp).</param>
    /// <param name="file">Die angezeigte Datei.</param>
    /// <param name="status">Version aus der Kopfzeile und PDF/A-Stufe, wie in der Statusleiste.</param>
    /// <param name="pdfALocked">PDF/A ohne Bearbeitungsfreigabe: nur anzeigen.</param>
    /// <param name="security">Berechtigungsbits und Revision des Sicherheits-Handlers aus PDFium; Revision -1 = unverschlüsselt.</param>
    /// <param name="openPassword">Die Datei verlangt ein Kennwort zum Öffnen.</param>
    /// <param name="attachments">Eingebettete Dateien (Name, Größe in Byte, -1 = unbekannt).</param>
    /// <param name="loadAttachment">Liest den Inhalt einer eingebetteten Datei über ihre Nummer.</param>
    public PropertiesForm(PdfInfo info, FileInfo file, PdfStatus? status, bool pdfALocked, (uint Permissions, int Revision) security, bool openPassword,
        IReadOnlyList<(string Name, long Size, DateTime? Modified)> attachments, Func<int, Task<byte[]>>? loadAttachment)
    {
        InitializeComponent();
        Lng.Apply(this);
        Lng.Apply(contextMenuAttachments); // Kontextmenüs hängen nicht im Control-Baum
        TextBoxMargins.Apply(this);
        PasswordReveal.Attach(textPassword, toolTipReveal); // Augensymbol: Eingabe sichtbar machen
        PasswordReveal.Attach(textPasswordRepeat, toolTipReveal);
        original = info;
        this.loadAttachment = loadAttachment;
        this.pdfALocked = pdfALocked;
        encrypted = security.Revision >= 0;
        this.openPassword = encrypted && openPassword;
        FillDescription(info, file, status);
        FillSecurity(security);
        FillFonts(info.Fonts ?? []);
        FillAttachments(attachments);
        UpdateEditing();
    }

    /// <summary>Was gerade bearbeitbar ist. Metadaten: nicht bei gesperrter PDF/A; bei einer Datei mit bloßen Einschränkungen erst,
    /// wenn sie aufgehoben werden – ihr Besitzerkennwort ist unbekannt, nach dem Schreiben wären die Einschränkungen ohnehin weg.
    /// Bei einer Datei mit Kennwort zum Öffnen schreibt PDFlight entschlüsselt und verschlüsselt mit demselben Kennwort wieder.</summary>
    private void UpdateEditing()
    {
        var metadata = !pdfALocked && (!encrypted || openPassword || RemoveRestrictions);
        textBoxTitle.ReadOnly = textBoxAuthor.ReadOnly = textBoxSubject.ReadOnly = textBoxKeywords.ReadOnly = !metadata;
        buttonRemove.Enabled = metadata && !RemoveRequested;
        checkPassword.Enabled = !pdfALocked;
        textPassword.Enabled = textPasswordRepeat.Enabled = labelPassword.Enabled = labelPasswordRepeat.Enabled = !pdfALocked && checkPassword.Checked;
    }

    // ==== Beschreibung ====================================================================================================================

    private void FillDescription(PdfInfo info, FileInfo file, PdfStatus? status)
    {
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
        labelCreatedValue.Text = labelModifiedValue.Text = labelCreatorValue.Text = labelProducerValue.Text = Lng.T("wird entfernt");
        buttonRemove.Enabled = false;
        buttonOK.Focus();
    }

    // ==== Sicherheit ======================================================================================================================

    /// <summary>Kennwortschutz in einem Satz, darunter Häkchen und Kennwortfelder (seit 06.10.2026 statt der Menüpunkte „Kennwort
    /// vergeben/entfernen“ und „Einschränkungen entfernen“, nach Acrobats Reiter „Sicherheit“, aber bewusst schlicht: nur ein Kennwort
    /// zum Öffnen, fest AES 256 Bit, keine eigenen Berechtigungen – die beachten Betrachter nur freiwillig).</summary>
    private void FillSecurity((uint Permissions, int Revision) security)
    {
        var (bits, revision) = security;
        var method = revision switch
        {
            2 => "RC4, 40 Bit",
            3 => "RC4, 128 Bit",
            4 => Lng.T("128 Bit (RC4 oder AES)"),
            5 or 6 => "AES, 256 Bit",
            _ => "–",
        };
        labelSecurityStatus.Text = !encrypted ? Lng.T("Die Datei ist nicht geschützt.")
            : openPassword ? string.Format(Lng.T("Zum Öffnen ist ein Kennwort nötig. Verschlüsselung: {0}."), method)
            : string.Format(Lng.T("Die Datei lässt sich ohne Kennwort öffnen. Verschlüsselung: {0}."), method); // die Einschränkungen zeigt die Liste darunter (fett)
        checkPassword.Checked = openPassword;
        checkRemoveRestrictions.Visible = RestrictionsOnly;
        labelPasswordHint.Text = pdfALocked ? Lng.T("PDF/A-Dateien dürfen nicht verschlüsselt werden.")
            : openPassword ? Lng.T("Felder leer lassen, um das bisherige Kennwort zu behalten. Ein neues Kennwort wird mit AES 256 Bit verschlüsselt (lesbar ab Acrobat X).")
            : Lng.T("Kennwort.Info", labelPasswordHint.Text); // deutscher Text aus dem Designer, mehrzeilig – deshalb ein benannter Schlüssel

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
        // eingeschränkte Vorgänge fett (Wunsch vom 06.10.2026) – der Satz darüber sagt nur noch, ob ein Kennwort nötig ist
        var allowed = Lng.T("Zulässig");
        listPermissions.BeginUpdate();
        foreach (var (action, state) in rows)
        {
            ListViewItem item = new([action, state]) { UseItemStyleForSubItems = false };
            if (state != allowed) { item.Font = restrictedFont ??= new Font(listPermissions.Font, FontStyle.Bold); }
            listPermissions.Items.Add(item);
        }
        listPermissions.EndUpdate();
    }

    private Font? restrictedFont; // fette Schrift für eingeschränkte Vorgänge, beim Schließen freigegeben

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        restrictedFont?.Dispose();
    }

    private static string Allowed(bool allowed) => Lng.T(allowed ? "Zulässig" : "Nicht zulässig");

    private void CheckPassword_CheckedChanged(object? sender, EventArgs e)
    {
        // Ein neues Kennwort hebt bei einer Datei mit bloßen Einschränkungen auch diese auf – angehakt und gesperrt zeigt das
        if (RestrictionsOnly)
        {
            if (checkPassword.Checked) { checkRemoveRestrictions.Checked = true; }
            checkRemoveRestrictions.Enabled = !checkPassword.Checked;
        }
        UpdateEditing();
        if (checkPassword.Checked && checkPassword.Focused) { textPassword.Focus(); }
    }

    private void CheckRemoveRestrictions_CheckedChanged(object? sender, EventArgs e) => UpdateEditing();

    /// <summary>„OK“: ein neues Kennwort muss da sein und zweimal gleich lauten; ein sehr kurzes nur nach Rückfrage. Sonst bleibt der
    /// Dialog offen (DialogResult zurück auf None).</summary>
    private void ButtonOK_Click(object? sender, EventArgs e)
    {
        if (PasswordAction != PasswordChange.Set) { return; }
        string? problem = null;
        if (textPassword.Text.Length == 0) { problem = Lng.T("Gib ein Kennwort ein."); }
        else if (textPassword.Text != textPasswordRepeat.Text) { problem = Lng.T("Die beiden Kennwörter stimmen nicht überein."); }
        if (problem != null)
        {
            DialogResult = DialogResult.None;
            tabControl.SelectedTab = tabSecurity;
            TaskDlg.MsgTaskDlg(Handle, problem, null, TaskDialogIcon.Warning);
            textPasswordRepeat.Text = string.Empty;
            (textPassword.Text.Length == 0 ? textPassword : textPasswordRepeat).Focus();
            return;
        }
        if (textPassword.Text.Length < 6 && !TaskDlg.ConfirmTaskDlg(Handle, Lng.T("Das Kennwort ist sehr kurz."),
            Lng.T("Kurze Kennwörter lassen sich leicht erraten. Trotzdem verwenden?"), TaskDialogIcon.Warning, defaultNo: true))
        {
            DialogResult = DialogResult.None;
            tabControl.SelectedTab = tabSecurity;
            textPassword.Focus();
        }
    }

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

    /// <summary>Mit dem Reiter „Anhänge“ öffnen (Klick auf die Büroklammer in der Statusleiste), sofern es ihn gibt.</summary>
    public void SelectAttachments()
    {
        if (tabControl.TabPages.Contains(tabAttachments)) { tabControl.SelectedTab = tabAttachments; }
    }

    private void FillAttachments(IReadOnlyList<(string Name, long Size, DateTime? Modified)> attachments)
    {
        if (attachments.Count == 0 || loadAttachment == null)
        {
            tabControl.TabPages.Remove(tabAttachments);
            return;
        }
        listAttachments.BeginUpdate();
        for (var i = 0; i < attachments.Count; i++)
        {
            var (name, size, modified) = attachments[i];
            listAttachments.Items.Add(new ListViewItem([name.Length > 0 ? name : "?", modified?.ToString("g") ?? "–", size >= 0 ? FormatSize(size) : "–"]) { Tag = i });
        }
        listAttachments.EndUpdate();
        tabAttachments.Text += $" ({attachments.Count})";
    }

    private void ListAttachments_SelectedIndexChanged(object? sender, EventArgs e) => UpdateAttachmentButtons();

    private void UpdateAttachmentButtons()
    {
        var selected = listAttachments.SelectedItems.Count == 1;
        buttonSaveAttachment.Enabled = selected;
        buttonOpenAttachment.Enabled = selected && !ShellUtil.IsDangerousFileType(SafeFileName(listAttachments.SelectedItems[0].Text));
    }

    private void ListAttachments_DoubleClick(object? sender, EventArgs e) => OpenSelectedAttachment(); // wie Acrobat

    private void ButtonSaveAttachment_Click(object? sender, EventArgs e) => SaveSelectedAttachment();

    private void ButtonOpenAttachment_Click(object? sender, EventArgs e) => OpenSelectedAttachment();

    private void ContextMenuAttachments_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (listAttachments.SelectedItems.Count != 1) { e.Cancel = true; return; }
        mnuOpenAttachment.Enabled = buttonOpenAttachment.Enabled;
        mnuSaveAttachment.Enabled = buttonSaveAttachment.Enabled;
    }

    /// <summary>Öffnet die gewählte eingebettete Datei mit dem Programm, das Windows ihrem Typ zuordnet (Wunsch vom 05.10.2026, wie Acrobat;
    /// vorher nur Speichern). Drei Schutzmaßnahmen: Programme, Skripte und Ähnliches (<see cref="ShellUtil.IsDangerousFileType"/>) werden
    /// nie geöffnet, nur zum Speichern angeboten; vor dem Öffnen eine Rückfrage; die Kopie im Temp-Ordner bekommt die Herkunftsmarke
    /// „aus dem Internet“ (Mark of the Web) – Office öffnet sie dann in der geschützten Ansicht, SmartScreen greift.</summary>
    private async void OpenSelectedAttachment()
    {
        if (loadAttachment == null || listAttachments.SelectedItems.Count != 1 || listAttachments.SelectedItems[0].Tag is not int index) { return; }
        var fileName = SafeFileName(listAttachments.SelectedItems[0].Text);
        if (ShellUtil.IsDangerousFileType(fileName))
        {
            if (Ask(string.Format(Lng.T("„{0}“ wird nicht geöffnet."), fileName),
                Lng.T("Programme, Skripte und ähnliche Dateien öffnet PDFlight aus Sicherheitsgründen nicht. Möchtest du die Datei stattdessen speichern?"),
                TaskDialogIcon.Shield, Lng.T("Speichern unter…"))) { SaveSelectedAttachment(); }
            return;
        }
        if (!Ask(string.Format(Lng.T("„{0}“ öffnen?"), fileName),
            Lng.T("Anhänge können Makros oder Schadcode enthalten. Öffne die Datei nur, wenn die PDF aus einer sicheren Quelle stammt."),
            TaskDialogIcon.Warning, Lng.T("Öffnen"))) { return; }
        UseWaitCursor = true;
        try
        {
            var bytes = await loadAttachment(index);
            var path = Path.Combine(AttachmentTempFolder(), fileName);
            await File.WriteAllBytesAsync(path, bytes);
            MarkFromInternet(path);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            TaskDlg.ErrTaskDlg(Handle, Lng.T("Die eingebettete Datei konnte nicht geöffnet werden."), ex);
        }
        finally
        {
            if (!IsDisposed) { UseWaitCursor = false; }
        }
    }

    /// <summary>Rückfrage mit der Aktion als Schaltfläche („Öffnen“, „Speichern unter…“) und Abbrechen – klarer als Ja/Nein.</summary>
    private bool Ask(string heading, string text, TaskDialogIcon icon, string action)
    {
        var ok = new TaskDialogButton(action);
        TaskDialogPage page = new() { Caption = Application.ProductName, SizeToContent = true, Heading = heading, Text = text, Icon = icon, AllowCancel = true, Buttons = { ok, TaskDialogButton.Cancel } };
        return TaskDialog.ShowDialog(Handle, page) == ok;
    }

    /// <summary>Eigener Unterordner je geöffnetem Anhang unter %TEMP%\PDFlight\Anhänge (gleiche Namen aus verschiedenen PDFs kommen sich
    /// nicht in die Quere); Ordner älter als einen Tag werden dabei weggeräumt – geöffnete Dateien sind dann meist längst geschlossen.</summary>
    private static string AttachmentTempFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), "PDFlight", "Anhänge");
        if (Directory.Exists(root))
        {
            foreach (var old in Directory.GetDirectories(root).Where(d => Directory.GetCreationTimeUtc(d) < DateTime.UtcNow.AddDays(-1)))
            {
                try { Directory.Delete(old, recursive: true); }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { Debug.WriteLine(ex.Message); } // noch geöffnet
            }
        }
        var folder = Path.Combine(root, Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(folder);
        return folder;
    }

    /// <summary>Herkunftsmarke wie bei einem Download (Zone.Identifier, Zone 3 = Internet): Word/Excel öffnen in der geschützten Ansicht,
    /// SmartScreen prüft. Ohne NTFS (z. B. Temp auf FAT) geht das nicht – dann bleibt es bei Rückfrage und Typsperre.</summary>
    private static void MarkFromInternet(string path)
    {
        try { File.WriteAllText(path + ":Zone.Identifier", "[ZoneTransfer]\r\nZoneId=3\r\n"); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException) { Debug.WriteLine(ex.Message); }
    }

    /// <summary>Speichert die gewählte eingebettete Datei.</summary>
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
                UpdateAttachmentButtons();
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

/// <summary>Kennwortschutz nach „OK“ im Eigenschaften-Dialog (steht hinter der Form – der VS-Designer verlangt die Form als erste Klasse).</summary>
internal enum PasswordChange { Keep, Set, Remove }
