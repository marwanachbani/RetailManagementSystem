using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Notifications.Domain;

namespace RMS.Modules.Notifications.Application.Contracts;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetUnreadAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteReadAsync(CancellationToken cancellationToken = default);
}
