using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventStore;
using RMS.Modules.Sales.Application.Contracts;
using RMS.Modules.Sales.Domain.Entities;

namespace RMS.Modules.Sales.Infrastructure.Persistence;

public sealed class SaleWriteStore : ISaleWriteStore
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IEventStore _eventStore;

    public SaleWriteStore(IDbConnectionFactory connectionFactory, IEventStore eventStore)
    {
        _connectionFactory = connectionFactory;
        _eventStore = eventStore;
    }

    public async Task InsertAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        const string saleSql = """
            INSERT INTO Sales (Id, SaleNumber, CashierId, SaleDate, Status, SubTotal, DiscountAmount, TaxAmount, TotalAmount,
                               DiscountPercentage, TaxPercentage, CompletedAt, RefundedAt, CreatedAt, Notes)
            VALUES (@Id, @SaleNumber, @CashierId, @SaleDate, @Status, @SubTotal, @DiscountAmount, @TaxAmount, @TotalAmount,
                    @DiscountPercentage, @TaxPercentage, @CompletedAt, @RefundedAt, @CreatedAt, @Notes);
            """;

        await connection.ExecuteAsync(saleSql, ToSaleParameters(sale), transaction);
        await InsertItemsAsync(sale, connection, transaction, cancellationToken);
        await AppendEventsAsync(sale, transaction, cancellationToken);
        transaction.Commit();
    }

    public async Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        const string saleSql = """
            UPDATE Sales
            SET SaleNumber = @SaleNumber, CashierId = @CashierId, SaleDate = @SaleDate, Status = @Status,
                SubTotal = @SubTotal, DiscountAmount = @DiscountAmount, TaxAmount = @TaxAmount, TotalAmount = @TotalAmount,
                DiscountPercentage = @DiscountPercentage, TaxPercentage = @TaxPercentage,
                CompletedAt = @CompletedAt, RefundedAt = @RefundedAt, CreatedAt = @CreatedAt, Notes = @Notes
            WHERE Id = @Id;
            """;

        await connection.ExecuteAsync(saleSql, ToSaleParameters(sale), transaction);

        // Delete existing items and re-insert (simplest approach for SQLite)
        await connection.ExecuteAsync("DELETE FROM SaleItems WHERE SaleId = @SaleId;", new { SaleId = sale.Id }, transaction);
        await InsertItemsAsync(sale, connection, transaction, cancellationToken);

        await AppendEventsAsync(sale, transaction, cancellationToken);
        transaction.Commit();
    }

    public async Task InsertReceiptAsync(Receipt receipt, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO Receipts (Id, SaleId, ReceiptNumber, PdfPath, GeneratedAt, StoreName, CashierName, TotalAmount)
            VALUES (@Id, @SaleId, @ReceiptNumber, @PdfPath, @GeneratedAt, @StoreName, @CashierName, @TotalAmount);
            """;
        await connection.ExecuteAsync(sql, new
        {
            Id = receipt.Id,
            SaleId = receipt.SaleId,
            ReceiptNumber = receipt.ReceiptNumber,
            PdfPath = receipt.PdfPath,
            GeneratedAt = receipt.GeneratedAt.ToString("O"),
            StoreName = receipt.StoreName,
            CashierName = receipt.CashierName,
            TotalAmount = receipt.TotalAmount
        });
    }

    private async Task InsertItemsAsync(Sale sale, System.Data.IDbConnection connection, System.Data.IDbTransaction transaction, CancellationToken cancellationToken)
    {
        const string itemSql = """
            INSERT INTO SaleItems (Id, SaleId, ProductId, ProductName, Quantity, UnitPrice, TotalPrice)
            VALUES (@Id, @SaleId, @ProductId, @ProductName, @Quantity, @UnitPrice, @TotalPrice);
            """;

        foreach (var item in sale.Items)
        {
            await connection.ExecuteAsync(itemSql, new
            {
                Id = item.Id,
                SaleId = item.SaleId,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            }, transaction);
        }
    }

    private async Task AppendEventsAsync(Sale sale, System.Data.IDbTransaction transaction, CancellationToken cancellationToken)
    {
        var version = 1;
        foreach (var domainEvent in sale.DomainEvents)
        {
            var storedEvent = SqliteEventStore.CreateStoredEvent(sale.Id, nameof(Sale), domainEvent, version++);
            await _eventStore.AppendAsync(storedEvent, transaction, cancellationToken);
        }
    }

    private static object ToSaleParameters(Sale sale) => new
    {
        Id = sale.Id,
        SaleNumber = sale.SaleNumber,
        CashierId = sale.CashierId,
        SaleDate = sale.SaleDate.ToString("O"),
        Status = (int)sale.Status,
        SubTotal = sale.SubTotal,
        DiscountAmount = sale.DiscountAmount,
        TaxAmount = sale.TaxAmount,
        TotalAmount = sale.TotalAmount,
        DiscountPercentage = sale.DiscountPercentage,
        TaxPercentage = sale.TaxPercentage,
        CompletedAt = sale.CompletedAt?.ToString("O"),
        RefundedAt = sale.RefundedAt?.ToString("O"),
        CreatedAt = sale.CreatedAt.ToString("O"),
        Notes = sale.Notes
    };
}
