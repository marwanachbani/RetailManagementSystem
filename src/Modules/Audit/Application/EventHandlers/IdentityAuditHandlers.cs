using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Audit.Application.Contracts;
using RMS.Modules.Audit.Application.EventHandlers;
using RMS.Modules.Audit.Domain.Entities;
using RMS.Modules.Identity.Application.IntegrationEvents;

namespace RMS.Modules.Audit.Application.EventHandlers;

public sealed class LoginSucceededAuditHandler : IIntegrationEventHandler<LoginSucceededIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public LoginSucceededAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(LoginSucceededIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Identity", "Login", "User", e.UserId.ToString(), null, e.UserName, _currentUserContext);
        entry = entry with { UserId = e.UserId, UserName = e.UserName };
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class LoginFailedAuditHandler : IIntegrationEventHandler<LoginFailedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public LoginFailedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(LoginFailedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Identity", "Failed Login", "User", null, null, e.UserName, _currentUserContext);
        entry = entry with { UserName = e.UserName };
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class LogoutSucceededAuditHandler : IIntegrationEventHandler<LogoutSucceededIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public LogoutSucceededAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(LogoutSucceededIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Identity", "Logout", "User", e.UserId.ToString(), null, e.UserName, _currentUserContext);
        entry = entry with { UserId = e.UserId, UserName = e.UserName };
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}
