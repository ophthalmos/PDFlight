using System.Drawing.Printing;
using PDFLight.Classes;
using PDFLight.Viewer;

namespace PDFLight.Forms;

/// <summary>Druckdialog nach dem Vorbild von Edge: links Drucker, Kopien, Layout, Seiten, beidseitiger Druck und unter „Weitere
/// Einstellungen“ Papierformat, Skalierung und Farbe; rechts die Vorschau der Druckbögen (<see cref="PrintPreviewView"/>, dieselbe
/// Seitengeometrie wie der Druck). Die Vorschau zeigt die Druckkopie des Dokuments (ohne Ansichtsdrehung, Formularwerte eingebrannt),
/// die der Aufrufer anlegt und danach auch druckt. „Drucken mithilfe des Systemdialogfelds“ schließt den Dialog und meldet
/// <see cref="SystemDialogRequested"/> – der Aufrufer zeigt dann den Druckdialog von Windows.</summary>
public partial class PrintForm : Form
{
    private readonly PdfiumDocument document;
    private readonly int currentPage;      // 1-basiert
    private readonly PrinterSettings printer = new();
    private readonly List<PaperSize> papers = [];
    private bool loading = true;           // während des Füllens keine Vorschau rechnen

    /// <summary>Drucker samt Kopien, Duplex, Papier und Farbe – gültig nach „Drucken“.</summary>
    internal PrinterSettings PrinterSettings => printer;

    /// <summary>Gewähltes Papier (null = Vorgabe des Druckers).</summary>
    internal PaperSize? Paper => comboPaper.SelectedIndex >= 0 && comboPaper.SelectedIndex < papers.Count ? papers[comboPaper.SelectedIndex] : null;

    /// <summary>In Farbe drucken (sonst Schwarzweiß).</summary>
    internal bool InColor => !comboColor.Enabled || comboColor.SelectedIndex != 1;

    /// <summary>Seiten, Ausrichtung und Skalierung – gesetzt bei „Drucken“.</summary>
    internal PrintJob? Job { get; private set; }

    /// <summary>Der Nutzer will lieber den Druckdialog von Windows (Link oder Strg+Umschalt+P).</summary>
    public bool SystemDialogRequested { get; private set; }

    internal PrintForm(PdfiumDocument document, int currentPage, string? lastPrinter)
    {
        InitializeComponent();
        Lng.Apply(this);
        this.document = document;
        this.currentPage = Math.Clamp(currentPage, 1, Math.Max(1, document.PageCount));
        rbCurrent.Text = string.Format(Lng.T("Aktuelle Seite ({0})"), this.currentPage);
        textRange.PlaceholderText = Lng.T("z. B. 1-5, 8, 11-13");
        comboDuplex.Items.AddRange([Lng.T("Einseitig"), Lng.T("Beidseitig, lange Kante"), Lng.T("Beidseitig, kurze Kante")]);
        comboScaling.Items.AddRange([Lng.T("An druckbaren Bereich anpassen"), Lng.T("Tatsächliche Größe"), Lng.T("Benutzerdefiniert (%)")]);
        comboColor.Items.AddRange([Lng.T("Farbe"), Lng.T("Schwarzweiß")]);
        comboScaling.SelectedIndex = 0;
        var defaultPrinter = printer.PrinterName; // ein neues PrinterSettings steht auf dem Standarddrucker
        foreach (string name in PrinterSettings.InstalledPrinters) { comboPrinter.Items.Add(name); }
        var initial = lastPrinter != null && comboPrinter.Items.Contains(lastPrinter) ? lastPrinter : defaultPrinter;
        comboPrinter.SelectedIndex = comboPrinter.Items.Contains(initial) ? comboPrinter.Items.IndexOf(initial) : Math.Min(0, comboPrinter.Items.Count - 1);
        loading = false;
        ApplyPrinter();
    }

    /// <summary>Groß wie in Edge: rund drei Viertel des Bildschirms (mindestens die Designer-Größe, höchstens 90 %), mittig über dem
    /// Hauptfenster.</summary>
    private void PrintForm_Load(object? sender, EventArgs e)
    {
        var area = Screen.FromControl(Owner ?? this).WorkingArea;
        var width = Math.Min(area.Width * 9 / 10, Math.Max(Width, area.Width * 3 / 4));
        var height = Math.Min(area.Height * 9 / 10, Math.Max(Height, area.Height * 85 / 100));
        Size = new Size(width, height);
        CenterToParent();
    }

    // ================================================================== Drucker

