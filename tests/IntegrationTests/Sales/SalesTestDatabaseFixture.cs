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
using RMS.Modules.Inventory.Infrastructure;
using RMS.Modules.Inventory.Infrastructure.Migrations;
using RMS.Modules.Products.Infrastructure;
using RMS.Modules.Products.Infrastructure.Migrations;
using RMS.Modules.Sales.Application;
using RMS.Modules.Sales.Application.Contracts;
using RMS.Modules.Sales.Domain.Entities;
using RMS.Modules.Sales.Infrastructure;
using RMS.Modules.Sales.Infrastructure.Migrations;
using Xunit;

namespace RMS.IntegrationTests.Sales;

public class SalesTestDatabaseFixture : IDisposable
{
    private readonly string _dbFilePath;
    private readonly ServiceProvider _serviceProvider;

    public SalesTestDatabaseFixture()
    {
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteNullableGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));

        _dbFilePath = Path.Combine(Path.GetTempPath(), $"rms_sales_test_{Guid.NewGuid():N}.db");

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

        services.AddSalesModule();
        services.AddSalesInfrastructure();
        services.AddInventoryModule();
        services.AddInventoryInfrastructure();
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(
                    typeof(InitialIdentityMigration).Assembly,
                    typeof(CreateProductsTablesMigration).Assembly,
                    typeof(CreateInventoryTablesMigration).Assembly,
                    typeof(CreateSalesTablesMigration).Assembly).For.Migrations()
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

public class SalesIntegrationTestBase
{
    protected readonly SalesTestDatabaseFixture Fixture;

    public SalesIntegrationTestBase(SalesTestDatabaseFixture fixture)
    {
        Fixture = fixture;
        ResetState();
    }

    public void ResetState()
    {
        using var connection = Fixture.Services.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        connection.Execute("DELETE FROM Receipts;");
        connection.Execute("DELETE FROM SaleItems;");
        connection.Execute("DELETE FROM Sales;");
        connection.Execute("DELETE FROM EventStore;");
    }

    protected ISaleReadStore ReadStore => Fixture.Services.GetRequiredService<ISaleReadStore>();
    protected ISaleWriteStore WriteStore => Fixture.Services.GetRequiredService<ISaleWriteStore>();

    protected static Sale CreateSampleSale(Guid? id = null, Guid? cashierId = null)
    {
        return Sale.Create(id ?? Guid.NewGuid(), cashierId ?? Guid.NewGuid());
    }
}
