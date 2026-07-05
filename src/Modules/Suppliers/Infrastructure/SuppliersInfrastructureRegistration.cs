using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using RMS.Modules.Suppliers.Application.Contracts;
using RMS.Modules.Suppliers.Infrastructure.Persistence;

namespace RMS.Modules.Suppliers.Infrastructure;

public static class SuppliersInfrastructureRegistration
{
    public static IServiceCollection AddSuppliersInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ISupplierReadStore, SupplierReadStore>();
        services.AddSingleton<ISupplierWriteStore, SupplierWriteStore>();
        return services;
    }

    public static IServiceCollection AddSuppliersMigrations(this IServiceCollection services, string connectionString)
    {
        services.AddFluentMigratorCore()
            .Configure<SelectingProcessorAccessorOptions>(options => options.ProcessorId = "sqlite")
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(SuppliersInfrastructureRegistration).Assembly).For.Migrations()
            );

        return services;
    }
}
