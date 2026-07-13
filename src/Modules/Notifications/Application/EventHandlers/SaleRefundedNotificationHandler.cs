using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;
using RMS.Modules.Sales.Application;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class SaleRefundedNotificationHandler : BaseNotificationHandler<SaleRefundedIntegrationEvent>
{
    public SaleRefundedNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(SaleRefundedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Refund Completed",
            $"Refund of {e.RefundAmount:C} has been processed for sale {e.SaleNumber}.",
            NotificationCategory.Sales,
            NotificationSeverity.Warning,
            "Sales",
            e.SaleId,
            cancellationToken);
    }
}