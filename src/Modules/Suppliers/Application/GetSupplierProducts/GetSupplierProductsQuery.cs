using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Suppliers.Application.Contracts;

namespace RMS.Modules.Suppliers.Application.GetSupplierProducts;

public sealed record GetSupplierProductsQuery(Guid SupplierId) : IRequest<Result<IReadOnlyList<SupplierProductReadModel>>>;
