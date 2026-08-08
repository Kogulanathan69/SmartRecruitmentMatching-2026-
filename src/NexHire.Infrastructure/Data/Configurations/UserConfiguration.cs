using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.Id);
        builder.Ignore(user => user.FullName);

        builder.Property(user => user.FirstName).HasMaxLength(60).IsRequired();
        builder.Property(user => user.LastName).HasMaxLength(60).IsRequired();
        builder.Property(user => user.PhoneNumber).HasMaxLength(30);
        builder.Property(user => user.Email).HasMaxLength(256).IsRequired();
        builder.Property(user => user.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(100).IsRequired();
        builder.Property(user => user.PasswordResetTokenHash).HasMaxLength(64);

        builder.Property(user => user.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(user => user.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique();

        builder.HasMany(user => user.RefreshTokens)
            .WithOne(token => token.User)
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
