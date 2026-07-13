using System.Data;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.EventStore;
using RMS.BuildingBlocks.Persistence;
using RMS.Modules.Printing.Application;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Infrastructure;
using RMS.Modules.Printing.Infrastructure.Migrations;
using RMS.Modules.Settings.Application;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Domain;
using RMS.Modules.Settings.Infrastructure;
using RMS.Modules.Settings.Infrastructure.Migrations;
using Xunit;

namespace RMS.IntegrationTests.Printing;

public class PrintingTestDatabaseFixture : IDisposable
{
    private readonly string _dbFilePath;
    private readonly ServiceProvider _serviceProvider;

    public PrintingTestDatabaseFixture()
    {
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteNullableGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));

        _dbFilePath = Path.Combine(Path.GetTempPath(), $"rms_printing_test_{Guid.NewGuid():N}.db");

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _dbFilePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        }.ToString();

        var services = new ServiceCollection();
        services.AddSingleton<IDbConnectionFactory>(new TestConnectionFactory(connectionString));
        services.AddSingleton<IEventBus, TestEventBus>();
        services.AddSingleton<IEventStore, SqliteEventStore>();

        services.AddSettingsModule(Path.GetTempPath());
        services.AddSettingsInfrastructure();
        services.AddSettingsMigrations(connectionString);

        services.AddPrintingModule();
        services.AddPrintingInfrastructure();
        services.AddPrintingMigrations(connectionString);

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(
                    typeof(CreateSettingsTablesMigration).Assembly,
                    typeof(CreatePrintingTablesMigration).Assembly).For.Migrations()
            );

        _serviceProvider = services.BuildServiceProvider();

        var runner = _serviceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    public IServiceProvider Services => _serviceProvider;

    public void Dispose()
    {
        _serviceProvider.Dispose();
        try { File.Delete(_dbFilePath); } catch { }
    }

    private sealed class TestConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public TestConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }

    private sealed class TestEventBus : IEventBus
    {
        public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent
        {
            return Task.CompletedTask;
        }
    }
}

public class PrintingIntegrationTestBase : IClassFixture<PrintingTestDatabaseFixture>
{
    protected readonly PrintingTestDatabaseFixture Fixture;

    protected ISettingsReadStore SettingsReadStore => Fixture.Services.GetRequiredService<RMS.Modules.Settings.Application.Contracts.ISettingsReadStore>();
    protected ISettingsWriteStore SettingsWriteStore => Fixture.Services.GetRequiredService<RMS.Modules.Settings.Application.Contracts.ISettingsWriteStore>();
    protected IPrintingService PrintingService => Fixture.Services.GetRequiredService<IPrintingService>();
    protected IBarcodeGenerator BarcodeGenerator => Fixture.Services.GetRequiredService<IBarcodeGenerator>();
    protected IDocumentRenderingService Renderer => Fixture.Services.GetRequiredService<IDocumentRenderingService>();
    protected IPrinterService PrinterService => Fixture.Services.GetRequiredService<IPrinterService>();
    protected IPrinterDiscovery PrinterDiscovery => Fixture.Services.GetRequiredService<IPrinterDiscovery>();
    protected IPrintJobRepository PrintJobRepository => Fixture.Services.GetRequiredService<IPrintJobRepository>();
    protected IPrintSettingsProvider PrintSettingsProvider => Fixture.Services.GetRequiredService<IPrintSettingsProvider>();

    public PrintingIntegrationTestBase(PrintingTestDatabaseFixture fixture)
    {
        Fixture = fixture;
        ResetState();
    }

    public void ResetState()
    {
        using var connection = Fixture.Services.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        connection.Execute("DELETE FROM PrintJobs;");
    }
}
