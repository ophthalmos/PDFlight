using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PDFLight.Classes;

/// <summary>Eine gespeicherte Unterschrift (Wunsch vom 03.11.2026 nach dem Vorbild von Acrobats „Unterschreiben“ und GNOME Papers):
/// gezeichnet als Striche oder aus einer Bilddatei. Striche: je Strich eine Punktfolge x0, y0, x1, y1 … in Anteilen der Breite
/// (x 0…1, y 0…<see cref="Aspect"/>, y nach unten) – so bleibt sie in jeder Größe scharf. Bild: PNG mit durchsichtigem Hintergrund.</summary>
internal sealed class Signature
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public List<float[]> Strokes { get; set; } = [];
    public float Aspect { get; set; } = 0.35f;   // Höhe / Breite
    public float LineWidth { get; set; } = 0.012f; // Strichstärke als Anteil der Breite
    public float? WidthPt { get; set; }          // Originalgröße in Punkt (Scan: aus seiner Auflösung, gezeichnet: wie im Feld); null = übliche Breite
    public string? Png { get; set; }             // Base64, nur bei importierten Bildern

    [JsonIgnore] public bool IsImage => Png != null;

    /// <summary>Originalbreite nur, wenn sie für eine Unterschrift taugt (1,5–16 cm) – ein Handyfoto mit 72 dpi würde sonst riesig.</summary>
    private static float? Plausible(float widthPt) => widthPt is >= 1.5f * 72 / 2.54f and <= 16 * 72 / 2.54f ? widthPt : null;

    /// <summary>Striche aus einer Zeichnung in Pixeln: auf ihr umschließendes Rechteck (samt halber Strichstärke) zugeschnitten und auf
    /// dessen Breite bezogen. Null, wenn nichts gezeichnet wurde.</summary>
    public static Signature? FromStrokes(IReadOnlyList<IReadOnlyList<PointF>> strokes, float penWidth, float dpi)
    {
        var points = strokes.SelectMany(s => s).ToList();
        if (points.Count == 0) { return null; }
        // etwas Rand um die Striche: die Anmerkung lässt sich so an ihrer Ecke greifen, ohne dass der Zeiger auf der Linie liegen muss
        var margin = Math.Max(penWidth * 1.5f, (points.Max(p => p.X) - points.Min(p => p.X)) * 0.025f);
        var (minX, minY) = (points.Min(p => p.X) - margin, points.Min(p => p.Y) - margin);
        var (maxX, maxY) = (points.Max(p => p.X) + margin, points.Max(p => p.Y) + margin);
        var width = Math.Max(maxX - minX, penWidth * 4);
        return new Signature
        {
            Strokes = [.. strokes.Where(s => s.Count > 0).Select(s => s.SelectMany(p => new[] { (p.X - minX) / width, (p.Y - minY) / width }).ToArray())],
            Aspect = Math.Max(maxY - minY, penWidth) / width,
            LineWidth = penWidth / width,
            WidthPt = Plausible(width / dpi * 72), // so groß, wie im Feld gezeichnet
        };
    }

    /// <summary>Unterschrift aus einem Bild (PNG mit Durchsichtigkeit, sonst wird der helle Hintergrund durchsichtig), auf den sichtbaren
    /// Teil zugeschnitten und auf höchstens 1200 px Breite verkleinert.</summary>
    public static Signature? FromImage(Bitmap source)
    {
        var scale = Math.Min(1.0, 1200.0 / Math.Max(1, source.Width));
        using Bitmap bitmap = new(Math.Max(1, (int)(source.Width * scale)), Math.Max(1, (int)(source.Height * scale)), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(source, 0, 0, bitmap.Width, bitmap.Height);
        }
        var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var pixels = new byte[data.Stride * data.Height];
        Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
        var transparent = 0;
        for (var i = 3; i < pixels.Length; i += 4) { if (pixels[i] < 250) { transparent++; } }
        if (transparent < pixels.Length / 4 / 20) // kaum Durchsichtiges: heller Hintergrund (Scan, Foto) wird durchsichtig
        {
            for (var i = 0; i < pixels.Length; i += 4)
            {
                var (b, gr, r) = (pixels[i], pixels[i + 1], pixels[i + 2]);
                var lightness = 0.299 * r + 0.587 * gr + 0.114 * b;
                var alpha = Math.Clamp((235 - lightness) / 110, 0, 1); // ab fast Weiß durchsichtig, schon mittleres Grau (Bleistift, blasser Scan) fast deckend
                // die Farbe der Tinte ohne den hellen Grund (Mischung mit Weiß zurückrechnen)
                byte Ink(byte c) => alpha <= 0 ? (byte)0 : (byte)Math.Clamp((c - 255 * (1 - alpha)) / alpha, 0, 255);
                (pixels[i], pixels[i + 1], pixels[i + 2], pixels[i + 3]) = (Ink(b), Ink(gr), Ink(r), (byte)(alpha * 255));
            }
        }
        // auf den sichtbaren Teil zuschneiden
        var (left, top, right, bottom) = (bitmap.Width, bitmap.Height, -1, -1);
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (pixels[y * data.Stride + x * 4 + 3] < 24) { continue; }
                (left, top, right, bottom) = (Math.Min(left, x), Math.Min(top, y), Math.Max(right, x), Math.Max(bottom, y));
            }
        }
        Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
        bitmap.UnlockBits(data);
        if (right < left || bottom < top) { return null; } // nichts zu sehen
        var crop = Rectangle.FromLTRB(Math.Max(0, left - 2), Math.Max(0, top - 2), Math.Min(bitmap.Width, right + 3), Math.Min(bitmap.Height, bottom + 3));
        using var cropped = bitmap.Clone(crop, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using MemoryStream png = new();
        cropped.Save(png, System.Drawing.Imaging.ImageFormat.Png);
        // Originalgröße aus der Auflösung der Datei (Scans meist 300 dpi); ohne Angabe meldet GDI+ meist 96 – dann ergibt das die Bildschirmgröße
        var dpi = source.HorizontalResolution > 1 ? source.HorizontalResolution : 96;
        return new Signature { Png = Convert.ToBase64String(png.ToArray()), Aspect = (float)crop.Height / crop.Width, WidthPt = Plausible((float)(crop.Width / scale / dpi * 72)) };
    }

    /// <summary>Das Bild eines importierten Unterschriftsbilds (null bei gezeichneten).</summary>
    public Bitmap? LoadImage()
    {
        if (Png == null) { return null; }
        using MemoryStream stream = new(Convert.FromBase64String(Png));
        using var image = Image.FromStream(stream);
        return new Bitmap(image);
    }

    /// <summary>Vorschau in <paramref name="bounds"/> (Seitenverhältnis bleibt, mittig). <paramref name="recolor"/>: auch ein Bild in der Farbe
    /// <paramref name="ink"/> zeigen (Durchsichtigkeit bleibt) – im Dunkelmodus wäre dunkle Tinte auf dunklem Grund kaum zu sehen.</summary>
    public void Draw(Graphics g, RectangleF bounds, Color ink, bool recolor = false)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0) { return; }
        var width = Math.Min(bounds.Width, bounds.Height / Aspect);
        var target = new RectangleF(bounds.X + (bounds.Width - width) / 2, bounds.Y + (bounds.Height - width * Aspect) / 2, width, width * Aspect);
        if (LoadImage() is { } image)
        {
            using (image)
            {
                var state = g.Save();
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                if (recolor)
                {
                    // Farbe durch die Tintenfarbe ersetzen, Alpha behalten
                    using System.Drawing.Imaging.ImageAttributes attributes = new();
                    attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(
                    [
                        [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [0, 0, 0, 1.8f, 0], // blasse Scan-Striche kräftiger
                        [ink.R / 255f, ink.G / 255f, ink.B / 255f, 0, 1],
                    ]));
                    g.DrawImage(image, Rectangle.Round(target), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
                else { g.DrawImage(image, target); }
                g.Restore(state);
            }
            return;
        }
        var saved = g.Save();
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using Pen pen = new(ink, Math.Max(1f, LineWidth * width)) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        foreach (var stroke in Strokes)
        {
            var points = Enumerable.Range(0, stroke.Length / 2).Select(i => new PointF(target.X + stroke[2 * i] * width, target.Y + stroke[2 * i + 1] * width)).ToArray();
            if (points.Length == 1) { g.DrawLine(pen, points[0], new PointF(points[0].X + 0.1f, points[0].Y)); }
            else if (points.Length > 1) { g.DrawCurve(pen, points, 0.5f); }
        }
        g.Restore(saved);
    }
}

