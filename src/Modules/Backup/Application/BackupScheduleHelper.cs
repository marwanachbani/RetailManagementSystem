namespace RMS.Modules.Backup.Application;

/// <summary>
/// Pure scheduling math for automatic/scheduled backups. Given the last backup time,
/// the configured frequency and the daily backup time, computes the next run time.
/// Kept free of DateTime.Now so it can be unit tested deterministically.
/// </summary>
public static class BackupScheduleHelper
{
    /// <summary>
    /// Computes the next scheduled backup time from the last backup date.
    /// <list type="bullet">
    ///   <item><description>Daily — one day after the last backup date, at the configured time.</description></item>
    ///   <item><description>Weekly — seven days after the last backup date, at the configured time.</description></item>
    ///   <item><description>Monthly — one month after the last backup date, at the configured time.</description></item>
    /// </list>
    /// When there is no previous backup, returns today at the configured time.
    /// If that time has already passed, returns the current time so the first backup
    /// runs immediately.
    /// </summary>
    public static DateTime? ComputeNextRun(
        DateTime? lastBackup,
        string frequency,
        TimeSpan timeOfDay,
        DateTime now)
    {
        if (lastBackup.HasValue)
            return AddInterval(lastBackup.Value.Date, frequency).Add(timeOfDay);

        var today = now.Date.Add(timeOfDay);
        return today > now ? today : now;
    }

    /// <summary>True when a scheduled backup is due (now is at/after the next run time).</summary>
    public static bool IsDue(DateTime? lastBackup, string frequency, TimeSpan timeOfDay, DateTime now)
    {
        var next = ComputeNextRun(lastBackup, frequency, timeOfDay, now);
        return next.HasValue && now >= next.Value;
    }

    private static DateTime AddInterval(DateTime date, string frequency) =>
        frequency.Trim().ToLowerInvariant() switch
        {
            "weekly" => date.AddDays(7),
            "monthly" => date.AddMonths(1),
            _ => date.AddDays(1)
        };
}
