using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class JobSeekerConfiguration
    : IEntityTypeConfiguration<JobSeekerProfile>
{
    public void Configure(
        EntityTypeBuilder<JobSeekerProfile> builder)
    {
        builder.ToTable("JobSeekerProfiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Headline)
            .HasMaxLength(200);

        builder.Property(p => p.Summary)
            .HasMaxLength(2000);

        builder.Property(p => p.Gender)
            .HasMaxLength(50);

        builder.Property(p => p.Address)
            .HasMaxLength(300);

        builder.Property(p => p.City)
            .HasMaxLength(100);

        builder.Property(p => p.Country)
            .HasMaxLength(100);

        builder.Property(p => p.ExpectedSalaryMin)
            .HasPrecision(18, 2);

        builder.Property(p => p.ExpectedSalaryMax)
            .HasPrecision(18, 2);

        builder.HasIndex(p => p.UserId)
            .IsUnique();

        // User -> JobSeekerProfile
        builder.HasOne(p => p.User)
            .WithOne(u => u.JobSeekerProfile)
            .HasForeignKey<JobSeekerProfile>(
                p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // JobSeekerProfile -> Education
        builder.HasMany(p => p.Educations)
            .WithOne(e => e.JobSeekerProfile)
            .HasForeignKey(e => e.JobSeekerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // JobSeekerProfile -> Experience
        builder.HasMany(p => p.Experiences)
            .WithOne(e => e.JobSeekerProfile)
            .HasForeignKey(e => e.JobSeekerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // JobSeekerProfile -> CandidateSkills
        builder.HasMany(p => p.CandidateSkills)
            .WithOne(cs => cs.JobSeekerProfile)
            .HasForeignKey(cs => cs.JobSeekerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // JobSeekerProfile -> Projects
        builder.HasMany(p => p.Projects)
            .WithOne(p => p.JobSeekerProfile)
            .HasForeignKey(p => p.JobSeekerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // JobSeekerProfile -> Certifications
        builder.HasMany(p => p.Certifications)
            .WithOne(c => c.JobSeekerProfile)
            .HasForeignKey(c => c.JobSeekerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // JobSeekerProfile -> Resumes
        builder.HasMany(p => p.Resumes)
            .WithOne(r => r.JobSeekerProfile)
            .HasForeignKey(r => r.JobSeekerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // JobSeekerProfile -> TalentPoolEntries
        // Prevent SQL Server multiple cascade path.
        builder.HasMany<TalentPoolEntry>()
            .WithOne(t => t.JobSeekerProfile)
            .HasForeignKey(t => t.JobSeekerProfileId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}