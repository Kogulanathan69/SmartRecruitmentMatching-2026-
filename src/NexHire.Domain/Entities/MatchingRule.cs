namespace NexHire.Domain.Entities;

public class MatchingRule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public decimal SkillsWeight { get; set; }

    public decimal ExperienceWeight { get; set; }

    public decimal EducationWeight { get; set; }

    public decimal CertificationWeight { get; set; }

    public decimal LocationWeight { get; set; }

    public decimal ProjectsWeight { get; set; }

    public decimal ProfileCompletionWeight { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public decimal TotalWeight =>
        SkillsWeight +
        ExperienceWeight +
        EducationWeight +
        CertificationWeight +
        LocationWeight +
        ProjectsWeight +
        ProfileCompletionWeight;

    public bool HasValidTotalWeight()
    {
        return TotalWeight == 100m;
    }
}