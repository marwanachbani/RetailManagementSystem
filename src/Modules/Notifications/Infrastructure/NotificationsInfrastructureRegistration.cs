using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Infrastructure.Persistence;

namespace RMS.Modules.Notifications.Infrastructure;

public static class NotificationsInfrastructureRegistration
{
    public static IServiceCollection AddNotificationsInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<INotificationRepository, NotificationRepository>();
        return services;
    }

    public static IServiceCollection AddNotificationsMigrations(this IServiceCollection services, string connectionString)
    {
        services.AddFluentMigratorCore()
            .Configure<SelectingProcessorAccessorOptions>(options => options.ProcessorId = "sqlite")
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(NotificationsInfrastructureRegistration).Assembly).For.Migrations()
            );

        return services;
    }
}
