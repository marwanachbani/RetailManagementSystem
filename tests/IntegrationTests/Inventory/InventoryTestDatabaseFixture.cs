using System.Data;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.EventStore;
using RMS.BuildingBlocks.Persistence;
using RMS.Modules.Identity.Infrastructure;
using RMS.Modules.Identity.Infrastructure.Migrations;
using RMS.Modules.Inventory.Application;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Domain.Entities;
using RMS.Modules.Inventory.Domain.ValueObjects;
using RMS.Modules.Inventory.Infrastructure;
using RMS.Modules.Inventory.Infrastructure.Migrations;
using RMS.Modules.Inventory.Infrastructure.Persistence;
using RMS.Modules.Products.Infrastructure;
using RMS.Modules.Products.Infrastructure.Migrations;
using Xunit;

namespace RMS.IntegrationTests.Inventory;

public class InventoryTestDatabaseFixture : IDisposable
{
    private readonly string _dbFilePath;
    private readonly ServiceProvider _serviceProvider;

    public InventoryTestDatabaseFixture()
    {
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteNullableGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));

        _dbFilePath = Path.Combine(Path.GetTempPath(), $"rms_inventory_test_{Guid.NewGuid():N}.db");

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

        services.AddInventoryModule();
        services.AddInventoryInfrastructure();
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(
                    typeof(InitialIdentityMigration).Assembly,
                    typeof(CreateProductsTablesMigration).Assembly,
                    typeof(CreateInventoryTablesMigration).Assembly).For.Migrations()
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

public class InventoryIntegrationTestBase
{
    protected readonly InventoryTestDatabaseFixture Fixture;

    public InventoryIntegrationTestBase(InventoryTestDatabaseFixture fixture)
    {
        Fixture = fixture;
        ResetState();
    }

    public void ResetState()
    {
        using var connection = Fixture.Services.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        connection.Execute("DELETE FROM InventoryTransactions;");
        connection.Execute("DELETE FROM InventoryItems;");
        connection.Execute("DELETE FROM EventStore;");
    }

    protected IInventoryReadStore ReadStore => Fixture.Services.GetRequiredService<IInventoryReadStore>();
    protected IInventoryWriteStore WriteStore => Fixture.Services.GetRequiredService<IInventoryWriteStore>();

    protected static InventoryItem CreateSampleItem(Guid? id = null, Guid? productId = null, int quantity = 0)
    {
        return InventoryItem.Create(
            id ?? Guid.NewGuid(),
            productId ?? Guid.NewGuid(),
            quantity,
            10);
    }
}
