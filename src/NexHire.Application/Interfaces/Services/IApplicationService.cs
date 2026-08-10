using NexHire.Application.DTOs.Application;

namespace NexHire.Application.Interfaces.Services;

public interface IApplicationService
{
    Task<ApplicationResponseDto> ApplyAsync(
        Guid candidateUserId,
        ApplyJobDto dto,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationResponseDto>>
        GetMineAsync(
            Guid candidateUserId,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationResponseDto>>
        GetForJobAsync(
            Guid jobId,
            Guid requestingUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default);

    Task<ApplicationResponseDto> GetByIdAsync(
        Guid applicationId,
        Guid requestingUserId,
        bool isEmployer,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
