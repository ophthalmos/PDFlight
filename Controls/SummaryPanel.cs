using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using PDFLight.Classes;
using PDFLight.Viewer;

namespace PDFLight.Controls;

/// <summary>Ein Abschnitt der KI-Unterhaltung: Überschrift (Art der Zusammenfassung oder die gestellte Frage) und Antwort als Markdown.</summary>
public sealed record SummaryTurnView(string Heading, string Markdown);

/// <summary>KI-Zusammenfassung in der Seitenleiste (seit 14.11.2026, Wunsch des Autors nach Acrobats „Generative Zusammenfassung“): oben die
/// Art der Zusammenfassung und „Erstellen“ (während einer Anfrage „Abbrechen“), darunter die Antworten untereinander – jede Zusammenfassung
/// und jede Frage ist ein Abschnitt, unter dem letzten stehen die von Gemini vorgeschlagenen Folgefragen zum Anklicken. Seitenangaben der
/// Antworten sind Links. Unten Kopieren, Speichern als Word-Datei (RTF), „Neu beginnen“, das Fragefeld und der Hinweis auf mögliche Fehler
/// mit Link auf Googles Bedingungen. Was die Befehle tun, entscheidet das Hauptfenster – das Bedienfeld meldet sie nur und zeigt an, was
/// es bekommt (<see cref="ShowThread"/>, <see cref="ShowStatus"/>). Die Antworten kommen als Markdown und werden hier in einfache
/// Formatierung übersetzt (Überschriften, Aufzählungen, fett, kursiv).</summary>
public partial class SummaryPanel : UserControl
{
    private enum Kind { Idle, Busy, Error }

    /// <summary>Farben einer Darstellung – am Bildschirm die der Seitenleiste, beim Kopieren und Speichern immer die hellen.</summary>
    private sealed record Palette(Color Fore, Color Secondary, Color Link, Color Error);

    private static readonly Palette ExportPalette = new(Color.Black, Color.FromArgb(96, 96, 96), Color.FromArgb(0, 102, 204), Color.Firebrick);

    private IReadOnlyList<SummaryTurnView> turns = [];
    private IReadOnlyList<string> followUps = [];
    private string footer = string.Empty;
    private string status = string.Empty;
    private string? pendingHeading;
    private string pendingMarkdown = string.Empty; // die Antwort, während sie entsteht (gestreamt)
    private const int ScrollToEnd = int.MaxValue, ScrollToPending = int.MinValue;
    private int pendingStart; // wo die Überschrift der laufenden Anfrage im Text beginnt
    private int tailStart = -1; // wo der untere Teil (laufende Anfrage, Status) beginnt
    private bool renderedBusy;  // die Ansicht zeigt eine laufende Anfrage – dann genügt es, den unteren Teil zu ersetzen
    private Kind kind = Kind.Idle;
    private int scrollToTurn = -1;
    private Palette palette = new(SystemColors.WindowText, SystemColors.GrayText, Color.FromArgb(0, 102, 204), Color.Firebrick);
    private Color iconColor = Color.FromArgb(64, 64, 64);
    private readonly List<(int Start, int Length, int Page, string? Question)> links = []; // anklickbare Stellen im Text

    /// <summary>„Erstellen“: eine Zusammenfassung der gewählten Art.</summary>
    public event EventHandler? RunRequested;

    /// <summary>„Abbrechen“ während einer Anfrage.</summary>
    public event EventHandler? CancelRequested;

    /// <summary>Eine Frage aus dem Fragefeld oder eine angeklickte Folgefrage.</summary>
    public event EventHandler<string>? QuestionAsked;

    /// <summary>Eine angeklickte Seitenangabe (1-basiert).</summary>
    public event EventHandler<int>? PageRequested;

    /// <summary>„Speichern…“ – das Hauptfenster fragt nach dem Ziel und ruft <see cref="SaveRtf"/>.</summary>
    public event EventHandler? SaveRequested;

    /// <summary>„Neu beginnen“: die Unterhaltung zu dieser Datei vergessen.</summary>
    public event EventHandler? ClearRequested;

    public SummaryPanel()
    {
        InitializeComponent();
        Lng.Apply(this);
        for (var i = 0; i < SummaryPresets.All.Length; i++)
        {
            ToolStripMenuItem item = new(Lng.T(SummaryPresets.All[i].Name)) { Tag = i };
            item.Click += PresetItem_Click;
            ddPreset.DropDownItems.Add(item);
        }
        textQuestion.PlaceholderText = TextBoxMargins.Placeholder(Lng.T("Frage zum Dokument…")); // Lng.Apply übersetzt nur Text-Eigenschaften
        toolTip.SetToolTip(btnAsk, Lng.T("Frage senden (Eingabe)"));
        var notice = Lng.T("KI-Antworten können unrichtig sein.");
        var link = Lng.T("Nutzungsbedingungen");
        linkDisclaimer.Text = notice + " " + link;
        linkDisclaimer.LinkArea = new LinkArea(notice.Length + 1, link.Length);
        SelectPreset(0);
        UpdateButtons();
    }

