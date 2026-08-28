using System.Diagnostics;
using Microsoft.Win32;

namespace PDFLight.Classes;

/// <summary>Erkennt installierte PDF-Programme über die App-Paths-Registrierung (vereinfachte Fassung der PDFMover-Programmliste).</summary>
internal static class ProgramFinder
{
    public const int MaxPrograms = 9; // Strg+1 … Strg+9

    private static readonly string[] KnownExeNames =
    [
        "Acrobat.exe",          // Adobe Acrobat
        "AcroRd32.exe",         // Acrobat Reader (32-Bit)
        "SumatraPDF.exe",
        "FoxitPDFReader.exe",
        "FoxitReader.exe",
        "FoxitPhantom.exe",
        "PDFXEdit.exe",         // PDF-XChange Editor
        "PDFXCview.exe",        // PDF-XChange Viewer
        "NitroPDF.exe",
        "pdf24-Toolbox.exe",
        "msedge.exe",
        "chrome.exe",
        "firefox.exe",
    ];

    /// <summary>Sucht bekannte PDF-Programme; Reihenfolge wie in der Liste oben, maximal MaxPrograms Einträge.</summary>
    public static List<string> DetectPrograms()
    {
        List<string> result = [];
        foreach (var exeName in KnownExeNames)
        {
            var path = GetAppPath(exeName);
            if (!string.IsNullOrEmpty(path) && File.Exists(path)
                && !result.Any(r => string.Equals(r, path, StringComparison.OrdinalIgnoreCase)))
            {
                result.Add(path);
                if (result.Count == MaxPrograms) { break; }
            }
        }
        return result;
    }

    /// <summary>Anzeigename eines Programms (Dateibeschreibung, sonst Dateiname).</summary>
    public static string GetDisplayName(string exePath)
    {
        try
        {
            var description = FileVersionInfo.GetVersionInfo(exePath).FileDescription;
            if (!string.IsNullOrWhiteSpace(description)) { return description.Trim(); }
        }
        catch (FileNotFoundException) { }
        return Path.GetFileNameWithoutExtension(exePath);
    }

    private static string GetAppPath(string exeName)
    {
        foreach (var root in new[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            try
            {
                using var key = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + exeName);
                if (key?.GetValue(null) is string value && value.Length > 0) { return value.Trim('"'); }
            }
            catch (Exception ex) when (ex is System.Security.SecurityException or IOException) { }
        }
        return null;
    }
}
