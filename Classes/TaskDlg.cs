using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
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

    // Updatesuche über die GitHub-Releases-API: neueste Veröffentlichung samt Setup-Datei
    private const string UpdateApiUrl = "https://api.github.com/repos/ophthalmos/PDFlight/releases/latest";
    private const string ReleasesPageUrl = "https://github.com/ophthalmos/PDFlight/releases";

    private static readonly Lazy<HttpClient> httpClient = new(() =>
    {
        HttpClient client = new() { Timeout = TimeSpan.FromSeconds(15) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("PDFlight"); // die GitHub-API verlangt einen User-Agent
        return client;
    });

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
        var msg = "PDFlight ist ein PDF-Viewer, dessen Highlight die" + Environment.NewLine +
            "integrierten Dateioperationen sind (Verschieben, Kopieren," + Environment.NewLine +
            "Löschen, Umbenennen, Verschicken). Für häufig benutzte" + Environment.NewLine +
            "Zielordner lässt sich eine Liste anlegen, so dass Dateien" + Environment.NewLine +
            "blitzschnell verschoben werden können, ohne das Programm" + Environment.NewLine +
            "zu verlassen. Darüber hinaus lassen sich Seiten aus dem" + Environment.NewLine +
            "PDF entfernen, in eine neue Datei einfügen oder eine" + Environment.NewLine +
            "andere PDF-Datei anhängen." + Environment.NewLine +
            "Andere PDF-Programme lassen sich aus PDFlight heraus" + Environment.NewLine +
            "starten – falls eine Funktion fehlen sollte.";
        TaskDialogButton paypalButton = new TaskDialogCommandLinkButton("Anerkennung spenden via PayPal");
        TaskDialogButton updateButton = new TaskDialogCommandLinkButton("Jetzt nach einem Update suchen") { AllowCloseDialog = false };
        using var icon32 = icon == null ? null : new Icon(icon, 32, 32); // sonst nimmt der TaskDialog die 16-px-Variante des Fenster-Icons
        var indent = new string(' ', 14);
        var foot = $"{indent}© {buildDate:yyyy} Wilhelm Happe · Version {threeVersion} ({buildDate:d})" +
            $"\n{indent}WebView2-Runtime {webView2Runtime} · SDK {typeof(CoreWebView2Environment).Assembly.GetName().Version}" +
            $"\n{indent}PDFsharp {typeof(PdfSharp.Pdf.PdfDocument).Assembly.GetName().Version?.ToString(3)} · {RuntimeInformation.FrameworkDescription}";
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
            Footnote = foot
        };

        // Updatesuche: Klick lädt die neueste GitHub-Veröffentlichung und blättert im Dialog zur Ergebnisseite (wie in Adressen)
        TaskDialogButton downloadButton = new TaskDialogCommandLinkButton("PDFlightSetup.exe herunterladen",
            "Das Setup wird im Browser heruntergeladen. Führen Sie es\naus, um die neue Version zu installieren.");
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
        var urlString = ReleasesPageUrl; // Fallback: die Releases-Seite, falls kein Setup-Asset gefunden wird
        updateButton.Click += async (sender, e) =>
        {
            updateButton.Enabled = false; // um doppelte Klicks zu verhindern
            Version updateVersion = null;
            var dateString = string.Empty;
            var failed = false;
            try
            {
                var json = await httpClient.Value.GetStringAsync(UpdateApiUrl);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var tag = root.GetProperty("tag_name").GetString() ?? string.Empty; // z.B. "v0.2.0"
                if (Version.TryParse(tag.TrimStart('v', 'V'), out var parsed)) { updateVersion = parsed; }
                if (root.TryGetProperty("published_at", out var published) && published.TryGetDateTime(out var publishedAt))
                { dateString = publishedAt.ToLocalTime().ToString("d"); }
                if (root.TryGetProperty("assets", out var assets))
                {
                    foreach (var asset in assets.EnumerateArray())
                    {
                        var name = asset.GetProperty("name").GetString();
                        if (name != null && name.EndsWith("Setup.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            urlString = asset.GetProperty("browser_download_url").GetString() ?? urlString;
                            break;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                failed = true;
                updatePage.Heading = "Die Update-Suche ist fehlgeschlagen.";
                updatePage.Text = ex.StatusCode == HttpStatusCode.NotFound
                    ? "Es wurden keine Veröffentlichungen gefunden."
                    : (ex.StatusCode != null ? $"Status-Code: {ex.StatusCode}\n" : string.Empty) + ex.Message;
            }
            catch (Exception ex) when (ex is TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
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
                updatePage.Text = "Die Versionsangabe der Veröffentlichung konnte nicht gelesen werden.";
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
