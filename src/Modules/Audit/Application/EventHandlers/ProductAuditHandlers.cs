using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Audit.Application.Contracts;
using RMS.Modules.Audit.Application.EventHandlers;
using RMS.Modules.Audit.Domain.Entities;
using RMS.Modules.Products.Application;

namespace RMS.Modules.Audit.Application.EventHandlers;

public sealed class ProductCreatedAuditHandler : IIntegrationEventHandler<ProductCreatedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public ProductCreatedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(ProductCreatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Products", "Created", "Product", e.ProductId.ToString(), null, e.Name, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class ProductUpdatedAuditHandler : IIntegrationEventHandler<ProductUpdatedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public ProductUpdatedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(ProductUpdatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Products", "Updated", "Product", e.ProductId.ToString(), null, e.Name, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class ProductDeactivatedAuditHandler : IIntegrationEventHandler<ProductDeactivatedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public ProductDeactivatedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(ProductDeactivatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Products", "Deactivated", "Product", e.ProductId.ToString(), null, null, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}
