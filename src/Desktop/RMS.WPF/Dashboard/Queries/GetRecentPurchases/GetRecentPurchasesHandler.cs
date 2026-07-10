using Dapper;
using MediatR;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.Results;

namespace RMS.WPF.Dashboard.Queries.GetRecentPurchases;

public sealed class GetRecentPurchasesHandler : IRequestHandler<GetRecentPurchasesQuery, Result<IReadOnlyList<RecentPurchaseDto>>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetRecentPurchasesHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<IReadOnlyList<RecentPurchaseDto>>> Handle(GetRecentPurchasesQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, PurchaseNumber, SupplierName, TotalAmount, OrderDate,
                   CASE Status WHEN 0 THEN 'Draft' WHEN 1 THEN 'Submitted' WHEN 2 THEN 'PartiallyReceived' WHEN 3 THEN 'Completed' WHEN 4 THEN 'Cancelled' END AS Status
            FROM PurchaseOrders
            ORDER BY CreatedAt DESC
            LIMIT @Limit;
            """;
        var rows = await connection.QueryAsync<RecentPurchaseDto>(sql, new { request.Limit });
        return Result.Success<IReadOnlyList<RecentPurchaseDto>>(rows.ToList());
    }
}
