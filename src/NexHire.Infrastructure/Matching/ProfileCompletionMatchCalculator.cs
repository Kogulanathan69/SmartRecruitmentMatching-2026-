using NexHire.Application.Common;
using NexHire.Application.Matching;

namespace NexHire.Infrastructure.Matching;

/// <summary>
/// Calculates the Profile Completion category score.
///
/// The actual profile completion percentage is calculated
/// outside this class from the final Job Seeker profile fields.
///
/// This keeps the matching calculator independent from
/// changing profile entities.
/// </summary>
public class ProfileCompletionMatchCalculator
{
    /// <summary>
    /// Uses the candidate's profile completion percentage
    /// directly as the raw category score.
    ///
    /// Rules:
    /// - Values below 0 are treated as 0.
    /// - Values above 100 are treated as 100.
    /// - Values from 0 to 100 are used directly.
    /// </summary>
    public CategoryScoreResult Calculate(ProfileCompletionMatchInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var rawScore = Math.Clamp(
            input.CompletionPercentage,
            MatchingConstants.MinimumScore,
            MatchingConstants.MaximumScore);

        rawScore = Math.Round(rawScore, 2);

        var explanation =
            $"Candidate profile is {rawScore:0.##}% complete.";

        return new CategoryScoreResult
        {
            RawScore = rawScore,
            Status = GetStatus(rawScore),
            Explanation = explanation
        };
    }

    /// <summary>
    /// Converts the numeric score into the common
    /// explainable matching status.
    /// </summary>
    private static string GetStatus(decimal score)
    {
        if (score >= MatchingConstants.HighlyRecommendedMinimum)
        {
            return "Strong";
        }

        if (score >= MatchingConstants.RecommendedForReviewMinimum)
        {
            return "Good";
        }

        if (score >= MatchingConstants.ConsiderWithCautionMinimum)
        {
            return "Partial";
        }

        return "Needs Improvement";
    }
}