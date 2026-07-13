using RMS.BuildingBlocks.Contracts;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public abstract class BaseNotificationHandler<TEvent> : IIntegrationEventHandler<TEvent>
    where TEvent : IIntegrationEvent
{
    protected readonly INotificationRepository Repository;
    protected readonly ICurrentUserContext CurrentUserContext;

    protected BaseNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
    {
        Repository = repository;
        CurrentUserContext = currentUserContext;
    }

    protected async Task CreateNotificationAsync(
        string title,
        string message,
        NotificationCategory category,
        NotificationSeverity severity,
        string relatedModule,
        Guid? relatedEntityId = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification(
            Guid.NewGuid(),
            title,
            message,
            category,
            severity,
            relatedModule,
            relatedEntityId,
            CurrentUserContext.UserId);

        await Repository.AddAsync(notification, cancellationToken);
    }

    public abstract Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);
}