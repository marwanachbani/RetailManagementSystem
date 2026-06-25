using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Inventory.Application.Contracts;

namespace RMS.Modules.Inventory.Application.GetInventoryPaged;

public sealed class GetInventoryPagedHandler : IRequestHandler<GetInventoryPagedQuery, Result<PagedResult<InventoryItemReadModel>>>
{
    private readonly IInventoryReadStore _readStore;

    public GetInventoryPagedHandler(IInventoryReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<PagedResult<InventoryItemReadModel>>> Handle(GetInventoryPagedQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetPagedAsync(
            request.PageNumber, request.PageSize, request.SearchTerm, request.IncludeInactive, cancellationToken);
        return Result.Success(result);
    }
}
