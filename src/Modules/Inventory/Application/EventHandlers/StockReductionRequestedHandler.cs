using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Domain.Entities;
using RMS.Modules.Sales.Application;

namespace RMS.Modules.Inventory.Application.EventHandlers;

public sealed class StockReductionRequestedHandler : IIntegrationEventHandler<StockReductionRequestedEvent>
{
    private readonly IInventoryReadStore _readStore;
    private readonly IInventoryWriteStore _writeStore;

    public StockReductionRequestedHandler(IInventoryReadStore readStore, IInventoryWriteStore writeStore)
    {
        _readStore = readStore;
        _writeStore = writeStore;
    }

    public async Task HandleAsync(StockReductionRequestedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var inventoryItem = await _readStore.GetByProductIdAsync(integrationEvent.ProductId, cancellationToken);
        if (inventoryItem is null)
            return;

        var item = InventoryItem.Rehydrate(
            inventoryItem.Id, inventoryItem.ProductId, inventoryItem.CurrentQuantity,
            inventoryItem.IsActive, inventoryItem.CreatedAt, inventoryItem.UpdatedAt, inventoryItem.LowStockThreshold);

        try
        {
            item.DecreaseStock(integrationEvent.Quantity, integrationEvent.Reason);
            await _writeStore.UpdateAsync(item, cancellationToken);
        }
        catch
        {
            // Stock reduction failures should not break the sale transaction.
            // In production, this should be logged and potentially retried.
        }
    }
}
