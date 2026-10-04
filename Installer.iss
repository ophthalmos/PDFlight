; ============================================================================
; PDFlight – Inno-Setup-Skript
;
; Voraussetzungen auf dem Zielrechner:
;   - .NET Desktop Runtime 10 (x64) — fehlt sie, zeigt Windows beim ersten
;     Start selbst einen Dialog mit Download-Link, daher keine Prüfung hier.
; Die PDF-Anzeige (PDFium) bringt PDFlight selbst mit: pdfhost.exe (Native AOT)
; und pdfium.dll liegen neben PDFlight.exe.
; ============================================================================

#define appName "PDFlight"
#define appVersion "1.1.0"
#define releaseDir "bin\x64\Release\net10.0-windows"

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
LicenseFile=LICENSE
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
; HKCU wird bewusst benutzt (nur Deinstallations-Vormerkung der ProgID, die das Programm selbst schreibt)
UsedUserAreasWarning=no

[Languages]
; Das Setup wählt die Sprache automatisch nach der Windows-Sprache; erste = Rückfall
Name: en; MessagesFile: "compiler:Default.isl"
Name: de; MessagesFile: "compiler:Languages\German.isl"
Name: fr; MessagesFile: "compiler:Languages\French.isl"
Name: es; MessagesFile: "compiler:Languages\Spanish.isl"

[Messages]
en.ConfirmUninstall=Are you sure you want to remove %1 and all of its components? You do not need to uninstall before an update.
de.ConfirmUninstall=Bist du sicher, dass du %1 und alle zugehörigen Komponenten entfernen möchtest? Vor einem Update ist keine Deinstallation erforderlich.
fr.ConfirmUninstall=Voulez-vous vraiment supprimer %1 et tous ses composants ? Une désinstallation n'est pas nécessaire avant une mise à jour.
es.ConfirmUninstall=¿Seguro que desea quitar %1 y todos sus componentes? No es necesario desinstalar antes de una actualización.

