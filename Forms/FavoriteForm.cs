using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Fragt beim Hinzufügen eines Favoriten den optionalen Namen ab (Vorbild: FavoriteAddForm in PDFMover).</summary>
public partial class FavoriteForm : Form
{
    /// <summary>Der eingegebene Name, ohne Randleerzeichen; leer = nur die Seitenzahl anzeigen.</summary>
    public string FavoriteName => textBoxName.Text.Trim();

    public FavoriteForm(string fileName, int page)
    {
        InitializeComponent();
        TextBoxMargins.Apply(this);
        Lng.Apply(this);
        labelFileValue.Text = fileName;
        labelPage.Text = string.Format(Lng.T("Seite {0} zu den Favoriten hinzufügen"), page);
    }
}
