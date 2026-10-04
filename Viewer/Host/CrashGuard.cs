using System.Runtime.InteropServices;

namespace PDFLight.Host;

/// <summary>Ein Absturz soll den Prozess sofort und still beenden; das Hauptprogramm erkennt es am Ende der Pipe. Ohne Vorkehrungen
/// übernähme die Absturzbehandlung von .NET und Windows (Fehlerbericht, Ereignisprotokoll). Die braucht Systemdienste, die der Container
/// nicht erreichen darf, und fiel dann auf eine Meldung „pdfhost.exe – Systemfehler“ auf dem Desktop zurück, die den Prozess bis
/// zum Klick festhielt (bis zur Frist des Hauptprogramms – die Meldung blieb danach stehen). Dasselbe gilt für den harten Abbruch von
/// .NET (FailFast, „Unknown Hard Error“) – SetErrorMode unterdrückt diese Meldungen nicht. Deshalb darf das Hilfsprogramm nie in diese
/// Wege laufen, sondern beendet sich in jedem Fehlerfall selbst per TerminateProcess:
/// – SetErrorMode: keine Fehlerdialoge des Systems, soweit es sie unterdrücken kann;
/// – unbehandelte .NET-Ausnahmen: sofort beenden, bevor .NET abbricht;
/// – ein Ausnahme-Handler beendet den Prozess bei schweren Ausnahmen (Zugriffsverletzung, ungültiger Befehl, Stapelüberlauf,
///   Heap-Beschädigung …) in nativem Code – in PDFium ebenso wie in Bibliotheken, die PDFium aufruft (memcpy der C-Laufzeit, GDI).
///   Er ist .NET-Code und steht deshalb HINTER den Handlern von .NET: Läge er davor, riefe Windows ihn auch bei Fehlern in verwaltetem
///   Code auf, in einem Zustand, in dem .NET keinen Rückruf zulässt – .NET bräche dann hart ab (0xC0000602). So übersetzt .NET Fehler
///   in eigenem Code zuerst in Ausnahmen (NullReferenceException …), und der Handler sieht nur den Rest. Zur Sicherheit lässt er
///   trotzdem durch, was nach einer solchen Übersetzung aussieht: Nullzeiger und Division durch null in verwaltetem Code (mit Native
///   AOT die eigene EXE, mit JIT Code außerhalb jeder DLL bzw. in den Assemblies der .NET-Laufzeit).</summary>
internal static unsafe class CrashGuard
{
    private const uint ExitCodeUnhandled = 0xE0434352; // wie .NETs eigener Code für unbehandelte Ausnahmen
    private static nint pdfiumModule, exeModule;
    private static string runtimeDirectory = string.Empty;
    private static bool nativeAot;

    /// <summary>Nach dem Laden von pdfium.dll aufrufen (FPDF_InitLibrary) – erst dann ist ihr Modul bekannt.</summary>
    public static void Install()
    {
        _ = Native.SetErrorMode(Native.SemFailCriticalErrors | Native.SemNoGpFaultErrorBox | Native.SemNoOpenFileErrorBox);
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        pdfiumModule = Native.GetModuleHandle("pdfium.dll");
        exeModule = Native.GetModuleHandle(null);
        nativeAot = !System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported;
        runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
        _ = Native.AddVectoredExceptionHandler(0, &OnException); // 0 = hinter denen von .NET (s. oben)
    }

    private static void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        try { Console.Error.WriteLine(e.ExceptionObject); Console.Error.Flush(); } // Grund für das Hauptprogramm (Fehlerausgabe)
        finally { _ = Native.TerminateProcess(Native.GetCurrentProcess(), ExitCodeUnhandled); }
    }

    [UnmanagedCallersOnly]
    private static int OnException(Native.ExceptionPointers* pointers)
    {
        var record = pointers->ExceptionRecord;
        if (IsFatal(record->ExceptionCode) && !IsManagedException(record))
        {
            _ = Native.TerminateProcess(Native.GetCurrentProcess(), record->ExceptionCode);
        }
        return 0; // EXCEPTION_CONTINUE_SEARCH
    }

    private static bool IsFatal(uint code) => code is 0xC0000005 or 0xC0000006 or 0xC000001D or 0xC0000096 or 0xC0000094 or 0xC0000095
        or 0xC00000FD or 0xC0000374 or 0xC000008C or 0x80000003;

    /// <summary>Eine Ausnahme, die .NET in eine verwaltete Ausnahme übersetzt: Nullzeiger-Zugriff oder Division durch null in
    /// verwaltetem Code.</summary>
    private static bool IsManagedException(Native.ExceptionRecord* record)
    {
        var nullAccess = record->ExceptionCode == 0xC0000005 && record->NumberParameters >= 2 && record->Information[1] < 0x10000;
        if (!nullAccess && record->ExceptionCode is not (0xC0000094 or 0xC0000095)) { return false; }
        if (!Native.GetModuleHandleEx(Native.ModuleFromAddressUnchangedRefCount, record->ExceptionAddress, out var module)) { module = 0; }
        if (module == pdfiumModule && module != 0) { return false; }
        if (nativeAot) { return module == exeModule; }
        if (module == 0) { return true; } // vom JIT erzeugter Code liegt in keiner DLL
        var path = stackalloc char[520];
        var length = Native.GetModuleFileName(module, path, 520);
        return length > 0 && new string(path, 0, (int)length).StartsWith(runtimeDirectory, StringComparison.OrdinalIgnoreCase);
    }
}
