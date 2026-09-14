using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Fragt die Zielposition für das Verschieben der angezeigten Seite ab.</summary>
public partial class MovePageForm : Form
{
    /// <summary>Die gewählte Zielposition, 1-basiert.</summary>
    public int TargetPage => (int)numTarget.Value;

    public MovePageForm(int page, int pageCount)
    {
        InitializeComponent();
        Lng.Apply(this);
        TextBoxMargins.Apply(this);
        labelPrompt.Text = string.Format(labelPrompt.Text, page, pageCount); // der Designer-Text ist der (bereits übersetzte) Formatstring
        numTarget.Maximum = pageCount;
        numTarget.Value = page;
        numTarget.Select(0, numTarget.Text.Length); // Tippen ersetzt den Vorschlag sofort
    }
}
