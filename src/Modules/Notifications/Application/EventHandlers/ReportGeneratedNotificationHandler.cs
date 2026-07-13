using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;
using RMS.Modules.Reporting.Application.IntegrationEvents;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class ReportGeneratedNotificationHandler : BaseNotificationHandler<ReportGeneratedIntegrationEvent>
{
    public ReportGeneratedNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(ReportGeneratedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Report Generated",
            $"Report '{e.ReportType}' has been generated in {e.Format} format.",
            NotificationCategory.Reports,
            NotificationSeverity.Information,
            "Reporting",
            cancellationToken: cancellationToken);
    }
}