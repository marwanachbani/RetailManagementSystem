using System.Data;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.EventStore;
using RMS.BuildingBlocks.Persistence;
using RMS.Modules.Customers.Application;
using RMS.Modules.Customers.Infrastructure;
using RMS.Modules.Customers.Infrastructure.Migrations;
using RMS.Modules.Identity.Infrastructure;
using RMS.Modules.Identity.Infrastructure.Migrations;
using RMS.Modules.Inventory.Application;
using RMS.Modules.Inventory.Infrastructure;
using RMS.Modules.Inventory.Infrastructure.Migrations;
using RMS.Modules.Products.Application;
using RMS.Modules.Products.Infrastructure;
using RMS.Modules.Products.Infrastructure.Migrations;
using RMS.Modules.Purchasing.Application;
using RMS.Modules.Purchasing.Infrastructure;
using RMS.Modules.Purchasing.Infrastructure.Migrations;
using RMS.Modules.Reporting.Application;
using RMS.Modules.Reporting.Application.Contracts;
using RMS.Modules.Reporting.Infrastructure;
using RMS.Modules.Sales.Application;
using RMS.Modules.Sales.Domain.Entities;
using RMS.Modules.Sales.Infrastructure;
using RMS.Modules.Sales.Infrastructure.Migrations;
using RMS.Modules.Suppliers.Application;
using RMS.Modules.Suppliers.Infrastructure;
using RMS.Modules.Suppliers.Infrastructure.Migrations;
using Xunit;

namespace RMS.IntegrationTests.Reporting;

public class ReportingTestDatabaseFixture : IDisposable
{
    private readonly string _dbFilePath;
    private readonly ServiceProvider _serviceProvider;

    public ReportingTestDatabaseFixture()
    {
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteNullableGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));

        _dbFilePath = Path.Combine(Path.GetTempPath(), $"rms_reporting_test_{Guid.NewGuid():N}.db");

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

        services.AddReportingModule();
        services.AddReportingInfrastructure();
        services.AddReportingMigrations(connectionString);

        services.AddSalesModule();
        services.AddSalesInfrastructure();
        services.AddSalesMigrations(connectionString);

        services.AddProductsModule();
        services.AddProductsInfrastructure();
        services.AddProductsMigrations(connectionString);

        services.AddInventoryModule();
        services.AddInventoryInfrastructure();
        services.AddInventoryMigrations(connectionString);

        services.AddCustomersModule();
        services.AddCustomersInfrastructure();
        services.AddCustomersMigrations(connectionString);

        services.AddSuppliersModule();
        services.AddSuppliersInfrastructure();
        services.AddSuppliersMigrations(connectionString);

        services.AddPurchasingModule();
        services.AddPurchasingInfrastructure();
        services.AddPurchasingMigrations(connectionString);

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(
                    typeof(InitialIdentityMigration).Assembly,
                    typeof(CreateProductsTablesMigration).Assembly,
                    typeof(CreateInventoryTablesMigration).Assembly,
                    typeof(CreateSalesTablesMigration).Assembly,
                    typeof(CreateCustomersTableMigration).Assembly,
                    typeof(CreateSuppliersTableMigration).Assembly,
                    typeof(CreatePurchasingTablesMigration).Assembly).For.Migrations()
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

public class ReportingTestBase : IClassFixture<ReportingTestDatabaseFixture>
{
    protected readonly ReportingTestDatabaseFixture Fixture;

    public ReportingTestBase(ReportingTestDatabaseFixture fixture)
    {
        Fixture = fixture;
        ResetState();
    }

    public void ResetState()
    {
        using var connection = Fixture.Services.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        connection.Execute("DELETE FROM GoodsReceipts;");
        connection.Execute("DELETE FROM PurchaseOrderItems;");
        connection.Execute("DELETE FROM PurchaseOrders;");
        connection.Execute("DELETE FROM InventoryTransactions;");
        connection.Execute("DELETE FROM InventoryItems;");
        connection.Execute("DELETE FROM SaleItems;");
        connection.Execute("DELETE FROM Receipts;");
        connection.Execute("DELETE FROM Sales;");
        connection.Execute("DELETE FROM EventStore;");
    }

    protected IReportingReadStore ReadStore => Fixture.Services.GetRequiredService<IReportingReadStore>();

    protected static Sale CreateSampleSale(Guid? id = null, Guid? cashierId = null, Guid? customerId = null)
    {
        var sale = Sale.Create(id ?? Guid.NewGuid(), cashierId ?? Guid.NewGuid());
        if (customerId.HasValue)
        {
            // Use reflection to set CustomerId since it might not be settable
            typeof(Sale).GetProperty("CustomerId")?.SetValue(sale, customerId.Value);
        }
        return sale;
    }
}
