using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RMS.BuildingBlocks.Domain;

namespace RMS.BuildingBlocks.EventBus;

/// <summary>
/// Marker for events that cross module boundaries (as opposed to IDomainEvent,
/// which stays inside the aggregate's own module until translated here).
/// </summary>
public interface IIntegrationEvent : IDomainEvent
{
}

public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);
}

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}

/// <summary>
/// Simple in-process event bus. Resolves all registered handlers for the event
/// type from the DI container and invokes them sequentially. Each module
/// registers its own handlers via its module-registration extension method;
/// no module references another module's handler types directly.
/// </summary>
public sealed class InProcessEventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InProcessEventBus> _logger;

    public InProcessEventBus(IServiceProvider serviceProvider, ILogger<InProcessEventBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IIntegrationEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            try
            {
                await handler.HandleAsync(integrationEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                // A failing subscriber must never break the publishing module's
                // own transaction/flow — log and continue with the next handler.
                _logger.LogError(ex,
                    "Integration event handler {Handler} failed while handling {Event} ({EventId})",
                    handler.GetType().Name, typeof(TEvent).Name, integrationEvent.EventId);
            }
        }
    }
}
