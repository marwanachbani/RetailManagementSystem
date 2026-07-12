using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using RMS.Modules.Audit.Application.Contracts;
using RMS.Modules.Audit.Infrastructure.Migrations;
using RMS.Modules.Audit.Infrastructure.Persistence;

namespace RMS.Modules.Audit.Infrastructure;

public static class AuditInfrastructureRegistration
{
    public static IServiceCollection AddAuditInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAuditReadStore, AuditReadStore>();
        services.AddSingleton<IAuditWriteStore, AuditWriteStore>();
        return services;
    }

    public static IServiceCollection AddAuditMigrations(this IServiceCollection services, string connectionString)
    {
        services.AddFluentMigratorCore()
            .Configure<SelectingProcessorAccessorOptions>(options => options.ProcessorId = "sqlite")
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(AuditInfrastructureRegistration).Assembly).For.Migrations());

        return services;
    }
}
