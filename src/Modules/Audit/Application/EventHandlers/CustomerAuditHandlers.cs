using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Audit.Application.Contracts;
using RMS.Modules.Audit.Application.EventHandlers;
using RMS.Modules.Audit.Domain.Entities;
using RMS.Modules.Customers.Application.CreateCustomer;
using RMS.Modules.Customers.Application.UpdateCustomer;
using RMS.Modules.Customers.Application.DeactivateCustomer;
using RMS.Modules.Customers.Application.ReactivateCustomer;

namespace RMS.Modules.Audit.Application.EventHandlers;

public sealed class CustomerCreatedAuditHandler : IIntegrationEventHandler<CustomerCreatedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public CustomerCreatedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(CustomerCreatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Customers", "Created", "Customer", e.CustomerId.ToString(), null, e.FullName, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class CustomerUpdatedAuditHandler : IIntegrationEventHandler<CustomerUpdatedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public CustomerUpdatedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(CustomerUpdatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Customers", "Updated", "Customer", e.CustomerId.ToString(), null, e.FullName, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class CustomerDeactivatedAuditHandler : IIntegrationEventHandler<CustomerDeactivatedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public CustomerDeactivatedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(CustomerDeactivatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Customers", "Deactivated", "Customer", e.CustomerId.ToString(), null, null, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class CustomerReactivatedAuditHandler : IIntegrationEventHandler<CustomerReactivatedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public CustomerReactivatedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(CustomerReactivatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Customers", "Reactivated", "Customer", e.CustomerId.ToString(), null, null, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}
