using System.Threading;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Backup.Application;
using RMS.Modules.Backup.Application.Contracts;

namespace RMS.Modules.Backup.Application;

/// <summary>
/// Background scheduler that creates automatic/scheduled backups according to the
/// configuration from the Settings module. Runs a lightweight check every minute and
/// triggers a backup when the next scheduled time has been reached. A failing scheduled
/// backup is swallowed (best-effort) so it can never crash the host application.
/// </summary>
public sealed class BackupScheduler : IBackupScheduler
{
    private readonly IBackupService _service;
    private readonly IBackupSettingsProvider _settings;
    private readonly IDateTimeProvider _clock;
    private readonly IBackupStore _store;
    private Timer? _timer;
    private readonly object _sync = new();
    private bool _running;

    public BackupScheduler(IBackupService service, IBackupSettingsProvider settings, IDateTimeProvider clock, IBackupStore store)
    {
        _service = service;
        _settings = settings;
        _clock = clock;
        _store = store;
    }

    public void Start()
    {
        if (_timer is null)
            _timer = new Timer(_ => _ = TickAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    private async Task TickAsync()
    {
        if (_running) return;
        lock (_sync)
        {
            if (_running) return;
            _running = true;
        }

        try
        {
            var config = await _settings.GetConfigurationAsync();
            if (!config.AutomaticBackup) return;

            var history = await _store.GetAllAsync();
            var last = history.Count > 0 ? history[0].BackupDate : (DateTime?)null;
            var now = TimeZoneInfo.ConvertTimeFromUtc(_clock.UtcNow, TimeZoneInfo.Local);

            if (BackupScheduleHelper.IsDue(last, config.Frequency, ParseTime(config.Time), now))
            {
                await _service.CreateBackupAsync("Scheduled backup", null, CancellationToken.None);
            }
        }
        catch
        {
            // Best-effort: a failing scheduled backup must never disrupt the application.
        }
        finally
        {
            lock (_sync) { _running = false; }
        }
    }

    private static TimeSpan ParseTime(string value) =>
        TimeSpan.TryParse(value, out var ts) ? ts : new TimeSpan(23, 0, 0);
}
