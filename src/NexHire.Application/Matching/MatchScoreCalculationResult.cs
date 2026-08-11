namespace NexHire.Application.Matching;

/// <summary>
/// Represents the complete weighted score calculation
/// for one candidate and one job.
///
/// This class contains only scoring information.
/// Eligibility and hiring recommendation are handled
/// separately and combined later in the matching service.
/// </summary>
public class MatchScoreCalculationResult
{
    /// <summary>
    /// Final weighted matching score.
    ///
    /// Expected range: 0 to 100.
    /// </summary>
    public decimal TotalScore { get; set; }

    /// <summary>
    /// Detailed weighted result for every
    /// configured matching category.
    /// </summary>
    public List<WeightedCategoryScoreResult> ScoreDetails { get; set; } = new();
}