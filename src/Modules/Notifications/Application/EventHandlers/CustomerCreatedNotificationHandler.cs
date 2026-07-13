using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Customers.Application.CreateCustomer;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class CustomerCreatedNotificationHandler : BaseNotificationHandler<CustomerCreatedIntegrationEvent>
{
    public CustomerCreatedNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(CustomerCreatedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "New Customer Created",
            $"Customer {e.FullName} ({e.CustomerCode}) has been created.",
            NotificationCategory.Customers,
            NotificationSeverity.Success,
            "Customers",
            e.CustomerId,
            cancellationToken);
    }
}