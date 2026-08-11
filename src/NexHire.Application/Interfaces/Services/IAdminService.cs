using NexHire.Application.DTOs.Admin;
namespace NexHire.Application.Interfaces.Services;
public interface IAdminService
{
    Task<IReadOnlyList<MemberResponseDto>> GetMembersAsync(CancellationToken ct=default);
    Task<MemberResponseDto?> GetMemberAsync(Guid id,CancellationToken ct=default);
    Task<MemberResponseDto> CreateMemberAsync(CreateMemberRequestDto dto,CancellationToken ct=default);
    Task<MemberResponseDto?> UpdateMemberAsync(Guid id,UpdateMemberRequestDto dto,CancellationToken ct=default);
    Task<bool> ChangeStatusAsync(Guid id,string status,Guid currentAdminId,CancellationToken ct=default);
    Task<bool> ResetPasswordAsync(Guid id,AdminResetPasswordRequestDto dto,CancellationToken ct=default);
    Task<bool> DeactivateAsync(Guid id,Guid currentAdminId,CancellationToken ct=default);
}
