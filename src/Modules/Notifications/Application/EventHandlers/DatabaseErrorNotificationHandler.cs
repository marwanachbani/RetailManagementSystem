using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Application.IntegrationEvents;
using RMS.Modules.Notifications.Domain;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class DatabaseErrorNotificationHandler : BaseNotificationHandler<DatabaseErrorNotificationEvent>
{
    public DatabaseErrorNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(DatabaseErrorNotificationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Database Error",
            $"A database error occurred: {e.ErrorMessage}",
            NotificationCategory.System,
            NotificationSeverity.Error,
            "System",
            cancellationToken: cancellationToken);
    }
}