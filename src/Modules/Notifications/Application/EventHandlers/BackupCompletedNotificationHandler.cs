using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Backup.Application.IntegrationEvents;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class BackupCompletedNotificationHandler : BaseNotificationHandler<BackupCreatedIntegrationEvent>
{
    public BackupCompletedNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(BackupCreatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Backup Completed",
            $"Backup '{e.FileName}' completed successfully. Size: {FormatBytes(e.Size)}",
            NotificationCategory.Backup,
            NotificationSeverity.Success,
            "Backup",
            e.BackupId,
            cancellationToken);
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