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
                CollapsedButtonText = "Technische Details anzeigen",
                ExpandedButtonText = "Details ausblenden",
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
                CollapsedButtonText = "Technische Details anzeigen",
                ExpandedButtonText = "Details ausblenden",
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
        var threeVersion = curVersion?.ToString(3) ?? "unbekannt";
        var buildDate = GetBuildDate();
        string webView2Runtime;
        try { webView2Runtime = CoreWebView2Environment.GetAvailableBrowserVersionString(); }
        catch (WebView2RuntimeNotFoundException) { webView2Runtime = "nicht gefunden"; }
        var msg = "PDFlight ist ein Viewer mit integrierten Dateioperationen" + Environment.NewLine +
            "(Verschieben, Kopieren, Löschen, Umbenennen, Mailen)." + Environment.NewLine +
            "Häufig benutzte Zielordner werden in einer Liste vorge-" + Environment.NewLine +
            "halten. Dateien lassen sich blitzschnell verschieben, ohne" + Environment.NewLine +
            "das Programm zu verlassen. Darüber hinaus lassen sich" + Environment.NewLine +
            "Seiten aus dem PDF entfernen, in eine neue Datei einfügen" + Environment.NewLine +
            "oder es kann eine andere PDF-Datei anhängt werden.";
        TaskDialogButton paypalButton = new TaskDialogCommandLinkButton("Anerkennung spenden via PayPal");
        TaskDialogButton updateButton = new TaskDialogCommandLinkButton("Jetzt nach einem Update suchen") { AllowCloseDialog = false };
        using var icon32 = icon == null ? null : new Icon(icon, 32, 32); // sonst nimmt der TaskDialog die 16-px-Variante des Fenster-Icons
        var indent = new string(' ', 14);
        var foot = $"{indent}© {buildDate:yyyy} Wilhelm Happe · Version {threeVersion} ({buildDate:d})" +
            $"\n{indent}WebView2-Runtime {webView2Runtime}" +
            $"\n{indent}PDFsharp {typeof(PdfSharp.Pdf.PdfDocument).Assembly.GetName().Version?.ToString(3)}";
        var initialPage = new TaskDialogPage()
        {
            Caption = "Über " + Application.ProductName,
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
                Text = "Strg+O – PDF-Datei öffnen\n" +
                    "Alt+← / Alt+→ – vorherige / nächste Datei des Ordners\n" +
                    "Bild ↑ / Bild ↓ – im Dokument blättern\n" +
                    "Strg+M – verschieben (Strg+Klick: erster Zielordner)\n" +
                    "Strg+K – kopieren\n" +
                    "F2 – umbenennen\n" +
                    "Entf – in den Papierkorb\n" +
                    "Strg+Entf – Seiten löschen\n" +
                    "Strg+R – Seiten drehen\n" +
                    "Strg+Z – Dokumentänderung rückgängig\n" +
                    "Strg+I – Dokumenteigenschaften\n" +
                    "Alt+Enter – Windows-Dateieigenschaften\n" +
                    "Strg+E – als E-Mail-Anhang senden\n" +
                    "Strg+1 … 9 – in externem Programm öffnen\n" +
                    "Strg+F – im Dokument suchen\n" +
                    "Strg+P – drucken\n" +
                    "F11 – Vollbild ein/aus\n" +
                    "2× Esc bzw. Umschalt+Esc – Programm beenden (Option)\n" +
                    "F1 – dieser Dialog",
                CollapsedButtonText = "Tastenkürzel anzeigen",
                ExpandedButtonText = "Tastenkürzel ausblenden",
                Position = TaskDialogExpanderPosition.AfterFootnote
            }
        };

        // Updatesuche: Klick lädt die neueste GitHub-Veröffentlichung und blättert im Dialog zur Ergebnisseite (wie in Adressen)
        TaskDialogButton downloadButton = new TaskDialogCommandLinkButton("PDFlightSetup.exe herunterladen",
            "PDFlightSetup.exe wird im Download-Ordner\ngespeichert. Führen Sie das Setupprogramm\naus, um die neueste Version zu installieren.");
        var updatePage = new TaskDialogPage()
        {
            Caption = "Über " + Application.ProductName,
            Heading = Application.ProductName + " ist auf dem neuesten Stand.",
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
                updatePage.Heading = "Die Update-Suche ist fehlgeschlagen.";
                updatePage.Text = ex.StatusCode == HttpStatusCode.NotFound
                    ? "Die Update-Informationen wurden nicht gefunden."
                    : (ex.StatusCode != null ? $"Status-Code: {ex.StatusCode}\n" : string.Empty) + ex.Message;
            }
            catch (Exception ex) when (ex is TaskCanceledException or System.Xml.XmlException or InvalidOperationException)
            {
                failed = true;
                updatePage.Heading = "Die Update-Suche ist fehlgeschlagen.";
                updatePage.Text = ex is TaskCanceledException
                    ? "Zeitüberschreitung — bitte prüfen Sie die Internetverbindung."
                    : ex.Message;
            }
            if (!failed && updateVersion == null)
            {
                failed = true;
                updatePage.Heading = "Die Update-Suche ist fehlgeschlagen.";
                updatePage.Text = "Die Versionsangabe in der Update-Datei konnte nicht gelesen werden.";
            }
            if (failed) { updatePage.Icon = TaskDialogIcon.Error; }
            else if (curVersion != null && updateVersion.CompareTo(curVersion) > 0)
            {
                updatePage.Heading = "Es steht ein Update zur Verfügung!";
                updatePage.Text = "Version " + updateVersion + (dateString.Length > 0 ? " vom " + dateString : string.Empty);
                updatePage.Buttons.Add(downloadButton);
            }
            initialPage.Navigate(updatePage);
        };

        var result = TaskDialog.ShowDialog(hwnd, initialPage);
        if (result == paypalButton) { StartLink(hwnd, "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=S8DVXHKFC2CVS&source=url"); }
        else if (result == downloadButton) { StartLink(hwnd, urlString); }
    }

    internal static void StartLink(nint hwnd, string url)
    {
        try
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else { MsgTaskDlg(hwnd, "Ungültiger Link!", $"'{url}' ist keine gültige URL.", TaskDialogIcon.ShieldWarningYellowBar); }
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException) { ErrTaskDlg(hwnd, "Der Link konnte nicht geöffnet werden.", ex); }
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
