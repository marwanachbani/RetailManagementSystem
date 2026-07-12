using RMS.Modules.Backup.Application.Models;

namespace RMS.Modules.Backup.Application.Contracts;

/// <summary>
/// Resolves the effective backup configuration from the host application's
/// settings. Implemented by the WPF host so the Backup module never depends on
/// the Settings module directly (configuration is not duplicated).
/// </summary>
public interface IBackupSettingsProvider
{
    Task<BackupConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
}
