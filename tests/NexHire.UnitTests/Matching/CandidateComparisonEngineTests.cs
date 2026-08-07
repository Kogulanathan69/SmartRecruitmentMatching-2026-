using NexHire.Application.Matching;
using NexHire.Infrastructure.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Tests candidate comparison validation and ordering.
/// </summary>
public class CandidateComparisonEngineTests
{
    private readonly CandidateComparisonEngine _engine = new();

    [Fact]
    public void Compare_WithTwoCandidates_ReturnsTwoCandidates()
    {
        var candidates = new List<CandidateComparisonInput>
        {
            CreateCandidate(80m),
            CreateCandidate(90m)
        };

        var result = _engine.Compare(candidates);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Compare_WithFourCandidates_ReturnsFourCandidates()
    {
        var candidates = new List<CandidateComparisonInput>
        {
            CreateCandidate(95m),
            CreateCandidate(85m),
            CreateCandidate(75m),
            CreateCandidate(65m)
        };

        var result = _engine.Compare(candidates);

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void Compare_WithLessThanTwoCandidates_ThrowsArgumentException()
    {
        var candidates = new List<CandidateComparisonInput>
        {
            CreateCandidate(90m)
        };

        var exception = Assert.Throws<ArgumentException>(
            () => _engine.Compare(candidates));

        Assert.Contains(
            "At least 2 candidates",
            exception.Message);
    }

    [Fact]
    public void Compare_WithMoreThanFourCandidates_ThrowsArgumentException()
    {
        var candidates = new List<CandidateComparisonInput>
        {
            CreateCandidate(95m),
            CreateCandidate(90m),
            CreateCandidate(85m),
            CreateCandidate(80m),
            CreateCandidate(75m)
        };

        var exception = Assert.Throws<ArgumentException>(
            () => _engine.Compare(candidates));

        Assert.Contains(
            "maximum of 4 candidates",
            exception.Message);
    }

    [Fact]
    public void Compare_WithDuplicateApplicationId_ThrowsArgumentException()
    {
        var duplicatedApplicationId = Guid.NewGuid();

        var candidates = new List<CandidateComparisonInput>
        {
            CreateCandidate(
                90m,
                duplicatedApplicationId),

            CreateCandidate(
                80m,
                duplicatedApplicationId)
        };

        var exception = Assert.Throws<ArgumentException>(
            () => _engine.Compare(candidates));

        Assert.Contains(
            "same application",
            exception.Message);
    }

    [Fact]
    public void Compare_OrdersCandidatesFromHighestScoreToLowestScore()
    {
        var candidates = new List<CandidateComparisonInput>
        {
            CreateCandidate(70m),
            CreateCandidate(95m),
            CreateCandidate(80m)
        };

        var result = _engine.Compare(candidates);

        Assert.Equal(95m, result[0].TotalScore);
        Assert.Equal(80m, result[1].TotalScore);
        Assert.Equal(70m, result[2].TotalScore);
    }

    [Fact]
    public void Compare_ClampsScoresToValidRange()
    {
        var candidates = new List<CandidateComparisonInput>
        {
            CreateCandidate(150m),
            CreateCandidate(-20m)
        };

        var result = _engine.Compare(candidates);

        Assert.Equal(100m, result[0].TotalScore);
        Assert.Equal(0m, result[1].TotalScore);
    }

    private static CandidateComparisonInput CreateCandidate(
        decimal totalScore,
        Guid? applicationId = null)
    {
        return new CandidateComparisonInput
        {
            ApplicationId =
                applicationId ?? Guid.NewGuid(),

            JobSeekerProfileId = Guid.NewGuid(),

            TotalScore = totalScore,

            IsEligible = true,

            Recommendation = "Test Recommendation",

            ScoreDetails = new List<WeightedCategoryScoreResult>()
        };
    }
}