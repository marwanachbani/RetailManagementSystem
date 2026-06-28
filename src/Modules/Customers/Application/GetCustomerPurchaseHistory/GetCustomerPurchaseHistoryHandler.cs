using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.Contracts;

namespace RMS.Modules.Customers.Application.GetCustomerPurchaseHistory;

public sealed class GetCustomerPurchaseHistoryHandler : IRequestHandler<GetCustomerPurchaseHistoryQuery, Result<IReadOnlyList<CustomerPurchaseHistoryItem>>>
{
    private readonly ICustomerReadStore _readStore;

    public GetCustomerPurchaseHistoryHandler(ICustomerReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<IReadOnlyList<CustomerPurchaseHistoryItem>>> Handle(GetCustomerPurchaseHistoryQuery request, CancellationToken cancellationToken)
    {
        var history = await _readStore.GetPurchaseHistoryAsync(request.CustomerId, cancellationToken);
        return Result.Success(history);
    }
}
