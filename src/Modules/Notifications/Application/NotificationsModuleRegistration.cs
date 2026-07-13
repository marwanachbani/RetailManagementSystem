using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Validation;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Application.EventHandlers;
using RMS.Modules.Notifications.Domain;
using RMS.Modules.Sales.Application;
using RMS.Modules.Inventory.Application;
using RMS.Modules.Purchasing.Application;
using RMS.Modules.Customers.Application.CreateCustomer;
using RMS.Modules.Suppliers.Application.CreateSupplier;
using RMS.Modules.Reporting.Application.IntegrationEvents;
using RMS.Modules.Backup.Application.IntegrationEvents;
using RMS.Modules.Settings.Application.IntegrationEvents;
using RMS.Modules.Notifications.Application.IntegrationEvents;

namespace RMS.Modules.Notifications.Application;

public static class NotificationsModuleRegistration
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(NotificationsModuleRegistration).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(NotificationsModuleRegistration).Assembly);

        services.AddSingleton<IIntegrationEventHandler<SaleCompletedIntegrationEvent>, SaleCompletedNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<SaleRefundedIntegrationEvent>, SaleRefundedNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<StockReductionRequestedEvent>, StockAdjustmentNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<StockRestorationRequestedEvent>, StockRestorationNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<LowStockIntegrationEvent>, LowStockNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<PurchaseOrderCreatedIntegrationEvent>, PurchaseOrderCreatedNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<GoodsReceivedIntegrationEvent>, GoodsReceivedNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<PurchaseCompletedIntegrationEvent>, PurchaseCompletedNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<CustomerCreatedIntegrationEvent>, CustomerCreatedNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<SupplierCreatedIntegrationEvent>, SupplierCreatedNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<ReportGeneratedIntegrationEvent>, ReportGeneratedNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<ReportExportedIntegrationEvent>, ReportExportedNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<BackupCreatedIntegrationEvent>, BackupCompletedNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<BackupRestoredIntegrationEvent>, RestoreCompletedNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<SettingChangedIntegrationEvent>, CriticalSettingChangedNotificationHandler>();

        services.AddSingleton<IIntegrationEventHandler<DatabaseErrorNotificationEvent>, DatabaseErrorNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<BackupFailedNotificationEvent>, BackupFailedNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<MissingStorageFolderNotificationEvent>, MissingStorageFolderNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<MigrationFailureNotificationEvent>, MigrationFailureNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<UnexpectedExceptionNotificationEvent>, UnexpectedExceptionNotificationHandler>();
        services.AddSingleton<IIntegrationEventHandler<DiskSpaceWarningNotificationEvent>, DiskSpaceWarningNotificationHandler>();

        return services;
    }
}
