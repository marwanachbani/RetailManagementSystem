using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Infrastructure.Persistence;

namespace RMS.Modules.Inventory.Infrastructure;

public static class InventoryInfrastructureRegistration
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IInventoryReadStore, InventoryReadStore>();
        services.AddSingleton<IInventoryWriteStore, InventoryWriteStore>();
        return services;
    }

    public static IServiceCollection AddInventoryMigrations(this IServiceCollection services, string connectionString)
    {
        services.AddFluentMigratorCore()
            .Configure<SelectingProcessorAccessorOptions>(options => options.ProcessorId = "sqlite")
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(InventoryInfrastructureRegistration).Assembly).For.Migrations()
            );

        return services;
    }
}
