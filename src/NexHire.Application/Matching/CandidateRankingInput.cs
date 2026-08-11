namespace NexHire.Application.Matching;

/// <summary>
/// Contains the information required to rank
/// one candidate application.
///
/// The ranking engine does not directly depend on
/// database entities or API DTOs.
/// </summary>
public class CandidateRankingInput
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
    /// Final weighted matching score.
    /// Expected range: 0 to 100.
    /// </summary>
    public decimal TotalScore { get; set; }

    /// <summary>
    /// Indicates whether the candidate passed
    /// the mandatory eligibility rules.
    /// </summary>
    public bool IsEligible { get; set; }

    /// <summary>
    /// Human-readable recommendation generated
    /// by the recommendation engine.
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;
}