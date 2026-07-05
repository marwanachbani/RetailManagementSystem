using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Purchasing.Application.Contracts;
using RMS.Modules.Purchasing.Domain.Entities;

namespace RMS.Modules.Purchasing.Application.ReceiveGoods;

public sealed class ReceiveGoodsHandler : IRequestHandler<ReceiveGoodsCommand, Result>
{
    private readonly IPurchaseOrderReadStore _readStore;
    private readonly IPurchaseOrderWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public ReceiveGoodsHandler(IPurchaseOrderReadStore readStore, IPurchaseOrderWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ReceiveGoodsCommand request, CancellationToken cancellationToken)
    {
        var readModel = await _readStore.GetByIdAsync(request.PurchaseOrderId, cancellationToken);
        if (readModel is null)
            return Result.Failure("PurchaseOrder.NotFound", "Purchase order not found.");

        var items = await _readStore.GetItemsByPurchaseOrderIdAsync(request.PurchaseOrderId, cancellationToken);
        var receipts = await _readStore.GetGoodsReceiptsByPurchaseOrderIdAsync(request.PurchaseOrderId, cancellationToken);

        var order = PurchaseOrder.Rehydrate(
            readModel.Id, readModel.PurchaseNumber, readModel.SupplierId, readModel.SupplierName,
            readModel.OrderDate, Enum.Parse<PurchaseStatus>(readModel.Status),
            readModel.SubTotal, readModel.TaxAmount, readModel.TotalAmount, readModel.TaxPercentage,
            readModel.CompletedAt, readModel.CancelledAt, readModel.CreatedAt, readModel.Notes, readModel.SupplierInvoiceNumber);

        order.RehydrateItems(items.Select(i => PurchaseOrderItem.Rehydrate(
            i.Id, i.PurchaseOrderId, i.ProductId, i.ProductName, i.Quantity, i.UnitCost, i.ReceivedQuantity)).ToList());

        order.RehydrateGoodsReceipts(receipts.Select(r => GoodsReceipt.Rehydrate(
            r.Id, r.PurchaseOrderId, r.ProductId, r.QuantityReceived, r.ReceivedAt, r.BatchNumber, r.ExpiryDate)).ToList());

        order.ReceiveGoods(Guid.NewGuid(), request.ProductId, request.QuantityReceived, request.BatchNumber, request.ExpiryDate);

        var receipt = order.GoodsReceipts.Last();
        await _writeStore.InsertGoodsReceiptAsync(receipt, cancellationToken);
        await _writeStore.UpdateAsync(order, cancellationToken);

        await _eventBus.PublishAsync(new GoodsReceivedIntegrationEvent(
            order.Id, order.PurchaseNumber, request.ProductId,
            items.First(i => i.ProductId == request.ProductId).ProductName, request.QuantityReceived), cancellationToken);

        await _eventBus.PublishAsync(new StockIncreaseRequestedEvent(
            order.Id, request.ProductId,
            items.First(i => i.ProductId == request.ProductId).ProductName,
            request.QuantityReceived, $"Goods received for PO {order.PurchaseNumber}"), cancellationToken);

        order.ClearDomainEvents();
        return Result.Success();
    }
}
