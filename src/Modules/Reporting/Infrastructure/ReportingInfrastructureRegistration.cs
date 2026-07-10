using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using RMS.Modules.Reporting.Application.Contracts;
using RMS.Modules.Reporting.Infrastructure.Persistence;

namespace RMS.Modules.Reporting.Infrastructure;

public static class ReportingInfrastructureRegistration
{
    public static IServiceCollection AddReportingInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IReportingReadStore, ReportingReadStore>();
        return services;
    }

    public static IServiceCollection AddReportingMigrations(this IServiceCollection services, string connectionString)
    {
        services.AddFluentMigratorCore()
            .Configure<SelectingProcessorAccessorOptions>(options => options.ProcessorId = "sqlite")
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(ReportingInfrastructureRegistration).Assembly).For.Migrations()
            );

        return services;
    }
}
