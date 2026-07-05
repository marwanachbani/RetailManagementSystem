using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Purchasing.Application.CompletePurchase;

public sealed record CompletePurchaseCommand(Guid PurchaseOrderId) : IRequest<Result>;
