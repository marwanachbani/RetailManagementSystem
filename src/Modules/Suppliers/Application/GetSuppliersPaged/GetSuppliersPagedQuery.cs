using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Suppliers.Application.Contracts;

namespace RMS.Modules.Suppliers.Application.GetSuppliersPaged;

public sealed record GetSuppliersPagedQuery(int PageNumber, int PageSize, string? SearchTerm, bool IncludeInactive = false)
    : IRequest<Result<PagedResult<SupplierReadModel>>>;
