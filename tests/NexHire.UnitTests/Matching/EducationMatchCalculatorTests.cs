using NexHire.Application.Matching;
using NexHire.Infrastructure.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Tests the deterministic Education category calculation.
/// </summary>
public class EducationMatchCalculatorTests
{
    private readonly EducationMatchCalculator _calculator = new();

    [Fact]
    public void Calculate_WhenNoEducationIsRequired_ReturnsFullScore()
    {
        var input = new EducationMatchInput
        {
            CandidateEducationLevel = 0,
            RequiredEducationLevel = 0
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateMeetsRequirement_ReturnsFullScore()
    {
        var input = new EducationMatchInput
        {
            CandidateEducationLevel = 3,
            RequiredEducationLevel = 3
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateExceedsRequirement_ReturnsFullScore()
    {
        var input = new EducationMatchInput
        {
            CandidateEducationLevel = 4,
            RequiredEducationLevel = 2
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateIsBelowRequirement_ReturnsZero()
    {
        var input = new EducationMatchInput
        {
            CandidateEducationLevel = 2,
            RequiredEducationLevel = 3
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(0m, result.RawScore);
        Assert.Equal("Needs Improvement", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateLevelIsNegative_TreatsItAsZero()
    {
        var input = new EducationMatchInput
        {
            CandidateEducationLevel = -1,
            RequiredEducationLevel = 2
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(0m, result.RawScore);
        Assert.Equal("Needs Improvement", result.Status);
    }
}