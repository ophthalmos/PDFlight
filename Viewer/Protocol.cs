namespace PDFLight.Viewer;

/// <summary>Befehle zwischen Hauptprogramm und Hilfsprogramm. Übertragen wird binär über die Standard-Ein- und -Ausgabe des
/// Hilfsprogramms (anonyme Pipes): ein Befehlsbyte, danach die Argumente (BinaryWriter). Die Antwort beginnt mit
/// <see cref="Ok"/> oder <see cref="Error"/> (dann folgt die Fehlermeldung als Zeichenfolge); davor können Ereignisse stehen, die
/// PDFium während des Befehls ausgelöst hat (<see cref="EventInvalidate"/>, <see cref="EventChanged"/>).
/// „Dok“ = Dokumentnummer aus <see cref="Open"/>; „Seite“ ist 0-basiert; „Seitenpixel“ meint die Seite als Bild der Größe
/// Breite × Höhe, Ursprung links oben.</summary>
internal static class Protocol
{
    public const byte Ok = 0, Error = 1;

    /// <summary>Ereignis vor der Antwort: int Dok, int Seite, float links, oben, rechts, unten – neu zu zeichnender Bereich als Anteil
    /// an Breite und Höhe der Seite (0…1).</summary>
    public const byte EventInvalidate = 2;

    /// <summary>Ereignis vor der Antwort: int Dok – ein Formularwert hat sich geändert.</summary>
    public const byte EventChanged = 3;

    // ================================================================== Allgemein

    /// <summary>→ (keine Argumente) ← Zeichenfolge mit Prozess- und Token-Angaben.</summary>
    public const byte Hello = 1;

    /// <summary>Nur für den Test: → Zeichenfolge Art, Zeichenfolge Ziel ← Zeichenfolge Ergebnis (beginnt mit
    /// <see cref="ProbeDenied"/> oder <see cref="ProbeAllowed"/>).</summary>
    public const byte Probe = 5;

    /// <summary>Nur für den Test: → byte Ort (<see cref="CrashInPdfium"/> oder <see cref="CrashOutsidePdfium"/>). Das Hilfsprogramm stürzt
    /// absichtlich mit einer Zugriffsverletzung ab.</summary>
    public const byte Crash = 6;

    /// <summary>Beenden.</summary>
    public const byte Quit = 7;

    // ================================================================== Dokument

    /// <summary>→ int Länge, Bytes der PDF-Datei, Zeichenfolge Kennwort (leer = keins) ← int Dok, bool Formular, int Seitenzahl, je Seite
    /// float Breite und Höhe in Punkt. Braucht die Datei ein Kennwort (oder war es falsch), kommt nur int <see cref="PasswordNeeded"/>.</summary>
    public const byte Open = 2;

    /// <summary>Statt einer Dokumentnummer: Die Datei ist kennwortgeschützt, und es fehlte ein Kennwort oder es war falsch.</summary>
    public const int PasswordNeeded = 0;

    /// <summary>→ int Dok ← (nichts). Schließt das Dokument.</summary>
    public const byte Close = 8;

    /// <summary>→ int Dok, bool ohne Sicherheit ← int Länge, Bytes: das Dokument samt Formularwerten als vollständige PDF-Datei; „ohne
    /// Sicherheit“ schreibt es unverschlüsselt (FPDF_REMOVE_SECURITY – entfernt Einschränkungen eines Besitzerkennworts verlustfrei).</summary>
    public const byte Save = 23;

    /// <summary>→ int Dok, int erste, int letzte Seite ← wie <see cref="Open"/>: eine Kopie (ohne Ansichtsdrehung), in der die Formularfelder dieser Seiten mit
    /// ihren Werten fest auf der Seite stehen (für den Druck – Felder zeichnet PDFium sonst nur in Bitmaps).</summary>
    public const byte PrintCopy = 24;

    // ================================================================== Darstellung

    /// <summary>→ int Dok, int Seite, int Seitenbreite, int Seitenhöhe, int x, int y, int Breite, int Höhe (Ausschnitt in Seitenpixeln)
    /// ← Bytes BGRA des Ausschnitts (Höhe × Breite × 4), samt Formularfeldern.</summary>
    public const byte Render = 3;

    /// <summary>→ int Dok, int Seite, int Breite, int Höhe (Druckerpixel) ← int Länge, Bytes einer EMF-Datei (Vektor, für den Druck).</summary>
    public const byte RenderEmf = 4;

