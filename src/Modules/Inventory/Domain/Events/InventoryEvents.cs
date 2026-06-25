using RMS.BuildingBlocks.Domain;

namespace RMS.Modules.Inventory.Domain.Events;

public sealed record InventoryCreatedEvent(
    Guid InventoryItemId,
    Guid ProductId,
    int InitialQuantity) : DomainEvent;

public sealed record StockIncreasedEvent(
    Guid InventoryItemId,
    Guid ProductId,
    int QuantityBefore,
    int QuantityAfter,
    int ChangeAmount,
    string Reason) : DomainEvent;

public sealed record StockDecreasedEvent(
    Guid InventoryItemId,
    Guid ProductId,
    int QuantityBefore,
    int QuantityAfter,
    int ChangeAmount,
    string Reason) : DomainEvent;

public sealed record StockAdjustedEvent(
    Guid InventoryItemId,
    Guid ProductId,
    int QuantityBefore,
    int QuantityAfter,
    int ChangeAmount,
    string Reason) : DomainEvent;

public sealed record LowStockDetectedEvent(
    Guid InventoryItemId,
    Guid ProductId,
    int CurrentQuantity,
    int LowStockThreshold) : DomainEvent;
