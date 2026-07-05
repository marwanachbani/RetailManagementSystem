using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Purchasing.Application.Contracts;
using RMS.Modules.Purchasing.Domain.Entities;

namespace RMS.Modules.Purchasing.Application.UpdatePurchaseOrder;

public sealed class UpdatePurchaseOrderHandler : IRequestHandler<UpdatePurchaseOrderCommand, Result>
{
    private readonly IPurchaseOrderReadStore _readStore;
    private readonly IPurchaseOrderWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public UpdatePurchaseOrderHandler(IPurchaseOrderReadStore readStore, IPurchaseOrderWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var readModel = await _readStore.GetByIdAsync(request.PurchaseOrderId, cancellationToken);
        if (readModel is null)
            return Result.Failure("PurchaseOrder.NotFound", "Purchase order not found.");

        var items = await _readStore.GetItemsByPurchaseOrderIdAsync(request.PurchaseOrderId, cancellationToken);
        var receipts = await _readStore.GetGoodsReceiptsByPurchaseOrderIdAsync(request.PurchaseOrderId, cancellationToken);

        var order = PurchaseOrder.Rehydrate(
            readModel.Id, readModel.PurchaseNumber, readModel.SupplierId, readModel.SupplierName,
            readModel.OrderDate, Enum.Parse<Domain.Entities.PurchaseStatus>(readModel.Status),
            readModel.SubTotal, readModel.TaxAmount, readModel.TotalAmount, readModel.TaxPercentage,
            readModel.CompletedAt, readModel.CancelledAt, readModel.CreatedAt, readModel.Notes, readModel.SupplierInvoiceNumber);

        order.RehydrateItems(items.Select(i => Domain.Entities.PurchaseOrderItem.Rehydrate(
            i.Id, i.PurchaseOrderId, i.ProductId, i.ProductName, i.Quantity, i.UnitCost, i.ReceivedQuantity)).ToList());

        order.RehydrateGoodsReceipts(receipts.Select(r => Domain.Entities.GoodsReceipt.Rehydrate(
            r.Id, r.PurchaseOrderId, r.ProductId, r.QuantityReceived, r.ReceivedAt, r.BatchNumber, r.ExpiryDate)).ToList());

        order.UpdateDetails(request.SupplierId, request.SupplierName, request.Notes, request.TaxPercentage);

        // Remove items not in the updated list
        var existingIds = request.Items.Where(i => i.Id.HasValue).Select(i => i.Id.Value).ToHashSet();
        foreach (var existing in order.Items.Where(i => !existingIds.Contains(i.Id)).ToList())
        {
            order.RemoveItem(existing.Id);
        }

        foreach (var itemDto in request.Items)
        {
            if (itemDto.Id.HasValue && order.Items.Any(i => i.Id == itemDto.Id.Value))
            {
                order.UpdateItem(itemDto.Id.Value, itemDto.Quantity, itemDto.UnitCost);
            }
            else
            {
                order.AddItem(itemDto.ProductId, itemDto.ProductName, itemDto.Quantity, itemDto.UnitCost);
            }
        }

        await _writeStore.UpdateAsync(order, cancellationToken);
        await _eventBus.PublishAsync(new PurchaseOrderUpdatedIntegrationEvent(order.Id, order.PurchaseNumber, order.SupplierId), cancellationToken);
        order.ClearDomainEvents();
        return Result.Success();
    }
}
