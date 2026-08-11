using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class CompanyVerificationConfiguration : IEntityTypeConfiguration<CompanyVerification>
{
    public void Configure(EntityTypeBuilder<CompanyVerification> builder)
    {
        builder.ToTable("CompanyVerifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.DeclarationName).HasMaxLength(200);
        builder.Property(x => x.DeclarationDesignation).HasMaxLength(150);
        builder.Property(x => x.Remarks).HasMaxLength(2000);
        builder.HasIndex(x => x.CompanyId).IsUnique();
    }
}
