using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventStore;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Domain.Entities;

namespace RMS.Modules.Inventory.Infrastructure.Persistence;

public sealed class InventoryWriteStore : IInventoryWriteStore
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IEventStore _eventStore;

    public InventoryWriteStore(IDbConnectionFactory connectionFactory, IEventStore eventStore)
    {
        _connectionFactory = connectionFactory;
        _eventStore = eventStore;
    }

    public async Task InsertAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        const string sql = """
            INSERT INTO InventoryItems (Id, ProductId, CurrentQuantity, IsActive, CreatedAt, UpdatedAt, LowStockThreshold)
            VALUES (@Id, @ProductId, @CurrentQuantity, @IsActive, @CreatedAt, @UpdatedAt, @LowStockThreshold);
            """;

        await connection.ExecuteAsync(sql, ToItemParameters(inventoryItem), transaction);
        await AppendEventsAsync(inventoryItem, transaction, cancellationToken);
        await InsertTransactionsAsync(inventoryItem, connection, transaction, cancellationToken);
        transaction.Commit();
    }

    public async Task UpdateAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        const string sql = """
            UPDATE InventoryItems
            SET CurrentQuantity = @CurrentQuantity,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt,
                LowStockThreshold = @LowStockThreshold
            WHERE Id = @Id;
            """;

        await connection.ExecuteAsync(sql, ToItemParameters(inventoryItem), transaction);
        await AppendEventsAsync(inventoryItem, transaction, cancellationToken);
        await InsertTransactionsAsync(inventoryItem, connection, transaction, cancellationToken);
        transaction.Commit();
    }

    public async Task InsertTransactionAsync(InventoryTransaction transaction, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO InventoryTransactions (Id, InventoryItemId, ProductId, QuantityBefore, QuantityAfter, ChangeAmount, Reason, UserId, Timestamp)
            VALUES (@Id, @InventoryItemId, @ProductId, @QuantityBefore, @QuantityAfter, @ChangeAmount, @Reason, @UserId, @Timestamp);
            """;
        await connection.ExecuteAsync(sql, ToTransactionParameters(transaction));
    }

    private async Task AppendEventsAsync(InventoryItem inventoryItem, System.Data.IDbTransaction transaction, CancellationToken cancellationToken)
    {
        var version = 1;
        foreach (var domainEvent in inventoryItem.DomainEvents)
        {
            var storedEvent = SqliteEventStore.CreateStoredEvent(inventoryItem.Id, nameof(InventoryItem), domainEvent, version++);
            await _eventStore.AppendAsync(storedEvent, transaction, cancellationToken);
        }
    }

    private async Task InsertTransactionsAsync(InventoryItem inventoryItem, System.Data.IDbConnection connection, System.Data.IDbTransaction transaction, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO InventoryTransactions (Id, InventoryItemId, ProductId, QuantityBefore, QuantityAfter, ChangeAmount, Reason, UserId, Timestamp)
            VALUES (@Id, @InventoryItemId, @ProductId, @QuantityBefore, @QuantityAfter, @ChangeAmount, @Reason, @UserId, @Timestamp);
            """;

        foreach (var t in inventoryItem.Transactions)
        {
            await connection.ExecuteAsync(sql, ToTransactionParameters(t), transaction);
        }
    }

    private static object ToItemParameters(InventoryItem item) => new
    {
        Id = item.Id,
        ProductId = item.ProductId,
        CurrentQuantity = item.CurrentQuantity.Value,
        IsActive = item.IsActive ? 1 : 0,
        CreatedAt = item.CreatedAt.ToString("O"),
        UpdatedAt = item.UpdatedAt?.ToString("O"),
        LowStockThreshold = item.LowStockThreshold
    };

    private static object ToTransactionParameters(InventoryTransaction transaction) => new
    {
        Id = transaction.Id,
        InventoryItemId = transaction.InventoryItemId,
        ProductId = transaction.ProductId,
        QuantityBefore = transaction.QuantityBefore,
        QuantityAfter = transaction.QuantityAfter,
        ChangeAmount = transaction.ChangeAmount,
        Reason = transaction.Reason,
        UserId = transaction.UserId,
        Timestamp = transaction.Timestamp.ToString("O")
    };
}
