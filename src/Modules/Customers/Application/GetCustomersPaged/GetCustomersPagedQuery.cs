using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.Contracts;

namespace RMS.Modules.Customers.Application.GetCustomersPaged;

public sealed record GetCustomersPagedQuery(
    int PageNumber,
    int PageSize,
    string? SearchTerm = null,
    bool IncludeInactive = false) : IRequest<Result<PagedResult<CustomerReadModel>>>;
