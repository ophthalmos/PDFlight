using System.Diagnostics;

namespace PDFLight.Classes;

/// <summary>Eingebettete Dateien öffnen und speichern – für die Anhangliste der Seitenleiste (seit 15.10.2026; vorher im Reiter
/// „Anhänge“ des Eigenschaften-Dialogs). Die Inhalte liest der Aufrufer aus dem Hilfsprogramm (<c>load</c>); das Hauptprogramm
/// schreibt sie nur in Dateien, es wertet sie nie aus.</summary>
internal static class AttachmentFiles
{
    /// <summary>Öffnet eine eingebettete Datei mit dem Programm, das Windows ihrem Typ zuordnet (Wunsch vom 05.10.2026, wie Acrobat).
    /// Drei Schutzmaßnahmen: Programme, Skripte und Ähnliches (<see cref="ShellUtil.IsDangerousFileType"/>) werden nie geöffnet, nur zum
    /// Speichern angeboten; vor dem Öffnen eine Rückfrage; die Kopie im Temp-Ordner bekommt die Herkunftsmarke „aus dem Internet“ (Mark of
    /// the Web) – Office öffnet sie dann in der geschützten Ansicht, SmartScreen greift.</summary>
    public static async Task OpenAsync(IWin32Window owner, string name, Func<Task<byte[]>> load)
    {
        var fileName = SafeFileName(name);
        if (ShellUtil.IsDangerousFileType(fileName))
        {
            if (Ask(owner, string.Format(Lng.T("„{0}“ wird nicht geöffnet."), fileName),
                Lng.T("Programme, Skripte und ähnliche Dateien öffnet PDFlight aus Sicherheitsgründen nicht. Möchtest du die Datei stattdessen speichern?"),
                TaskDialogIcon.Shield, Lng.T("Speichern unter…"))) { await SaveAsAsync(owner, name, load); }
            return;
        }
        if (!Ask(owner, string.Format(Lng.T("„{0}“ öffnen?"), fileName),
            Lng.T("Anhänge können Makros oder Schadcode enthalten. Öffne die Datei nur, wenn die PDF aus einer sicheren Quelle stammt."),
            TaskDialogIcon.Warning, Lng.T("Öffnen"))) { return; }
        try
        {
            var bytes = await load();
            var path = Path.Combine(TempFolder(), fileName);
            await File.WriteAllBytesAsync(path, bytes);
            MarkFromInternet(path);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            TaskDlg.ErrTaskDlg(owner.Handle, Lng.T("Die eingebettete Datei konnte nicht geöffnet werden."), ex);
        }
    }

    /// <summary>Speichert eine eingebettete Datei unter einem gewählten Namen; true, wenn gespeichert.</summary>
    public static async Task<bool> SaveAsAsync(IWin32Window owner, string name, Func<Task<byte[]>> load)
    {
        using SaveFileDialog dialog = new()
        {
            Title = Lng.T("Eingebettete Datei speichern"),
            Filter = Lng.T("Alle Dateien (*.*)|*.*"),
            FileName = SafeFileName(name),
        };
        if (dialog.ShowDialog(owner) != DialogResult.OK) { return false; }
        try
        {
            await File.WriteAllBytesAsync(dialog.FileName, await load());
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            TaskDlg.ErrTaskDlg(owner.Handle, Lng.T("Die eingebettete Datei konnte nicht gespeichert werden."), ex);
            return false;
        }
    }

    /// <summary>„Alle speichern…“: alle eingebetteten Dateien in einen gewählten Ordner; vorhandene Dateien bleiben unberührt, der Anhang
    /// bekommt dann „Name (2).ext“. Liefert Ordner und Anzahl der gespeicherten Dateien (null = abgebrochen).</summary>
    public static async Task<(string Folder, int Count)?> SaveAllAsync(IWin32Window owner, IReadOnlyList<(string Name, Func<Task<byte[]>> Load)> files, string initialFolder)
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = Lng.T("Ordner für die eingebetteten Dateien"),
            UseDescriptionForTitle = true,
            InitialDirectory = initialFolder,
        };
        if (dialog.ShowDialog(owner) != DialogResult.OK) { return null; }
        var folder = dialog.SelectedPath;
        var count = 0;
        try
        {
            foreach (var (name, load) in files)
            {
                await File.WriteAllBytesAsync(FileUtil.UniquePath(Path.Combine(folder, SafeFileName(name))), await load());
                count++;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            TaskDlg.ErrTaskDlg(owner.Handle, Lng.T("Die eingebettete Datei konnte nicht gespeichert werden."), ex);
        }
        return (folder, count);
    }

    /// <summary>Rückfrage mit der Aktion als Schaltfläche („Öffnen“, „Speichern unter…“) und Abbrechen – klarer als Ja/Nein.</summary>
    private static bool Ask(IWin32Window owner, string heading, string text, TaskDialogIcon icon, string action)
    {
        var ok = new TaskDialogButton(action);
        TaskDialogPage page = new() { Caption = Application.ProductName, SizeToContent = true, Heading = heading, Text = text, Icon = icon, AllowCancel = true, Buttons = { ok, TaskDialogButton.Cancel } };
        return TaskDialog.ShowDialog(owner.Handle, page) == ok;
    }

    /// <summary>Eigener Unterordner je geöffnetem Anhang unter %TEMP%\PDFlight\Anhänge (gleiche Namen aus verschiedenen PDFs kommen sich
    /// nicht in die Quere); Ordner älter als einen Tag werden dabei weggeräumt – geöffnete Dateien sind dann meist längst geschlossen.</summary>
    private static string TempFolder()
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

    /// <summary>Nur der Dateiname, ohne Pfadanteile und unzulässige Zeichen – der Name in der PDF stammt vom Ersteller der Datei.</summary>
    public static string SafeFileName(string name)
    {
        var fileName = name.Replace('/', '\\');
        fileName = fileName[(fileName.LastIndexOf('\\') + 1)..];
        foreach (var c in Path.GetInvalidFileNameChars()) { fileName = fileName.Replace(c, '_'); }
        fileName = fileName.Trim().TrimEnd('.');
        return fileName.Length > 0 ? fileName : "attachment";
    }
}