[CustomMessages]
en.Run=Launch {#appName}
en.DesktopIcon=Create a desktop shortcut
en.PdfDocument=PDF file
de.Run={#appName} starten
de.DesktopIcon=Verknüpfung auf dem Desktop anlegen
de.PdfDocument=PDF-Datei
fr.Run=Lancer {#appName}
fr.DesktopIcon=Créer un raccourci sur le Bureau
fr.PdfDocument=Fichier PDF
es.Run=Iniciar {#appName}
es.DesktopIcon=Crear un acceso directo en el escritorio
es.PdfDocument=Archivo PDF

[Tasks]
Name: desktopicon; Description: "{cm:DesktopIcon}"; Flags: unchecked

[Files]
; Excludes: Altlasten aus Builds bis 1.0.3, falls der Release-Ordner nicht geleert wurde (WebView2, Adobe-Ansicht)
Source: "{#releaseDir}\*"; Excludes: "*.pdb,Microsoft.Web.WebView2.*.dll,\runtimes\*,\adobe\*,adobe-clientid*.txt"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#appName}"; Filename: "{app}\{#appName}.exe"
Name: "{autodesktop}\{#appName}"; Filename: "{app}\{#appName}.exe"; Tasks: desktopicon

[Registry]
; ProgID, damit PDFlight im "Öffnen mit"-Dialog erscheint (Standard-App bleibt Sache des Benutzers)
Root: HKLM; Subkey: "Software\Classes\{#appName}.Document"; ValueType: string; ValueData: "{cm:PdfDocument}"; Flags: uninsdeletekey
; Die HKCU-Registrierung, die PDFlight bei jedem Start selbst schreibt (ShellUtil.RegisterFileType),
; wird hier nur zum Deinstallieren vorgemerkt
Root: HKCU; Subkey: "Software\Classes\{#appName}.Document"; ValueType: none; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\.pdf\OpenWithProgids"; ValueType: none; ValueName: "{#appName}.Document"; Flags: uninsdeletevalue
; neutrales Dokument-Icon für PDF-Dateien im Explorer (statt des Programm-Icons): pdffile.ico steckt als zweites Icon
; in der EXE (Post-Build-Schritt InsertIcons im Release-Build), deshalb Index 1
Root: HKLM; Subkey: "Software\Classes\{#appName}.Document\DefaultIcon"; ValueType: string; ValueData: "{app}\{#appName}.exe,1"
Root: HKLM; Subkey: "Software\Classes\{#appName}.Document\shell\open\command"; ValueType: string; ValueData: """{app}\{#appName}.exe"" ""%1"""
Root: HKLM; Subkey: "Software\Classes\.pdf\OpenWithProgids"; ValueType: string; ValueName: "{#appName}.Document"; ValueData: ""; Flags: uninsdeletevalue
Root: HKLM; Subkey: "Software\Classes\Applications\{#appName}.exe\shell\open\command"; ValueType: string; ValueData: """{app}\{#appName}.exe"" ""%1"""; Flags: uninsdeletekey

[Run]
; --help: PDFlight erzeugt die Hilfedatei (Downloads-Ordner, Programmsprache) und zeigt sie als erstes Dokument an
Filename: "{app}\{#appName}.exe"; Parameters: "--help"; Description: "{cm:Run}"; Flags: nowait postinstall skipifsilent

; Hinweis: Die Benutzereinstellungen (%APPDATA%\PDFlight\settings.json) und der Ordner
; %LOCALAPPDATA%\PDFlight (Rückgängig-Sicherungen) bleiben bei der Deinstallation erhalten.

[InstallDelete]
; Vorgänger von setup.default aus früheren Versionen
Type: files; Name: "{app}\language.default"
; früher separat ausgeliefertes Dokumentsymbol – steckt jetzt in der EXE
Type: files; Name: "{app}\pdffile.ico"
; bis 1.0.3: Anzeige über WebView2 und die optionale Adobe-Ansicht (seit 1.1.0 PDFium)
Type: files; Name: "{app}\Microsoft.Web.WebView2.*.dll"
Type: filesandordirs; Name: "{app}\runtimes"
Type: filesandordirs; Name: "{app}\adobe"
Type: files; Name: "{app}\adobe-clientid.txt"
Type: filesandordirs; Name: "{localappdata}\PDFlight\WebView2.*"

[UninstallDelete]
Type: files; Name: "{app}\setup.default"

[Code]
function GetSystemMetrics(nIndex: Integer): Integer; external 'GetSystemMetrics@user32.dll stdcall';
function GetDpiForSystem(): Cardinal; external 'GetDpiForSystem@user32.dll stdcall delayload';

{ Abstufung der Symbolleiste nach der logischen Breite des Hauptbildschirms (physische Pixel durch die DPI-Skalierung):
  gemessener Platzbedarf 1390 px mit großen Symbolen und Programm-Icons, 1245 px mit kleinen Symbolen, 1153 px ohne
  Programm-Icons, 977 px nur Text. 0 = alles an, 1 = kleine Symbole, 2 = zusätzlich ohne Programm-Icons, 3 = nur Text. }
function ToolbarLevel(): Integer;
var
  Width, Dpi: Integer;
begin
  Dpi := 96;
  try
    Dpi := GetDpiForSystem();
  except
  end;
  if Dpi <= 0 then Dpi := 96;
  Width := GetSystemMetrics(0) * 96 div Dpi;
  if Width >= 1440 then Result := 0
  else if Width >= 1280 then Result := 1
  else if Width >= 1100 then Result := 2
  else Result := 3;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  { Sprachwahl des Setups und Symbolleisten-Abstufung für PDFlight hinterlegen; das Programm übernimmt beides
    einmalig beim ersten Start nach der Installation (AppSettings.ApplyInstallerDefaults) }
  if CurStep = ssPostInstall then
    SaveStringToFile(ExpandConstant('{app}\setup.default'),
      'language=' + ActiveLanguage + #13#10 + 'toolbar=' + IntToStr(ToolbarLevel()) + #13#10, False);
end;
