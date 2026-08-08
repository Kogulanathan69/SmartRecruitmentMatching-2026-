using NexHire.Application.DTOs.Auth;
namespace NexHire.Application.Interfaces.Services;
public interface IAuthService
{
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default);
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<LoginResponseDto?> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task<string?> ForgotPasswordAsync(string email, CancellationToken ct = default);
    Task<bool> ResetPasswordAsync(ResetPasswordRequestDto dto, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto dto, CancellationToken ct = default);
    Task<bool> LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task<CurrentUserResponseDto?> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
}
