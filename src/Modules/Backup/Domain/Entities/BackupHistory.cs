namespace RMS.Modules.Backup.Domain.Entities;

/// <summary>
/// A single row in the BackupHistory table. Stores the metadata of a backup that
/// was created by the application (manual, automatic or scheduled).
/// </summary>
public sealed class BackupHistory
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public DateTime BackupDate { get; init; }
    public long Size { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public string Checksum { get; init; } = string.Empty;

    public BackupHistory() { }

    public BackupHistory(
        Guid id,
        string fileName,
        string filePath,
        DateTime backupDate,
        long size,
        string userName,
        string version,
        string? notes,
        string checksum)
    {
        Id = id;
        FileName = fileName;
        FilePath = filePath;
        BackupDate = backupDate;
        Size = size;
        UserName = userName;
        Version = version;
        Notes = notes;
        Checksum = checksum;
    }
}
