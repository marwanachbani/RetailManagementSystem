using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Backup.Application.Contracts;
using RMS.Modules.Backup.Domain.Entities;

namespace RMS.Modules.Backup.Infrastructure.Persistence;

public sealed class BackupStore : IBackupStore
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BackupStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertAsync(BackupHistory history, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO BackupHistory
                (Id, FileName, FilePath, BackupDate, Size, UserName, Version, Notes, Checksum)
            VALUES
                (@Id, @FileName, @FilePath, @BackupDate, @Size, @UserName, @Version, @Notes, @Checksum);
            """;
        await connection.ExecuteAsync(sql, history);
    }

    public async Task<IReadOnlyList<BackupHistory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, FileName, FilePath, BackupDate, Size, UserName, Version, Notes, Checksum
            FROM BackupHistory
            ORDER BY BackupDate DESC;
            """;
        var rows = await connection.QueryAsync<BackupHistory>(sql);
        return rows.ToList();
    }

    public async Task<BackupHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, FileName, FilePath, BackupDate, Size, UserName, Version, Notes, Checksum
            FROM BackupHistory
            WHERE Id = @Id;
            """;
        return await connection.QueryFirstOrDefaultAsync<BackupHistory>(sql, new { Id = id });
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM BackupHistory WHERE Id = @Id;", new { Id = id });
    }
}
