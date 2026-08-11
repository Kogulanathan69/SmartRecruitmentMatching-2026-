namespace NexHire.Application.DTOs.Matching;

public class CandidateComparisonDto
{
    public Guid ApplicationId { get; set; }

    public Guid JobSeekerProfileId { get; set; }

    public decimal TotalScore { get; set; }

    public bool IsEligible { get; set; }

    public string Recommendation { get; set; } = string.Empty;

    public List<MatchScoreDetailDto> ScoreDetails { get; set; } = new();
}