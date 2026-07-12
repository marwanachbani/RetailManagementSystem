using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Audit.Application.Contracts;
using RMS.Modules.Audit.Application.EventHandlers;
using RMS.Modules.Audit.Domain.Entities;
using RMS.Modules.Inventory.Application;

namespace RMS.Modules.Audit.Application.EventHandlers;

public sealed class InventoryItemCreatedAuditHandler : IIntegrationEventHandler<InventoryItemCreatedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public InventoryItemCreatedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(InventoryItemCreatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Inventory", "Stock Increased", "InventoryItem", e.InventoryItemId.ToString(), null, e.InitialQuantity.ToString(), _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class StockChangedAuditHandler : IIntegrationEventHandler<StockChangedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public StockChangedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(StockChangedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var action = e.ChangeType switch
        {
            "Increase" => "Stock Increased",
            "Decrease" => "Stock Decreased",
            "Adjust" => "Stock Adjustment",
            _ => "Stock Changed"
        };

        var entry = AuditEntryFactory.Create("Inventory", action, "InventoryItem", e.InventoryItemId.ToString(), null, e.NewQuantity.ToString(), _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}
