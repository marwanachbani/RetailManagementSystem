using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Inventory.Application.Contracts;

namespace RMS.Modules.Inventory.Infrastructure.Persistence;

public sealed class InventoryReadStore : IInventoryReadStore
{
    private readonly IDbConnectionFactory _connectionFactory;

    public InventoryReadStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<InventoryItemReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT Id, ProductId, CurrentQuantity, IsActive, CreatedAt, UpdatedAt, LowStockThreshold FROM InventoryItems WHERE Id = @Id;";
        return await connection.QueryFirstOrDefaultAsync<InventoryItemReadModel>(sql, new { Id = id });
    }

    public async Task<InventoryItemReadModel?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT Id, ProductId, CurrentQuantity, IsActive, CreatedAt, UpdatedAt, LowStockThreshold FROM InventoryItems WHERE ProductId = @ProductId;";
        return await connection.QueryFirstOrDefaultAsync<InventoryItemReadModel>(sql, new { ProductId = productId });
    }

    public async Task<IReadOnlyList<InventoryItemReadModel>> GetLowStockItemsAsync(int threshold, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, ProductId, CurrentQuantity, IsActive, CreatedAt, UpdatedAt, LowStockThreshold
            FROM InventoryItems
            WHERE CurrentQuantity < @Threshold AND IsActive = 1
            ORDER BY CurrentQuantity ASC;
            """;
        var rows = await connection.QueryAsync<InventoryItemReadModel>(sql, new { Threshold = threshold });
        return rows.ToList();
    }

    public async Task<IReadOnlyList<InventoryTransactionReadModel>> GetHistoryAsync(Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, InventoryItemId, ProductId, QuantityBefore, QuantityAfter, ChangeAmount, Reason, UserId, Timestamp
            FROM InventoryTransactions
            WHERE InventoryItemId = @InventoryItemId
            ORDER BY Timestamp DESC;
            """;
        var rows = await connection.QueryAsync<InventoryTransactionReadModel>(sql, new { InventoryItemId = inventoryItemId });
        return rows.ToList();
    }

    public async Task<PagedResult<InventoryItemReadModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var offset = (pageNumber - 1) * pageSize;
        var trimmed = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();

        var parameters = new
        {
            IncludeInactive = includeInactive ? 1 : 0,
            SearchTerm = trimmed,
            SearchPattern = trimmed is null ? null : $"%{trimmed}%",
            Offset = offset,
            PageSize = pageSize
        };

        const string whereClause = """
            WHERE (@IncludeInactive = 1 OR IsActive = 1)
              AND (@SearchTerm IS NULL
                   OR CAST(ProductId AS TEXT) LIKE @SearchPattern
                   OR CAST(Id AS TEXT) LIKE @SearchPattern)
            """;

        var items = await connection.QueryAsync<InventoryItemReadModel>(
            $"SELECT Id, ProductId, CurrentQuantity, IsActive, CreatedAt, UpdatedAt, LowStockThreshold FROM InventoryItems {whereClause} ORDER BY UpdatedAt DESC LIMIT @PageSize OFFSET @Offset;",
            parameters);

        var total = await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(1) FROM InventoryItems {whereClause};",
            parameters);

        return new PagedResult<InventoryItemReadModel>(items.ToList(), pageNumber, pageSize, total);
    }
}
