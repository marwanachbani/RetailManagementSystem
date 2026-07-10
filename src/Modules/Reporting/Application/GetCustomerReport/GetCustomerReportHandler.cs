using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetCustomerReport;

public sealed class GetCustomerReportHandler : IRequestHandler<GetCustomerReportQuery, Result<CustomerReportResult>>
{
    private readonly IReportingReadStore _readStore;

    public GetCustomerReportHandler(IReportingReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<CustomerReportResult>> Handle(GetCustomerReportQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetCustomerReportAsync(request.SearchTerm, request.IncludeInactive, cancellationToken);
        return Result.Success(result);
    }
}
