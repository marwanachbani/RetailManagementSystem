using Microsoft.Extensions.DependencyInjection;
using RMS.Modules.Identity.Application.Contracts;
using RMS.Modules.Identity.Domain.Services;
using RMS.Modules.Identity.Infrastructure.Persistence;
using RMS.Modules.Identity.Infrastructure.Security;
using FluentMigrator.Runner;

namespace RMS.Modules.Identity.Infrastructure;

public static class IdentityInfrastructureRegistration
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        // Dapper-based persistence (thin, feature-specific — no generic repository).
        services.AddSingleton<IUserReadStore, UserReadStore>();
        services.AddSingleton<IUserWriteStore, UserWriteStore>();

        // Security.
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        return services;
    }

    /// <summary>
    /// Registers FluentMigrator migrations from the Identity module assembly.
    /// Called by the host after AddIdentityInfrastructure.
    /// </summary>
    public static IServiceCollection AddIdentityMigrations(this IServiceCollection services, string connectionString)
    {
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(IdentityInfrastructureRegistration).Assembly).For.Migrations()
            );

        return services;
    }
}
