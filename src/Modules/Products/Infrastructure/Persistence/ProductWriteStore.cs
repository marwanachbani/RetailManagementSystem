using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventStore;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Domain.Entities;

namespace RMS.Modules.Products.Infrastructure.Persistence;

public sealed class ProductWriteStore : IProductWriteStore
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IEventStore _eventStore;

    public ProductWriteStore(IDbConnectionFactory connectionFactory, IEventStore eventStore)
    {
        _connectionFactory = connectionFactory;
        _eventStore = eventStore;
    }

    public async Task InsertAsync(Product product, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        const string sql = """
            INSERT INTO Products (Id, ProductCode, Name, Description, Barcode, CategoryId, SalePrice, CostPrice, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Id, @ProductCode, @Name, @Description, @Barcode, @CategoryId, @SalePrice, @CostPrice, @IsActive, @CreatedAt, @UpdatedAt);
            """;

        await connection.ExecuteAsync(sql, ToParameters(product), transaction);
        await AppendEventsAsync(product, transaction, cancellationToken);
        transaction.Commit();
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        const string sql = """
            UPDATE Products
            SET Name = @Name,
                Description = @Description,
                Barcode = @Barcode,
                CategoryId = @CategoryId,
                SalePrice = @SalePrice,
                CostPrice = @CostPrice,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
            """;

        await connection.ExecuteAsync(sql, ToParameters(product), transaction);
        await AppendEventsAsync(product, transaction, cancellationToken);
        transaction.Commit();
    }

    private async Task AppendEventsAsync(Product product, System.Data.IDbTransaction transaction, CancellationToken cancellationToken)
    {
        var version = 1;
        foreach (var domainEvent in product.DomainEvents)
        {
            var storedEvent = SqliteEventStore.CreateStoredEvent(product.Id, nameof(Product), domainEvent, version++);
            await _eventStore.AppendAsync(storedEvent, transaction, cancellationToken);
        }
    }

    private static object ToParameters(Product product) => new
    {
        Id = product.Id.ToString(),
        product.ProductCode,
        product.Name,
        product.Description,
        Barcode = product.Barcode.Value,
        CategoryId = product.CategoryId.ToString(),
        SalePrice = product.SalePrice.Amount,
        CostPrice = product.CostPrice.Amount,
        IsActive = product.IsActive ? 1 : 0,
        CreatedAt = product.CreatedAt.ToString("O"),
        UpdatedAt = product.UpdatedAt?.ToString("O")
    };
}
