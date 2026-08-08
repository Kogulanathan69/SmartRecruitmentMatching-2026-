namespace NexHire.Application.Matching;

/// <summary>
/// Represents the ranked result of one candidate application.
///
/// This is the internal matching result used by the
/// ranking engine before mapping to API DTOs.
/// </summary>
public class CandidateRankingResult
{
    /// <summary>
    /// Unique job application identifier.
    /// </summary>
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// Candidate's job seeker profile identifier.
    /// </summary>
    public Guid JobSeekerProfileId { get; set; }

    /// <summary>
    /// Competition rank assigned by the ranking engine.
    ///
    /// Example:
    /// Scores: 95, 90, 90, 80
    /// Ranks : 1,  2,  2,  4
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// Final weighted matching score.
    /// </summary>
    public decimal TotalScore { get; set; }

    /// <summary>
    /// Indicates whether the candidate passed
    /// the mandatory eligibility rules.
    /// </summary>
    public bool IsEligible { get; set; }

    /// <summary>
    /// True when another candidate has the same
    /// ranking score and therefore shares the same rank.
    /// </summary>
    public bool IsTied { get; set; }

    /// <summary>
    /// Recommendation produced by the
    /// recommendation engine.
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;
}