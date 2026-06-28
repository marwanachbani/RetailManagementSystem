using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.Contracts;

namespace RMS.Modules.Customers.Application.SearchCustomers;

public sealed class SearchCustomersHandler : IRequestHandler<SearchCustomersQuery, Result<IReadOnlyList<CustomerReadModel>>>
{
    private readonly ICustomerReadStore _readStore;

    public SearchCustomersHandler(ICustomerReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<IReadOnlyList<CustomerReadModel>>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _readStore.SearchAsync(request.SearchTerm, request.IncludeInactive, cancellationToken);
        return Result.Success(customers);
    }
}
