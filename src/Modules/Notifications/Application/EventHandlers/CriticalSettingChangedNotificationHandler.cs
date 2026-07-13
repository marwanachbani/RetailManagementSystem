using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;
using RMS.Modules.Settings.Application.IntegrationEvents;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class CriticalSettingChangedNotificationHandler : BaseNotificationHandler<SettingChangedIntegrationEvent>
{
    private static readonly HashSet<string> CriticalSettings = new(StringComparer.OrdinalIgnoreCase)
    {
        "Application.Theme",
        "Backup.AutomaticBackup",
        "Inventory.AllowNegativeStock",
        "Sales.AllowManualPriceChange"
    };

    public CriticalSettingChangedNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(SettingChangedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(e.Section) || !CriticalSettings.Contains(e.Section))
            return;

        await CreateNotificationAsync(
            "Critical Setting Changed",
            $"Setting '{e.Section}' was changed from '{e.OldValue ?? "(empty)"}' to '{e.NewValue ?? "(empty)"}'.",
            NotificationCategory.Settings,
            NotificationSeverity.Warning,
            "Settings",
            cancellationToken: cancellationToken);
    }
}