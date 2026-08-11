using NexHire.Application.Matching;
using NexHire.Infrastructure.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Tests the deterministic Experience category calculation.
///
/// These tests make sure future changes in team entity models
/// do not accidentally change the experience scoring rules.
/// </summary>
public class ExperienceMatchCalculatorTests
{
    private readonly ExperienceMatchCalculator _calculator = new();

    [Fact]
    public void Calculate_WhenNoExperienceIsRequired_ReturnsFullScore()
    {
        var input = new ExperienceMatchInput
        {
            CandidateExperienceYears = 0,
            RequiredExperienceYears = 0
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateMeetsRequirement_ReturnsFullScore()
    {
        var input = new ExperienceMatchInput
        {
            CandidateExperienceYears = 3,
            RequiredExperienceYears = 3
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateExceedsRequirement_DoesNotExceedFullScore()
    {
        var input = new ExperienceMatchInput
        {
            CandidateExperienceYears = 8,
            RequiredExperienceYears = 4
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateHasHalfRequiredExperience_ReturnsFifty()
    {
        var input = new ExperienceMatchInput
        {
            CandidateExperienceYears = 2,
            RequiredExperienceYears = 4
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(50m, result.RawScore);
        Assert.Equal("Needs Improvement", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateHasThreeQuartersExperience_ReturnsGoodStatus()
    {
        var input = new ExperienceMatchInput
        {
            CandidateExperienceYears = 3,
            RequiredExperienceYears = 4
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(75m, result.RawScore);
        Assert.Equal("Good", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateExperienceIsNegative_TreatsItAsZero()
    {
        var input = new ExperienceMatchInput
        {
            CandidateExperienceYears = -2,
            RequiredExperienceYears = 4
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(0m, result.RawScore);
        Assert.Equal("Needs Improvement", result.Status);
    }
}