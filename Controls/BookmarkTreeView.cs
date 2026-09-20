namespace PDFLight.Controls;

/// <summary>TreeView für den Lesezeichen-Editor: Die Form zeichnet die Knoten selbst (Titel links, Zielseite am rechten Rand
/// des sichtbaren Bereichs). Weil die Seitenspalte am Viewport hängt und nicht am Knoten, bleiben beim Vergrößern des Fensters
/// und beim waagerechten Scrollen sonst alte Ziffern stehen – dieses Control zeichnet in diesen Fällen den ganzen Baum neu.</summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal class BookmarkTreeView : TreeView
{
    private const int WM_HSCROLL = 0x0114, WM_MOUSEWHEEL = 0x020A, WM_MOUSEHWHEEL = 0x020E;
    private const int TVS_NOHSCROLL = 0x8000;

    /// <summary>Ohne waagerechte Bildlaufleiste (TVS_NOHSCROLL): lange Titel werden mit „…“ gekürzt, der Benutzer zieht das Fenster breiter.</summary>
    protected override CreateParams CreateParams
    {
        get
        {
            var parameters = base.CreateParams;
            parameters.Style |= TVS_NOHSCROLL;
            return parameters;
        }
    }


    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg is WM_HSCROLL or WM_MOUSEWHEEL or WM_MOUSEHWHEEL) { Invalidate(); }
    }

    protected override void OnClientSizeChanged(EventArgs e)
    {
        base.OnClientSizeChanged(e);
        Invalidate(); // die Seitenspalte wandert mit dem rechten Rand (auch wenn eine Bildlaufleiste erscheint oder verschwindet)
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate(); // die Auswahlfarbe hängt am Fokus
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void OnAfterExpand(TreeViewEventArgs e)
    {
        base.OnAfterExpand(e);
        Invalidate();
    }

    protected override void OnAfterCollapse(TreeViewEventArgs e)
    {
        base.OnAfterCollapse(e);
        Invalidate();
    }
}
