using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Suppliers.Application.ReactivateSupplier;

public sealed record ReactivateSupplierCommand(Guid SupplierId) : IRequest<Result>;