/// <summary>Die gespeicherten Unterschriften: eine Datei im Einstellungsordner, mit Windows (DPAPI) für das Benutzerkonto verschlüsselt –
/// andere Konten, ein anderer Rechner oder eine kopierte Datei können sie nicht lesen (Entscheidung Wilhelms vom 03.11.2026).</summary>
internal static partial class SignatureStore
{
    public const int Limit = 5; // wie Acrobat: wenige, gut unterscheidbare Unterschriften

    public static string FilePath => Path.Combine(Path.GetDirectoryName(AppSettings.SettingsPath)!, "signatures.dat"); // der Einstellungsordner hat immer einen Elternordner
    private static readonly byte[] Entropy = "PDFlight.Signatures"u8.ToArray();

    private enum ReadResult { Ok, Busy, Unreadable }

    private static ReadResult Read(out List<Signature> signatures)
    {
        signatures = [];
        try
        {
            if (!File.Exists(FilePath)) { return ReadResult.Ok; }
            var json = Unprotect(File.ReadAllBytes(FilePath));
            if (json == null) { return ReadResult.Unreadable; }
            signatures = JsonSerializer.Deserialize(json, SignatureJsonContext.Default.ListSignature) ?? [];
            return ReadResult.Ok;
        }
        catch (JsonException) { return ReadResult.Unreadable; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return ReadResult.Busy; }
    }

