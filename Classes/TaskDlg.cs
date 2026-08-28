namespace PDFLight.Classes;

/// <summary>TaskDialog-Helfer als moderner Ersatz für MessageBox.
/// MsgTaskDlg und ErrTaskDlg sind unverändert aus einem anderen Projekt des Autors übernommen;
/// ConfirmTaskDlg und die ErrTaskDlg-Variante mit fachlicher Überschrift ergänzen sie im selben Stil.</summary>
internal static class TaskDlg
{
    public static void MsgTaskDlg(nint hwnd, string heading, string message, TaskDialogIcon icon = null)
    {
        TaskDialog.ShowDialog(hwnd, new TaskDialogPage() { Caption = Application.ProductName, SizeToContent = true, Heading = heading, Text = message, Icon = icon ?? TaskDialogIcon.None, AllowCancel = true, Buttons = { TaskDialogButton.OK } });
    }

    public static void ErrTaskDlg(nint? hwnd, Exception error)
    {
        TaskDialogPage page = new()
        {
            Caption = Application.ProductName,
            Heading = error.GetType().ToString(),
            Text = error.Message,
            Icon = TaskDialogIcon.Error,
            SizeToContent = true,
            AllowCancel = true,
            Buttons = { TaskDialogButton.OK },
            Expander = new TaskDialogExpander()
            {
                Text = error.ToString(),
                CollapsedButtonText = "Technische Details anzeigen",
                ExpandedButtonText = "Details ausblenden",
                Position = TaskDialogExpanderPosition.AfterFootnote
            }
        };
        TaskDialog.ShowDialog(hwnd ?? 0, page);
    }

    /// <summary>Fehlerdialog mit fachlicher Überschrift (z.B. "Verschieben fehlgeschlagen.") statt des Ausnahmetyps.</summary>
    public static void ErrTaskDlg(nint? hwnd, string heading, Exception error)
    {
        TaskDialogPage page = new()
        {
            Caption = Application.ProductName,
            Heading = heading,
            Text = error.Message,
            Icon = TaskDialogIcon.Error,
            SizeToContent = true,
            AllowCancel = true,
            Buttons = { TaskDialogButton.OK },
            Expander = new TaskDialogExpander()
            {
                Text = error.ToString(),
                CollapsedButtonText = "Technische Details anzeigen",
                ExpandedButtonText = "Details ausblenden",
                Position = TaskDialogExpanderPosition.AfterFootnote
            }
        };
        TaskDialog.ShowDialog(hwnd ?? 0, page);
    }

    /// <summary>Ja/Nein-Frage; true nur bei ausdrücklichem Ja (Abbrechen/Esc zählt als Nein).
    /// Mit defaultNo steht der Fokus auf "Nein" — für destruktive Aktionen wie das Ersetzen von Dateien.</summary>
    public static bool ConfirmTaskDlg(nint hwnd, string heading, string message, TaskDialogIcon icon = null, bool defaultNo = false)
    {
        TaskDialogPage page = new() { Caption = Application.ProductName, SizeToContent = true, Heading = heading, Text = message, Icon = icon ?? TaskDialogIcon.None, AllowCancel = true, Buttons = { TaskDialogButton.Yes, TaskDialogButton.No } };
        if (defaultNo) { page.DefaultButton = page.Buttons[1]; }
        return TaskDialog.ShowDialog(hwnd, page) == TaskDialogButton.Yes;
    }
}
