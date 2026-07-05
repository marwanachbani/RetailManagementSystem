using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Purchasing.Application.Contracts;

namespace RMS.Modules.Purchasing.Infrastructure.Persistence;

public sealed class PurchaseReadStore : IPurchaseOrderReadStore
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PurchaseReadStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PurchaseOrderReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, PurchaseNumber, SupplierId, SupplierName, OrderDate,
                   CASE Status WHEN 0 THEN 'Draft' WHEN 1 THEN 'Submitted' WHEN 2 THEN 'PartiallyReceived' WHEN 3 THEN 'Completed' WHEN 4 THEN 'Cancelled' END AS Status,
                   SubTotal, TaxAmount, TotalAmount, TaxPercentage, CompletedAt, CancelledAt, CreatedAt, Notes, SupplierInvoiceNumber
            FROM PurchaseOrders WHERE Id = @Id;
            """;
        var order = await connection.QueryFirstOrDefaultAsync<PurchaseOrderReadModel>(sql, new { Id = id });
        if (order is null) return null;

        var items = await GetItemsByPurchaseOrderIdAsync(id, cancellationToken);
        var receipts = await GetGoodsReceiptsByPurchaseOrderIdAsync(id, cancellationToken);
        return order with { Items = items, GoodsReceipts = receipts };
    }

    public async Task<IReadOnlyList<PurchaseOrderItemReadModel>> GetItemsByPurchaseOrderIdAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, PurchaseOrderId, ProductId, ProductName, Quantity, UnitCost, TotalCost, ReceivedQuantity
            FROM PurchaseOrderItems WHERE PurchaseOrderId = @PurchaseOrderId;
            """;
        var rows = await connection.QueryAsync<PurchaseOrderItemReadModel>(sql, new { PurchaseOrderId = purchaseOrderId });
        return rows.ToList();
    }

    public async Task<IReadOnlyList<GoodsReceiptReadModel>> GetGoodsReceiptsByPurchaseOrderIdAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, PurchaseOrderId, ProductId, QuantityReceived, ReceivedAt, BatchNumber, ExpiryDate
            FROM GoodsReceipts WHERE PurchaseOrderId = @PurchaseOrderId ORDER BY ReceivedAt DESC;
            """;
        var rows = await connection.QueryAsync<GoodsReceiptReadModel>(sql, new { PurchaseOrderId = purchaseOrderId });
        return rows.ToList();
    }

    public async Task<PagedResult<PurchaseOrderReadModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, int? statusFilter, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var offset = (pageNumber - 1) * pageSize;
        var trimmed = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();

        var parameters = new
        {
            StatusFilter = statusFilter,
            SearchTerm = trimmed,
            SearchPattern = trimmed is null ? null : $"%{trimmed}%",
            Offset = offset,
            PageSize = pageSize
        };

        const string whereClause = """
            WHERE (@StatusFilter IS NULL OR Status = @StatusFilter)
              AND (@SearchTerm IS NULL
                   OR PurchaseNumber LIKE @SearchPattern
                   OR SupplierName LIKE @SearchPattern
                   OR CAST(Id AS TEXT) LIKE @SearchPattern)
            """;

        var orders = await connection.QueryAsync<PurchaseOrderReadModel>(
            $"SELECT Id, PurchaseNumber, SupplierId, SupplierName, OrderDate, CASE Status WHEN 0 THEN 'Draft' WHEN 1 THEN 'Submitted' WHEN 2 THEN 'PartiallyReceived' WHEN 3 THEN 'Completed' WHEN 4 THEN 'Cancelled' END AS Status, SubTotal, TaxAmount, TotalAmount, TaxPercentage, CompletedAt, CancelledAt, CreatedAt, Notes, SupplierInvoiceNumber FROM PurchaseOrders {whereClause} ORDER BY CreatedAt DESC LIMIT @PageSize OFFSET @Offset;",
            parameters);

        var total = await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(1) FROM PurchaseOrders {whereClause};",
            parameters);

        var result = new List<PurchaseOrderReadModel>();
        foreach (var order in orders)
        {
            var items = await GetItemsByPurchaseOrderIdAsync(order.Id, cancellationToken);
            var receipts = await GetGoodsReceiptsByPurchaseOrderIdAsync(order.Id, cancellationToken);
            result.Add(order with { Items = items, GoodsReceipts = receipts });
        }

        return new PagedResult<PurchaseOrderReadModel>(result, pageNumber, pageSize, total);
    }

    public async Task<IReadOnlyList<PurchaseOrderReadModel>> SearchAsync(string? searchTerm, int? statusFilter, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var trimmed = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();

        var parameters = new
        {
            StatusFilter = statusFilter,
            SearchTerm = trimmed,
            SearchPattern = trimmed is null ? null : $"%{trimmed}%"
        };

        const string whereClause = """
            WHERE (@StatusFilter IS NULL OR Status = @StatusFilter)
              AND (@SearchTerm IS NULL
                   OR PurchaseNumber LIKE @SearchPattern
                   OR SupplierName LIKE @SearchPattern
                   OR CAST(Id AS TEXT) LIKE @SearchPattern)
            """;

        var orders = await connection.QueryAsync<PurchaseOrderReadModel>(
            $"SELECT Id, PurchaseNumber, SupplierId, SupplierName, OrderDate, CASE Status WHEN 0 THEN 'Draft' WHEN 1 THEN 'Submitted' WHEN 2 THEN 'PartiallyReceived' WHEN 3 THEN 'Completed' WHEN 4 THEN 'Cancelled' END AS Status, SubTotal, TaxAmount, TotalAmount, TaxPercentage, CompletedAt, CancelledAt, CreatedAt, Notes, SupplierInvoiceNumber FROM PurchaseOrders {whereClause} ORDER BY CreatedAt DESC;",
            parameters);

        var result = new List<PurchaseOrderReadModel>();
        foreach (var order in orders)
        {
            var items = await GetItemsByPurchaseOrderIdAsync(order.Id, cancellationToken);
            var receipts = await GetGoodsReceiptsByPurchaseOrderIdAsync(order.Id, cancellationToken);
            result.Add(order with { Items = items, GoodsReceipts = receipts });
        }

        return result;
    }
}
