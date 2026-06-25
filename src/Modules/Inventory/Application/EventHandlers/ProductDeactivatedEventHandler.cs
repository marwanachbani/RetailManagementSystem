using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Domain.Entities;
using RMS.Modules.Products.Application;

namespace RMS.Modules.Inventory.Application.EventHandlers;

/// <summary>
/// Deactivates the inventory item when a product is deactivated,
/// preventing future stock operations.
/// </summary>
public sealed class ProductDeactivatedEventHandler : IIntegrationEventHandler<ProductDeactivatedIntegrationEvent>
{
    private readonly IInventoryReadStore _readStore;
    private readonly IInventoryWriteStore _writeStore;

    public ProductDeactivatedEventHandler(IInventoryReadStore readStore, IInventoryWriteStore writeStore)
    {
        _readStore = readStore;
        _writeStore = writeStore;
    }

    public async Task HandleAsync(ProductDeactivatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var existing = await _readStore.GetByProductIdAsync(integrationEvent.ProductId, cancellationToken);
        if (existing is null)
            return;

        var item = InventoryItem.Rehydrate(
            existing.Id, existing.ProductId, existing.CurrentQuantity,
            existing.IsActive, existing.CreatedAt, existing.UpdatedAt, existing.LowStockThreshold);

        item.Deactivate();
        await _writeStore.UpdateAsync(item, cancellationToken);
    }
}
