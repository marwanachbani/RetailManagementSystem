using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Infrastructure.Migrations;
using RMS.Modules.Settings.Infrastructure.Persistence;

namespace RMS.Modules.Settings.Infrastructure;

public static class SettingsInfrastructureRegistration
{
    public static IServiceCollection AddSettingsInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ISettingsReadStore, SettingsReadStore>();
        services.AddSingleton<ISettingsWriteStore, SettingsWriteStore>();
        return services;
    }

    public static IServiceCollection AddSettingsMigrations(this IServiceCollection services, string connectionString)
    {
        services.AddFluentMigratorCore()
            .Configure<SelectingProcessorAccessorOptions>(options => options.ProcessorId = "sqlite")
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(SettingsInfrastructureRegistration).Assembly).For.Migrations());

        return services;
    }
}
