using RMS.BuildingBlocks.Domain;

namespace RMS.Modules.Products.Domain.Events;

public sealed record ProductCreatedEvent(
    Guid ProductId,
    string ProductCode,
    string Name,
    Guid CategoryId,
    decimal SalePrice,
    decimal CostPrice) : DomainEvent;

public sealed record ProductUpdatedEvent(
    Guid ProductId,
    string ProductCode,
    string Name,
    Guid CategoryId,
    decimal SalePrice,
    decimal CostPrice) : DomainEvent;

public sealed record ProductDeactivatedEvent(
    Guid ProductId,
    string ProductCode) : DomainEvent;
