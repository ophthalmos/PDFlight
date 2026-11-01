namespace PDFLight.Classes;

/// <summary>Augensymbol am rechten Rand eines Kennwortfelds, das die Eingabe sichtbar macht – im Kennwortfenster beim Öffnen und in den
/// Eigenschaften (Reiter „Sicherheit“, seit 06.10.2026: beim zweimaligen Eintippen eines neuen Kennworts fallen Tippfehler sonst erst beim
/// nächsten Öffnen auf).</summary>
internal static class PasswordReveal
{
    /// <param name="box">Das Kennwortfeld (UseSystemPasswordChar).</param>
    /// <param name="tip">Tooltip-Komponente des Formulars für „Kennwort anzeigen/verbergen“.</param>
    public static void Attach(TextBox box, ToolTip tip)
    {
        RevealButton reveal = new(box, tip) { Width = box.Height };
        box.Controls.Add(reveal);
        reveal.SetBoxMargin();
    }

    /// <summary>Der Knopf selbst: schaltet beim Klick die Kennwortzeichen des Felds um und passt seinen Hintergrund an – auch, wenn das Feld
    /// gesperrt wird (er erbt Enabled vom Feld). So hängt kein Ereignis am Designer-Feld (Designer-Regel, Review 06.10.2026).</summary>
    private sealed class RevealButton : Button
    {
        private readonly TextBox box;
        private readonly ToolTip tip;

        public RevealButton(TextBox box, ToolTip tip)
        {
            this.box = box;
            this.tip = tip;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Default;
            TabStop = false;
            Dock = DockStyle.Right;
            if (!ToolbarIcons.FontAvailable) { Text = "*"; }
            tip.SetToolTip(this, Lng.T("Kennwort anzeigen"));
            UpdateBack();
            UpdateImage();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            box.UseSystemPasswordChar = !box.UseSystemPasswordChar;
            UpdateBack();
            UpdateImage();
            tip.SetToolTip(this, Lng.T(box.UseSystemPasswordChar ? "Kennwort anzeigen" : "Kennwort verbergen"));
            box.Focus();
            box.SelectionStart = box.TextLength;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            UpdateBack();
        }

        /// <summary>Text nicht unter dem Auge: rechter Innenabstand des Felds. Umschalten der Kennwortzeichen legt das Feld samt Knopf neu an
        /// und setzt den Abstand zurück (23.10.2026 bemerkt) – deshalb bei jedem neuen Handle des Knopfs (das Feld hat dann schon seins).</summary>
        internal void SetBoxMargin()
        {
            if (box.IsHandleCreated) { NativeMethods.SendMessage(box.Handle, 0xD3 /*EM_SETMARGINS*/, 2 /*EC_RIGHTMARGIN*/, Width << 16); }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetBoxMargin();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            UpdateBack(); // erst im Feld kennt der Knopf dessen Zustand
        }

        /// <summary>Auge, solange die Eingabe verborgen ist (Klick zeigt sie), durchgestrichenes Auge, solange sie sichtbar ist (Klick verbirgt
        /// sie) – wie in anderen Programmen (Wunsch vom 23.10.2026; vorher immer dasselbe Auge, nur der Hintergrund wechselte).</summary>
        private void UpdateImage()
        {
            if (!ToolbarIcons.FontAvailable) { return; }
            var size = box.LogicalToDeviceUnits(new Size(16, 16));
            Image = box.UseSystemPasswordChar ? ToolbarIcons.Get(ToolbarIcons.Eye, size) : ToolbarIcons.EyeHidden(size, Color.FromArgb(64, 64, 64));
        }

        /// <summary>Ein gesperrtes Feld zeichnet Windows grau – das Auge soll nicht als weißes Kästchen darin stehen.</summary>
        private void UpdateBack() => BackColor = !Enabled ? SystemColors.Control : box.UseSystemPasswordChar ? SystemColors.Window : SystemColors.ControlLight;
    }
}