    /// <summary>Die gewählte Art der Zusammenfassung (Index in <see cref="SummaryPresets.All"/>).</summary>
    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int PresetIndex { get; private set; }

    /// <summary>Eine Anfrage läuft (der Knopf heißt dann „Abbrechen“).</summary>
    [System.ComponentModel.Browsable(false)]
    public bool Busy => kind == Kind.Busy;

    /// <summary>Im Fragefeld oder im Antworttext liegt der Fokus – die Tasten gehören dann ihnen, nicht den Kürzeln des Hauptfensters.</summary>
    [System.ComponentModel.Browsable(false)]
    public bool TextFocused => textQuestion.Focused || richText.Focused;

    public void SelectPreset(int index)
    {
        PresetIndex = Math.Clamp(index, 0, SummaryPresets.All.Length - 1);
        ddPreset.Text = Lng.T(SummaryPresets.All[PresetIndex].Name);
        foreach (ToolStripMenuItem item in ddPreset.DropDownItems) { item.Checked = (int)item.Tag! == PresetIndex; } // jeder Eintrag trägt seinen Index (Konstruktor)
    }

    /// <summary>Die Unterhaltung samt Folgefragen und grauem Nachsatz (Modell, Seiten). <paramref name="scrollTo"/>: zu diesem Abschnitt
    /// blättern (der neue), sonst an den Anfang.</summary>
    public void ShowThread(IReadOnlyList<SummaryTurnView> thread, IReadOnlyList<string> questions, string footerText, int scrollTo = -1)
    {
        (turns, followUps, footer, scrollToTurn) = (thread, questions, footerText, scrollTo);
        (status, pendingHeading, pendingMarkdown, kind) = (string.Empty, null, string.Empty, Kind.Idle);
        Render();
        UpdateButtons();
    }

    /// <summary>Ein Hinweis unter der Unterhaltung (bzw. allein): grau, bei <paramref name="error"/> rot; <paramref name="busy"/> = eine Anfrage
    /// läuft, <paramref name="heading"/> = ihre Überschrift (die gestellte Frage).</summary>
    public void ShowStatus(string text, bool busy = false, bool error = false, string? heading = null)
    {
        var newKind = busy ? Kind.Busy : error ? Kind.Error : Kind.Idle;
        if (newKind == kind && text == status && heading == pendingHeading && pendingMarkdown.Length == 0 && richText.TextLength > 0) { return; } // Fortschritt ohne Änderung: nicht flackern
        var tailOnly = renderedBusy && newKind == Kind.Busy; // Sekundenzähler: nur den unteren Teil ersetzen
        (status, kind, pendingHeading, pendingMarkdown, scrollToTurn) = (text, newKind, heading, string.Empty, ScrollToEnd);
        if (tailOnly) { UpdateTail(scrollToPendingStart: false); } else { Render(); }
        UpdateButtons();
    }

    /// <summary>Die Antwort, während sie entsteht, unter ihrer Überschrift – beim ersten Stück blättert die Ansicht an ihren Anfang, danach
    /// bleibt sie, wo der Benutzer sie hat.</summary>
    public void ShowPending(string heading, string partialMarkdown)
    {
        var first = pendingMarkdown.Length == 0;
        (status, kind, pendingHeading, pendingMarkdown, scrollToTurn) = (string.Empty, Kind.Busy, heading, partialMarkdown, ScrollToPending);
        if (renderedBusy) { UpdateTail(scrollToPendingStart: first); } else { Render(); }
        UpdateButtons();
    }

    /// <summary>Ohne Unterhaltung: nur ein grauer Hinweis.</summary>
    public void ShowMessage(string text) => ShowThreadless(text);

    private void ShowThreadless(string text)
    {
        (turns, followUps, footer) = ([], [], string.Empty);
        (status, pendingHeading, pendingMarkdown, kind, scrollToTurn) = (text, null, string.Empty, Kind.Idle, -1);
        Render();
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        var busy = kind == Kind.Busy;
        btnRun.Text = busy ? Lng.T("Abbrechen") : Lng.T("Erstellen");
        btnRun.ToolTipText = busy ? Lng.T("Anfrage abbrechen") : Lng.T("Zusammenfassung erstellen");
        ddPreset.Enabled = !busy;
        btnCopy.Enabled = btnSave.Enabled = turns.Count > 0;
        btnClear.Enabled = turns.Count > 0 && !busy;
        textQuestion.Enabled = !busy;
        btnAsk.Enabled = !busy && textQuestion.Text.Trim().Length > 0;
    }

