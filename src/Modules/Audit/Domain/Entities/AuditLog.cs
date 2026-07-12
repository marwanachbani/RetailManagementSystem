namespace RMS.Modules.Audit.Domain.Entities;

public sealed record AuditLog(
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
    public AuditLog() : this(default, default, null, string.Empty, string.Empty, string.Empty, string.Empty, null, null, string.Empty, string.Empty, string.Empty) { }
}
