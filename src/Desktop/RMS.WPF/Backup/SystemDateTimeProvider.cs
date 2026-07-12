using RMS.BuildingBlocks.Contracts;

namespace RMS.WPF.Backup;

/// <summary>
/// <see cref="IDateTimeProvider"/> backed by the system clock. Registered by the
/// WPF host so background services (e.g. the backup scheduler) can obtain the
/// current UTC time without a direct dependency on the host.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
