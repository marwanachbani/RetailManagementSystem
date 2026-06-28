using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.Contracts;

namespace RMS.Modules.Customers.Application.GetCustomerStatistics;

public sealed class GetCustomerStatisticsHandler : IRequestHandler<GetCustomerStatisticsQuery, Result<CustomerStatistics>>
{
    private readonly ICustomerReadStore _readStore;

    public GetCustomerStatisticsHandler(ICustomerReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<CustomerStatistics>> Handle(GetCustomerStatisticsQuery request, CancellationToken cancellationToken)
    {
        var statistics = await _readStore.GetStatisticsAsync(request.CustomerId, cancellationToken);
        if (statistics is null)
            return Result.Failure<CustomerStatistics>("Customer statistics not found.", "Customers.NotFound");

        return Result.Success(statistics);
    }
}
