using NexHire.Application.Common;
using NexHire.Application.Matching;

namespace NexHire.Infrastructure.Matching;

/// <summary>
/// Calculates the final weighted matching score
/// using all seven matching categories.
///
/// Each category first produces a raw score from 0 to 100.
/// This calculator then applies the configured category weight.
///
/// Formula:
/// Weighted Points = Raw Score * Weight / 100
/// </summary>
public class MatchScoreCalculator
{
    /// <summary>
    /// Calculates the final score and detailed weighted
    /// result for every matching category.
    ///
    /// The total configured weight must equal 100 percent.
    /// </summary>
    public MatchScoreCalculationResult Calculate(
        MatchScoreCalculationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        ValidateWeights(input);

        var scoreDetails = new List<WeightedCategoryScoreResult>
        {
            CalculateCategory(
                MatchingConstants.Skills,
                input.Skills,
                input.SkillsWeight),

            CalculateCategory(
                MatchingConstants.Experience,
                input.Experience,
                input.ExperienceWeight),

            CalculateCategory(
                MatchingConstants.Education,
                input.Education,
                input.EducationWeight),

            CalculateCategory(
                MatchingConstants.Certification,
                input.Certification,
                input.CertificationWeight),

            CalculateCategory(
                MatchingConstants.Location,
                input.Location,
                input.LocationWeight),

            CalculateCategory(
                MatchingConstants.Projects,
                input.Projects,
                input.ProjectsWeight),

            CalculateCategory(
                MatchingConstants.ProfileCompletion,
                input.ProfileCompletion,
                input.ProfileCompletionWeight)
        };

        var totalScore = scoreDetails.Sum(
            detail => detail.WeightedPoints);

        totalScore = Math.Clamp(
            totalScore,
            MatchingConstants.MinimumScore,
            MatchingConstants.MaximumScore);

        totalScore = Math.Round(totalScore, 2);

        return new MatchScoreCalculationResult
        {
            TotalScore = totalScore,
            ScoreDetails = scoreDetails
        };
    }

    /// <summary>
    /// Calculates the weighted contribution of one category.
    /// </summary>
    private static WeightedCategoryScoreResult CalculateCategory(
        string category,
        CategoryScoreResult categoryResult,
        decimal weight)
    {
        ArgumentNullException.ThrowIfNull(categoryResult);

        var rawScore = Math.Clamp(
            categoryResult.RawScore,
            MatchingConstants.MinimumScore,
            MatchingConstants.MaximumScore);

        var weightedPoints =
            rawScore *
            weight /
            MatchingConstants.MaximumScore;

        rawScore = Math.Round(rawScore, 2);
        weightedPoints = Math.Round(weightedPoints, 2);

        return new WeightedCategoryScoreResult
        {
            Category = category,
            RawScore = rawScore,
            Weight = weight,
            WeightedPoints = weightedPoints,
            MaximumWeightedPoints = weight,
            Status = categoryResult.Status,
            Explanation = categoryResult.Explanation
        };
    }

    /// <summary>
    /// Protects the scoring engine from invalid
    /// matching-rule configurations.
    ///
    /// Every individual weight must be between 0 and 100,
    /// and all seven weights together must equal 100.
    /// </summary>
    private static void ValidateWeights(
        MatchScoreCalculationInput input)
    {
        var weights = new[]
        {
            input.SkillsWeight,
            input.ExperienceWeight,
            input.EducationWeight,
            input.CertificationWeight,
            input.LocationWeight,
            input.ProjectsWeight,
            input.ProfileCompletionWeight
        };

        if (weights.Any(weight =>
                weight < MatchingConstants.MinimumScore ||
                weight > MatchingConstants.MaximumScore))
        {
            throw new ArgumentException(
                "Each matching category weight must be between 0 and 100.",
                nameof(input));
        }

        if (!input.HasValidTotalWeight())
        {
            throw new ArgumentException(
                $"Matching category weights must total 100. " +
                $"Current total is {input.TotalWeight:0.##}.",
                nameof(input));
        }
    }
}