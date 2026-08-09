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
}
