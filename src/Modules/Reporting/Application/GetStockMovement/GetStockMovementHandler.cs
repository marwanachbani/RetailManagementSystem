using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetStockMovement;

public sealed class GetStockMovementHandler : IRequestHandler<GetStockMovementQuery, Result<StockMovementResult>>
{
    private readonly IReportingReadStore _readStore;

    public GetStockMovementHandler(IReportingReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<StockMovementResult>> Handle(GetStockMovementQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetStockMovementAsync(request.DateRange, request.SearchTerm, cancellationToken);
        return Result.Success(result);
    }
}
