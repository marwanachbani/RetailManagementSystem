using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using RMS.Modules.Backup.Application.Contracts;
using RMS.Modules.Backup.Infrastructure.Migrations;
using RMS.Modules.Backup.Infrastructure.Persistence;
using RMS.Modules.Backup.Infrastructure.Services;

namespace RMS.Modules.Backup.Infrastructure;

public static class BackupInfrastructureRegistration
{
    public static IServiceCollection AddBackupInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IBackupStore, BackupStore>();
        services.AddSingleton<IBackupService, BackupService>();
        return services;
    }

    public static IServiceCollection AddBackupMigrations(this IServiceCollection services, string connectionString)
    {
        services.AddFluentMigratorCore()
            .Configure<SelectingProcessorAccessorOptions>(options => options.ProcessorId = "sqlite")
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(BackupInfrastructureRegistration).Assembly).For.Migrations());

        return services;
    }
}
