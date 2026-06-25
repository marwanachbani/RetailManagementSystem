using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Inventory.Application.Contracts;

namespace RMS.Modules.Inventory.Application.GetInventoryItem;

public sealed class GetInventoryItemHandler : IRequestHandler<GetInventoryItemQuery, Result<InventoryItemReadModel>>
{
    private readonly IInventoryReadStore _readStore;

    public GetInventoryItemHandler(IInventoryReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<InventoryItemReadModel>> Handle(GetInventoryItemQuery request, CancellationToken cancellationToken)
    {
        var item = await _readStore.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
            return Result.Failure<InventoryItemReadModel>("Inventory item not found.", "Inventory.NotFound");

        return Result.Success(item);
    }
}
