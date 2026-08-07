namespace NexHire.Application.DTOs.ApplicationStatus;

public class ApplicationStatusHistoryDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string? FromStatus { get; set; }
    public string ToStatus { get; set; } = string.Empty;
    public Guid ChangedByUserId { get; set; }
    public string ChangedByRole { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime ChangedAtUtc { get; set; }
}
