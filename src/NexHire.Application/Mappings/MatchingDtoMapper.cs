using NexHire.Application.DTOs.Matching;
using NexHire.Application.Matching;

namespace NexHire.Application.Mappings;

/// <summary>
/// Converts internal matching results into
/// API-friendly DTO objects.
///
/// Keeping this mapping in one place prevents
/// controllers from containing repeated mapping code.
/// </summary>
public static class MatchingDtoMapper
{
    public static MatchScoreResponseDto ToMatchScoreResponseDto(
        MatchingCalculationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new MatchScoreResponseDto
        {
            JobSeekerProfileId =
                result.JobSeekerProfileId,

            JobId =
                result.JobId,

            TotalScore =
                result.TotalScore,

            IsEligible =
                result.IsEligible,

            Recommendation =
                result.Recommendation,

            Summary =
                result.Summary,

            ScoreDetails = result.ScoreDetails
                .Select(ToMatchScoreDetailDto)
                .ToList()
        };
    }

    public static CandidateRankingDto ToCandidateRankingDto(
        CandidateRankingResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new CandidateRankingDto
        {
            ApplicationId =
                result.ApplicationId,

            JobSeekerProfileId =
                result.JobSeekerProfileId,

            Rank =
                result.Rank,

            TotalScore =
                result.TotalScore,

            IsEligible =
                result.IsEligible,

            IsTied =
                result.IsTied,

            Recommendation =
                result.Recommendation
        };
    }

    public static CandidateComparisonDto ToCandidateComparisonDto(
        CandidateComparisonResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new CandidateComparisonDto
        {
            ApplicationId =
                result.ApplicationId,

            JobSeekerProfileId =
                result.JobSeekerProfileId,

            TotalScore =
                result.TotalScore,

            IsEligible =
                result.IsEligible,

            Recommendation =
                result.Recommendation,

            ScoreDetails = result.ScoreDetails
                .Select(ToMatchScoreDetailDto)
                .ToList()
        };
    }

    public static ExplainMatchDto ToExplainMatchDto(
        Guid applicationId,
        MatchingCalculationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new ExplainMatchDto
        {
            ApplicationId =
                applicationId,

            TotalScore =
                result.TotalScore,

            IsEligible =
                result.IsEligible,

            Recommendation =
                result.Recommendation,

            Summary =
                result.Summary,

            Strengths =
                result.Strengths.ToList(),

            ImprovementAreas =
                result.ImprovementAreas.ToList(),

            MatchedMandatorySkills =
                result.MatchedMandatorySkills.ToList(),

            MissingMandatorySkills =
                result.MissingMandatorySkills.ToList(),

            EligibilityFailures =
                result.EligibilityFailures.ToList(),

            ScoreDetails = result.ScoreDetails
                .Select(ToMatchScoreDetailDto)
                .ToList()
        };
    }

    private static MatchScoreDetailDto ToMatchScoreDetailDto(
        WeightedCategoryScoreResult detail)
    {
        return new MatchScoreDetailDto
        {
            Category =
                detail.Category,

            RawScore =
                detail.RawScore,

            Weight =
                detail.Weight,

            WeightedPoints =
                detail.WeightedPoints,

            MaximumWeightedPoints =
                detail.MaximumWeightedPoints,

            Status =
                detail.Status,

            Explanation =
                detail.Explanation
        };
    }
}