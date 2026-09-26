using System.Diagnostics;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace PDFLight.Classes;

/// <summary>
/// Kapselt den WebView2-PDF-Viewer (Chromium/PDFium). Das Dokument wird aus dem Speicher
/// serviert, damit die Datei auf der Platte nie gesperrt ist und jederzeit verschoben,
/// umbenannt oder gelöscht werden kann. Die Kapselung erlaubt später einen Wechsel auf
/// PDF.js, ohne dass das Hauptformular angepasst werden muss.
/// </summary>
// Hinweis: Der "Dateieigenschaften"-Eintrag im "…"-Menü der Viewer-Toolbar ist erweiterungsinterne
// Chromium-UI und von außen nicht auslösbar (in den per ContextMenuRequested abfangbaren Kontextmenüs
// kommt er nicht vor — geprüft für Seite, Rand und Toolbar). Alt+Enter zeigt darum die
// Windows-Dateieigenschaften (ShellUtil.ShowFileProperties), wie im Explorer.
internal partial class PdfViewHost(WebView2 webView)
{
    private const string VirtualHost = "pdflight.doc";
    private readonly WebView2 webView = webView;
    private byte[]? currentBytes;

    /// <summary>Wird ausgelöst, wenn eine PDF-Datei auf den Viewer gezogen wurde (Drop löst dort eine file://-Navigation aus).</summary>
    public event EventHandler<string>? PdfFileDropped;

    public bool IsReady { get; private set; }

    /// <summary>Anzeigehintergrund dunkel: setzt das bevorzugte Farbschema des WebView2-Profils. Der Chromium-PDF-Viewer richtet
    /// Leiste und Fläche neben den Seiten danach aus (die Seiten selbst bleiben unverändert). Vor <see cref="InitializeAsync"/>
    /// gesetzt gilt es ab dem ersten Dokument; eine spätere Änderung wirkt sofort auf das geladene Dokument (geprüft 20.09.2026). Die eigene
    /// Leerseite (<see cref="ShowEmptyPage"/>) folgt per prefers-color-scheme, die Hintergrundfarbe des Controls ebenfalls.</summary>
    public bool DarkScheme
    {
        get => darkScheme;
        set
        {
            darkScheme = value;
            var background = value ? Color.FromArgb(51, 51, 51) : Color.FromArgb(243, 243, 243); // wie die Leerseite
            webView.BackColor = background;               // das Control selbst, bevor Chromium überhaupt zeichnet – sonst blitzt beim Start erst „Control“-Grau auf (Wunsch vom 20.09.2026)
            webView.DefaultBackgroundColor = background;  // Chromiums Hintergrund zwischen zwei Dokumenten
            if (IsReady) { webView.CoreWebView2.Profile.PreferredColorScheme = value ? CoreWebView2PreferredColorScheme.Dark : CoreWebView2PreferredColorScheme.Light; }
        }
    }
    private bool darkScheme;

    /// <summary>Die WebView2-Umgebung des Viewers – weitere WebViews im selben Prozess (Vorschau im Anmerkungsdialog)
    /// müssen dieselbe verwenden, weil sie am selben Datenordner hängen.</summary>
    public static CoreWebView2Environment? SharedEnvironment { get; private set; }

    /// <summary>Die Bytes des angezeigten Dokuments (null ohne Dokument) — z.B. um eine extern
    /// verschwundene Datei aus der Anzeige wiederherzustellen.</summary>
    public byte[]? DocumentBytes => currentBytes;

    public async Task InitializeAsync()
    {
        // Eigener Datenordner, damit das Programm auch aus einem schreibgeschützten Installationsordner läuft.
        // Je Sprache getrennt: Alle Prozesse am selben Ordner müssen identische Optionen verwenden, sonst
        // scheitert die Initialisierung (z.B. alte Instanz läuft nach einem Sprachwechsel noch).
        var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PDFlight", "WebView2." + Lng.CultureCode);
        var options = new CoreWebView2EnvironmentOptions
        {
            Language = Lng.CultureCode, // Viewer-Oberfläche in der Programmsprache
            IsCustomCrashReportingEnabled = true, // Absturzberichte nicht an Microsoft senden (Minidumps bleiben lokal im Datenordner)
            // Datensparsamkeit — nur Schalter, die Chromium tatsächlich kennt (chrome_switches.h, metrics_switches.h; unbekannte
            // wie „--disable-telemetry“ würden still ignoriert): Metriken (UMA) nur aufzeichnen, nicht hochladen; keine
            // Hintergrund-Netzwerkdienste; keine Domain-Reliability-Berichte; keine Komponenten-Updates aus dem Viewer heraus.
            // Was Microsoft als „erforderliche Diagnosedaten“ des WebView2-Laufzeitmoduls einstuft, lässt sich per App nicht abstellen.
            AdditionalBrowserArguments = "--metrics-recording-only --disable-background-networking --disable-domain-reliability --disable-component-update",
        };
        var environment = await CoreWebView2Environment.CreateAsync(null, dataFolder, options);
        SharedEnvironment = environment;
        // Beim Anlegen seines Chromium-Fensters zeigt WebView2 kurz (≈50 ms) eine einfarbige Fläche, ehe das erste Bild steht – DefaultBackgroundColor
        // hilft dagegen nicht, und ein unsichtbares Control (Visible = false) verschiebt die Fläche nur auf den Moment des Einblendens, weil
        // Chromium unsichtbar nichts zeichnet. Deshalb liegt bis zum ersten gezeichneten Bild der Leerseite eine gleichfarbige Abdeckung
        // darüber (Geschwister-Control über dem WebView; die Leerseite meldet „painted“ nach zwei Animationsframes). Gemessen 21.09.2026
        // per PrintWindow-Zeitleiste: ohne Abdeckung ≈300 ms nach dem Fenster eine Fläche für 30–60 ms.
        startCover = new Panel { Dock = webView.Dock, Bounds = webView.Bounds, BackColor = webView.BackColor };
        webView.Parent?.Controls.Add(startCover);
        startCover.BringToFront();
        await webView.EnsureCoreWebView2Async(environment);

        var core = webView.CoreWebView2;
        core.Settings.AreDevToolsEnabled = false;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.IsGeneralAutofillEnabled = false;
        core.Settings.IsPasswordAutosaveEnabled = false;
        core.Settings.IsReputationCheckingRequired = false; // SmartScreen aus: PDFlight zeigt nur lokale Dateien, nichts geht zur Prüfung an Microsoft
        core.Settings.AreDefaultScriptDialogsEnabled = false; // s. Core_ScriptDialogOpening
        core.ScriptDialogOpening += Core_ScriptDialogOpening;
        core.Profile.PreferredColorScheme = darkScheme ? CoreWebView2PreferredColorScheme.Dark : CoreWebView2PreferredColorScheme.Light; // Anzeigehintergrund (Einstellungen), nie „Auto“ – sonst hinge er am Windows-Design
        core.Settings.HiddenPdfToolbarItems = HiddenToolbarItems(saveButtonVisible);
        core.DownloadStarting += Core_DownloadStarting;
        core.AddWebResourceRequestedFilter("https://" + VirtualHost + "/*", CoreWebView2WebResourceContext.All);
        // Optionale Adobe-Ansicht (s. AdobeEmbed): die Seite mit dem SDK unter ihrem eigenen Ursprung, Adobes Nutzungsprotokoll aus allen
        // Frames abgefangen. Ohne Zustimmung und Knopfdruck wird die Seite nie geladen – bis dahin geht nichts zu Adobe.
        if (Directory.Exists(AdobeEmbed.PageFolder)) { core.SetVirtualHostNameToFolderMapping(AdobeEmbed.Host, AdobeEmbed.PageFolder, CoreWebView2HostResourceAccessKind.Allow); }
        core.AddWebResourceRequestedFilter(AdobeEmbed.BlockedLogUrl + "*", CoreWebView2WebResourceContext.All, CoreWebView2WebResourceRequestSourceKinds.All);
        core.WebResourceRequested += Core_WebResourceRequested;
        core.NavigationStarting += Core_NavigationStarting;
        core.NavigationCompleted += Core_NavigationCompleted;
        core.NewWindowRequested += Core_NewWindowRequested;
        core.WebMessageReceived += Core_WebMessageReceived; // Drop-Meldungen der Leerseite
        webView.AllowExternalDrop = true; // Drops aufs Dokument landen als file://-Navigation in Core_NavigationStarting

        IsReady = true;
        ShowEmptyPage();
        WarmUpAutomation(); // Chromiums Accessibility-Baum schon jetzt aktivieren, nicht erst beim ersten Strg+Entf
    }