    /// <summary>→ int Dok, int Drehung (Viertel im Uhrzeigersinn, 0–3) ← (nichts). Dreht die Ansicht aller Seiten: Darstellung und alle
    /// Seitenpixel-Angaben beziehen sich danach auf die gedrehte Seite (Breite und Höhe bei ungerader Drehung vertauscht). Nicht für EMF.</summary>
    public const byte SetRotation = 28; // seit 03.10.2026 vom Client nicht mehr genutzt (die Viewer-Leiste dreht die Seiten selbst)

    // ================================================================== Text

    /// <summary>→ int Dok, int Seite ← int Zeichenzahl.</summary>
    public const byte CharCount = 9;

    /// <summary>→ int Dok, int Seite, int Seitenbreite, int Seitenhöhe, int x, int y, double Toleranz (Punkt) ← int Zeichen (-1 = keins).</summary>
    public const byte CharAt = 10;

    /// <summary>→ int Dok, int Seite, int Seitenbreite, int Seitenhöhe, int x, int y ← int nächstgelegenes Zeichen (-1 ohne Text).</summary>
    public const byte NearestChar = 11;

    /// <summary>→ int Dok, int Seite, int Zeichen ← int erstes Zeichen, int Anzahl des Worts.</summary>
    public const byte WordAt = 12;

    /// <summary>→ int Dok, int Seite, int Seitenbreite, int Seitenhöhe, int erstes Zeichen, int Anzahl ← int n, n × (x, y, Breite, Höhe).</summary>
    public const byte SelectionRects = 13;

    /// <summary>→ int Dok, int Seite, int erstes Zeichen, int Anzahl ← Zeichenfolge.</summary>
    public const byte Text = 14;

    /// <summary>→ int Dok, int Seite, Zeichenfolge Suchbegriff, bool Groß/klein, bool ganzes Wort ← int n, n × (erstes Zeichen, Anzahl).</summary>
    public const byte SearchPage = 15;

    // ================================================================== Links und Lesezeichen

    /// <summary>→ int Dok, int Seite, int Seitenbreite, int Seitenhöhe, int x, int y ← bool vorhanden; dann Ziel (s. WriteTarget).</summary>
    public const byte LinkAt = 16;

    /// <summary>→ int Dok ← Baum: int n, je Eintrag Zeichenfolge Titel, bool Ziel vorhanden, Ziel, Unterbaum.</summary>
    public const byte Outline = 17;

    // ================================================================== Formulare (jeweils mit int angezeigte Seite – PDFium fragt danach)

    /// <summary>→ int Dok, int angezeigte Seite, byte Art (<see cref="MouseMove"/> …), int Seite, int Seitenbreite, int Seitenhöhe,
    /// int x, int y, int Modifizierer ← int: Feldart an der Stelle (-1 = keins) bzw. 1/0 verarbeitet.</summary>
    public const byte FormMouse = 18;

    public const byte MouseMove = 0, MouseDown = 1, MouseUp = 2;

    /// <summary>→ int Dok, int angezeigte Seite, byte Art (<see cref="KeyDown"/> …), int Taste bzw. Zeichen, int Modifizierer ← bool verarbeitet.</summary>
    public const byte FormKey = 19;

    public const byte KeyDown = 0, KeyUp = 1, KeyChar = 2;

    /// <summary>→ int Dok, int angezeigte Seite, byte Art (<see cref="EditSelectAll"/> …) ← bool verarbeitet.</summary>
    public const byte FormEdit = 20;

    public const byte EditSelectAll = 0, EditUndo = 1, EditRedo = 2, EditKillFocus = 3;

    /// <summary>→ int Dok, int angezeigte Seite ← Zeichenfolge: markierter Text im Feld mit dem Fokus.</summary>
    public const byte FormSelectedText = 21;

    /// <summary>→ int Dok, int angezeigte Seite, Zeichenfolge ← bool: ersetzt die Markierung im Feld mit dem Fokus.</summary>
    public const byte FormReplaceSelection = 22;

    // ================================================================== Hervorhebungen (Markup-Anmerkungen)

    /// <summary>→ int Dok, int Seite, int erstes Zeichen, int Anzahl, int Farbe (0xRRGGBB) ← bool angelegt. Legt eine
    /// Hervorhebung über den Zeichenbereich (eine Anmerkung je Seite, ein Viereck je Textzeile).</summary>
    public const byte AddHighlight = 25;

