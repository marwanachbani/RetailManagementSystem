using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Customers.Application.ReactivateCustomer;

public sealed record ReactivateCustomerCommand(Guid CustomerId) : IRequest<Result>;
