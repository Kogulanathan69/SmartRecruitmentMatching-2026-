namespace NexHire.Application.DTOs.Job;

public sealed class JobSkillRequirementDto
{
    public string SkillName { get; set; } = string.Empty;

    public int MinimumProficiencyLevel { get; set; } = 1;
}
