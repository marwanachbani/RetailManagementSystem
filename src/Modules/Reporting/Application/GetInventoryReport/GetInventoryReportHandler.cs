using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetInventoryReport;

public sealed class GetInventoryReportHandler : IRequestHandler<GetInventoryReportQuery, Result<InventoryReportResult>>
{
    private readonly IReportingReadStore _readStore;

    public GetInventoryReportHandler(IReportingReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<InventoryReportResult>> Handle(GetInventoryReportQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetInventoryReportAsync(request.SearchTerm, cancellationToken);
        return Result.Success(result);
    }
}
