namespace NexHire.Application.Matching;

/// <summary>
/// Represents the complete result of matching
/// one candidate profile against one job.
/// </summary>
public class MatchingCalculationResult
{
    public Guid JobSeekerProfileId { get; set; }

    public Guid JobId { get; set; }

    public decimal TotalScore { get; set; }

    public bool IsEligible { get; set; }

    public string Recommendation { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Categories where the candidate performed strongly.
    /// </summary>
    public List<string> Strengths { get; set; } = new();

    /// <summary>
    /// Categories that may need improvement.
    /// </summary>
    public List<string> ImprovementAreas { get; set; } = new();

    /// <summary>
    /// Mandatory skills successfully matched
    /// during eligibility checking.
    /// </summary>
    public List<string> MatchedMandatorySkills { get; set; } = new();

    /// <summary>
    /// Mandatory skills that were missing
    /// or below the required proficiency.
    /// </summary>
    public List<string> MissingMandatorySkills { get; set; } = new();

    /// <summary>
    /// Reasons why the candidate failed
    /// mandatory eligibility rules.
    /// </summary>
    public List<string> EligibilityFailures { get; set; } = new();

    /// <summary>
    /// Detailed weighted score information
    /// for all seven matching categories.
    /// </summary>
    public List<WeightedCategoryScoreResult> ScoreDetails { get; set; } = new();
}