    private void Core_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (startCover != null) { RemoveStartCoverLater(); }
        twoPageActive = false; // jedes Dokumentladen startet im einseitigen Viewer-Standard
        documentLoaded?.TrySetResult();
        if (AdobeActive) { PostAdobeFile(); } else { RequestZoomUpdate(); ArmFormEventsLater(); } // die Adobe-Seite steht: jetzt die Datei hinüberreichen
    }

    /// <summary>JavaScript-Dialoge des WebView: Die Adobe-Seite meldet bei ungespeicherten Anmerkungen ein beforeunload, das Chromium als
    /// „Website verlassen?“ zeigte und die Navigation anhielt (geprüft 26.09.2026) – die Rückfrage stellt PDFlight vorher selbst, also hier
    /// durchwinken. alert/confirm/prompt kommen in PDFlights Seiten nicht vor; auch sie werden bestätigt, statt das WebView anzuhalten.</summary>
    private void Core_ScriptDialogOpening(object? sender, CoreWebView2ScriptDialogOpeningEventArgs e)
    {
        e.Accept();
    }

    /// <summary>Drop auf die Leerseite: deren Skript meldet die Dateien per postMessageWithAdditionalObjects mit echten Pfaden.</summary>
    private void Core_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        if (AdobeActive && (e.Source ?? string.Empty).StartsWith("https://" + AdobeEmbed.Host + "/", StringComparison.OrdinalIgnoreCase))
        {
            HandleAdobeMessage(e.WebMessageAsJson); // JSON-Objekte – TryGetWebMessageAsString würde daran scheitern
            return;
        }
        if (e.AdditionalObjects == null)
        {
            if (e.TryGetWebMessageAsString() == "painted") { RemoveStartCover(); } // erstes Bild der Leerseite steht
            return;
        }
        foreach (var item in e.AdditionalObjects)
        {
            if (item is CoreWebView2File file && !string.IsNullOrEmpty(file.Path)
                && file.Path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) && File.Exists(file.Path))
            {
                RaisePdfFileDropped(file.Path); // jede Datei einzeln melden — der Empfänger verteilt auf Instanzen
            }
        }
    }

    /// <summary>Manche Drops und Links landen als "neues Fenster": PDFs übernehmen, Web-Links in den Standardbrowser.</summary>
    private void Core_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        var uri = e.Uri ?? string.Empty;
        if (uri.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var path = new Uri(uri).LocalPath;
                if (path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) && File.Exists(path)) { RaisePdfFileDropped(path); }
            }
            catch (UriFormatException) { }
        }
        else { OpenInBrowser(uri); }
    }

    private void RaisePdfFileDropped(string path)
    {
        // Die Save-Schaltfläche des Viewers navigiert nach dem Schreiben zur gespeicherten Datei (Chromium öffnet sie) – das ist kein Drop:
        // PDFlight öffnete sonst seine eigene Temp-Datei in einem zweiten Fenster (geprüft 26.09.2026)
        if (path.StartsWith(FormTempFolder, StringComparison.OrdinalIgnoreCase)) { return; }
        // nicht innerhalb eines WebView2-Ereignisses neu navigieren → entkoppeln
        webView.BeginInvoke(new Action(() => PdfFileDropped?.Invoke(this, path)));
    }

    /// <summary>Lässt nur eigene Inhalte zu; abgelegte PDF-Dateien werden gemeldet, Web-Links im Standardbrowser geöffnet.</summary>
    private void Core_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        var uri = e.Uri ?? string.Empty;
        if (uri.StartsWith("https://" + VirtualHost + "/", StringComparison.OrdinalIgnoreCase)
            || uri.StartsWith("about:", StringComparison.OrdinalIgnoreCase)
            || uri.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            || (AdobeActive && uri.StartsWith("https://" + AdobeEmbed.Host + "/", StringComparison.OrdinalIgnoreCase))) { return; } // Adobes iframes sind keine Hauptnavigation

        e.Cancel = true;
        if (uri.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var path = new Uri(uri).LocalPath;
                if (path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) && File.Exists(path)) { RaisePdfFileDropped(path); }
            }
            catch (UriFormatException) { }
        }
        else if (uri.StartsWith("http:", StringComparison.OrdinalIgnoreCase) || uri.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
        {
            OpenInBrowser(uri);
        }
    }

    private static void OpenInBrowser(string uri)
    {
        if (string.IsNullOrEmpty(uri) || !(uri.StartsWith("http:", StringComparison.OrdinalIgnoreCase) || uri.StartsWith("https:", StringComparison.OrdinalIgnoreCase))) { return; }
        try { Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true }); }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException) { }
    }

    private bool saveButtonVisible;

    /// <summary>Save/SaveAs der Viewer-Leiste nur bei PDFs mit Formularfeldern (Wunsch vom 26.09.2026): Die Save-Schaltfläche ist der einzige
    /// Weg, ausgefüllte Felder aus dem Viewer zu holen (s. Abschnitt Formularfelder; ausgeblendet fehlt sie auch für UIA, Strg+S wirkt
    /// dann nicht – geprüft 26.09.2026). Sonst bleibt die Leiste wie früher ohne sie – Speichern übernimmt PDFlight selbst. Die Einstellung
    /// greift erst bei der nächsten Navigation, deshalb setzt das Hauptfenster sie vor <see cref="Load"/>.</summary>
    public void SetSaveButtonVisible(bool visible)
    {
        saveButtonVisible = visible;
        if (IsReady && webView.CoreWebView2.Settings.HiddenPdfToolbarItems != HiddenToolbarItems(visible)) { webView.CoreWebView2.Settings.HiddenPdfToolbarItems = HiddenToolbarItems(visible); }
    }

    private static CoreWebView2PdfToolbarItems HiddenToolbarItems(bool saveVisible) =>
        CoreWebView2PdfToolbarItems.FullScreen // der Chromium-Vollbildmodus ist im Host-Fenster kaum beendbar → PDFlight bietet stattdessen F11
        | CoreWebView2PdfToolbarItems.Print    // Drucken sitzt in der Hauptmenüleiste — die Viewer-Leiste bleibt den Ansichts-Funktionen vorbehalten
        | (saveVisible ? CoreWebView2PdfToolbarItems.None : CoreWebView2PdfToolbarItems.Save | CoreWebView2PdfToolbarItems.SaveAs);

    /// <summary>Lädt die PDF-Datei in den Speicher und zeigt sie an; die Datei bleibt danach ungesperrt.
    /// Mit page &gt; 0 springt der Viewer direkt zu dieser Seite (z.B. nach dem Löschen von Seiten).</summary>
    public void Load(string filePath, int page = 0, int zoomPercent = 0)
    {
        documentLoaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
        ResetAdobe(); // ein Dokumentladen beendet die Adobe-Ansicht (die Rückfrage bei ungespeicherten Anmerkungen stellt das Hauptfenster vorher)
        ResetForm();  // und verwirft Formulareingaben des Viewers (das Hauptfenster hat sie vorher gespeichert oder nachgefragt)
        currentBytes = File.ReadAllBytes(filePath); // wirft IOException etc. → behandelt der Aufrufer
        // Öffnen-Parameter des Chromium-Viewers: page (1-basiert) und zoom (Prozent) – beide erst nach dem vollständigen Laden wirksam
        var parameters = new List<string>();
        if (page > 0) { parameters.Add("page=" + page); }
        if (zoomPercent > 0) { parameters.Add("zoom=" + zoomPercent); pendingZoomPercent = zoomPercent; }
        var fragment = parameters.Count > 0 ? "#" + string.Join("&", parameters) : string.Empty;
        webView.CoreWebView2.Navigate($"https://{VirtualHost}/{Uri.EscapeDataString(Path.GetFileName(filePath))}?t={DateTime.Now.Ticks}{fragment}");
    }

    /// <summary>Springt nach dem Laden zur gemerkten Seite – aber nur, wenn der Viewer dann noch auf Seite 1 steht (der Nutzer also noch
    /// nicht geblättert hat) und PDFlight im Vordergrund ist. Ein „#page=“-Fragment beim Laden wendet Chromium erst nach dem vollständigen
    /// Laden an; bei großen Dateien zeigt er vorher Seite 1, und wer da schon scrollt, wird später überraschend weggeholt (19.09.2026).
    /// Eine Fragment-Navigation im geladenen Dokument ignoriert der Viewer, und UIA-SetValue im Seitenfeld ändert nur den Text –
    /// es bleibt der Weg über das Seitenfeld wie bei Strg+G: Fokus hinein, Zahl tippen, Enter.</summary>
    public async Task GoToPageIfUntouchedAsync(int page)
    {
        if (page <= 1 || documentLoaded is not { } loaded || !IsReady) { return; }
        await loaded.Task;
        for (var attempt = 0; attempt < 50; attempt++) // bis 10 s auf das Seitenfeld warten (großes Dokument, kalte Laufzeit)
        {
            if (documentLoaded != loaded || currentBytes == null) { return; } // inzwischen ein anderes Dokument
            var chromium = FindDescendant(webView.Handle, "Chrome_RenderWidgetHostHWND", 4);
            var current = chromium == IntPtr.Zero ? 0 : await Task.Run(() => ReadPageNumber(chromium));
            if (current > 0)
            {
                if (current != 1 || documentLoaded != loaded) { return; } // schon geblättert oder anderes Dokument
                if (webView.FindForm() is not { } form || NativeMethods.GetForegroundWindow() != form.Handle) { return; } // nicht den Nutzer aus einem anderen Programm holen
                var edit = await Task.Run(() => FindPageNumberEdit(chromium));
                if (edit == null || documentLoaded != loaded) { return; }
                try
                {
                    webView.Focus();
                    edit.SetFocus();
                    if (edit.TryGetCurrentPattern(System.Windows.Automation.TextPattern.Pattern, out var text)) { ((System.Windows.Automation.TextPattern)text).DocumentRange.Select(); }
                    SendKeys.SendWait(page.ToString(System.Globalization.CultureInfo.InvariantCulture) + "{ENTER}");
                }
                catch (Exception ex) when (ex is System.Windows.Automation.ElementNotAvailableException or System.Runtime.InteropServices.COMException or InvalidOperationException) { }
                return;
            }
            await Task.Delay(200);
        }
    }

    public void CloseDocument()
    {
        currentBytes = null;
        ResetAdobe();
        ResetForm();
        if (IsReady) { ShowEmptyPage(); }
    }

    /// <summary>Öffnet die Druckvorschau des Viewers — dieselbe wie bei Strg+P.</summary>
    public void ShowPrintDialog()
    {
        if (IsReady) { webView.CoreWebView2.ShowPrintUI(CoreWebView2PrintDialogKind.Browser); }
    }

    private Panel? startCover; // deckt das WebView beim Start ab, bis die Leerseite ihr erstes Bild gezeichnet hat (s. InitializeAsync)

    private void RemoveStartCover() { startCover?.Dispose(); startCover = null; }

    /// <summary>Rückfall für den Start mit Dokument: Die Leerseite wird sofort ersetzt, ihr „painted“ kommt nie – dann fällt die
    /// Abdeckung kurz nach dem ersten NavigationCompleted (Chromium zeichnet längst, die Fläche vom Anlegen des Fensters ist vorbei).</summary>
    private async void RemoveStartCoverLater()
    {
        await Task.Delay(250);
        RemoveStartCover();
    }
    private TaskCompletionSource? documentLoaded; // wird mit jedem Load neu gesetzt und bei NavigationCompleted erfüllt

    /// <summary>Druckt das angezeigte Dokument ohne Dialog (Start mit /print): wartet das Laden ab, gibt dem PDF-Viewer
    /// noch einen Moment zum Rendern und druckt dann auf dem Standarddrucker bzw. dem genannten Drucker.</summary>
    public async Task<CoreWebView2PrintStatus> PrintAsync(string? printerName)
    {
        if (!IsReady || currentBytes == null) { return CoreWebView2PrintStatus.OtherError; }
        if (documentLoaded != null) { await documentLoaded.Task; }
        await Task.Delay(1500);
        var settings = webView.CoreWebView2.Environment.CreatePrintSettings();
        if (!string.IsNullOrEmpty(printerName)) { settings.PrinterName = printerName; }
        return await webView.CoreWebView2.PrintAsync(settings);
    }

    /// <summary>Dreht die Viewer-Ansicht um 90° (nur Anzeige, die Datei bleibt unverändert): drückt den
    /// Drehen-Button der Viewer-Toolbar per UI Automation. Dessen Kürzel Strg+] ist auf deutschen
    /// Tastaturen unerreichbar, und über die WebView2-API bzw. das DevTools-Protokoll ist der Viewer
    /// (ein isoliertes Gast-Dokument) nicht ansprechbar — der Automation-Baum schon, mit den gleichen
    /// Regeln wie bei der Seitenabfrage: Hintergrund-Task am Chromium-Kindfenster.</summary>
    public void RotateView(bool clockwise)
    {
        InvokeViewerButton("rotate", clockwise ? 1 : 3); // der Viewer kennt nur rechtsherum — dreimal rechts ist einmal links
    }

    /// <summary>Blendet die Inhalte-Leiste am linken Rand ein oder aus.</summary>
    public void ToggleContents()
    {
        InvokeViewerButton("contents", 1);
    }

    /// <summary>Passt die Seite an die Fensterbreite an (bzw. zurück auf ganze Seite — der Button wechselt).</summary>
    public void FitToWidth()
    {
        InvokeViewerButton("pagefit", 1);
    }

    // Der Viewer verrät sein aktuelles Layout nicht zuverlässig (IsSelected der Radio-Einträge ist
    // nicht belastbar) — deshalb führt PDFlight den Zustand selbst; jedes Dokumentladen setzt ihn zurück.
    private bool twoPageActive;

    /// <summary>Schaltet zwischen ein- und zweiseitigem Layout um (Strg+Umschalt+A):
    /// klappt das Seitenansicht-Menü per UIA auf und wählt den jeweils anderen Eintrag.</summary>
    public void ToggleLayout()
    {
        if (!IsReady || currentBytes == null || AdobeActive) { return; }
        var chromium = FindDescendant(webView.Handle, "Chrome_RenderWidgetHostHWND", 4);
        if (chromium == IntPtr.Zero) { return; }
        var wantTwoPages = !twoPageActive;
        _ = Task.Run(() =>
        {
            // Zustand erst nach dem vollzogenen Klick übernehmen — sonst geriete die eigene
            // Buchführung aus dem Tritt, wenn der Klick fehlschlägt (z.B. Baum noch nicht bereit)
            if (SelectLayout(chromium, wantTwoPages)) { twoPageActive = wantTwoPages; }
        });
    }

    internal static string LayoutDiag = "nicht aufgerufen"; // nur für die Test-Diagnose

    private static bool SelectLayout(IntPtr chromiumHandle, bool twoPage)
    {
        try
        {
            LayoutDiag = "gestartet";
            var root = System.Windows.Automation.AutomationElement.FromHandle(chromiumHandle);
            System.Windows.Automation.AutomationElement? layouts = null;
            for (var i = 0; i < 40 && layouts == null; i++) // beim Kaltstart braucht Chromiums Accessibility-Baum mehrere Sekunden
            {
                layouts = root.FindFirst(System.Windows.Automation.TreeScope.Descendants,
                    new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.AutomationIdProperty, "layouts"));
                if (layouts == null) { System.Threading.Thread.Sleep(200); }
            }
            if (layouts == null) { LayoutDiag = "layouts-Button nicht gefunden"; return false; }
            // Dieses Menü lässt sich nur wie von Menschenhand bedienen: UIA-Invoke/Select verpuffen,
            // Tastatur-Auswahlen werden beim Schließen wieder verworfen (Vorschau-Semantik), und auch
            // ExpandCollapse zickt beim zweiten Mal. Also beide Schritte als echte Mausklicks —
            // Cursor sichern, Button und dann Eintrag anklicken, Cursor zurücksetzen.
            _ = GetCursorPos(out var before);
            try
            {
                // direkt nach einem Dokumentladen reagiert die frische Toolbar noch nicht immer auf
                // den ersten Klick — deshalb das Öffnen bei Bedarf wiederholen
                System.Windows.Automation.AutomationElement? target = null;
                for (var attempt = 0; attempt < 3 && target == null; attempt++)
                {
                    ClickCenter(layouts.Current.BoundingRectangle); // Menü öffnen
                    System.Threading.Thread.Sleep(500);             // bis die Einträge bedienbar sind
                    for (var i = 0; i < 6 && target == null; i++)   // die Radio-Einträge existieren erst im offenen Menü
                    {
                        target = FindLayoutRadio(root, twoPage ? "id1" : "id0");
                        if (target == null) { System.Threading.Thread.Sleep(200); }
                    }
                }
                if (target == null) { LayoutDiag = "Menüeintrag nicht gefunden"; return false; }
                ClickCenter(target.Current.BoundingRectangle); // übernimmt die Auswahl
                System.Threading.Thread.Sleep(250);
                // das Menü bleibt nach der Auswahl mitunter offen — ein Klick auf die leere
                // Toolbar-Fläche daneben schließt es, ohne etwas auszulösen
                var buttonRect = layouts.Current.BoundingRectangle;
                ClickCenter(new System.Windows.Rect(buttonRect.Right + 30, buttonRect.Y, buttonRect.Height, buttonRect.Height));
                System.Threading.Thread.Sleep(100);
                LayoutDiag = "Klick auf " + (twoPage ? "id1" : "id0");
                return true;
            }
            finally { _ = SetCursorPos(before.X, before.Y); }
        }
        catch (Exception ex) when (ex is System.Windows.Automation.ElementNotAvailableException
            or System.Runtime.InteropServices.COMException or InvalidOperationException)
        {
            LayoutDiag = ex.GetType().Name + ": " + ex.Message;
            return false; // reine Komfortfunktion — schlägt sie fehl, bleibt einfach das bisherige Layout
        }
    }

    private static System.Windows.Automation.AutomationElement FindLayoutRadio(System.Windows.Automation.AutomationElement root, string automationId)
    {
        return root.FindFirst(System.Windows.Automation.TreeScope.Descendants, new System.Windows.Automation.AndCondition(
            new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.RadioButton),
            new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.AutomationIdProperty, automationId)));
    }

    /// <summary>Drückt einen Button der Viewer-Toolbar per UI Automation, adressiert über die HTML-id
    /// (wird zur sprachunabhängigen AutomationId). Gleiche Regeln wie die Seitenabfrage:
    /// Hintergrund-Task am Chromium-Kindfenster.</summary>
    private void InvokeViewerButton(string automationId, int clicks)
    {
        if (!IsReady || currentBytes == null || AdobeActive) { return; }
        var chromium = FindDescendant(webView.Handle, "Chrome_RenderWidgetHostHWND", 4);
        if (chromium == IntPtr.Zero) { return; }
        _ = Task.Run(() => InvokeButton(chromium, automationId, clicks));
    }

    private static void InvokeButton(IntPtr chromiumHandle, string automationId, int clicks)
    {
        try
        {
            var root = System.Windows.Automation.AutomationElement.FromHandle(chromiumHandle);
            var button = root.FindFirst(System.Windows.Automation.TreeScope.Descendants,
                new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.AutomationIdProperty, automationId));
            if (button == null) { return; }
            for (var i = 0; i < clicks; i++)
            {
                if (button.TryGetCurrentPattern(System.Windows.Automation.InvokePattern.Pattern, out var invoke))
                {
                    ((System.Windows.Automation.InvokePattern)invoke).Invoke();
                }
                else if (button.TryGetCurrentPattern(System.Windows.Automation.ExpandCollapsePattern.Pattern, out var expand))
                {
                    // der Inhalte-Button ist ein Auf-/Zuklapper — je nach Zustand öffnen oder schließen
                    var pattern = (System.Windows.Automation.ExpandCollapsePattern)expand;
                    if (pattern.Current.ExpandCollapseState == System.Windows.Automation.ExpandCollapseState.Expanded) { pattern.Collapse(); }
                    else { pattern.Expand(); }
                }
            }
        }
        catch (Exception ex) when (ex is System.Windows.Automation.ElementNotAvailableException
            or System.Runtime.InteropServices.COMException or InvalidOperationException)
        {
            // reine Komfortfunktionen — schlägt der Klick fehl, ändert sich die Ansicht einfach nicht
        }
    }

    // ------------------------------------------------------------------ Zoomstufe

    // Der Viewer verrät seine Zoomstufe nicht per API. Aber: Die Zoom-Buttons, Strg+Mausrad und die Anpassen-Buttons
    // füttern eine ARIA-Live-Region („Vergrößert, 110 Prozent“), und dabei feuert das Chromium-Fenster jedes Mal das
    // WinEvent EVENT_OBJECT_LIVEREGIONCHANGED – das ist der Auslöser ohne Timer. Tastaturzoom (Strg+Plus/Minus/0)
    // meldet keine Live-Region, den sieht das Hauptfenster selbst im KeyDown und ruft RequestZoomUpdate direkt auf.
    // Der Wert selbst kommt aus der UIA-Geometrie des ersten Seitenelements gegen die Seitengröße aus der PDF-Datei –
    // über die Fläche, damit die gedrehte Ansicht dasselbe Ergebnis liefert.
    private const uint EVENT_OBJECT_LIVEREGIONCHANGED = 0x8019;
    private const uint WINEVENT_OUTOFCONTEXT = 0x0000;
    private const uint WINEVENT_SKIPOWNPROCESS = 0x0002;
    private NativeMethods.WinEventProc? zoomHookProc; // Referenz halten, sonst räumt der GC den Callback ab
    private nint zoomHook;
    private nint zoomHookWindow;   // das Chromium-Fenster, auf das der Hook gefiltert ist
    private double pageAreaPt;     // Fläche der ersten Seite in Punkt² (Referenz für 100 %); 0 = keine Anzeige
    private int zoomPercent;       // zuletzt gemeldete Zoomstufe (0 = unbekannt)
    private int zoomRequests;      // laufende Nummer, damit nur die jüngste Abfrage meldet
    private int pendingZoomPercent; // Zoom-Parameter des letzten Load: Chromium wendet ihn erst nach dem Laden an, die Messung wartet darauf

    /// <summary>Zoomstufe des Viewers in Prozent – gemeldet auf dem UI-Thread nach jeder erkannten Änderung.</summary>
    public event EventHandler<int>? ZoomChanged;

    /// <summary>Zuletzt gemessene Zoomstufe des Chromium-Viewers in Prozent (0 = unbekannt) – Startwert für die Adobe-Ansicht.</summary>
    public int ZoomPercent => zoomPercent;

    /// <summary>Größe der ersten Seite in Punkt (aus der PDF-Datei) – die Referenz, aus der die Zoomstufe berechnet wird;
    /// vor dem Laden setzen. 0 schaltet die Anzeige ab (z.B. verschlüsselte Datei).</summary>
    public void SetPageSize(double widthPt, double heightPt)
    {
        pageAreaPt = widthPt * heightPt;
        zoomPercent = 0;
    }

    /// <summary>Liest die Zoomstufe neu – nach einem Live-Region-Ereignis des Viewers, nach dem Laden oder nach einem
    /// Tastaturzoom. Das Layout ist beim Auslöser noch nicht fertig, deshalb fasst ein Hintergrund-Task kurz nach, bis
    /// sich der Wert geändert hat (höchstens ~1 s): ein einmaliges Nachfassen je Auslöser, kein laufender Timer.</summary>
    public void RequestZoomUpdate()
    {
        if (!IsReady || currentBytes == null || pageAreaPt <= 0 || AdobeActive) { return; } // in der Adobe-Ansicht gibt es keine Chromium-Zoomstufe
        var chromium = FindDescendant(webView.Handle, "Chrome_RenderWidgetHostHWND", 4);
        if (chromium == IntPtr.Zero) { return; }
        EnsureZoomHook(chromium);
        var previous = zoomPercent;
        var request = ++zoomRequests;
        var dpiScale = webView.DeviceDpi / 72.0;                 // Punkt → Gerätepixel bei 100 %
        var referenceArea = pageAreaPt * dpiScale * dpiScale;
        var expected = pendingZoomPercent; // nach Load mit Zoom-Parameter: bis zu 3 s nachfassen, bis der Viewer ihn angewendet hat (sonst bliebe „100 %“ stehen)
        pendingZoomPercent = 0;
        _ = Task.Run(() =>
        {
            var percent = 0;
            for (var i = 0; i < (expected > 0 ? 60 : 20); i++)
            {
                percent = ReadZoomPercent(chromium, referenceArea);
                if (percent > 0 && percent != previous && (expected == 0 || Math.Abs(percent - expected) <= 2)) { break; }
                if (request != zoomRequests) { return; } // ein neuerer Auslöser übernimmt
                Thread.Sleep(50);
            }
            if (percent <= 0 || percent == previous) { return; }
            webView.BeginInvoke(() =>
            {
                if (request != zoomRequests) { return; }
                zoomPercent = percent;
                ZoomChanged?.Invoke(this, percent);
            });
        });
    }

    /// <summary>Hängt den WinEvent-Hook an den Browserprozess – nur für das Live-Region-Ereignis und nur für das
    /// Chromium-Fenster des Viewers (nach einem Fensterwechsel neu).</summary>
    private void EnsureZoomHook(IntPtr chromium)
    {
        if (chromium == zoomHookWindow && zoomHook != 0) { return; }
        ReleaseZoomHook();
        NativeMethods.GetWindowThreadProcessId(chromium, out var browserProcess);
        zoomHookProc ??= (hook, eventType, hwnd, idObject, idChild, thread, time) =>
        {
            if (hwnd == zoomHookWindow) { RequestZoomUpdate(); } // läuft auf dem UI-Thread (Nachrichtenschleife des Hook-Threads)
        };
        zoomHook = NativeMethods.SetWinEventHook(EVENT_OBJECT_LIVEREGIONCHANGED, EVENT_OBJECT_LIVEREGIONCHANGED, 0, zoomHookProc, browserProcess, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);
        zoomHookWindow = zoomHook != 0 ? chromium : 0;
        formHookProc ??= FormEventCallback; // dieselben Regeln: Referenz halten, nur der Browserprozess, gefiltert auf das Chromium-Fenster
        foreach (var eventType in new[] { EVENT_OBJECT_SELECTION, EVENT_OBJECT_STATECHANGE, EVENT_OBJECT_VALUECHANGE }) // drei einzelne Hooks – ein Bereich schlösse LOCATIONCHANGE (Dauerfeuer) ein
        {
            var hook = NativeMethods.SetWinEventHook(eventType, eventType, 0, formHookProc, browserProcess, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);
            if (hook != 0) { formHooks.Add(hook); }
        }
        // Der „Speichern unter“-Dialog der Save-Schaltfläche (s. HandleSaveDialog) kommt nicht aus dem Browserprozess – Chromium zeigt
        // Dateidialoge in einem eigenen Hilfsprozess –, deshalb ohne Prozessfilter; der Rückruf prüft den Besitzer des Dialogs
        dialogHookProc ??= DialogEventCallback;
        var dialogHook = NativeMethods.SetWinEventHook(EVENT_SYSTEM_DIALOGSTART, EVENT_SYSTEM_DIALOGSTART, 0, dialogHookProc, 0, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);
        if (dialogHook != 0) { formHooks.Add(dialogHook); }
    }

    /// <summary>Löst die WinEvent-Hooks (beim Beenden und vor dem Neuanlegen).</summary>
    public void ReleaseZoomHook()
    {
        if (zoomHook != 0) { NativeMethods.UnhookWinEvent(zoomHook); }
        zoomHook = 0;
        zoomHookWindow = 0;
        foreach (var hook in formHooks) { NativeMethods.UnhookWinEvent(hook); }
        formHooks.Clear();
        if (createHook != 0) { NativeMethods.UnhookWinEvent(createHook); createHook = 0; }
    }

    /// <summary>Zoomstufe aus der Fläche des ersten Seitenelements (erste Gruppe unter dem Dokument-Element) gegen die
    /// Referenzfläche bei 100 %; 0, wenn das Element (noch) nicht im Baum steht.</summary>
    private static int ReadZoomPercent(IntPtr chromiumHandle, double referenceArea)
    {
        try
        {
            var root = System.Windows.Automation.AutomationElement.FromHandle(chromiumHandle);
            // Dokument-Elemente sind geschachtelt (Viewer-Seite → „PDF Document“ → das eigentliche PDF); das erste mit
            // einer Gruppe als Kind ist das PDF, die Gruppe seine erste Seite
            System.Windows.Automation.AutomationElement? page = null;
            foreach (System.Windows.Automation.AutomationElement document in root.FindAll(System.Windows.Automation.TreeScope.Descendants,
                new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Document)))
            {
                page = document.FindFirst(System.Windows.Automation.TreeScope.Children,
                    new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Group));
                if (page != null) { break; }
            }
            if (page == null) { return 0; }
            var rect = page.Current.BoundingRectangle;
            if (rect.Width <= 0 || rect.Height <= 0) { return 0; }
            return (int)Math.Round(Math.Sqrt(rect.Width * rect.Height / referenceArea) * 100);
        }
        catch (Exception ex) when (ex is System.Windows.Automation.ElementNotAvailableException
            or System.Runtime.InteropServices.COMException or InvalidOperationException) { return 0; }
    }

    // ------------------------------------------------------------------ Aktuelle Seite per UI Automation

    /// <summary>Aktuelle Seite laut dem Seitenzahl-Feld der Viewer-Toolbar, per UI Automation gelesen —
    /// die WebView2-API selbst verrät die Seite nicht, aber Chromium exponiert seine Oberfläche als
    /// Automation-Baum. 0, wenn das Feld nicht (rechtzeitig) gelesen werden kann.
    /// Die Abfrage läuft mit Zeitbudget im Hintergrund und setzt am Chromium-Kindfenster an: Es gehört
    /// einem fremden Thread — eine Abfrage am eigenen WebView-Fenster würde den wartenden UI-Thread
    /// per WM_GETOBJECT anfragen und sich damit selbst blockieren.</summary>
    public int TryGetCurrentPage()
    {
        if (webView.CoreWebView2 == null) { return 0; }
        if (AdobeActive) { return AdobePage; } // die Adobe-Seite meldet ihre Seite selbst; ihr Seitenfeld ist nicht das des Chromium-Viewers
        var chromium = FindDescendant(webView.Handle, "Chrome_RenderWidgetHostHWND", 4);
        if (chromium == IntPtr.Zero) { return 0; }
        var task = Task.Run(() => ReadPageNumber(chromium));
        return task.Wait(TimeSpan.FromMilliseconds(1500)) ? task.Result : 0; // lieber ohne Vorbelegung als eingefroren
    }

    /// <summary>Das Seitenzahl-Feld der Viewer-Toolbar; null, wenn es (noch) nicht im Baum steht.</summary>
    private static System.Windows.Automation.AutomationElement FindPageNumberEdit(IntPtr chromiumHandle)
    {
        var root = System.Windows.Automation.AutomationElement.FromHandle(chromiumHandle);
        // FindFirst bricht beim ersten Treffer ab; die Toolbar steht im Baum vor dem Dokumentinhalt
        var edit = root.FindFirst(System.Windows.Automation.TreeScope.Descendants, new System.Windows.Automation.AndCondition(
            new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Edit),
            new System.Windows.Automation.OrCondition( // Feldname je nach Viewer-Sprache (de/en/fr/es)
                new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.NameProperty, "Seitenzahl"),
                new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.NameProperty, "Page number"),
                new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.NameProperty, "Numéro de page"),
                new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.NameProperty, "Número de página"))));
        return edit ?? root.FindFirst(System.Windows.Automation.TreeScope.Descendants, // zur Sicherheit, falls das Feld einmal anders heißt
            new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Edit));
    }

    private static int ReadPageNumber(IntPtr chromiumHandle)
    {
        try
        {
            var edit = FindPageNumberEdit(chromiumHandle);
            if (edit != null && edit.TryGetCurrentPattern(System.Windows.Automation.ValuePattern.Pattern, out var pattern)
                && int.TryParse(((System.Windows.Automation.ValuePattern)pattern).Current.Value, out var page) && page >= 1)
            {
                return page;
            }
        }
        catch (Exception ex) when (ex is System.Windows.Automation.ElementNotAvailableException or System.Runtime.InteropServices.COMException or InvalidOperationException) { }
        return 0;
    }

    /// <summary>„Gehe zu Seite": setzt den Eingabefokus in das Seitenzahl-Feld der Viewer-Toolbar —
    /// Zahl eintippen und Enter springt (die native Sprungfunktion des Viewers).</summary>
    public void FocusPageField()
    {
        if (!IsReady || currentBytes == null || AdobeActive) { return; }
        var chromium = FindDescendant(webView.Handle, "Chrome_RenderWidgetHostHWND", 4);
        if (chromium == IntPtr.Zero) { return; }
        webView.Focus(); // Windows-Fokus zum Viewer, sonst landet die Eingabe woanders
        _ = Task.Run(() =>
        {
            try
            {
                var edit = FindPageNumberEdit(chromium);
                if (edit == null) { return; }
                edit.SetFocus();
                if (edit.TryGetCurrentPattern(System.Windows.Automation.TextPattern.Pattern, out var text))
                {
                    // vorhandene Seitenzahl markieren, damit die getippte Zahl sie ersetzt statt anzuhängen
                    ((System.Windows.Automation.TextPattern)text).DocumentRange.Select();
                }
            }
            catch (Exception ex) when (ex is System.Windows.Automation.ElementNotAvailableException
                or System.Runtime.InteropServices.COMException or InvalidOperationException) { }
        });
    }

    /// <summary>Stößt Chromiums Accessibility-Modus einmalig an (bleibt danach aktiv), damit die erste
    /// echte Seitenabfrage nicht auf den Aufbau des kompletten Baums warten muss.</summary>
    private void WarmUpAutomation()
    {
        var chromium = FindDescendant(webView.Handle, "Chrome_RenderWidgetHostHWND", 4);
        if (chromium == IntPtr.Zero) { return; }
        _ = Task.Run(() => ReadPageNumber(chromium));
    }

    /// <summary>Sucht das Chromium-Eingabefenster unterhalb des WebView-Handles.</summary>
    private static unsafe IntPtr FindDescendant(IntPtr parent, string className, int depth)
    {
        if (depth == 0) { return IntPtr.Zero; }
        var buffer = stackalloc char[64]; // vor der Schleife — CA2014
        for (var child = FindWindowEx(parent, IntPtr.Zero, null, null); child != IntPtr.Zero; child = FindWindowEx(parent, child, null, null))
        {
            var length = GetClassName(child, buffer, 64);
            if (string.Equals(new string(buffer, 0, length), className, StringComparison.Ordinal)) { return child; }
            var descendant = FindDescendant(child, className, depth - 1);
            if (descendant != IntPtr.Zero) { return descendant; }
        }
        return IntPtr.Zero;
    }

    [System.Runtime.InteropServices.LibraryImport("user32.dll", EntryPoint = "FindWindowExW", StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf16)]
    private static partial IntPtr FindWindowEx(IntPtr parent, IntPtr after, string? className, string? windowName);

    [System.Runtime.InteropServices.LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static partial bool PostMessage(IntPtr hWnd, uint msg, nint wParam, nint lParam);

    private static void ClickCenter(System.Windows.Rect rect)
    {
        _ = SetCursorPos((int)(rect.X + rect.Width / 2), (int)(rect.Y + rect.Height / 2));
        MouseEvent(0x0002); // MOUSEEVENTF_LEFTDOWN
        MouseEvent(0x0004); // MOUSEEVENTF_LEFTUP
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    [System.Runtime.InteropServices.LibraryImport("user32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out POINT point);

    [System.Runtime.InteropServices.LibraryImport("user32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static partial bool SetCursorPos(int x, int y);

    [System.Runtime.InteropServices.LibraryImport("user32.dll", EntryPoint = "mouse_event")]
    private static partial void MouseEvent(uint flags, uint dx = 0, uint dy = 0, uint data = 0, nint extraInfo = 0);

    [System.Runtime.InteropServices.LibraryImport("user32.dll", EntryPoint = "GetClassNameW")]
    private static unsafe partial int GetClassName(IntPtr hWnd, char* buffer, int maxCount);

    // ------------------------------------------------------------------ Formularfelder des Chromium-Viewers

    // Der Chromium-Viewer füllt Formularfelder selbst aus (auch bei PDF/A – die Sperre kennt nur PDFlight), aber PDFlight erfuhr davon nichts:
    // Keine WebView2-API meldet Änderungen, und die Save-Schaltfläche des Viewers war ausgeblendet (Fehlerbericht 26.09.2026). Deshalb:
    // Erkennung über die MSAA-WinEvents VALUECHANGE/STATECHANGE/SELECTION des Chromium-Fensters (wie der Zoom-Hook), aufgelöst per
    // AccessibleObjectFromEvent – ein Feld gilt als Formularfeld, wenn es unter dem verschachtelten PDF-Dokument liegt (mindestens zwei
    // Dokument-Vorfahren) und nicht in der Viewer-Leiste (kein Toolbar-Vorfahr). Die Zahl der Dokument-Ebenen allein trennt nicht: Das
    // Seitenfeld der Leiste hat inzwischen ebenfalls zwei und meldet beim Blättern jede neue Seitenzahl als VALUECHANGE – PDFlight hielt
    // das für eine Formulareingabe (Fehlerbericht 26.09.2026; gemessen: Seitenfeld 2 Dokumente unter einer Toolbar, PDF-Felder 3–4
    // ohne). Speichern geht nur über die Save-Schaltfläche des
    // Viewers: Sie schreibt die PDF samt Feldwerten über einen „Speichern unter“-Dialog des Browserprozesses (chrome.fileSystem.chooseEntry –
    // kein Download, DownloadStarting bleibt stumm; geprüft 26.09.2026). PDFlight fängt den Dialog per WinEvent DIALOGSTART ab, trägt eine
    // Temp-Datei ein, drückt Speichern und schreibt die Temp-Datei in die angezeigte Datei (MainForm.WriteFormFile) – ob der Nutzer die
    // Schaltfläche drückt oder PDFlight sie per UIA drückt (SaveFormAsync).
    private const uint EVENT_SYSTEM_DIALOGSTART = 0x0010;
    private const uint EVENT_OBJECT_SELECTION = 0x8006;
    private const uint EVENT_OBJECT_STATECHANGE = 0x800A;
    private const uint EVENT_OBJECT_VALUECHANGE = 0x800E;
    private const int RoleDocument = 0x0F, RoleToolBar = 0x16, RoleList = 0x21, RoleListItem = 0x22, RoleText = 0x2A, RoleCheckButton = 0x2C, RoleRadioButton = 0x2D, RoleComboBox = 0x2E; // ROLE_SYSTEM_*
    private readonly List<nint> formHooks = [];
    private NativeMethods.WinEventProc? formHookProc;
    private NativeMethods.WinEventProc? dialogHookProc;
    private bool formEventsArmed;   // erst kurz nach dem Laden – Ereignisse des Seitenaufbaus zählen nicht
    private int formCheckRunning;   // höchstens eine Auflösung gleichzeitig (STATECHANGE kommt gehäuft)
    private TaskCompletionSource<string?>? formSave; // wartet auf die Temp-Datei, die SaveFormAsync über die Save-Schaltfläche angestoßen hat
    private TaskCompletionSource<bool>? formDialogSeen; // der „Speichern unter“-Dialog ist erschienen (SaveFormAsync wartet sonst nicht lange)

    /// <summary>Ergebnis von <see cref="SaveFormAsync"/>: die Temp-Datei mit der PDF samt Feldwerten (null = nicht bekommen) und ob der
    /// Speichern-Dialog des Viewers überhaupt erschienen ist – dann steht er noch offen und der Nutzer kann selbst speichern.</summary>
    public readonly record struct FormSaveResult(string? Path, bool DialogShown);

    /// <summary>Ungespeicherte Formulareingaben im Chromium-Viewer (seit dem Laden bzw. dem letzten Speichern).</summary>
    public bool FormDirty { get; private set; }

    /// <summary>FormDirty hat gewechselt (auf dem UI-Thread) – fürs „*“ im Fenstertitel.</summary>
    public event EventHandler? FormDirtyChanged;

    /// <summary>Der Nutzer hat die Save-Schaltfläche des Viewers gedrückt: Temp-Datei mit der PDF samt Feldwerten – das Hauptfenster
    /// schreibt sie in die angezeigte Datei.</summary>
    public event EventHandler<string>? FormDownloaded;

    /// <summary>Formulareingaben als erledigt betrachten (nach dem Schreiben in die Datei oder bewusstem Verwerfen).</summary>
    public void DiscardForm()
    {
        if (!FormDirty) { return; }
        FormDirty = false;
        UpdateDialogCloak();
        FormDirtyChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ResetForm()
    {
        formEventsArmed = false;
        formSave?.TrySetResult(null);
        formSave = null;
        DiscardForm();
        UpdateDialogCloak();
    }

    /// <summary>Formularereignisse erst 1,5 s nach dem Laden werten – bis dahin baut Chromium den Accessibility-Baum auf.</summary>
    private async void ArmFormEventsLater()
    {
        var loaded = documentLoaded;
        await Task.Delay(1500);
        if (documentLoaded != loaded || currentBytes == null || AdobeActive) { return; }
        var chromium = FindDescendant(webView.Handle, "Chrome_RenderWidgetHostHWND", 4);
        if (chromium != IntPtr.Zero) { EnsureZoomHook(chromium); } // auch ohne Zoomanzeige (verschlüsselte Datei) die Hooks setzen
        formEventsArmed = true;
    }

    private void FormEventCallback(nint hook, uint eventType, nint hwnd, int idObject, int idChild, uint thread, uint time)
    {
        if (hwnd != zoomHookWindow || !formEventsArmed || FormDirty || AdobeActive || currentBytes == null) { return; }
        if (Interlocked.CompareExchange(ref formCheckRunning, 1, 0) != 0) { return; }
        _ = Task.Run(() =>
        {
            try { if (IsPdfFormFieldEvent(hwnd, idObject, idChild, eventType)) { webView.BeginInvoke(MarkFormDirty); } }
            finally { formCheckRunning = 0; }
        });
    }

    private void MarkFormDirty()
    {
        if (FormDirty || !formEventsArmed) { return; }
        FormDirty = true;
        UpdateDialogCloak();
        FormDirtyChanged?.Invoke(this, EventArgs.Empty);
    }

    // Der „Speichern unter“-Dialog soll beim automatischen Speichern nicht sichtbar auf- und wieder zugehen (Wunsch vom 26.09.2026: „wie
    // von Geisterhand“). DIALOGSTART kommt erst, wenn er schon zu sehen ist; deshalb beobachtet PDFlight, solange Eingaben offen sind,
    // zusätzlich das Anlegen von Fenstern (EVENT_OBJECT_CREATE, ohne Prozessfilter – der Dialog kommt aus einem Hilfsprozess) und macht
    // jeden Dialog (#32770), dessen Besitzer PDFlights Fenster ist, sofort vollständig transparent (WS_EX_LAYERED, Alpha 0) – das Anlegen
    // des Dateidialogs dauert deutlich länger als die Zustellung des Ereignisses. HandleSaveDialog füllt ihn unsichtbar aus. Jeder
    // andere Ausgang macht ihn wieder sichtbar (Uncloak): ein fremder Dialog (Systemdruckdialog), ein gescheitertes Ausfüllen, eine
    // Zeitüberschreitung – nie darf ein unsichtbarer modaler Dialog stehen bleiben. Der Hook läuft nur bei offenen Eingaben, weil
    // OBJECT_CREATE systemweit häufig ist.
    private const uint EVENT_OBJECT_CREATE = 0x8000;
    private nint createHook;
    private NativeMethods.WinEventProc? createHookProc;
    private nint cloakedDialog; // der zuletzt verborgene Dialog – für Uncloak nach einem Fehlschlag

    /// <summary>Den Hook fürs Verbergen an- oder abmelden, je nachdem, ob ein Speichern der Formulareingaben ansteht (UI-Thread).</summary>
    private void UpdateDialogCloak()
    {
        var wanted = FormDirty || formSave != null;
        if (wanted == (createHook != 0)) { return; }
        if (wanted)
        {
            createHookProc ??= CreateEventCallback;
            createHook = NativeMethods.SetWinEventHook(EVENT_OBJECT_CREATE, EVENT_OBJECT_CREATE, 0, createHookProc, 0, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);
        }
        else
        {
            NativeMethods.UnhookWinEvent(createHook);
            createHook = 0;
        }
    }

    private void CreateEventCallback(nint hook, uint eventType, nint hwnd, int idObject, int idChild, uint thread, uint time)
    {
        if (idObject != 0 || idChild != 0 || !(FormDirty || formSave != null)) { return; } // nur Fenster selbst (OBJID_WINDOW, CHILDID_SELF)
        if (!IsDialogWindow(hwnd)) { return; }
        var owner = GetAncestor(GetWindow(hwnd, 4), 2); // GW_OWNER, GA_ROOT – Kindfenster haben keinen Besitzer
        if (owner == 0 || webView.FindForm() is not { } form || owner != form.Handle) { return; }
        SetWindowLongPtr(hwnd, GWL_EXSTYLE, GetWindowLongPtr(hwnd, GWL_EXSTYLE) | WS_EX_LAYERED);
        SetLayeredWindowAttributes(hwnd, 0, 0, LWA_ALPHA);
        cloakedDialog = hwnd;
    }

    /// <summary>Einen verborgenen Dialog wieder sichtbar machen (aus jedem Thread; ein inzwischen geschlossenes Fenster schadet nicht).</summary>
    private void Uncloak(nint hwnd)
    {
        if (hwnd == 0 || hwnd != cloakedDialog) { return; }
        cloakedDialog = 0;
        SetLayeredWindowAttributes(hwnd, 0, 255, LWA_ALPHA);
        SetWindowLongPtr(hwnd, GWL_EXSTYLE, GetWindowLongPtr(hwnd, GWL_EXSTYLE) & ~WS_EX_LAYERED);
    }

    private static unsafe bool IsDialogWindow(nint hwnd)
    {
        var buffer = stackalloc char[16];
        var length = GetClassName(hwnd, buffer, 16);
        return new ReadOnlySpan<char>(buffer, length).SequenceEqual("#32770");
    }

    private const int GWL_EXSTYLE = -20;
    private const nint WS_EX_LAYERED = 0x80000;
    private const uint LWA_ALPHA = 0x2;

    [System.Runtime.InteropServices.LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static partial nint GetWindowLongPtr(nint hWnd, int index);

    [System.Runtime.InteropServices.LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static partial nint SetWindowLongPtr(nint hWnd, int index, nint value);

    [System.Runtime.InteropServices.LibraryImport("user32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static partial bool SetLayeredWindowAttributes(nint hWnd, uint colorKey, byte alpha, uint flags);

    /// <summary>Gehört das Ereignis zu einem Formularfeld der PDF? Rolle passend zur Ereignisart, mindestens zwei Dokument-Vorfahren
    /// (Viewer-Seite → „PDF Document“ → PDF) und kein Toolbar-Vorfahr (Viewer-Leiste); läuft im Hintergrund, COM-Aufrufe in den
    /// Browserprozess.</summary>
    private static bool IsPdfFormFieldEvent(nint hwnd, int idObject, int idChild, uint eventType)
    {
        try
        {
            if (AccessibleObjectFromEvent(hwnd, (uint)idObject, (uint)idChild, out var accessible, out var child) != 0 || accessible == null) { return false; }
            var role = accessible.get_accRole(child) as int? ?? 0;
            var matches = eventType switch
            {
                EVENT_OBJECT_VALUECHANGE => role is RoleText or RoleComboBox or RoleList,
                EVENT_OBJECT_STATECHANGE => role is RoleCheckButton or RoleRadioButton,
                _ => role is RoleListItem or RoleList,
            };
            if (!matches) { return false; }
            var documents = 0;
            object? parent = child is int id && id != 0 ? accessible : accessible.accParent; // ein einfaches Kind hängt an seinem Container
            for (var depth = 0; depth < 40 && parent is Accessibility.IAccessible element; depth++)
            {
                var parentRole = element.get_accRole(0) as int? ?? 0;
                if (parentRole == RoleToolBar) { return false; } // Bedienelement des Viewers (Seitenfeld, Zoomauswahl), kein Feld der PDF
                if (parentRole == RoleDocument) { documents++; }
                parent = element.accParent;
            }
            return documents >= 2;
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or InvalidCastException or ArgumentException or NotImplementedException) { return false; }
    }

    // DllImport statt LibraryImport: IAccessible ist eine IDispatch-Schnittstelle der eingebauten COM-Interop (Accessibility.dll), die der
    // Quellgenerator nicht marshallt
    [System.Runtime.InteropServices.DllImport("oleacc.dll")]
    private static extern int AccessibleObjectFromEvent(nint hwnd, uint idObject, uint idChild, out Accessibility.IAccessible accessible, out object childId);

    /// <summary>Downloads gibt es in PDFlight nicht (die Save-Schaltfläche geht über den Dateidialog) – keine Download-Leiste, nichts speichern.</summary>
    private void Core_DownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs e)
    {
        e.Handled = true;
        e.Cancel = true;
    }

    private static readonly string FormTempFolder = Path.Combine(Path.GetTempPath(), "PDFlight");

    private static string NewFormTempPath()
    {
        Directory.CreateDirectory(FormTempFolder);
        return Path.Combine(FormTempFolder, $"form-{Guid.NewGuid():N}.pdf");
    }

    /// <summary>Ein Dialog des Browserprozesses geht auf – bei offenen Formulareingaben ist es der „Speichern unter“-Dialog der Save-Schaltfläche.</summary>
    private void DialogEventCallback(nint hook, uint eventType, nint hwnd, int idObject, int idChild, uint thread, uint time)
    {
        if (!IsReady || currentBytes == null || AdobeActive || (!FormDirty && formSave == null)) { return; }
        var owner = GetAncestor(GetWindow(hwnd, 4), 2); // GW_OWNER, GA_ROOT: der Dialog gehört zu PDFlights Fenster
        if (owner != 0 && webView.FindForm() is { } form && owner != form.Handle) { return; }
        _ = Task.Run(() => HandleSaveDialog(hwnd));
    }

    [System.Runtime.InteropServices.LibraryImport("user32.dll")]
    private static partial nint GetWindow(nint hWnd, uint cmd);

    [System.Runtime.InteropServices.LibraryImport("user32.dll")]
    private static partial nint GetAncestor(nint hWnd, uint flags);

    /// <summary>Den „Speichern unter“-Dialog des Viewers ausfüllen: Temp-Datei ins Dateinamenfeld (AutomationId 1001 des gemeinsamen
    /// Dateidialogs), Speichern (AutomationId 1) drücken, auf die fertig geschriebene Datei warten. Andere Dialoge (Drucken) bleiben unberührt.</summary>
    private void HandleSaveDialog(nint hwnd)
    {
        try
        {
            System.Windows.Automation.AutomationElement? dialog = null;
            for (var i = 0; i < 20 && dialog == null; i++) // die Steuerelemente stehen kurz nach DIALOGSTART
            {
                Thread.Sleep(100);
                var candidate = System.Windows.Automation.AutomationElement.FromHandle(hwnd);
                // Erkennung über die Steuerelemente statt über den (sprachabhängigen) Titel: Dateinamenfeld und Speichern-Knopf des
                // gemeinsamen Dateidialogs – Besitzer und offene Formulareingaben hat der Rückruf schon geprüft
                if (FindAutomationId(candidate, "1001") != null && FindAutomationId(candidate, "1", System.Windows.Automation.ControlType.Button) != null) { dialog = candidate; }
            }
            if (dialog == null) { Uncloak(hwnd); return; } // kein Speichern-Dialog (etwa der Systemdruckdialog): wieder zeigen
            formDialogSeen?.TrySetResult(true);
            var temp = NewFormTempPath();
            var fileName = FindAutomationId(dialog, "1001")!;
            if (!fileName.TryGetCurrentPattern(System.Windows.Automation.ValuePattern.Pattern, out var value)) { FinishFormDownload(null); return; }
            ((System.Windows.Automation.ValuePattern)value).SetValue(temp);
            Thread.Sleep(150);
            var saveButton = FindAutomationId(dialog, "1", System.Windows.Automation.ControlType.Button); // die Dateiliste hat Gruppen mit derselben Id
            if (saveButton == null || !saveButton.TryGetCurrentPattern(System.Windows.Automation.InvokePattern.Pattern, out var invoke)) { FinishFormDownload(null); return; }
            ((System.Windows.Automation.InvokePattern)invoke).Invoke();
            for (var i = 0; i < 100; i++) // bis 10 s, bis der Viewer die Datei geschrieben und geschlossen hat
            {
                Thread.Sleep(100);
                if (!File.Exists(temp)) { continue; }
                try { using var probe = new FileStream(temp, FileMode.Open, FileAccess.Read, FileShare.None); if (probe.Length > 0) { FinishFormDownload(temp); return; } }
                catch (IOException) { } // noch in Arbeit
            }
            FinishFormDownload(null);
        }
        catch (Exception ex) when (ex is System.Windows.Automation.ElementNotAvailableException or System.Runtime.InteropServices.COMException
            or InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            FinishFormDownload(null);
        }
    }

    private static System.Windows.Automation.AutomationElement? FindAutomationId(System.Windows.Automation.AutomationElement root, string automationId, System.Windows.Automation.ControlType? type = null)
    {
        System.Windows.Automation.Condition condition = new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.AutomationIdProperty, automationId);
        if (type != null) { condition = new System.Windows.Automation.AndCondition(condition, new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.ControlTypeProperty, type)); }
        return root.FindFirst(System.Windows.Automation.TreeScope.Descendants, condition);
    }

    private void FinishFormDownload(string? temp)
    {
        if (temp == null) { Uncloak(cloakedDialog); } // gescheitert: steht der Dialog noch, muss der Nutzer ihn sehen
        var pending = formSave;
        formSave = null;
        if (pending != null) { pending.TrySetResult(temp); }
        else if (temp != null) { webView.BeginInvoke(() => FormDownloaded?.Invoke(this, temp)); } // nicht im WebView2-Rückruf weiterarbeiten
    }

    /// <summary>Formulareingaben aus dem Viewer holen: drückt die Save-Schaltfläche der Viewer-Leiste per UIA (bei Änderungen öffnet sie ein
    /// Menü – dort den Eintrag „mit Änderungen“); den Dateidialog füllt <see cref="HandleSaveDialog"/>. Liefert die Temp-Datei, null bei
    /// Fehlschlag oder ohne Eingaben.</summary>
    public async Task<FormSaveResult> SaveFormAsync()
    {
        if (!IsReady || !FormDirty || AdobeActive || currentBytes == null) { return new(null, false); }
        var chromium = FindDescendant(webView.Handle, "Chrome_RenderWidgetHostHWND", 4);
        if (chromium == IntPtr.Zero) { return new(null, false); }
        var pending = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var seen = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        formSave = pending;
        formDialogSeen = seen;
        _ = Task.Run(() => ClickViewerSave(chromium));
        // Erscheint binnen 4 s kein Dialog, hat die Schaltfläche nicht reagiert (etwa nach einer Änderung der Viewer-IDs) – nicht länger warten
        var first = await Task.WhenAny(seen.Task, pending.Task, Task.Delay(4000));
        var shown = first == seen.Task || (first == pending.Task && seen.Task.IsCompleted);
        if (first != pending.Task && !shown) { formSave = null; formDialogSeen = null; return new(null, false); }
        var finished = await Task.WhenAny(pending.Task, Task.Delay(15000)); // Dialog ausfüllen, Datei schreiben
        formDialogSeen = null;
        if (finished != pending.Task) { formSave = null; Uncloak(cloakedDialog); return new(null, true); } // der Dialog steht offen – sichtbar machen
        return new(pending.Task.Result, true);
    }

    private static void ClickViewerSave(IntPtr chromiumHandle)
    {
        try
        {
            var root = System.Windows.Automation.AutomationElement.FromHandle(chromiumHandle);
            var save = root.FindFirst(System.Windows.Automation.TreeScope.Descendants,
                new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.AutomationIdProperty, "save"));
            if (save == null || !save.TryGetCurrentPattern(System.Windows.Automation.InvokePattern.Pattern, out var invoke)) { return; }
            ((System.Windows.Automation.InvokePattern)invoke).Invoke();
            for (var i = 0; i < 20; i++) // bei Änderungen öffnet sich ein Menü (Original / mit Änderungen)
            {
                Thread.Sleep(100);
                var edited = root.FindFirst(System.Windows.Automation.TreeScope.Descendants,
                    new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.AutomationIdProperty, "save-edited"));
                if (edited != null && edited.TryGetCurrentPattern(System.Windows.Automation.InvokePattern.Pattern, out var invokeEdited))
                {
                    ((System.Windows.Automation.InvokePattern)invokeEdited).Invoke();
                    return;
                }
            }
        }
        catch (Exception ex) when (ex is System.Windows.Automation.ElementNotAvailableException
            or System.Runtime.InteropServices.COMException or InvalidOperationException) { }
    }

    // ------------------------------------------------------------------ Adobe PDF Embed API (optionale Ansicht, s. AdobeEmbed)

    // Dieselbe WebView2-Instanz zeigt statt des Chromium-Viewers die Seite adobe\index.html (virtueller Host AdobeEmbed.Host), die das
    // Adobe-SDK lädt und die Datei als SharedBuffer bekommt – genau wie im Testprojekt PdfEmbed. Solange AdobeActive gilt, sind die
    // UIA-Funktionen des Chromium-Viewers (Seitenfeld, Zoom, Drehen, Layout) abgeschaltet; Load und CloseDocument beenden die Ansicht.

    /// <summary>True, solange das WebView die Adobe-Seite statt des Chromium-Viewers zeigt (bis zum nächsten Load/CloseDocument).</summary>
    public bool AdobeActive { get; private set; }

    /// <summary>Ungespeicherte Anmerkungen in der Adobe-Ansicht: gesetzt bei jedem Hinzufügen, Ändern oder Löschen einer Anmerkung
    /// (Ereignisse des Annotation-Managers), zurückgesetzt nach dem Speichern und beim Laden. Konservativ – wer eine Anmerkung hinzufügt
    /// und wieder löscht, gilt als geändert. Das Hauptfenster setzt es zurück auf true, wenn das Schreiben der Datei scheitert.</summary>
    public bool AdobeDirty { get; set; }

    /// <summary>Zuletzt gemeldete Seite der Adobe-Ansicht (PAGE_VIEW), anfangs die Startseite – damit der Chromium-Viewer beim
    /// Zurückwechseln dieselbe Seite zeigt.</summary>
    public int AdobePage { get; private set; }

    /// <summary>Zuletzt gemeldete Zoomstufe der Adobe-Ansicht, umgerechnet in Chromium-Prozent (ZOOM_LEVEL bzw. getPageZoom; 0 = unbekannt) –
    /// damit der Chromium-Viewer beim Zurückwechseln etwa gleich groß zeigt.</summary>
    public int AdobeZoomPercent { get; private set; }

    /// <summary>Adobes Zoomstufe 1 ist 1 px je Punkt (72 dpi), Chromiums 100 % sind 96 dpi (1,333 px je Punkt) – gemessen 26.09.2026 bei
    /// 100 % Windows-Skalierung an der Seitenbreite (612 pt: Adobe 1,5 → 918 px, Chromium 100 % → 816 px). Der Faktor rechnet um.</summary>
    private const double AdobeZoomBase = 1.0 / (96.0 / 72.0);

    /// <summary>Die Adobe-Seite zeigt das Dokument.</summary>
    public event EventHandler? AdobeReady;

    /// <summary>„Speichern“ in der Adobe-Leiste: die PDF samt Anmerkungen – das Hauptfenster schreibt sie in die angezeigte Datei.</summary>
    public event EventHandler<byte[]>? AdobeSaveRequested;

    /// <summary>Fehlermeldung der Adobe-Seite (SDK nicht erreichbar, Viewer-Fehler).</summary>
    public event EventHandler<string>? AdobeError;

    private string? adobeInfo; // Begleitdaten für PostSharedBufferToScript, bis die Seite geladen ist

    /// <summary>Zeigt die Datei in der Adobe-Ansicht: lädt die Adobe-Seite; sobald sie steht (NavigationCompleted), geht die Datei als
    /// SharedBuffer hinüber – mit Dateiname, Client-ID, Startseite, Sprache und den Hinweistexten. Wirft IOException usw. wie Load.</summary>
    public void ShowAdobe(string filePath, string clientId, int page, int zoomPercent, string loadingText, string offlineText)
    {
        documentLoaded = new(TaskCreationOptions.RunContinuationsAsynchronously); // eine laufende GoToPageIfUntouchedAsync sieht ein anderes Dokument und hört auf
        currentBytes = File.ReadAllBytes(filePath);
        ResetForm(); // Formulareingaben hat das Hauptfenster vorher gespeichert
        AdobeActive = true;
        AdobeDirty = false;
        AdobePage = Math.Max(page, 1);
        AdobeZoomPercent = zoomPercent;
        zoomRequests++;  // eine laufende Zoomabfrage meldet nichts mehr
        this.zoomPercent = 0; // nach dem Zurückwechseln meldet die nächste Messung auch eine unveränderte Zoomstufe wieder (Statusleiste)
        adobeInfo = JsonSerializer.Serialize(new { fileName = Path.GetFileName(filePath), clientId, page = AdobePage, zoom = zoomPercent > 0 ? zoomPercent / 100.0 / AdobeZoomBase : (double?)null,
            locale = AdobeEmbed.Locale, loadingText, offlineText });
        webView.CoreWebView2.Navigate($"https://{AdobeEmbed.Host}/index.html");
    }

    private void ResetAdobe()
    {
        AdobeActive = false;
        AdobeDirty = false;
        adobeInfo = null;
    }

    private void PostAdobeFile()
    {
        if (adobeInfo == null || currentBytes == null) { return; }
        var core = webView.CoreWebView2;
        using var buffer = core.Environment.CreateSharedBuffer((ulong)Math.Max(currentBytes.Length, 1));
        using (var stream = buffer.OpenStream()) { stream.Write(currentBytes); }
        core.PostSharedBufferToScript(buffer, CoreWebView2SharedBufferAccess.ReadOnly, adobeInfo);
        adobeInfo = null;
    }

    /// <summary>Meldungen der Adobe-Seite: ready, page, changed, save (PDF samt Anmerkungen als Base64), error, log. Die Ereignisse gehen
    /// per BeginInvoke ans Hauptfenster – dort folgen Dialoge, und die haben in einem WebView2-Rückruf nichts verloren.</summary>
    private void HandleAdobeMessage(string json)
    {
        try
        {
            using var message = JsonDocument.Parse(json);
            var root = message.RootElement;
            if (root.ValueKind != JsonValueKind.Object) { return; }
            switch (root.TryGetProperty("type", out var t) ? t.GetString() : null)
            {
                case "ready":
                    webView.BeginInvoke(() => AdobeReady?.Invoke(this, EventArgs.Empty));
                    break;
                case "page":
                    if (root.TryGetProperty("page", out var p) && p.ValueKind == JsonValueKind.Number) { AdobePage = p.GetInt32(); }
                    break;
                case "zoom":
                    if (root.TryGetProperty("zoom", out var z) && z.ValueKind == JsonValueKind.Number) { AdobeZoomPercent = (int)Math.Round(z.GetDouble() * AdobeZoomBase * 100); }                    break;
                case "changed":
                    AdobeDirty = true;
                    break;
                case "save":
                    var bytes = Convert.FromBase64String(root.GetProperty("data").GetString() ?? string.Empty);
                    currentBytes = bytes; // DocumentBytes bleibt der gespeicherte Stand
                    AdobeDirty = false;
                    webView.BeginInvoke(() => AdobeSaveRequested?.Invoke(this, bytes));
                    break;
                case "error":
                    var text = root.TryGetProperty("message", out var m) ? m.GetString() ?? string.Empty : string.Empty;
                    webView.BeginInvoke(() => AdobeError?.Invoke(this, text));
                    break;
            }
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException) { }
    }

    /// <summary>Adobes Nutzungsprotokoll: Die Anfragen gehen nicht ins Netz, sondern bekommen hier ein leeres „204 No Content“. Sie kommen
    /// aus Adobes iframe (anderer Ursprung), deshalb mit passenden CORS-Kopfzeilen – sonst sähe das SDK einen Netzwerkfehler statt einer
    /// erfolgreichen, leeren Antwort (geprüft im Testprojekt PdfEmbed, 24.09.2026).</summary>
    private void BlockAdobeLog(CoreWebView2WebResourceRequestedEventArgs e)
    {
        var request = e.Request.Headers;
        var origin = request.Contains("Origin") ? request.GetHeader("Origin") : "*";
        var allowHeaders = request.Contains("Access-Control-Request-Headers") ? request.GetHeader("Access-Control-Request-Headers") : "Content-Type";
        var headers = $"Access-Control-Allow-Origin: {origin}\nAccess-Control-Allow-Credentials: true\nAccess-Control-Allow-Methods: POST, OPTIONS\n"
            + $"Access-Control-Allow-Headers: {allowHeaders}\nContent-Length: 0";
        e.Response = webView.CoreWebView2.Environment.CreateWebResourceResponse(null, 204, "No Content", headers);
    }

    private void ShowEmptyPage()
    {
        webView.CoreWebView2.NavigateToString("""
            <!doctype html><html lang="de"><head><meta charset="utf-8"><meta name="color-scheme" content="light dark"><title>PDFlight</title>
            <style>
              body { margin:0; font-family:'Segoe UI',sans-serif; background:#f3f3f3; color:#666; display:flex; align-items:center; justify-content:center; height:100vh }
              @media (prefers-color-scheme: dark) { body { background:#333333; color:#bbbbbb } } /* Anzeigehintergrund „dunkel“: wie die Fläche des PDF-Viewers (RGB 51) */
            </style></head>
            <body>
              <div id="hint" style="text-align:center;border:3px dashed transparent;border-radius:16px;padding:40px">
                <div style="font-size:56px">&#128196;</div>
                <h2 style="font-weight:600;margin:8px 0 4px">Kein Dokument ge&ouml;ffnet</h2>
                <p>&Ouml;ffne eine PDF-Datei &uuml;ber die Symbolleiste (Strg+O)<br>oder ziehe sie einfach hierher.</p>
              </div>
              <script>
                const hint = document.getElementById('hint');
                document.addEventListener('dragover', e => { e.preventDefault(); hint.style.borderColor = '#7aa7d4'; });
                document.addEventListener('dragleave', () => { hint.style.borderColor = 'transparent'; });
                document.addEventListener('drop', e => {
                  e.preventDefault();
                  hint.style.borderColor = 'transparent';
                  const files = [...e.dataTransfer.files];
                  if (files.length) { chrome.webview.postMessageWithAdditionalObjects('drop', files); }
                });
                window.__pdflightDrop = true; // Marker f&uuml;r automatisierte Tests
                requestAnimationFrame(() => requestAnimationFrame(() => chrome.webview.postMessage('painted'))); // erstes Bild gezeichnet: PDFlight nimmt die Start-Abdeckung weg
              </script>
            </body></html>
            """);
    }

    private void Core_WebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
        var environment = webView.CoreWebView2.Environment;
        if (e.Request.Uri.StartsWith(AdobeEmbed.BlockedLogUrl, StringComparison.OrdinalIgnoreCase)) { BlockAdobeLog(e); return; }
        e.Response = currentBytes == null
            ? environment.CreateWebResourceResponse(null, 404, "Not Found", string.Empty)
            : environment.CreateWebResourceResponse(new MemoryStream(currentBytes), 200, "OK", "Content-Type: application/pdf\nCache-Control: no-store");
    }
}
