using NexHire.Application.Common;
using NexHire.Application.Matching;
using NexHire.Application.Services;
using NexHire.Infrastructure.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Verifies the complete MatchingService orchestration flow.
/// </summary>
public class MatchingServiceTests
{
    [Fact]
    public void CalculateMatch_WithEligibleCandidate_ReturnsCompleteResult()
    {
        var service = CreateService();

        var input = CreateValidInput();

        input.ProfileCompletion.CompletionPercentage = 50m;

        var result = service.CalculateMatch(input);

        Assert.True(result.IsEligible);
        Assert.Equal(97.5m, result.TotalScore);
        Assert.Equal(
            MatchingConstants.HighlyRecommended,
            result.Recommendation);

        Assert.Equal(7, result.ScoreDetails.Count);
        Assert.Empty(result.EligibilityFailures);
        Assert.Contains("eligible", result.Summary);
    }

    [Fact]
    public void CalculateMatch_WhenEligibilityFails_StillCalculatesScore()
    {
        var service = CreateService();

        var input = CreateValidInput();

        input.Eligibility.IsJobAvailable = false;
        input.Eligibility.JobUnavailableReason =
            "Job is closed.";

        var result = service.CalculateMatch(input);

        Assert.False(result.IsEligible);

        // Score is still calculated for explanation purposes.
        Assert.Equal(100m, result.TotalScore);

        Assert.Equal(
            MatchingConstants.NotEligible,
            result.Recommendation);

        Assert.NotEmpty(result.EligibilityFailures);
        Assert.Contains("not eligible", result.Summary);
    }

    [Fact]
    public void RankCandidates_ReturnsCompetitionRanking()
    {
        var service = CreateService();

        var candidates = new List<CandidateRankingInput>
        {
            CreateRankingCandidate(95m),
            CreateRankingCandidate(90m),
            CreateRankingCandidate(90m),
            CreateRankingCandidate(80m)
        };

        var result = service.RankCandidates(candidates);

        Assert.Equal(1, result[0].Rank);
        Assert.Equal(2, result[1].Rank);
        Assert.Equal(2, result[2].Rank);
        Assert.Equal(4, result[3].Rank);
    }

    [Fact]
    public void CompareCandidates_ReturnsHigherScoreFirst()
    {
        var service = CreateService();

        var lowerScoreCandidate =
            CreateComparisonCandidate(70m);

        var higherScoreCandidate =
            CreateComparisonCandidate(90m);

        var candidates = new List<CandidateComparisonInput>
        {
            lowerScoreCandidate,
            higherScoreCandidate
        };

        var result = service.CompareCandidates(candidates);

        Assert.Equal(2, result.Count);

        Assert.Equal(
            higherScoreCandidate.ApplicationId,
            result[0].ApplicationId);

        Assert.Equal(
            lowerScoreCandidate.ApplicationId,
            result[1].ApplicationId);
    }

    private static MatchingService CreateService()
    {
        var engine = new MatchingEngine(
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

        return new MatchingService(engine);
    }

    private static MatchingCalculationInput CreateValidInput()
    {
        return new MatchingCalculationInput
        {
            JobSeekerProfileId = Guid.NewGuid(),
            JobId = Guid.NewGuid(),

            Eligibility = new EligibilityInput
            {
                IsJobAvailable = true,
                CandidateExperienceYears = 3m,
                MinimumExperienceYears = 2m,
                CandidateEducationLevel = 3,
                MinimumEducationLevel = 2
            },

            Skills = new SkillMatchInput(),

            Experience = new ExperienceMatchInput
            {
                CandidateExperienceYears = 3m,
                RequiredExperienceYears = 2m
            },

            Education = new EducationMatchInput
            {
                CandidateEducationLevel = 3,
                RequiredEducationLevel = 2
            },

            Certification = new CertificationMatchInput(),

            Location = new LocationMatchInput(),

            Projects = new ProjectMatchInput(),

            ProfileCompletion =
                new ProfileCompletionMatchInput
                {
                    CompletionPercentage = 100m
                },

            SkillsWeight = 30m,
            ExperienceWeight = 20m,
            EducationWeight = 15m,
            CertificationWeight = 10m,
            LocationWeight = 10m,
            ProjectsWeight = 10m,
            ProfileCompletionWeight = 5m
        };
    }

    private static CandidateRankingInput CreateRankingCandidate(
        decimal score)
    {
        return new CandidateRankingInput
        {
            ApplicationId = Guid.NewGuid(),
            JobSeekerProfileId = Guid.NewGuid(),
            TotalScore = score,
            IsEligible = true,
            Recommendation = "Test"
        };
    }

    private static CandidateComparisonInput CreateComparisonCandidate(
        decimal score)
    {
        return new CandidateComparisonInput
        {
            ApplicationId = Guid.NewGuid(),
            JobSeekerProfileId = Guid.NewGuid(),
            TotalScore = score,
            IsEligible = true,
            Recommendation = "Test",
            ScoreDetails = new()
        };
    }
}