using RMS.Modules.Audit.Domain.Entities;

namespace RMS.Modules.Audit.Application.Contracts;

public interface IAuditWriteStore
{
    Task InsertAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
