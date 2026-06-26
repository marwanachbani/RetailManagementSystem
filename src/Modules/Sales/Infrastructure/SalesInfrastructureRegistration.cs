using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using RMS.Modules.Sales.Application.Contracts;
using RMS.Modules.Sales.Infrastructure.Persistence;
using RMS.Modules.Sales.Infrastructure.ReceiptGeneration;

namespace RMS.Modules.Sales.Infrastructure;

public static class SalesInfrastructureRegistration
{
    public static IServiceCollection AddSalesInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ISaleReadStore, SaleReadStore>();
        services.AddSingleton<ISaleWriteStore, SaleWriteStore>();
        services.AddSingleton<IReceiptGenerator, ReceiptGenerator>();
        return services;
    }

    public static IServiceCollection AddSalesMigrations(this IServiceCollection services, string connectionString)
    {
        services.AddFluentMigratorCore()
            .Configure<SelectingProcessorAccessorOptions>(options => options.ProcessorId = "sqlite")
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(SalesInfrastructureRegistration).Assembly).For.Migrations()
            );

        return services;
    }
}
