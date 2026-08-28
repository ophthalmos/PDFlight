using System.Runtime.InteropServices;

namespace PDFLight.Classes;

/// <summary>
/// Fallback-Weg für den E-Mail-Versand über Simple MAPI (primär ist MailSender/SendMail-DropTarget).
/// Öffnet das "Neue E-Mail"-Fenster des registrierten MAPI-Clients mit Dateianhang. Zuerst wird die
/// Unicode-Variante versucht, bei Nichtunterstützung die ANSI-Variante. Der Aufruf blockiert bis zum
/// Schließen des E-Mail-Fensters und gehört deshalb auf einen Hintergrund-Thread.
/// </summary>
internal static partial class MapiMailer
{
    private const int MAPI_LOGON_UI = 0x0001;
    private const int MAPI_DIALOG = 0x0008;
    private const int SUCCESS_SUCCESS = 0;
    private const int MAPI_E_USER_ABORT = 1;

    // Blittable Strukturen (Strings als Zeiger) — dasselbe Layout gilt für die W- und die ANSI-Variante
    [StructLayout(LayoutKind.Sequential)]
    private struct MapiMessage
    {
        public int Reserved;
        public IntPtr Subject;
        public IntPtr NoteText;
        public IntPtr MessageType;
        public IntPtr DateReceived;
        public IntPtr ConversationID;
        public int Flags;
        public IntPtr Originator;
        public int RecipCount;
        public IntPtr Recips;
        public int FileCount;
        public IntPtr Files;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MapiFileDesc
    {
        public int Reserved;
        public int Flags;
        public int Position;
        public IntPtr PathName;
        public IntPtr FileName;
        public IntPtr FileType;
    }

    [LibraryImport("MAPI32.DLL", EntryPoint = "MAPISendMailW")]
    private static partial int MAPISendMailW(IntPtr session, IntPtr hwnd, ref MapiMessage message, int flags, int reserved);

    [LibraryImport("MAPI32.DLL", EntryPoint = "MAPISendMail")]
    private static partial int MAPISendMailA(IntPtr session, IntPtr hwnd, ref MapiMessage message, int flags, int reserved);

    /// <summary>Erstellt eine neue E-Mail mit der Datei als Anhang; null bei Erfolg oder Benutzerabbruch, sonst eine deutsche Fehlermeldung.</summary>
    public static string SendWithAttachment(string filePath, string subject)
    {
        int result;
        try { result = Send(filePath, subject, unicode: true); }
        catch (Exception ex) when (ex is EntryPointNotFoundException or DllNotFoundException or SEHException) { result = -1; }

        if (result is not (SUCCESS_SUCCESS or MAPI_E_USER_ABORT)) // Unicode nicht unterstützt oder fehlgeschlagen → ANSI versuchen
        {
            try { result = Send(filePath, subject, unicode: false); }
            catch (Exception ex) when (ex is EntryPointNotFoundException or DllNotFoundException or SEHException)
            {
                return "Auf diesem System ist keine MAPI-Unterstützung vorhanden (MAPI32.DLL).";
            }
        }
        return result is SUCCESS_SUCCESS or MAPI_E_USER_ABORT ? null : TranslateError(result);
    }

    private static int Send(string filePath, string subject, bool unicode)
    {
        var subjectPtr = AllocString(subject, unicode);
        var pathPtr = AllocString(filePath, unicode);
        var namePtr = AllocString(Path.GetFileName(filePath), unicode);
        var filesPtr = Marshal.AllocHGlobal(Marshal.SizeOf<MapiFileDesc>());
        try
        {
            MapiFileDesc file = new() { Position = -1, PathName = pathPtr, FileName = namePtr };
            Marshal.StructureToPtr(file, filesPtr, false);
            MapiMessage message = new() { Subject = subjectPtr, FileCount = 1, Files = filesPtr };
            return unicode
                ? MAPISendMailW(IntPtr.Zero, IntPtr.Zero, ref message, MAPI_DIALOG | MAPI_LOGON_UI, 0)
                : MAPISendMailA(IntPtr.Zero, IntPtr.Zero, ref message, MAPI_DIALOG | MAPI_LOGON_UI, 0);
        }
        finally
        {
            Marshal.FreeHGlobal(filesPtr);
            Marshal.FreeHGlobal(subjectPtr);
            Marshal.FreeHGlobal(pathPtr);
            Marshal.FreeHGlobal(namePtr);
        }
    }

    private static IntPtr AllocString(string value, bool unicode)
    {
        return unicode ? Marshal.StringToHGlobalUni(value) : Marshal.StringToHGlobalAnsi(value);
    }

    private static string TranslateError(int code)
    {
        return code switch
        {
            2 => "Allgemeiner MAPI-Fehler (kein Standard-Mailprogramm eingerichtet?).",
            3 => "Die Anmeldung beim Mailprogramm ist fehlgeschlagen.",
            4 => "Der Datenträger ist voll.",
            5 => "Nicht genügend Arbeitsspeicher.",
            11 => "Die Anhang-Datei wurde nicht gefunden.",
            12 => "Die Anhang-Datei konnte nicht geöffnet werden.",
            26 => "Das Mailprogramm unterstützt diese Funktion nicht.",
            _ => "MAPI-Fehlercode " + code + ".",
        };
    }
}
