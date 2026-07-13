using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Application.IntegrationEvents;
using RMS.Modules.Notifications.Domain;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class BackupFailedNotificationHandler : BaseNotificationHandler<BackupFailedNotificationEvent>
{
    public BackupFailedNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(BackupFailedNotificationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Backup Failed",
            $"Backup operation failed: {e.ErrorMessage}",
            NotificationCategory.Backup,
            NotificationSeverity.Error,
            "Backup",
            cancellationToken: cancellationToken);
    }
}