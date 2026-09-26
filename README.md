# PDFlight

A lightweight PDF viewer with built-in file management for Windows.

![PDFlight](screenshot.png)

PDFlight is a lean PDF viewer for everyone who works through scans and incoming
documents every day. What other programs force you to do via a detour through Explorer,
PDFlight does right at the open document: move or copy it to another folder – with a
single click if you like. Frequently used locations are kept in a list.

Further actions are renaming, moving to the recycle bin, sending by e-mail or handing the
file over to another program. One keystroke flips to the next file in the folder.

The displayed file is never locked; other programs can change it at any time. The editing
tools – deleting or rotating pages, extracting pages into a new file, appending PDFs,
changing document properties or the password – save immediately. A slip can be undone
with Ctrl+Z. PDFlight recognises PDF/A files and protects them from accidental editing.
Form fields filled in the viewer are marked with an asterisk in the title bar; PDFlight asks
before closing and writes them into the file silently before any other action.

For highlighting and commenting, PDFlight offers an optional second view based on the
Adobe PDF Embed API (Settings → "Adobe PDF Embed API", button "Adobe" or F8). It is off by
default because it loads Adobe's viewer from the web; the PDF itself stays on your device.
Any other command switches back to the built-in viewer.

PDFlight is open source (MIT), runs on Windows 10 and 11 and speaks German, English,
French and Spanish.

## Requirements

Windows 10/11 (64-bit), [.NET Desktop Runtime 10](https://dotnet.microsoft.com/download/dotnet/10.0)
and the [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/).

## Building

`dotnet build PDFlight.csproj -c Release` with the .NET 10 SDK; the setup is produced with
[Inno Setup](https://jrsoftware.org/isinfo.php) from `Installer.iss`.

## License

[MIT](LICENSE) – © 2026 Wilhelm Happe
