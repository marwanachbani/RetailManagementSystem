using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.EventBus;

namespace RMS.Modules.Settings.Application.IntegrationEvents;

public sealed record SettingChangedIntegrationEvent(string Section, string? OldValue, string? NewValue) : DomainEvent, IIntegrationEvent;
public sealed record SettingsResetIntegrationEvent() : DomainEvent, IIntegrationEvent;
public sealed record FolderChangedIntegrationEvent(string FolderType, string? OldPath, string? NewPath) : DomainEvent, IIntegrationEvent;
