using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Validation;

namespace RMS.Modules.Purchasing.Application;

public static class PurchasingModuleRegistration
{
    public static IServiceCollection AddPurchasingModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(PurchasingModuleRegistration).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(PurchasingModuleRegistration).Assembly);
        return services;
    }
}
