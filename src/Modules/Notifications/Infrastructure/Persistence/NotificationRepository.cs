using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;

namespace RMS.Modules.Notifications.Infrastructure.Persistence;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public NotificationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO Notifications
                (Id, Title, Message, Category, Severity, CreatedOn, ReadOn, IsRead, UserId, RelatedModule, RelatedEntityId)
            VALUES
                (@Id, @Title, @Message, @Category, @Severity, @CreatedOn, @ReadOn, @IsRead, @UserId, @RelatedModule, @RelatedEntityId);
            """;
        await connection.ExecuteAsync(sql, new
        {
            notification.Id,
            notification.Title,
            notification.Message,
            Category = (int)notification.Category,
            Severity = (int)notification.Severity,
            notification.CreatedOn,
            ReadOn = notification.ReadOn.HasValue ? notification.ReadOn.Value.ToString("O") : null,
            notification.IsRead,
            UserId = notification.UserId.HasValue ? notification.UserId.Value.ToString() : null,
            notification.RelatedModule,
            RelatedEntityId = notification.RelatedEntityId.HasValue ? notification.RelatedEntityId.Value.ToString() : null
        });
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Notifications WHERE Id = @Id;";
        var row = await connection.QueryFirstOrDefaultAsync<NotificationRow>(sql, new { Id = id });
        return row is null ? null : ToDomain(row);
    }

    public async Task<IReadOnlyList<Notification>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Notifications ORDER BY CreatedOn DESC;";
        var rows = await connection.QueryAsync<NotificationRow>(sql);
        return rows.Select(ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Notification>> GetUnreadAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Notifications WHERE IsRead = 0 ORDER BY CreatedOn DESC;";
        var rows = await connection.QueryAsync<NotificationRow>(sql);
        return rows.Select(ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Notifications WHERE UserId = @UserId ORDER BY CreatedOn DESC;";
        var rows = await connection.QueryAsync<NotificationRow>(sql, new { UserId = userId.ToString() });
        return rows.Select(ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Notification>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var offset = (pageNumber - 1) * pageSize;
        const string sql = "SELECT * FROM Notifications ORDER BY CreatedOn DESC LIMIT @PageSize OFFSET @Offset;";
        var rows = await connection.QueryAsync<NotificationRow>(sql, new { PageSize = pageSize, Offset = offset });
        return rows.Select(ToDomain).ToList();
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM Notifications WHERE IsRead = 0;";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM Notifications WHERE IsRead = 0 AND UserId = @UserId;";
        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId.ToString() });
    }

    public async Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE Notifications SET IsRead = 1, ReadOn = @ReadOn WHERE Id = @Id AND IsRead = 0;";
        await connection.ExecuteAsync(sql, new { Id = id, ReadOn = DateTime.UtcNow.ToString("O") });
    }

    public async Task MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE Notifications SET IsRead = 1, ReadOn = @ReadOn WHERE IsRead = 0;";
        await connection.ExecuteAsync(sql, new { ReadOn = DateTime.UtcNow.ToString("O") });
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM Notifications WHERE Id = @Id;", new { Id = id });
    }

    public async Task DeleteReadAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM Notifications WHERE IsRead = 1;");
    }

    private static Notification ToDomain(NotificationRow row) => new()
    {
        Id = row.Id,
        Title = row.Title,
        Message = row.Message,
        Category = (NotificationCategory)row.Category,
        Severity = (NotificationSeverity)row.Severity,
        CreatedOn = row.CreatedOn,
        ReadOn = row.ReadOn,
        IsRead = row.IsRead,
        UserId = string.IsNullOrWhiteSpace(row.UserId) ? null : Guid.Parse(row.UserId),
        RelatedModule = row.RelatedModule,
        RelatedEntityId = string.IsNullOrWhiteSpace(row.RelatedEntityId) ? null : Guid.Parse(row.RelatedEntityId)
    };

    private sealed class NotificationRow
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int Category { get; set; }
        public int Severity { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ReadOn { get; set; }
        public bool IsRead { get; set; }
        public string? UserId { get; set; }
        public string RelatedModule { get; set; } = string.Empty;
        public string? RelatedEntityId { get; set; }
    }
}
