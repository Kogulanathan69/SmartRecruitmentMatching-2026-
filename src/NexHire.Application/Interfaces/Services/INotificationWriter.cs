namespace NexHire.Application.Interfaces.Services;
public interface INotificationWriter
{
    Task QueueAsync(Guid userId, string type, string title, string message, string? entityType = null, Guid? entityId = null);
}
