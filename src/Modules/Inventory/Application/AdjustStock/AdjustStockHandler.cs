using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Domain.Entities;

namespace RMS.Modules.Inventory.Application.AdjustStock;

public sealed class AdjustStockHandler : IRequestHandler<AdjustStockCommand, Result>
{
    private readonly IInventoryReadStore _readStore;
    private readonly IInventoryWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public AdjustStockHandler(IInventoryReadStore readStore, IInventoryWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var readModel = await _readStore.GetByIdAsync(request.InventoryItemId, cancellationToken);
        if (readModel is null)
            return Result.Failure("Inventory item not found.", "Inventory.NotFound");

        var item = InventoryItem.Rehydrate(
            readModel.Id, readModel.ProductId, readModel.CurrentQuantity,
            readModel.IsActive, readModel.CreatedAt, readModel.UpdatedAt, readModel.LowStockThreshold);

        item.AdjustStock(request.NewQuantity, request.Reason, request.UserId);

        await _writeStore.UpdateAsync(item, cancellationToken);
        await _eventBus.PublishAsync(new StockChangedIntegrationEvent(item.Id, item.ProductId, item.CurrentQuantity.Value, "Adjust"), cancellationToken);
        item.ClearDomainEvents();
        return Result.Success();
    }
}
