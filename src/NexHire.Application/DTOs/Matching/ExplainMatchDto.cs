namespace NexHire.Application.DTOs.Matching;

public class ExplainMatchDto
{
    public Guid ApplicationId { get; set; }

    public decimal TotalScore { get; set; }

    public bool IsEligible { get; set; }

    public string Recommendation { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public List<string> Strengths { get; set; } = new();

    public List<string> ImprovementAreas { get; set; } = new();

    public List<string> MatchedMandatorySkills { get; set; } = new();

    public List<string> MissingMandatorySkills { get; set; } = new();

    public List<string> EligibilityFailures { get; set; } = new();

    public List<MatchScoreDetailDto> ScoreDetails { get; set; } = new();
}