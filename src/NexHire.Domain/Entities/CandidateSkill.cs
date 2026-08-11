namespace NexHire.Domain.Entities;

public class CandidateSkill
{
    public Guid Id { get; set; }
    public Guid JobSeekerProfileId { get; set; }
    public JobSeekerProfile JobSeekerProfile { get; set; } = null!;
    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public int ProficiencyLevel { get; set; }
    public int YearsOfExperience { get; set; }
}
