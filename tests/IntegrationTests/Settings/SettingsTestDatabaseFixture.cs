using System.Data;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Persistence;
using RMS.Modules.Settings.Application;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Domain;
using RMS.Modules.Settings.Infrastructure;
using RMS.Modules.Settings.Infrastructure.Migrations;
using Xunit;

namespace RMS.IntegrationTests.Settings;

public partial class SettingsTestDatabaseFixture : IDisposable
{
    private readonly string _dbFilePath;
    private readonly ServiceProvider _serviceProvider;
    private readonly string _baseDirectory;

    public SettingsTestDatabaseFixture()
    {
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteNullableGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));

        _dbFilePath = Path.Combine(Path.GetTempPath(), $"rms_settings_test_{Guid.NewGuid():N}.db");
        _baseDirectory = Path.Combine(Path.GetTempPath(), $"rms_settings_base_{Guid.NewGuid():N}");

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _dbFilePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        }.ToString();

        var services = new ServiceCollection();
        services.AddSingleton<IDbConnectionFactory>(new TestConnectionFactory(connectionString));
        services.AddSingleton<IEventBus, TestEventBus>();
        services.AddSettingsModule(_baseDirectory);
        services.AddSettingsInfrastructure();
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(CreateSettingsTablesMigration).Assembly).For.Migrations());

        _serviceProvider = services.BuildServiceProvider();
        _serviceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();

        Directory.CreateDirectory(_baseDirectory);
        var resolver = _serviceProvider.GetRequiredService<RMS.Modules.Settings.Application.Services.IFolderResolver>();
        foreach (var folder in SettingCatalog.FolderDefinitions)
            resolver.EnsureExists(resolver.GetDefaultPath(folder.FolderSubPath!));
    }

    public IServiceProvider Services => _serviceProvider;

    public void Dispose()
    {
        _serviceProvider.Dispose();
        try { File.Delete(_dbFilePath); } catch { /* best effort */ }
        try { if (Directory.Exists(_baseDirectory)) Directory.Delete(_baseDirectory, true); } catch { /* best effort */ }
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

public class SettingsIntegrationTestBase
{
    protected readonly SettingsTestDatabaseFixture Fixture;

    public SettingsIntegrationTestBase(SettingsTestDatabaseFixture fixture)
    {
        Fixture = fixture;
        Fixture.ResetState();
    }

    protected ISettingsReadStore ReadStore =>
        Fixture.Services.GetRequiredService<ISettingsReadStore>();

    protected ISettingsWriteStore WriteStore =>
        Fixture.Services.GetRequiredService<ISettingsWriteStore>();
}

public partial class SettingsTestDatabaseFixture
{
    public void ResetState()
    {
        using var connection = Services.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        connection.Execute("DELETE FROM Settings;");
        connection.Execute("DELETE FROM SettingCategories;");
    }
}
