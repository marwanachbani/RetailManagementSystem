using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Validation;
using RMS.Modules.Inventory.Application.EventHandlers;
using RMS.Modules.Products.Application;

namespace RMS.Modules.Inventory.Application;

public static class InventoryModuleRegistration
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(InventoryModuleRegistration).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(InventoryModuleRegistration).Assembly);

        // Register integration event handlers that bridge Products module events to Inventory actions.
        services.AddSingleton<IIntegrationEventHandler<ProductCreatedIntegrationEvent>, ProductCreatedEventHandler>();
        services.AddSingleton<IIntegrationEventHandler<ProductDeactivatedIntegrationEvent>, ProductDeactivatedEventHandler>();

        return services;
    }
}
