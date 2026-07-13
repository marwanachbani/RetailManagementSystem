using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;
using RMS.Modules.Purchasing.Application;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class PurchaseCompletedNotificationHandler : BaseNotificationHandler<PurchaseCompletedIntegrationEvent>
{
    public PurchaseCompletedNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(PurchaseCompletedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Purchase Completed",
            $"Purchase order {e.PurchaseNumber} has been completed. Total amount: {e.TotalAmount:C}",
            NotificationCategory.Purchasing,
            NotificationSeverity.Success,
            "Purchasing",
            e.PurchaseOrderId,
            cancellationToken);
    }
}