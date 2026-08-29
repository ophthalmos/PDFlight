using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;
using Microsoft.Web.WebView2.Core;

namespace PDFLight.Classes;

/// <summary>TaskDialog-Helfer als moderner Ersatz für MessageBox.
/// MsgTaskDlg und ErrTaskDlg sind unverändert aus einem anderen Projekt des Autors übernommen;
/// ConfirmTaskDlg, AboutTaskDlg und die ErrTaskDlg-Variante mit fachlicher Überschrift ergänzen sie im selben Stil.</summary>
internal static class TaskDlg
{
    public static void MsgTaskDlg(nint hwnd, string heading, string message, TaskDialogIcon icon = null)
    {
        TaskDialog.ShowDialog(hwnd, new TaskDialogPage() { Caption = Application.ProductName, SizeToContent = true, Heading = heading, Text = message, Icon = icon ?? TaskDialogIcon.None, AllowCancel = true, Buttons = { TaskDialogButton.OK } });
    }

    public static void ErrTaskDlg(nint? hwnd, Exception error)
    {
        TaskDialogPage page = new()
        {
            Caption = Application.ProductName,
            Heading = error.GetType().ToString(),
            Text = error.Message,
            Icon = TaskDialogIcon.Error,
            SizeToContent = true,
            AllowCancel = true,
            Buttons = { TaskDialogButton.OK },
            Expander = new TaskDialogExpander()
            {
                Text = error.ToString(),
                CollapsedButtonText = Lng.T("Technische Details anzeigen"),
                ExpandedButtonText = Lng.T("Details ausblenden"),
                Position = TaskDialogExpanderPosition.AfterFootnote
            }
        };
        TaskDialog.ShowDialog(hwnd ?? 0, page);
    }

    /// <summary>Fehlerdialog mit fachlicher Überschrift (z.B. "Verschieben fehlgeschlagen.") statt des Ausnahmetyps.</summary>
    public static void ErrTaskDlg(nint? hwnd, string heading, Exception error)
    {
        TaskDialogPage page = new()
        {
            Caption = Application.ProductName,
            Heading = heading,
            Text = error.Message,
            Icon = TaskDialogIcon.Error,
            SizeToContent = true,
            AllowCancel = true,
            Buttons = { TaskDialogButton.OK },
            Expander = new TaskDialogExpander()
            {
                Text = error.ToString(),
                CollapsedButtonText = Lng.T("Technische Details anzeigen"),
                ExpandedButtonText = Lng.T("Details ausblenden"),
                Position = TaskDialogExpanderPosition.AfterFootnote
            }
        };
        TaskDialog.ShowDialog(hwnd ?? 0, page);
    }

    /// <summary>Ja/Nein-Frage; true nur bei ausdrücklichem Ja (Abbrechen/Esc zählt als Nein).
    /// Mit defaultNo steht der Fokus auf "Nein" — für destruktive Aktionen wie das Ersetzen von Dateien.</summary>
    public static bool ConfirmTaskDlg(nint hwnd, string heading, string message, TaskDialogIcon icon = null, bool defaultNo = false)
    {
        TaskDialogPage page = new() { Caption = Application.ProductName, SizeToContent = true, Heading = heading, Text = message, Icon = icon ?? TaskDialogIcon.None, AllowCancel = true, Buttons = { TaskDialogButton.Yes, TaskDialogButton.No } };
        if (defaultNo) { page.DefaultButton = page.Buttons[1]; }
        return TaskDialog.ShowDialog(hwnd, page) == TaskDialogButton.Yes;
    }

