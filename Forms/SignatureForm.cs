using PDFLight.Classes;

namespace PDFLight.Forms;

/// <summary>„Unterschrift hinzufügen“ (Wunsch vom 03.11.2026): im Feld zeichnen oder ein Bild laden (PNG mit Durchsichtigkeit oder ein Scan,
/// dessen heller Hintergrund durchsichtig wird). <see cref="Signature"/> ist das Ergebnis nach „Übernehmen“.</summary>
public partial class SignatureForm : Form
{
    internal Signature? Signature { get; private set; }

    public SignatureForm()
    {
        InitializeComponent();
        Lng.Apply(this);
        signaturePad.EmptyText = Lng.T("Hier unterschreiben"); // eigene Eigenschaft – Lng.Apply übersetzt nur Text
        // Text mit Platzhalter für den Pfad – als %APPDATA%\…, so bleibt seine Länge unabhängig vom Benutzernamen; zweizeilig aus dem
        // Designer, deshalb ein benannter Schlüssel (resx-Schlüssel dürfen keinen Zeilenumbruch enthalten)
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        labelStorage.Text = string.Format(Lng.T("Unterschrift.Speicherort", labelStorage.Text), Path.Combine("%APPDATA%", Path.GetRelativePath(appData, SignatureStore.FilePath)));
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (DialogResult == DialogResult.OK) { Signature = signaturePad.CreateSignature(); }
    }

    private void SignaturePad_ContentChanged(object? sender, EventArgs e) => buttonOK.Enabled = buttonClear.Enabled = signaturePad.HasContent;

    private void ButtonClear_Click(object? sender, EventArgs e)
    {
        signaturePad.Clear();
        signaturePad.Focus();
    }

    private void ButtonLoadImage_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new()
        {
            Title = Lng.T("Bild der Unterschrift laden"),
            Filter = Lng.T("Bilder (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff"),
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        Signature? signature;
        try
        {
            using var image = Image.FromFile(dialog.FileName);
            using Bitmap bitmap = new(image);
            bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution); // die Kopie hätte sonst 96 dpi – die Auflösung ergibt die Originalgröße
            signature = Signature.FromImage(bitmap);
        }
        catch (Exception ex) when (ex is OutOfMemoryException or ArgumentException or IOException or UnauthorizedAccessException) // GDI+ meldet unlesbare Bilder als OutOfMemory
        {
            TaskDlg.ErrTaskDlg(Handle, Lng.T("Das Bild konnte nicht geladen werden."), ex);
            return;
        }
        if (signature == null)
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Auf dem Bild ist keine Unterschrift zu erkennen."), Lng.T("Es scheint nur hellen Hintergrund zu enthalten."), TaskDialogIcon.Warning);
            return;
        }
        signaturePad.ShowImage(signature);
    }
}
