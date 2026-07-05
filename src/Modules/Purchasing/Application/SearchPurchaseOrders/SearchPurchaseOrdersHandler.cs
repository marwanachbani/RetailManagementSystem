using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Purchasing.Application.Contracts;

namespace RMS.Modules.Purchasing.Application.SearchPurchaseOrders;

public sealed class SearchPurchaseOrdersHandler : IRequestHandler<SearchPurchaseOrdersQuery, Result<IReadOnlyList<PurchaseOrderReadModel>>>
{
    private readonly IPurchaseOrderReadStore _readStore;

    public SearchPurchaseOrdersHandler(IPurchaseOrderReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<IReadOnlyList<PurchaseOrderReadModel>>> Handle(SearchPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.SearchAsync(request.SearchTerm, request.StatusFilter, cancellationToken);
        return Result.Success(result);
    }
}
