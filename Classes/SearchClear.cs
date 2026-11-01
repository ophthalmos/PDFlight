namespace PDFLight.Classes;

/// <summary>„✕“ am rechten Rand eines Suchfelds, sichtbar sobald etwas eingetippt ist; ein Klick leert das Feld (Wunsch vom 23.10.2026, wie
/// in vielen Programmen) – für die Suchfelder der Startseite und der Anmerkungsliste. Aufbau wie das Augensymbol der Kennwortfelder
/// (<see cref="PasswordReveal"/>): ein Kind-Steuerelement im Feld, der Text endet davor (EM_SETMARGINS). Am Feld selbst hängt kein Ereignis
/// (Designer-Regel): Das Formular ruft <see cref="Update"/> aus seinem TextChanged-Handler und nach Farbwechseln.</summary>
internal static class SearchClear
{
    private const uint EM_SETMARGINS = 0xD3;
    private const int EC_RIGHTMARGIN = 2;

    public static void Attach(TextBox box)
    {
        box.Controls.Add(new ClearButton(box) { Visible = false });
        Update(box);
    }

    /// <summary>Sichtbarkeit, Farben und rechten Innenabstand nachführen – nach jeder Textänderung und nach einem neuen Fensterhandle
    /// (der Rahmenwechsel im Dunkelmodus setzt den Abstand zurück).</summary>
    public static void Update(TextBox box)
    {
        if (box.Controls.OfType<ClearButton>().FirstOrDefault() is not { } button) { return; }
        button.Visible = box.TextLength > 0;
        button.BackColor = box.BackColor;
        button.Invalidate();
        if (!box.IsHandleCreated) { return; }
        var right = button.Visible ? box.ClientSize.Width - button.Left : box.LogicalToDeviceUnits(2); // ohne Knopf wie TextBoxMargins.Keep
        NativeMethods.SendMessage(box.Handle, EM_SETMARGINS, EC_RIGHTMARGIN, right << 16);
    }

    /// <summary>Der Knopf: ✕ aus der Symbolschrift in Grau, beim Überfahren in der Textfarbe des Felds.</summary>
    private sealed class ClearButton : Control
    {
        private readonly TextBox box;
        private bool hot, pressed;

        public ClearButton(TextBox box)
        {
            this.box = box;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);
            TabStop = false;
            Cursor = Cursors.Default;
            // 1 px Abstand zum Rand: im Dunkelmodus (FixedSingle) zeichnet das Feld seinen Rahmen im Innern, ein angedockter Knopf deckte ihn ab
            var inset = box.LogicalToDeviceUnits(1);
            var width = box.LogicalToDeviceUnits(22);
            Bounds = new Rectangle(box.ClientSize.Width - width - inset, inset, width, Math.Max(1, box.ClientSize.Height - 2 * inset));
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            AccessibleName = Lng.T("Suche leeren");
            AccessibleRole = AccessibleRole.PushButton;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // beim Überfahren eine Fläche wie beim Augenknopf der Kennwortfelder, gedrückt etwas kräftiger (Wunsch vom 23.10.2026 – vorher
            // wechselte nur die Farbe des Kreuzes, kaum zu sehen); in Textfarbe halbdurchsichtig, passt so auf hell und dunkel
            if (hot) { using SolidBrush hover = new(Color.FromArgb(pressed ? 60 : 30, box.ForeColor)); e.Graphics.FillRectangle(hover, ClientRectangle); }
            var color = hot ? box.ForeColor : Color.FromArgb(200, box.ForeColor);
            var size = LogicalToDeviceUnits(12);
            var area = new Rectangle((Width - size) / 2, (Height - size) / 2, size, size);
            if (ToolbarIcons.FontAvailable) { e.Graphics.DrawImage(ToolbarIcons.Get(ToolbarIcons.Cancel, new Size(size, size), color), area); }
            else { TextRenderer.DrawText(e.Graphics, "✕", Font, ClientRectangle, color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter); }
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hot = true; Invalidate(); }

        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hot = pressed = false; Invalidate(); }

        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); pressed = e.Button == MouseButtons.Left; Invalidate(); }

        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); pressed = false; Invalidate(); }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            box.Clear(); // löst TextChanged aus – das Formular filtert neu und blendet den Knopf aus (Update)
            box.Focus();
        }
    }
}
