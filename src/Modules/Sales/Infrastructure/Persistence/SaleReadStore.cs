using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Sales.Application.Contracts;

namespace RMS.Modules.Sales.Infrastructure.Persistence;

public sealed class SaleReadStore : ISaleReadStore
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SaleReadStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<SaleReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, SaleNumber, CashierId, SaleDate,
                   CASE Status WHEN 0 THEN 'Pending' WHEN 1 THEN 'Completed' WHEN 2 THEN 'Refunded' END AS Status,
                   SubTotal, DiscountAmount, TaxAmount, TotalAmount,
                   DiscountPercentage, TaxPercentage, CompletedAt, RefundedAt, CreatedAt, Notes
            FROM Sales WHERE Id = @Id;
            """;
        var sale = await connection.QueryFirstOrDefaultAsync<SaleReadModel>(sql, new { Id = id });
        if (sale is null) return null;

        var items = await GetItemsBySaleIdAsync(id, cancellationToken);
        return sale with { Items = items };
    }

    public async Task<IReadOnlyList<SaleItemReadModel>> GetItemsBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, ProductId, ProductName, Quantity, UnitPrice, TotalPrice
            FROM SaleItems WHERE SaleId = @SaleId;
            """;
        var rows = await connection.QueryAsync<SaleItemReadModel>(sql, new { SaleId = saleId });
        return rows.ToList();
    }

    public async Task<ReceiptReadModel?> GetReceiptBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, SaleId, ReceiptNumber, PdfPath, GeneratedAt, StoreName, CashierName, TotalAmount
            FROM Receipts WHERE SaleId = @SaleId;
            """;
        return await connection.QueryFirstOrDefaultAsync<ReceiptReadModel>(sql, new { SaleId = saleId });
    }

    public async Task<PagedResult<SaleReadModel>> GetPagedAsync(int pageNumber, int pageSize, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var offset = (pageNumber - 1) * pageSize;

        var whereClause = "";
        if (fromDate.HasValue && toDate.HasValue)
            whereClause = "WHERE SaleDate >= @FromDate AND SaleDate < @ToDate";
        else if (fromDate.HasValue)
            whereClause = "WHERE SaleDate >= @FromDate";
        else if (toDate.HasValue)
            whereClause = "WHERE SaleDate < @ToDate";

        var parameters = new { FromDate = fromDate, ToDate = toDate?.AddDays(1), Offset = offset, PageSize = pageSize };

        var sales = await connection.QueryAsync<SaleReadModel>(
            $"SELECT Id, SaleNumber, CashierId, SaleDate, CASE Status WHEN 0 THEN 'Pending' WHEN 1 THEN 'Completed' WHEN 2 THEN 'Refunded' END AS Status, SubTotal, DiscountAmount, TaxAmount, TotalAmount, DiscountPercentage, TaxPercentage, CompletedAt, RefundedAt, CreatedAt, Notes FROM Sales {whereClause} ORDER BY CreatedAt DESC LIMIT @PageSize OFFSET @Offset;",
            parameters);

        var total = await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(1) FROM Sales {whereClause};",
            parameters);

        var result = new List<SaleReadModel>();
        foreach (var sale in sales)
        {
            var items = await GetItemsBySaleIdAsync(sale.Id, cancellationToken);
            result.Add(sale with { Items = items });
        }

        return new PagedResult<SaleReadModel>(result, pageNumber, pageSize, total);
    }

    public async Task<IReadOnlyList<SaleReadModel>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, SaleNumber, CashierId, SaleDate,
                   CASE Status WHEN 0 THEN 'Pending' WHEN 1 THEN 'Completed' WHEN 2 THEN 'Refunded' END AS Status,
                   SubTotal, DiscountAmount, TaxAmount, TotalAmount,
                   DiscountPercentage, TaxPercentage, CompletedAt, RefundedAt, CreatedAt, Notes
            FROM Sales WHERE date(SaleDate) = date(@Date) ORDER BY CreatedAt DESC;
            """;
        var sales = await connection.QueryAsync<SaleReadModel>(sql, new { Date = date });

        var result = new List<SaleReadModel>();
        foreach (var sale in sales)
        {
            var items = await GetItemsBySaleIdAsync(sale.Id, cancellationToken);
            result.Add(sale with { Items = items });
        }
        return result;
    }

    public async Task<DailySalesSummary> GetDailySummaryAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT COUNT(1) as TotalSales, COALESCE(SUM(TotalAmount), 0) as TotalRevenue,
                   COALESCE(SUM(DiscountAmount), 0) as TotalDiscounts, COALESCE(SUM(TaxAmount), 0) as TotalTaxes
            FROM Sales WHERE date(SaleDate) = date(@Date) AND Status = 1;
            """;
        var result = await connection.QueryFirstOrDefaultAsync<DailySalesSummary>(sql, new { Date = date });
        return result ?? new DailySalesSummary(date, 0, 0, 0, 0);
    }
}
