namespace NexHire.Application.DTOs.Notification;
public class NotificationResponseDto { public Guid Id { get; set; } public string Type { get; set; } = string.Empty; public string Title { get; set; } = string.Empty; public string Message { get; set; } = string.Empty; public bool IsRead { get; set; } public DateTime CreatedAtUtc { get; set; } }
