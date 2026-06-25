using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Inventory.Application.Contracts;

namespace RMS.Modules.Inventory.Application.GetLowStockItems;

public sealed class GetLowStockItemsHandler : IRequestHandler<GetLowStockItemsQuery, Result<IReadOnlyList<InventoryItemReadModel>>>
{
    private readonly IInventoryReadStore _readStore;

    public GetLowStockItemsHandler(IInventoryReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<IReadOnlyList<InventoryItemReadModel>>> Handle(GetLowStockItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _readStore.GetLowStockItemsAsync(request.Threshold, cancellationToken);
        return Result.Success(items);
    }
}
