using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Application.IntegrationEvents;
using RMS.Modules.Notifications.Domain;

namespace RMS.Modules.Notifications.Application.EventHandlers;

public sealed class MissingStorageFolderNotificationHandler : BaseNotificationHandler<MissingStorageFolderNotificationEvent>
{
    public MissingStorageFolderNotificationHandler(INotificationRepository repository, ICurrentUserContext currentUserContext)
        : base(repository, currentUserContext) { }

    public override async Task HandleAsync(MissingStorageFolderNotificationEvent e, CancellationToken cancellationToken = default)
    {
        await CreateNotificationAsync(
            "Missing Storage Folder",
            $"Storage folder '{e.FolderType}' is missing at path: {e.Path}",
            NotificationCategory.System,
            NotificationSeverity.Warning,
            "System",
            cancellationToken: cancellationToken);
    }
}