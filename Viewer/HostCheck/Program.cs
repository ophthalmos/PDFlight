using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using PDFLight.Viewer;

namespace PDFLight.HostCheck;

/// <summary>Prüft die Sandbox: startet das Hilfsprogramm abgeschottet, lässt es verbotene Dinge versuchen (Dateien, Netz, Registry,
/// Prozesse, ausführbarer Speicher), rendert eine Seite als Bitmap und als EMF, misst die Zeiten und testet, ob ein Absturz im
/// Hilfsprogramm das Hauptprogramm unberührt lässt – ohne Fehlermeldung des Systems auf dem Desktop.
/// Aufruf: HostCheck.exe Datei.pdf [Ausgabeordner]. Umgebungsvariablen: PDFHOST (Pfad eines anderen Hilfsprogramms, etwa des
/// Native-AOT-Hilfsprogramms im Release-Ordner von PDFlight), SANDBOX_EMF_DPI (Auflösung des Druckwegs, Vorgabe 600), SANDBOX_OHNE (s. SandboxProcess.Without).</summary>
internal static class Program
{
    private static int failures;

    private static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        if (args.Length == 0 || !File.Exists(args[0])) { Console.WriteLine("Aufruf: HostCheck.exe Datei.pdf [Ausgabeordner]"); return 2; }
        var pdfPath = Path.GetFullPath(args[0]);
        var outputFolder = args.Length > 1 ? args[1] : Environment.CurrentDirectory;
        // PDFHOST: ein anderes Hilfsprogramm (etwa das mit Native AOT übersetzte aus dem Release-Ordner von PDFlight), sonst das daneben
        var helper = Environment.GetEnvironmentVariable("PDFHOST") is { Length: > 0 } other
            ? Path.GetFullPath(other)
            : Path.Combine(AppContext.BaseDirectory, "pdfhost.exe");
        Environment.SetEnvironmentVariable("DOTNET_EnableDiagnostics", "0"); // das Hilfsprogramm erbt es: kein Diagnose-Kanal nach außen
        var dialogsAtStart = Native.VisibleWindows(Path.GetFileName(helper)); // Fehlermeldungen des Systems zum Hilfsprogramm

        // LPAC startet die AOT-Laufzeit nicht (Exit-Code −1 noch vor Main, Ursache offen) – AOT läuft deshalb im normalen AppContainer,
        // dafür mit Arbitrary Code Guard; das JIT-Hilfsprogramm versucht zuerst LPAC
        var aot = !File.Exists(Path.ChangeExtension(helper, ".dll"));
        var lessPrivileged = !aot;
        SandboxProcess sandbox;
        var watch = Stopwatch.StartNew();
        try { sandbox = StartChecked(helper, lessPrivileged); }
        catch (Exception ex) when (lessPrivileged)
        {
            Console.WriteLine($"Start als LPAC fehlgeschlagen: {ex.Message}");
            Console.WriteLine("Zweiter Versuch als normaler AppContainer…");
            lessPrivileged = false;
            watch.Restart();
            sandbox = StartChecked(helper, lessPrivileged);
        }
        using (sandbox)
        {
            var startTime = watch.Elapsed;
            Console.WriteLine($"Gestartet als {(!sandbox.InContainer ? "normaler Prozess OHNE Container (Vergleich)" : sandbox.LessPrivileged ? "LPAC (less privileged AppContainer)" : "AppContainer")}" +
                $"{(sandbox.ProhibitsDynamicCode ? ", Native AOT mit Arbitrary Code Guard" : ", mit JIT")} in {startTime.TotalMilliseconds:0} ms");
            Console.WriteLine("Selbstbericht: " + sandbox.Call(Protocol.Hello, null, r => r.ReadString()));
            Console.WriteLine();
            if (RunProbes(sandbox, pdfPath))
            {
                Console.WriteLine();
                RenderChecks(sandbox, pdfPath, outputFolder);
            }
        }
        Console.WriteLine();
        CrashCheck(helper, lessPrivileged);
        Console.WriteLine();
        Thread.Sleep(1500);
        var newDialogs = Native.VisibleWindows(Path.GetFileName(helper)).Except(dialogsAtStart).Count();
        Console.WriteLine(newDialogs == 0 ? "✔ keine neue Fehlermeldung des Systems auf dem Desktop" : $"✘ {newDialogs} neue Fehlermeldung(en) des Systems auf dem Desktop");
        if (newDialogs != 0) { failures++; }
        Console.WriteLine(failures == 0 ? "ERGEBNIS: alle Prüfungen bestanden" : $"ERGEBNIS: {failures} Prüfung(en) NICHT bestanden");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>Startet und wartet auf die erste Antwort – schlägt der Start der .NET-Laufzeit im Container fehl, steht der Grund in
    /// der Fehlerausgabe des Hilfsprogramms.</summary>
    private static SandboxProcess StartChecked(string helper, bool lessPrivileged)
    {
        var sandbox = SandboxProcess.Start(helper, lessPrivileged);
        try
        {
            sandbox.Call(Protocol.Hello, null, r => r.ReadString());
            return sandbox;
        }
        catch (SandboxCrashedException ex)
        {
            var error = sandbox.ErrorOutput.Trim();
            sandbox.Dispose();
            throw new InvalidOperationException(ex.Message + (error.Length > 0 ? Environment.NewLine + error : string.Empty), ex);
        }
    }

