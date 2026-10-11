using PDFLight.Classes;

namespace PDFLight.Controls;

/// <summary>Karte eines Kommentars (Notiz-Anmerkung) über der Seitenansicht, nach dem Vorbild von Edge (Wunsch vom 10.10.2026, bewusst
/// ohne Antworten): gelbe Fläche mit dem Text, oben rechts „…“ (Löschen, Bearbeiten) und ×. Bearbeitet wird direkt in der Karte;
/// ein Klick daneben übernimmt, Strg+Eingabe oder das Häkchen unten rechts übernimmt und schließt, Esc verwirft die Änderung. Farben bei Edge gemessen (Fläche 255/249/199, Rand
/// 255/243/134, weicher grauer Schatten). Die Karte ist ein Kind der Seitenansicht und wandert beim Blättern mit; was mit dem Text
/// geschieht, entscheidet MainForm (Ereignisse).</summary>
public partial class NoteCard : UserControl
{
    private static readonly Color Surface = Color.FromArgb(255, 249, 199);
    private static readonly Color Border = Color.FromArgb(255, 243, 134);
    private const int ShadowSize = 8; // logische px rund um die Karte, für den Schatten (Designer: Karte bei 8/8, 215 × 120)

    private string savedText = string.Empty;

    /// <summary>Bearbeiten beendet (Klick daneben, Strg+Eingabe, ×): der eingegebene Text, ohne Leerraum an den Rändern.</summary>
    public event EventHandler<string>? Committed;

    /// <summary>Esc beim Bearbeiten: der alte Text steht wieder da (bei einem neuen Kommentar schließt MainForm die Karte).</summary>
    public event EventHandler? EditCanceled;

    public event EventHandler? DeleteRequested;
    public event EventHandler? CloseRequested;

    /// <summary>Wird gerade bearbeitet?</summary>
    public bool IsEditing { get; private set; }

    public NoteCard()
    {
        InitializeComponent();
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
        textNote.PlaceholderText = Lng.T("Notiz hinzufügen");
        toolTip.SetToolTip(labelMore, Lng.T("Weitere Optionen")); // ToolTip-Komponenten übersetzt Lng.Apply nicht
        toolTip.SetToolTip(labelClose, Lng.T("Schließen"));
        toolTip.SetToolTip(buttonCommit, Lng.T("Übernehmen (Strg+Eingabe)"));
        buttonCommit.Glyph = ToolbarIcons.CheckMark; // als Bild in der Häkchenfarbe gezeichnet (ohne Symbolschrift bleibt der Text)
        readTextHeight = textNote.Height; // nach dem Skalieren: so hoch ist das Feld beim Lesen, beim Schreiben endet es über dem Häkchen
    }

    private readonly int readTextHeight;

    /// <summary>Beim Schreiben unten rechts das Häkchen wie bei Edge (Wunsch vom 10.10.2026), das Textfeld endet dann darüber.</summary>
    private void ShowCommitButton(bool editing)
    {
        buttonCommit.Visible = editing;
        textNote.Height = editing ? Math.Max(LogicalToDeviceUnits(20), buttonCommit.Top - LogicalToDeviceUnits(4) - textNote.Top) : readTextHeight;
        UpdateScrollBar();
    }

    /// <summary>Symbole für „Löschen“ und „Bearbeiten“ (aus MainForm, wie die übrigen Menüsymbole; null = ohne).</summary>
    public void SetMenuIcons(Image? delete, Image? edit)
    {
        mnuDelete.Image = delete;
        mnuEdit.Image = edit;
    }

    /// <summary>Zeigt den Text; <paramref name="edit"/> öffnet gleich zum Bearbeiten (neuer Kommentar), <paramref name="canEdit"/> = false
    /// zeigt nur an (gesperrte Datei).</summary>
    public void ShowNote(string text, bool edit, bool canEdit)
    {
        savedText = text;
        textNote.Text = text;
        textNote.ReadOnly = true;
        IsEditing = false;
        ShowCommitButton(false);
        mnuDelete.Enabled = mnuEdit.Enabled = canEdit;
        UpdateScrollBar();
        Visible = true;
        BringToFront();
        if (edit && canEdit) { BeginEdit(); }
    }

    public void BeginEdit()
    {
        IsEditing = true;
        textNote.ReadOnly = false;
        ShowCommitButton(true);
        textNote.Focus();
        textNote.SelectionStart = textNote.TextLength;
    }

    /// <summary>Bearbeiten beenden und den Text melden; ohne laufendes Bearbeiten nichts.</summary>
    public void CommitEdit()
    {
        if (!IsEditing) { return; }
        IsEditing = false;
        textNote.ReadOnly = true;
        var text = textNote.Text.Trim();
        savedText = text;
        ShowCommitButton(false);
        Committed?.Invoke(this, text);
    }

    private void CancelEdit()
    {
        IsEditing = false;
        textNote.ReadOnly = true;
        textNote.Text = savedText;
        ShowCommitButton(false);
        EditCanceled?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Bildlaufleiste nur, wenn der Text nicht in die Karte passt.</summary>
    private void UpdateScrollBar()
    {
        var needed = TextRenderer.MeasureText(textNote.Text + " ", textNote.Font, new Size(textNote.ClientSize.Width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height;
        textNote.ScrollBars = needed > textNote.ClientSize.Height ? ScrollBars.Vertical : ScrollBars.None;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var shadow = LogicalToDeviceUnits(ShadowSize);
        var card = new Rectangle(shadow, shadow, Width - 2 * shadow, Height - 2 * shadow);
        for (var i = shadow; i >= 1; i--) // weicher Schatten: nach außen immer durchsichtiger
        {
            var alpha = (int)(28 * Math.Pow(1 - (double)i / (shadow + 1), 2));
            using SolidBrush brush = new(Color.FromArgb(alpha, 0, 0, 0));
            var ring = Rectangle.Inflate(card, i, i);
            ring.Offset(0, i / 3); // etwas nach unten wie bei Edge
            e.Graphics.FillRectangle(brush, ring);
        }
        using (SolidBrush fill = new(Surface)) { e.Graphics.FillRectangle(fill, card); }
        using Pen pen = new(Border);
        e.Graphics.DrawRectangle(pen, card.X, card.Y, card.Width - 1, card.Height - 1);
    }

    private void TextNote_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            e.SuppressKeyPress = true;
            if (IsEditing) { CancelEdit(); } else { CloseRequested?.Invoke(this, EventArgs.Empty); }
        }
        else if (e.KeyData == (Keys.Control | Keys.Enter) && IsEditing) // übernehmen und schließen (Wunsch vom 10.10.2026)
        {
            e.SuppressKeyPress = true;
            CommitEdit();
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    private void TextNote_Leave(object? sender, EventArgs e)
    {
        if (!contextMenuNote.Visible) { CommitEdit(); } // Klick daneben übernimmt wie bei Edge (auch der aufs Häkchen, das den Fokus nimmt)
    }

    /// <summary>Häkchen: übernehmen und schließen wie Strg+Eingabe.</summary>
    private void ButtonCommit_Click(object? sender, EventArgs e)
    {
        CommitEdit();
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void LabelMore_Click(object? sender, EventArgs e)
    {
        contextMenuNote.Show(labelMore, new Point(0, labelMore.Height));
    }

    private void LabelClose_Click(object? sender, EventArgs e)
    {
        CommitEdit();
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void MnuDelete_Click(object? sender, EventArgs e)
    {
        IsEditing = false;
        DeleteRequested?.Invoke(this, EventArgs.Empty);
    }

    private void MnuEdit_Click(object? sender, EventArgs e)
    {
        BeginEdit();
    }
}
