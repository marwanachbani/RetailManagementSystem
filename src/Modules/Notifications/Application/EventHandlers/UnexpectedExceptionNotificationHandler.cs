using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Application.IntegrationEvents;
using RMS.Modules.Notifications.Domain;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class UnexpectedExceptionNotificationHandler : BaseNotificationHandler<UnexpectedExceptionNotificationEvent>
{
    public UnexpectedExceptionNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(UnexpectedExceptionNotificationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Unexpected Exception",
            $"An unexpected exception occurred in {e.Source}: {e.ErrorMessage}",
            NotificationCategory.System,
            NotificationSeverity.Error,
            "System",
            cancellationToken: cancellationToken);
    }
}