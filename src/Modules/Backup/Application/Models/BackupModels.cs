namespace RMS.Modules.Backup.Application.Models;

/// <summary>
/// Effective backup configuration resolved from the Settings module by the host.
/// The Backup module never stores these values itself.
/// </summary>
public sealed class BackupConfiguration
{
    /// <summary>Absolute path to the live SQLite database file.</summary>
    public string DatabasePath { get; init; } = string.Empty;

    /// <summary>Resolved, absolute path to the backup folder.</summary>
    public string BackupFolder { get; init; } = string.Empty;

    /// <summary>Absolute paths of the content folders to include in every backup.</summary>
    public IReadOnlyList<string> ContentFolders { get; init; } = Array.Empty<string>();

    public bool AutomaticBackup { get; init; }
    public string Frequency { get; init; } = "Daily";
    public string Time { get; init; } = "23:00";
    public int MaximumCount { get; init; } = 10;
    public bool Compress { get; init; } = true;
    public bool VerifyIntegrity { get; init; } = true;

    /// <summary>Version of the running application (used in backup metadata).</summary>
    public string ApplicationVersion { get; init; } = "1.0.0.0";
}

/// <summary>Live progress report for a backup operation.</summary>
public sealed record BackupProgress(string Stage, int Percent, string Message);

/// <summary>Result of a completed backup operation.</summary>
public sealed class BackupResult
{
    public Guid BackupId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public long Size { get; init; }
    public string Checksum { get; init; } = string.Empty;
    public bool Compressed { get; init; }
}

/// <summary>Outcome of a backup integrity check.</summary>
public sealed record BackupVerificationResult(bool IsValid, string? Error = null);

/// <summary>Versioned, human-readable metadata persisted inside every backup artifact.</summary>
public sealed class BackupMetadata
{
    public Guid BackupId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string User { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ApplicationVersion { get; set; } = string.Empty;
    public string DatabaseVersion { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Checksum { get; set; } = string.Empty;

    /// <summary>Leaf names of the content folders that were included (e.g. "Images", "Reports").</summary>
    public IReadOnlyList<string> Contents { get; set; } = Array.Empty<string>();
}

/// <summary>Aggregated data for the Backup Dashboard screen.</summary>
public sealed class BackupDashboard
{
    public DateTime? LastBackupDate { get; set; }
    public string? LastBackupFileName { get; set; }
    public DateTime? NextScheduledBackup { get; set; }
    public string BackupFolder { get; set; } = string.Empty;
    public int TotalBackups { get; set; }
    public long TotalSize { get; set; }
    public bool AutomaticBackupEnabled { get; set; }
    public string Frequency { get; set; } = string.Empty;
}

/// <summary>A backup record presented in the Backup History grid.</summary>
public sealed class BackupHistoryEntry
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime BackupDate { get; set; }
    public long Size { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Checksum { get; set; } = string.Empty;

    /// <summary>"Available" when the artifact file still exists, otherwise "Missing".</summary>
    public string Status { get; set; } = "Available";
}

/// <summary>Request to restore the system from a backup artifact.</summary>
public sealed class RestoreRequest
{
    public string BackupPath { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public bool VerifyBeforeRestore { get; init; } = true;
}

/// <summary>Live progress report for a restore operation.</summary>
public sealed record RestoreProgress(string Stage, int Percent, string Message);

/// <summary>Result of a restore operation. The host is responsible for restarting.</summary>
public sealed record RestoreResult(bool Succeeded, string? BackupPath = null, string? PreRestoreBackupPath = null, string? Error = null);
