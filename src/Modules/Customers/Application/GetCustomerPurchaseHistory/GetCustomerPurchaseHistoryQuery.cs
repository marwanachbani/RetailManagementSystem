using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.Contracts;

namespace RMS.Modules.Customers.Application.GetCustomerPurchaseHistory;

public sealed record GetCustomerPurchaseHistoryQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<CustomerPurchaseHistoryItem>>>;
