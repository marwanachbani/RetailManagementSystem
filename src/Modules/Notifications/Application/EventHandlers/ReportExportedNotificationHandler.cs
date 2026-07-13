using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;
using RMS.Modules.Reporting.Application.IntegrationEvents;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class ReportExportedNotificationHandler : BaseNotificationHandler<ReportExportedIntegrationEvent>
{
    public ReportExportedNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(ReportExportedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Report Exported",
            $"Report '{e.ReportType}' has been exported in {e.Format} format.",
            NotificationCategory.Reports,
            NotificationSeverity.Information,
            "Reporting",
            cancellationToken: cancellationToken);
    }
}