using NexHire.Application.Common;
using NexHire.Application.Matching;
using NexHire.Infrastructure.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Verifies that MatchingEngine correctly connects
/// the individual matching components through one facade.
/// </summary>
public class MatchingEngineTests
{
    private readonly MatchingEngine _engine = CreateEngine();

    [Fact]
    public void EvaluateEligibility_WithValidInput_ReturnsEligible()
    {
        var input = new EligibilityInput
        {
            IsJobAvailable = true,
            CandidateExperienceYears = 3m,
            MinimumExperienceYears = 2m,
            CandidateEducationLevel = 3,
            MinimumEducationLevel = 2
        };

        var result = _engine.EvaluateEligibility(input);

        Assert.True(result.IsEligible);
        Assert.Empty(result.FailureReasons);
    }

    [Fact]
    public void CalculateSkills_WhenNoSkillsAreRequired_ReturnsFullScore()
    {
        var input = new SkillMatchInput();

        var result = _engine.CalculateSkills(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void CalculateWeightedScore_WithValidWeights_ReturnsExpectedScore()
    {
        var input = new MatchScoreCalculationInput
        {
            Skills = CreateCategory(80m),
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

        var result = _engine.CalculateWeightedScore(input);

        Assert.Equal(80m, result.TotalScore);
        Assert.Equal(7, result.ScoreDetails.Count);
    }

    [Fact]
    public void GetRecommendation_WithHighEligibleScore_ReturnsHighlyRecommended()
    {
        var result = _engine.GetRecommendation(
            totalScore: 95m,
            isEligible: true);

        Assert.Equal(
            MatchingConstants.HighlyRecommended,
            result);
    }

    [Fact]
    public void RankCandidates_WithTie_UsesCompetitionRanking()
    {
        var candidates = new List<CandidateRankingInput>
        {
            CreateCandidate(95m),
            CreateCandidate(90m),
            CreateCandidate(90m),
            CreateCandidate(80m)
        };

        var result = _engine.RankCandidates(candidates);

        Assert.Equal(1, result[0].Rank);
        Assert.Equal(2, result[1].Rank);
        Assert.Equal(2, result[2].Rank);
        Assert.Equal(4, result[3].Rank);
    }

    private static MatchingEngine CreateEngine()
    {
        return new MatchingEngine(
            new EligibilityEngine(),
            new SkillMatchCalculator(),
            new ExperienceMatchCalculator(),
            new EducationMatchCalculator(),
            new CertificationMatchCalculator(),
            new LocationMatchCalculator(),
            new ProjectMatchCalculator(),
            new ProfileCompletionMatchCalculator(),
            new MatchScoreCalculator(),
            new RecommendationEngine(),
            new CandidateRankingEngine(),
            new CandidateComparisonEngine());
    }

    private static CategoryScoreResult CreateCategory(
        decimal rawScore)
    {
        return new CategoryScoreResult
        {
            RawScore = rawScore,
            Status = "Test",
            Explanation = "Test explanation"
        };
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