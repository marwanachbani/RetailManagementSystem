using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Purchasing.Application.Contracts;

namespace RMS.Modules.Purchasing.Application.GetPurchaseOrder;

public sealed record GetPurchaseOrderQuery(Guid PurchaseOrderId) : IRequest<Result<PurchaseOrderReadModel>>;
