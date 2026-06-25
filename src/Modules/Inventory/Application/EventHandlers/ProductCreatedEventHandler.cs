using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Domain.Entities;
using RMS.Modules.Products.Application;

namespace RMS.Modules.Inventory.Application.EventHandlers;

/// <summary>
/// Automatically creates an inventory item when a product is created.
/// This handler bridges the Products and Inventory modules via the in-process event bus.
/// </summary>
public sealed class ProductCreatedEventHandler : IIntegrationEventHandler<ProductCreatedIntegrationEvent>
{
    private readonly IInventoryReadStore _readStore;
    private readonly IInventoryWriteStore _writeStore;

    public ProductCreatedEventHandler(IInventoryReadStore readStore, IInventoryWriteStore writeStore)
    {
        _readStore = readStore;
        _writeStore = writeStore;
    }

    public async Task HandleAsync(ProductCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var existing = await _readStore.GetByProductIdAsync(integrationEvent.ProductId, cancellationToken);
        if (existing is not null)
            return;

        var item = InventoryItem.Create(Guid.NewGuid(), integrationEvent.ProductId, 0, 10);
        await _writeStore.InsertAsync(item, cancellationToken);
    }
}
