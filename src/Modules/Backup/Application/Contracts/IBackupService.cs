using RMS.BuildingBlocks.Results;
using RMS.Modules.Backup.Application.Models;

namespace RMS.Modules.Backup.Application.Contracts;

/// <summary>
/// Core backup &amp; restore engine. Long-running operations accept an
/// <see cref="IProgress{T}"/> for live UI feedback and a <see cref="CancellationToken"/>
/// so the user can cancel. Implemented in the Backup Infrastructure layer.
/// </summary>
public interface IBackupService
{
    /// <summary>Creates a new backup (manual, automatic or scheduled).</summary>
    Task<BackupResult> CreateBackupAsync(
        string? notes,
        IProgress<BackupProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>Verifies the integrity (checksum) of a backup artifact.</summary>
    Task<BackupVerificationResult> VerifyBackupAsync(
        string backupPath,
        CancellationToken cancellationToken = default);

    /// <summary>Reads the metadata of a backup artifact without restoring it.</summary>
    Task<BackupMetadata?> GetBackupDetailsAsync(
        string backupPath,
        CancellationToken cancellationToken = default);

    /// <summary>Aggregated data for the Backup Dashboard.</summary>
    Task<BackupDashboard> GetDashboardAsync(CancellationToken cancellationToken = default);

    /// <summary>All backups recorded in the BackupHistory table.</summary>
    Task<IReadOnlyList<BackupHistoryEntry>> GetHistoryAsync(CancellationToken cancellationToken = default);

    /// <summary>Deletes a backup file and its history record.</summary>
    Task<Result> DeleteBackupAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Removes the oldest backups beyond the configured maximum count.</summary>
    Task<Result> CleanupOldBackupsAsync(CancellationToken cancellationToken = default);

    /// <summary>Restores the database and content from a backup artifact.</summary>
    Task<RestoreResult> RestoreAsync(
        RestoreRequest request,
        IProgress<RestoreProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
