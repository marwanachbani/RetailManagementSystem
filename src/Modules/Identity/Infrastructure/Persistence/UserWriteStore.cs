using System.Data;
using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Identity.Application.Contracts;
using RMS.Modules.Identity.Domain.Entities;

namespace RMS.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// Dapper-based write store. Accepts the aggregate and maps it to raw SQL.
/// No repository abstraction — just a thin, feature-specific persistence class.
/// </summary>
public sealed class UserWriteStore : IUserWriteStore
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserWriteStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertAsync(User user, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Users (Id, UserName, Email, PasswordHash, FullName, Role, IsActive, CreatedAt)
            VALUES (@Id, @UserName, @Email, @PasswordHash, @FullName, @Role, @IsActive, @CreatedAt);";

        var parameters = new
        {
            Id = user.Id.ToString(),
            user.UserName,
            Email = user.Email.Value,
            PasswordHash = user.PasswordHash.Value,
            user.FullName,
            Role = user.Role.ToString(),
            IsActive = user.IsActive ? 1 : 0,
            CreatedAt = user.CreatedAt.ToString("O")
        };

        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Users
            SET UserName = @UserName,
                Email = @Email,
                PasswordHash = @PasswordHash,
                FullName = @FullName,
                Role = @Role,
                IsActive = @IsActive
            WHERE Id = @Id;";

        var parameters = new
        {
            Id = user.Id.ToString(),
            user.UserName,
            Email = user.Email.Value,
            PasswordHash = user.PasswordHash.Value,
            user.FullName,
            Role = user.Role.ToString(),
            IsActive = user.IsActive ? 1 : 0
        };

        await connection.ExecuteAsync(sql, parameters);
    }
}