    /// <summary>→ int Dok, int Seite, int Seitenbreite, int Seitenhöhe, int x, int y ← int Nummer der Hervorhebung (Unterstreichung,
    /// Durchstreichung …) an der Stelle, -1 = keine.</summary>
    public const byte MarkupAt = 26;

    /// <summary>→ int Dok, int Seite, int Nummer (aus <see cref="MarkupAt"/> oder <see cref="InkAt"/>) ← bool entfernt. Entfernt nur
    /// Textmarkierungen und Zeichnungen (Ink).</summary>
    public const byte RemoveMarkup = 27;

    /// <summary>→ int Dok, int Seite, int Seitenbreite, int Seitenhöhe, int x, int y ← int Nummer der obersten Zeichnung (Ink), deren
    /// Linie die Stelle trifft (halbe Strichstärke plus ein paar Pixel Spielraum), -1 = keine.</summary>
    public const byte InkAt = 33;

    /// <summary>→ int Dok ← int PDF-Version (z. B. 17, 0 = unbekannt), int Revision des Sicherheits-Handlers (-1 = nicht verschlüsselt).
    /// Ersetzt seit 04.10.2026 das Lesen der Datei mit PDFsharp beim Öffnen – PDFsharp lief ungeschützt im Hauptprozess.</summary>
    public const byte Info = 34;

    // ================================================================== Umrechnung

    /// <summary>→ int Dok, int Seite, int Seitenbreite, int Seitenhöhe, int x, int y ← double x, double y: die Stelle in PDF-Koordinaten
    /// der Seite (Punkt, Ursprung unten links; /Rotate der Seite und die Drehung der Ansicht sind berücksichtigt).</summary>
    public const byte PagePoint = 29;

    /// <summary>→ int Dok, int Seite, int Seitenbreite, int Seitenhöhe, int x, int y, int Maske der Anmerkungsarten (Bit 1 &lt;&lt;
    /// FPDF_ANNOT_*, z. B. <see cref="AnnotFreeText"/>) ← int Nummer der obersten passenden Anmerkung an der Stelle (-1 = keine), int
    /// ihre Art, dann int x, y, Breite, Höhe ihres Rechtecks in Seitenpixeln.</summary>
    public const byte AnnotAt = 30;

    /// <summary>→ int Dok, int Seite, int Nummer, bool ausblenden ← bool erledigt. Blendet eine Anmerkung nur in der Anzeige aus
    /// (Hidden-Flag, beim Einblenden die alten Flags zurück) – beim Verschieben steht sie sonst bis zum Loslassen doppelt da.</summary>
    public const byte SetAnnotHidden = 31;

    /// <summary>→ int Dok, int Seite, int Seitenbreite, int Seitenhöhe, int Farbe (0xRRGGBB), float Strichstärke in Punkt, int Anzahl,
    /// dann je Punkt int x, y in Seitenpixeln ← bool angelegt. Legt einen Freihand-Strich als Ink-Anmerkung an (Zeichnen).</summary>
    public const byte AddInk = 32;

    public const int AnnotFreeText = 3, AnnotStamp = 13; // FPDF_ANNOT_FREETEXT, FPDF_ANNOT_STAMP

    // ================================================================== Testwerte

    public const string ProbeDenied = "verweigert", ProbeAllowed = "MÖGLICH";
    public const byte CrashInPdfium = 0, CrashOutsidePdfium = 1;

    // Ein Link- oder Lesezeichenziel: int Seite (-1 = keine), bool Höhe vorhanden, float Höhe (Punkt von unten), bool Adresse vorhanden,
    // Zeichenfolge Adresse. Lesen und Schreiben stehen hier, damit beide Seiten dasselbe Format benutzen.

    public static void WriteTarget(BinaryWriter writer, int page, float? top, string? uri)
    {
        writer.Write(page);
        writer.Write(top.HasValue);
        writer.Write(top ?? 0);
        writer.Write(uri != null);
        writer.Write(uri ?? string.Empty);
    }

    public static (int Page, float? Top, string? Uri) ReadTarget(BinaryReader reader)
    {
        var page = reader.ReadInt32();
        var hasTop = reader.ReadBoolean();
        var top = reader.ReadSingle();
        var hasUri = reader.ReadBoolean();
        var uri = reader.ReadString();
        return (page, hasTop ? top : null, hasUri ? uri : null);
    }
}
