using NexHire.Application.Common;
using NexHire.Infrastructure.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Tests the deterministic recommendation rules.
/// </summary>
public class RecommendationEngineTests
{
    private readonly RecommendationEngine _engine = new();

    [Fact]
    public void GetRecommendation_WhenCandidateIsNotEligible_ReturnsNotEligible()
    {
        var result = _engine.GetRecommendation(
            totalScore: 100m,
            isEligible: false);

        Assert.Equal(
            MatchingConstants.NotEligible,
            result);
    }

    [Fact]
    public void GetRecommendation_WhenScoreIsNinety_ReturnsHighlyRecommended()
    {
        var result = _engine.GetRecommendation(
            totalScore: 90m,
            isEligible: true);

        Assert.Equal(
            MatchingConstants.HighlyRecommended,
            result);
    }

    [Fact]
    public void GetRecommendation_WhenScoreIsSeventyFive_ReturnsRecommendedForReview()
    {
        var result = _engine.GetRecommendation(
            totalScore: 75m,
            isEligible: true);

        Assert.Equal(
            MatchingConstants.RecommendedForReview,
            result);
    }

    [Fact]
    public void GetRecommendation_WhenScoreIsSixty_ReturnsConsiderWithCaution()
    {
        var result = _engine.GetRecommendation(
            totalScore: 60m,
            isEligible: true);

        Assert.Equal(
            MatchingConstants.ConsiderWithCaution,
            result);
    }

    [Fact]
    public void GetRecommendation_WhenScoreIsBelowSixty_ReturnsNotRecommended()
    {
        var result = _engine.GetRecommendation(
            totalScore: 59.99m,
            isEligible: true);

        Assert.Equal(
            MatchingConstants.NotRecommended,
            result);
    }

    [Fact]
    public void GetRecommendation_WhenScoreIsAboveOneHundred_ClampsToMaximum()
    {
        var result = _engine.GetRecommendation(
            totalScore: 150m,
            isEligible: true);

        Assert.Equal(
            MatchingConstants.HighlyRecommended,
            result);
    }

    [Fact]
    public void GetRecommendation_WhenScoreIsBelowZero_ClampsToMinimum()
    {
        var result = _engine.GetRecommendation(
            totalScore: -20m,
            isEligible: true);

        Assert.Equal(
            MatchingConstants.NotRecommended,
            result);
    }
}