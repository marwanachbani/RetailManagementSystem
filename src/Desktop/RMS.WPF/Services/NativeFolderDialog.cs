using System.Runtime.InteropServices;
using System.Text;

namespace RMS.WPF.Services;

/// <summary>
/// Thin P/Invoke wrapper around the Windows Shell folder browser dialog
/// (SHBrowseForFolder). Avoids a WinForms dependency so the WPF project keeps a
/// single UI stack.
/// </summary>
internal static class NativeFolderDialog
{
    private const uint BIF_NEWDIALOGSTYLE = 0x00000040;
    private const uint BIF_RETURNONLYFSDIRS = 0x00000001;
    private const uint BFFM_INITIALIZED = 0x00000001;
    private const uint BFFM_SETSELECTIONW = 0x00000467;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct BROWSEINFO
    {
        public IntPtr hwndOwner;
        public IntPtr pidlRoot;
        public IntPtr pszDisplayName;
        public string lpszTitle;
        public uint ulFlags;
        public IntPtr lpfn;
        public IntPtr lParam;
        public int iImage;
    }

    private delegate int BrowseCallbackProc(IntPtr hwnd, uint uMsg, IntPtr lParam, IntPtr lpData);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern bool SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);

    public static string? ShowDialog(IntPtr owner, string title, string? initialPath)
    {
        var initialPtr = IntPtr.Zero;
        try
        {
            var bi = new BROWSEINFO
            {
                hwndOwner = owner,
                pidlRoot = IntPtr.Zero,
                lpszTitle = title,
                ulFlags = BIF_NEWDIALOGSTYLE | BIF_RETURNONLYFSDIRS
            };

            BrowseCallbackProc callback = (hwnd, uMsg, _, lpData) =>
            {
                if (uMsg == BFFM_INITIALIZED && lpData != IntPtr.Zero)
                {
                    var path = Marshal.PtrToStringUni(lpData);
                    if (!string.IsNullOrEmpty(path))
                        SendMessageW(hwnd, BFFM_SETSELECTIONW, 1, path);
                }
                return 0;
            };

            bi.lpfn = Marshal.GetFunctionPointerForDelegate(callback);

            if (!string.IsNullOrWhiteSpace(initialPath))
            {
                initialPtr = Marshal.StringToHGlobalUni(initialPath);
                bi.lParam = initialPtr;
            }

            var pidl = SHBrowseForFolder(ref bi);
            if (pidl == IntPtr.Zero) return null;

            var sb = new StringBuilder(260);
            return SHGetPathFromIDList(pidl, sb) ? sb.ToString() : null;
        }
        finally
        {
            if (initialPtr != IntPtr.Zero) Marshal.FreeHGlobal(initialPtr);
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessageW(IntPtr hWnd, uint Msg, int wParam, string lParam);
}
