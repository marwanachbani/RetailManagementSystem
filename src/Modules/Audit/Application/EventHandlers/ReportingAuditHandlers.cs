using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Audit.Application.Contracts;
using RMS.Modules.Audit.Application.EventHandlers;
using RMS.Modules.Audit.Domain.Entities;
using RMS.Modules.Reporting.Application.IntegrationEvents;

namespace RMS.Modules.Audit.Application.EventHandlers;

public sealed class ReportGeneratedAuditHandler : IIntegrationEventHandler<ReportGeneratedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public ReportGeneratedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(ReportGeneratedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Reports", "Report Generated", e.ReportType, null, null, e.Format, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class ReportExportedAuditHandler : IIntegrationEventHandler<ReportExportedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public ReportExportedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(ReportExportedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Reports", "Report Exported", e.ReportType, null, null, e.Format, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}

public sealed class ReportPrintedAuditHandler : IIntegrationEventHandler<ReportPrintedIntegrationEvent>
{
    private readonly IAuditWriteStore _writeStore;
    private readonly ICurrentUserContext _currentUserContext;

    public ReportPrintedAuditHandler(IAuditWriteStore writeStore, ICurrentUserContext currentUserContext)
    {
        _writeStore = writeStore;
        _currentUserContext = currentUserContext;
    }

    public async Task HandleAsync(ReportPrintedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var entry = AuditEntryFactory.Create("Reports", "Report Printed", e.ReportType, null, null, null, _currentUserContext);
        await _writeStore.InsertAsync(entry, cancellationToken);
    }
}
