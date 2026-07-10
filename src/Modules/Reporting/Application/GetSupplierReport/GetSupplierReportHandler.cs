using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetSupplierReport;

public sealed class GetSupplierReportHandler : IRequestHandler<GetSupplierReportQuery, Result<SupplierReportResult>>
{
    private readonly IReportingReadStore _readStore;

    public GetSupplierReportHandler(IReportingReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<SupplierReportResult>> Handle(GetSupplierReportQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetSupplierReportAsync(request.SearchTerm, request.IncludeInactive, cancellationToken);
        return Result.Success(result);
    }
}
