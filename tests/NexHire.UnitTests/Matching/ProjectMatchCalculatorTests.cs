using NexHire.Application.Matching;
using NexHire.Infrastructure.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Tests the deterministic Projects category calculation.
/// </summary>
public class ProjectMatchCalculatorTests
{
    private readonly ProjectMatchCalculator _calculator = new();

    [Fact]
    public void Calculate_WhenNoProjectTargetIsRequired_ReturnsFullScore()
    {
        var input = new ProjectMatchInput
        {
            RelevantProjectCount = 0,
            TargetProjectCount = 0
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateMeetsTarget_ReturnsFullScore()
    {
        var input = new ProjectMatchInput
        {
            RelevantProjectCount = 4,
            TargetProjectCount = 4
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateExceedsTarget_ReturnsFullScore()
    {
        var input = new ProjectMatchInput
        {
            RelevantProjectCount = 6,
            TargetProjectCount = 4
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateHasThreeOfFourProjects_ReturnsSeventyFive()
    {
        var input = new ProjectMatchInput
        {
            RelevantProjectCount = 3,
            TargetProjectCount = 4
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(75m, result.RawScore);
        Assert.Equal("Good", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateHasTwoOfFourProjects_ReturnsFifty()
    {
        var input = new ProjectMatchInput
        {
            RelevantProjectCount = 2,
            TargetProjectCount = 4
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(50m, result.RawScore);
        Assert.Equal("Needs Improvement", result.Status);
    }

    [Fact]
    public void Calculate_WhenProjectCountIsNegative_TreatsItAsZero()
    {
        var input = new ProjectMatchInput
        {
            RelevantProjectCount = -2,
            TargetProjectCount = 4
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(0m, result.RawScore);
        Assert.Equal("Needs Improvement", result.Status);
    }
}