    /// <summary>Farben wie die übrige Seitenleiste (hell oder dunkel).</summary>
    public void ApplyTheme(bool dark, Color back, Color fore, Color secondary, Color icon, Color link, Color error, ToolStripRenderer renderer, ToolStripRenderer dropDownRenderer)
    {
        BackColor = panelBody.BackColor = richText.BackColor = linkDisclaimer.BackColor = panelAsk.BackColor = back;
        ForeColor = richText.ForeColor = fore;
        palette = new Palette(fore, secondary, link, error);
        iconColor = icon;
        foreach (var strip in new[] { toolStripTop, toolStripActions })
        {
            strip.Renderer = renderer;
            strip.BackColor = back;
        }
        ddPreset.ForeColor = btnRun.ForeColor = btnCopy.ForeColor = btnSave.ForeColor = btnClear.ForeColor = fore;
        ddPreset.DropDown.Renderer = dropDownRenderer;
        textQuestion.BackColor = dark ? ViewerStripRenderer.FieldBackground : SystemColors.Window;
        textQuestion.ForeColor = dark ? ViewerStripRenderer.Foreground : SystemColors.WindowText;
        textQuestion.BorderStyle = dark ? BorderStyle.FixedSingle : BorderStyle.Fixed3D; // wie die Suche der Anmerkungsliste
        TextBoxMargins.Keep(textQuestion, 2);
        btnAsk.BackColor = back;
        btnAsk.ForeColor = fore;
        btnAsk.FlatAppearance.MouseOverBackColor = dark ? Color.FromArgb(64, 64, 64) : Color.FromArgb(229, 229, 229);
        linkDisclaimer.ForeColor = secondary;
        linkDisclaimer.LinkColor = linkDisclaimer.ActiveLinkColor = linkDisclaimer.VisitedLinkColor = link;
        UpdateIcons();
        Render(); // die Farben stecken im Text
    }

    /// <summary>Symbole in der Größe der Viewer-Leiste; „Kopieren“ behält seinen Text wie bei Acrobat.</summary>
    private void UpdateIcons()
    {
        if (!ToolbarIcons.FontAvailable) { return; }
        var size = new Size(LogicalToDeviceUnits(16), LogicalToDeviceUnits(16));
        toolStripActions.ImageScalingSize = size;
        foreach (var (item, glyph, style) in new (ToolStripItem, char, ToolStripItemDisplayStyle)[]
        {
            (btnCopy, ToolbarIcons.Copy, ToolStripItemDisplayStyle.ImageAndText),
            (btnSave, ToolbarIcons.SaveAs, ToolStripItemDisplayStyle.Image),
            (btnClear, ToolbarIcons.Refresh, ToolStripItemDisplayStyle.Image), // kein ✕ – das läse sich wie „Schließen“
        })
        {
            item.Image = ToolbarIcons.Get(glyph, size, iconColor);
            item.DisplayStyle = style;
        }
        btnAsk.Image = ToolbarIcons.Get(ToolbarIcons.Send, size, iconColor);
        btnAsk.Text = string.Empty;
    }

