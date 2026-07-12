using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.EventBus;

namespace RMS.Modules.Identity.Application.IntegrationEvents;

public sealed record LoginSucceededIntegrationEvent(Guid UserId, string UserName) : DomainEvent, IIntegrationEvent;
public sealed record LoginFailedIntegrationEvent(string UserName, string Reason) : DomainEvent, IIntegrationEvent;
public sealed record LogoutSucceededIntegrationEvent(Guid UserId, string UserName) : DomainEvent, IIntegrationEvent;
