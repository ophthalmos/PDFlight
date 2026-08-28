using System.Globalization;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
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

    /// <summary>Schlägt "name_1.pdf" usw. vor — den nächsten freien _n-Index im Zielordner (wie in PDFMover).</summary>
    public static FileInfo SuggestAdditionalFileName(FileInfo destFile)
    {
        var folder = destFile.DirectoryName;
        var extension = destFile.Extension;
        var nameNoExt = Path.GetFileNameWithoutExtension(destFile.FullName);
        var foundIndex = 0;
        try
        {
            foreach (var file in Directory.EnumerateFiles(folder, nameNoExt + "_*" + extension))
            {
                var match = Regex.Match(Path.GetFileName(file), "^" + Regex.Escape(nameNoExt) + @"_(\d+)" + Regex.Escape(extension) + "$", RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var index) && index > foundIndex) { foundIndex = index; }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return null; }
        return new FileInfo(Path.Combine(folder, nameNoExt + "_" + (foundIndex + 1) + extension));
    }

    /// <summary>Sucht im Ordner eine inhaltsgleiche PDF-Datei (gleiche Größe und gleicher Hash, wie in PDFMover); null, wenn keine existiert.</summary>
    public static FileInfo FindDuplicateInFolder(FileInfo file, string folder)
    {
        try
        {
            byte[] hash = null; // erst berechnen, wenn es überhaupt einen Kandidaten gleicher Größe gibt
            foreach (var candidate in Directory.EnumerateFiles(folder, "*.pdf"))
            {
                if (string.Equals(candidate, file.FullName, StringComparison.OrdinalIgnoreCase)) { continue; }
                FileInfo candidateInfo = new(candidate);
                if (candidateInfo.Length != file.Length) { continue; }
                hash ??= ComputeHash(file.FullName);
                if (hash.AsSpan().SequenceEqual(ComputeHash(candidate))) { return candidateInfo; }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        return null;
    }

    private static byte[] ComputeHash(string path)
    {
        using var stream = File.OpenRead(path);
        return SHA256.HashData(stream);
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
