using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Fragt das Kennwort einer geschützten PDF-Datei ab; mit confirm auch zum Vergeben
/// eines neuen Kennworts (Wiederholungsfeld, Prüfung auf Übereinstimmung).</summary>
public partial class PasswordForm : Form
{
    public string Password => textBoxPassword.Text;

    private readonly bool confirm;

    public PasswordForm(string fileName, bool confirm = false)
    {
        InitializeComponent();
        this.confirm = confirm;
        if (confirm)
        {
            Text = "Kennwort vergeben";
            buttonOK.DialogResult = DialogResult.None; // erst nach Prüfung schließen
            labelInfo.Text = Lng.T("Kennwort.Info",
                "Die Datei wird mit AES-256 (PDF 2.0) verschlüsselt." + Environment.NewLine +
                "Das Benutzer-Kennwort wird künftig bei jedem Öffnen abgefragt —" + Environment.NewLine +
                "ohne Kennwort lässt sich die Datei nicht mehr anzeigen.");
            labelInfo.Visible = true;
            var shift = labelInfo.Height + 10; // Eingabezeilen unter den Erklärtext rücken
            labelPassword.Top += shift;
            textBoxPassword.Top += shift;
            labelRepeat.Top += shift;
            textBoxRepeat.Top += shift;
            Height += textBoxRepeat.Height + 10 + shift;
        }
        else
        {
            labelRepeat.Visible = false;
            textBoxRepeat.Visible = false;
        }
        Lng.Apply(this);
        labelFileValue.Text = fileName;
    }

    private void ButtonOK_Click(object sender, EventArgs e)
    {
        if (!confirm) { return; } // DialogResult.OK schließt das Formular selbst
        if (textBoxPassword.Text.Length == 0)
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Bitte geben Sie ein Kennwort ein."), null, TaskDialogIcon.Warning);
            return;
        }
        if (textBoxPassword.Text != textBoxRepeat.Text)
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Die Kennwörter stimmen nicht überein."), null, TaskDialogIcon.Warning);
            textBoxRepeat.SelectAll();
            textBoxRepeat.Focus();
            return;
        }
        DialogResult = DialogResult.OK;
    }
}
