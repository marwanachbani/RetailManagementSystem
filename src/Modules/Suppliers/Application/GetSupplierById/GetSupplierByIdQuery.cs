using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Suppliers.Application.Contracts;

namespace RMS.Modules.Suppliers.Application.GetSupplierById;

public sealed record GetSupplierByIdQuery(Guid SupplierId) : IRequest<Result<SupplierReadModel>>;