    private void ComboPrinter_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (!loading) { ApplyPrinter(); }
    }

    /// <summary>Neuer Drucker: seine Papierformate, Duplex- und Farbfähigkeit; Vorgaben aus seinen Einstellungen.</summary>
    private void ApplyPrinter()
    {
        if (comboPrinter.SelectedItem is not string name) { buttonPrint.Enabled = false; return; }
        loading = true;
        try
        {
            printer.PrinterName = name;
            papers.Clear();
            comboPaper.Items.Clear();
            if (printer.IsValid)
            {
                foreach (PaperSize paper in printer.PaperSizes) { papers.Add(paper); comboPaper.Items.Add(paper.PaperName); }
                var current = printer.DefaultPageSettings.PaperSize;
                var index = papers.FindIndex(p => p.Kind == current.Kind && p.PaperName == current.PaperName);
                if (index < 0) { index = papers.FindIndex(p => p.Width == current.Width && p.Height == current.Height); }
                comboPaper.SelectedIndex = index >= 0 ? index : Math.Min(0, papers.Count - 1);
                comboDuplex.Enabled = printer.CanDuplex;
                comboDuplex.SelectedIndex = printer.CanDuplex ? printer.Duplex switch { Duplex.Vertical => 1, Duplex.Horizontal => 2, _ => 0 } : 0;
                comboColor.Enabled = printer.SupportsColor;
                comboColor.SelectedIndex = printer.SupportsColor && printer.DefaultPageSettings.Color ? 0 : 1;
            }
        }
        catch (Exception ex) when (ex is InvalidPrinterException or System.ComponentModel.Win32Exception)
        {
            comboPaper.Items.Clear(); // Drucker nicht erreichbar: die Vorschau zeigt dann A4
        }
        finally { loading = false; }
        UpdatePreview();
    }

    // ================================================================== Einstellungen und Vorschau

    private void Settings_Changed(object? sender, EventArgs e)
    {
        if (sender is RadioButton { Checked: false }) { return; } // beim Wechsel melden sich beide – einmal rechnen genügt
        UpdatePreview();
    }

    private void TextRange_TextChanged(object? sender, EventArgs e)
    {
        if (!rbCustom.Checked) { rbCustom.Checked = true; return; } // löst seinerseits die Vorschau aus
        UpdatePreview();
    }

    private void LinkMore_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        panelMore.Visible = !panelMore.Visible;
    }

    private PrintOrientation Orientation => rbPortrait.Checked ? PrintOrientation.Portrait : rbLandscape.Checked ? PrintOrientation.Landscape : PrintOrientation.Auto;

    private PrintScaling Scaling => comboScaling.SelectedIndex switch { 1 => PrintScaling.ActualSize, 2 => PrintScaling.Custom, _ => PrintScaling.Fit };

    /// <summary>Die zu druckenden Seiten (0-basiert); null bei ungültiger Eingabe im Feld.</summary>
    private List<int>? SelectedPages()
    {
        var all = Enumerable.Range(0, document.PageCount);
        if (rbCurrent.Checked) { return [currentPage - 1]; }
        if (rbOdd.Checked) { return [.. all.Where(p => p % 2 == 0)]; } // Seite 1, 3, 5 …
        if (rbEven.Checked) { return [.. all.Where(p => p % 2 == 1)]; }
        if (rbCustom.Checked) { return PdfEditService.ParsePageRange(textRange.Text, document.PageCount)?.Select(p => p - 1).ToList(); }
        return [.. all];
    }

    private void UpdatePreview()
    {
        if (loading) { return; }
        var pages = SelectedPages();
        labelRangeError.Visible = rbCustom.Checked && textRange.Text.Trim().Length > 0 && pages == null;
        buttonPrint.Enabled = pages is { Count: > 0 } && comboPrinter.SelectedItem != null;
        numScale.Enabled = Scaling == PrintScaling.Custom;
        var paper = Paper;
        var paperSize = paper == null ? new SizeF(827, 1169) : new SizeF(paper.Width, paper.Height); // Hundertstel Zoll, Hochformat
        var printable = new RectangleF(PointF.Empty, paperSize);
        try
        {
            if (paper != null && printer.IsValid)
            {
                var pageSettings = (PageSettings)printer.DefaultPageSettings.Clone();
                pageSettings.PaperSize = paper;
                pageSettings.Landscape = false;
                printable = pageSettings.PrintableArea;
            }
        }
        catch (Exception ex) when (ex is InvalidPrinterException or System.ComponentModel.Win32Exception) { }
        previewView.Configure(document, pages ?? [], paperSize, printable, Orientation, Scaling, (int)numScale.Value, !InColor);
        var sheets = pages == null ? 0 : (comboDuplex.SelectedIndex > 0 ? (pages.Count + 1) / 2 : pages.Count) * (int)numCopies.Value;
        labelSheets.Text = sheets == 1 ? Lng.T("Insgesamt: 1 Papierbogen") : string.Format(Lng.T("Insgesamt: {0} Papierbögen"), sheets);
    }

    // ================================================================== Drucken

    private void ButtonPrint_Click(object? sender, EventArgs e)
    {
        if (SelectedPages() is not { Count: > 0 } pages) { return; }
        printer.Copies = (short)numCopies.Value;
        printer.Collate = true;
        if (printer.CanDuplex) { printer.Duplex = comboDuplex.SelectedIndex switch { 1 => Duplex.Vertical, 2 => Duplex.Horizontal, _ => Duplex.Simplex }; }
        if (Paper is { } paper) { printer.DefaultPageSettings.PaperSize = paper; }
        printer.DefaultPageSettings.Color = InColor;
        Job = new PrintJob(pages, Orientation, Scaling, (int)numScale.Value);
        DialogResult = DialogResult.OK;
    }

    private void LinkSystem_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        RequestSystemDialog();
    }

    private void RequestSystemDialog()
    {
        SystemDialogRequested = true;
        DialogResult = DialogResult.Cancel;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.Shift | Keys.P)) { RequestSystemDialog(); return true; } // wie in Edge
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
