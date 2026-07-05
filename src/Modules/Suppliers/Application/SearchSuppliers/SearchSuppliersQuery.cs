using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Suppliers.Application.Contracts;

namespace RMS.Modules.Suppliers.Application.SearchSuppliers;

public sealed record SearchSuppliersQuery(string? SearchTerm, bool IncludeInactive = false) : IRequest<Result<IReadOnlyList<SupplierReadModel>>>;
