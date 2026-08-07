using NexHire.Application.Matching;

namespace NexHire.Application.Interfaces.Repositories;

/// <summary>
/// Defines the data required by the matching module.
///
/// The repository hides the actual database entities
/// from the matching business logic.
///
/// After the team entities are finalized, the
/// Infrastructure repository will map Job, Candidate,
/// Skills and other database data into
/// MatchingCalculationInput.
/// </summary>
public interface IMatchingRepository
{
    /// <summary>
    /// Loads all information required to calculate
    /// one candidate-to-job match.
    ///
    /// Returns null when the requested candidate
    /// or job cannot be found.
    /// </summary>
    Task<MatchingCalculationInput?> GetMatchingCalculationInputAsync(
        Guid jobId,
        Guid jobSeekerProfileId,
        CancellationToken cancellationToken = default);
}