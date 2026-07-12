using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Audit.Application.Contracts;
using RMS.Modules.Audit.Domain.Entities;

namespace RMS.Modules.Audit.Infrastructure.Persistence;

public sealed class AuditWriteStore : IAuditWriteStore
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AuditWriteStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO AuditLogs
                (AuditId, Timestamp, UserId, UserName, Module, Action, Entity, EntityId, OldValue, NewValue, MachineName, ApplicationVersion)
            VALUES
                (@AuditId, @Timestamp, @UserId, @UserName, @Module, @Action, @Entity, @EntityId, @OldValue, @NewValue, @MachineName, @ApplicationVersion);
            """;
        await connection.ExecuteAsync(sql, auditLog);
    }
}
