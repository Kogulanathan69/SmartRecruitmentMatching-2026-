using NexHire.Application.Matching;

namespace NexHire.Application.Interfaces.Services;

public interface IMatchingService
{
    MatchingCalculationResult CalculateMatch(
        MatchingCalculationInput input);

    Task<MatchingCalculationResult?> CalculateMatchAsync(
        Guid jobId,
        Guid jobSeekerProfileId,
        CancellationToken cancellationToken = default);

    Task<MatchingCalculationResult?> CalculateAndSaveMatchAsync(
        Guid jobId,
        Guid jobSeekerProfileId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CandidateRankingResult>>
        GetRankedCandidatesForJobAsync(
            Guid jobId,
            int top = 10,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CandidateComparisonResult>>
        CompareCandidatesForJobAsync(
            Guid jobId,
            IReadOnlyCollection<Guid> applicationIds,
            CancellationToken cancellationToken = default);

    List<CandidateRankingResult> RankCandidates(
        IEnumerable<CandidateRankingInput> candidates);

    List<CandidateComparisonResult> CompareCandidates(
        IEnumerable<CandidateComparisonInput> candidates);
}
