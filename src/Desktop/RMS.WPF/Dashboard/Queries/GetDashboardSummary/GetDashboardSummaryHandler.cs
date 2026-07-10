using Dapper;
using MediatR;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.Results;

namespace RMS.WPF.Dashboard.Queries.GetDashboardSummary;

public sealed class GetDashboardSummaryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<KpiSummary>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetDashboardSummaryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<KpiSummary>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var today = DateTime.UtcNow.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        const string sql = """
            SELECT
                (SELECT COUNT(1) FROM Sales WHERE date(SaleDate) = date(@Today) AND Status = 1) AS TodaysSales,
                COALESCE((SELECT SUM(TotalAmount) FROM Sales WHERE date(SaleDate) = date(@Today) AND Status = 1), 0) AS TodaysRevenue,
                COALESCE((SELECT SUM(TotalAmount) FROM Sales WHERE SaleDate >= @FirstDayOfMonth AND Status = 1), 0) AS MonthlyRevenue,
                (SELECT COUNT(1) FROM Products) AS TotalProducts,
                (SELECT COUNT(1) FROM Customers WHERE Status = 0) AS ActiveCustomers,
                (SELECT COUNT(1) FROM Suppliers WHERE Status = 0) AS ActiveSuppliers,
                (SELECT COUNT(1) FROM PurchaseOrders WHERE date(OrderDate) = date(@Today)) AS PurchaseOrdersToday,
                (SELECT COUNT(1) FROM InventoryItems i JOIN Products p ON i.ProductId = p.Id WHERE i.CurrentQuantity <= i.LowStockThreshold AND i.CurrentQuantity > 0 AND i.IsActive = 1) AS LowStockProducts,
                (SELECT COUNT(1) FROM InventoryItems i JOIN Products p ON i.ProductId = p.Id WHERE i.CurrentQuantity = 0 AND i.IsActive = 1) AS OutOfStockProducts,
                COALESCE((SELECT SUM(p.CostPrice * i.CurrentQuantity) FROM InventoryItems i JOIN Products p ON i.ProductId = p.Id WHERE i.IsActive = 1), 0) AS InventoryValue;
            """;

        var result = await connection.QueryFirstOrDefaultAsync<KpiSummary>(sql, new { Today = today, FirstDayOfMonth = firstDayOfMonth });
        return Result.Success(result ?? new KpiSummary());
    }
}
