using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Validation;
using RMS.Modules.Settings.Application.Services;

namespace RMS.Modules.Settings.Application;

public static class SettingsModuleRegistration
{
    public static IServiceCollection AddSettingsModule(this IServiceCollection services, string baseDirectory)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(SettingsModuleRegistration).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(SettingsModuleRegistration).Assembly);

        services.AddSingleton<IFolderResolver>(_ => new FolderResolver(baseDirectory));

        return services;
    }
}
