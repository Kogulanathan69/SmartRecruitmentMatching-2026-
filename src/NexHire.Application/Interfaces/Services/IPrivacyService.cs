using NexHire.Application.DTOs.Privacy;
namespace NexHire.Application.Interfaces.Services;
public interface IPrivacyService
{
 Task<PrivacyPreferencesDto> GetPreferencesAsync(Guid userId);
 Task<PrivacyPreferencesDto> UpdatePreferencesAsync(Guid userId, PrivacyPreferencesDto dto);
 Task<DeletionRequestResponseDto> RequestDeletionAsync(Guid userId, DeletionRequestDto dto);
 Task<IReadOnlyList<DeletionRequestResponseDto>> GetDeletionRequestsAsync(Guid userId);
 Task<DeletionRequestResponseDto> ReviewDeletionAsync(Guid adminUserId, Guid requestId, string status);
}
