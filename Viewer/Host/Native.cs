using System.Runtime.InteropServices;

namespace PDFLight.Host;

/// <summary>Die Windows-Funktionen, die das Hilfsprogramm braucht (PDFium selbst: <see cref="Pdfium"/>).</summary>
internal static unsafe partial class Native
{
    // ================================================================== EMF (Druckweg)

    // PDFium zeichnet vektoriell in eine EMF im Speicher, das Hauptprogramm spielt sie auf dem Drucker ab
    [LibraryImport("gdi32.dll", EntryPoint = "CreateEnhMetaFileW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint CreateEnhMetaFile(nint referenceDc, string? fileName, nint frame, string? description);
    [LibraryImport("gdi32.dll", EntryPoint = "CloseEnhMetaFile")] public static partial nint CloseEnhMetaFile(nint dc);
    [LibraryImport("gdi32.dll", EntryPoint = "GetEnhMetaFileBits")] public static partial uint GetEnhMetaFileBits(nint metafile, uint size, nint buffer);
    [LibraryImport("gdi32.dll", EntryPoint = "DeleteEnhMetaFile")] [return: MarshalAs(UnmanagedType.Bool)] public static partial bool DeleteEnhMetaFile(nint metafile);
    [LibraryImport("gdi32.dll", EntryPoint = "GetDeviceCaps")] public static partial int GetDeviceCaps(nint dc, int index);
    [LibraryImport("gdi32.dll", EntryPoint = "SetGraphicsMode")] public static partial int SetGraphicsMode(nint dc, int mode);
    [LibraryImport("gdi32.dll", EntryPoint = "SetWorldTransform")] [return: MarshalAs(UnmanagedType.Bool)] public static partial bool SetWorldTransform(nint dc, XForm* transform);

    public const int HorzSize = 4, VertSize = 6, HorzRes = 8, VertRes = 10; // Bezugsgerät: Maße in mm bzw. Pixeln
    public const int GraphicsModeAdvanced = 2;                              // GM_ADVANCED: Welttransformationen erlaubt

    [StructLayout(LayoutKind.Sequential)]
    public struct XForm
    {
        public float M11, M12, M21, M22, Dx, Dy;
    }

    // ================================================================== Prozess, Token (Selbstbericht)

    public const uint TokenQuery = 0x0008;
    public const int TokenIntegrityLevel = 25, TokenIsAppContainer = 29;

    [LibraryImport("kernel32.dll", EntryPoint = "GetCurrentProcess")] public static partial nint GetCurrentProcess();
    [LibraryImport("kernel32.dll", EntryPoint = "CloseHandle")] [return: MarshalAs(UnmanagedType.Bool)] public static partial bool CloseHandle(nint handle);
    [LibraryImport("kernel32.dll", EntryPoint = "TerminateProcess")] [return: MarshalAs(UnmanagedType.Bool)] public static partial bool TerminateProcess(nint process, uint exitCode);
    [LibraryImport("advapi32.dll", EntryPoint = "OpenProcessToken")] [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool OpenProcessToken(nint process, uint access, out nint token);
    [LibraryImport("advapi32.dll", EntryPoint = "GetTokenInformation")] [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetTokenInformation(nint token, int infoClass, nint buffer, int length, out int returned);
    [LibraryImport("advapi32.dll", EntryPoint = "GetSidSubAuthorityCount")] public static partial nint GetSidSubAuthorityCount(nint sid);
    [LibraryImport("advapi32.dll", EntryPoint = "GetSidSubAuthority")] public static partial nint GetSidSubAuthority(nint sid, uint index);

    // ================================================================== Abstürze (s. CrashGuard)

    public const uint SemFailCriticalErrors = 0x0001, SemNoGpFaultErrorBox = 0x0002, SemNoOpenFileErrorBox = 0x8000;
    public const uint ModuleFromAddressUnchangedRefCount = 0x4 | 0x2; // GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | _UNCHANGED_REFCOUNT

    [LibraryImport("kernel32.dll", EntryPoint = "SetErrorMode")] public static partial uint SetErrorMode(uint mode);
    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", StringMarshalling = StringMarshalling.Utf16)] public static partial nint GetModuleHandle(string? name);
    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleExW")] [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetModuleHandleEx(uint flags, nint address, out nint module);
    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleFileNameW")] public static partial uint GetModuleFileName(nint module, char* path, uint size);
    [LibraryImport("kernel32.dll", EntryPoint = "AddVectoredExceptionHandler")]
    public static partial nint AddVectoredExceptionHandler(uint first, delegate* unmanaged<ExceptionPointers*, int> handler);

    [StructLayout(LayoutKind.Sequential)]
    public struct ExceptionRecord
    {
        public uint ExceptionCode, ExceptionFlags;
        public ExceptionRecord* Next;
        public nint ExceptionAddress;
        public uint NumberParameters;
        public fixed ulong Information[15]; // Zugriffsverletzung: [0] = lesen/schreiben, [1] = Zieladresse
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ExceptionPointers
    {
        public ExceptionRecord* ExceptionRecord;
        public nint ContextRecord;
    }

    // ================================================================== Nur für die Proben des Prüfprogramms

    public const uint MemCommitReserve = 0x3000, MemRelease = 0x8000, PageExecuteReadWrite = 0x40;
    [LibraryImport("kernel32.dll", EntryPoint = "VirtualAlloc", SetLastError = true)] public static partial nint VirtualAlloc(nint address, nuint size, uint type, uint protect);
    [LibraryImport("kernel32.dll", EntryPoint = "VirtualFree")] [return: MarshalAs(UnmanagedType.Bool)] public static partial bool VirtualFree(nint address, nuint size, uint type);
    [LibraryImport("ucrtbase.dll", EntryPoint = "memset")] public static partial nint MemSet(nint destination, int value, nuint count); // Absturz außerhalb von PDFium
}
