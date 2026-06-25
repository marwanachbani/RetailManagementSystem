using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Infrastructure.Persistence;

namespace RMS.Modules.Products.Infrastructure;

public static class ProductsInfrastructureRegistration
{
    public static IServiceCollection AddProductsInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IProductReadStore, ProductReadStore>();
        services.AddSingleton<IProductWriteStore, ProductWriteStore>();
        return services;
    }

    public static IServiceCollection AddProductsMigrations(this IServiceCollection services, string connectionString)
    {
        services.AddFluentMigratorCore()
            .Configure<SelectingProcessorAccessorOptions>(options => options.ProcessorId = "sqlite")
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(ProductsInfrastructureRegistration).Assembly).For.Migrations()
            );

        return services;
    }
}
