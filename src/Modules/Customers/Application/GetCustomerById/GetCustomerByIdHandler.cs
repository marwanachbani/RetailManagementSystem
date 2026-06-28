using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.Contracts;

namespace RMS.Modules.Customers.Application.GetCustomerById;

public sealed class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerReadModel>>
{
    private readonly ICustomerReadStore _readStore;

    public GetCustomerByIdHandler(ICustomerReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<CustomerReadModel>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _readStore.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
            return Result.Failure<CustomerReadModel>("Customer not found.", "Customers.NotFound");

        return Result.Success(customer);
    }
}
