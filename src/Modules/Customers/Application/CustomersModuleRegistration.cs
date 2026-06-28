using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Validation;

namespace RMS.Modules.Customers.Application;

public static class CustomersModuleRegistration
{
    public static IServiceCollection AddCustomersModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CustomersModuleRegistration).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(CustomersModuleRegistration).Assembly);
        return services;
    }
}
