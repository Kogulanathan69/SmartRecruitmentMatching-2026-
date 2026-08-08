using NexHire.Application.Matching;

namespace NexHire.Application.Interfaces.Services;

/// <summary>
/// Defines the application-level matching operations.
///
/// The service coordinates eligibility, scoring,
/// recommendation, ranking and comparison without
/// depending directly on database entities.
/// </summary>
public interface IMatchingService
{
    /// <summary>
    /// Calculates the complete match between
    /// one candidate profile and one job.
    /// </summary>
    MatchingCalculationResult CalculateMatch(
        MatchingCalculationInput input);

    /// <summary>
    /// Loads the candidate and job data from the database,
    /// then calculates the complete match.
    /// Returns null when the job or candidate profile is not found.
    /// </summary>
    Task<MatchingCalculationResult?> CalculateMatchAsync(
        Guid jobId,
        Guid jobSeekerProfileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ranks candidate applications using
    /// competition ranking such as 1, 2, 2, 4.
    /// </summary>
    List<CandidateRankingResult> RankCandidates(
        IEnumerable<CandidateRankingInput> candidates);

    /// <summary>
    /// Compares between 2 and 4 candidate applications.
    /// </summary>
    List<CandidateComparisonResult> CompareCandidates(
        IEnumerable<CandidateComparisonInput> candidates);
}