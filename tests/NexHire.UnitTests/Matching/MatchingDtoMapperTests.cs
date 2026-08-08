using NexHire.Application.Mappings;
using NexHire.Application.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Verifies that matching results are correctly
/// converted into API response DTOs.
/// </summary>
public class MatchingDtoMapperTests
{
    [Fact]
    public void ToMatchScoreResponseDto_MapsMainResultFields()
    {
        var result = CreateMatchingResult();

        var dto =
            MatchingDtoMapper.ToMatchScoreResponseDto(result);

        Assert.Equal(
            result.JobSeekerProfileId,
            dto.JobSeekerProfileId);

        Assert.Equal(result.JobId, dto.JobId);
        Assert.Equal(result.TotalScore, dto.TotalScore);
        Assert.Equal(result.IsEligible, dto.IsEligible);
        Assert.Equal(result.Recommendation, dto.Recommendation);
        Assert.Equal(result.Summary, dto.Summary);

        Assert.Single(dto.ScoreDetails);
        Assert.Equal(
            "Skills",
            dto.ScoreDetails[0].Category);
    }

    [Fact]
    public void ToCandidateRankingDto_MapsRankingFields()
    {
        var result = new CandidateRankingResult
        {
            ApplicationId = Guid.NewGuid(),
            JobSeekerProfileId = Guid.NewGuid(),
            Rank = 2,
            TotalScore = 88m,
            IsEligible = true,
            IsTied = true,
            Recommendation = "Recommended for Review"
        };

        var dto =
            MatchingDtoMapper.ToCandidateRankingDto(result);

        Assert.Equal(result.ApplicationId, dto.ApplicationId);
        Assert.Equal(
            result.JobSeekerProfileId,
            dto.JobSeekerProfileId);

        Assert.Equal(result.Rank, dto.Rank);
        Assert.Equal(result.TotalScore, dto.TotalScore);
        Assert.Equal(result.IsEligible, dto.IsEligible);
        Assert.Equal(result.IsTied, dto.IsTied);
        Assert.Equal(
            result.Recommendation,
            dto.Recommendation);
    }

    [Fact]
    public void ToCandidateComparisonDto_MapsScoreDetails()
    {
        var result = new CandidateComparisonResult
        {
            ApplicationId = Guid.NewGuid(),
            JobSeekerProfileId = Guid.NewGuid(),
            TotalScore = 75m,
            IsEligible = true,
            Recommendation = "Recommended for Review",
            ScoreDetails = new()
            {
                CreateScoreDetail()
            }
        };

        var dto =
            MatchingDtoMapper.ToCandidateComparisonDto(result);

        Assert.Equal(result.ApplicationId, dto.ApplicationId);
        Assert.Equal(result.TotalScore, dto.TotalScore);
        Assert.Single(dto.ScoreDetails);

        Assert.Equal(
            result.ScoreDetails[0].WeightedPoints,
            dto.ScoreDetails[0].WeightedPoints);
    }

    [Fact]
    public void ToExplainMatchDto_MapsExplanationInformation()
    {
        var applicationId = Guid.NewGuid();
        var result = CreateMatchingResult();

        var dto =
            MatchingDtoMapper.ToExplainMatchDto(
                applicationId,
                result);

        Assert.Equal(applicationId, dto.ApplicationId);
        Assert.Equal(result.TotalScore, dto.TotalScore);
        Assert.Equal(result.IsEligible, dto.IsEligible);

        Assert.Equal(
            result.Strengths,
            dto.Strengths);

        Assert.Equal(
            result.ImprovementAreas,
            dto.ImprovementAreas);

        Assert.Equal(
            result.MatchedMandatorySkills,
            dto.MatchedMandatorySkills);

        Assert.Equal(
            result.MissingMandatorySkills,
            dto.MissingMandatorySkills);

        Assert.Equal(
            result.EligibilityFailures,
            dto.EligibilityFailures);

        Assert.Single(dto.ScoreDetails);
    }

    private static MatchingCalculationResult CreateMatchingResult()
    {
        return new MatchingCalculationResult
        {
            JobSeekerProfileId = Guid.NewGuid(),
            JobId = Guid.NewGuid(),
            TotalScore = 92m,
            IsEligible = true,
            Recommendation = "Highly Recommended",
            Summary = "Test matching summary.",

            Strengths = new()
            {
                "Skills",
                "Experience"
            },

            ImprovementAreas = new()
            {
                "Profile Completion"
            },

            MatchedMandatorySkills = new()
            {
                "C#"
            },

            MissingMandatorySkills = new(),

            EligibilityFailures = new(),

            ScoreDetails = new()
            {
                CreateScoreDetail()
            }
        };
    }

    private static WeightedCategoryScoreResult CreateScoreDetail()
    {
        return new WeightedCategoryScoreResult
        {
            Category = "Skills",
            RawScore = 90m,
            Weight = 30m,
            WeightedPoints = 27m,
            MaximumWeightedPoints = 30m,
            Status = "Strong",
            Explanation = "Candidate matches required skills."
        };
    }
}