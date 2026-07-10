using Dapper;
using MediatR;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.Results;

namespace RMS.WPF.Dashboard.Queries.GetRecentSales;

public sealed class GetRecentSalesHandler : IRequestHandler<GetRecentSalesQuery, Result<IReadOnlyList<RecentSaleDto>>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetRecentSalesHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<IReadOnlyList<RecentSaleDto>>> Handle(GetRecentSalesQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, SaleNumber, TotalAmount, SaleDate,
                   CASE Status WHEN 0 THEN 'Pending' WHEN 1 THEN 'Completed' WHEN 2 THEN 'Refunded' END AS Status
            FROM Sales
            ORDER BY CreatedAt DESC
            LIMIT @Limit;
            """;
        var rows = await connection.QueryAsync<RecentSaleDto>(sql, new { request.Limit });
        return Result.Success<IReadOnlyList<RecentSaleDto>>(rows.ToList());
    }
}
