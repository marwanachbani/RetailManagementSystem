using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Domain.Entities;

namespace RMS.Modules.Inventory.Application.DecreaseStock;

public sealed class DecreaseStockHandler : IRequestHandler<DecreaseStockCommand, Result>
{
    private readonly IInventoryReadStore _readStore;
    private readonly IInventoryWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public DecreaseStockHandler(IInventoryReadStore readStore, IInventoryWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DecreaseStockCommand request, CancellationToken cancellationToken)
    {
        var readModel = await _readStore.GetByIdAsync(request.InventoryItemId, cancellationToken);
        if (readModel is null)
            return Result.Failure("Inventory item not found.", "Inventory.NotFound");

        var item = InventoryItem.Rehydrate(
            readModel.Id, readModel.ProductId, readModel.CurrentQuantity,
            readModel.IsActive, readModel.CreatedAt, readModel.UpdatedAt, readModel.LowStockThreshold);

        try
        {
            item.DecreaseStock(request.Amount, request.Reason, request.UserId);
        }
        catch (Exception ex) when (ex.Message.Contains("Insufficient stock"))
        {
            return Result.Failure(ex.Message, "Inventory.InsufficientStock");
        }

        await _writeStore.UpdateAsync(item, cancellationToken);
        await _eventBus.PublishAsync(new StockChangedIntegrationEvent(item.Id, item.ProductId, item.CurrentQuantity.Value, "Decrease"), cancellationToken);
        item.ClearDomainEvents();
        return Result.Success();
    }
}
