using System.ComponentModel;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace PDFLight.Viewer;

/// <summary>Startet das Hilfsprogramm pdfhost abgeschottet und spricht über anonyme Pipes mit ihm.
/// Abschottung:
/// – AppContainer (wahlweise „less privileged“, LPAC): eigenes Sicherheitstoken ohne Zugriff auf Benutzerdateien, Netz, Loopback und
///   Benutzer-Registry; lesbar ist nur, was ausdrücklich für den Container (bzw. „alle Anwendungspakete“) freigegeben ist – das
///   Programmverzeichnis gibt <see cref="GrantFolderAccess"/> frei, die .NET-Laufzeit unter Program Files ist es von Haus aus.
/// – keine Kindprozesse (Prozessattribut und Job-Limit), Job-Objekt mit Speichergrenze, Kill-on-Close und UI-Beschränkungen.
/// – Mitigations: keine DLLs von Netzlaufwerken oder mit niedriger Integritätsstufe, keine Erweiterungspunkte, strenge Handle-Prüfung;
///   beim Native-AOT-Hilfsprogramm zusätzlich Arbitrary Code Guard (kein zur Laufzeit erzeugter Maschinencode).
/// Die Pipe-Handles erbt das Hilfsprogramm (Handle-Liste) – nur diese drei, keine anderen Handles des Hauptprogramms.</summary>
internal sealed class SandboxProcess : IDisposable
{
    public const string ContainerName = "PDFlight.pdfhost";
    private const long ProcessMemoryLimit = 2L << 30; // 2 GB

    private readonly AnonymousPipeServerStream toChild, fromChild, errorFromChild;
    private readonly BinaryWriter writer;
    private readonly BinaryReader reader;
    private readonly StringBuilder errorText = new();
    private nint process, job;

    public int ProcessId { get; }
    public bool LessPrivileged { get; }
    public bool InContainer { get; }

    /// <summary>Das Hilfsprogramm ist mit Native AOT übersetzt (kein JIT, keine pdfhost.dll daneben) – dann gilt zusätzlich das
    /// Verbot, zur Laufzeit ausführbaren Code zu erzeugen (Arbitrary Code Guard).</summary>
    public bool ProhibitsDynamicCode { get; }

    /// <summary>Was das Hilfsprogramm auf die Standard-Fehlerausgabe geschrieben hat (Startfehler der .NET-Laufzeit u. ä.).</summary>
    public string ErrorOutput { get { lock (errorText) { return errorText.ToString(); } } }

    private SandboxProcess(string exePath, bool lessPrivileged)
    {
        LessPrivileged = lessPrivileged && !Without("container") && !Without("lpac");
        InContainer = !Without("container");
        ProhibitsDynamicCode = !File.Exists(Path.ChangeExtension(exePath, ".dll"));
        toChild = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
        fromChild = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
        errorFromChild = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
        try
        {
            var sid = ContainerSid();
            GrantFolderAccess(Path.GetDirectoryName(exePath)!, sid, LessPrivileged); // exePath ist ein voller Pfad
            (process, job, ProcessId) = Launch(exePath, sid, lessPrivileged, ProhibitsDynamicCode,
                toChild.ClientSafePipeHandle.DangerousGetHandle(), fromChild.ClientSafePipeHandle.DangerousGetHandle(),
                errorFromChild.ClientSafePipeHandle.DangerousGetHandle());
        }
        catch
        {
            DisposePipes();
            throw;
        }
        // die Enden des Hilfsprogramms gehören jetzt ihm – sonst merkten wir sein Ende nicht (die Pipe bliebe offen)
        toChild.DisposeLocalCopyOfClientHandle();
        fromChild.DisposeLocalCopyOfClientHandle();
        errorFromChild.DisposeLocalCopyOfClientHandle();
        writer = new BinaryWriter(toChild);
        reader = new BinaryReader(fromChild);
        _ = Task.Run(CollectErrorOutput);
    }

    /// <summary>Startet das Hilfsprogramm. Ohne Angabe: das Native-AOT-Hilfsprogramm im normalen AppContainer mit Arbitrary Code Guard,
    /// das JIT-Hilfsprogramm (Entwicklung) im strengeren LPAC – die AOT-Laufzeit startet in LPAC nicht (Exit-Code −1 noch vor Main,
    /// Ursache offen), und der Code Guard wiegt schwerer.</summary>
    public static SandboxProcess Start(string exePath, bool? lessPrivileged = null)
    {
        exePath = Path.GetFullPath(exePath);
        return new(exePath, lessPrivileged ?? File.Exists(Path.ChangeExtension(exePath, ".dll")));
    }

