namespace NexHire.Domain.Entities;

public class JobPreferredSkill
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid JobId { get; set; }

    public Job Job { get; set; } = null!;

    public Guid SkillId { get; set; }

    public Skill Skill { get; set; } = null!;

    // Preferred skills improve the match score,
    // but they do not determine basic eligibility.
    public int MinimumProficiencyLevel { get; set; }
}