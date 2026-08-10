using NexHire.Application.DTOs.Notification;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Domain.Entities;

namespace NexHire.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(
        INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notifications =
            await _repository.GetByUserAsync(
                userId,
                cancellationToken);

        return notifications
            .Select(Map)
            .ToList();
    }

    public Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetUnreadCountAsync(
            userId,
            cancellationToken);
    }

    public async Task<NotificationDto> MarkReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notification =
            await _repository.GetOwnedByIdAsync(
                notificationId,
                userId,
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

            _repository.Update(notification);

            await _repository.SaveChangesAsync(
                cancellationToken);
        }

        return Map(notification);
    }

    public async Task<int> MarkAllReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notifications =
            await _repository.GetUnreadByUserAsync(
                userId,
                cancellationToken);

        if (notifications.Count == 0)
        {
            return 0;
        }

        var now = DateTime.UtcNow;

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = now;

            _repository.Update(notification);
        }

        await _repository.SaveChangesAsync(
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

        await _repository.AddAsync(
            notification,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);
    }

    private static NotificationDto Map(
        Notification notification)
    {
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
}
