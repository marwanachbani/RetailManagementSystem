using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Customers.Application.DeactivateCustomer;

public sealed record DeactivateCustomerCommand(Guid CustomerId) : IRequest<Result>;
