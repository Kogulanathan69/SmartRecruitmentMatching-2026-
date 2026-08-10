using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;
namespace NexHire.Infrastructure.Data.Configurations;
public class UserConfiguration:IEntityTypeConfiguration<User>
{
 public void Configure(EntityTypeBuilder<User>b){b.ToTable("Users");b.HasKey(x=>x.Id);b.Ignore(x=>x.FullName);b.Property(x=>x.FirstName).HasMaxLength(60).IsRequired();b.Property(x=>x.LastName).HasMaxLength(60).IsRequired();b.Property(x=>x.PhoneNumber).HasMaxLength(30);b.Property(x=>x.Email).HasMaxLength(256).IsRequired();b.Property(x=>x.NormalizedEmail).HasMaxLength(256).IsRequired();b.Property(x=>x.PasswordHash).HasMaxLength(500).IsRequired();b.Property(x=>x.Role).HasConversion<string>().HasMaxLength(30).IsRequired();b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();b.HasIndex(x=>x.NormalizedEmail).IsUnique();b.HasMany(x=>x.RefreshTokens).WithOne(x=>x.User).HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Cascade);}
}
