using System.Runtime.InteropServices;

namespace PDFLight.Viewer;

/// <summary>Windows-Funktionen für den abgeschotteten Start (AppContainer, Prozessattribute, Job-Objekte).</summary>
internal static unsafe partial class SandboxNative
{
    // ================================================================== AppContainer (userenv.dll)

    public const int HResultAlreadyExists = unchecked((int)0x800700B7);

    [LibraryImport("userenv.dll", EntryPoint = "CreateAppContainerProfile", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int CreateAppContainerProfile(string name, string displayName, string description, nint capabilities, int capabilityCount, out nint sid);

    [LibraryImport("userenv.dll", EntryPoint = "DeriveAppContainerSidFromAppContainerName", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int DeriveAppContainerSidFromAppContainerName(string name, out nint sid);

    [LibraryImport("advapi32.dll", EntryPoint = "FreeSid")] public static partial nint FreeSid(nint sid);

    [StructLayout(LayoutKind.Sequential)]
    public struct SecurityCapabilities
    {
        public nint AppContainerSid;
        public nint Capabilities;
        public uint CapabilityCount;
        public uint Reserved;
    }

    // ================================================================== Prozess

    public const nint AttributeMitigationPolicy = 0x20007;
    public const nint AttributeHandleList = 0x20002;
    public const nint AttributeSecurityCapabilities = 0x20009;
    public const nint AttributeChildProcessPolicy = 0x2000E;
    public const nint AttributeAllApplicationPackagesPolicy = 0x2000F;
    public const uint ChildProcessRestricted = 0x01;          // PROCESS_CREATION_CHILD_PROCESS_RESTRICTED
    public const uint AllApplicationPackagesOptOut = 0x01;    // PROCESS_CREATION_ALL_APPLICATION_PACKAGES_OPT_OUT (LPAC)
    public const ulong MitigationStrictHandleChecks = 1UL << 24;
    public const ulong MitigationExtensionPointDisable = 1UL << 32;
    public const ulong MitigationImageLoadNoRemote = 1UL << 52;
    public const ulong MitigationImageLoadNoLowLabel = 1UL << 56;
    public const ulong MitigationImageLoadPreferSystem32 = 1UL << 60;
    public const ulong MitigationProhibitDynamicCode = 1UL << 36;     // Arbitrary Code Guard: kein neuer ausführbarer Speicher (nur ohne JIT)

    public const uint ExtendedStartupInfoPresent = 0x00080000, CreateSuspended = 0x00000004, CreateNoWindow = 0x08000000;
    public const int StartfUseStdHandles = 0x00000100;
    public const uint StillActive = 259;

    [StructLayout(LayoutKind.Sequential)]
    public struct StartupInfo
    {
        public int Size;
        public nint Reserved, Desktop, Title;
        public int X, Y, XSize, YSize, XCountChars, YCountChars, FillAttribute, Flags;
        public short ShowWindow, Reserved2Size;
        public nint Reserved2, StdInput, StdOutput, StdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StartupInfoEx
    {
        public StartupInfo StartupInfo;
        public nint AttributeList;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessInformation
    {
        public nint Process, Thread;
        public int ProcessId, ThreadId;
    }

    [LibraryImport("kernel32.dll", EntryPoint = "InitializeProcThreadAttributeList", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool InitializeProcThreadAttributeList(nint list, int count, int flags, ref nint size);

    [LibraryImport("kernel32.dll", EntryPoint = "UpdateProcThreadAttribute", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UpdateProcThreadAttribute(nint list, uint flags, nint attribute, nint value, nint size, nint previousValue, nint returnSize);

    [LibraryImport("kernel32.dll", EntryPoint = "DeleteProcThreadAttributeList")]
    public static partial void DeleteProcThreadAttributeList(nint list);

    [LibraryImport("kernel32.dll", EntryPoint = "CreateProcessW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool CreateProcess(string? applicationName, char* commandLine, nint processAttributes, nint threadAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool inheritHandles, uint creationFlags, nint environment, string? currentDirectory,
        StartupInfoEx* startupInfo, ProcessInformation* processInformation);

    [LibraryImport("kernel32.dll", EntryPoint = "ResumeThread")] public static partial uint ResumeThread(nint thread);
    [LibraryImport("kernel32.dll", EntryPoint = "TerminateProcess")] [return: MarshalAs(UnmanagedType.Bool)] public static partial bool TerminateProcess(nint process, uint exitCode);
    [LibraryImport("kernel32.dll", EntryPoint = "WaitForSingleObject")] public static partial uint WaitForSingleObject(nint handle, uint milliseconds);
    [LibraryImport("kernel32.dll", EntryPoint = "GetExitCodeProcess")] [return: MarshalAs(UnmanagedType.Bool)] public static partial bool GetExitCodeProcess(nint process, out uint exitCode);
    [LibraryImport("kernel32.dll", EntryPoint = "CloseHandle")] [return: MarshalAs(UnmanagedType.Bool)] public static partial bool CloseHandle(nint handle);

    // ================================================================== Job-Objekt

    public const int JobObjectBasicUiRestrictions = 4, JobObjectExtendedLimitInformation = 9;
    public const uint JobActiveProcess = 0x0008, JobProcessMemory = 0x0100, JobDieOnUnhandledException = 0x0400, JobKillOnClose = 0x2000;
    public const uint JobUiLimitAll = 0xFF; // HANDLES, READ/WRITECLIPBOARD, SYSTEMPARAMETERS, DISPLAYSETTINGS, GLOBALATOMS, DESKTOP, EXITWINDOWS

    [StructLayout(LayoutKind.Sequential)]
    public struct JobBasicLimitInformation
    {
        public long PerProcessUserTimeLimit, PerJobUserTimeLimit;
        public uint LimitFlags;
        public nuint MinimumWorkingSetSize, MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public nuint Affinity;
        public uint PriorityClass, SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IoCounters
    {
        public ulong ReadOperationCount, WriteOperationCount, OtherOperationCount, ReadTransferCount, WriteTransferCount, OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JobExtendedLimitInformation
    {
        public JobBasicLimitInformation BasicLimitInformation;
        public IoCounters IoInfo;
        public nuint ProcessMemoryLimit, JobMemoryLimit, PeakProcessMemoryUsed, PeakJobMemoryUsed;
    }

    [LibraryImport("kernel32.dll", EntryPoint = "CreateJobObjectW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint CreateJobObject(nint attributes, string? name);

    [LibraryImport("kernel32.dll", EntryPoint = "SetInformationJobObject", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetInformationJobObject(nint job, int infoClass, void* info, int length);

    public const uint ExitCodeTimeout = 0x5AD0_0001; // eigener Exit-Code für „nach Frist beendet“

    [LibraryImport("kernel32.dll", EntryPoint = "TerminateJobObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool TerminateJobObject(nint job, uint exitCode);

    [LibraryImport("kernel32.dll", EntryPoint = "AssignProcessToJobObject", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AssignProcessToJobObject(nint job, nint process);
}
