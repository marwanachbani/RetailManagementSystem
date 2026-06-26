using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Domain.Entities;
using RMS.Modules.Sales.Application;

namespace RMS.Modules.Inventory.Application.EventHandlers;

public sealed class StockRestorationRequestedHandler : IIntegrationEventHandler<StockRestorationRequestedEvent>
{
    private readonly IInventoryReadStore _readStore;
    private readonly IInventoryWriteStore _writeStore;

    public StockRestorationRequestedHandler(IInventoryReadStore readStore, IInventoryWriteStore writeStore)
    {
        _readStore = readStore;
        _writeStore = writeStore;
    }

    public async Task HandleAsync(StockRestorationRequestedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var inventoryItem = await _readStore.GetByProductIdAsync(integrationEvent.ProductId, cancellationToken);
        if (inventoryItem is null)
            return;

        var item = InventoryItem.Rehydrate(
            inventoryItem.Id, inventoryItem.ProductId, inventoryItem.CurrentQuantity,
            inventoryItem.IsActive, inventoryItem.CreatedAt, inventoryItem.UpdatedAt, inventoryItem.LowStockThreshold);

        item.IncreaseStock(integrationEvent.Quantity, integrationEvent.Reason);
        await _writeStore.UpdateAsync(item, cancellationToken);
    }
}
