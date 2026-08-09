using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Notification?> GetOwnedByIdAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notification>> GetUnreadByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Notification notification,
        CancellationToken cancellationToken = default);

    void Update(Notification notification);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
