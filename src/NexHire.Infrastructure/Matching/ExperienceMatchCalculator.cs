using NexHire.Application.Common;
using NexHire.Application.Matching;

namespace NexHire.Infrastructure.Matching;

/// <summary>
/// Calculates the Experience category score.
///
/// This calculator receives only ExperienceMatchInput.
/// It does not directly depend on Job or JobSeekerExperience
/// database entities.
///
/// Because of this, if another team member changes their entity
/// structure later, only the mapping code needs to change.
/// </summary>
public class ExperienceMatchCalculator
{
    /// <summary>
    /// Calculates a deterministic experience score from 0 to 100.
    ///
    /// Rule:
    /// - If the job requires no experience, score = 100.
    /// - If candidate experience meets or exceeds the requirement, score = 100.
    /// - Otherwise, score is calculated proportionally.
    ///
    /// Example:
    /// Candidate = 2 years
    /// Required  = 4 years
    /// Score     = 50
    /// </summary>
    public CategoryScoreResult Calculate(ExperienceMatchInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Negative values should not produce an invalid score.
        // If incomplete team data reaches this calculator,
        // we safely treat negative experience as zero.
        var candidateYears = Math.Max(
            MatchingConstants.MinimumScore,
            input.CandidateExperienceYears);

        var requiredYears = Math.Max(
            MatchingConstants.MinimumScore,
            input.RequiredExperienceYears);

        decimal rawScore;
        string explanation;

        // ---------------------------------------------------------
        // CASE 1: JOB REQUIRES NO EXPERIENCE
        // ---------------------------------------------------------
        if (requiredYears == 0)
        {
            rawScore = MatchingConstants.MaximumScore;

            explanation =
                "This job does not require previous experience.";
        }

        // ---------------------------------------------------------
        // CASE 2: CANDIDATE MEETS OR EXCEEDS REQUIREMENT
        // ---------------------------------------------------------
        else if (candidateYears >= requiredYears)
        {
            rawScore = MatchingConstants.MaximumScore;

            explanation =
                $"Candidate experience ({candidateYears:0.##} year(s)) " +
                $"meets or exceeds the required experience " +
                $"({requiredYears:0.##} year(s)).";
        }

        // ---------------------------------------------------------
        // CASE 3: CANDIDATE IS BELOW REQUIREMENT
        // ---------------------------------------------------------
        else
        {
            rawScore =
                candidateYears /
                requiredYears *
                MatchingConstants.MaximumScore;

            explanation =
                $"Candidate has {candidateYears:0.##} year(s) of experience " +
                $"out of the required {requiredYears:0.##} year(s).";
        }

        // Keep the final score safely inside the valid 0–100 range.
        rawScore = Math.Clamp(
            rawScore,
            MatchingConstants.MinimumScore,
            MatchingConstants.MaximumScore);

        rawScore = Math.Round(rawScore, 2);

        return new CategoryScoreResult
        {
            RawScore = rawScore,
            Status = GetStatus(rawScore),
            Explanation = explanation
        };
    }

    /// <summary>
    /// Converts the numeric score into the common
    /// matching explanation status.
    ///
    /// These thresholds use the central MatchingConstants values,
    /// so future team changes can be handled in one place.
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