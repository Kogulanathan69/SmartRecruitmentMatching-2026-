using System.Security.Cryptography;
using NexHire.Application.DTOs.Auth;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Validators;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;

namespace NexHire.Application.Services;

public sealed class AuthService : IAuthService
{
    private const int OtpExpiryMinutes = 10;
    private const int OtpResendCooldownSeconds = 60;
    private const int MaxOtpAttempts = 5;

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwords;
    private readonly ITokenService _tokens;
    private readonly IEmailSender _emailSender;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwords,
        ITokenService tokens,
        IEmailSender emailSender)
    {
        _users = users;
        _passwords = passwords;
        _tokens = tokens;
        _emailSender = emailSender;
    }

    // ---------------------------------------------------------
    // REGISTER
    // ---------------------------------------------------------

    public async Task RegisterAsync(
        RegisterRequestDto dto,
        CancellationToken ct = default)
    {
        var validation =
            await new RegisterRequestValidator()
                .ValidateAsync(dto, ct);

        if (!validation.IsValid)
        {
            throw new ArgumentException(
                string.Join(
                    " ",
                    validation.Errors.Select(x => x.ErrorMessage)));
        }

        var normalizedEmail =
            dto.Email.Trim().ToUpperInvariant();

        if (await _users.EmailExistsAsync(normalizedEmail, ct))
        {
            throw new InvalidOperationException(
                "Email is already registered.");
        }

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),

            LastName = dto.LastName.Trim(),

            PhoneNumber = dto.PhoneNumber?.Trim(),

            Email = dto.Email.Trim().ToLowerInvariant(),

            NormalizedEmail = normalizedEmail,

            Role = Enum.Parse<UserRole>(
                dto.Role,
                true),

            Status = UserStatus.Active,

            IsEmailVerified = false,

            CreatedAtUtc = DateTime.UtcNow
        };

        user.PasswordHash =
            _passwords.Hash(dto.Password);

        var otp = GenerateOtp();

        user.EmailVerificationOtpHash =
            _tokens.HashToken(otp);

        user.EmailVerificationOtpExpiresAtUtc =
            DateTime.UtcNow.AddMinutes(
                OtpExpiryMinutes);

        user.EmailVerificationOtpLastSentAtUtc =
            DateTime.UtcNow;

        user.EmailVerificationOtpFailedAttempts = 0;

        await _users.AddUserAsync(user, ct);

        await _users.SaveChangesAsync(ct);

        await SendVerificationOtpAsync(
            user.Email,
            user.FullName,
            otp,
            ct);
    }

    // ---------------------------------------------------------
    // VERIFY EMAIL OTP
    // ---------------------------------------------------------

    public async Task<bool> VerifyEmailOtpAsync(
        VerifyEmailOtpRequestDto dto,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Otp))
        {
            return false;
        }

        var normalizedEmail =
            dto.Email.Trim().ToUpperInvariant();

        var user =
            await _users.GetByEmailAsync(
                normalizedEmail,
                ct);

        if (user is null)
        {
            return false;
        }

        // Already verified
        if (user.IsEmailVerified)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(
                user.EmailVerificationOtpHash))
        {
            return false;
        }

        if (!user.EmailVerificationOtpExpiresAtUtc.HasValue ||
            user.EmailVerificationOtpExpiresAtUtc.Value <=
            DateTime.UtcNow)
        {
            return false;
        }

        if (user.EmailVerificationOtpFailedAttempts >=
            MaxOtpAttempts)
        {
            return false;
        }

        var otp = dto.Otp.Trim();

        var otpHash =
            _tokens.HashToken(otp);

        if (!string.Equals(
                otpHash,
                user.EmailVerificationOtpHash,
                StringComparison.OrdinalIgnoreCase))
        {
            user.EmailVerificationOtpFailedAttempts++;

            await _users.SaveChangesAsync(ct);

            return false;
        }

        user.IsEmailVerified = true;

        user.EmailVerificationOtpHash = null;

        user.EmailVerificationOtpExpiresAtUtc = null;

        user.EmailVerificationOtpLastSentAtUtc = null;

        user.EmailVerificationOtpFailedAttempts = 0;

        await _users.SaveChangesAsync(ct);

        return true;
    }

    // ---------------------------------------------------------
    // RESEND OTP
    // ---------------------------------------------------------

    public async Task<bool> ResendEmailOtpAsync(
        ResendEmailOtpRequestDto dto,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return false;
        }

        var normalizedEmail =
            dto.Email.Trim().ToUpperInvariant();

        var user =
            await _users.GetByEmailAsync(
                normalizedEmail,
                ct);

        if (user is null)
        {
            // Don't reveal whether an account exists.
            return true;
        }

        if (user.IsEmailVerified)
        {
            return true;
        }

        if (user.EmailVerificationOtpLastSentAtUtc.HasValue)
        {
            var nextAllowedTime =
                user.EmailVerificationOtpLastSentAtUtc.Value
                    .AddSeconds(
                        OtpResendCooldownSeconds);

            if (nextAllowedTime > DateTime.UtcNow)
            {
                return false;
            }
        }

        var otp = GenerateOtp();

        user.EmailVerificationOtpHash =
            _tokens.HashToken(otp);

        user.EmailVerificationOtpExpiresAtUtc =
            DateTime.UtcNow.AddMinutes(
                OtpExpiryMinutes);

        user.EmailVerificationOtpLastSentAtUtc =
            DateTime.UtcNow;

        user.EmailVerificationOtpFailedAttempts = 0;

        await _users.SaveChangesAsync(ct);

        await SendVerificationOtpAsync(
            user.Email,
            user.FullName,
            otp,
            ct);

        return true;
    }

    // ---------------------------------------------------------
    // LOGIN
    // ---------------------------------------------------------

    public async Task<LoginResponseDto?> LoginAsync(
        LoginRequestDto dto,
        CancellationToken ct = default)
    {
        var user =
            await _users.GetByEmailAsync(
                dto.Email.Trim().ToUpperInvariant(),
                ct);

        if (user is null)
        {
            return null;
        }

        if (user.Status != UserStatus.Active)
        {
            return null;
        }

        if (!_passwords.Verify(
                dto.Password,
                user.PasswordHash))
        {
            return null;
        }

        // OTP verification required
        if (!user.IsEmailVerified)
        {
            throw new InvalidOperationException(
                "Email is not verified. Please verify the OTP first.");
        }

        return await IssueTokensAsync(
            user,
            ct);
    }

    // ---------------------------------------------------------
    // REFRESH TOKEN
    // ---------------------------------------------------------

    public async Task<LoginResponseDto?> RefreshAsync(
        string raw,
        CancellationToken ct = default)
    {
        var old =
            await _users.GetRefreshTokenAsync(
                _tokens.HashToken(raw),
                ct);

        if (old is null ||
            !old.IsValid ||
            old.User.Status != UserStatus.Active ||
            !old.User.IsEmailVerified)
        {
            return null;
        }

        old.RevokedAtUtc =
            DateTime.UtcNow;

        return await IssueTokensAsync(
            old.User,
            ct);
    }

    // ---------------------------------------------------------
    // FORGOT PASSWORD
    // ---------------------------------------------------------

    public async Task<string?> ForgotPasswordAsync(
        string email,
        CancellationToken ct = default)
    {
        var user =
            await _users.GetByEmailAsync(
                email.Trim().ToUpperInvariant(),
                ct);

        if (user is null)
        {
            return null;
        }

        var raw =
            _tokens.CreateSecureToken();

        user.PasswordResetTokenHash =
            _tokens.HashToken(raw);

        user.PasswordResetExpiresAtUtc =
            DateTime.UtcNow.AddMinutes(30);

        await _users.SaveChangesAsync(ct);

        return raw;
    }

    // ---------------------------------------------------------
    // RESET PASSWORD
    // ---------------------------------------------------------

    public async Task<bool> ResetPasswordAsync(
        ResetPasswordRequestDto dto,
        CancellationToken ct = default)
    {
        if (!PasswordPolicy.IsStrong(dto.NewPassword) ||
            dto.NewPassword != dto.ConfirmNewPassword)
        {
            return false;
        }

        var user =
            await _users.GetByResetTokenHashAsync(
                _tokens.HashToken(dto.Token),
                ct);

        if (user is null ||
            user.PasswordResetExpiresAtUtc <=
            DateTime.UtcNow)
        {
            return false;
        }

        user.PasswordHash =
            _passwords.Hash(dto.NewPassword);

        user.PasswordResetTokenHash = null;

        user.PasswordResetExpiresAtUtc = null;

        RevokeAll(user);

        await _users.SaveChangesAsync(ct);

        return true;
    }

    // ---------------------------------------------------------
    // CHANGE PASSWORD
    // ---------------------------------------------------------

    public async Task<bool> ChangePasswordAsync(
        Guid id,
        ChangePasswordRequestDto dto,
        CancellationToken ct = default)
    {
        if (!PasswordPolicy.IsStrong(dto.NewPassword) ||
            dto.NewPassword != dto.ConfirmNewPassword ||
            dto.CurrentPassword == dto.NewPassword)
        {
            return false;
        }

        var user =
            await _users.GetByIdAsync(
                id,
                ct);

        if (user is null ||
            !_passwords.Verify(
                dto.CurrentPassword,
                user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash =
            _passwords.Hash(dto.NewPassword);

        RevokeAll(user);

        await _users.SaveChangesAsync(ct);

        return true;
    }

    // ---------------------------------------------------------
    // LOGOUT
    // ---------------------------------------------------------

    public async Task<bool> LogoutAsync(
        string raw,
        CancellationToken ct = default)
    {
        var token =
            await _users.GetRefreshTokenAsync(
                _tokens.HashToken(raw),
                ct);

        if (token is null ||
            token.RevokedAtUtc is not null)
        {
            return false;
        }

        token.RevokedAtUtc =
            DateTime.UtcNow;

        await _users.SaveChangesAsync(ct);

        return true;
    }

    // ---------------------------------------------------------
    // CURRENT USER
    // ---------------------------------------------------------

    public async Task<CurrentUserResponseDto?>
        GetCurrentUserAsync(
            Guid id,
            CancellationToken ct = default)
    {
        var user =
            await _users.GetByIdAsync(
                id,
                ct);

        if (user is null)
        {
            return null;
        }

        return new CurrentUserResponseDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            user.Status.ToString());
    }

    // ---------------------------------------------------------
    // ISSUE JWT + REFRESH TOKEN
    // ---------------------------------------------------------

    private async Task<LoginResponseDto> IssueTokensAsync(
        User user,
        CancellationToken ct)
    {
        var (access, expires) =
            _tokens.CreateAccessToken(user);

        var rawRefreshToken =
            _tokens.CreateSecureToken();

        var refreshToken =
            new RefreshToken
            {
                UserId = user.Id,

                TokenHash =
                    _tokens.HashToken(
                        rawRefreshToken),

                ExpiresAtUtc =
                    DateTime.UtcNow.AddDays(7)
            };

        await _users.AddRefreshTokenAsync(
            refreshToken,
            ct);

        await _users.SaveChangesAsync(ct);

        return new LoginResponseDto(
            access,
            rawRefreshToken,
            expires,
            user.Role.ToString(),
            user.FullName);
    }

    // ---------------------------------------------------------
    // GENERATE 6 DIGIT OTP
    // ---------------------------------------------------------

    private static string GenerateOtp()
    {
        return RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();
    }

    // ---------------------------------------------------------
    // SEND OTP EMAIL
    // ---------------------------------------------------------

    private async Task SendVerificationOtpAsync(
        string email,
        string fullName,
        string otp,
        CancellationToken ct)
    {
        var subject =
            "NexHire - Verify your email";

        var htmlBody = $"""
            <div style="font-family:Arial,sans-serif;
                        max-width:600px;
                        margin:auto;
                        padding:24px;">

                <h2>NexHire Email Verification</h2>

                <p>Hello {fullName},</p>

                <p>
                    Use the following verification code
                    to verify your NexHire account:
                </p>

                <div style="
                    font-size:32px;
                    font-weight:bold;
                    letter-spacing:8px;
                    margin:24px 0;">
                    {otp}
                </div>

                <p>
                    This OTP will expire in
                    {OtpExpiryMinutes} minutes.
                </p>

                <p>
                    If you did not create this account,
                    you can ignore this email.
                </p>

                <p>
                    NexHire Team
                </p>
            </div>
            """;

        await _emailSender.SendEmailAsync(
            email,
            subject,
            htmlBody,
            ct);
    }

    // ---------------------------------------------------------
    // REVOKE TOKENS
    // ---------------------------------------------------------

    private static void RevokeAll(
        User user)
    {
        foreach (var token in
                 user.RefreshTokens.Where(
                     x => x.RevokedAtUtc is null))
        {
            token.RevokedAtUtc =
                DateTime.UtcNow;
        }
    }
}