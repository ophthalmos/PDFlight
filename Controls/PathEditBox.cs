using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PDFLight.Controls;

/// <summary>
/// Pfad-Eingabefeld mit Übernahme-Button und Ordner-Autovervollständigung.
/// Ersatz für Jam.Shell.PathEdit (ShellBrowser.NET) — die Pfadvalidierung übernimmt der Aufrufer im ButtonClick-Handler.
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal class PathEditBox : UserControl
{
    private readonly TextBox textBox = new();
    private readonly Button button = new();
    private readonly Button historyButton = new();

    public event EventHandler ButtonClick;
    public event EventHandler HistoryButtonClick;
    public event EventHandler EditFieldEnter;
    public event EventHandler EditFieldLeave;
    public event EventHandler EditFieldClick;

    public PathEditBox()
    {
        button.Text = "↵";
        button.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        button.Width = 27;
        button.Dock = DockStyle.Right;
        button.TabIndex = 1;
        button.Click += (sender, e) => ButtonClick?.Invoke(this, EventArgs.Empty);
        historyButton.Text = "▼";
        historyButton.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point);
        historyButton.Width = 22;
        historyButton.Dock = DockStyle.Right;
        historyButton.TabIndex = 2;
        historyButton.Enabled = false; // wird über FolderHistoryToolBar.DropDownAnchor aktiviert, sobald ein Verlauf existiert
        historyButton.Click += (sender, e) => HistoryButtonClick?.Invoke(this, EventArgs.Empty);
        textBox.Dock = DockStyle.Fill;
        textBox.AutoCompleteMode = AutoCompleteMode.Suggest;
        textBox.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
        textBox.TabIndex = 0;
        textBox.Enter += (sender, e) => EditFieldEnter?.Invoke(this, e);
        textBox.Leave += (sender, e) => EditFieldLeave?.Invoke(this, e);
        textBox.Click += (sender, e) => EditFieldClick?.Invoke(this, e);
        Controls.Add(textBox);
        Controls.Add(button);
        Controls.Add(historyButton); // zuletzt hinzugefügt → wird beim Andocken zuerst platziert und sitzt ganz rechts
        Height = textBox.PreferredHeight;
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TextBox TextBox => textBox;

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Button Button => button;

    /// <summary>Der ▼-Button am rechten Rand, der das Verlaufsmenü öffnet.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Button HistoryButton => historyButton;

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string Text
    {
        get => textBox.Text;
        set => textBox.Text = value;
    }

    /// <summary>True, wenn der eingegebene Text ein existierender Ordner ist.</summary>
    [Browsable(false)]
    public bool IsValidPath => Directory.Exists(textBox.Text.Trim().Trim('"'));

    public new bool Focus() { return textBox.Focus(); }
}
