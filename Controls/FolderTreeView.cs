using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PDFLight.Controls;

/// <summary>
/// Ordnerbaum auf Basis des Standard-TreeView mit Explorer-Icons und verzögertem Laden.
/// Ersatz für Jam.Shell.ShellTreeView (ShellBrowser.NET) — zeigt nur Dateisystem-Ordner:
/// Desktop, Benutzerordner und Laufwerke als Wurzelknoten.
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal class FolderTreeView : TreeView
{
    private const string DummyNodeKey = "<dummy>"; // Platzhalterkind, damit der Aufklapp-Pfeil erscheint, bevor der Ordner eingelesen wurde
    private readonly ImageList iconList = new() { ColorDepth = ColorDepth.Depth32Bit, ImageSize = SystemInformation.SmallIconSize };
    private readonly Dictionary<int, int> systemIconMap = []; // System-Iconindex → Index in iconList
    private bool showHidden;
    private string pendingSelectPath; // SelectedPath, das vor der Handle-Erzeugung gesetzt wurde

    public FolderTreeView()
    {
        HideSelection = false;
        ImageList = iconList;
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string SelectedPath
    {
        get => SelectedNode?.Tag as string ?? string.Empty;
        set
        {
            if (string.IsNullOrEmpty(value)) { return; } // leerer Pfad: Auswahl (auch eine noch ausstehende) unverändert lassen
            if (!IsHandleCreated) { pendingSelectPath = value; return; } // Wurzeln werden erst mit dem Handle geladen
            SelectNodeByPath(value);
        }
    }

    [DefaultValue(false)]
    public bool ShowHidden
    {
        get => showHidden;
        set
        {
            if (showHidden == value) { return; }
            showHidden = value;
            if (IsHandleCreated && !DesignMode) { ReloadRoots(); }
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (!DesignMode)
        {
            if (Nodes.Count == 0) { LoadRoots(); }
            if (!string.IsNullOrEmpty(pendingSelectPath)) { var path = pendingSelectPath; pendingSelectPath = null; SelectNodeByPath(path); }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { iconList.Dispose(); }
        base.Dispose(disposing);
    }

    protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
    {
        PopulateNode(e.Node);
        base.OnBeforeExpand(e);
    }

    protected override void OnAfterLabelEdit(NodeLabelEditEventArgs e)
    {
        BeginInvoke(new Action(() => LabelEdit = false)); // LabelEdit wird nur für CreateDir aktiviert
        var newName = e.Label?.Trim();
        if (!string.IsNullOrEmpty(newName) && newName != e.Node.Text)
        {
            var oldPath = (string)e.Node.Tag;
            try
            {
                var newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName);
                Directory.Move(oldPath, newPath);
                e.Node.Tag = newPath;
                e.Node.Name = newPath;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                e.CancelEdit = true;
                PDFLight.Classes.TaskDlg.ErrTaskDlg(Handle, PDFLight.Classes.Lng.T("Der Ordner konnte nicht umbenannt werden."), ex);
            }
        }
        base.OnAfterLabelEdit(e);
    }

    /// <summary>Legt im ausgewählten Ordner einen neuen Unterordner an und startet optional die Umbenennung (wie Jam CreateDir).</summary>
    public void CreateDir(string name, bool allowRename)
    {
        var parent = SelectedNode;
        if (parent == null) { return; }
        if (string.IsNullOrWhiteSpace(name)) { name = "Neuer Ordner"; }
        PopulateNode(parent); // vor dem Einfügen laden, sonst würde der Dummy-Knoten den neuen Ordner überschreiben
        var parentPath = (string)parent.Tag;
        var newPath = Path.Combine(parentPath, name);
        for (var i = 2; Directory.Exists(newPath); i++) { newPath = Path.Combine(parentPath, name + " (" + i + ")"); }
        Directory.CreateDirectory(newPath); // Exceptions (fehlende Rechte etc.) behandelt der Aufrufer
        var node = CreateFolderNode(newPath, Path.GetFileName(newPath));
        parent.Nodes.Add(node);
        parent.Expand();
        SelectedNode = node;
        node.EnsureVisible();
        if (allowRename)
        {
            BeginInvoke(new Action(() => { LabelEdit = true; node.BeginEdit(); })); // BeginEdit direkt nach SelectedNode-Wechsel schlägt sonst fehl
        }
    }

    private void LoadRoots()
    {
        BeginUpdate();
        AddRootNode(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        AddRootNode(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        AddRootNode(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));
        AddRootNode(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
        foreach (var drive in DriveInfo.GetDrives())
        {
            try { if (drive.IsReady) { AddRootNode(drive.Name); } }
            catch (IOException) { } // Laufwerk nicht verfügbar
        }
        EndUpdate();
    }

    private void ReloadRoots()
    {
        BeginUpdate();
        Nodes.Clear();
        LoadRoots();
        EndUpdate();
    }

    private void AddRootNode(string path)
    {
        if (string.IsNullOrEmpty(path) || Nodes.ContainsKey(path) || !Directory.Exists(path)) { return; }
        Nodes.Add(CreateFolderNode(path, ShellInfo.GetDisplayName(path)));
    }

    private TreeNode CreateFolderNode(string path, string text)
    {
        TreeNode node = new(text) { Tag = path, Name = path };
        node.ImageIndex = node.SelectedImageIndex = GetIconIndex(path);
        node.Nodes.Add(new TreeNode(string.Empty) { Name = DummyNodeKey });
        return node;
    }

    /// <summary>Ersetzt den Dummy-Knoten durch die tatsächlichen Unterordner (einmalig pro Knoten).</summary>
    private void PopulateNode(TreeNode node)
    {
        if (node == null || node.Nodes.Count != 1 || node.Nodes[0].Name != DummyNodeKey) { return; }
        node.Nodes.Clear();
        try
        {
            List<TreeNode> children = [];
            foreach (var dir in new DirectoryInfo((string)node.Tag).EnumerateDirectories())
            {
                if (!showHidden && (dir.Attributes & FileAttributes.Hidden) != 0) { continue; }
                children.Add(CreateFolderNode(dir.FullName, dir.Name));
            }
            children.Sort((a, b) => ShellInfo.CompareNatural(a.Text, b.Text));
            node.Nodes.AddRange([.. children]);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or System.Security.SecurityException) { } // Ordner nicht lesbar → keine Unterknoten
    }

    private void SelectNodeByPath(string path)
    {
        var node = FindNodeByPath(path);
        if (node != null)
        {
            SelectedNode = node;
            node.EnsureVisible();
        }
    }

    private TreeNode FindNodeByPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) { return null; }
        path = path.Trim().Trim('"');
        if (path.Length > 3) { path = path.TrimEnd(Path.DirectorySeparatorChar); } // "C:\" behalten

        // Wurzel mit dem längsten übereinstimmenden Pfad suchen (z.B. Desktop statt C:\)
        TreeNode current = null;
        string currentPath = null;
        foreach (TreeNode root in Nodes)
        {
            var rootPath = ((string)root.Tag).TrimEnd(Path.DirectorySeparatorChar);
            var matches = path.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase)
                && (path.Length == rootPath.Length || path[rootPath.Length] == Path.DirectorySeparatorChar);
            if (matches && (currentPath == null || rootPath.Length > currentPath.Length)) { current = root; currentPath = rootPath; }
        }
        if (current == null)
        {
            // Pfad außerhalb der geladenen Wurzeln (z.B. UNC-Pfad oder erst jetzt verfügbares Laufwerk): Wurzel nachträglich anlegen
            var pathRoot = Path.GetPathRoot(path); // "E:\" bzw. "\\Server\Freigabe"
            if (string.IsNullOrEmpty(pathRoot) || !Directory.Exists(path)) { return null; }
            AddRootNode(pathRoot);
            var rootIndex = Nodes.IndexOfKey(pathRoot);
            if (rootIndex < 0) { return null; }
            current = Nodes[rootIndex];
            currentPath = ((string)current.Tag).TrimEnd(Path.DirectorySeparatorChar);
        }

        foreach (var segment in path[currentPath.Length..].Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            PopulateNode(current);
            currentPath = Path.Combine((string)current.Tag, segment);
            var child = FindChild(current, currentPath);
            if (child == null)
            {
                if (!Directory.Exists(currentPath)) { return null; }
                child = CreateFolderNode(currentPath, segment); // existiert, wurde aber herausgefiltert (z.B. versteckt)
                current.Nodes.Add(child);
            }
            current = child;
        }
        return current;
    }

    private static TreeNode FindChild(TreeNode parent, string fullPath)
    {
        foreach (TreeNode child in parent.Nodes)
        {
            if (string.Equals(child.Tag as string, fullPath, StringComparison.OrdinalIgnoreCase)) { return child; }
        }
        return null;
    }

    /// <summary>Liefert den Index des Explorer-Icons in iconList; identische System-Icons werden nur einmal gespeichert.</summary>
    private int GetIconIndex(string path)
    {
        (var systemIndex, var hIcon) = ShellInfo.GetIcon(path);
        if (systemIconMap.TryGetValue(systemIndex, out var index))
        {
            if (hIcon != IntPtr.Zero) { ShellInfo.FreeIcon(hIcon); }
            return index;
        }
        if (hIcon == IntPtr.Zero) { return 0; }
        using (var icon = Icon.FromHandle(hIcon))
        {
            iconList.Images.Add(icon); // ImageList erstellt eine eigene Kopie
        }
        ShellInfo.FreeIcon(hIcon);
        index = iconList.Images.Count - 1;
        systemIconMap.Add(systemIndex, index);
        return index;
    }
}

/// <summary>Win32-Zugriff auf Explorer-Icons und -Anzeigenamen (SHGetFileInfo) sowie natürliche Sortierung.</summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal static partial class ShellInfo
{
    private const uint SHGFI_ICON = 0x100;
    private const uint SHGFI_DISPLAYNAME = 0x200;
    private const uint SHGFI_SYSICONINDEX = 0x4000;
    private const uint SHGFI_SMALLICON = 0x1;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x10;

    // Blittable Variante (feste Zeichenpuffer statt Strings), damit der LibraryImport-Quellgenerator sie marshallen kann
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        public fixed char szDisplayName[260];
        public fixed char szTypeName[80];
    }

    [LibraryImport("shell32.dll", EntryPoint = "SHGetFileInfoW", StringMarshalling = StringMarshalling.Utf16)] // der Generator macht kein A/W-Probing
    private static partial IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyIcon(IntPtr hIcon);

    [LibraryImport("shlwapi.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int StrCmpLogicalW(string psz1, string psz2);

    /// <summary>Kleines Explorer-Icon eines Pfads: System-Iconindex und Icon-Handle (mit FreeIcon freigeben).</summary>
    public static (int SystemIndex, IntPtr Handle) GetIcon(string path)
    {
        SHFILEINFO info = default;
        var result = SHGetFileInfo(path, 0, ref info, (uint)Marshal.SizeOf<SHFILEINFO>(), SHGFI_ICON | SHGFI_SYSICONINDEX | SHGFI_SMALLICON);
        return result == IntPtr.Zero ? (0, IntPtr.Zero) : (info.iIcon, info.hIcon);
    }

    public static void FreeIcon(IntPtr hIcon) { _ = DestroyIcon(hIcon); }

    private static readonly Dictionary<string, Image> typeIconCache = [];

    /// <summary>Kleines Explorer-Icon für einen Dateityp (z.B. ".pdf") oder — bei extension null —
    /// für Ordner. Generisch über die Dateiattribute, also ohne Plattenzugriff; Bilder werden geteilt
    /// und dürfen deshalb nicht freigegeben werden. Null, wenn die Shell kein Icon liefert.</summary>
    public static Image GetTypeIcon(string extension)
    {
        var key = extension ?? "<ordner>";
        if (typeIconCache.TryGetValue(key, out var cached)) { return cached; }
        SHFILEINFO info = default;
        var attributes = extension == null ? 0x10u /*DIRECTORY*/ : 0x80u /*NORMAL*/;
        var result = SHGetFileInfo(extension == null ? "ordner" : "datei" + extension, attributes,
            ref info, (uint)Marshal.SizeOf<SHFILEINFO>(), SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES);
        Image image = null;
        if (result != IntPtr.Zero && info.hIcon != IntPtr.Zero)
        {
            using var icon = Icon.FromHandle(info.hIcon);
            image = icon.ToBitmap(); // eigene Kopie — das Handle kann danach weg
            FreeIcon(info.hIcon);
        }
        typeIconCache[key] = image;
        return image;
    }

    /// <summary>Explorer-Anzeigename, z.B. "Lokaler Datenträger (C:)"; bei Fehlern der Pfad selbst.</summary>
    public static unsafe string GetDisplayName(string path)
    {
        SHFILEINFO info = default;
        var result = SHGetFileInfo(path, 0, ref info, (uint)Marshal.SizeOf<SHFILEINFO>(), SHGFI_DISPLAYNAME);
        if (result == IntPtr.Zero) { return path; }
        var name = new string(info.szDisplayName); // liest den Puffer bis zum Nullzeichen
        return string.IsNullOrEmpty(name) ? path : name;
    }

    /// <summary>Sortierung wie im Explorer ("Ordner 2" vor "Ordner 10").</summary>
    public static int CompareNatural(string x, string y) { return StrCmpLogicalW(x ?? string.Empty, y ?? string.Empty); }
}
