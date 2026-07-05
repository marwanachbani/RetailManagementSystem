using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventStore;
using RMS.Modules.Purchasing.Application.Contracts;
using RMS.Modules.Purchasing.Domain.Entities;

namespace RMS.Modules.Purchasing.Infrastructure.Persistence;

public sealed class PurchaseWriteStore : IPurchaseOrderWriteStore
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IEventStore _eventStore;

    public PurchaseWriteStore(IDbConnectionFactory connectionFactory, IEventStore eventStore)
    {
        _connectionFactory = connectionFactory;
        _eventStore = eventStore;
    }

    public async Task InsertAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        const string orderSql = """
            INSERT INTO PurchaseOrders (Id, PurchaseNumber, SupplierId, SupplierName, OrderDate, Status, SubTotal, TaxAmount, TotalAmount, TaxPercentage, CompletedAt, CancelledAt, CreatedAt, Notes, SupplierInvoiceNumber)
            VALUES (@Id, @PurchaseNumber, @SupplierId, @SupplierName, @OrderDate, @Status, @SubTotal, @TaxAmount, @TotalAmount, @TaxPercentage, @CompletedAt, @CancelledAt, @CreatedAt, @Notes, @SupplierInvoiceNumber);
            """;

        await connection.ExecuteAsync(orderSql, ToOrderParameters(purchaseOrder), transaction);
        await InsertItemsAsync(purchaseOrder, connection, transaction, cancellationToken);
        await AppendEventsAsync(purchaseOrder, transaction, cancellationToken);
        transaction.Commit();
    }

    public async Task UpdateAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        const string orderSql = """
            UPDATE PurchaseOrders
            SET PurchaseNumber = @PurchaseNumber, SupplierId = @SupplierId, SupplierName = @SupplierName, OrderDate = @OrderDate,
                Status = @Status, SubTotal = @SubTotal, TaxAmount = @TaxAmount, TotalAmount = @TotalAmount,
                TaxPercentage = @TaxPercentage, CompletedAt = @CompletedAt, CancelledAt = @CancelledAt, CreatedAt = @CreatedAt,
                Notes = @Notes, SupplierInvoiceNumber = @SupplierInvoiceNumber
            WHERE Id = @Id;
            """;

        await connection.ExecuteAsync(orderSql, ToOrderParameters(purchaseOrder), transaction);

        await connection.ExecuteAsync("DELETE FROM PurchaseOrderItems WHERE PurchaseOrderId = @PurchaseOrderId;", new { PurchaseOrderId = purchaseOrder.Id }, transaction);
        await InsertItemsAsync(purchaseOrder, connection, transaction, cancellationToken);

        await AppendEventsAsync(purchaseOrder, transaction, cancellationToken);
        transaction.Commit();
    }

    public async Task InsertGoodsReceiptAsync(GoodsReceipt goodsReceipt, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO GoodsReceipts (Id, PurchaseOrderId, ProductId, QuantityReceived, ReceivedAt, BatchNumber, ExpiryDate)
            VALUES (@Id, @PurchaseOrderId, @ProductId, @QuantityReceived, @ReceivedAt, @BatchNumber, @ExpiryDate);
            """;
        await connection.ExecuteAsync(sql, new
        {
            Id = goodsReceipt.Id,
            PurchaseOrderId = goodsReceipt.PurchaseOrderId,
            ProductId = goodsReceipt.ProductId,
            QuantityReceived = goodsReceipt.QuantityReceived,
            ReceivedAt = goodsReceipt.ReceivedAt.ToString("O"),
            BatchNumber = goodsReceipt.BatchNumber,
            ExpiryDate = goodsReceipt.ExpiryDate?.ToString("O")
        });
    }

    private async Task InsertItemsAsync(PurchaseOrder purchaseOrder, System.Data.IDbConnection connection, System.Data.IDbTransaction transaction, CancellationToken cancellationToken)
    {
        const string itemSql = """
            INSERT INTO PurchaseOrderItems (Id, PurchaseOrderId, ProductId, ProductName, Quantity, UnitCost, TotalCost, ReceivedQuantity)
            VALUES (@Id, @PurchaseOrderId, @ProductId, @ProductName, @Quantity, @UnitCost, @TotalCost, @ReceivedQuantity);
            """;

        foreach (var item in purchaseOrder.Items)
        {
            await connection.ExecuteAsync(itemSql, new
            {
                Id = item.Id,
                PurchaseOrderId = item.PurchaseOrderId,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost,
                TotalCost = item.TotalCost,
                ReceivedQuantity = item.ReceivedQuantity
            }, transaction);
        }
    }

    private async Task AppendEventsAsync(PurchaseOrder purchaseOrder, System.Data.IDbTransaction transaction, CancellationToken cancellationToken)
    {
        var version = 1;
        foreach (var domainEvent in purchaseOrder.DomainEvents)
        {
            var storedEvent = SqliteEventStore.CreateStoredEvent(purchaseOrder.Id, nameof(PurchaseOrder), domainEvent, version++);
            await _eventStore.AppendAsync(storedEvent, transaction, cancellationToken);
        }
    }

    private static object ToOrderParameters(PurchaseOrder order) => new
    {
        Id = order.Id,
        PurchaseNumber = order.PurchaseNumber,
        SupplierId = order.SupplierId,
        SupplierName = order.SupplierName,
        OrderDate = order.OrderDate.ToString("O"),
        Status = (int)order.Status,
        SubTotal = order.SubTotal,
        TaxAmount = order.TaxAmount,
        TotalAmount = order.TotalAmount,
        TaxPercentage = order.TaxPercentage,
        CompletedAt = order.CompletedAt?.ToString("O"),
        CancelledAt = order.CancelledAt?.ToString("O"),
        CreatedAt = order.CreatedAt.ToString("O"),
        Notes = order.Notes,
        SupplierInvoiceNumber = order.SupplierInvoiceNumber
    };
}
