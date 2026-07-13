using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Application.IntegrationEvents;
using RMS.Modules.Notifications.Domain;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class DiskSpaceWarningNotificationHandler : BaseNotificationHandler<DiskSpaceWarningNotificationEvent>
{
    public DiskSpaceWarningNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(DiskSpaceWarningNotificationEvent e, CancellationToken cancellationToken = default)
    {
        var sizeInfo = FormatBytes(e.AvailableBytes);
        await CreateNotificationAsync(
            "Disk Space Warning",
            $"Low disk space on {e.DriveName}. Available: {sizeInfo}",
            NotificationCategory.System,
            NotificationSeverity.Warning,
            "System",
            cancellationToken: cancellationToken);
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}