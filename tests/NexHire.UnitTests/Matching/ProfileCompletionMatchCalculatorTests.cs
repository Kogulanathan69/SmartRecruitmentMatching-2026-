using NexHire.Application.Matching;
using NexHire.Infrastructure.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Tests the deterministic Profile Completion category calculation.
/// </summary>
public class ProfileCompletionMatchCalculatorTests
{
    private readonly ProfileCompletionMatchCalculator _calculator = new();

    [Fact]
    public void Calculate_WhenProfileIsFullyComplete_ReturnsFullScore()
    {
        var input = new ProfileCompletionMatchInput
        {
            CompletionPercentage = 100m
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenCompletionIsNinety_ReturnsStrong()
    {
        var input = new ProfileCompletionMatchInput
        {
            CompletionPercentage = 90m
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(90m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenCompletionIsSeventyFive_ReturnsGood()
    {
        var input = new ProfileCompletionMatchInput
        {
            CompletionPercentage = 75m
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(75m, result.RawScore);
        Assert.Equal("Good", result.Status);
    }

    [Fact]
    public void Calculate_WhenCompletionIsSixty_ReturnsPartial()
    {
        var input = new ProfileCompletionMatchInput
        {
            CompletionPercentage = 60m
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(60m, result.RawScore);
        Assert.Equal("Partial", result.Status);
    }

    [Fact]
    public void Calculate_WhenCompletionIsBelowSixty_ReturnsNeedsImprovement()
    {
        var input = new ProfileCompletionMatchInput
        {
            CompletionPercentage = 45m
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(45m, result.RawScore);
        Assert.Equal("Needs Improvement", result.Status);
    }

    [Fact]
    public void Calculate_WhenCompletionIsBelowZero_ClampsScoreToZero()
    {
        var input = new ProfileCompletionMatchInput
        {
            CompletionPercentage = -20m
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(0m, result.RawScore);
        Assert.Equal("Needs Improvement", result.Status);
    }

    [Fact]
    public void Calculate_WhenCompletionIsAboveOneHundred_ClampsScoreToOneHundred()
    {
        var input = new ProfileCompletionMatchInput
        {
            CompletionPercentage = 125m
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }
}