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
}
