using System.Globalization;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using PDFLight.Controls;

namespace PDFLight.Classes;

internal static class FileUtil
{
    /// <summary>Ersetzt Umlaute/ß und entfernt alle übrigen diakritischen Zeichen ("Café" → "Cafe").</summary>
    public static string RemoveDiacritics(string s)
    {
        s = s.Replace("ß", "ss").Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("Ä", "Ae").Replace("Ö", "Oe").Replace("Ü", "Ue");
        var normalized = s.Normalize(NormalizationForm.FormD);
        StringBuilder builder = new();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) { builder.Append(c); }
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>Alle PDF-Dateien eines Ordners in natürlicher Sortierung (wie im Explorer).</summary>
    public static List<string> GetPdfFilesInFolder(string folder)
    {
        try
        {
            List<string> files = [.. Directory.EnumerateFiles(folder, "*.pdf")];
            files.Sort((a, b) => ShellInfo.CompareNatural(Path.GetFileName(a), Path.GetFileName(b)));
            return files;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return []; }
    }

    /// <summary>Grobe Prüfung, ob im Zielordner geschrieben werden darf (wie in PDFMover).</summary>
    public static bool HasFolderWritePermission(string destDir)
    {
        try
        {
            var rules = new DirectoryInfo(destDir).GetAccessControl().GetAccessRules(true, true, typeof(NTAccount));
            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.AccessControlType == AccessControlType.Allow) { return true; }
            }
            return false;
        }
        catch { return false; }
    }
}
