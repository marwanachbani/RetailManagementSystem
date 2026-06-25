using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.EventBus;

namespace RMS.Modules.Inventory.Application;

public sealed record InventoryItemCreatedIntegrationEvent(
    Guid InventoryItemId,
    Guid ProductId,
    int InitialQuantity) : DomainEvent, IIntegrationEvent;

public sealed record StockChangedIntegrationEvent(
    Guid InventoryItemId,
    Guid ProductId,
    int NewQuantity,
    string ChangeType) : DomainEvent, IIntegrationEvent;

public sealed record LowStockIntegrationEvent(
    Guid InventoryItemId,
    Guid ProductId,
    int CurrentQuantity,
    int LowStockThreshold) : DomainEvent, IIntegrationEvent;
