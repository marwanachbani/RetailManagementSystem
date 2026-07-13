using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;
using RMS.Modules.Sales.Application;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class StockRestorationNotificationHandler : BaseNotificationHandler<StockRestorationRequestedEvent>
{
    public StockRestorationNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(StockRestorationRequestedEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Stock Adjustment Completed",
            $"Stock restored for {e.ProductName}. Quantity: {e.Quantity}. Reason: {e.Reason}",
            NotificationCategory.Inventory,
            NotificationSeverity.Information,
            "Inventory",
            e.ProductId,
            cancellationToken);
    }
}