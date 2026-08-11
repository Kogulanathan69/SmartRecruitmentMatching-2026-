using System.Net;
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

    public AuthService(IUserRepository users, IPasswordHasher passwords, ITokenService tokens, IEmailSender emailSender)
    {
        _users = users;
        _passwords = passwords;
        _tokens = tokens;
        _emailSender = emailSender;
    }

    public async Task RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default)
    {
        var validation = await new RegisterRequestValidator().ValidateAsync(dto, ct);
        if (!validation.IsValid)
            throw new ArgumentException(string.Join(" ", validation.Errors.Select(x => x.ErrorMessage)));

        var normalizedEmail = dto.Email.Trim().ToUpperInvariant();
        if (await _users.EmailExistsAsync(normalizedEmail, ct))
            throw new InvalidOperationException("Email is already registered.");

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            NormalizedEmail = normalizedEmail,
            Role = Enum.Parse<UserRole>(dto.Role, true),
            Status = UserStatus.Active,
            IsEmailVerified = false,
            CreatedAtUtc = DateTime.UtcNow
        };
        user.PasswordHash = _passwords.Hash(dto.Password);

        var otp = GenerateOtp();
        user.EmailVerificationOtpHash = _tokens.HashToken(otp);
        user.EmailVerificationOtpExpiresAtUtc = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
        user.EmailVerificationOtpLastSentAtUtc = DateTime.UtcNow;
        user.EmailVerificationOtpFailedAttempts = 0;

        // Send before persisting. If SMTP fails the address is not stranded as an
        // unverified duplicate and the user can simply retry registration.
        await SendVerificationOtpAsync(user.Email, user.FullName, otp, ct);
        await _users.AddUserAsync(user, ct);
        await _users.SaveChangesAsync(ct);
    }

    public async Task<bool> VerifyEmailOtpAsync(VerifyEmailOtpRequestDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Otp)) return false;
        var user = await _users.GetByEmailAsync(dto.Email.Trim().ToUpperInvariant(), ct);
        if (user is null) return false;
        if (user.IsEmailVerified) return true;
        if (string.IsNullOrWhiteSpace(user.EmailVerificationOtpHash)) return false;
        if (!user.EmailVerificationOtpExpiresAtUtc.HasValue || user.EmailVerificationOtpExpiresAtUtc.Value <= DateTime.UtcNow) return false;
        if (user.EmailVerificationOtpFailedAttempts >= MaxOtpAttempts) return false;

        var suppliedHash = _tokens.HashToken(dto.Otp.Trim());
        if (!string.Equals(suppliedHash, user.EmailVerificationOtpHash, StringComparison.OrdinalIgnoreCase))
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

    public async Task<bool> ResendEmailOtpAsync(ResendEmailOtpRequestDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email)) return false;
        var user = await _users.GetByEmailAsync(dto.Email.Trim().ToUpperInvariant(), ct);

        // Do not reveal whether an account exists.
        if (user is null || user.IsEmailVerified) return true;

        if (user.EmailVerificationOtpLastSentAtUtc.HasValue &&
            user.EmailVerificationOtpLastSentAtUtc.Value.AddSeconds(OtpResendCooldownSeconds) > DateTime.UtcNow)
            return false;

        var otp = GenerateOtp();
        var newHash = _tokens.HashToken(otp);
        var newExpiry = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
        var newSentAt = DateTime.UtcNow;

        // Persist the new OTP only after email delivery succeeds. An SMTP failure
        // therefore leaves the previous valid OTP untouched in the database.
        await SendVerificationOtpAsync(user.Email, user.FullName, otp, ct);
        user.EmailVerificationOtpHash = newHash;
        user.EmailVerificationOtpExpiresAtUtc = newExpiry;
        user.EmailVerificationOtpLastSentAtUtc = newSentAt;
        user.EmailVerificationOtpFailedAttempts = 0;
        await _users.SaveChangesAsync(ct);
        return true;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(dto.Email.Trim().ToUpperInvariant(), ct);
        if (user is null || user.Status != UserStatus.Active || !_passwords.Verify(dto.Password, user.PasswordHash)) return null;
        if (!user.IsEmailVerified)
            throw new InvalidOperationException("Email is not verified. Enter the OTP sent to your email first.");
        return await IssueTokensAsync(user, ct);
    }

    public async Task<LoginResponseDto?> RefreshAsync(string raw, CancellationToken ct = default)
    {
        var old = await _users.GetRefreshTokenAsync(_tokens.HashToken(raw), ct);
        if (old is null || !old.IsValid || old.User.Status != UserStatus.Active || !old.User.IsEmailVerified) return null;
        old.RevokedAtUtc = DateTime.UtcNow;
        return await IssueTokensAsync(old.User, ct);
    }

    public async Task<string?> ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(email.Trim().ToUpperInvariant(), ct);
        if (user is null) return null;
        var raw = _tokens.CreateSecureToken();
        user.PasswordResetTokenHash = _tokens.HashToken(raw);
        user.PasswordResetExpiresAtUtc = DateTime.UtcNow.AddMinutes(30);
        await _users.SaveChangesAsync(ct);
        return raw;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto dto, CancellationToken ct = default)
    {
        if (!PasswordPolicy.IsStrong(dto.NewPassword) || dto.NewPassword != dto.ConfirmNewPassword) return false;
        var user = await _users.GetByResetTokenHashAsync(_tokens.HashToken(dto.Token), ct);
        if (user is null || user.PasswordResetExpiresAtUtc <= DateTime.UtcNow) return false;
        user.PasswordHash = _passwords.Hash(dto.NewPassword);
        user.PasswordResetTokenHash = null;
        user.PasswordResetExpiresAtUtc = null;
        RevokeAll(user);
        await _users.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid id, ChangePasswordRequestDto dto, CancellationToken ct = default)
    {
        if (!PasswordPolicy.IsStrong(dto.NewPassword) || dto.NewPassword != dto.ConfirmNewPassword || dto.CurrentPassword == dto.NewPassword) return false;
        var user = await _users.GetByIdAsync(id, ct);
        if (user is null || !_passwords.Verify(dto.CurrentPassword, user.PasswordHash)) return false;
        user.PasswordHash = _passwords.Hash(dto.NewPassword);
        RevokeAll(user);
        await _users.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> LogoutAsync(string raw, CancellationToken ct = default)
    {
        var token = await _users.GetRefreshTokenAsync(_tokens.HashToken(raw), ct);
        if (token is null || token.RevokedAtUtc is not null) return false;
        token.RevokedAtUtc = DateTime.UtcNow;
        await _users.SaveChangesAsync(ct);
        return true;
    }

    public async Task<CurrentUserResponseDto?> GetCurrentUserAsync(Guid id, CancellationToken ct = default)
    {
        var u = await _users.GetByIdAsync(id, ct);
        return u is null ? null : new(u.Id, u.Email, u.FullName, u.Role.ToString(), u.Status.ToString());
    }

    private async Task<LoginResponseDto> IssueTokensAsync(User user, CancellationToken ct)
    {
        var (access, expires) = _tokens.CreateAccessToken(user);
        var raw = _tokens.CreateSecureToken();
        await _users.AddRefreshTokenAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokens.HashToken(raw),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        }, ct);
        await _users.SaveChangesAsync(ct);
        return new(access, raw, expires, user.Role.ToString(), user.FullName);
    }

    private static string GenerateOtp() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private async Task SendVerificationOtpAsync(string email, string fullName, string otp, CancellationToken ct)
    {
        var safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(fullName) ? "NexHire user" : fullName);
        var body = $"<div style='font-family:Arial,sans-serif;max-width:520px;margin:auto;padding:24px'>" +
                   $"<h2 style='margin:0 0 12px'>Verify your NexHire email</h2>" +
                   $"<p>Hello {safeName},</p>" +
                   $"<p>Use this 6-digit code to finish creating your NexHire account:</p>" +
                   $"<div style='font-size:32px;font-weight:700;letter-spacing:8px;margin:20px 0'>{otp}</div>" +
                   $"<p>This code expires in {OtpExpiryMinutes} minutes. Do not share it with anyone.</p>" +
                   $"<p style='color:#666'>If you did not request this account, you can ignore this email.</p></div>";
        await _emailSender.SendEmailAsync(email, "Your NexHire verification code", body, ct);
    }

    private static void RevokeAll(User user)
    {
        foreach (var t in user.RefreshTokens.Where(x => x.RevokedAtUtc is null)) t.RevokedAtUtc = DateTime.UtcNow;
    }
}