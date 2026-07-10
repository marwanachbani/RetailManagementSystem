using Dapper;
using MediatR;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.Results;

namespace RMS.WPF.Dashboard.Queries.GetQuickStatistics;

public sealed class GetQuickStatisticsHandler : IRequestHandler<GetQuickStatisticsQuery, Result<QuickStatistics>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetQuickStatisticsHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<QuickStatistics>> Handle(GetQuickStatisticsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var today = DateTime.UtcNow.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        const string sql = """
            SELECT
                (SELECT COUNT(1) FROM Sales WHERE SaleDate >= @StartOfWeek AND Status = 1) AS SalesThisWeek,
                (SELECT COUNT(1) FROM Sales WHERE SaleDate >= @FirstDayOfMonth AND Status = 1) AS SalesThisMonth,
                COALESCE((SELECT SUM(TotalAmount) FROM Sales WHERE SaleDate >= @StartOfWeek AND Status = 1), 0) AS RevenueThisWeek,
                COALESCE((SELECT SUM(TotalAmount) FROM Sales WHERE SaleDate >= @FirstDayOfMonth AND Status = 1), 0) AS RevenueThisMonth,
                (SELECT COUNT(1) FROM Customers WHERE CreatedAt >= @FirstDayOfMonth) AS NewCustomersThisMonth,
                (SELECT COUNT(1) FROM Products WHERE CreatedAt >= @FirstDayOfMonth) AS NewProductsThisMonth;
            """;

        var result = await connection.QueryFirstOrDefaultAsync<QuickStatistics>(sql, new { StartOfWeek = startOfWeek, FirstDayOfMonth = firstDayOfMonth });
        return Result.Success(result ?? new QuickStatistics());
    }
}
