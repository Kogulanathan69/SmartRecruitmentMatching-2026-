namespace NexHire.Application.DTOs.Matching;

public class CandidateRankingDto
{
    public Guid ApplicationId { get; set; }

    public Guid JobSeekerProfileId { get; set; }

    public int Rank { get; set; }

    public decimal TotalScore { get; set; }

    public bool IsEligible { get; set; }

    public bool IsTied { get; set; }

    public string Recommendation { get; set; } = string.Empty;
}