    /// <summary>Der Hinweis unten bricht in schmalen Seitenleisten um – seine Höhe folgt dem Text.</summary>
    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        var width = Math.Max(40, ClientSize.Width - linkDisclaimer.Padding.Horizontal);
        var text = TextRenderer.MeasureText(linkDisclaimer.Text, linkDisclaimer.Font, new Size(width, int.MaxValue), TextFormatFlags.WordBreak);
        var height = text.Height + linkDisclaimer.Padding.Vertical + LogicalToDeviceUnits(2);
        if (linkDisclaimer.Height != height) { linkDisclaimer.Height = height; }
    }

    // ================================================================== Befehle

    private void BtnRun_Click(object? sender, EventArgs e)
    {
        if (kind == Kind.Busy) { CancelRequested?.Invoke(this, EventArgs.Empty); }
        else { RunRequested?.Invoke(this, EventArgs.Empty); }
    }

    private void PresetItem_Click(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem { Tag: int index }) { SelectPreset(index); }
    }

    private void TextQuestion_TextChanged(object? sender, EventArgs e) => UpdateButtons();

    private void TextQuestion_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter || e.Modifiers != Keys.None) { return; }
        e.SuppressKeyPress = true; // kein Signalton
        Ask();
    }

    private void BtnAsk_Click(object? sender, EventArgs e) => Ask();

    private void Ask()
    {
        var question = textQuestion.Text.Trim();
        if (question.Length == 0 || kind == Kind.Busy) { return; }
        textQuestion.Clear();
        QuestionAsked?.Invoke(this, question);
    }

    private void BtnClear_Click(object? sender, EventArgs e) => ClearRequested?.Invoke(this, EventArgs.Empty);

    private void BtnSave_Click(object? sender, EventArgs e) => SaveRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>Als formatierter Text (Word, Mail) und als Markdown (Editoren, Chat) in die Zwischenablage – immer in den hellen Farben,
    /// auch im Dunkelmodus (sonst käme weiße Schrift in Word an).</summary>
    private void BtnCopy_Click(object? sender, EventArgs e)
    {
        if (turns.Count == 0) { return; }
        DataObject data = new();
        data.SetData(DataFormats.Rtf, ExportRtf());
        data.SetData(DataFormats.UnicodeText, ExportMarkdown());
        try { Clipboard.SetDataObject(data, copy: true, retryTimes: 5, retryDelay: 50); }
        catch (System.Runtime.InteropServices.ExternalException) { } // Zwischenablage gerade gesperrt
    }

    /// <summary>Schreibt die Unterhaltung als RTF-Datei (Word, WordPad, LibreOffice).</summary>
    public void SaveRtf(string path) => File.WriteAllText(path, ExportRtf(), Encoding.ASCII); // RTF kodiert Umlaute selbst als \u-Folgen

    private string ExportRtf()
    {
        using RichTextBox box = new() { Font = richText.Font };
        Render(box, ExportPalette, export: true);
        return box.Rtf ?? string.Empty;
    }

    private string ExportMarkdown()
    {
        StringBuilder text = new();
        foreach (var turn in turns)
        {
            if (text.Length > 0) { text.AppendLine().AppendLine(); }
            text.AppendLine("## " + turn.Heading).AppendLine();
            text.Append(CitationRegex().Replace(turn.Markdown.Trim(), m => "(" + PageLabel(m) + ")"));
        }
        if (footer.Length > 0) { text.AppendLine().AppendLine().Append(footer); }
        return text.ToString();
    }

    private void LinkDisclaimer_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo(GeminiClient.TermsUrl + "?hl=" + Lng.CultureCode) { UseShellExecute = true }); }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException) { Debug.WriteLine(ex); }
    }

    // ================================================================== Links im Text

    /// <summary>Die anklickbare Stelle unter dem Mauszeiger – nur, wenn er wirklich über ihren Zeichen steht (die nächstgelegene Stelle
    /// liefert RichTextBox auch weit rechts neben dem Zeilenende).</summary>
    private (int Page, string? Question)? LinkAt(Point point)
    {
        if (links.Count == 0) { return null; }
        var index = richText.GetCharIndexFromPosition(point);
        var start = richText.GetPositionFromCharIndex(index);
        var next = index + 1 < richText.TextLength ? richText.GetPositionFromCharIndex(index + 1) : start;
        var right = next.Y == start.Y && next.X > start.X ? next.X : start.X + richText.Font.Height;
        if (point.X < start.X - 1 || point.X > right + 1 || point.Y < start.Y || point.Y > start.Y + richText.Font.Height * 3 / 2) { return null; }
        foreach (var (s, length, page, question) in links)
        {
            if (index >= s && index < s + length) { return (page, question); }
        }
        return null;
    }

    private void RichText_MouseMove(object? sender, MouseEventArgs e)
    {
        var cursor = LinkAt(e.Location) != null ? Cursors.Hand : Cursors.IBeam;
        if (richText.Cursor != cursor) { richText.Cursor = cursor; }
    }

    private void RichText_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || richText.SelectionLength > 0 || LinkAt(e.Location) is not { } link) { return; }
        if (link.Question is { } question) { if (kind != Kind.Busy) { QuestionAsked?.Invoke(this, question); } }
        else { PageRequested?.Invoke(this, link.Page); }
    }

    // ================================================================== Darstellung

    private void Render()
    {
        SetRedraw(false); // ohne Zwischenbilder – sonst sah man den Text kurz verschwinden
        try
        {
            links.Clear();
            var starts = Render(richText, palette, export: false);
            renderedBusy = kind == Kind.Busy;
            if (scrollToTurn == ScrollToEnd) { richText.Select(richText.TextLength, 0); richText.ScrollToCaret(); } // eine Anfrage läuft: ans Ende
            else if (scrollToTurn == ScrollToPending) { richText.Select(pendingStart, 0); richText.ScrollToCaret(); } // die entstehende Antwort von oben lesen
            else if (scrollToTurn >= 0 && scrollToTurn < starts.Count) { richText.Select(starts[scrollToTurn], 0); richText.ScrollToCaret(); }
            else { richText.Select(0, 0); }
        }
        finally { SetRedraw(true); }
    }

    /// <summary>Während einer Anfrage ändert sich nur der untere Teil (Überschrift der Anfrage, entstehende Antwort, Status) – den ersetzt
    /// diese Methode, statt alles neu zu setzen. Fehlerbericht 14.11.2026: Bei jedem Sekundenschritt und jedem Stück der Antwort wurde die
    /// ganze Unterhaltung neu aufgebaut, die obere Hälfte blendete im Sekundentakt kurz aus. Die Bildlaufposition bleibt stehen.</summary>
    private void UpdateTail(bool scrollToPendingStart)
    {
        if (tailStart < 0 || tailStart > richText.TextLength) { Render(); return; }
        var scroll = GetScrollPosition();
        SetRedraw(false);
        try
        {
            richText.Select(tailStart, richText.TextLength - tailStart);
            richText.ReadOnly = false; // schreibgeschützt löscht SelectedText = "" nichts (Einfügen geht trotzdem) – im Test hing der neue Teil am alten
            richText.SelectedText = string.Empty;
            richText.ReadOnly = true;
            links.RemoveAll(l => l.Start >= tailStart);
            using Fonts fonts = new(richText.Font);
            MarkdownWriter writer = new(richText, fonts, palette, links, LogicalToDeviceUnits(14), LogicalToDeviceUnits(4), started: tailStart > 0);
            WriteTail(writer, fonts, palette, richText);
            if (scrollToPendingStart) { richText.Select(pendingStart, 0); richText.ScrollToCaret(); }
            else { richText.Select(tailStart, 0); SetScrollPosition(scroll); }
        }
        finally { SetRedraw(true); }
    }

    private void SetRedraw(bool on)
    {
        if (!richText.IsHandleCreated) { return; }
        NativeMethods.SendMessage(richText.Handle, WM_SETREDRAW, on ? 1 : 0, 0);
        if (on) { richText.Invalidate(); }
    }

    private ScrollPoint GetScrollPosition()
    {
        ScrollPoint point = default;
        if (richText.IsHandleCreated) { SendMessage(richText.Handle, EM_GETSCROLLPOS, 0, ref point); }
        return point;
    }

    private void SetScrollPosition(ScrollPoint point)
    {
        if (richText.IsHandleCreated) { SendMessage(richText.Handle, EM_SETSCROLLPOS, 0, ref point); }
    }

    private const uint WM_SETREDRAW = 0x000B, EM_GETSCROLLPOS = 0x04DD, EM_SETSCROLLPOS = 0x04DE;

    [System.Runtime.InteropServices.LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial nint SendMessage(nint hWnd, uint msg, nint wParam, ref ScrollPoint lParam);

    /// <summary>POINT für EM_GETSCROLLPOS/EM_SETSCROLLPOS (System.Drawing.Point kann der LibraryImport-Generator nicht übergeben).</summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct ScrollPoint
    {
        public int X;
        public int Y;
    }

    /// <summary>Schreibt die Unterhaltung in <paramref name="box"/>: je Abschnitt die Überschrift grau und fett, darunter die Antwort; danach
    /// (nicht beim Exportieren) die Folgefragen, der Nachsatz und ein Hinweis. Liefert, wo die Abschnitte beginnen.</summary>
    private List<int> Render(RichTextBox box, Palette colors, bool export)
    {
        box.Clear();
        List<int> starts = [];
        var baseFont = box.Font;
        using Fonts fonts = new(baseFont);
        MarkdownWriter writer = new(box, fonts, colors, export ? null : links, LogicalToDeviceUnits(14), LogicalToDeviceUnits(4));
        foreach (var turn in turns)
        {
            writer.StartParagraph(0, 0, gapBefore: true);
            starts.Add(box.TextLength);
            writer.Insert(turn.Heading, fonts.Heading3, colors.Secondary);
            writer.Write(turn.Markdown);
            writer.RequestGap();
        }
        if (!export && followUps.Count > 0 && kind == Kind.Idle)
        {
            foreach (var (question, i) in followUps.Select((q, i) => (q, i)))
            {
                writer.StartParagraph(LogicalToDeviceUnits(4), LogicalToDeviceUnits(16), gapBefore: i == 0);
                writer.Insert("↳ ", baseFont, colors.Secondary);
                writer.InsertLink(question, fonts.Link, colors.Link, 0, question);
            }
        }
        if (footer.Length > 0 && turns.Count > 0)
        {
            writer.StartParagraph(0, 0, gapBefore: true);
            writer.Insert(footer, fonts.Small, colors.Secondary);
        }
        if (!export)
        {
            tailStart = box.TextLength;
            WriteTail(writer, fonts, colors, box);
        }
        return starts;
    }

    /// <summary>Der untere Teil während einer Anfrage: ihre Überschrift, die entstehende Antwort und der Status (s. <see cref="UpdateTail"/>).</summary>
    private void WriteTail(MarkdownWriter writer, Fonts fonts, Palette colors, RichTextBox box)
    {
        if (pendingHeading != null)
        {
            writer.StartParagraph(0, 0, gapBefore: true);
            pendingStart = box.TextLength;
            writer.Insert(pendingHeading, fonts.Heading3, colors.Secondary);
            if (pendingMarkdown.Length > 0) { writer.Write(pendingMarkdown); }
        }
        if (status.Length > 0)
        {
            writer.StartParagraph(0, 0, gapBefore: turns.Count > 0 && pendingHeading == null);
            writer.Insert(status, fonts.Base, kind == Kind.Error ? colors.Error : colors.Secondary);
        }
    }

    /// <summary>„S. 3“ bzw. „S. 3–5“ für eine Seitenangabe der Antwort.</summary>
    private static string PageLabel(Match citation) => citation.Groups["to"].Success
        ? string.Format(Lng.T("S. {0}–{1}"), citation.Groups["from"].Value, citation.Groups["to"].Value)
        : string.Format(Lng.T("S. {0}"), citation.Groups["from"].Value);

    /// <summary>Die Schriften einer Darstellung, abgeleitet von der Grundschrift.</summary>
    private sealed class Fonts(Font baseFont) : IDisposable
    {
        public readonly Font Bold = new(baseFont, FontStyle.Bold), Italic = new(baseFont, FontStyle.Italic), BoldItalic = new(baseFont, FontStyle.Bold | FontStyle.Italic);
        public readonly Font Heading1 = new(baseFont.FontFamily, baseFont.Size * 1.25f, FontStyle.Bold), Heading2 = new(baseFont.FontFamily, baseFont.Size * 1.12f, FontStyle.Bold);
        public readonly Font Heading3 = new(baseFont, FontStyle.Bold);
        public readonly Font Gap = new(baseFont.FontFamily, baseFont.Size * 0.5f), Small = new(baseFont.FontFamily, baseFont.Size * 0.9f);
        public readonly Font Link = new(baseFont, FontStyle.Underline);
        public readonly Font Base = baseFont;

        public void Dispose()
        {
            foreach (var font in new[] { Bold, Italic, BoldItalic, Heading1, Heading2, Heading3, Gap, Small, Link }) { font.Dispose(); }
        }
    }

    /// <summary>Übersetzt Markdown in RichTextBox-Formatierung: Überschriften (#), Aufzählungen (-, *, +), nummerierte Listen, Leerzeilen als
    /// kleiner Abstand, fett und kursiv im Text, Seitenangaben [[p.N]] als Links. Tabellen und Code kommen als einfacher Text – die Anweisung
    /// an Gemini schließt sie aus. Punkte und Nummern stehen als Text mit hängendem Einzug: mit den Aufzählungen der RichTextBox begannen die
    /// Folgezeilen unter dem Punkt statt unter dem Text.</summary>
    private sealed class MarkdownWriter(RichTextBox box, Fonts fonts, Palette colors, List<(int, int, int, string?)>? links, int indentStep, int indentBase, bool started = false)
    {
        private bool first = !started; // started: es steht schon Text da (UpdateTail schreibt nur den unteren Teil neu)
        private bool pendingGap;

        public void RequestGap() { if (!first) { pendingGap = true; } }

        public void StartParagraph(int indent, int hanging, bool gapBefore)
        {
            if (!first)
            {
                Insert("\n", fonts.Base, colors.Fore);
                if (gapBefore || pendingGap)
                {
                    box.SelectionIndent = 0; // die leere Abstandszeile ohne Einzug
                    box.SelectionHangingIndent = 0;
                    Insert("\n", fonts.Gap, colors.Fore);
                }
            }
            box.Select(box.TextLength, 0);
            box.SelectionIndent = indent;
            box.SelectionHangingIndent = hanging;
            first = false;
            pendingGap = false;
        }

        public void Insert(string text, Font font, Color color)
        {
            box.Select(box.TextLength, 0);
            box.SelectionFont = font;
            box.SelectionColor = color;
            box.SelectedText = text;
        }

        public void InsertLink(string text, Font font, Color color, int page, string? question)
        {
            var start = box.TextLength;
            Insert(text, font, color);
            links?.Add((start, box.TextLength - start, page, question));
        }

        public void Write(string markdown)
        {
            foreach (var raw in markdown.Replace("\r\n", "\n").Split('\n'))
            {
                var line = raw.TrimEnd();
                var trimmed = line.TrimStart();
                if (trimmed.Length == 0 || RuleRegex().IsMatch(trimmed)) { RequestGap(); continue; }
                if (trimmed.StartsWith("```", StringComparison.Ordinal)) { continue; } // Codeblöcke: nur ihr Inhalt
                if (trimmed.StartsWith('>')) { trimmed = trimmed.TrimStart('>').TrimStart(); } // Zitate als einfacher Absatz
                var level = (line.Length - line.TrimStart().Length) / 2; // eingerückte Unterpunkte
                if (HeadingRegex().Match(trimmed) is { Success: true } heading)
                {
                    var depth = heading.Groups[1].Value.Length;
                    var font = depth == 1 ? fonts.Heading1 : depth == 2 ? fonts.Heading2 : fonts.Bold;
                    StartParagraph(0, 0, gapBefore: true);
                    Inline(heading.Groups[2].Value.Trim().Trim('*'), font, font, font, font);
                    continue;
                }
                if (BulletRegex().Match(trimmed) is { Success: true } bullet)
                {
                    var mark = level == 0 ? "• " : "◦ ";
                    StartParagraph(indentBase + level * indentStep, TextRenderer.MeasureText(mark, fonts.Base, Size.Empty, TextFormatFlags.NoPadding).Width, gapBefore: false);
                    Insert(mark, fonts.Base, colors.Fore);
                    Inline(bullet.Groups[1].Value, fonts.Base, fonts.Bold, fonts.Italic, fonts.BoldItalic);
                    continue;
                }
                if (NumberRegex().Match(trimmed) is { Success: true } number)
                {
                    var label = number.Groups[1].Value + ". ";
                    StartParagraph(indentBase + level * indentStep, TextRenderer.MeasureText(label, fonts.Base, Size.Empty, TextFormatFlags.NoPadding).Width, gapBefore: false);
                    Insert(label, fonts.Base, colors.Fore);
                    Inline(number.Groups[2].Value, fonts.Base, fonts.Bold, fonts.Italic, fonts.BoldItalic);
                    continue;
                }
                StartParagraph(0, 0, gapBefore: false);
                Inline(trimmed, fonts.Base, fonts.Bold, fonts.Italic, fonts.BoldItalic);
            }
        }

        private void Inline(string text, Font regular, Font strong, Font emphasis, Font both)
        {
            var position = 0;
            foreach (Match match in InlineRegex().Matches(text))
            {
                if (match.Index > position) { Insert(text[position..match.Index], regular, colors.Fore); }
                if (match.Groups["from"].Success)
                {
                    InsertLink(PageLabel(match), fonts.Link, colors.Link, int.Parse(match.Groups["from"].Value, System.Globalization.CultureInfo.InvariantCulture), null);
                }
                else if (match.Groups["bi"].Success) { Insert(match.Groups["bi"].Value, both, colors.Fore); }
                else if (match.Groups["b"].Success) { Insert(match.Groups["b"].Value, strong, colors.Fore); }
                else if (match.Groups["b2"].Success) { Insert(match.Groups["b2"].Value, strong, colors.Fore); }
                else if (match.Groups["i"].Success) { Insert(match.Groups["i"].Value, emphasis, colors.Fore); }
                else { Insert(match.Groups["c"].Value, regular, colors.Fore); }
                position = match.Index + match.Length;
            }
            if (position < text.Length) { Insert(text[position..], regular, colors.Fore); }
        }
    }

    [GeneratedRegex(@"^(#{1,6})\s+(.*)$")]
    private static partial Regex HeadingRegex();

    [GeneratedRegex(@"^[-*+•]\s+(.*)$")]
    private static partial Regex BulletRegex();

    [GeneratedRegex(@"^(\d{1,3})[.)]\s+(.*)$")]
    private static partial Regex NumberRegex();

    [GeneratedRegex(@"^([-*_])(\s*\1){2,}$")]
    private static partial Regex RuleRegex();

    /// <summary>Seitenangabe, wie <see cref="SummaryPresets.SystemInstruction"/> sie verlangt: [[p.3]] oder [[p.3-5]] (auch mit einfachen
    /// Klammern oder Leerzeichen).</summary>
    [GeneratedRegex(@"\[\[?\s*p\.?\s*(?<from>\d{1,5})(?:\s*[-–]\s*p?\.?\s*(?<to>\d{1,5}))?\s*\]\]?")]
    private static partial Regex CitationRegex();

    [GeneratedRegex(@"\[\[?\s*p\.?\s*(?<from>\d{1,5})(?:\s*[-–]\s*p?\.?\s*(?<to>\d{1,5}))?\s*\]\]?|\*\*\*(?<bi>[^*]+)\*\*\*|\*\*(?<b>.+?)\*\*|__(?<b2>[^_]+)__|(?<![\w*])\*(?<i>[^*\s](?:[^*]*[^*\s])?)\*(?![\w*])|`(?<c>[^`]+)`")]
    private static partial Regex InlineRegex();
}

