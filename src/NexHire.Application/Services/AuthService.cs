using NexHire.Application.DTOs.Auth;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Validators;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;

namespace NexHire.Application.Services;

public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher passwords,
    ITokenService tokens) : IAuthService
{
    public async Task<LoginResponseDto> RegisterAsync(
        RegisterRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await new RegisterRequestValidator().ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ArgumentException(
                string.Join(" ", validation.Errors.Select(error => error.ErrorMessage)));
        }

        var normalizedEmail = dto.Email.Trim().ToUpperInvariant();
        if (await users.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            NormalizedEmail = normalizedEmail,
            Role = Enum.Parse<UserRole>(dto.Role, ignoreCase: true),
            PasswordHash = passwords.Hash(dto.Password)
        };

        await users.AddUserAsync(user, cancellationToken);
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<LoginResponseDto?> LoginAsync(
        LoginRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToUpperInvariant();
        var user = await users.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null ||
            user.Status != UserStatus.Active ||
            !passwords.Verify(dto.Password, user.PasswordHash))
        {
            return null;
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<LoginResponseDto?> RefreshAsync(
        string rawRefreshToken,
        CancellationToken cancellationToken = default)
    {
        var oldToken = await users.GetRefreshTokenAsync(
            tokens.HashToken(rawRefreshToken),
            cancellationToken);

        if (oldToken is null || !oldToken.IsValid || oldToken.User.Status != UserStatus.Active)
        {
            return null;
        }

        oldToken.RevokedAtUtc = DateTime.UtcNow;
        return await IssueTokensAsync(oldToken.User, cancellationToken);
    }

    public async Task<string?> ForgotPasswordAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var user = await users.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var rawToken = tokens.CreateSecureToken();
        user.PasswordResetTokenHash = tokens.HashToken(rawToken);
        user.PasswordResetExpiresAtUtc = DateTime.UtcNow.AddMinutes(30);
        await users.SaveChangesAsync(cancellationToken);
        return rawToken;
    }

    public async Task<bool> ResetPasswordAsync(
        ResetPasswordRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!PasswordPolicy.IsStrong(dto.NewPassword) ||
            dto.NewPassword != dto.ConfirmNewPassword)
        {
            return false;
        }

        var user = await users.GetByResetTokenHashAsync(
            tokens.HashToken(dto.Token),
            cancellationToken);

        if (user is null || user.PasswordResetExpiresAtUtc <= DateTime.UtcNow)
        {
            return false;
        }

        user.PasswordHash = passwords.Hash(dto.NewPassword);
        user.PasswordResetTokenHash = null;
        user.PasswordResetExpiresAtUtc = null;
        RevokeAllRefreshTokens(user);
        await users.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!PasswordPolicy.IsStrong(dto.NewPassword) ||
            dto.NewPassword != dto.ConfirmNewPassword ||
            dto.CurrentPassword == dto.NewPassword)
        {
            return false;
        }

        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null || !passwords.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = passwords.Hash(dto.NewPassword);
        RevokeAllRefreshTokens(user);
        await users.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> LogoutAsync(
        string rawRefreshToken,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await users.GetRefreshTokenAsync(
            tokens.HashToken(rawRefreshToken),
            cancellationToken);

        if (refreshToken is null || refreshToken.RevokedAtUtc is not null)
        {
            return false;
        }

        refreshToken.RevokedAtUtc = DateTime.UtcNow;
        await users.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<CurrentUserResponseDto?> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        return user is null
            ? null
            : new CurrentUserResponseDto(
                user.Id,
                user.Email,
                user.FullName,
                user.Role.ToString(),
                user.Status.ToString());
    }

    private async Task<LoginResponseDto> IssueTokensAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var (accessToken, expiresAtUtc) = tokens.CreateAccessToken(user);
        var rawRefreshToken = tokens.CreateSecureToken();

        await users.AddRefreshTokenAsync(
            new RefreshToken
            {
                UserId = user.Id,
                TokenHash = tokens.HashToken(rawRefreshToken),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            },
            cancellationToken);

        await users.SaveChangesAsync(cancellationToken);
        return new LoginResponseDto(
            accessToken,
            rawRefreshToken,
            expiresAtUtc,
            user.Role.ToString(),
            user.FullName);
    }

    private static void RevokeAllRefreshTokens(User user)
    {
        foreach (var token in user.RefreshTokens.Where(token => token.RevokedAtUtc is null))
        {
            token.RevokedAtUtc = DateTime.UtcNow;
        }
    }
}
