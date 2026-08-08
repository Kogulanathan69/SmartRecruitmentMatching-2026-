using NexHire.Domain.Enums;

namespace NexHire.Domain.Entities;

public class ApplicationStatusHistory
{
    public Guid Id { get; set; }
    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public ApplicationStatus? FromStatus { get; set; }
    public ApplicationStatus ToStatus { get; set; }
    public Guid ChangedByUserId { get; set; }
    public string ChangedByRole { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
}
