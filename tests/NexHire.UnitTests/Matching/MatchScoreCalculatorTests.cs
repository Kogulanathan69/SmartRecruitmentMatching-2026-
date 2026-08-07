using NexHire.Application.Matching;
using NexHire.Infrastructure.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Tests the final weighted matching score calculation.
///
/// The weight values used in these tests are sample test values only.
/// They are NOT the final production matching-rule weights.
/// Production weights will later come from the active MatchingRule.
/// </summary>
public class MatchScoreCalculatorTests
{
    private readonly MatchScoreCalculator _calculator = new();

    [Fact]
    public void Calculate_WithValidScoresAndWeights_ReturnsCorrectTotalScore()
    {
        var input = CreateValidInput();

        var result = _calculator.Calculate(input);

        // Calculation:
        // Skills:             80 * 30 / 100 = 24
        // Experience:        100 * 20 / 100 = 20
        // Education:          60 * 15 / 100 = 9
        // Certification:      50 * 10 / 100 = 5
        // Location:          100 * 10 / 100 = 10
        // Projects:           75 * 10 / 100 = 7.5
        // Profile Completion: 90 *  5 / 100 = 4.5
        //
        // Total = 80

        Assert.Equal(80m, result.TotalScore);
    }

    [Fact]
    public void Calculate_WithValidInput_ReturnsAllSevenCategoryDetails()
    {
        var input = CreateValidInput();

        var result = _calculator.Calculate(input);

        Assert.Equal(7, result.ScoreDetails.Count);

        Assert.Contains(
            result.ScoreDetails,
            detail => detail.Category == "Skills");

        Assert.Contains(
            result.ScoreDetails,
            detail => detail.Category == "Experience");

        Assert.Contains(
            result.ScoreDetails,
            detail => detail.Category == "Education");

        Assert.Contains(
            result.ScoreDetails,
            detail => detail.Category == "Certification");

        Assert.Contains(
            result.ScoreDetails,
            detail => detail.Category == "Location");

        Assert.Contains(
            result.ScoreDetails,
            detail => detail.Category == "Projects");

        Assert.Contains(
            result.ScoreDetails,
            detail => detail.Category == "Profile Completion");
    }

    [Fact]
    public void Calculate_PreservesCategoryStatusAndExplanation()
    {
        var input = CreateValidInput();

        var result = _calculator.Calculate(input);

        var skillsDetail = result.ScoreDetails.Single(
            detail => detail.Category == "Skills");

        Assert.Equal(80m, skillsDetail.RawScore);
        Assert.Equal(30m, skillsDetail.Weight);
        Assert.Equal(24m, skillsDetail.WeightedPoints);
        Assert.Equal(30m, skillsDetail.MaximumWeightedPoints);
        Assert.Equal("Good", skillsDetail.Status);
        Assert.Equal(
            "Candidate matches most required skills.",
            skillsDetail.Explanation);
    }

    [Fact]
    public void Calculate_WhenTotalWeightIsNotOneHundred_ThrowsArgumentException()
    {
        var input = CreateValidInput();

        input.ProfileCompletionWeight = 0m;

        var exception = Assert.Throws<ArgumentException>(
            () => _calculator.Calculate(input));

        Assert.Contains(
            "must total 100",
            exception.Message);
    }

    [Fact]
    public void Calculate_WhenIndividualWeightIsOutsideValidRange_ThrowsArgumentException()
    {
        var input = CreateValidInput();

        // Total still equals 100, but individual values are invalid.
        input.SkillsWeight = 101m;
        input.ExperienceWeight = -51m;
        input.EducationWeight = 15m;
        input.CertificationWeight = 10m;
        input.LocationWeight = 10m;
        input.ProjectsWeight = 10m;
        input.ProfileCompletionWeight = 5m;

        var exception = Assert.Throws<ArgumentException>(
            () => _calculator.Calculate(input));

        Assert.Contains(
            "between 0 and 100",
            exception.Message);
    }

    [Fact]
    public void Calculate_WhenRawScoreIsAboveOneHundred_ClampsItToOneHundred()
    {
        var input = new MatchScoreCalculationInput
        {
            Skills = CreateCategory(
                150m,
                "Strong",
                "Test score above maximum."),

            Experience = CreateCategory(0m),
            Education = CreateCategory(0m),
            Certification = CreateCategory(0m),
            Location = CreateCategory(0m),
            Projects = CreateCategory(0m),
            ProfileCompletion = CreateCategory(0m),

            SkillsWeight = 100m,
            ExperienceWeight = 0m,
            EducationWeight = 0m,
            CertificationWeight = 0m,
            LocationWeight = 0m,
            ProjectsWeight = 0m,
            ProfileCompletionWeight = 0m
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.TotalScore);

        var skillsDetail = result.ScoreDetails.Single(
            detail => detail.Category == "Skills");

        Assert.Equal(100m, skillsDetail.RawScore);
        Assert.Equal(100m, skillsDetail.WeightedPoints);
    }

    private static MatchScoreCalculationInput CreateValidInput()
    {
        return new MatchScoreCalculationInput
        {
            Skills = CreateCategory(
                80m,
                "Good",
                "Candidate matches most required skills."),

            Experience = CreateCategory(
                100m,
                "Strong",
                "Experience requirement is fully satisfied."),

            Education = CreateCategory(
                60m,
                "Partial",
                "Education score calculated."),

            Certification = CreateCategory(
                50m,
                "Needs Improvement",
                "Certification score calculated."),

            Location = CreateCategory(
                100m,
                "Strong",
                "Location matches."),

            Projects = CreateCategory(
                75m,
                "Good",
                "Project score calculated."),

            ProfileCompletion = CreateCategory(
                90m,
                "Strong",
                "Profile is mostly complete."),

            // Sample unit-test weights only.
            SkillsWeight = 30m,
            ExperienceWeight = 20m,
            EducationWeight = 15m,
            CertificationWeight = 10m,
            LocationWeight = 10m,
            ProjectsWeight = 10m,
            ProfileCompletionWeight = 5m
        };
    }

    private static CategoryScoreResult CreateCategory(
        decimal rawScore,
        string status = "",
        string explanation = "")
    {
        return new CategoryScoreResult
        {
            RawScore = rawScore,
            Status = status,
            Explanation = explanation
        };
    }
}