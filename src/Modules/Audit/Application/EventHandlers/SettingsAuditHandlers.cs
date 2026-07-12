using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Audit.Application.Contracts;
using RMS.Modules.Audit.Application.EventHandlers;
using RMS.Modules.Audit.Domain.Entities;
using RMS.Modules.Settings.Application.IntegrationEvents;

namespace RMS.Modules.Audit.Application.EventHandlers;

public sealed class SettingChangedAuditHandler : IIntegrationEventHandler<SettingChangedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public SettingChangedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(SettingChangedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Settings", "Setting Changed", "Setting", null, e.OldValue, e.NewValue, _currentUserContext);
        entry = entry with { Entity = $"Settings.{e.Section}" };
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class SettingsResetAuditHandler : IIntegrationEventHandler<SettingsResetIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public SettingsResetAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(SettingsResetIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Settings", "Setting Reset", "Settings", null, null, null, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class FolderChangedAuditHandler : IIntegrationEventHandler<FolderChangedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public FolderChangedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(FolderChangedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Settings", "Folder Changed", "Folder", e.FolderType, e.OldPath, e.NewPath, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}