/// <summary>Die Arten der Zusammenfassung: Name für die Auswahl (deutscher Text = Übersetzungsschlüssel) und Anweisung an Gemini
/// (englisch – das Modell antwortet trotzdem in der Programmsprache, s. <see cref="SummaryPresets.SystemInstruction"/>).</summary>
internal static partial class SummaryPresets
{
    public sealed record Preset(string Name, string Instruction);

    public static readonly Preset[] All =
    [
        new("Zusammenfassung", "Write a structured summary of the document: one short overview paragraph, then the main points grouped under a few short headings, with bullet points."),
        new("Kurzfassung", "Summarize the document in at most five sentences as one paragraph, without headings or bullet points."),
        new("Zahlen, Daten, Fristen", "List the important figures, amounts, dates, deadlines, names and obligations in the document as bullet points, grouped under short headings. Quote numbers exactly as they appear."),
        new("Einfach erklärt", "Explain the content of the document in plain, simple language for a non-expert reader. Briefly explain technical terms. Use short paragraphs and a few bullet points."),
    ];

    /// <summary>Trennt die vorgeschlagenen Folgefragen von der Antwort (s. <see cref="SystemInstruction"/>).</summary>
    public const string FollowUpMarker = "%%%";

    /// <summary>Die Systemanweisung: Sprache der Antwort, Format (einfaches Markdown, das <see cref="Controls.SummaryPanel"/> darstellen kann),
    /// nur aus dem Dokument, Seitenangaben als [[p.N]] (werden Links), am Ende drei Folgefragen hinter <see cref="FollowUpMarker"/>.</summary>
    public static string SystemInstruction(string languageCode)
    {
        var language = languageCode switch { "en" => "English", "fr" => "French", "es" => "Spanish", _ => "German" };
        var address = languageCode switch { "en" => string.Empty, "fr" => " Address the reader informally (tu).", "es" => " Address the reader informally (tú).", _ => " Address the reader informally (du)." };
        return $"You help the reader with the text of a PDF document: you summarize it or answer questions about it. Always answer in {language}, even if the document is in another language.{address} " +
            "Format the answer in simple Markdown: headings start with '## ', bullet points with '- ', use **bold** sparingly; no tables, no code blocks, no links. " +
            "Use only information from the document; do not add outside knowledge and do not invent details. If the document does not answer a question, say so. If the text appears incomplete or garbled, say so briefly. " +
            "The document text contains page markers like [Page 3]. Where it helps the reader, cite pages exactly in the form [[p.3]] or [[p.3-5]] and never in any other form. " +
            $"After the answer, write a line containing only {FollowUpMarker}, followed by exactly three short follow-up questions the reader might ask next about this document, one per line, in {language}, without numbering or bullets.";
    }

    /// <summary>Teilt eine Antwort in Text und Folgefragen.</summary>
    public static (string Answer, List<string> FollowUps) Split(string response)
    {
        var lines = response.Replace("\r\n", "\n").Split('\n');
        var marker = Array.FindLastIndex(lines, l => l.Trim() == FollowUpMarker);
        if (marker < 0) { return (response.Trim(), []); }
        var questions = lines.Skip(marker + 1)
            .Select(l => l.Trim().TrimStart('-', '*', '•', ' ').Trim())
            .Select(l => NumberPrefixRegex().Replace(l, string.Empty))
            .Where(l => l.Length > 0 && l.Length <= 200)
            .Take(3).ToList();
        return (string.Join('\n', lines.Take(marker)).Trim(), questions);
    }

    [GeneratedRegex(@"^\d{1,2}[.)]\s*")]
    private static partial Regex NumberPrefixRegex();
}
