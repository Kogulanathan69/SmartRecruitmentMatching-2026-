using NexHire.Application.Common;
using NexHire.Application.Matching;

namespace NexHire.Infrastructure.Matching;

/// <summary>
/// Calculates the Projects category score.
///
/// This calculator uses only the number of relevant projects
/// and the target number of projects expected for the job.
///
/// It does not depend directly on the team's database entities,
/// so final Project model changes can be handled in the mapping layer.
/// </summary>
public class ProjectMatchCalculator
{
    /// <summary>
    /// Calculates a deterministic project score from 0 to 100.
    ///
    /// Formula:
    /// relevant projects / target projects * 100
    ///
    /// Rules:
    /// - No target project requirement = 100.
    /// - Candidate meets or exceeds target = 100.
    /// - Candidate below target receives a proportional score.
    /// - Negative values are treated as zero.
    /// </summary>
    public CategoryScoreResult Calculate(ProjectMatchInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var relevantProjects = Math.Max(
            0,
            input.RelevantProjectCount);

        var targetProjects = Math.Max(
            0,
            input.TargetProjectCount);

        decimal rawScore;
        string explanation;

        // ---------------------------------------------------------
        // CASE 1: NO PROJECT TARGET
        // ---------------------------------------------------------
        if (targetProjects == 0)
        {
            rawScore = MatchingConstants.MaximumScore;

            explanation =
                "This job does not have a project experience target.";
        }

        // ---------------------------------------------------------
        // CASE 2: CANDIDATE MEETS OR EXCEEDS PROJECT TARGET
        // ---------------------------------------------------------
        else if (relevantProjects >= targetProjects)
        {
            rawScore = MatchingConstants.MaximumScore;

            explanation =
                $"Candidate has {relevantProjects} relevant project(s) " +
                $"and meets or exceeds the target of {targetProjects}.";
        }

        // ---------------------------------------------------------
        // CASE 3: CANDIDATE IS BELOW PROJECT TARGET
        // ---------------------------------------------------------
        else
        {
            rawScore =
                (decimal)relevantProjects /
                targetProjects *
                MatchingConstants.MaximumScore;

            explanation =
                $"Candidate has {relevantProjects} relevant project(s) " +
                $"out of the target {targetProjects}.";
        }

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