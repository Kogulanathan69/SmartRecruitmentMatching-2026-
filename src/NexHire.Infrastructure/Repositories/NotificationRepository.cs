using Microsoft.EntityFrameworkCore;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Repositories;

public class NotificationRepository
    : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Notification>>
        GetByUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public Task<Notification?> GetOwnedByIdAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _context.Notifications
            .SingleOrDefaultAsync(
                x =>
                    x.Id == notificationId &&
                    x.UserId == userId,
                cancellationToken);
    }

    public Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _context.Notifications
            .CountAsync(
                x =>
                    x.UserId == userId &&
                    !x.IsRead,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>>
        GetUnreadByUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(x =>
                x.UserId == userId &&
                !x.IsRead)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        return _context.Notifications
            .AddAsync(
                notification,
                cancellationToken)
            .AsTask();
    }

    public void Update(
        Notification notification)
    {
        _context.Notifications.Update(notification);
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(
            cancellationToken);
    }
}
