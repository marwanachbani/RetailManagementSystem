using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.Contracts;

namespace RMS.Modules.Customers.Application.GetCustomerStatistics;

public sealed record GetCustomerStatisticsQuery(Guid CustomerId) : IRequest<Result<CustomerStatistics>>;
