using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using PDFLight.Classes;

namespace PDFLight.Viewer;

/// <summary>Miniaturen der ersten Seite für die Startseite (<see cref="RecentFilesView"/>), gerendert im Hilfsprogramm wie jede Seite und
/// als PNG zwischengespeichert in %LOCALAPPDATA%\PDFlight\Thumbs – sonst müsste die Startseite bei jedem Erscheinen alle Dateien öffnen.
/// Der Schlüssel enthält Pfad, Änderungszeit und Größe: eine geänderte Datei bekommt eine neue Miniatur. Dateien ohne Miniatur
/// (Kennwort, keine PDF, zu groß, Darstellung gescheitert) merkt eine leere Datei, damit sie nicht bei jedem Erscheinen erneut geöffnet
/// werden.</summary>
internal static class RecentThumbnails
{
    /// <summary>Längste Seite der gespeicherten Miniatur in Pixeln – doppelt so groß wie bei 100 % gezeigt, für hohe Skalierungen.</summary>
    public const int RenderSize = 96;

    private const long MaxFileSize = 300L * 1024 * 1024; // größere Dateien nicht nur für eine Miniatur einlesen

    private static string Folder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PDFlight", "Thumbs");

    private static string CachePath(FileInfo file)
    {
        var key = $"{file.FullName.ToUpperInvariant()}|{file.LastWriteTimeUtc.Ticks}|{file.Length}";
        return Path.Combine(Folder, Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(key))) + ".png");
    }

    /// <summary>Aus dem Zwischenspeicher: Found = schon einmal versucht, Image = die Miniatur (null = es gibt keine).</summary>
    public static (bool Found, Bitmap? Image) TryLoad(FileInfo file)
    {
        try
        {
            var path = CachePath(file);
            if (!File.Exists(path)) { return (false, null); }
            if (new FileInfo(path).Length == 0) { return (true, null); }
            using var stream = File.OpenRead(path);
            using var image = Image.FromStream(stream);
            return (true, new Bitmap(image)); // vom Strom gelöst, die Datei bleibt nicht gesperrt
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or ExternalException) { return (false, null); }
    }

    /// <summary>Rendert die erste Seite im Hilfsprogramm und legt das Ergebnis ab; null, wenn es keine Miniatur gibt.</summary>
    public static async Task<Bitmap?> CreateAsync(FileInfo file, Func<bool> stillWanted)
    {
        var image = await RenderAsync(file, stillWanted);
        if (!stillWanted()) { image?.Dispose(); return null; } // abgebrochen: nichts merken, beim nächsten Mal erneut
        try
        {
            Directory.CreateDirectory(Folder);
            if (image == null) { File.WriteAllBytes(CachePath(file), []); }
            else { image.Save(CachePath(file), ImageFormat.Png); }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ExternalException) { } // dann eben beim nächsten Mal neu
        return image;
    }

    private static async Task<Bitmap?> RenderAsync(FileInfo file, Func<bool> stillWanted)
    {
        if (file.Length > MaxFileSize) { return null; }
        byte[] bytes;
        try { bytes = await File.ReadAllBytesAsync(file.FullName); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return null; }
        if (!PdfStatus.HasPdfHeader(bytes) || !stillWanted()) { return null; }
        PdfiumDocument document;
        try { document = await PdfiumDocument.OpenAsync(bytes, file.FullName); }
        catch (Exception ex) when (ex is PasswordRequiredException or InvalidOperationException) { return null; } // Kennwort, beschädigt, Hilfsprogramm abgestürzt
        using (document)
        {
            if (document.PageCount == 0) { return null; }
            var size = document.PageSizes[0];
            var scale = RenderSize / Math.Max(1f, Math.Max(size.Width, size.Height));
            var width = Math.Max(1, (int)Math.Round(size.Width * scale));
            var height = Math.Max(1, (int)Math.Round(size.Height * scale));
            return await document.RenderAsync(0, width, height, stillWanted);
        }
    }

    /// <summary>Entfernt Miniaturen, die zu keiner der Dateien mehr gehören (alte Stände, aus der Liste genommene Dateien).</summary>
    public static void Prune(IEnumerable<FileInfo> keep)
    {
        var wanted = keep.Select(f => Path.GetFileName(CachePath(f))).ToHashSet(StringComparer.OrdinalIgnoreCase);
        _ = Task.Run(() =>
        {
            try
            {
                if (!Directory.Exists(Folder)) { return; }
                foreach (var path in Directory.EnumerateFiles(Folder, "*.png"))
                {
                    if (!wanted.Contains(Path.GetFileName(path))) { File.Delete(path); }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { } // Aufräumen darf scheitern
        });
    }
}
