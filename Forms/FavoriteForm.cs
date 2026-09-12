using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Fragt beim Hinzufügen eines Favoriten den optionalen Namen ab (Vorbild: FavoriteAddForm in PDFMover).</summary>
public partial class FavoriteForm : Form
{
    /// <summary>Der eingegebene Name, ohne Randleerzeichen; leer = der Dateiname erscheint im Menü.</summary>
    public string FavoriteName => textBoxName.Text.Trim();

    public FavoriteForm(string fileName)
    {
        InitializeComponent();
        TextBoxMargins.Apply(this);
        Lng.Apply(this);
        labelFileValue.Text = fileName;
    }
}
