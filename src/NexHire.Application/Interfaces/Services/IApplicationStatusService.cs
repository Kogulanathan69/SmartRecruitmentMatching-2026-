using NexHire.Application.DTOs.ApplicationStatus;

namespace NexHire.Application.Interfaces.Services;

public interface IApplicationStatusService
{
    Task<ApplicationStatusResultDto> UpdateByEmployerAsync(
        Guid applicationId,
        Guid employerUserId,
        UpdateApplicationStatusRequestDto dto);

    Task<ApplicationStatusResultDto> WithdrawByCandidateAsync(
        Guid applicationId,
        Guid candidateUserId,
        WithdrawApplicationRequestDto dto);

    Task<IReadOnlyList<ApplicationStatusHistoryDto>> GetCandidateHistoryAsync(
        Guid applicationId,
        Guid candidateUserId);

    Task<IReadOnlyList<ApplicationStatusHistoryDto>> GetEmployerHistoryAsync(
        Guid applicationId,
        Guid employerUserId);
}