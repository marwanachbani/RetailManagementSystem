using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Audit.Application.Contracts;
using RMS.Modules.Audit.Application.EventHandlers;
using RMS.Modules.Audit.Domain.Entities;
using RMS.Modules.Inventory.Application;
using RMS.Modules.Purchasing.Application;
using RMS.Modules.Sales.Application;

namespace RMS.Modules.Audit.Application.EventHandlers;

public sealed class StockReductionRequestedAuditHandler : IIntegrationEventHandler<StockReductionRequestedEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public StockReductionRequestedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(StockReductionRequestedEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Inventory", "Stock Decreased", "InventoryItem", e.ProductId.ToString(), null, $"{e.ProductName} x{e.Quantity}", _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class StockRestorationRequestedAuditHandler : IIntegrationEventHandler<StockRestorationRequestedEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public StockRestorationRequestedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(StockRestorationRequestedEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Inventory", "Stock Increased", "InventoryItem", e.ProductId.ToString(), null, $"{e.ProductName} x{e.Quantity}", _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class StockIncreaseRequestedAuditHandler : IIntegrationEventHandler<StockIncreaseRequestedEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public StockIncreaseRequestedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(StockIncreaseRequestedEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Inventory", "Stock Increased", "InventoryItem", e.ProductId.ToString(), null, $"{e.ProductName} x{e.Quantity}", _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}
