using NexHire.Application.DTOs.Application;
using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces.Repositories;

public interface IApplicationRepository
{
    Task<bool> ExistsAsync(
        Guid candidateUserId,
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task<JobApplication?> GetEntityByIdAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default);

    Task<ApplicationResponseDto?> GetResponseByIdAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationResponseDto>>
        GetByCandidateAsync(
            Guid candidateUserId,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationResponseDto>>
        GetByJobAsync(
            Guid jobId,
            CancellationToken cancellationToken = default);

    Task AddAsync(
        JobApplication application,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
