using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetSalesReport;

public sealed class GetSalesReportHandler : IRequestHandler<GetSalesReportQuery, Result<SalesReportResult>>
{
    private readonly IReportingReadStore _readStore;

    public GetSalesReportHandler(IReportingReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<SalesReportResult>> Handle(GetSalesReportQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetSalesReportAsync(request.DateRange, request.SearchTerm, request.SortColumn, request.SortDescending, cancellationToken);
        return Result.Success(result);
    }
}
