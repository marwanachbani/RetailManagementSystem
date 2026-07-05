using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Purchasing.Application.Contracts;
using RMS.Modules.Purchasing.Domain.Entities;

namespace RMS.Modules.Purchasing.Application.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderHandler : IRequestHandler<CreatePurchaseOrderCommand, Result<Guid>>
{
    private readonly IPurchaseOrderWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public CreatePurchaseOrderHandler(IPurchaseOrderWriteStore writeStore, IEventBus eventBus)
    {
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result<Guid>> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), request.SupplierId, request.SupplierName, request.Notes);
        order.UpdateDetails(request.SupplierId, request.SupplierName, request.Notes, request.TaxPercentage);

        foreach (var item in request.Items)
        {
            order.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitCost);
        }

        await _writeStore.InsertAsync(order, cancellationToken);
        await _eventBus.PublishAsync(new PurchaseOrderCreatedIntegrationEvent(order.Id, order.PurchaseNumber, order.SupplierId, order.OrderDate), cancellationToken);
        order.ClearDomainEvents();
        return Result.Success(order.Id);
    }
}
