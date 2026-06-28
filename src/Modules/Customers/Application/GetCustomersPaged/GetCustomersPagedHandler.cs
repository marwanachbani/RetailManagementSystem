using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.Contracts;

namespace RMS.Modules.Customers.Application.GetCustomersPaged;

public sealed class GetCustomersPagedHandler : IRequestHandler<GetCustomersPagedQuery, Result<PagedResult<CustomerReadModel>>>
{
    private readonly ICustomerReadStore _readStore;

    public GetCustomersPagedHandler(ICustomerReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<PagedResult<CustomerReadModel>>> Handle(GetCustomersPagedQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetPagedAsync(
            request.PageNumber, request.PageSize, request.SearchTerm, request.IncludeInactive, cancellationToken);
        return Result.Success(result);
    }
}
