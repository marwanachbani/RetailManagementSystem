using RMS.Modules.Backup.Domain.Entities;

namespace RMS.Modules.Backup.Application.Contracts;

/// <summary>
/// Read/write access to the BackupHistory table. Implemented in the
/// Backup Infrastructure layer; only depends on the Building Blocks
/// <see cref="RMS.BuildingBlocks.Contracts.IDbConnectionFactory"/>.
/// </summary>
public interface IBackupStore
{
    Task InsertAsync(BackupHistory history, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BackupHistory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BackupHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
