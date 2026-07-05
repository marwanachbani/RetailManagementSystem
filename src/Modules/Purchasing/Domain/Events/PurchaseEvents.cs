using RMS.BuildingBlocks.Domain;

namespace RMS.Modules.Purchasing.Domain.Events;

public sealed record PurchaseOrderCreatedEvent(
    Guid PurchaseOrderId,
    string PurchaseNumber,
    Guid SupplierId,
    DateTime OrderDate) : DomainEvent;

public sealed record PurchaseOrderUpdatedEvent(
    Guid PurchaseOrderId,
    string PurchaseNumber,
    Guid SupplierId,
    DateTime OrderDate) : DomainEvent;

public sealed record PurchaseOrderCancelledEvent(
    Guid PurchaseOrderId,
    string PurchaseNumber,
    DateTime CancelledAt) : DomainEvent;

public sealed record GoodsReceivedEvent(
    Guid PurchaseOrderId,
    string PurchaseNumber,
    Guid ProductId,
    string ProductName,
    int QuantityReceived,
    int TotalReceived,
    int OrderedQuantity) : DomainEvent;

public sealed record PurchaseCompletedEvent(
    Guid PurchaseOrderId,
    string PurchaseNumber,
    decimal TotalAmount,
    DateTime CompletedAt) : DomainEvent;
