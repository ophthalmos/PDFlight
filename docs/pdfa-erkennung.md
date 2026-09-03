# PDF/A- und Versionserkennung

Beim Laden einer Datei ruft `MainForm.LoadPdf` einmal `PdfEditService.TryReadStatus` auf.
Die Methode öffnet die Datei mit PDFsharp (`PdfReader.Open`, Import-Modus) — derselbe Öffnungsvorgang,
der vorher schon für die Seitenzahl nötig war — und liest aus dem geparsten Dokument drei Dinge:
Seitenzahl, PDF-Version und PDF/A-Stufe. Die Ergebnisse landen in der Statusleiste (`statusFormat`)
und steuern den PDF/A-Schreibschutz (Banner, gesperrte Menüpunkte).

## PDF-Version

Jede PDF-Datei beginnt mit einer Kopfzeile wie `%PDF-1.7`. PDFsharp liest sie beim Öffnen und
stellt sie als Zahl bereit (z.B. 17); `TryReadStatus` formatiert sie nur noch als „1.7".

## PDF/A-Stufe

Der PDF/A-Standard (ISO 19005) verlangt, dass sich eine konforme Datei selbst ausweist:
Im Dokumentkatalog hängt unter `/Metadata` ein XMP-Block (XML), der im `pdfaid`-Namensraum
zwei Angaben enthält — `pdfaid:part` (Norm-Teil: 1, 2, 3 …) und `pdfaid:conformance`
(Stufe: A, B oder U). `GetPdfALevel` entpackt diesen Stream, dekodiert ihn als UTF-8 und
sucht per Regex nach beiden Angaben, in Attribut- (`pdfaid:part="2"`) wie in
Element-Schreibweise (`<pdfaid:part>2</pdfaid:part>`) — beide kommen in freier Wildbahn vor.
Aus „2" und „B" wird die Anzeige „PDF/A-2b".

**Einordnung:** Das ist eine Deklarationsprüfung, keine Validierung. PDFlight glaubt der Datei,
dass sie ist, was sie behauptet; ob sie die PDF/A-Regeln (eingebettete Schriften, Farbprofile …)
wirklich einhält, prüft nur ein Validator wie veraPDF. Acrobat macht es beim Öffnen genauso —
auch dessen Banner beruht auf der Selbstauskunft.

**Verlust der Kennzeichnung:** PDFsharp 6.2 schreibt beim Speichern eigene XMP-Metadaten und
ersetzt die vorhandenen. Jede Bearbeitung in PDFlight (Seiten löschen/drehen, Metadaten ändern …)
entfernt daher die PDF/A-Deklaration — darauf weist der Warndialog vor dem Aktivieren der
Bearbeitung hin. Das bloße Aktivieren ändert die Datei nicht.

## Geschwindigkeit

Kein messbarer Unterschied: Datei öffnen und Struktur parsen fiel vorher schon für die Seitenzahl
an; Version und PDF/A-Stufe kommen aus demselben geparsten Dokument ohne zweiten Dateizugriff.
Messung (100 Aufrufe, warmgelaufen, 2-seitige PDF/A-Datei, 03.09.2026): alt wie neu je 0,59 ms
pro Aufruf — die Zusatzkosten (ein Wörterbuch-Zugriff, ein kleiner Stream, zwei Regex-Suchen)
verschwinden im Messrauschen. Die Anzeige selbst rendert der WebView2-Viewer ohnehin unabhängig
davon in seinem eigenen Prozess.