    /// <summary>Zum Anzeigen: alle Unterschriften; leer, wenn es keine gibt oder die Datei gerade nicht zu lesen ist.</summary>
    public static List<Signature> Load()
    {
        _ = Read(out var signatures);
        return signatures;
    }

    /// <summary>Vor dem Ändern der Liste. False: Die Datei ist gerade nicht zu lesen (gesperrt) – dann darf nichts geschrieben werden, das
    /// Speichern einer leeren Liste löschte sonst alle Unterschriften (derselbe Fehler kostete am 20.09.2026 die Einstellungen). Eine Datei, die
    /// sich nie mehr lesen lässt (anderes Konto, beschädigt), kommt beiseite, sie soll neue Unterschriften nicht dauerhaft blockieren.</summary>
    public static bool TryLoadForChange(out List<Signature> signatures)
    {
        var result = Read(out signatures);
        if (result != ReadResult.Unreadable) { return result == ReadResult.Ok; }
        try
        {
            File.Move(FilePath, Path.ChangeExtension(FilePath, ".unreadable.dat"), overwrite: true);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return false; }
    }

    public static void Save(List<Signature> signatures)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        if (signatures.Count == 0) { File.Delete(FilePath); return; }
        var protectedBytes = Protect(JsonSerializer.SerializeToUtf8Bytes(signatures, SignatureJsonContext.Default.ListSignature));
        var temp = FilePath + ".tmp";
        File.WriteAllBytes(temp, protectedBytes);
        File.Move(temp, FilePath, overwrite: true);
    }

    // ------------------------------------------------------------------ DPAPI

    [StructLayout(LayoutKind.Sequential)]
    private struct DataBlob
    {
        public int Size;
        public nint Data;
    }

    private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;

    [LibraryImport("crypt32.dll", EntryPoint = "CryptProtectData", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CryptProtectData(ref DataBlob dataIn, string? description, ref DataBlob entropy, nint reserved, nint prompt, int flags, out DataBlob dataOut);

    [LibraryImport("crypt32.dll", EntryPoint = "CryptUnprotectData", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CryptUnprotectData(ref DataBlob dataIn, nint description, ref DataBlob entropy, nint reserved, nint prompt, int flags, out DataBlob dataOut);

    [LibraryImport("kernel32.dll", EntryPoint = "LocalFree")]
    private static partial nint LocalFree(nint memory);

    private static byte[] Protect(byte[] plain) => Transform(plain, protect: true) ?? throw new IOException("Die Unterschriften konnten nicht verschlüsselt werden.");

    private static byte[]? Unprotect(byte[] encrypted) => Transform(encrypted, protect: false);

    private static unsafe byte[]? Transform(byte[] input, bool protect)
    {
        fixed (byte* inputPointer = input, entropyPointer = Entropy)
        {
            DataBlob dataIn = new() { Size = input.Length, Data = (nint)inputPointer };
            DataBlob entropy = new() { Size = Entropy.Length, Data = (nint)entropyPointer };
            var ok = protect
                ? CryptProtectData(ref dataIn, "PDFlight", ref entropy, 0, 0, CRYPTPROTECT_UI_FORBIDDEN, out var output)
                : CryptUnprotectData(ref dataIn, 0, ref entropy, 0, 0, CRYPTPROTECT_UI_FORBIDDEN, out output);
            if (!ok) { return null; }
            try
            {
                var result = new byte[output.Size];
                Marshal.Copy(output.Data, result, 0, output.Size);
                return result;
            }
            finally { LocalFree(output.Data); }
        }
    }
}

[JsonSerializable(typeof(List<Signature>))]
internal sealed partial class SignatureJsonContext : JsonSerializerContext
{
}
