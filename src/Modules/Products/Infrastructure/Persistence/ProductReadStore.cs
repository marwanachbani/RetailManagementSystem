using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Products.Application.Contracts;

namespace RMS.Modules.Products.Infrastructure.Persistence;

public sealed class ProductReadStore : IProductReadStore
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductReadStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = BaseSelect + " WHERE p.Id = @Id;";
        return await connection.QueryFirstOrDefaultAsync<ProductReadModel>(sql, new { Id = id.ToString() });
    }

    public async Task<ProductReadModel?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = BaseSelect + " WHERE p.Barcode = @Barcode;";
        return await connection.QueryFirstOrDefaultAsync<ProductReadModel>(sql, new { Barcode = barcode.Trim() });
    }

    public async Task<IReadOnlyList<ProductReadModel>> SearchAsync(string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<ProductReadModel>(
            BaseSelect + SearchWhere + " ORDER BY p.Name ASC LIMIT 100;",
            SearchParameters(searchTerm, includeInactive));
        return rows.ToList();
    }

    public async Task<PagedResult<ProductReadModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = SearchParameters(searchTerm, includeInactive, (pageNumber - 1) * pageSize, pageSize);

        var items = await connection.QueryAsync<ProductReadModel>(
            BaseSelect + SearchWhere + " ORDER BY p.Name ASC LIMIT @PageSize OFFSET @Offset;",
            parameters);

        var total = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Products p JOIN Categories c ON c.Id = p.CategoryId " + SearchWhere,
            parameters);

        return new PagedResult<ProductReadModel>(items.ToList(), pageNumber, pageSize, total);
    }

    public async Task<IReadOnlyList<CategoryReadModel>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT Id, Name, Description FROM Categories ORDER BY Name ASC;";
        var rows = await connection.QueryAsync<CategoryReadModel>(sql);
        return rows.ToList();
    }

    private const string BaseSelect = """
        SELECT
            p.Id,
            p.ProductCode,
            p.Name,
            p.Description,
            p.Barcode,
            p.CategoryId,
            c.Name AS CategoryName,
            p.SalePrice,
            p.CostPrice,
            p.IsActive,
            p.CreatedAt,
            p.UpdatedAt
        FROM Products p
        JOIN Categories c ON c.Id = p.CategoryId
        """;

    private const string SearchWhere = """
         WHERE (@IncludeInactive = 1 OR p.IsActive = 1)
           AND (@SearchTerm IS NULL
                OR p.Name LIKE @SearchPattern
                OR p.Barcode LIKE @SearchPattern)
        """;

    private static object SearchParameters(string? searchTerm, bool includeInactive, int offset = 0, int pageSize = 25)
    {
        var trimmed = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
        return new
        {
            IncludeInactive = includeInactive ? 1 : 0,
            SearchTerm = trimmed,
            SearchPattern = trimmed is null ? null : $"%{trimmed}%",
            Offset = offset,
            PageSize = pageSize
        };
    }
}
