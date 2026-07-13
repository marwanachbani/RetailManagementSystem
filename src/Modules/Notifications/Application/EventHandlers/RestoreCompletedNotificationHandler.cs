using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Backup.Application.IntegrationEvents;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class RestoreCompletedNotificationHandler : BaseNotificationHandler<BackupRestoredIntegrationEvent>
{
    public RestoreCompletedNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(BackupRestoredIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Restore Completed",
            $"Backup '{e.FileName}' has been restored successfully by {e.UserName}.",
            NotificationCategory.Backup,
            NotificationSeverity.Success,
            "Backup",
            e.BackupId,
            cancellationToken);
    }
}