    /// <summary>Ja/Nein-Frage mit Kontrollkästchen (z.B. "Immer fragen"); verificationChecked
    /// gibt den Anfangszustand vor und liefert den Endzustand zurück.</summary>
    public static bool ConfirmTaskDlg(nint hwnd, string heading, string message, string verificationText, ref bool verificationChecked, TaskDialogIcon icon = null, bool defaultNo = false)
    {
        TaskDialogPage page = new()
        {
            Caption = Application.ProductName,
            SizeToContent = true,
            Heading = heading,
            Text = message,
            Icon = icon ?? TaskDialogIcon.None,
            AllowCancel = true,
            Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
            Verification = new TaskDialogVerificationCheckBox(verificationText) { Checked = verificationChecked }
        };
        if (defaultNo) { page.DefaultButton = page.Buttons[1]; }
        var confirmed = TaskDialog.ShowDialog(hwnd, page) == TaskDialogButton.Yes;
        verificationChecked = page.Verification.Checked;
        return confirmed;
    }

    // Updatesuche über die XML-Datei auf der Webseite des Autors (wie bei den übrigen Programmen);
    // erwartete Elemente unterhalb der Wurzel: <version>, <date>, <url64>
    private const string UpdateXmlUrl = "https://www.netradio.info/download/pdflight.xml";
    private const string WebsiteUrl = "https://www.netradio.info";

    private static readonly Lazy<HttpClient> httpClient = new(() =>
        new HttpClient() { Timeout = TimeSpan.FromSeconds(15) });

