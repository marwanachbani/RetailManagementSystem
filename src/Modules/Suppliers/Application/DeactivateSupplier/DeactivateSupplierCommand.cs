using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Suppliers.Application.DeactivateSupplier;

public sealed record DeactivateSupplierCommand(Guid SupplierId) : IRequest<Result>;
