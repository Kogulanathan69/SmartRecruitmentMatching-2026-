namespace NexHire.Application.DTOs.Matching;

public class MatchScoreResponseDto
{
    public Guid JobSeekerProfileId { get; set; }

    public Guid JobId { get; set; }

    public decimal TotalScore { get; set; }

    public bool IsEligible { get; set; }

    public string Recommendation { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public List<MatchScoreDetailDto> ScoreDetails { get; set; } = new();
}

public class MatchScoreDetailDto
{
    public string Category { get; set; } = string.Empty;

    public decimal RawScore { get; set; }

    public decimal Weight { get; set; }

    public decimal WeightedPoints { get; set; }

    public decimal MaximumWeightedPoints { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;
}