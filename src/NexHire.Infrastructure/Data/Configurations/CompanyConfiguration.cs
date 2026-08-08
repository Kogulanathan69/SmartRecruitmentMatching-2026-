using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.LegalName).HasMaxLength(250);
        builder.Property(c => c.RegistrationNumber).HasMaxLength(100);
        builder.Property(c => c.OfficialEmail).HasMaxLength(250);
        builder.Property(c => c.PhoneNumber).HasMaxLength(30);
        builder.Property(c => c.RegisteredAddress).HasMaxLength(500);
        builder.Property(c => c.Website).HasMaxLength(300);
        builder.Property(c => c.Industry).HasMaxLength(100);
        builder.Property(c => c.CompanySize).HasMaxLength(50);
        builder.Property(c => c.TrustLevel).HasMaxLength(50);
        builder.Property(c => c.TrustScore).IsRequired();

        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(40).IsRequired();

        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.RegistrationNumber).IsUnique().HasFilter("[RegistrationNumber] IS NOT NULL");
        builder.HasIndex(c => c.OfficialEmail).IsUnique().HasFilter("[OfficialEmail] IS NOT NULL");

        builder.HasMany(c => c.Documents).WithOne(d => d.Company).HasForeignKey(d => d.CompanyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Verification).WithOne(v => v.Company).HasForeignKey<CompanyVerification>(v => v.CompanyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Jobs).WithOne(j => j.Company).HasForeignKey(j => j.CompanyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.TalentPoolEntries).WithOne(t => t.Company).HasForeignKey(t => t.CompanyId).OnDelete(DeleteBehavior.Cascade);
    }
}
