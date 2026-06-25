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
using RMS.Modules.Products.Application;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Domain.Entities;
using RMS.Modules.Products.Domain.ValueObjects;
using RMS.Modules.Products.Infrastructure;
using RMS.Modules.Products.Infrastructure.Migrations;
using RMS.Modules.Products.Infrastructure.Persistence;
using Xunit;

namespace RMS.IntegrationTests.Products;

public partial class TestDatabaseFixture : IDisposable
{
    private readonly string _dbFilePath;
    private readonly ServiceProvider _serviceProvider;

    public TestDatabaseFixture()
    {
        // Register SQLite GUID type handlers before any Dapper queries execute.
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteNullableGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));

        _dbFilePath = Path.Combine(Path.GetTempPath(), $"rms_products_test_{Guid.NewGuid():N}.db");

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

        services.AddProductsModule();
        services.AddProductsInfrastructure();
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(
                    typeof(InitialIdentityMigration).Assembly,
                    typeof(CreateProductsTablesMigration).Assembly).For.Migrations()
            );

        _serviceProvider = services.BuildServiceProvider();

        var runner = _serviceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    public IServiceProvider Services => _serviceProvider;

    public void Dispose()
    {
        _serviceProvider.Dispose();
        try { File.Delete(_dbFilePath); } catch { /* best effort cleanup */ }
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

public class ProductsIntegrationTestBase
{
    protected readonly TestDatabaseFixture Fixture;

    public ProductsIntegrationTestBase(TestDatabaseFixture fixture)
    {
        Fixture = fixture;
        Fixture.ResetState();
    }

    protected IProductReadStore ReadStore => Fixture.Services.GetRequiredService<IProductReadStore>();
    protected IProductWriteStore WriteStore => Fixture.Services.GetRequiredService<IProductWriteStore>();

    protected static Product CreateSampleProduct(Guid? id = null, Guid? categoryId = null, string? barcode = null)
    {
        return Product.Create(
            id ?? Guid.NewGuid(),
            "Sample Product",
            "Sample Description",
            Barcode.Create(barcode ?? $"BAR-{Guid.NewGuid():N}"),
            categoryId ?? CreateProductsTablesMigration.ElectronicsCategoryId,
            Money.Create(100),
            Money.Create(50));
    }
}

public partial class TestDatabaseFixture
{
    public void ResetState()
    {
        using var connection = _serviceProvider.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        connection.Execute("DELETE FROM EventStore;");
        connection.Execute("DELETE FROM Products;");
    }
}
