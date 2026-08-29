using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Fragt das Kennwort einer geschützten PDF-Datei ab.</summary>
public partial class PasswordForm : Form
{
    public string Password => textBoxPassword.Text;

    public PasswordForm(string fileName)
    {
        InitializeComponent();
        Lng.Apply(this);
        labelFileValue.Text = fileName;
    }
}
