using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Purchasing.Application.ReceiveGoods;

public sealed record ReceiveGoodsCommand(
    Guid PurchaseOrderId,
    Guid ProductId,
    int QuantityReceived,
    string? BatchNumber,
    DateTime? ExpiryDate) : IRequest<Result>;
