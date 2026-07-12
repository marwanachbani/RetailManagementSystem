namespace RMS.Modules.Backup.Application.Contracts;

/// <summary>
/// Drives automatic/scheduled backups in the background. Started by the host
/// (WPF) after startup and stopped on shutdown.
/// </summary>
public interface IBackupScheduler
{
    void Start();
    void Stop();
}
