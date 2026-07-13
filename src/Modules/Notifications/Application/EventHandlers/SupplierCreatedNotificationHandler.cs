using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;
using RMS.Modules.Suppliers.Application.CreateSupplier;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class SupplierCreatedNotificationHandler : BaseNotificationHandler<SupplierCreatedIntegrationEvent>
{
    public SupplierCreatedNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(SupplierCreatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "New Supplier Created",
            $"Supplier {e.CompanyName} ({e.SupplierCode}) has been created.",
            NotificationCategory.Suppliers,
            NotificationSeverity.Success,
            "Suppliers",
            e.SupplierId,
            cancellationToken);
    }
}