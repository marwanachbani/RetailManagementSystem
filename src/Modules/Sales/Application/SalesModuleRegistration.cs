using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Validation;

namespace RMS.Modules.Sales.Application;

public static class SalesModuleRegistration
{
    public static IServiceCollection AddSalesModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(SalesModuleRegistration).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(SalesModuleRegistration).Assembly);
        return services;
    }
}
