# PDFlight

Schlanker PDF-Betrachter mit Dateiverwaltung für Windows — ansehen, einsortieren, fertig.

![PDFlight](docs/screenshot.png)

PDFlight zeigt PDF-Dateien im nativen Chromium-Viewer (WebView2) an und verbindet die
Anzeige mit den Dateioperationen, die beim Abarbeiten voller Scan- und Eingangsordner
anfallen: Verschieben und Kopieren über eine Zielordnerliste, Umbenennen mit
Namensvorlagen, Löschen in den Papierkorb, Blättern durch den Ordner sowie Weitergabe
per E-Mail oder an andere PDF-Programme. Die angezeigte Datei wird aus dem Speicher
geladen und ist dadurch nie gesperrt. Mit PDFsharp lassen sich außerdem Seiten löschen,
drehen und extrahieren, PDF-Dateien anhängen und Dokumenteigenschaften bearbeiten —
mit einstufigem Rückgängig.

Oberfläche in Deutsch, Englisch, Französisch und Spanisch. Eine Übersicht aller
Tastenkürzel zeigt der Über-Dialog (F1).

## Voraussetzungen

- Windows 10/11 (64-Bit)
- [.NET Desktop Runtime 10](https://dotnet.microsoft.com/download/dotnet/10.0) und
  [WebView2-Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) —
  beides ist auf aktuellen Systemen meist vorhanden; fehlt etwas, weisen Setup
  bzw. Programmstart darauf hin

## Bauen

Mit dem .NET-10-SDK:

```
dotnet build PDFlight.csproj -c Release
```

Das Setup entsteht mit [Inno Setup](https://jrsoftware.org/isinfo.php) aus `Installer.iss`
direkt aus dem Release-Ordner.

## Technik

WebView2/Chromium rendert die Anzeige (das Dokument wird über ein virtuelles Host-Schema
aus dem Speicher serviert), [PDFsharp](https://www.pdfsharp.net/) (MIT) übernimmt die
Seitenoperationen, sämtlicher Windows-Interop ist quellgeneriert — keine kommerziellen
Abhängigkeiten.

## Lizenz und Unterstützung

[MIT](LICENSE) — © 2026 Wilhelm Happe. Wer das Programm nützlich findet, kann über den
Über-Dialog (F1) oder [direkt per PayPal](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=S8DVXHKFC2CVS&source=url)
spenden.
