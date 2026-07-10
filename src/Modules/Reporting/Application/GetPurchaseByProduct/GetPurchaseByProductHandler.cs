using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetPurchaseByProduct;

public sealed class GetPurchaseByProductHandler : IRequestHandler<GetPurchaseByProductQuery, Result<PurchaseByProductResult>>
{
    private readonly IReportingReadStore _readStore;

    public GetPurchaseByProductHandler(IReportingReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<PurchaseByProductResult>> Handle(GetPurchaseByProductQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetPurchaseByProductAsync(request.DateRange, request.SearchTerm, cancellationToken);
        return Result.Success(result);
    }
}
