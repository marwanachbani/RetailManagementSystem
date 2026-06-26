using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Sales.Application.Contracts;

namespace RMS.Modules.Sales.Application.GetSalesPaged;

public sealed class GetSalesPagedHandler : IRequestHandler<GetSalesPagedQuery, Result<PagedResult<SaleReadModel>>>
{
    private readonly ISaleReadStore _readStore;

    public GetSalesPagedHandler(ISaleReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<PagedResult<SaleReadModel>>> Handle(GetSalesPagedQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetPagedAsync(
            request.PageNumber, request.PageSize, request.FromDate, request.ToDate, cancellationToken);
        return Result.Success(result);
    }
}
