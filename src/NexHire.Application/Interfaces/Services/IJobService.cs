using NexHire.Application.DTOs.Job;

namespace NexHire.Application.Interfaces.Services;

public interface IJobService
{
    Task<IReadOnlyList<JobResponseDto>> SearchAsync(
        JobSearchDto search,
        CancellationToken cancellationToken = default);

    Task<JobResponseDto> GetByIdAsync(
        Guid jobId,
        Guid userId,
        bool isEmployer,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobResponseDto>> GetMineAsync(
        Guid employerUserId,
        CancellationToken cancellationToken = default);

    Task<JobResponseDto> CreateAsync(
        Guid employerUserId,
        CreateJobDto dto,
        CancellationToken cancellationToken = default);

    Task<JobResponseDto> UpdateAsync(
        Guid jobId,
        Guid employerUserId,
        UpdateJobDto dto,
        CancellationToken cancellationToken = default);

    Task<JobResponseDto> CloseAsync(
        Guid jobId,
        Guid employerUserId,
        CancellationToken cancellationToken = default);
}
