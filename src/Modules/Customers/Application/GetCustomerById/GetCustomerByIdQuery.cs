using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.Contracts;

namespace RMS.Modules.Customers.Application.GetCustomerById;

public sealed record GetCustomerByIdQuery(Guid CustomerId) : IRequest<Result<CustomerReadModel>>;
