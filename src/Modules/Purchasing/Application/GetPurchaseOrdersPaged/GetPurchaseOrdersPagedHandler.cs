using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Purchasing.Application.Contracts;

namespace RMS.Modules.Purchasing.Application.GetPurchaseOrdersPaged;

public sealed class GetPurchaseOrdersPagedHandler : IRequestHandler<GetPurchaseOrdersPagedQuery, Result<PagedResult<PurchaseOrderReadModel>>>
{
    private readonly IPurchaseOrderReadStore _readStore;

    public GetPurchaseOrdersPagedHandler(IPurchaseOrderReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<PagedResult<PurchaseOrderReadModel>>> Handle(GetPurchaseOrdersPagedQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetPagedAsync(
            request.PageNumber, request.PageSize, request.SearchTerm, request.StatusFilter, cancellationToken);
        return Result.Success(result);
    }
}
