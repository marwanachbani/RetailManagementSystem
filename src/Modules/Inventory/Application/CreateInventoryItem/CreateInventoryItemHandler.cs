using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Domain.Entities;

namespace RMS.Modules.Inventory.Application.CreateInventoryItem;

public sealed class CreateInventoryItemHandler : IRequestHandler<CreateInventoryItemCommand, Result<Guid>>
{
    private readonly IInventoryReadStore _readStore;
    private readonly IInventoryWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public CreateInventoryItemHandler(IInventoryReadStore readStore, IInventoryWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result<Guid>> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var existing = await _readStore.GetByProductIdAsync(request.ProductId, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>("An inventory item already exists for this product.", "Inventory.AlreadyExists");

        var item = InventoryItem.Create(Guid.NewGuid(), request.ProductId, request.InitialQuantity, request.LowStockThreshold);

        await _writeStore.InsertAsync(item, cancellationToken);
        await _eventBus.PublishAsync(new InventoryItemCreatedIntegrationEvent(item.Id, item.ProductId, item.CurrentQuantity.Value), cancellationToken);
        item.ClearDomainEvents();
        return Result.Success(item.Id);
    }
}
