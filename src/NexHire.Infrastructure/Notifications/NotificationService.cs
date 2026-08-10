using Microsoft.EntityFrameworkCore;
using NexHire.Application.DTOs.Notification;
using NexHire.Application.Interfaces.Services;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Notifications;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new NotificationDto
            {
                Id = x.Id,
                Type = x.Type,
                Title = x.Title,
                Message = x.Message,
                RelatedEntityId = x.RelatedEntityId,
                IsRead = x.IsRead,
                CreatedAtUtc = x.CreatedAtUtc,
                ReadAtUtc = x.ReadAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Notifications
            .AsNoTracking()
            .CountAsync(
                x => x.UserId == userId && !x.IsRead,
                cancellationToken);
    }

    public async Task<NotificationDto> MarkReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(
                x =>
                    x.Id == notificationId &&
                    x.UserId == userId,
                cancellationToken);

        if (notification is null)
        {
            throw new KeyNotFoundException(
                "Notification was not found.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(
                cancellationToken);
        }

        return new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            RelatedEntityId = notification.RelatedEntityId,
            IsRead = notification.IsRead,
            CreatedAtUtc = notification.CreatedAtUtc,
            ReadAtUtc = notification.ReadAtUtc
        };
    }

    public async Task<int> MarkAllReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _db.Notifications
            .Where(x =>
                x.UserId == userId &&
                !x.IsRead)
            .ToListAsync(cancellationToken);

        if (notifications.Count == 0)
        {
            return 0;
        }

        var now = DateTime.UtcNow;

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = now;
        }

        await _db.SaveChangesAsync(
            cancellationToken);

        return notifications.Count;
    }

    public async Task CreateAsync(
        Guid userId,
        string type,
        string title,
        string message,
        Guid? relatedEntityId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException(
                "Notification type is required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Notification title is required.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "Notification message is required.");
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type.Trim(),
            Title = title.Trim(),
            Message = message.Trim(),
            RelatedEntityId = relatedEntityId,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _db.Notifications.AddAsync(
            notification,
            cancellationToken);

        await _db.SaveChangesAsync(
            cancellationToken);
    }
}