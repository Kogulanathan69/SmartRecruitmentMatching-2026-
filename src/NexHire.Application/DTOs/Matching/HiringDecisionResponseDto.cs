namespace NexHire.Application.DTOs.Matching;

public class HiringDecisionResponseDto
{
    public Guid ApplicationId { get; set; }

    public string PreviousStatus { get; set; } = string.Empty;

    public string CurrentStatus { get; set; } = string.Empty;

    public string Decision { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public Guid ChangedByUserId { get; set; }

    public DateTime ChangedAtUtc { get; set; }

    public string Message { get; set; } = string.Empty;
}