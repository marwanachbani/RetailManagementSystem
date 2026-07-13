using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Application.IntegrationEvents;
using RMS.Modules.Notifications.Domain;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class MigrationFailureNotificationHandler : BaseNotificationHandler<MigrationFailureNotificationEvent>
{
    public MigrationFailureNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(MigrationFailureNotificationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Migration Failure",
            $"Database migration failed: {e.ErrorMessage}",
            NotificationCategory.System,
            NotificationSeverity.Critical,
            "System",
            cancellationToken: cancellationToken);
    }
}