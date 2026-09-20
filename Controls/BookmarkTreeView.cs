namespace PDFLight.Controls;

/// <summary>TreeView für den Lesezeichen-Editor: Die Form zeichnet die Knoten selbst (Titel links, Zielseite am rechten Rand
/// des sichtbaren Bereichs). Weil die Seitenspalte am Viewport hängt und nicht am Knoten, blieben beim Vergrößern des Fensters sonst
/// alte Ziffern stehen – dieses Control zeichnet dann den ganzen Baum neu; waagerechtes Scrollen gibt es nicht (TVS_NOHSCROLL).</summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal class BookmarkTreeView : TreeView
{
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