    /// <summary>Über-Dialog mit den Versionen der verwendeten Komponenten, PayPal-Spendenlink
    /// (Button-Details aus PDFMover übernommen) und manueller Updatesuche.</summary>
    public static void AboutTaskDlg(nint hwnd, Icon icon)
    {
        var curVersion = Assembly.GetExecutingAssembly().GetName().Version;
        var threeVersion = curVersion?.ToString(3) ?? Lng.T("unbekannt");
        var buildDate = GetBuildDate();
        string webView2Runtime;
        try { webView2Runtime = CoreWebView2Environment.GetAvailableBrowserVersionString(); }
        catch (WebView2RuntimeNotFoundException) { webView2Runtime = Lng.T("nicht gefunden"); }
        var msg = Lng.T("About.Text",
            "PDFlight ist ein Viewer mit integrierten Dateioperationen" + Environment.NewLine +
            "(Verschieben, Kopieren, Löschen, Umbenennen, Mailen)." + Environment.NewLine +
            "Häufig benutzte Zielordner werden in einer Liste vorge-" + Environment.NewLine +
            "halten. Dateien lassen sich blitzschnell verschieben, ohne" + Environment.NewLine +
            "das Programm zu verlassen. Darüber hinaus lassen sich" + Environment.NewLine +
            "Seiten aus dem PDF entfernen, in eine neue Datei einfügen" + Environment.NewLine +
            "oder es kann eine andere PDF-Datei anhängt werden.");
        TaskDialogButton paypalButton = new TaskDialogCommandLinkButton(Lng.T("Anerkennung spenden via PayPal"));
        TaskDialogButton updateButton = new TaskDialogCommandLinkButton(Lng.T("Jetzt nach einem Update suchen")) { AllowCloseDialog = false };
        using var icon32 = icon == null ? null : new Icon(icon, 32, 32); // sonst nimmt der TaskDialog die 16-px-Variante des Fenster-Icons
        var indent = new string(' ', 14);
        var foot = $"{indent}© {buildDate:yyyy} Wilhelm Happe · Version {threeVersion} ({buildDate:d})" +
            $"\n{indent}WebView2-Runtime {webView2Runtime}" +
            $"\n{indent}PDFsharp {typeof(PdfSharp.Pdf.PdfDocument).Assembly.GetName().Version?.ToString(3)}";
        var initialPage = new TaskDialogPage()
        {
            Caption = Lng.T("Über") + " " + Application.ProductName,
            Heading = Application.ProductName,
            Text = msg,
            Icon = icon32 == null ? null : new TaskDialogIcon(icon32),
            AllowCancel = true,
            SizeToContent = true,
            Buttons = { paypalButton, updateButton, TaskDialogButton.OK },
            DefaultButton = TaskDialogButton.OK,
            Footnote = foot,
            Expander = new TaskDialogExpander()
            {
                Text = BuildShortcutTable(),
                CollapsedButtonText = Lng.T("Tastenkürzel anzeigen"),
                ExpandedButtonText = Lng.T("Tastenkürzel ausblenden"),
                Position = TaskDialogExpanderPosition.AfterFootnote
            }
        };

        // Updatesuche: Klick lädt die neueste GitHub-Veröffentlichung und blättert im Dialog zur Ergebnisseite (wie in Adressen)
        TaskDialogButton downloadButton = new TaskDialogCommandLinkButton(Lng.T("PDFlightSetup.exe herunterladen"),
            Lng.T("Download.Detail", "PDFlightSetup.exe wird im Download-Ordner\ngespeichert. Führen Sie das Setupprogramm\naus, um die neueste Version zu installieren."));
        var updatePage = new TaskDialogPage()
        {
            Caption = Lng.T("Über") + " " + Application.ProductName,
            Heading = string.Format(Lng.T("{0} ist auf dem neuesten Stand."), Application.ProductName),
            Text = $"Version {threeVersion} (64-Bit)",
            Icon = TaskDialogIcon.Information,
            AllowCancel = true,
            SizeToContent = true,
            Buttons = { TaskDialogButton.Close }
        };
        var urlString = WebsiteUrl; // Fallback: die Webseite, falls die XML keinen Download-Link nennt
        updateButton.Click += async (sender, e) =>
        {
            updateButton.Enabled = false; // um doppelte Klicks zu verhindern
            Version updateVersion = null;
            var dateString = string.Empty;
            var failed = false;
            try
            {
                await using var stream = await httpClient.Value.GetStreamAsync(UpdateXmlUrl);
                var root = System.Xml.Linq.XDocument.Load(stream).Root; // Wurzelname egal — Load verkraftet auch die BOM
                var versionString = root?.Element("version")?.Value;
                if (Version.TryParse(versionString ?? string.Empty, out var parsed)) { updateVersion = parsed; }
                dateString = root?.Element("date")?.Value ?? string.Empty;
                var url64 = root?.Element("url64")?.Value;
                if (!string.IsNullOrEmpty(url64)) { urlString = url64; }
            }
            catch (HttpRequestException ex)
            {
                failed = true;
                updatePage.Heading = Lng.T("Die Update-Suche ist fehlgeschlagen.");
                updatePage.Text = ex.StatusCode == HttpStatusCode.NotFound
                    ? Lng.T("Die Update-Informationen wurden nicht gefunden.")
                    : (ex.StatusCode != null ? $"Status-Code: {ex.StatusCode}\n" : string.Empty) + ex.Message;
            }
            catch (Exception ex) when (ex is TaskCanceledException or System.Xml.XmlException or InvalidOperationException)
            {
                failed = true;
                updatePage.Heading = Lng.T("Die Update-Suche ist fehlgeschlagen.");
                updatePage.Text = ex is TaskCanceledException
                    ? Lng.T("Zeitüberschreitung — bitte prüfen Sie die Internetverbindung.")
                    : ex.Message;
            }
            if (!failed && updateVersion == null)
            {
                failed = true;
                updatePage.Heading = Lng.T("Die Update-Suche ist fehlgeschlagen.");
                updatePage.Text = Lng.T("Die Versionsangabe in der Update-Datei konnte nicht gelesen werden.");
            }
            if (failed) { updatePage.Icon = TaskDialogIcon.Error; }
            else if (curVersion != null && updateVersion.CompareTo(curVersion) > 0)
            {
                updatePage.Heading = Lng.T("Es steht ein Update zur Verfügung!");
                updatePage.Text = "Version " + updateVersion + (dateString.Length > 0 ? " " + Lng.T("vom") + " " + dateString : string.Empty);
                updatePage.Buttons.Add(downloadButton);
            }
            initialPage.Navigate(updatePage);
        };

        var result = TaskDialog.ShowDialog(hwnd, initialPage);
        if (result == paypalButton) { StartLink(hwnd, "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=S8DVXHKFC2CVS&source=url"); }
        else if (result == downloadButton) { StartLink(hwnd, urlString); }
    }

