using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> builder)
    {
        builder.ToTable("Experiences");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.JobTitle).IsRequired().HasMaxLength(150);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.HasIndex(e => e.JobSeekerProfileId);
    }
}
