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
using RMS.Modules.Customers.Application.Contracts;
using RMS.Modules.Customers.Domain.Entities;
using RMS.Modules.Customers.Infrastructure;
using RMS.Modules.Customers.Infrastructure.Migrations;
using RMS.Modules.Identity.Infrastructure;
using RMS.Modules.Identity.Infrastructure.Migrations;
using RMS.Modules.Products.Infrastructure;
using RMS.Modules.Products.Infrastructure.Migrations;
using RMS.Modules.Sales.Infrastructure;
using RMS.Modules.Sales.Infrastructure.Migrations;
using Xunit;

namespace RMS.IntegrationTests.Customers;

public class CustomerTestDatabaseFixture : IDisposable
{
    private readonly string _dbFilePath;
    private readonly ServiceProvider _serviceProvider;

    public CustomerTestDatabaseFixture()
    {
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteNullableGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));

        _dbFilePath = Path.Combine(Path.GetTempPath(), $"rms_customers_test_{Guid.NewGuid():N}.db");

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

        services.AddCustomersModule();
        services.AddCustomersInfrastructure();
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(
                    typeof(InitialIdentityMigration).Assembly,
                    typeof(CreateProductsTablesMigration).Assembly,
                    typeof(CreateSalesTablesMigration).Assembly,
                    typeof(CreateCustomersTableMigration).Assembly).For.Migrations()
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

public class CustomerIntegrationTestBase
{
    protected readonly CustomerTestDatabaseFixture Fixture;

    public CustomerIntegrationTestBase(CustomerTestDatabaseFixture fixture)
    {
        Fixture = fixture;
        ResetState();
    }

    public void ResetState()
    {
        using var connection = Fixture.Services.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        connection.Execute("DELETE FROM EventStore;");
        connection.Execute("DELETE FROM Customers;");
    }

    protected ICustomerReadStore ReadStore => Fixture.Services.GetRequiredService<ICustomerReadStore>();
    protected ICustomerWriteStore WriteStore => Fixture.Services.GetRequiredService<ICustomerWriteStore>();

    protected static Customer CreateSampleCustomer(
        Guid? id = null,
        string firstName = "John",
        string lastName = "Doe",
        string phone = "+1234567890",
        string? email = "john@example.com",
        string? street = "123 Main St",
        string? city = "New York")
    {
        var phoneNumber = PhoneNumber.Create(phone);
        var emailObj = email is not null ? Email.Create(email) : null;
        var address = Address.Create(street!, city!, "10001", "USA");
        return Customer.Create(id ?? Guid.NewGuid(), firstName, lastName, phoneNumber, emailObj, address);
    }
}
