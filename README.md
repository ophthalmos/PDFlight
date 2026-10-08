# PDFlight

A lightweight PDF viewer with built-in file management for Windows.

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

Selected text can be highlighted (right-click → Highlight) and the highlight removed again;
both are saved immediately. A sidebar shows page thumbnails or the document's bookmarks.

PDFs are rendered with [PDFium](https://pdfium.googlesource.com/pdfium/) – not inside PDFlight
itself, but in a separate helper process (`pdfhost.exe`) that runs in a Windows AppContainer
sandbox: it can't read your files, reach the network or start programs, and it is compiled
ahead of time so it may not generate executable code at run time. PDFlight hands it the
document as bytes; if a malformed PDF crashes it, PDFlight simply offers to reload. PDF
JavaScript is never executed.

PDFlight is open source (MIT), runs on Windows 10 and 11 and speaks German, English,
French and Spanish.

## Requirements

Windows 10/11 (64-bit) and the [.NET Desktop Runtime 10](https://dotnet.microsoft.com/download/dotnet/10.0).

## Building

`dotnet build PDFlight.csproj -c Release` with the .NET 10 SDK; the Release build publishes
the helper `pdfhost.exe` with Native AOT, which needs the C++ build tools of Visual Studio
("Desktop development with C++"). The setup is produced with
[Inno Setup](https://jrsoftware.org/isinfo.php) from `Installer.iss`.

## License

[MIT](LICENSE) – © 2026 Wilhelm Happe
