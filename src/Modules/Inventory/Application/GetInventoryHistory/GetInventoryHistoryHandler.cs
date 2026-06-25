using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Inventory.Application.Contracts;

namespace RMS.Modules.Inventory.Application.GetInventoryHistory;

public sealed class GetInventoryHistoryHandler : IRequestHandler<GetInventoryHistoryQuery, Result<IReadOnlyList<InventoryTransactionReadModel>>>
{
    private readonly IInventoryReadStore _readStore;

    public GetInventoryHistoryHandler(IInventoryReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<IReadOnlyList<InventoryTransactionReadModel>>> Handle(GetInventoryHistoryQuery request, CancellationToken cancellationToken)
    {
        var history = await _readStore.GetHistoryAsync(request.InventoryItemId, cancellationToken);
        return Result.Success(history);
    }
}
