using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace RMS.BuildingBlocks.Logging;

public static class LoggingRegistration
{
    /// <summary>
    /// Configures Serilog to write rolling daily file logs to
    /// %ProgramData%\RetailManagementSystem\logs and routes Microsoft.Extensions.Logging
    /// through it. Called once from the Desktop host's composition root.
    /// </summary>
    public static IServiceCollection AddRmsLogging(this IServiceCollection services, string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: Path.Combine(logDirectory, "rms-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        return services;
    }
}
