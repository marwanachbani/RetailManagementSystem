using Dapper;
using MediatR;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.Results;

namespace RMS.WPF.Dashboard.Queries.GetRecentActivities;

public sealed class GetRecentActivitiesHandler : IRequestHandler<GetRecentActivitiesQuery, Result<IReadOnlyList<ActivityDto>>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetRecentActivitiesHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<IReadOnlyList<ActivityDto>>> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT 'Sale' AS ActivityType,
                   'Sale ' || SaleNumber || ' — ' || printf('%.2f', TotalAmount) AS Description,
                   CreatedAt AS Timestamp,
                   '💰' AS IconGlyph
            FROM Sales
            UNION ALL
            SELECT 'Purchase' AS ActivityType,
                   'PO ' || PurchaseNumber || ' — ' || SupplierName AS Description,
                   CreatedAt AS Timestamp,
                   '📦' AS IconGlyph
            FROM PurchaseOrders
            UNION ALL
            SELECT 'Stock' AS ActivityType,
                   'Stock adjusted for product ' || CAST(ProductId AS TEXT) || ' (' || ChangeAmount || ')' AS Description,
                   Timestamp AS Timestamp,
                   '📊' AS IconGlyph
            FROM InventoryTransactions
            UNION ALL
            SELECT 'Customer' AS ActivityType,
                   'New customer ' || FirstName || ' ' || LastName AS Description,
                   CreatedAt AS Timestamp,
                   '👤' AS IconGlyph
            FROM Customers
            UNION ALL
            SELECT 'Product' AS ActivityType,
                   'Product ' || Name || ' added' AS Description,
                   CreatedAt AS Timestamp,
                   '🏷️' AS IconGlyph
            FROM Products
            ORDER BY Timestamp DESC
            LIMIT @Limit;
            """;

        var rows = await connection.QueryAsync<ActivityDto>(sql, new { request.Limit });
        return Result.Success<IReadOnlyList<ActivityDto>>(rows.ToList());
    }
}
