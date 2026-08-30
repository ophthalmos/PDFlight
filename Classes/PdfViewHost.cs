using System.Diagnostics;
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
    private byte[] currentBytes;

    /// <summary>Wird ausgelöst, wenn eine PDF-Datei auf den Viewer gezogen wurde (Drop löst dort eine file://-Navigation aus).</summary>
    public event EventHandler<string> PdfFileDropped;

    public bool IsReady { get; private set; }

    public async Task InitializeAsync()
    {
        // Eigener Datenordner, damit das Programm auch aus einem schreibgeschützten Installationsordner läuft.
        // Je Sprache getrennt: Alle Prozesse am selben Ordner müssen identische Optionen verwenden, sonst
        // scheitert die Initialisierung (z.B. alte Instanz läuft nach einem Sprachwechsel noch).
        var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PDFlight", "WebView2." + Lng.CultureCode);
        var options = new CoreWebView2EnvironmentOptions { Language = Lng.CultureCode }; // Viewer-Oberfläche in der Programmsprache
        var environment = await CoreWebView2Environment.CreateAsync(null, dataFolder, options);
        await webView.EnsureCoreWebView2Async(environment);

        var core = webView.CoreWebView2;
        core.Settings.AreDevToolsEnabled = false;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.IsGeneralAutofillEnabled = false;
        core.Settings.IsPasswordAutosaveEnabled = false;
        core.Settings.HiddenPdfToolbarItems = CoreWebView2PdfToolbarItems.Save | CoreWebView2PdfToolbarItems.SaveAs // Speichern übernimmt PDFlight selbst
            | CoreWebView2PdfToolbarItems.FullScreen; // der Chromium-Vollbildmodus ist im Host-Fenster kaum beendbar → PDFlight bietet stattdessen F11
        core.AddWebResourceRequestedFilter("https://" + VirtualHost + "/*", CoreWebView2WebResourceContext.All);
        core.WebResourceRequested += Core_WebResourceRequested;
        core.NavigationStarting += Core_NavigationStarting;
        core.NavigationCompleted += (s, e) => twoPageActive = false; // jedes Dokumentladen startet im einseitigen Viewer-Standard
        core.NewWindowRequested += Core_NewWindowRequested;
        core.WebMessageReceived += Core_WebMessageReceived; // Drop-Meldungen der Leerseite
        webView.AllowExternalDrop = true; // Drops aufs Dokument landen als file://-Navigation in Core_NavigationStarting

        IsReady = true;
        ShowEmptyPage();
        WarmUpAutomation(); // Chromiums Accessibility-Baum schon jetzt aktivieren, nicht erst beim ersten Strg+Entf
    }

    /// <summary>Drop auf die Leerseite: deren Skript meldet die Dateien per postMessageWithAdditionalObjects mit echten Pfaden.</summary>
    private void Core_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        if (e.AdditionalObjects == null) { return; }
        foreach (var item in e.AdditionalObjects)
        {
            if (item is CoreWebView2File file && !string.IsNullOrEmpty(file.Path)
                && file.Path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) && File.Exists(file.Path))
            {
                RaisePdfFileDropped(file.Path);
                break;
            }
        }
    }

    /// <summary>Manche Drops und Links landen als "neues Fenster": PDFs übernehmen, Web-Links in den Standardbrowser.</summary>
    private void Core_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
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
        // nicht innerhalb eines WebView2-Ereignisses neu navigieren → entkoppeln
        webView.BeginInvoke(new Action(() => PdfFileDropped?.Invoke(this, path)));
    }

    /// <summary>Lässt nur eigene Inhalte zu; abgelegte PDF-Dateien werden gemeldet, Web-Links im Standardbrowser geöffnet.</summary>
    private void Core_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
    {
        var uri = e.Uri ?? string.Empty;
        if (uri.StartsWith("https://" + VirtualHost + "/", StringComparison.OrdinalIgnoreCase)
            || uri.StartsWith("about:", StringComparison.OrdinalIgnoreCase)
            || uri.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) { return; }

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

    /// <summary>Lädt die PDF-Datei in den Speicher und zeigt sie an; die Datei bleibt danach ungesperrt.
    /// Mit page &gt; 0 springt der Viewer direkt zu dieser Seite (z.B. nach dem Löschen von Seiten).</summary>
    public void Load(string filePath, int page = 0)
    {
        currentBytes = File.ReadAllBytes(filePath); // wirft IOException etc. → behandelt der Aufrufer
        var fragment = page > 0 ? "#page=" + page : string.Empty;
        webView.CoreWebView2.Navigate($"https://{VirtualHost}/{Uri.EscapeDataString(Path.GetFileName(filePath))}?t={DateTime.Now.Ticks}{fragment}");
    }

    public void CloseDocument()
    {
        currentBytes = null;
        if (IsReady) { ShowEmptyPage(); }
    }

    /// <summary>Öffnet die Druckvorschau des Viewers — dieselbe wie bei Strg+P.</summary>
    public void ShowPrintDialog()
    {
        if (IsReady) { webView.CoreWebView2.ShowPrintUI(CoreWebView2PrintDialogKind.Browser); }
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
        if (!IsReady || currentBytes == null) { return; }
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
            System.Windows.Automation.AutomationElement layouts = null;
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
                System.Windows.Automation.AutomationElement target = null;
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
        if (!IsReady || currentBytes == null) { return; }
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
        if (!IsReady || currentBytes == null) { return; }
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
    private static partial IntPtr FindWindowEx(IntPtr parent, IntPtr after, string className, string windowName);

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

    private void ShowEmptyPage()
    {
        webView.CoreWebView2.NavigateToString("""
            <!doctype html><html lang="de"><head><meta charset="utf-8"><title>PDFlight</title></head>
            <body style="margin:0;font-family:'Segoe UI',sans-serif;background:#f3f3f3;color:#666;
                         display:flex;align-items:center;justify-content:center;height:100vh">
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
              </script>
            </body></html>
            """);
    }

    private void Core_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
        var environment = webView.CoreWebView2.Environment;
        e.Response = currentBytes == null
            ? environment.CreateWebResourceResponse(null, 404, "Not Found", string.Empty)
            : environment.CreateWebResourceResponse(new MemoryStream(currentBytes), 200, "OK", "Content-Type: application/pdf\nCache-Control: no-store");
    }
}
