using NexHire.Application.DTOs.Matching;

namespace NexHire.Application.Interfaces.Services;

public interface ICoreMatchingService
{
    Task<CoreMatchDto> CalculateAsync(Guid candidateProfileId, Guid jobId);
}
