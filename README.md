# PDFlight

A lightweight PDF viewer with built-in file management for Windows – view, sort, done.

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

PDFlight is open source (MIT), runs on Windows 10 and 11 and speaks German, English,
French and Spanish. A printable help sheet with all keyboard shortcuts is one keystroke
away (F1).

## Requirements

- Windows 10/11 (64-bit)
- [.NET Desktop Runtime 10](https://dotnet.microsoft.com/download/dotnet/10.0) and the
  [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) – both are
  usually present on current systems; if something is missing, the setup or the program
  start will tell you.

## Building

With the .NET 10 SDK:

```
dotnet build PDFlight.csproj -c Release
```

The setup is produced with [Inno Setup](https://jrsoftware.org/isinfo.php) from
`Installer.iss` directly out of the release folder.

## Under the hood

WebView2/Chromium renders the display (the document is served from memory through a
virtual host scheme), [PDFsharp](https://www.pdfsharp.net/) (MIT) handles the page
operations, and all Windows interop is source-generated – no commercial dependencies.

## License and support

[MIT](LICENSE) – © 2026 Wilhelm Happe. If you find the program useful, you can donate via
the About dialog or [directly through PayPal](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=S8DVXHKFC2CVS&source=url).
