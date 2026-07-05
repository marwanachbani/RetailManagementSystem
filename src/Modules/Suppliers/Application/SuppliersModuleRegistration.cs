using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Validation;

namespace RMS.Modules.Suppliers.Application;

public static class SuppliersModuleRegistration
{
    public static IServiceCollection AddSuppliersModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(SuppliersModuleRegistration).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(SuppliersModuleRegistration).Assembly);
        return services;
    }
}
