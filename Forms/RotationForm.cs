using System.Reflection;
using PDFLight.Classes;
using PDFLight.Viewer;

namespace PDFLight.Forms;

/// <summary>Seiten drehen (Pfeil neben den Drehen-Knöpfen der Viewer-Leiste, Strg+R; Wunsch vom 10.10.2026 nach PDFMovers RotationFormR –
/// vorher „Seiten drehen…“ im Bearbeiten-Menü, das sofort in die Datei schrieb, neben den Knöpfen, die ungespeichert drehten): kleines
/// Fenster ohne Rahmen im Stil der Farbpaletten der Leiste (hell/dunkel) – alle Seiten, aktuelle Seite oder Bereich, 90° rechts, 90° links
/// oder 180°.
/// Nicht modal wie ein Dropdown: Ein Klick daneben schließt es ohne Änderung, Doppelklick auf eine Drehung bestätigt. Gedreht wird wie
/// alles über die Viewer-Leiste ungespeichert (MainForm.RotationForm_FormClosed).</summary>
public partial class RotationForm : Form
{
    private readonly int currentPage;
    private readonly int pageCount;
    private readonly Color borderColor;
    private bool closing;

    /// <summary>Die gewählten Seiten, 1-basiert und aufsteigend.</summary>
    public List<int> SelectedPages => radioAllPages.Checked ? [.. Enumerable.Range(1, pageCount)]
        : radioCurrentPage.Checked ? [currentPage]
        : [.. Enumerable.Range((int)numFrom.Value, (int)(numTo.Value - numFrom.Value) + 1)];

    /// <summary>+90 (rechts), -90 (links) oder 180 Grad.</summary>
    public int RotationDelta => radioLeft.Checked ? -90 : radioTurn.Checked ? 180 : 90;

    public RotationForm(int currentPage, int pageCount, bool dark)
    {
        InitializeComponent();
        Lng.Apply(this);
        this.currentPage = currentPage;
        this.pageCount = pageCount;
        numFrom.Maximum = numTo.Maximum = pageCount;
        numFrom.Value = currentPage; // Bereich: von der angezeigten bis zur letzten Seite (wie PDFMover)
        numTo.Value = pageCount;
        // Doppelklick auf eine Drehung bestätigt (wie in PDFMover) – ein RadioButton meldet Doppelklicks erst mit diesen Stilen
        var setStyle = typeof(RadioButton).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var radio in new[] { radioRight, radioLeft, radioTurn }) { setStyle?.Invoke(radio, [ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true]); }
        // Farben der Paletten an der Leiste (InkPalette.ApplyTheme), Rahmen wie ein Dropdown
        BackColor = dark ? Color.FromArgb(44, 44, 44) : SystemColors.Window;
        ForeColor = dark ? ViewerStripRenderer.Foreground : SystemColors.ControlText;
        borderColor = dark ? Color.FromArgb(80, 80, 80) : Color.FromArgb(160, 160, 160);
        if (dark)
        {
            foreach (var number in new[] { numFrom, numTo })
            {
                number.BackColor = ViewerStripRenderer.FieldBackground;
                number.ForeColor = ViewerStripRenderer.Foreground;
                number.BorderStyle = BorderStyle.FixedSingle;
            }
            foreach (var button in new[] { buttonOK, buttonCancel })
            {
                button.FlatStyle = FlatStyle.Flat;
                button.BackColor = ViewerStripRenderer.Background;
                button.FlatAppearance.BorderColor = Color.FromArgb(110, 110, 110);
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using Pen pen = new(borderColor);
        e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
    }

    /// <summary>Wie ein Dropdown: Ein Klick daneben (oder ein Fensterwechsel) schließt ohne Änderung.</summary>
    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);
        if (!closing && IsHandleCreated) { closing = true; BeginInvoke(Close); }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        closing = true;
        base.OnFormClosing(e);
    }

    private void NumFrom_ValueChanged(object? sender, EventArgs e)
    {
        if (numFrom.Value > numTo.Value) { numTo.Value = numFrom.Value; }
    }

    private void NumTo_ValueChanged(object? sender, EventArgs e)
    {
        if (numTo.Value < numFrom.Value) { numFrom.Value = numTo.Value; }
    }

    /// <summary>In ein Zahlenfeld: Bereich wählen, Zahl markieren – Tippen ersetzt sie.</summary>
    private void NumRange_Enter(object? sender, EventArgs e)
    {
        radioRange.Checked = true;
        if (sender is NumericUpDown number) { number.Select(0, number.Text.Length); }
    }

    private void RadioRotation_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        ButtonOK_Click(sender, e);
    }

    private void ButtonOK_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.OK; // nicht modal: DialogResult schließt nicht von selbst
        Close();
    }

    private void ButtonCancel_Click(object? sender, EventArgs e)
    {
        Close();
    }
}