    private void CollectErrorOutput()
    {
        using var text = new StreamReader(errorFromChild);
        var buffer = new char[1024];
        int read;
        try
        {
            while ((read = text.Read(buffer, 0, buffer.Length)) > 0)
            {
                lock (errorText) { errorText.Append(buffer, 0, read); }
            }
        }
        catch (IOException) { } // Hilfsprogramm beendet
    }

    // ================================================================== Befehle

    /// <summary>Frist je Auftrag: Antwortet das Hilfsprogramm nicht rechtzeitig (PDFium hängt, Endlosschleife in einer präparierten
    /// Datei, ein Absturz, der nicht zum Ende führt), wird es hart beendet.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Ereignisse, die das Hilfsprogramm vor einer Antwort meldet (Protocol.EventInvalidate …): Kennung und Leser – der Handler
    /// muss die Nutzdaten vollständig lesen. Läuft auf dem Thread, der <see cref="Call"/> aufruft.</summary>
    public Action<byte, BinaryReader>? EventReceived { get; set; }

    /// <summary>Sendet einen Befehl und liest die Antwort. Fehler des Hilfsprogramms → <see cref="SandboxErrorException"/>; ist es
    /// abgestürzt, beendet oder nach <see cref="Timeout"/> abgebrochen → <see cref="SandboxCrashedException"/>. Nicht thread-sicher:
    /// immer nur ein Befehl zugleich.</summary>
    public T Call<T>(byte command, Action<BinaryWriter>? arguments, Func<BinaryReader, T> result)
    {
        var timedOut = false;
        // beendet den ganzen Job: das schließt seine Pipe-Enden, und das blockierte Lesen hier endet mit einer Ausnahme
        using var watchdog = new System.Threading.Timer(_ => { timedOut = true; _ = SandboxNative.TerminateJobObject(job, SandboxNative.ExitCodeTimeout); }, null, Timeout, System.Threading.Timeout.InfiniteTimeSpan);
        try
        {
            writer.Write(command);
            arguments?.Invoke(writer);
            writer.Flush();
            var status = reader.ReadByte();
            while (status is not (Protocol.Ok or Protocol.Error))
            {
                if (EventReceived is not { } handler) { throw new InvalidDataException($"Unerwartetes Ereignis {status} ohne Empfänger."); }
                handler(status, reader);
                status = reader.ReadByte();
            }
            if (status == Protocol.Error) { throw new SandboxErrorException(reader.ReadString()); }
            return result(reader);
        }
        catch (Exception ex) when (ex is IOException or EndOfStreamException or ObjectDisposedException)
        {
            throw new SandboxCrashedException(ExitCode(), timedOut, ErrorOutput.Trim(), ex);
        }
    }

    /// <summary>Exit-Code des Hilfsprogramms (wartet bis 2 s auf sein Ende); null, solange es läuft.</summary>
    public uint? ExitCode()
    {
        if (process == 0) { return null; }
        _ = SandboxNative.WaitForSingleObject(process, 2000);
        return SandboxNative.GetExitCodeProcess(process, out var code) && code != SandboxNative.StillActive ? code : null;
    }

    // ================================================================== AppContainer

    private static SecurityIdentifier ContainerSid()
    {
        var result = SandboxNative.CreateAppContainerProfile(ContainerName, "PDFlight – PDF-Darstellung", "Abgeschotteter PDF-Prozess von PDFlight", 0, 0, out var sid);
        if (result == SandboxNative.HResultAlreadyExists) { result = SandboxNative.DeriveAppContainerSidFromAppContainerName(ContainerName, out sid); }
        Marshal.ThrowExceptionForHR(result);
        try { return new SecurityIdentifier(sid); }
        finally { _ = SandboxNative.FreeSid(sid); }
    }

