using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetPurchaseReport;

public sealed class GetPurchaseReportHandler : IRequestHandler<GetPurchaseReportQuery, Result<PurchaseReportResult>>
{
    private readonly IReportingReadStore _readStore;

    public GetPurchaseReportHandler(IReportingReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<PurchaseReportResult>> Handle(GetPurchaseReportQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetPurchaseReportAsync(request.DateRange, request.SupplierId, request.SearchTerm, request.SortColumn, request.SortDescending, cancellationToken);
        return Result.Success(result);
    }
}
