using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Audit.Application.Contracts;
using RMS.Modules.Audit.Domain.Entities;

namespace RMS.Modules.Audit.Application.EventHandlers;

public static class AuditEntryFactory
{
    public static AuditLog Create(
        string module,
        string action,
        string entity,
        string? entityId,
        string? oldValue,
        string? newValue,
        ICurrentUserContext currentUserContext)
    {
        return new AuditLog(
            Guid.NewGuid(),
            DateTime.UtcNow,
            currentUserContext.UserId,
            currentUserContext.UserName ?? "System",
            module,
            action,
            entity,
            entityId,
            oldValue,
            newValue,
            Environment.MachineName,
            typeof(AuditEntryFactory).Assembly.GetName().Version?.ToString() ?? "1.0.0.0");
    }
}
