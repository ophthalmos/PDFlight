using System.Runtime.InteropServices;

namespace PDFLight.Classes;

/// <summary>Verschlüsselung mit Windows (DPAPI) für das angemeldete Benutzerkonto: andere Konten, ein anderer Rechner oder eine kopierte
/// Datei können die Daten nicht lesen. Genutzt für die gespeicherten Unterschriften und den Gemini-API-Schlüssel; jede Anwendung
/// gibt einen eigenen Zusatzwert (<c>entropy</c>) mit, damit sich die Dateien nicht gegenseitig entschlüsseln lassen.</summary>
internal static partial class DataProtection
{
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

    /// <summary>Verschlüsselt; null, wenn Windows es ablehnt.</summary>
    public static byte[]? Protect(byte[] plain, byte[] entropy) => Transform(plain, entropy, protect: true);

    /// <summary>Entschlüsselt; null bei fremden oder beschädigten Daten.</summary>
    public static byte[]? Unprotect(byte[] encrypted, byte[] entropy) => Transform(encrypted, entropy, protect: false);

    private static unsafe byte[]? Transform(byte[] input, byte[] entropyBytes, bool protect)
    {
        fixed (byte* inputPointer = input, entropyPointer = entropyBytes)
        {
            DataBlob dataIn = new() { Size = input.Length, Data = (nint)inputPointer };
            DataBlob entropy = new() { Size = entropyBytes.Length, Data = (nint)entropyPointer };
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
