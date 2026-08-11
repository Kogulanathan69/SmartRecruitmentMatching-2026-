using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.FirstName)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(user => user.LastName)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(user => user.PhoneNumber)
            .HasMaxLength(30);

        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(user => user.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(user => user.PasswordHash)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(user => user.Role)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(user => user.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(user => user.PasswordResetTokenHash)
            .HasMaxLength(64);

        // Database default true deliberately preserves all pre-OTP accounts
        // and Admin-created accounts. Public registration explicitly writes false.
        builder.Property(user => user.IsEmailVerified)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(user => user.EmailVerificationOtpHash)
            .HasMaxLength(64);

        builder.Property(user => user.EmailVerificationOtpExpiresAtUtc);
        builder.Property(user => user.EmailVerificationOtpLastSentAtUtc);

        builder.Property(user => user.EmailVerificationOtpFailedAttempts)
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique();
    }
}