using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;
using RMS.Modules.Purchasing.Application;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class GoodsReceivedNotificationHandler : BaseNotificationHandler<GoodsReceivedIntegrationEvent>
{
    public GoodsReceivedNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(GoodsReceivedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Goods Received",
            $"Received {e.QuantityReceived} units of {e.ProductName} for purchase order {e.PurchaseNumber}.",
            NotificationCategory.Purchasing,
            NotificationSeverity.Success,
            "Purchasing",
            e.PurchaseOrderId,
            cancellationToken);
    }
}