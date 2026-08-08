namespace NexHire.Domain.Entities;

public class MatchResult
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Candidate and job involved in this match.
    public Guid JobSeekerProfileId { get; set; }

    public Guid JobId { get; set; }

    // Final weighted score from 0 to 100.
    public decimal TotalScore { get; set; }

    // Mandatory eligibility result.
    public bool IsEligible { get; set; }

    // Example:
    // Highly Recommended / Recommended for Review /
    // Consider with Caution / Not Recommended / Not Eligible.
    public string Recommendation { get; set; } = string.Empty;

    // Simple explanation shown to the user/employer.
    public string Summary { get; set; } = string.Empty;

    // When this result was calculated.
    public DateTime CalculatedAtUtc { get; set; } = DateTime.UtcNow;

    // Transparent category-by-category score details.
    public ICollection<MatchScoreDetail> ScoreDetails { get; set; }
        = new List<MatchScoreDetail>();
}