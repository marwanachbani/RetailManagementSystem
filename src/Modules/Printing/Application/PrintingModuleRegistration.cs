using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Validation;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Application.Services;

namespace RMS.Modules.Printing.Application;

public static class PrintingModuleRegistration
{
    public static IServiceCollection AddPrintingModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(PrintingModuleRegistration).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(PrintingModuleRegistration).Assembly);

        services.AddSingleton<IPrintSettingsProvider, PrintSettingsProvider>();
        services.AddSingleton<IPrintingService, PrintingService>();

        return services;
    }
}