    /// <summary>Gibt dem Container Lesen und Ausführen für den Programmordner (vererbt auf alle Dateien darin) – nur, falls er es nicht
    /// schon darf. Unter Program Files ist das von Haus aus so (Windows gibt dort „alle Anwendungspakete“ und „alle eingeschränkten
    /// Anwendungspakete“ frei); ändern dürfte PDFlight die Rechte dort ohnehin nicht. Schreiben darf der Container nirgends.</summary>
    private static void GrantFolderAccess(string folder, SecurityIdentifier sid, bool lessPrivileged)
    {
        var directory = new DirectoryInfo(folder);
        var security = directory.GetAccessControl();
        // LPAC zählt nicht zu „alle Anwendungspakete“ (S-1-15-2-1), nur zu „alle eingeschränkten Anwendungspakete“ (S-1-15-2-2)
        SecurityIdentifier[] accepted = lessPrivileged ? [sid, new("S-1-15-2-2")] : [sid, new("S-1-15-2-1"), new("S-1-15-2-2")];
        var granted = security.GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier))
            .OfType<FileSystemAccessRule>()
            .Any(r => accepted.Contains(r.IdentityReference) && r.AccessControlType == AccessControlType.Allow && r.FileSystemRights.HasFlag(FileSystemRights.ReadAndExecute));
        if (granted) { return; }
        security.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.ReadAndExecute,
            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
        try { directory.SetAccessControl(security); }
        catch (UnauthorizedAccessException) { } // keine Rechte am Ordner: Dann scheitert gleich der Start mit einer klaren Meldung
    }

    private static unsafe (nint Process, nint Job, int ProcessId) Launch(string exePath, SecurityIdentifier sid, bool lessPrivileged, bool prohibitDynamicCode, nint stdIn, nint stdOut, nint stdErr)
    {
        var sidBytes = new byte[sid.BinaryLength];
        sid.GetBinaryForm(sidBytes, 0);
        var unmanaged = new List<nint>();
        nint Alloc(int size) { var p = Marshal.AllocHGlobal(size); unmanaged.Add(p); return p; }
        nint attributes = 0;
        var job = SandboxNative.CreateJobObject(0, null);
        try
        {
            if (job == 0) { throw new Win32Exception(); }
            if (!Without("job")) { LimitJob(job); }

            var sidMemory = Alloc(sidBytes.Length);
            Marshal.Copy(sidBytes, 0, sidMemory, sidBytes.Length);
            var capabilities = (SandboxNative.SecurityCapabilities*)Alloc(sizeof(SandboxNative.SecurityCapabilities));
            *capabilities = new SandboxNative.SecurityCapabilities { AppContainerSid = sidMemory }; // keine Capabilities: kein Netz, keine Bibliotheken
            var handles = (nint*)Alloc(3 * sizeof(nint));
            handles[0] = stdIn; handles[1] = stdOut; handles[2] = stdErr;
            var noChildren = (uint*)Alloc(sizeof(uint));
            *noChildren = SandboxNative.ChildProcessRestricted;
            var optOut = (uint*)Alloc(sizeof(uint));
            *optOut = SandboxNative.AllApplicationPackagesOptOut;
            var mitigations = (ulong*)Alloc(sizeof(ulong));
            *mitigations = SandboxNative.MitigationStrictHandleChecks | SandboxNative.MitigationExtensionPointDisable | SandboxNative.MitigationImageLoadNoRemote
                | SandboxNative.MitigationImageLoadNoLowLabel | SandboxNative.MitigationImageLoadPreferSystem32
                | (prohibitDynamicCode ? SandboxNative.MitigationProhibitDynamicCode : 0);

            var container = !Without("container");
            lessPrivileged &= container && !Without("lpac");
            var count = 1 + (container ? 1 : 0) + (lessPrivileged ? 1 : 0) + (Without("child") ? 0 : 1) + (Without("mitigation") ? 0 : 1);
            nint size = 0;
            _ = SandboxNative.InitializeProcThreadAttributeList(0, count, 0, ref size); // liefert nur die nötige Größe
            attributes = Marshal.AllocHGlobal(size);
            if (!SandboxNative.InitializeProcThreadAttributeList(attributes, count, 0, ref size)) { throw new Win32Exception(); }
            if (container) { Update(attributes, SandboxNative.AttributeSecurityCapabilities, (nint)capabilities, sizeof(SandboxNative.SecurityCapabilities)); }
            Update(attributes, SandboxNative.AttributeHandleList, (nint)handles, 3 * sizeof(nint));
            if (!Without("child")) { Update(attributes, SandboxNative.AttributeChildProcessPolicy, (nint)noChildren, sizeof(uint)); }
            if (!Without("mitigation")) { Update(attributes, SandboxNative.AttributeMitigationPolicy, (nint)mitigations, sizeof(ulong)); }
            if (lessPrivileged) { Update(attributes, SandboxNative.AttributeAllApplicationPackagesPolicy, (nint)optOut, sizeof(uint)); }

            var startup = new SandboxNative.StartupInfoEx
            {
                StartupInfo = new SandboxNative.StartupInfo
                {
                    Size = sizeof(SandboxNative.StartupInfoEx),
                    Flags = SandboxNative.StartfUseStdHandles,
                    StdInput = stdIn,
                    StdOutput = stdOut,
                    StdError = stdErr,
                },
                AttributeList = attributes,
            };
            var commandLine = $"\"{exePath}\"\0".ToCharArray(); // CreateProcessW darf die Befehlszeile verändern – eigener Puffer
            SandboxNative.ProcessInformation info;
            fixed (char* command = commandLine)
            {
                if (!SandboxNative.CreateProcess(exePath, command, 0, 0, true,
                    SandboxNative.ExtendedStartupInfoPresent | SandboxNative.CreateSuspended | SandboxNative.CreateNoWindow,
                    0, Path.GetDirectoryName(exePath), &startup, &info))
                {
                    throw new Win32Exception();
                }
            }
            // erst in den Job, dann loslaufen lassen – so gilt jede Grenze vom ersten Befehl an
            if (!SandboxNative.AssignProcessToJobObject(job, info.Process))
            {
                var error = new Win32Exception();
                _ = SandboxNative.TerminateProcess(info.Process, 1);
                _ = SandboxNative.CloseHandle(info.Thread);
                _ = SandboxNative.CloseHandle(info.Process);
                throw error;
            }
            _ = SandboxNative.ResumeThread(info.Thread);
            _ = SandboxNative.CloseHandle(info.Thread);
            return (info.Process, job, info.ProcessId);
        }
        catch
        {
            if (job != 0) { _ = SandboxNative.CloseHandle(job); }
            throw;
        }
        finally
        {
            if (attributes != 0)
            {
                SandboxNative.DeleteProcThreadAttributeList(attributes);
                Marshal.FreeHGlobal(attributes);
            }
            foreach (var p in unmanaged) { Marshal.FreeHGlobal(p); }
        }
    }

    private static void Update(nint list, nint attribute, nint value, int size)
    {
        if (!SandboxNative.UpdateProcThreadAttribute(list, 0, attribute, value, size, 0, 0)) { throw new Win32Exception(); }
    }

    private static unsafe void LimitJob(nint job)
    {
        var limits = new SandboxNative.JobExtendedLimitInformation
        {
            BasicLimitInformation = new SandboxNative.JobBasicLimitInformation
            {
                LimitFlags = SandboxNative.JobKillOnClose | SandboxNative.JobActiveProcess | SandboxNative.JobProcessMemory | SandboxNative.JobDieOnUnhandledException,
                ActiveProcessLimit = 1,
            },
            ProcessMemoryLimit = (nuint)ProcessMemoryLimit,
        };
        if (!SandboxNative.SetInformationJobObject(job, SandboxNative.JobObjectExtendedLimitInformation, &limits, sizeof(SandboxNative.JobExtendedLimitInformation))) { throw new Win32Exception(); }
        var ui = SandboxNative.JobUiLimitAll; // keine fremden Fenster-Handles, Zwischenablage, Systemeinstellungen, Desktops, Abmelden …
        if (!Without("ui") && !SandboxNative.SetInformationJobObject(job, SandboxNative.JobObjectBasicUiRestrictions, &ui, sizeof(uint))) { throw new Win32Exception(); }
    }

    /// <summary>Zur Fehlersuche: Umgebungsvariable SANDBOX_OHNE=container,lpac,child,mitigation,ui,job schaltet einzelne Schutzmaßnahmen ab.</summary>
    internal static bool Without(string protection) =>
        (Environment.GetEnvironmentVariable("SANDBOX_OHNE") ?? string.Empty).Split(',').Contains(protection);

    // ================================================================== Aufräumen

    private void DisposePipes()
    {
        toChild.Dispose();
        fromChild.Dispose();
        errorFromChild.Dispose();
    }

    public void Dispose()
    {
        try
        {
            writer?.Write(Protocol.Quit);
            writer?.Flush();
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException) { } // schon beendet
        DisposePipes();
        if (process != 0) { _ = SandboxNative.WaitForSingleObject(process, 2000); _ = SandboxNative.CloseHandle(process); process = 0; }
        if (job != 0) { _ = SandboxNative.CloseHandle(job); job = 0; } // Kill-on-Close: was noch läuft, endet spätestens hier
    }
}

/// <summary>Das Hilfsprogramm hat einen Befehl abgelehnt (ungültige Datei, Seite fehlt …) – die Meldung kommt von ihm.</summary>
internal sealed class SandboxErrorException(string message) : InvalidOperationException(message);

/// <summary>Das Hilfsprogramm ist abgestürzt oder hat sich beendet; <see cref="ExitCode"/> ist sein Exit-Code (null = unbekannt),
/// <see cref="ErrorOutput"/> was es zuletzt auf die Fehlerausgabe geschrieben hat.</summary>
internal sealed class SandboxCrashedException(uint? exitCode, bool timedOut, string errorOutput, Exception inner)
    : InvalidOperationException((timedOut ? "Das Hilfsprogramm hat nicht rechtzeitig geantwortet und wurde beendet" : "Das Hilfsprogramm ist beendet") +
        $" (Exit-Code {(exitCode is { } c ? $"0x{c:X8}" : "unbekannt")}).", inner)
{
    public uint? ExitCode { get; } = exitCode;
    public bool TimedOut { get; } = timedOut;
    public string ErrorOutput { get; } = errorOutput;
}
