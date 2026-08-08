namespace NexHire.Domain.Entities;

public class TalentPoolEntry
{
    public Guid Id { get; set; }

    public Guid JobSeekerProfileId { get; set; }

    public JobSeekerProfile JobSeekerProfile { get; set; } = null!;

    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }
}