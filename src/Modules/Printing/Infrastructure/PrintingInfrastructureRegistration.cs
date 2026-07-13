using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Infrastructure.Barcode;
using RMS.Modules.Printing.Infrastructure.EscPos;
using RMS.Modules.Printing.Infrastructure.Persistence;
using RMS.Modules.Printing.Infrastructure.Printing;
using RMS.Modules.Printing.Infrastructure.QuestPdf;

namespace RMS.Modules.Printing.Infrastructure;

public static class PrintingInfrastructureRegistration
{
    public static IServiceCollection AddPrintingInfrastructure(this IServiceCollection services)
    {
        // QuestPDF is used for community / evaluation use.
        QuestPDF.Settings.License = LicenseType.Community;

        services.AddSingleton<IBarcodeGenerator, BarcodeGenerator>();
        services.AddSingleton<IDocumentRenderingService, QuestPdfDocumentRenderer>();
        services.AddSingleton<IReceiptPrinter, ThermalPosPrinter>();
        services.AddSingleton<ILabelPrinter, ThermalPosPrinter>();
        services.AddSingleton<IPrinterDiscovery, PrinterDiscovery>();
        services.AddSingleton<IPrinterService, WindowsPrinterService>();
        services.AddSingleton<IPrintJobRepository, PrintJobRepository>();

        return services;
    }

    public static IServiceCollection AddPrintingMigrations(this IServiceCollection services, string connectionString)
    {
        services.AddFluentMigratorCore()
            .Configure<SelectingProcessorAccessorOptions>(options => options.ProcessorId = "sqlite")
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(PrintingInfrastructureRegistration).Assembly).For.Migrations());

        return services;
    }
}
