namespace RMS.Modules.Audit.Application.Contracts;

public sealed record AuditLogReadModel(
    Guid AuditId,
    DateTime Timestamp,
    Guid? UserId,
    string UserName,
    string Module,
    string Action,
    string Entity,
    string? EntityId,
    string? OldValue,
    string? NewValue,
    string MachineName,
    string ApplicationVersion)
{
    private AuditLogReadModel() : this(default, default, null, string.Empty, string.Empty, string.Empty, string.Empty, null, null, string.Empty, string.Empty, string.Empty) { }
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public interface IAuditReadStore
{
    Task<PagedResult<AuditLogReadModel>> GetPagedAsync(int pageNumber, int pageSize, DateTime? fromDate, DateTime? toDate, string? userId, string? module, string? action, string? searchTerm, CancellationToken cancellationToken = default);
    Task<AuditLogReadModel?> GetByIdAsync(Guid auditId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogReadModel>> SearchAsync(string? searchTerm, DateTime? fromDate, DateTime? toDate, string? userId, string? module, string? action, CancellationToken cancellationToken = default);
}
