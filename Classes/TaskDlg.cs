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
            Buttons = { paypalButton, TaskDialogButton.OK },
            DefaultButton = TaskDialogButton.OK,
            Footnote = foot
        };

        var result = TaskDialog.ShowDialog(hwnd, initialPage);
        if (result == paypalButton) { StartLink(hwnd, "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=S8DVXHKFC2CVS&source=url"); }
    }

    /// <summary>Manuelle Updatesuche (Info-Menü): lädt die XML-Datei von der Webseite des Autors
    /// und zeigt das Ergebnis; bei einem Update mit Download-Schaltfläche.</summary>
    public static async Task UpdateTaskDlg(nint hwnd)
    {
        var curVersion = Assembly.GetExecutingAssembly().GetName().Version;
        var threeVersion = curVersion?.ToString(3) ?? Lng.T("unbekannt");
        TaskDialogButton downloadButton = new TaskDialogCommandLinkButton(Lng.T("PDFlightSetup.exe herunterladen"),
            Lng.T("Download.Detail", "PDFlightSetup.exe wird im Download-Ordner\ngespeichert. Führen Sie das Setupprogramm\naus, um die neueste Version zu installieren."));
        var updatePage = new TaskDialogPage()
        {
            Caption = Application.ProductName,
            Heading = string.Format(Lng.T("{0} ist auf dem neuesten Stand."), Application.ProductName),
            Text = $"Version {threeVersion} (64-Bit)",
            Icon = TaskDialogIcon.Information,
            AllowCancel = true,
            SizeToContent = true,
            Buttons = { TaskDialogButton.Close }
        };
        var urlString = WebsiteUrl; // Fallback: die Webseite, falls die XML keinen Download-Link nennt
        Version updateVersion = null;
        var dateString = string.Empty;
        var failed = false;
        Cursor.Current = Cursors.WaitCursor; // die Abfrage dauert im Normalfall unter einer Sekunde
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
        finally { Cursor.Current = Cursors.Default; }
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
        if (TaskDialog.ShowDialog(hwnd, updatePage) == downloadButton) { StartLink(hwnd, urlString); }
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

    /// <summary>Alle Tastenkürzel: Kürzel, Kurztext (Dialog-Tabelle) und ausführliche Erklärung (PDF-Übersicht).</summary>
    public static readonly (string Key, string Text, string Detail)[] ShortcutRows =
    [
        ("Strg+O", "PDF-Datei öffnen",
            "Öffnet den Dateiauswahl-Dialog. Der Pfeil neben der Schaltfläche listet die zuletzt geöffneten Dateien."),
        ("Strg+Umschalt+← / →", "vorherige / nächste Datei des Ordners",
            "Wechselt zur vorherigen bzw. nächsten PDF-Datei im Ordner der angezeigten Datei — ideal zum Abarbeiten eines Scan-Ordners."),
        ("Bild ↑ / Bild ↓", "im Dokument blättern",
            "Blättert innerhalb des Dokuments seitenweise vor und zurück."),
        ("Strg+M", "verschieben (Strg+Klick: erster Zielordner)",
            "Verschiebt die Datei in einen Ordner der Zielliste. Strg+Klick auf die Schaltfläche verschiebt sofort in den ersten Zielordner, der Pfeil öffnet die Liste."),
        ("Strg+K", "Datei kopieren",
            "Kopiert die Datei in einen wählbaren Ordner; die angezeigte Datei bleibt unverändert."),
        ("F2", "Datei umbenennen",
            "Benennt die Datei um. Der Dialog bietet Bausteine wie das Datum sowie eine gespeicherte Namensliste an."),
        ("Strg+Umschalt+Entf", "Datei in den Papierkorb",
            "Verschiebt die Datei in den Papierkorb. Die Sicherheitsabfrage lässt sich in den Einstellungen abschalten."),
        ("Strg+Entf", "Seiten löschen",
            "Löscht die angegebenen Seiten dauerhaft aus dem Dokument — rückgängig mit Strg+Z."),
        ("Strg+R", "Seiten drehen",
            "Dreht die angegebenen Seiten dauerhaft um 90° oder 180°; die Änderung wird sofort in der Datei gespeichert."),
        ("Strg+Umschalt+R / L", "Ansicht drehen (ändert die Datei nicht)",
            "Dreht nur die Bildschirmansicht rechts- bzw. linksherum. Die Datei bleibt unverändert; beim Neuladen ist die Drehung weg."),
        ("Strg+Umschalt+I", "Inhalte-Leiste ein-/ausblenden",
            "Öffnet oder schließt die Leiste mit den Miniaturansichten am linken Rand."),
        ("Strg+Umschalt+B", "Seite an Fensterbreite anpassen",
            "Wechselt zwischen den Zoomstufen „an Fensterbreite“ und „ganze Seite sichtbar“."),
        ("Strg+G", "Gehe zu Seite (Zahl tippen + Enter)",
            "Setzt den Cursor in das Seitenzahl-Feld der Viewer-Leiste. Seitenzahl eintippen und mit Enter springen."),
        ("Strg+X", "Seiten als neue Datei extrahieren",
            "Speichert die angegebenen Seiten als neue PDF-Datei; das Original bleibt unverändert."),
        ("Strg+Z", "Dokumentänderung rückgängig",
            "Macht die letzte Dokumentänderung rückgängig — etwa Seiten löschen/drehen, Anhängen oder eine Kennwort-Änderung."),
        ("Strg+I", "Dokumenteigenschaften",
            "Zeigt Titel, Autor, Betreff und Stichwörter (editierbar) sowie PDF-Version und erzeugendes Programm."),
        ("Alt+Enter", "Windows-Dateieigenschaften",
            "Öffnet das Eigenschaften-Fenster der Datei, wie man es aus dem Windows-Explorer kennt."),
        ("Strg+E", "als E-Mail-Anhang senden",
            "Erstellt im Standard-Mailprogramm eine neue E-Mail mit der Datei als Anhang."),
        ("Strg+1 … 9", "in externem Programm öffnen",
            "Öffnet die Datei im jeweiligen Programm der Programme-Liste (Reihenfolge wie im Menü)."),
        ("Strg+F", "im Dokument suchen",
            "Öffnet die Suchleiste des Viewers; F3 springt zum nächsten Treffer, Esc schließt die Leiste."),
        ("Strg+P", "Dokument drucken",
            "Öffnet die Druckvorschau mit allen Druckoptionen."),
        ("F7", "Textcursor-Navigation ein/aus (Markieren per Tastatur)",
            "Setzt einen Textcursor ins Dokument: Pfeiltasten bewegen ihn, Umschalt+Pfeile markieren Text, Strg+C kopiert — Markieren ganz ohne Maus."),
        ("F11", "Vollbild ein/aus",
            "Schaltet die Vollbild-Anzeige um; auch Esc beendet das Vollbild."),
        ("2× Esc / Umschalt+Esc", "Programm beenden (Option)",
            "Beendet das Programm, wenn die Option in den Einstellungen aktiv ist. Das erste Esc schließt zunächst eine offene Viewer-Leiste, Umschalt+Esc beendet sofort."),
        ("F1", "dieser Dialog",
            "Öffnet diese Kürzel-Übersicht."),
    ];

    private static string BuildShortcutTable()
    {
        return AlignShortcutColumns([.. ShortcutRows.Select(r => (r.Key, r.Text))]);
    }

    /// <summary>Eigener Kürzel-Dialog (F1 und Info-Menü); openPdf zeigt die erstellte PDF-Übersicht an.</summary>
    public static void ShortcutsTaskDlg(nint hwnd, Action<string> openPdf)
    {
        TaskDialogButton pdfButton = new TaskDialogCommandLinkButton(Lng.T("Als PDF anzeigen"),
            Lng.T("Shortcut.PdfDetail", "Erstellt eine druckbare Übersicht mit Erklärungen im" + Environment.NewLine +
                "Downloads-Ordner und zeigt sie in PDFlight an."));
        var page = new TaskDialogPage()
        {
            Caption = Application.ProductName,
            Heading = Lng.T("Tastenkürzel"),
            Text = BuildShortcutTable(),
            AllowCancel = true,
            SizeToContent = true,
            Buttons = { pdfButton, TaskDialogButton.Close },
            DefaultButton = TaskDialogButton.Close
        };
        if (TaskDialog.ShowDialog(hwnd, page) == pdfButton)
        {
            try { openPdf(ShortcutsPdf.Create()); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or PdfSharp.PdfSharpException)
            {
                ErrTaskDlg(hwnd, Lng.T("Die PDF-Übersicht konnte nicht erstellt werden."), ex);
            }
        }
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
