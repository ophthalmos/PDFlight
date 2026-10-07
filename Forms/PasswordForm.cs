using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Fragt beim Öffnen das Kennwort einer geschützten PDF-Datei ab. Vergeben und Entfernen des Kennworts laufen seit 06.10.2026
/// über die Eigenschaften (Reiter „Sicherheit“) – der frühere Modus mit Wiederholungsfeld entfiel.</summary>
public partial class PasswordForm : Form
{
    public string Password => textBoxPassword.Text;

    private readonly ToolTip revealTip = new();

    public PasswordForm(string fileName)
    {
        InitializeComponent();
        TextBoxMargins.Apply(this);
        PasswordReveal.Attach(textBoxPassword, revealTip);
        Lng.Apply(this);
        labelFileValue.Text = fileName;
    }
}
