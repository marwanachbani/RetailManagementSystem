using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;
using RMS.Modules.Sales.Application;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class SaleCompletedNotificationHandler : BaseNotificationHandler<SaleCompletedIntegrationEvent>
{
    public SaleCompletedNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(SaleCompletedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Sale Completed",
            $"Sale {e.SaleNumber} has been completed successfully. Total amount: {e.TotalAmount:C}",
            NotificationCategory.Sales,
            NotificationSeverity.Success,
            "Sales",
            e.SaleId,
            cancellationToken);
    }
}