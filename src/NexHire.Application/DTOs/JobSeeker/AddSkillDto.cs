namespace NexHire.Application.DTOs.JobSeeker;

public class AddSkillDto
{
    public string SkillName { get; set; } = string.Empty;
    public int ProficiencyLevel { get; set; }
    public int YearsOfExperience { get; set; }
}
