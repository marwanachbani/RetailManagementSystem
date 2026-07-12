using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Audit.Application.Contracts;

namespace RMS.Modules.Audit.Infrastructure.Persistence;

public sealed class AuditReadStore : IAuditReadStore
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AuditReadStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AuditLogReadModel?> GetByIdAsync(Guid auditId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT AuditId, Timestamp, UserId, UserName, Module, Action, Entity, EntityId,
                   OldValue, NewValue, MachineName, ApplicationVersion
            FROM AuditLogs WHERE AuditId = @AuditId;
            """;
        return await connection.QueryFirstOrDefaultAsync<AuditLogReadModel>(sql, new { AuditId = auditId });
    }

    public async Task<PagedResult<AuditLogReadModel>> GetPagedAsync(int pageNumber, int pageSize, DateTime? fromDate, DateTime? toDate, string? userId, string? module, string? action, string? searchTerm, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var offset = (pageNumber - 1) * pageSize;

        var where = new System.Text.StringBuilder("WHERE 1=1");
        var parameters = new DynamicParameters();

        if (fromDate.HasValue)
        {
            where.Append(" AND Timestamp >= @FromDate");
            parameters.Add("FromDate", fromDate.Value);
        }
        if (toDate.HasValue)
        {
            where.Append(" AND Timestamp < @ToDate");
            parameters.Add("ToDate", toDate.Value.AddDays(1));
        }
        if (!string.IsNullOrWhiteSpace(userId))
        {
            where.Append(" AND UserId = @UserId");
            parameters.Add("UserId", userId);
        }
        if (!string.IsNullOrWhiteSpace(module))
        {
            where.Append(" AND Module = @Module");
            parameters.Add("Module", module);
        }
        if (!string.IsNullOrWhiteSpace(action))
        {
            where.Append(" AND Action = @Action");
            parameters.Add("Action", action);
        }
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            where.Append(" AND (UserName LIKE @Search OR EntityId LIKE @Search OR NewValue LIKE @Search)");
            parameters.Add("Search", $"%{searchTerm}%");
        }

        var sql = $"""
            SELECT AuditId, Timestamp, UserId, UserName, Module, Action, Entity, EntityId,
                   OldValue, NewValue, MachineName, ApplicationVersion
            FROM AuditLogs
            {where}
            ORDER BY Timestamp DESC
            LIMIT @PageSize OFFSET @Offset;
            """;

        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", offset);

        var items = await connection.QueryAsync<AuditLogReadModel>(sql, parameters);
        var list = items.ToList();

        var countSql = $"SELECT COUNT(1) FROM AuditLogs {where};";
        var total = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        return new PagedResult<AuditLogReadModel>(list, pageNumber, pageSize, total);
    }

    public async Task<IReadOnlyList<AuditLogReadModel>> SearchAsync(string? searchTerm, DateTime? fromDate, DateTime? toDate, string? userId, string? module, string? action, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var where = new System.Text.StringBuilder("WHERE 1=1");
        var parameters = new DynamicParameters();

        if (fromDate.HasValue)
        {
            where.Append(" AND Timestamp >= @FromDate");
            parameters.Add("FromDate", fromDate.Value);
        }
        if (toDate.HasValue)
        {
            where.Append(" AND Timestamp < @ToDate");
            parameters.Add("ToDate", toDate.Value.AddDays(1));
        }
        if (!string.IsNullOrWhiteSpace(userId))
        {
            where.Append(" AND UserId = @UserId");
            parameters.Add("UserId", userId);
        }
        if (!string.IsNullOrWhiteSpace(module))
        {
            where.Append(" AND Module = @Module");
            parameters.Add("Module", module);
        }
        if (!string.IsNullOrWhiteSpace(action))
        {
            where.Append(" AND Action = @Action");
            parameters.Add("Action", action);
        }
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            where.Append(" AND (UserName LIKE @Search OR EntityId LIKE @Search OR NewValue LIKE @Search)");
            parameters.Add("Search", $"%{searchTerm}%");
        }

        var sql = $"""
            SELECT AuditId, Timestamp, UserId, UserName, Module, Action, Entity, EntityId,
                   OldValue, NewValue, MachineName, ApplicationVersion
            FROM AuditLogs
            {where}
            ORDER BY Timestamp DESC;
            """;

        var result = await connection.QueryAsync<AuditLogReadModel>(sql, parameters);
        return result.ToList();
    }
}
