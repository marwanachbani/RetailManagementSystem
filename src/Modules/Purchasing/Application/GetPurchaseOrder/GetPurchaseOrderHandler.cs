using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Purchasing.Application.Contracts;

namespace RMS.Modules.Purchasing.Application.GetPurchaseOrder;

public sealed class GetPurchaseOrderHandler : IRequestHandler<GetPurchaseOrderQuery, Result<PurchaseOrderReadModel>>
{
    private readonly IPurchaseOrderReadStore _readStore;

    public GetPurchaseOrderHandler(IPurchaseOrderReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<PurchaseOrderReadModel>> Handle(GetPurchaseOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _readStore.GetByIdAsync(request.PurchaseOrderId, cancellationToken);
        if (order is null)
            return Result.Failure<PurchaseOrderReadModel>("PurchaseOrder.NotFound", "Purchase order not found.");

        var items = await _readStore.GetItemsByPurchaseOrderIdAsync(request.PurchaseOrderId, cancellationToken);
        var receipts = await _readStore.GetGoodsReceiptsByPurchaseOrderIdAsync(request.PurchaseOrderId, cancellationToken);

        return Result.Success(order with { Items = items, GoodsReceipts = receipts });
    }
}
