using System.IO;

namespace RMS.Modules.Settings.Application.Services;

/// <summary>
/// Resolves relative folder settings against a configurable base directory and
/// guarantees that the underlying directories exist on disk.
/// </summary>
public interface IFolderResolver
{
    string BaseDirectory { get; }
    string GetDefaultPath(string subPath);
    string Resolve(string? storedValue, string? subPath);
    string GetRelativeOrAbsolute(string value);
    void EnsureExists(string path);
}

public sealed class FolderResolver : IFolderResolver
{
    public FolderResolver(string baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }

    public string BaseDirectory { get; }

    public string GetDefaultPath(string subPath) =>
        Path.Combine(BaseDirectory, subPath ?? string.Empty);

    /// <summary>
    /// Returns the absolute path to use for a folder setting. If <paramref name="storedValue"/>
    /// is rooted it is used verbatim; otherwise it is treated as a sub-path of the base
    /// directory (or the default sub-path if empty).
    /// </summary>
    public string Resolve(string? storedValue, string? subPath)
    {
        if (!string.IsNullOrWhiteSpace(storedValue) && Path.IsPathRooted(storedValue))
            return storedValue!;

        var relative = string.IsNullOrWhiteSpace(storedValue) ? subPath : storedValue;
        return Path.Combine(BaseDirectory, relative ?? string.Empty);
    }

    public void EnsureExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        Directory.CreateDirectory(path);
    }

    /// <summary>
    /// Returns <paramref name="value"/> verbatim when rooted, otherwise combines it
    /// with the base directory. Used to persist a folder setting in a stable form.
    /// </summary>
    public string GetRelativeOrAbsolute(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return Path.IsPathRooted(value) ? value : Path.Combine(BaseDirectory, value);
    }
}