    /// <summary>Richtet zweispaltige Kürzel-Listen bündig aus: TaskDialog-Text kennt keine Tabulatoren,
    /// darum wird jedes Kürzel mit geschützten Leerzeichen aufgefüllt, bis es die gemessene Spaltenbreite
    /// erreicht — gemessen mit derselben Schrift, in der der TaskDialog seinen Text setzt (Segoe UI 9 pt).</summary>
    public static string AlignShortcutColumns((string Key, string Text)[] rows)
    {
        using Font font = new("Segoe UI", 9f);
        int Width(string s) => TextRenderer.MeasureText(s + "|", font, Size.Empty, TextFormatFlags.NoPadding).Width; // Sentinel, damit Leerzeichen am Ende mitzählen
        var sentinel = Width(string.Empty);
        var translated = rows.Select(r => (Key: Lng.T(r.Key), Text: Lng.T(r.Text))).ToArray(); // zentral übersetzen (Strg -> Ctrl usw.)
        var column = translated.Max(r => Width(r.Key) - sentinel) + 14;
        var lines = translated.Select(r =>
        {
            var key = r.Key;
            while (Width(key) - sentinel < column) { key += '\u00A0'; } // geschützte Leerzeichen werden nicht getrimmt
            return key + r.Text;
        });
        return string.Join("\n", lines);
    }

    private static string BuildShortcutTable()
    {
        return AlignShortcutColumns(
        [
            ("Strg+O", "PDF-Datei öffnen"),
            ("Strg+Umschalt+← / →", "vorherige / nächste Datei des Ordners"),
            ("Bild ↑ / Bild ↓", "im Dokument blättern"),
            ("Strg+M", "verschieben (Strg+Klick: erster Zielordner)"),
            ("Strg+K", "Datei kopieren"),
            ("F2", "Datei umbenennen"),
            ("Strg+Umschalt+Entf", "Datei in den Papierkorb"),
            ("Strg+Entf", "Seiten löschen"),
            ("Strg+R", "Seiten drehen"),
            ("Strg+Umschalt+R / L", "Ansicht drehen (ändert die Datei nicht)"),
            ("Strg+X", "Seiten als neue Datei extrahieren"),
            ("Strg+Z", "Dokumentänderung rückgängig"),
            ("Strg+I", "Dokumenteigenschaften"),
            ("Alt+Enter", "Windows-Dateieigenschaften"),
            ("Strg+E", "als E-Mail-Anhang senden"),
            ("Strg+1 … 9", "in externem Programm öffnen"),
            ("Strg+F", "im Dokument suchen"),
            ("Strg+P", "Dokument drucken"),
            ("F11", "Vollbild ein/aus"),
            ("2× Esc / Umschalt+Esc", "Programm beenden (Option)"),
            ("F1", "dieser Dialog"),
        ]);
    }

    internal static void StartLink(nint hwnd, string url)
    {
        try
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else { MsgTaskDlg(hwnd, Lng.T("Ungültiger Link!"), string.Format(Lng.T("'{0}' ist keine gültige URL."), url), TaskDialogIcon.ShieldWarningYellowBar); }
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException) { ErrTaskDlg(hwnd, Lng.T("Der Link konnte nicht geöffnet werden."), ex); }
    }

    private static DateTime GetBuildDate()
    { // s. <SourceRevisionId>build$([System.DateTime]::UtcNow.ToString("yyyyMMddHHmmss"))</SourceRevisionId> in PDFlight.csproj
        const string BuildVersionMetadataPrefix = "+build";
        var attribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(BuildVersionMetadataPrefix, StringComparison.Ordinal);
            if (index > 0)
            {
                value = value[(index + BuildVersionMetadataPrefix.Length)..];
                if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) { return result; }
            }
        }
        return File.GetLastWriteTime(Application.ExecutablePath);
    }
}
