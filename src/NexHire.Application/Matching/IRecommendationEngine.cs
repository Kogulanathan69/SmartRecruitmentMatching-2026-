namespace NexHire.Application.Matching;

/// <summary>
/// Defines the rule used to convert a candidate's
/// eligibility and final matching score into a
/// human-readable recommendation.
/// </summary>
public interface IRecommendationEngine
{
    /// <summary>
    /// Returns the candidate recommendation.
    ///
    /// Eligibility is checked first.
    /// An ineligible candidate must always return
    /// "Not Eligible" regardless of the numeric score.
    /// </summary>
    string GetRecommendation(
        decimal totalScore,
        bool isEligible);
}