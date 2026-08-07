using NexHire.Application.Common;
using NexHire.Application.Matching;

namespace NexHire.Infrastructure.Matching;

/// <summary>
/// Converts the candidate's eligibility and final
/// weighted matching score into a recommendation.
///
/// Important:
/// Eligibility is checked before the numeric score.
/// An ineligible candidate can never receive a positive
/// recommendation based only on a high score.
/// </summary>
public class RecommendationEngine : IRecommendationEngine
{
    /// <summary>
    /// Returns the recommendation using the configured
    /// matching score bands.
    ///
    /// Rules:
    /// - Mandatory eligibility failure = Not Eligible
    /// - 90 to 100 = Highly Recommended
    /// - 75 to below 90 = Recommended for Review
    /// - 60 to below 75 = Consider with Caution
    /// - Below 60 = Not Recommended
    /// </summary>
    public string GetRecommendation(
        decimal totalScore,
        bool isEligible)
    {
        // ---------------------------------------------------------
        // ELIGIBILITY ALWAYS HAS FIRST PRIORITY
        // ---------------------------------------------------------
        if (!isEligible)
        {
            return MatchingConstants.NotEligible;
        }

        // Protect the recommendation engine from scores
        // outside the supported 0 to 100 range.
        var score = Math.Clamp(
            totalScore,
            MatchingConstants.MinimumScore,
            MatchingConstants.MaximumScore);

        // ---------------------------------------------------------
        // RECOMMENDATION BANDS
        // ---------------------------------------------------------
        if (score >= MatchingConstants.HighlyRecommendedMinimum)
        {
            return MatchingConstants.HighlyRecommended;
        }

        if (score >= MatchingConstants.RecommendedForReviewMinimum)
        {
            return MatchingConstants.RecommendedForReview;
        }

        if (score >= MatchingConstants.ConsiderWithCautionMinimum)
        {
            return MatchingConstants.ConsiderWithCaution;
        }

        return MatchingConstants.NotRecommended;
    }
}