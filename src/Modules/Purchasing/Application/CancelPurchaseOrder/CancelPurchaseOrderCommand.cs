using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Purchasing.Application.CancelPurchaseOrder;

public sealed record CancelPurchaseOrderCommand(Guid PurchaseOrderId) : IRequest<Result>;
