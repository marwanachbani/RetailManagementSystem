using System.Data;
using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Identity.Application.Contracts;

namespace RMS.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// Dapper-based read store. Returns lightweight DTOs; no domain logic.
/// </summary>
public sealed class UserReadStore : IUserReadStore
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserReadStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Users WHERE Id = @Id;";
        var row = await connection.QueryFirstOrDefaultAsync<UserRow>(sql, new { Id = id.ToString() });
        return row is null ? null : Map(row);
    }

    public async Task<UserReadModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Users WHERE Email = @Email;";
        var row = await connection.QueryFirstOrDefaultAsync<UserRow>(sql, new { Email = email.Trim().ToLowerInvariant() });
        return row is null ? null : Map(row);
    }

    public async Task<UserReadModel?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Users WHERE UserName = @UserName;";
        var row = await connection.QueryFirstOrDefaultAsync<UserRow>(sql, new { UserName = userName.Trim() });
        return row is null ? null : Map(row);
    }

    public async Task<IReadOnlyList<UserReadModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Users ORDER BY CreatedAt DESC;";
        var rows = await connection.QueryAsync<UserRow>(sql);
        return rows.Select(Map).ToList();
    }

    private static UserReadModel Map(UserRow row) => new(
        Guid.Parse(row.Id),
        row.UserName,
        row.Email,
        row.PasswordHash,
        row.FullName,
        row.Role,
        row.IsActive == 1,
        DateTime.Parse(row.CreatedAt));

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class UserRow
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int IsActive { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }
}
