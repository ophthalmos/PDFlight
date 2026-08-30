using System.Runtime.InteropServices;
using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>Fragt das Kennwort einer geschützten PDF-Datei ab; mit confirm auch zum Vergeben
/// eines neuen Kennworts (Wiederholungsfeld, Prüfung auf Übereinstimmung).</summary>
public partial class PasswordForm : Form
{
    public string Password => textBoxPassword.Text;

    private readonly bool confirm;
    private readonly ToolTip revealTip = new();

    public PasswordForm(string fileName, bool confirm = false)
    {
        InitializeComponent();
        this.confirm = confirm;
        if (confirm)
        {
            Text = "Benutzer-Kennwort vergeben";
            buttonOK.DialogResult = DialogResult.None; // erst nach Prüfung schließen
            labelInfo.Text = Lng.T("Kennwort.Info",
                "Die Datei wird mit AES-256 (PDF 2.0) verschlüsselt." + Environment.NewLine +
                "Das Kennwort wird künftig bei jedem Öffnen abgefragt." + Environment.NewLine +
                "Ohne Kennwort lässt sich die Datei nicht mehr anzeigen.");
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
        AddRevealButton(textBoxPassword);
        if (confirm) { AddRevealButton(textBoxRepeat); }
        Lng.Apply(this);
        labelFileValue.Text = fileName;
    }

    private void ButtonOK_Click(object sender, EventArgs e)
    {
        if (!confirm) { return; } // DialogResult.OK schließt das Formular selbst
        if (textBoxPassword.Text.Length == 0)
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Bitte gib ein Kennwort ein."), null, TaskDialogIcon.Warning);
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

    /// <summary>Setzt ein Augensymbol an den rechten Rand des Eingabefelds, das die Eingabe sichtbar macht.</summary>
    private void AddRevealButton(TextBox box)
    {
        Button reveal = new()
        {
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Default,
            TabStop = false,
            Dock = DockStyle.Right,
            Width = box.Height,
            BackColor = SystemColors.Window
        };
        reveal.FlatAppearance.BorderSize = 0;
        if (ToolbarIcons.FontAvailable) { reveal.Image = ToolbarIcons.Get(ToolbarIcons.Eye, LogicalToDeviceUnits(new Size(16, 16))); }
        else { reveal.Text = "*"; }
        revealTip.SetToolTip(reveal, Lng.T("Kennwort anzeigen"));
        reveal.Click += (s, e) =>
        {
            box.UseSystemPasswordChar = !box.UseSystemPasswordChar;
            reveal.BackColor = box.UseSystemPasswordChar ? SystemColors.Window : SystemColors.ControlLight;
            revealTip.SetToolTip(reveal, Lng.T(box.UseSystemPasswordChar ? "Kennwort anzeigen" : "Kennwort verbergen"));
            box.Focus();
            box.SelectionStart = box.TextLength;
        };
        box.Controls.Add(reveal);
        SendMessageW(box.Handle, 0xD3 /*EM_SETMARGINS*/, 2 /*EC_RIGHTMARGIN*/, reveal.Width << 16); // Text nicht unter dem Auge
    }

    [LibraryImport("user32.dll")]
    private static partial nint SendMessageW(nint hWnd, uint msg, nint wParam, nint lParam);
}
