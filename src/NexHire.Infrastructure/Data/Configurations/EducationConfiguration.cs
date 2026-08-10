using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class EducationConfiguration : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> builder)
    {
        builder.ToTable("Educations");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Institution).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Degree).IsRequired().HasMaxLength(150);
        builder.Property(e => e.FieldOfStudy).HasMaxLength(150);
        builder.Property(e => e.EducationLevel).IsRequired();
        builder.Property(e => e.GradeOrGpa).HasMaxLength(50);
        builder.HasIndex(e => e.JobSeekerProfileId);
    }
}
