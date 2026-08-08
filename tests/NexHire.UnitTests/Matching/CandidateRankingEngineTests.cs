using NexHire.Application.Matching;
using NexHire.Infrastructure.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Tests candidate competition ranking and tie detection.
/// </summary>
public class CandidateRankingEngineTests
{
    private readonly CandidateRankingEngine _engine = new();

    [Fact]
    public void Rank_WithTiedScores_UsesCompetitionRanking()
    {
        var candidates = new List<CandidateRankingInput>
        {
            CreateCandidate(95m),
            CreateCandidate(90m),
            CreateCandidate(90m),
            CreateCandidate(80m)
        };

        var results = _engine.Rank(candidates);

        Assert.Equal(4, results.Count);

        Assert.Equal(1, results[0].Rank);
        Assert.Equal(2, results[1].Rank);
        Assert.Equal(2, results[2].Rank);
        Assert.Equal(4, results[3].Rank);
    }

    [Fact]
    public void Rank_WhenCandidatesShareScore_MarksThemAsTied()
    {
        var candidates = new List<CandidateRankingInput>
        {
            CreateCandidate(95m),
            CreateCandidate(90m),
            CreateCandidate(90m),
            CreateCandidate(80m)
        };

        var results = _engine.Rank(candidates);

        var score95 = results.Single(
            result => result.TotalScore == 95m);

        var score90 = results.Where(
            result => result.TotalScore == 90m).ToList();

        var score80 = results.Single(
            result => result.TotalScore == 80m);

        Assert.False(score95.IsTied);

        Assert.All(
            score90,
            result => Assert.True(result.IsTied));

        Assert.False(score80.IsTied);
    }

    [Fact]
    public void Rank_WhenThereAreNoTies_AssignsSequentialRanks()
    {
        var candidates = new List<CandidateRankingInput>
        {
            CreateCandidate(90m),
            CreateCandidate(80m),
            CreateCandidate(70m)
        };

        var results = _engine.Rank(candidates);

        Assert.Equal(1, results[0].Rank);
        Assert.Equal(2, results[1].Rank);
        Assert.Equal(3, results[2].Rank);

        Assert.All(
            results,
            result => Assert.False(result.IsTied));
    }

    [Fact]
    public void Rank_WhenAllCandidatesHaveSameScore_AssignsRankOneToAll()
    {
        var candidates = new List<CandidateRankingInput>
        {
            CreateCandidate(85m),
            CreateCandidate(85m),
            CreateCandidate(85m)
        };

        var results = _engine.Rank(candidates);

        Assert.All(
            results,
            result => Assert.Equal(1, result.Rank));

        Assert.All(
            results,
            result => Assert.True(result.IsTied));
    }

    [Fact]
    public void Rank_WhenCandidateListIsEmpty_ReturnsEmptyResult()
    {
        var candidates = new List<CandidateRankingInput>();

        var results = _engine.Rank(candidates);

        Assert.Empty(results);
    }

    [Fact]
    public void Rank_WhenScoresAreOutsideValidRange_ClampsScores()
    {
        var candidates = new List<CandidateRankingInput>
        {
            CreateCandidate(150m),
            CreateCandidate(-20m)
        };

        var results = _engine.Rank(candidates);

        Assert.Equal(100m, results[0].TotalScore);
        Assert.Equal(1, results[0].Rank);

        Assert.Equal(0m, results[1].TotalScore);
        Assert.Equal(2, results[1].Rank);
    }

    private static CandidateRankingInput CreateCandidate(
        decimal totalScore)
    {
        return new CandidateRankingInput
        {
            ApplicationId = Guid.NewGuid(),
            JobSeekerProfileId = Guid.NewGuid(),
            TotalScore = totalScore,
            IsEligible = true,
            Recommendation = "Test Recommendation"
        };
    }
}