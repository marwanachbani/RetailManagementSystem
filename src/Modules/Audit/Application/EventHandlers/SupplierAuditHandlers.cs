using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Audit.Application.Contracts;
using RMS.Modules.Audit.Application.EventHandlers;
using RMS.Modules.Audit.Domain.Entities;
using RMS.Modules.Suppliers.Application.CreateSupplier;
using RMS.Modules.Suppliers.Application.UpdateSupplier;
using RMS.Modules.Suppliers.Application.DeactivateSupplier;
using RMS.Modules.Suppliers.Application.ReactivateSupplier;

namespace RMS.Modules.Audit.Application.EventHandlers;

public sealed class SupplierCreatedAuditHandler : IIntegrationEventHandler<SupplierCreatedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public SupplierCreatedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(SupplierCreatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Suppliers", "Created", "Supplier", e.SupplierId.ToString(), null, e.CompanyName, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class SupplierUpdatedAuditHandler : IIntegrationEventHandler<SupplierUpdatedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public SupplierUpdatedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(SupplierUpdatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Suppliers", "Updated", "Supplier", e.SupplierId.ToString(), null, e.CompanyName, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class SupplierDeactivatedAuditHandler : IIntegrationEventHandler<SupplierDeactivatedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public SupplierDeactivatedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(SupplierDeactivatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Suppliers", "Deactivated", "Supplier", e.SupplierId.ToString(), null, null, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class SupplierReactivatedAuditHandler : IIntegrationEventHandler<SupplierReactivatedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public SupplierReactivatedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(SupplierReactivatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Suppliers", "Reactivated", "Supplier", e.SupplierId.ToString(), null, null, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}
