using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;
using RMS.Modules.Inventory.Application;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class LowStockNotificationHandler : BaseNotificationHandler<LowStockIntegrationEvent>
{
    public LowStockNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(LowStockIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Low Stock",
            $"Product {e.ProductId} is running low. Current quantity: {e.CurrentQuantity}, threshold: {e.LowStockThreshold}",
            NotificationCategory.Inventory,
            NotificationSeverity.Warning,
            "Inventory",
            e.ProductId,
            cancellationToken);
    }
}