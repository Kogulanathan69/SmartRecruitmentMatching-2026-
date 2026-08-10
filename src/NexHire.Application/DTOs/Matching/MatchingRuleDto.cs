namespace NexHire.Application.DTOs.Matching;

public sealed class MatchingRuleDto
{
    public Guid Id { get; set; }

    public decimal SkillsWeight { get; set; }

    public decimal ExperienceWeight { get; set; }

    public decimal EducationWeight { get; set; }

    public decimal CertificationWeight { get; set; }

    public decimal LocationWeight { get; set; }

    public decimal ProjectsWeight { get; set; }

    public decimal ProfileCompletionWeight { get; set; }

    public decimal TotalWeight { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
