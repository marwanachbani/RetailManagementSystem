using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetProductReport;

public sealed class GetProductReportHandler : IRequestHandler<GetProductReportQuery, Result<ProductReportResult>>
{
    private readonly IReportingReadStore _readStore;

    public GetProductReportHandler(IReportingReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<ProductReportResult>> Handle(GetProductReportQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetProductReportAsync(request.SearchTerm, request.SortColumn, request.SortDescending, cancellationToken);
        return Result.Success(result);
    }
}
