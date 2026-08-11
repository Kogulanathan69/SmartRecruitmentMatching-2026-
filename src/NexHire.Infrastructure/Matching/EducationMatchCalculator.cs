using NexHire.Application.Common;
using NexHire.Application.Matching;

namespace NexHire.Infrastructure.Matching;

/// <summary>
/// Calculates the Education category score.
///
/// This calculator uses only ordered education-level values.
/// It does not directly depend on the team's database entities.
///
/// The actual mapping from Diploma, Degree, Masters, etc.
/// will be connected later when the team education models are final.
/// </summary>
public class EducationMatchCalculator
{
    /// <summary>
    /// Calculates a deterministic education score from 0 to 100.
    ///
    /// Rule:
    /// - No education requirement = 100.
    /// - Candidate meets or exceeds the requirement = 100.
    /// - Candidate is below the requirement = 0.
    ///
    /// We intentionally avoid calculating percentages between
    /// education levels because education levels are categories,
    /// not continuous numeric measurements.
    /// </summary>
    public CategoryScoreResult Calculate(EducationMatchInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var candidateLevel = Math.Max(
            0,
            input.CandidateEducationLevel);

        var requiredLevel = Math.Max(
            0,
            input.RequiredEducationLevel);

        decimal rawScore;
        string explanation;

        // ---------------------------------------------------------
        // CASE 1: JOB HAS NO EDUCATION REQUIREMENT
        // ---------------------------------------------------------
        if (requiredLevel == 0)
        {
            rawScore = MatchingConstants.MaximumScore;

            explanation =
                "This job does not have a minimum education requirement.";
        }

        // ---------------------------------------------------------
        // CASE 2: CANDIDATE MEETS OR EXCEEDS REQUIREMENT
        // ---------------------------------------------------------
        else if (candidateLevel >= requiredLevel)
        {
            rawScore = MatchingConstants.MaximumScore;

            explanation =
                $"Candidate education level ({candidateLevel}) " +
                $"meets or exceeds the required level ({requiredLevel}).";
        }

        // ---------------------------------------------------------
        // CASE 3: CANDIDATE IS BELOW REQUIREMENT
        // ---------------------------------------------------------
        else
        {
            rawScore = MatchingConstants.MinimumScore;

            explanation =
                $"Candidate education level ({candidateLevel}) " +
                $"is below the required level ({requiredLevel}).";
        }

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