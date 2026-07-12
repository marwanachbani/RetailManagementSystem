using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Audit.Application.Contracts;
using RMS.Modules.Audit.Application.EventHandlers;
using RMS.Modules.Audit.Domain.Entities;
using RMS.Modules.Purchasing.Application;

namespace RMS.Modules.Audit.Application.EventHandlers;

public sealed class PurchaseOrderCreatedAuditHandler : IIntegrationEventHandler<PurchaseOrderCreatedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public PurchaseOrderCreatedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(PurchaseOrderCreatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Purchasing", "Purchase Created", "PurchaseOrder", e.PurchaseOrderId.ToString(), null, e.PurchaseNumber, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class PurchaseOrderUpdatedAuditHandler : IIntegrationEventHandler<PurchaseOrderUpdatedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public PurchaseOrderUpdatedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(PurchaseOrderUpdatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Purchasing", "Purchase Updated", "PurchaseOrder", e.PurchaseOrderId.ToString(), null, e.PurchaseNumber, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class PurchaseOrderCancelledAuditHandler : IIntegrationEventHandler<PurchaseOrderCancelledIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public PurchaseOrderCancelledAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(PurchaseOrderCancelledIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Purchasing", "Purchase Cancelled", "PurchaseOrder", e.PurchaseOrderId.ToString(), null, e.PurchaseNumber, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class GoodsReceivedAuditHandler : IIntegrationEventHandler<GoodsReceivedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public GoodsReceivedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(GoodsReceivedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Purchasing", "Goods Received", "PurchaseOrder", e.PurchaseOrderId.ToString(), null, $"{e.ProductName} x{e.QuantityReceived}", _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class PurchaseCompletedAuditHandler : IIntegrationEventHandler<PurchaseCompletedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public PurchaseCompletedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(PurchaseCompletedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Purchasing", "Purchase Completed", "PurchaseOrder", e.PurchaseOrderId.ToString(), null, e.PurchaseNumber, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}
