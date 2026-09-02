using System.Drawing.Drawing2D;

namespace PDFLight.Controls;

internal class RoundButton : Button
{
    public RoundButton()
    {
        DoubleBuffered = true;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
        //e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
        //e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
        //e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        //e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using (GraphicsPath gp = new())
        {
            gp.AddEllipse(0, 0, ClientSize.Width, ClientSize.Height);
            Region = new Region(gp);
        }
        base.OnPaint(e);
    }
}
