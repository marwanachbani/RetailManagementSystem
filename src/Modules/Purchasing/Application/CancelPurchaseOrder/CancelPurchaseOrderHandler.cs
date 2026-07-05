using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Purchasing.Application.Contracts;
using RMS.Modules.Purchasing.Domain.Entities;

namespace RMS.Modules.Purchasing.Application.CancelPurchaseOrder;

public sealed class CancelPurchaseOrderHandler : IRequestHandler<CancelPurchaseOrderCommand, Result>
{
    private readonly IPurchaseOrderReadStore _readStore;
    private readonly IPurchaseOrderWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public CancelPurchaseOrderHandler(IPurchaseOrderReadStore readStore, IPurchaseOrderWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
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

        order.Cancel();

        await _writeStore.UpdateAsync(order, cancellationToken);
        await _eventBus.PublishAsync(new PurchaseOrderCancelledIntegrationEvent(order.Id, order.PurchaseNumber, order.CancelledAt!.Value), cancellationToken);
        order.ClearDomainEvents();
        return Result.Success();
    }
}
