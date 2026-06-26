using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Sales.Application.Contracts;

namespace RMS.Modules.Sales.Application.GetDailySalesSummary;

public sealed class GetDailySalesSummaryHandler : IRequestHandler<GetDailySalesSummaryQuery, Result<DailySalesSummary>>
{
    private readonly ISaleReadStore _readStore;

    public GetDailySalesSummaryHandler(ISaleReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<DailySalesSummary>> Handle(GetDailySalesSummaryQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetDailySummaryAsync(request.Date, cancellationToken);
        return Result.Success(result);
    }
}
