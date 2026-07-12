using System.IO;
using System.Runtime.InteropServices;

namespace RMS.WPF.Backup;

/// <summary>
/// Maps low-level exceptions thrown by the backup engine to friendly, human-readable
/// messages for the UI (missing/corrupted backups, locked database, disk full, permissions).
/// </summary>
public static class BackupErrorMapper
{
    public static string ToFriendlyMessage(Exception ex) => ex switch
    {
        OperationCanceledException => "The operation was cancelled.",
        UnauthorizedAccessException => "Permission denied. The application could not access the required files or folders. Try running as an administrator or check folder permissions.",
        IOException io when IsDiskFull(io) => "The disk is full. Free up space and try again.",
        IOException io when IsFileLocked(io) => "The database or a backup file is locked by another process. Close other instances and try again.",
        IOException io => $"A file error occurred: {io.Message}",
        _ => $"An unexpected error occurred: {ex.Message}"
    };

    private static bool IsDiskFull(IOException ex)
    {
        const int ERROR_DISK_FULL = unchecked((int)0x80070070);
        return ex.HResult == ERROR_DISK_FULL;
    }

    private static bool IsFileLocked(IOException ex)
    {
        // Common Win32 "file in use" HRESULTs (0x80070020 / 0x80070021).
        return ex.HResult is unchecked((int)0x80070020) or unchecked((int)0x80070021);
    }
}
