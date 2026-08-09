using NexHire.Application.Matching;

namespace NexHire.Application.Interfaces.Repositories;

public interface IMatchingRepository
{
    Task<MatchingCalculationInput?> GetMatchingCalculationInputAsync(
        Guid jobId,
        Guid jobSeekerProfileId,
        CancellationToken cancellationToken = default);

    Task SaveMatchResultAsync(
        MatchingCalculationResult result,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CandidateRankingInput>>
        GetRankingInputsForJobAsync(
            Guid jobId,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CandidateComparisonInput>>
        GetComparisonInputsForJobAsync(
            Guid jobId,
            IReadOnlyCollection<Guid> applicationIds,
            CancellationToken cancellationToken = default);

    Task<bool> EmployerOwnsJobAsync(
        Guid jobId,
        Guid employerUserId,
        CancellationToken cancellationToken = default);

    Task<(Guid JobId, Guid JobSeekerProfileId)?>
        GetApplicationMatchingTargetAsync(
            Guid applicationId,
            CancellationToken cancellationToken = default);
}
