using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Suppliers.Application.Contracts;

namespace RMS.Modules.Suppliers.Infrastructure.Persistence;

public sealed class SupplierReadStore : ISupplierReadStore
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SupplierReadStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<SupplierReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = BaseSelect + " WHERE s.Id = @Id;";
        return await connection.QueryFirstOrDefaultAsync<SupplierReadModel>(sql, new { Id = id.ToString() });
    }

    public async Task<IReadOnlyList<SupplierReadModel>> SearchAsync(string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<SupplierReadModel>(
            BaseSelect + SearchWhere + " ORDER BY s.CompanyName ASC LIMIT 100;",
            SearchParameters(searchTerm, includeInactive));
        return rows.ToList();
    }

    public async Task<PagedResult<SupplierReadModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = SearchParameters(searchTerm, includeInactive, (pageNumber - 1) * pageSize, pageSize);

        var items = await connection.QueryAsync<SupplierReadModel>(
            BaseSelect + SearchWhere + " ORDER BY s.CompanyName ASC LIMIT @PageSize OFFSET @Offset;",
            parameters);

        var total = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Suppliers s " + SearchWhere,
            parameters);

        return new PagedResult<SupplierReadModel>(items.ToList(), pageNumber, pageSize, total);
    }

    public async Task<IReadOnlyList<SupplierProductReadModel>> GetSupplierProductsAsync(Guid supplierId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Future integration: join with PurchaseOrders to derive supplier-product catalog.
        // For now return empty list to avoid cross-module coupling.
        return new List<SupplierProductReadModel>();
    }

    public async Task<SupplierStatisticsReadModel?> GetStatisticsAsync(Guid supplierId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Future integration: compute from PurchaseOrders.
        // For now return empty stats if supplier exists.
        var exists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Suppliers WHERE Id = @Id;", new { Id = supplierId.ToString() });
        if (exists == 0) return null;

        return new SupplierStatisticsReadModel(0, 0, null, null);
    }

    public async Task<bool> PhoneNumberExistsAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var normalized = phoneNumber.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Suppliers WHERE PhoneNumber = @PhoneNumber;",
            new { PhoneNumber = normalized });
        return count > 0;
    }

    public async Task<bool> EmailExistsAsync(string? email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        using var connection = _connectionFactory.CreateConnection();
        var normalized = email.Trim().ToLowerInvariant();
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Suppliers WHERE Email = @Email;",
            new { Email = normalized });
        return count > 0;
    }

    public async Task<bool> VatNumberExistsAsync(string? vatNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vatNumber)) return false;
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Suppliers WHERE VatNumber = @VatNumber;",
            new { VatNumber = vatNumber.Trim() });
        return count > 0;
    }

    private const string BaseSelect = """
        SELECT
            s.Id,
            s.SupplierCode,
            s.CompanyName,
            s.ContactPerson,
            s.PhoneNumber,
            s.Email,
            s.VatNumber,
            s.Street,
            s.City,
            s.PostalCode,
            s.Country,
            CASE s.Status WHEN 0 THEN 'Active' WHEN 1 THEN 'Inactive' END AS Status,
            s.CreatedAt,
            s.UpdatedAt
        FROM Suppliers s
        """;

    private const string SearchWhere = """
         WHERE (@IncludeInactive = 1 OR s.Status = 0)
           AND (@SearchTerm IS NULL
                OR s.CompanyName LIKE @SearchPattern
                OR s.SupplierCode LIKE @SearchPattern
                OR s.ContactPerson LIKE @SearchPattern
                OR s.PhoneNumber LIKE @SearchPattern
                OR s.Email LIKE @SearchPattern
                OR s.City LIKE @SearchPattern)
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
