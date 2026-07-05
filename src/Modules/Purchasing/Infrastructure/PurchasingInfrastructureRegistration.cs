using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using RMS.Modules.Purchasing.Application.Contracts;
using RMS.Modules.Purchasing.Infrastructure.Persistence;

namespace RMS.Modules.Purchasing.Infrastructure;

public static class PurchasingInfrastructureRegistration
{
    public static IServiceCollection AddPurchasingInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPurchaseOrderReadStore, PurchaseReadStore>();
        services.AddSingleton<IPurchaseOrderWriteStore, PurchaseWriteStore>();
        return services;
    }

    public static IServiceCollection AddPurchasingMigrations(this IServiceCollection services, string connectionString)
    {
        services.AddFluentMigratorCore()
            .Configure<SelectingProcessorAccessorOptions>(options => options.ProcessorId = "sqlite")
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(PurchasingInfrastructureRegistration).Assembly).For.Migrations()
            );

        return services;
    }
}
