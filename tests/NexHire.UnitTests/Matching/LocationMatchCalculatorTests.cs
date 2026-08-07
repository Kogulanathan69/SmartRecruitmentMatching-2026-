using NexHire.Application.Matching;
using NexHire.Infrastructure.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Tests the deterministic Location category calculation.
/// </summary>
public class LocationMatchCalculatorTests
{
    private readonly LocationMatchCalculator _calculator = new();

    [Fact]
    public void Calculate_WhenJobHasNoLocationRequirement_ReturnsFullScore()
    {
        var input = new LocationMatchInput
        {
            CandidateLocation = "Colombo",
            JobLocation = string.Empty
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenLocationsMatch_ReturnsFullScore()
    {
        var input = new LocationMatchInput
        {
            CandidateLocation = "Colombo",
            JobLocation = "Colombo"
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenLocationsUseDifferentCase_StillMatches()
    {
        var input = new LocationMatchInput
        {
            CandidateLocation = "colombo",
            JobLocation = "COLOMBO"
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenLocationsContainExtraSpaces_StillMatches()
    {
        var input = new LocationMatchInput
        {
            CandidateLocation = "  Colombo  ",
            JobLocation = "Colombo"
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenLocationsAreDifferent_ReturnsZero()
    {
        var input = new LocationMatchInput
        {
            CandidateLocation = "Kandy",
            JobLocation = "Colombo"
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(0m, result.RawScore);
        Assert.Equal("Needs Improvement", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateLocationIsMissingButJobLocationExists_ReturnsZero()
    {
        var input = new LocationMatchInput
        {
            CandidateLocation = string.Empty,
            JobLocation = "Colombo"
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(0m, result.RawScore);
        Assert.Equal("Needs Improvement", result.Status);
    }
}