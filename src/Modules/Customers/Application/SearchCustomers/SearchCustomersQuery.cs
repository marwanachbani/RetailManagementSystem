using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.Contracts;

namespace RMS.Modules.Customers.Application.SearchCustomers;

public sealed record SearchCustomersQuery(
    string? SearchTerm,
    bool IncludeInactive = false) : IRequest<Result<IReadOnlyList<CustomerReadModel>>>;
