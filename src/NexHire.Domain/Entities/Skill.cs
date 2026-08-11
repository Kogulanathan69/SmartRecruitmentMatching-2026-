namespace NexHire.Domain.Entities;

public class Skill
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string? Description { get; set; }

    public ICollection<CandidateSkill> CandidateSkills { get; set; }
        = new List<CandidateSkill>();
}