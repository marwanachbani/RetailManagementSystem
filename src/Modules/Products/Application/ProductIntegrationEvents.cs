using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.EventBus;

namespace RMS.Modules.Products.Application;

public sealed record ProductCreatedIntegrationEvent(Guid ProductId, string ProductCode, string Name) : DomainEvent, IIntegrationEvent;
public sealed record ProductUpdatedIntegrationEvent(Guid ProductId, string ProductCode, string Name) : DomainEvent, IIntegrationEvent;
public sealed record ProductDeactivatedIntegrationEvent(Guid ProductId, string ProductCode) : DomainEvent, IIntegrationEvent;
