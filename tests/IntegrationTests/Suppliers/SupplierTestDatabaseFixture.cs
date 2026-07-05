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
using RMS.Modules.Products.Infrastructure;
using RMS.Modules.Products.Infrastructure.Migrations;
using RMS.Modules.Sales.Infrastructure;
using RMS.Modules.Sales.Infrastructure.Migrations;
using RMS.Modules.Suppliers.Application;
using RMS.Modules.Suppliers.Application.Contracts;
using RMS.Modules.Suppliers.Domain.Entities;
using RMS.Modules.Suppliers.Domain.ValueObjects;
using RMS.Modules.Suppliers.Infrastructure;
using RMS.Modules.Suppliers.Infrastructure.Migrations;
using Xunit;

namespace RMS.IntegrationTests.Suppliers;

public class SupplierTestDatabaseFixture : IDisposable
{
    private readonly string _dbFilePath;
    private readonly ServiceProvider _serviceProvider;

    public SupplierTestDatabaseFixture()
    {
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteNullableGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));

        _dbFilePath = Path.Combine(Path.GetTempPath(), $"rms_suppliers_test_{Guid.NewGuid():N}.db");

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

        services.AddSuppliersModule();
        services.AddSuppliersInfrastructure();
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(
                    typeof(InitialIdentityMigration).Assembly,
                    typeof(CreateProductsTablesMigration).Assembly,
                    typeof(CreateSalesTablesMigration).Assembly,
                    typeof(CreateSuppliersTableMigration).Assembly).For.Migrations()
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

public class SupplierIntegrationTestBase
{
    protected readonly SupplierTestDatabaseFixture Fixture;

    public SupplierIntegrationTestBase(SupplierTestDatabaseFixture fixture)
    {
        Fixture = fixture;
        ResetState();
    }

    public void ResetState()
    {
        using var connection = Fixture.Services.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        connection.Execute("DELETE FROM EventStore;");
        connection.Execute("DELETE FROM Suppliers;");
    }

    protected ISupplierReadStore ReadStore => Fixture.Services.GetRequiredService<ISupplierReadStore>();
    protected ISupplierWriteStore WriteStore => Fixture.Services.GetRequiredService<ISupplierWriteStore>();

    protected static Supplier CreateSampleSupplier(
        Guid? id = null,
        string companyName = "Acme Supplies",
        string phone = "+1234567890",
        string? email = "acme@example.com",
        string? street = "123 Main St",
        string? city = "New York")
    {
        var phoneNumber = PhoneNumber.Create(phone);
        var emailObj = email is not null ? Email.Create(email) : null;
        var address = Address.Create(street!, city!, "10001", "USA");
        return Supplier.Create(id ?? Guid.NewGuid(), companyName, phoneNumber, "John Doe", emailObj, $"VAT-{Guid.NewGuid():N}", address);
    }
}
