namespace PDFLight.Viewer;

/// <summary>Ausrichtung der Druckbögen: je Seite nach ihrem Format (Vorgabe) oder für alle Bögen gleich.</summary>
internal enum PrintOrientation { Auto, Portrait, Landscape }

/// <summary>Größe der Seite auf dem Bogen: in den bedruckbaren Bereich eingepasst (Vorgabe), in Originalgröße oder in Prozent davon.</summary>
internal enum PrintScaling { Fit, ActualSize, Custom }

/// <summary>Was gedruckt wird: die Seiten (0-basiert, in dieser Reihenfolge je ein Bogen), Ausrichtung und Größe. Drucker, Papier,
/// Exemplare, Farbe und Duplex stehen in den <see cref="System.Drawing.Printing.PrinterSettings"/>.</summary>
internal sealed record PrintJob(IReadOnlyList<int> Pages, PrintOrientation Orientation, PrintScaling Scaling, int ScalePercent);

/// <summary>Lage einer Seite auf dem Druckbogen – dieselbe Rechnung für die Vorschau im Druckdialog (Hundertstel Zoll) und den Druck
/// (Druckerpixel), damit beide übereinstimmen.</summary>
internal static class PrintLayout
{
    /// <summary>Kommt die Seite auf einen Bogen im Querformat?</summary>
    public static bool IsLandscape(PrintOrientation orientation, SizeF pagePt) =>
        orientation == PrintOrientation.Landscape || (orientation == PrintOrientation.Auto && pagePt.Width > pagePt.Height);

    /// <summary>Rechteck der Seite im bedruckbaren Bereich <paramref name="printable"/>: mittig, eingepasst oder in Originalgröße (bzw.
    /// Prozent davon; was über den Bereich hinausragt, fällt weg). <paramref name="unitsPerPoint"/> rechnet Punkt in die Einheit des
    /// Bereichs um.</summary>
    public static RectangleF Place(SizeF pagePt, RectangleF printable, float unitsPerPoint, PrintScaling scaling, int percent)
    {
        var width = pagePt.Width * unitsPerPoint;
        var height = pagePt.Height * unitsPerPoint;
        var scale = scaling switch
        {
            PrintScaling.Fit => Math.Min(printable.Width / width, printable.Height / height),
            PrintScaling.ActualSize => 1f,
            _ => Math.Clamp(percent, 10, 400) / 100f,
        };
        width *= scale;
        height *= scale;
        return new RectangleF(printable.X + (printable.Width - width) / 2, printable.Y + (printable.Height - height) / 2, width, height);
    }

    /// <summary>Bedruckbarer Bereich auf einem Bogen im Querformat aus dem des Hochformats (beide relativ zur linken oberen Papierecke):
    /// die Ränder wandern mit der Drehung um 90°.</summary>
    public static RectangleF Rotate(RectangleF portraitPrintable, SizeF portraitPaper) =>
        new(portraitPrintable.Y, portraitPaper.Width - portraitPrintable.Right, portraitPrintable.Height, portraitPrintable.Width);
}
