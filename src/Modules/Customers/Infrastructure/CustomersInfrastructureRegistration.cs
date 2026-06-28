using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using RMS.Modules.Customers.Application.Contracts;
using RMS.Modules.Customers.Infrastructure.Persistence;

namespace RMS.Modules.Customers.Infrastructure;

public static class CustomersInfrastructureRegistration
{
    public static IServiceCollection AddCustomersInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ICustomerReadStore, CustomerReadStore>();
        services.AddSingleton<ICustomerWriteStore, CustomerWriteStore>();
        return services;
    }

    public static IServiceCollection AddCustomersMigrations(this IServiceCollection services, string connectionString)
    {
        services.AddFluentMigratorCore()
            .Configure<SelectingProcessorAccessorOptions>(options => options.ProcessorId = "sqlite")
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(CustomersInfrastructureRegistration).Assembly).For.Migrations()
            );

        return services;
    }
}
