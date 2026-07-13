using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;
using RMS.Modules.Purchasing.Application;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class PurchaseOrderCreatedNotificationHandler : BaseNotificationHandler<PurchaseOrderCreatedIntegrationEvent>
{
    public PurchaseOrderCreatedNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(PurchaseOrderCreatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Purchase Order Created",
            $"Purchase order {e.PurchaseNumber} has been created.",
            NotificationCategory.Purchasing,
            NotificationSeverity.Information,
            "Purchasing",
            e.PurchaseOrderId,
            cancellationToken);
    }
}