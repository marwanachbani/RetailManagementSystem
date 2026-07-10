using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetFinancialReport;

public sealed class GetFinancialReportHandler : IRequestHandler<GetFinancialReportQuery, Result<FinancialReportResult>>
{
    private readonly IReportingReadStore _readStore;

    public GetFinancialReportHandler(IReportingReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<FinancialReportResult>> Handle(GetFinancialReportQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetFinancialReportAsync(request.DateRange, request.PeriodType, cancellationToken);
        return Result.Success(result);
    }
}
