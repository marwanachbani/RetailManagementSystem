using System.Windows;
using System.Windows.Interop;

namespace RMS.WPF.Services;

/// <summary>
/// Abstraction over a native folder picker so view models stay testable and the
/// underlying OS dialog stays behind an interface.
/// </summary>
public interface IFolderBrowserService
{
    /// <summary>Shows a folder browser dialog, returning the selected folder or null.</summary>
    string? Browse(string? initialPath, string description);
}

public sealed class FolderBrowserService : IFolderBrowserService
{
    public string? Browse(string? initialPath, string description)
    {
        var owner = Application.Current?.MainWindow is { } window
            ? new WindowInteropHelper(window).Handle
            : IntPtr.Zero;

        return NativeFolderDialog.ShowDialog(owner, description, initialPath);
    }
}
