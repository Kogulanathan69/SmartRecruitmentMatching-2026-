namespace NexHire.Application.DTOs.Matching;

public class RankingSummaryDto
{
    public Guid JobId { get; set; }

    public int TotalCandidates { get; set; }

    public int EligibleCandidates { get; set; }

    public decimal HighestScore { get; set; }

    public decimal AverageScore { get; set; }

    public List<CandidateRankingDto> Rankings { get; set; } = new();
}