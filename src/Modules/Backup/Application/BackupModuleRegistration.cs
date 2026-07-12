using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Validation;
using RMS.Modules.Backup.Application.Contracts;

namespace RMS.Modules.Backup.Application;

public static class BackupModuleRegistration
{
    public static IServiceCollection AddBackupModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(BackupModuleRegistration).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(BackupModuleRegistration).Assembly);

        services.AddSingleton<IBackupScheduler, BackupScheduler>();

        return services;
    }
}
