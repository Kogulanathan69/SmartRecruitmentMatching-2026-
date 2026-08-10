namespace NexHire.Domain.Entities;

public class JobRequiredSkill
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid JobId { get; set; }

    public Job Job { get; set; } = null!;

    public Guid SkillId { get; set; }

    public Skill Skill { get; set; } = null!;

    // Minimum proficiency required for the candidate
    // to satisfy this mandatory job skill.
    public int MinimumProficiencyLevel { get; set; }
}