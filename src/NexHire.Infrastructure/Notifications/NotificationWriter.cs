using NexHire.Application.Interfaces.Services;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Notifications;

public class NotificationWriter : INotificationWriter
{
    private readonly AppDbContext _db;

    public NotificationWriter(AppDbContext db)
    {
        _db = db;
    }

    public Task QueueAsync(
        Guid userId,
        string type,
        string title,
        string message,
        string? entityType = null,
        Guid? entityId = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            RelatedEntityId = entityId,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);

        var outbox = new OutboxMessage
        {
            Type = "NotificationCreated",
            Payload =
                $"{{\"userId\":\"{userId}\",\"type\":\"{type}\",\"entityId\":\"{entityId}\"}}"
        };

        _db.OutboxMessages.Add(outbox);

        return Task.CompletedTask;
    }
}