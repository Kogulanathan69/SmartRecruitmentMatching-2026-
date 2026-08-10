using NexHire.Domain.Enums;

namespace NexHire.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Existing accounts and Admin-created accounts remain usable.
    // Public registration explicitly sets this to false until OTP verification.
    public bool IsEmailVerified { get; set; } = true;
    public string? EmailVerificationOtpHash { get; set; }
    public DateTime? EmailVerificationOtpExpiresAtUtc { get; set; }
    public DateTime? EmailVerificationOtpLastSentAtUtc { get; set; }
    public int EmailVerificationOtpFailedAttempts { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public JobSeekerProfile? JobSeekerProfile { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
}