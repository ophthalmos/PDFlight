# PDFlight

Schlanker PDF-Betrachter mit Dateiverwaltung. .NET 10 WinForms; Anzeige über den nativen Chromium-PDF-Viewer von WebView2 (Kapselung in `Classes\PdfViewHost.cs`), Seitenoperationen über PDFsharp-GDI. Nachfolger von PDFMover, bewusst ohne Scan/OCR. MIT-Lizenz, © Wilhelm Happe („ophthalmos").

Der sichtbare Name ist **PDFlight** (kleines l — Doppeldeutigkeit light/Flight ist gewollt); die Namespaces bleiben `PDFLight` (RootNamespace explizit gesetzt).

## Bauen

- `dotnet build` auf `PDFlight.csproj` (Solution: `PDFlight.slnx`); Release-Ausgabe in `bin\Release\net10.0-windows`.
- Installer: `Installer.iss` mit **Inno Setup 7** kompilieren (`C:\Program Files\Inno Setup 7\ISCC.exe` — nicht die parallel installierte v6 unter Program Files (x86)).
- csproj-Besonderheiten nicht „aufräumen": SDK-Importe sind absichtlich explizit (`Sdk.props`/`Sdk.targets`), damit die `Reference Remove`-Gruppe (entfernt Microsoft.Web.WebView2.Wpf, verhindert MSB3277) nach den NuGet-Targets ausgewertet wird. `UseWPF=true` dient nur UIAutomationClient; die dadurch entfallenden impliziten Usings `System.IO`/`System.Net.Http` sind per `<Using>`-Items wiederhergestellt.

## Aufbau

- `Forms\MainForm` — Hauptfenster, Toolbar, Hotkeys; Dialoge: SettingsForm, RenameForm, PageRangeForm, PasswordForm, PropertiesForm, FolderSelectForm.
- `Classes\` — PdfViewHost (WebView2 + UI-Automation), PdfEditService (PDFsharp), AppSettings (JSON in `%APPDATA%\PDFlight`), Lng (Mehrsprachigkeit), TaskDlg (alle Dialoge), ShellUtil, MailSender/MapiMailer, ToolbarIcons (Segoe-MDL2-Glyphen), ShortcutsPdf, FileUtil, ProgramFinder.
- `Controls\` — FolderTreeView, PathEditBox, FolderHistoryToolBar (eigene Controls, Ersatz für ShellBrowser.NET).
- `Languages\` — `lng.resx` (Deutsch, einkompilierter Rückfall) und `lng.<kultur>.resx` für en/fr/es.

## Konventionen

- Deutsch ist die Quellsprache; Benutzer werden in allen Programmtexten **geduzt**.
- Mehrsprachigkeit: Der **deutsche Text ist der resx-Schlüssel**. Code-Strings über `Lng.T("…")`, Designer-Texte über `Lng.Apply(form)` nach `InitializeComponent`. resx-Fallen: keine Zeilenumbrüche und keine Randleerzeichen in Schlüsseln; Ressourcennamen sind case-insensitiv (Kollisionsgefahr). Wird ein deutscher Text geändert, müssen die Schlüssel in allen Sprachdateien mitgezogen werden.
- Keine `MessageBox` — immer `TaskDlg` (MsgTaskDlg/ErrTaskDlg/ConfirmTaskDlg).
- Neue Tastenkürzel an drei Stellen pflegen: `TaskDlg.ShortcutRows` (samt Übersetzungen), Kürzel-PDF-Inhalte (`ShortcutsPdf`), README-Tabelle.
- Codestil: `var` statt expliziter Typen; Quellgeneratoren bevorzugen (LibraryImport — EntryPoint „…W" explizit angeben —, GeneratedRegex, GeneratedComInterface).
- `PDFlight.ico` liegt im Projekt-Basisordner. Achtung: Die Fenster-Icons stecken zusätzlich Base64-kodiert in `MainForm.resx`, `FolderSelectForm.resx` und `SettingsForm.resx` — bei einem Icon-Wechsel mit regenerieren.
- UI-Automation am Viewer: nur am Chromium-Kindfenster (`Chrome_RenderWidgetHostHWND`) ansetzen, in `Task.Run` mit Zeitbudget, nie aus dem WebView-KeyDown-Handler (stattdessen `BeginInvoke`). Details und weitere Fallstricke stehen im Projektgedächtnis.
- Git: mehrzeilige Commit-Botschaften mit Umlauten scheitern in PowerShell an `git -m` — stattdessen `git commit -F <datei>` verwenden.
