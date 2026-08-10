namespace NexHire.Application.DTOs.Matching;

public class CoreMatchDto
{
    public Guid JobId { get; set; }
    public Guid CandidateProfileId { get; set; }
    public decimal TotalScore { get; set; }
    public decimal RequiredSkillsScore { get; set; }
    public decimal ExperienceScore { get; set; }
    public decimal EducationScore { get; set; }
    public decimal LocationScore { get; set; }
    public bool IsEligible { get; set; }
    public string RuleVersion { get; set; } = "RM-1.0";
    public List<string> SkillGaps { get; set; } = new();
    public string Explanation { get; set; } = string.Empty;
}
