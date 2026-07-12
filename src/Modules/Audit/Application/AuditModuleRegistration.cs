using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Validation;
using RMS.Modules.Audit.Application.Contracts;
using RMS.Modules.Audit.Application.EventHandlers;
using RMS.Modules.Customers.Application.CreateCustomer;
using RMS.Modules.Customers.Application.UpdateCustomer;
using RMS.Modules.Customers.Application.DeactivateCustomer;
using RMS.Modules.Customers.Application.ReactivateCustomer;
using RMS.Modules.Identity.Application.IntegrationEvents;
using RMS.Modules.Inventory.Application;
using RMS.Modules.Products.Application;
using RMS.Modules.Purchasing.Application;
using RMS.Modules.Reporting.Application.IntegrationEvents;
using RMS.Modules.Sales.Application;
using RMS.Modules.Settings.Application.IntegrationEvents;
using RMS.Modules.Suppliers.Application.CreateSupplier;
using RMS.Modules.Suppliers.Application.UpdateSupplier;
using RMS.Modules.Suppliers.Application.DeactivateSupplier;
using RMS.Modules.Suppliers.Application.ReactivateSupplier;

namespace RMS.Modules.Audit.Application;

public static class AuditModuleRegistration
{
    public static IServiceCollection AddAuditModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(AuditModuleRegistration).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(AuditModuleRegistration).Assembly);

        services.AddSingleton<IIntegrationEventHandler<ProductCreatedIntegrationEvent>, ProductCreatedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<ProductUpdatedIntegrationEvent>, ProductUpdatedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<ProductDeactivatedIntegrationEvent>, ProductDeactivatedAuditHandler>();

        services.AddSingleton<IIntegrationEventHandler<SaleCreatedIntegrationEvent>, SaleCreatedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<SaleCompletedIntegrationEvent>, SaleCompletedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<SaleRefundedIntegrationEvent>, SaleRefundedAuditHandler>();

        services.AddSingleton<IIntegrationEventHandler<StockReductionRequestedEvent>, StockReductionRequestedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<StockRestorationRequestedEvent>, StockRestorationRequestedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<StockIncreaseRequestedEvent>, StockIncreaseRequestedAuditHandler>();

        services.AddSingleton<IIntegrationEventHandler<InventoryItemCreatedIntegrationEvent>, InventoryItemCreatedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<StockChangedIntegrationEvent>, StockChangedAuditHandler>();

        services.AddSingleton<IIntegrationEventHandler<PurchaseOrderCreatedIntegrationEvent>, PurchaseOrderCreatedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<PurchaseOrderUpdatedIntegrationEvent>, PurchaseOrderUpdatedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<PurchaseOrderCancelledIntegrationEvent>, PurchaseOrderCancelledAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<GoodsReceivedIntegrationEvent>, GoodsReceivedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<PurchaseCompletedIntegrationEvent>, PurchaseCompletedAuditHandler>();

        services.AddSingleton<IIntegrationEventHandler<SupplierCreatedIntegrationEvent>, SupplierCreatedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<SupplierUpdatedIntegrationEvent>, SupplierUpdatedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<SupplierDeactivatedIntegrationEvent>, SupplierDeactivatedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<SupplierReactivatedIntegrationEvent>, SupplierReactivatedAuditHandler>();

        services.AddSingleton<IIntegrationEventHandler<CustomerCreatedIntegrationEvent>, CustomerCreatedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<CustomerUpdatedIntegrationEvent>, CustomerUpdatedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<CustomerDeactivatedIntegrationEvent>, CustomerDeactivatedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<CustomerReactivatedIntegrationEvent>, CustomerReactivatedAuditHandler>();

        services.AddSingleton<IIntegrationEventHandler<LoginSucceededIntegrationEvent>, LoginSucceededAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<LoginFailedIntegrationEvent>, LoginFailedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<LogoutSucceededIntegrationEvent>, LogoutSucceededAuditHandler>();

        services.AddSingleton<IIntegrationEventHandler<SettingChangedIntegrationEvent>, SettingChangedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<SettingsResetIntegrationEvent>, SettingsResetAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<FolderChangedIntegrationEvent>, FolderChangedAuditHandler>();

        services.AddSingleton<IIntegrationEventHandler<ReportGeneratedIntegrationEvent>, ReportGeneratedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<ReportExportedIntegrationEvent>, ReportExportedAuditHandler>();
        services.AddSingleton<IIntegrationEventHandler<ReportPrintedIntegrationEvent>, ReportPrintedAuditHandler>();

        return services;
    }
}
