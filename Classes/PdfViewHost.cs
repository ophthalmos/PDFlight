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
internal class PdfViewHost(WebView2 webView)
{
    private const string VirtualHost = "pdflight.doc";
    private readonly WebView2 webView = webView;
    private byte[] currentBytes;

    /// <summary>Wird ausgelöst, wenn eine PDF-Datei auf den Viewer gezogen wurde (Drop löst dort eine file://-Navigation aus).</summary>
    public event EventHandler<string> PdfFileDropped;

    public bool IsReady { get; private set; }

    public async Task InitializeAsync()
    {
        // Eigener Datenordner, damit das Programm auch aus einem schreibgeschützten Installationsordner läuft
        var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PDFLight", "WebView2");
        var environment = await CoreWebView2Environment.CreateAsync(null, dataFolder);
        await webView.EnsureCoreWebView2Async(environment);

        var core = webView.CoreWebView2;
        core.Settings.AreDevToolsEnabled = false;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.IsGeneralAutofillEnabled = false;
        core.Settings.IsPasswordAutosaveEnabled = false;
        core.Settings.HiddenPdfToolbarItems = CoreWebView2PdfToolbarItems.Save | CoreWebView2PdfToolbarItems.SaveAs // Speichern übernimmt PDFLight selbst
            | CoreWebView2PdfToolbarItems.FullScreen; // der Chromium-Vollbildmodus ist im Host-Fenster kaum beendbar → PDFLight bietet stattdessen F11
        core.AddWebResourceRequestedFilter("https://" + VirtualHost + "/*", CoreWebView2WebResourceContext.All);
        core.WebResourceRequested += Core_WebResourceRequested;
        core.NavigationStarting += Core_NavigationStarting;
        core.NewWindowRequested += Core_NewWindowRequested;
        core.WebMessageReceived += Core_WebMessageReceived; // Drop-Meldungen der Leerseite
        webView.AllowExternalDrop = true; // Drops aufs Dokument landen als file://-Navigation in Core_NavigationStarting

        IsReady = true;
        ShowEmptyPage();
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

    private void ShowEmptyPage()
    {
        webView.CoreWebView2.NavigateToString("""
            <!doctype html><html lang="de"><head><meta charset="utf-8"><title>PDFLight</title></head>
            <body style="margin:0;font-family:'Segoe UI',sans-serif;background:#f3f3f3;color:#666;
                         display:flex;align-items:center;justify-content:center;height:100vh">
              <div id="hint" style="text-align:center;border:3px dashed transparent;border-radius:16px;padding:40px">
                <div style="font-size:56px">&#128196;</div>
                <h2 style="font-weight:600;margin:8px 0 4px">Kein Dokument ge&ouml;ffnet</h2>
                <p>&Ouml;ffnen Sie eine PDF-Datei &uuml;ber die Symbolleiste (Strg+O)<br>oder ziehen Sie sie einfach hierher.</p>
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
