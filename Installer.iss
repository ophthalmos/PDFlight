; ============================================================================
; PDFlight – Inno-Setup-Skript
;
; Quelle ist der normale Release-Ordner: In Visual Studio einfach die
; Release-Konfiguration bauen, dann dieses Skript kompilieren.
;
; Voraussetzungen auf dem Zielrechner:
;   - .NET Desktop Runtime 10 (x64) — fehlt sie, zeigt Windows beim ersten
;     Start selbst einen Dialog mit Download-Link, daher keine Prüfung hier.
;   - WebView2-Runtime (auf Windows 10/11 in der Regel vorhanden; das Setup warnt, falls sie fehlt).
; ============================================================================

#define appName "PDFlight"
#define appVersion "0.1.0"
#define releaseDir "bin\Release\net10.0-windows"

[Setup]
AppId={{7E1B0A4C-5A34-4B7A-9C57-3D8A41C6F2B9}
AppName={#appName}
AppVersion={#appVersion}
AppVerName={#appName} {#appVersion} (64-Bit)
VersionInfoVersion={#appVersion}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
AppPublisher=Wilhelm Happe
AppCopyright=© 2026 W. Happe
UsePreviousAppDir=yes
DefaultDirName={autopf}\{#appName}
DefaultGroupName={#appName}
ChangesAssociations=yes
DisableWelcomePage=yes
DisableReadyPage=yes
SetupIconFile=PDFlight.ico
UninstallDisplayIcon={app}\{#appName}.exe
OutputDir=.
OutputBaseFilename={#appName}Setup
Compression=lzma2/ultra
SolidCompression=yes
DirExistsWarning=no
CloseApplications=yes
SetupMutex={#appName}_SetupMutex
WizardStyle=modern

[Languages]
Name: de; MessagesFile: "compiler:Languages\German.isl"

[Messages]
de.ConfirmUninstall=Sind Sie sicher, dass Sie %1 und alle zugehörigen Komponenten entfernen möchten? Vor einem Update ist keine Deinstallation erforderlich.

[CustomMessages]
de.Run={#appName} starten
de.DesktopIcon=Verknüpfung auf dem Desktop anlegen
de.FileAssoc={#appName} im "Öffnen mit"-Menü für PDF-Dateien anbieten
de.WebView2Missing=Die Microsoft-WebView2-Runtime wurde nicht gefunden.%n%n{#appName} benötigt sie für die PDF-Anzeige. Bitte laden Sie sie herunter von:%nhttps://developer.microsoft.com/microsoft-edge/webview2/%n%nDie Installation wird trotzdem fortgesetzt.

[Tasks]
Name: desktopicon; Description: "{cm:DesktopIcon}"; Flags: unchecked
Name: fileassoc; Description: "{cm:FileAssoc}"

[Files]
Source: "{#releaseDir}\*"; Excludes: "*.pdb"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#appName}"; Filename: "{app}\{#appName}.exe"
Name: "{autodesktop}\{#appName}"; Filename: "{app}\{#appName}.exe"; Tasks: desktopicon

[Registry]
; ProgID, damit PDFlight im "Öffnen mit"-Dialog erscheint (Standard-App bleibt Sache des Benutzers)
Root: HKLM; Subkey: "Software\Classes\{#appName}.Document"; ValueType: string; ValueData: "PDF-Datei"; Flags: uninsdeletekey; Tasks: fileassoc
Root: HKLM; Subkey: "Software\Classes\{#appName}.Document\DefaultIcon"; ValueType: string; ValueData: "{app}\{#appName}.exe,0"; Tasks: fileassoc
Root: HKLM; Subkey: "Software\Classes\{#appName}.Document\shell\open\command"; ValueType: string; ValueData: """{app}\{#appName}.exe"" ""%1"""; Tasks: fileassoc
Root: HKLM; Subkey: "Software\Classes\.pdf\OpenWithProgids"; ValueType: string; ValueName: "{#appName}.Document"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKLM; Subkey: "Software\Classes\Applications\{#appName}.exe\shell\open\command"; ValueType: string; ValueData: """{app}\{#appName}.exe"" ""%1"""; Flags: uninsdeletekey; Tasks: fileassoc

[Run]
Filename: "{app}\{#appName}.exe"; Description: "{cm:Run}"; Flags: nowait postinstall skipifsilent

; Hinweis: Die Benutzereinstellungen (%APPDATA%\PDFlight\settings.json) und der
; WebView2-Datenordner (%LOCALAPPDATA%\PDFlight) bleiben bei der Deinstallation erhalten.

[Code]
function InitializeSetup(): Boolean;
var
  WebView2Version: String;
begin
  Result := True;
  { WebView2-Runtime vorhanden? (.NET-Runtime prüft Windows beim ersten Start selbst) }
  if not RegQueryStringValue(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', WebView2Version) then
    if not RegQueryStringValue(HKCU, 'Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', WebView2Version) then
      MsgBox(CustomMessage('WebView2Missing'), mbInformation, MB_OK);
end;
