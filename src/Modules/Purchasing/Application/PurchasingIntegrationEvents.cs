using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.EventBus;

namespace RMS.Modules.Purchasing.Application;

public sealed record StockIncreaseRequestedEvent(
    Guid PurchaseOrderId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    string Reason) : DomainEvent, IIntegrationEvent;

public sealed record PurchaseOrderCreatedIntegrationEvent(
    Guid PurchaseOrderId,
    string PurchaseNumber,
    Guid SupplierId,
    DateTime OrderDate) : DomainEvent, IIntegrationEvent;

public sealed record PurchaseOrderUpdatedIntegrationEvent(
    Guid PurchaseOrderId,
    string PurchaseNumber,
    Guid SupplierId) : DomainEvent, IIntegrationEvent;

public sealed record PurchaseOrderCancelledIntegrationEvent(
    Guid PurchaseOrderId,
    string PurchaseNumber,
    DateTime CancelledAt) : DomainEvent, IIntegrationEvent;

public sealed record GoodsReceivedIntegrationEvent(
    Guid PurchaseOrderId,
    string PurchaseNumber,
    Guid ProductId,
    string ProductName,
    int QuantityReceived) : DomainEvent, IIntegrationEvent;

public sealed record PurchaseCompletedIntegrationEvent(
    Guid PurchaseOrderId,
    string PurchaseNumber,
    decimal TotalAmount,
    DateTime CompletedAt) : DomainEvent, IIntegrationEvent;
