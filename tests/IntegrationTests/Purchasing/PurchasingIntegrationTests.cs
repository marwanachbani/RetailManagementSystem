using Dapper;
using FluentAssertions;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.EventStore;
using RMS.BuildingBlocks.Persistence;
using RMS.Modules.Purchasing.Application;
using RMS.Modules.Purchasing.Application.CancelPurchaseOrder;
using RMS.Modules.Purchasing.Application.CompletePurchase;
using RMS.Modules.Purchasing.Application.CreatePurchaseOrder;
using RMS.Modules.Purchasing.Application.GetPurchaseOrder;
using RMS.Modules.Purchasing.Application.ReceiveGoods;
using RMS.Modules.Purchasing.Application.SearchPurchaseOrders;
using RMS.Modules.Purchasing.Infrastructure;
using Xunit;

namespace RMS.IntegrationTests.Purchasing;

public class PurchasingIntegrationTests : IDisposable
{
    private readonly string _dbPath;
    private readonly IServiceProvider _services;

    public PurchasingIntegrationTests()
    {
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteNullableGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));

        _dbPath = Path.Combine(Path.GetTempPath(), $"rms_test_purchasing_{Guid.NewGuid()}.db");
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        }.ToString();

        var sc = new ServiceCollection();
        sc.AddSingleton<IDbConnectionFactory>(_ => new TestConnectionFactory(connectionString));
        sc.AddSingleton<IEventStore, SqliteEventStore>();
        sc.AddSingleton<IEventBus, TestEventBus>();
        sc.AddPurchasingInfrastructure();
        sc.AddPurchasingModule();
        _services = sc.BuildServiceProvider();

        // Ensure tables exist by running a simple migration check
        using var conn = _services.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS EventStore (
                EventId TEXT PRIMARY KEY,
                AggregateId TEXT NOT NULL,
                AggregateType TEXT NOT NULL,
                EventType TEXT NOT NULL,
                PayloadJson TEXT NOT NULL,
                OccurredOn TEXT NOT NULL,
                Version INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS PurchaseOrders (
                Id TEXT PRIMARY KEY,
                PurchaseNumber TEXT NOT NULL UNIQUE,
                SupplierId TEXT NOT NULL,
                SupplierName TEXT NOT NULL,
                OrderDate TEXT NOT NULL,
                Status INTEGER NOT NULL DEFAULT 0,
                SubTotal REAL NOT NULL DEFAULT 0,
                TaxAmount REAL NOT NULL DEFAULT 0,
                TotalAmount REAL NOT NULL DEFAULT 0,
                TaxPercentage REAL NOT NULL DEFAULT 0,
                CompletedAt TEXT,
                CancelledAt TEXT,
                CreatedAt TEXT NOT NULL,
                Notes TEXT,
                SupplierInvoiceNumber TEXT
            );
            CREATE TABLE IF NOT EXISTS PurchaseOrderItems (
                Id TEXT PRIMARY KEY,
                PurchaseOrderId TEXT NOT NULL,
                ProductId TEXT NOT NULL,
                ProductName TEXT NOT NULL,
                Quantity INTEGER NOT NULL,
                UnitCost REAL NOT NULL,
                TotalCost REAL NOT NULL,
                ReceivedQuantity INTEGER NOT NULL DEFAULT 0
            );
            CREATE TABLE IF NOT EXISTS GoodsReceipts (
                Id TEXT PRIMARY KEY,
                PurchaseOrderId TEXT NOT NULL,
                ProductId TEXT NOT NULL,
                QuantityReceived INTEGER NOT NULL,
                ReceivedAt TEXT NOT NULL,
                BatchNumber TEXT,
                ExpiryDate TEXT
            );
            """);
    }

    public void Dispose()
    {
        try { (_services as IDisposable)?.Dispose(); } catch { }
        try { File.Delete(_dbPath); } catch { }
    }

    [Fact]
    public async Task CreatePurchaseOrder_Should_Persist_Order()
    {
        var mediator = _services.GetRequiredService<IMediator>();
        var command = new CreatePurchaseOrderCommand(
            Guid.NewGuid(), "Test Supplier", "Notes", 10.00m,
            new List<CreatePurchaseOrderItemDto>
            {
                new(Guid.NewGuid(), "Product A", 5, 10.00m)
            });

        var result = await mediator.Send(command);
        result.IsSuccess.Should().BeTrue();

        var query = new GetPurchaseOrderQuery(result.Value);
        var orderResult = await mediator.Send(query);
        orderResult.IsSuccess.Should().BeTrue();
        orderResult.Value.SupplierName.Should().Be("Test Supplier");
        orderResult.Value.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task CancelPurchaseOrder_Should_Update_Status()
    {
        var mediator = _services.GetRequiredService<IMediator>();
        var create = new CreatePurchaseOrderCommand(
            Guid.NewGuid(), "Test Supplier", null, 0,
            new List<CreatePurchaseOrderItemDto> { new(Guid.NewGuid(), "Product A", 1, 1.00m) });
        var created = await mediator.Send(create);

        var cancel = new CancelPurchaseOrderCommand(created.Value);
        var cancelResult = await mediator.Send(cancel);
        cancelResult.IsSuccess.Should().BeTrue();

        var query = new GetPurchaseOrderQuery(created.Value);
        var order = await mediator.Send(query);
        order.Value.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task ReceiveGoods_Should_Update_ReceivedQuantity()
    {
        var mediator = _services.GetRequiredService<IMediator>();
        var productId = Guid.NewGuid();
        var create = new CreatePurchaseOrderCommand(
            Guid.NewGuid(), "Test Supplier", null, 0,
            new List<CreatePurchaseOrderItemDto> { new(productId, "Product A", 10, 1.00m) });
        var created = await mediator.Send(create);

        var receive = new ReceiveGoodsCommand(created.Value, productId, 5, null, null);
        var receiveResult = await mediator.Send(receive);
        receiveResult.IsSuccess.Should().BeTrue();

        var query = new GetPurchaseOrderQuery(created.Value);
        var order = await mediator.Send(query);
        order.Value.Items.First().ReceivedQuantity.Should().Be(5);
    }

    [Fact]
    public async Task CompletePurchase_Should_Set_Status_To_Completed()
    {
        var mediator = _services.GetRequiredService<IMediator>();
        var create = new CreatePurchaseOrderCommand(
            Guid.NewGuid(), "Test Supplier", null, 0,
            new List<CreatePurchaseOrderItemDto> { new(Guid.NewGuid(), "Product A", 1, 1.00m) });
        var created = await mediator.Send(create);

        var complete = new CompletePurchaseCommand(created.Value);
        var completeResult = await mediator.Send(complete);
        completeResult.IsSuccess.Should().BeTrue();

        var query = new GetPurchaseOrderQuery(created.Value);
        var order = await mediator.Send(query);
        order.Value.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task SearchPurchaseOrders_Should_Return_Results()
    {
        var mediator = _services.GetRequiredService<IMediator>();
        var create = new CreatePurchaseOrderCommand(
            Guid.NewGuid(), "Searchable Supplier", null, 0,
            new List<CreatePurchaseOrderItemDto> { new(Guid.NewGuid(), "Product A", 1, 1.00m) });
        await mediator.Send(create);

        var query = new SearchPurchaseOrdersQuery("Searchable", null);
        var result = await mediator.Send(query);
        result.IsSuccess.Should().BeTrue();
        result.Value.Count.Should().BeGreaterThan(0);
    }

    private class TestConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        public TestConnectionFactory(string connectionString) => _connectionString = connectionString;
        public System.Data.IDbConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }

    private class TestEventBus : IEventBus
    {
        public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent
        {
            return Task.CompletedTask;
        }
    }
}

internal static class DapperExtensions
{
    public static int Execute(this System.Data.IDbConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        return cmd.ExecuteNonQuery();
    }
}
