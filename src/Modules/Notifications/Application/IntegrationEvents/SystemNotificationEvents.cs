using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.EventBus;

namespace RMS.Modules.Notifications.Application.IntegrationEvents;

public sealed record DatabaseErrorNotificationEvent(string ErrorMessage, string? StackTrace = null) : DomainEvent, IIntegrationEvent;

public sealed record BackupFailedNotificationEvent(string ErrorMessage) : DomainEvent, IIntegrationEvent;

public sealed record MissingStorageFolderNotificationEvent(string FolderType, string Path) : DomainEvent, IIntegrationEvent;

public sealed record MigrationFailureNotificationEvent(string ErrorMessage) : DomainEvent, IIntegrationEvent;

public sealed record UnexpectedExceptionNotificationEvent(string Source, string ErrorMessage, string? StackTrace = null) : DomainEvent, IIntegrationEvent;

public sealed record DiskSpaceWarningNotificationEvent(long AvailableBytes, string DriveName) : DomainEvent, IIntegrationEvent;
