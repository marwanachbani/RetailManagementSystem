using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.EventBus;

namespace RMS.Modules.Backup.Application.IntegrationEvents;

public sealed record BackupCreatedIntegrationEvent(
    Guid BackupId,
    string FileName,
    long Size,
    string UserName) : DomainEvent, IIntegrationEvent;

public sealed record BackupRestoredIntegrationEvent(
    Guid BackupId,
    string FileName,
    string UserName) : DomainEvent, IIntegrationEvent;