    // ================================================================== Angriffsproben

    /// <summary>false, wenn das Hilfsprogramm dabei abgestürzt ist.</summary>
    private static bool RunProbes(SandboxProcess sandbox, string pdfPath)
    {
        var secret = Path.Combine(Path.GetTempPath(), "pdfium-sandbox-geheim.txt");
        File.WriteAllText(secret, "vertraulich");
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var helperFolder = AppContext.BaseDirectory;
        using var listener = new TcpListener(IPAddress.Loopback, 0); // ein Dienst auf diesem Rechner, den die Sandbox nicht erreichen soll
        listener.Start();
        var loopbackPort = ((IPEndPoint)listener.LocalEndpoint).Port;

        (string Label, string Kind, string Target, bool? MustBeDenied)[] probes =
        [
            ("Datei im Temp-Ordner lesen", "read", secret, true),
            ("Die PDF-Datei selbst lesen", "read", pdfPath, true),
            ("Dokumente-Ordner auflisten", "list", documents, true),
            ("Laufwerk der PDF auflisten", "list", Path.GetPathRoot(pdfPath)!, true),
            ("Datei im Temp-Ordner anlegen", "write", Path.Combine(Path.GetTempPath(), "pdfium-sandbox-probe.txt"), true),
            ("Datei im eigenen Programmordner anlegen", "write", Path.Combine(helperFolder, "pdfium-sandbox-probe.txt"), true),
            ("Registry-Schlüssel unter HKCU anlegen", "registry", @"Software\PDFlightHostProbe", true),
            ("Internet: TCP zu 1.1.1.1:443", "net", "1.1.1.1:443", true),
            ($"Loopback: TCP zu 127.0.0.1:{loopbackPort}", "net", $"127.0.0.1:{loopbackPort}", true),
            ("DNS-Auflösung example.com", "dns", "example.com", null),
            ("Programm starten (cmd.exe)", "process", Path.Combine(Environment.SystemDirectory, "cmd.exe"), true),
            ("Ausführbaren Speicher anlegen", "execmem", "", sandbox.ProhibitsDynamicCode ? true : null),
            ("Nullzeiger in .NET-Code (normale Ausnahme)", "nullref", "", true),
            ("Systemdatei lesen (win.ini)", "read", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "win.ini"), null),
        ];
        Console.WriteLine("Angriffsproben (✔ = wie erwartet verweigert, ✘ = Sandbox lässt es zu, · = nur zur Information):");
        foreach (var (label, kind, target, mustBeDenied) in probes)
        {
            string result;
            try { result = sandbox.Call(Protocol.Probe, w => { w.Write(kind); w.Write(target); }, r => r.ReadString()); }
            catch (SandboxCrashedException ex)
            {
                failures++;
                Console.WriteLine($"  ✘ {label,-42} Hilfsprogramm dabei abgestürzt: {ex.Message}");
                File.Delete(secret);
                return false;
            }
            var denied = result.StartsWith(Protocol.ProbeDenied, StringComparison.Ordinal);
            var mark = mustBeDenied == null ? "·" : denied == mustBeDenied ? "✔" : "✘";
            if (mark == "✘") { failures++; }
            Console.WriteLine($"  {mark} {label,-42} {Shorten(result)}");
        }
        File.Delete(secret);
        return true;
    }

    private static string Shorten(string text)
    {
        text = text.ReplaceLineEndings(" ");
        return text.Length > 110 ? text[..109] + "…" : text;
    }

    // ================================================================== Rendern

    private static void RenderChecks(SandboxProcess sandbox, string pdfPath, string outputFolder)
    {
        var bytes = File.ReadAllBytes(pdfPath); // gelesen wird im Hauptprogramm, das Hilfsprogramm bekommt nur die Bytes
        var watch = Stopwatch.StartNew();
        var (document, sizes) = sandbox.Call(Protocol.Open, w => { w.Write(bytes.Length); w.Write(bytes); w.Write(string.Empty); }, r =>
        {
            var id = r.ReadInt32();
            _ = r.ReadBoolean(); // Formular
            var result = new SizeF[r.ReadInt32()];
            for (var i = 0; i < result.Length; i++) { result[i] = new SizeF(r.ReadSingle(), r.ReadSingle()); }
            return (id, result);
        });
        Console.WriteLine($"Geöffnet: {Path.GetFileName(pdfPath)}, {bytes.Length / 1024.0 / 1024.0:0.0} MB, {sizes.Length} Seiten – " +
            $"Übertragen und geladen in {watch.Elapsed.TotalMilliseconds:0} ms");

        // Seite 1 bei 150 dpi als Bitmap
        var width = (int)(sizes[0].Width * 150 / 72);
        var height = (int)(sizes[0].Height * 150 / 72);
        watch.Restart();
        using (var bitmap = RenderBitmap(sandbox, document, 0, width, height))
        {
            Console.WriteLine($"Seite 1 als Bitmap {width} × {height} (150 dpi): {watch.Elapsed.TotalMilliseconds:0} ms samt Übertragung " +
                $"({width * height * 4 / 1024.0 / 1024.0:0.0} MB)");
            bitmap.Save(Path.Combine(outputFolder, "sandbox-seite1.png"), ImageFormat.Png);
        }

        // viele kleine Aufträge: Aufwand der Prozessgrenze je Auftrag (Miniaturen, Kacheln)
        var thumbWidth = 200;
        var thumbHeight = (int)(thumbWidth * sizes[0].Height / sizes[0].Width);
        var count = Math.Min(20, sizes.Length);
        watch.Restart();
        for (var i = 0; i < count; i++) { RenderBitmap(sandbox, document, i, thumbWidth, thumbHeight).Dispose(); }
        Console.WriteLine($"{count} Miniaturen {thumbWidth} × {thumbHeight}: {watch.Elapsed.TotalMilliseconds / count:0.0} ms je Seite");

        // Druckweg: Seite 1 als EMF (Vektor), im Hauptprogramm abgespielt
        watch.Restart();
        var emfDpi = int.TryParse(Environment.GetEnvironmentVariable("SANDBOX_EMF_DPI"), out var dpi) ? dpi : 600; // Druckerauflösung
        var emf = sandbox.Call(Protocol.RenderEmf, w => { w.Write(document); w.Write(0); w.Write((int)(sizes[0].Width * emfDpi / 72)); w.Write((int)(sizes[0].Height * emfDpi / 72)); },
            r => r.ReadBytes(r.ReadInt32()));
        Console.WriteLine($"Seite 1 als EMF bei {emfDpi} dpi (Druckweg): {emf.Length / 1024.0:0} KB in {watch.Elapsed.TotalMilliseconds:0} ms");
        File.WriteAllBytes(Path.Combine(outputFolder, "sandbox-seite1.emf"), emf);
        using var metafile = new Metafile(new MemoryStream(emf));
        using var played = new Bitmap(width, height);
        using (var g = Graphics.FromImage(played))
        {
            g.Clear(Color.White);
            g.DrawImage(metafile, 0, 0, width, height);
        }
        played.Save(Path.Combine(outputFolder, "sandbox-seite1-emf.png"), ImageFormat.Png);
        Console.WriteLine($"Bilder: sandbox-seite1.png, sandbox-seite1-emf.png in {outputFolder}");

        // Eigenschaften-Dialog: Sicherheit und eingebettete Dateien; eine ungültige Nummer muss einen Fehler melden, keinen Absturz
        var (permissions, revision) = sandbox.Call(Protocol.Security, w => w.Write(document), r => (r.ReadUInt32(), r.ReadInt32()));
        var names = sandbox.Call(Protocol.Attachments, w => w.Write(document), r =>
        {
            var list = new List<string>();
            for (var i = r.ReadInt32(); i > 0; i--) { list.Add($"{r.ReadString()} ({r.ReadInt64()} Byte)"); }
            return list;
        });
        var attachments = names.Count;
        Console.WriteLine($"Sicherheit: Revision {revision}, Berechtigungen 0x{permissions:X8}; eingebettete Dateien: {string.Join(", ", names)}");
        if (attachments > 0)
        {
            var first = sandbox.Call(Protocol.AttachmentData, w => { w.Write(document); w.Write(0); }, r => r.ReadBytes(r.ReadInt32()));
            Console.WriteLine($"Erste eingebettete Datei gelesen: {first.Length} Byte");
        }
        try
        {
            sandbox.Call(Protocol.AttachmentData, w => { w.Write(document); w.Write(attachments); }, r => r.ReadBytes(r.ReadInt32()));
            Console.WriteLine("FEHLER: Anhang mit ungültiger Nummer wurde geliefert");
        }
        catch (SandboxErrorException) { Console.WriteLine("Anhang mit ungültiger Nummer: abgewiesen (ok)"); }
    }

    private static Bitmap RenderBitmap(SandboxProcess sandbox, int document, int page, int width, int height) =>
        sandbox.Call(Protocol.Render, w => { w.Write(document); w.Write(page); w.Write(width); w.Write(height); w.Write(0); w.Write(0); w.Write(width); w.Write(height); }, r =>
        {
            var stride = width * 4;
            var pixels = r.ReadBytes(stride * height);
            // wie PdfiumDocument.ToBitmap: ReadBytes liefert am Ende des Datenstroms weniger, Marshal.Copy würfe sonst unverständlich (Ultra-Review 04.10.2026)
            if (pixels.Length != stride * height) { throw new InvalidDataException("Unvollständiges Bild vom Hilfsprogramm."); }
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (var y = 0; y < height; y++) { Marshal.Copy(pixels, y * stride, data.Scan0 + y * data.Stride, width * 4); }
            }
            finally { bitmap.UnlockBits(data); }
            return bitmap;
        });

    // ================================================================== Absturz

    private static void CrashCheck(string helper, bool lessPrivileged)
    {
        Console.WriteLine("Absturz im Hilfsprogramm (simulierte Zugriffsverletzung):");
        foreach (var (where, label) in new[] { (Protocol.CrashInPdfium, "in PDFium"), (Protocol.CrashOutsidePdfium, "in der C-Laufzeit (memset)") })
        {
            var dialogsBefore = Native.VisibleWindows(Path.GetFileName(helper));
            var watch = Stopwatch.StartNew();
            using (var sandbox = SandboxProcess.Start(helper, lessPrivileged))
            {
                try
                {
                    sandbox.Call(Protocol.Crash, w => w.Write(where), r => r.ReadByte());
                    Console.WriteLine($"  ✘ {label}: Hilfsprogramm hat den Absturz überlebt?");
                    failures++;
                }
                catch (SandboxCrashedException ex)
                {
                    var mark = ex.TimedOut ? "✘" : "✔"; // erst nach der Frist beendet: die Absturzbehandlung hing
                    if (ex.TimedOut) { failures++; }
                    Console.WriteLine($"  {mark} {label}: erkannt nach {watch.Elapsed.TotalSeconds:0.0} s – {ex.Message}");
                }
            }
            Thread.Sleep(1500); // Zeit für eine Fehlermeldung des Systems, falls doch eine käme
            var dialogs = Native.VisibleWindows(Path.GetFileName(helper)).Except(dialogsBefore).Count();
            Console.WriteLine(dialogs == 0 ? $"  ✔ {label}: keine Fehlermeldung auf dem Desktop" : $"  ✘ {label}: {dialogs} Fehlermeldung(en) auf dem Desktop");
            if (dialogs != 0) { failures++; }
        }
        var restartWatch = Stopwatch.StartNew();
        using var restarted = SandboxProcess.Start(helper, lessPrivileged);
        var hello = restarted.Call(Protocol.Hello, null, r => r.ReadString());
        Console.WriteLine($"  ✔ Neustart in {restartWatch.Elapsed.TotalMilliseconds:0} ms: {hello}");
    }
}
