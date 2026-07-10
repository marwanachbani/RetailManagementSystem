using Dapper;
using MediatR;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.Results;

namespace RMS.WPF.Dashboard.Queries.GetLowStockProducts;

public sealed class GetLowStockProductsHandler : IRequestHandler<GetLowStockProductsQuery, Result<IReadOnlyList<LowStockProductDto>>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetLowStockProductsHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<IReadOnlyList<LowStockProductDto>>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT p.Id AS ProductId, p.Name AS ProductName, i.CurrentQuantity, i.LowStockThreshold
            FROM InventoryItems i
            JOIN Products p ON i.ProductId = p.Id
            WHERE i.CurrentQuantity <= i.LowStockThreshold AND i.IsActive = 1
            ORDER BY i.CurrentQuantity ASC
            LIMIT @Limit;
            """;
        var rows = await connection.QueryAsync<LowStockProductDto>(sql, new { request.Limit });
        return Result.Success<IReadOnlyList<LowStockProductDto>>(rows.ToList());
    }
}
