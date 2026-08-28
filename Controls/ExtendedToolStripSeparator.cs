using System.Drawing;

namespace PDFLight.Controls;

/// <summary>Durchgezogener schwarzer Trennstrich — markiert im Datums-Menü die Grenze zwischen Präfixen und Suffixen.</summary>
internal class ExtendedToolStripSeparator : ToolStripSeparator
{
    public ExtendedToolStripSeparator() { Paint += ExtendedToolStripSeparator_Paint; }

    private void ExtendedToolStripSeparator_Paint(object sender, PaintEventArgs e)
    {
        using SolidBrush background = new(Color.FromArgb(253, 253, 253));
        using Pen line = new(Color.Black);
        e.Graphics.FillRectangle(background, 0, 0, Width, Height);
        e.Graphics.DrawLine(line, 0, Height / 2, Width - 1, Height / 2);
    }
}
