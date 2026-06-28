using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Customers.Application.Contracts;

namespace RMS.Modules.Customers.Infrastructure.Persistence;

public sealed class CustomerReadStore : ICustomerReadStore
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CustomerReadStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CustomerReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, CustomerCode, FirstName, LastName, PhoneNumber, Email,
                   Street, City, PostalCode, Country,
                   CASE Status WHEN 0 THEN 'Active' WHEN 1 THEN 'Inactive' END AS Status,
                   CreatedAt, UpdatedAt
            FROM Customers WHERE Id = @Id;
            """;
        return await connection.QueryFirstOrDefaultAsync<CustomerReadModel>(sql, new { Id = id });
    }

    public async Task<CustomerReadModel?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, CustomerCode, FirstName, LastName, PhoneNumber, Email,
                   Street, City, PostalCode, Country,
                   CASE Status WHEN 0 THEN 'Active' WHEN 1 THEN 'Inactive' END AS Status,
                   CreatedAt, UpdatedAt
            FROM Customers WHERE PhoneNumber = @PhoneNumber;
            """;
        var trimmed = phoneNumber.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        return await connection.QueryFirstOrDefaultAsync<CustomerReadModel>(sql, new { PhoneNumber = trimmed });
    }

    public async Task<CustomerReadModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, CustomerCode, FirstName, LastName, PhoneNumber, Email,
                   Street, City, PostalCode, Country,
                   CASE Status WHEN 0 THEN 'Active' WHEN 1 THEN 'Inactive' END AS Status,
                   CreatedAt, UpdatedAt
            FROM Customers WHERE Email = @Email;
            """;
        return await connection.QueryFirstOrDefaultAsync<CustomerReadModel>(sql, new { Email = email.Trim().ToLowerInvariant() });
    }

    public async Task<CustomerReadModel?> GetByCustomerCodeAsync(string customerCode, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, CustomerCode, FirstName, LastName, PhoneNumber, Email,
                   Street, City, PostalCode, Country,
                   CASE Status WHEN 0 THEN 'Active' WHEN 1 THEN 'Inactive' END AS Status,
                   CreatedAt, UpdatedAt
            FROM Customers WHERE CustomerCode = @CustomerCode;
            """;
        return await connection.QueryFirstOrDefaultAsync<CustomerReadModel>(sql, new { CustomerCode = customerCode.Trim() });
    }

    public async Task<IReadOnlyList<CustomerReadModel>> SearchAsync(string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var trimmed = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
        var parameters = new
        {
            IncludeInactive = includeInactive ? 1 : 0,
            SearchTerm = trimmed,
            SearchPattern = trimmed is null ? null : $"%{trimmed}%"
        };

        const string sql = """
            SELECT Id, CustomerCode, FirstName, LastName, PhoneNumber, Email,
                   Street, City, PostalCode, Country,
                   CASE Status WHEN 0 THEN 'Active' WHEN 1 THEN 'Inactive' END AS Status,
                   CreatedAt, UpdatedAt
            FROM Customers
            WHERE (@IncludeInactive = 1 OR Status = 0)
              AND (@SearchTerm IS NULL
                   OR CustomerCode LIKE @SearchPattern
                   OR FirstName LIKE @SearchPattern
                   OR LastName LIKE @SearchPattern
                   OR PhoneNumber LIKE @SearchPattern
                   OR Email LIKE @SearchPattern
                   OR City LIKE @SearchPattern)
            ORDER BY LastName, FirstName;
            """;

        var rows = await connection.QueryAsync<CustomerReadModel>(sql, parameters);
        return rows.ToList();
    }

    public async Task<PagedResult<CustomerReadModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var offset = (pageNumber - 1) * pageSize;
        var trimmed = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
        var parameters = new
        {
            IncludeInactive = includeInactive ? 1 : 0,
            SearchTerm = trimmed,
            SearchPattern = trimmed is null ? null : $"%{trimmed}%",
            Offset = offset,
            PageSize = pageSize
        };

        const string whereClause = """
            WHERE (@IncludeInactive = 1 OR Status = 0)
              AND (@SearchTerm IS NULL
                   OR CustomerCode LIKE @SearchPattern
                   OR FirstName LIKE @SearchPattern
                   OR LastName LIKE @SearchPattern
                   OR PhoneNumber LIKE @SearchPattern
                   OR Email LIKE @SearchPattern
                   OR City LIKE @SearchPattern)
            """;

        var items = await connection.QueryAsync<CustomerReadModel>(
            $"SELECT Id, CustomerCode, FirstName, LastName, PhoneNumber, Email, Street, City, PostalCode, Country, CASE Status WHEN 0 THEN 'Active' WHEN 1 THEN 'Inactive' END AS Status, CreatedAt, UpdatedAt FROM Customers {whereClause} ORDER BY LastName, FirstName LIMIT @PageSize OFFSET @Offset;",
            parameters);

        var total = await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(1) FROM Customers {whereClause};",
            parameters);

        return new PagedResult<CustomerReadModel>(items.ToList(), pageNumber, pageSize, total);
    }

    public async Task<IReadOnlyList<CustomerPurchaseHistoryItem>> GetPurchaseHistoryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Sales module stores customer association in Sales table; if CustomerId column does not exist yet, this returns empty.
        const string sql = """
            SELECT Id as SaleId, SaleNumber, SaleDate, TotalAmount,
                   CASE Status WHEN 0 THEN 'Pending' WHEN 1 THEN 'Completed' WHEN 2 THEN 'Refunded' END AS Status
            FROM Sales
            WHERE CustomerId = @CustomerId
            ORDER BY SaleDate DESC;
            """;
        var rows = await connection.QueryAsync<CustomerPurchaseHistoryItem>(sql, new { CustomerId = customerId });
        return rows.ToList();
    }

    public async Task<CustomerStatistics?> GetStatisticsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT COUNT(1) as TotalSales, COALESCE(SUM(TotalAmount), 0) as TotalSpent,
                   COALESCE(AVG(TotalAmount), 0) as AverageOrderValue,
                   MIN(SaleDate) as FirstPurchaseDate, MAX(SaleDate) as LastPurchaseDate
            FROM Sales
            WHERE CustomerId = @CustomerId AND Status = 1;
            """;
        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { CustomerId = customerId });
        if (row is null) return null;

        var customer = await GetByIdAsync(customerId, cancellationToken);
        if (customer is null) return null;

        return new CustomerStatistics(
            customerId,
            customer.CustomerCode,
            customer.FullName,
            (int)row.TotalSales,
            (decimal)row.TotalSpent,
            (decimal)row.AverageOrderValue,
            row.FirstPurchaseDate is null ? (DateTime?)null : (DateTime)row.FirstPurchaseDate,
            row.LastPurchaseDate is null ? (DateTime?)null : (DateTime)row.LastPurchaseDate);
    }

    public async Task<bool> HasSalesAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM Sales WHERE CustomerId = @CustomerId;";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { CustomerId = customerId });
        return count > 0;
    }
}
