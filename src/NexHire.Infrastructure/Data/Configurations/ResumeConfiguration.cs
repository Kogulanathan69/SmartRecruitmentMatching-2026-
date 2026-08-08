using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.ToTable("Resumes");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ResumeName).IsRequired().HasMaxLength(120);
        builder.Property(r => r.CareerObjective).HasMaxLength(1200);
        builder.Property(r => r.Languages).HasMaxLength(500);
        builder.Property(r => r.LinkedInUrl).HasMaxLength(500);
        builder.Property(r => r.GitHubUrl).HasMaxLength(500);
        builder.Property(r => r.PortfolioUrl).HasMaxLength(500);
        builder.Property(r => r.QualityRating).HasMaxLength(50);
        builder.Property(r => r.MissingSections).HasMaxLength(1000);
        builder.Property(r => r.FileName).HasMaxLength(255);
        builder.Property(r => r.FileUrl).HasMaxLength(1000);

        builder.HasIndex(r => new { r.JobSeekerProfileId, r.ResumeName }).IsUnique();

        builder.HasOne(r => r.ResumeTemplate)
            .WithMany(t => t.Resumes)
            .HasForeignKey(r => r.ResumeTemplateId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
