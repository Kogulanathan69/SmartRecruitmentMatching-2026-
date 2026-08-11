using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class JobApplicationConfiguration
    : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("JobApplications");

        builder.HasKey(a => a.JobApplicationId);

        builder.Property(a => a.Status)
            .HasMaxLength(50)
            .IsRequired();

        // CandidateId stores the Job Seeker's UserId.
        // Link it to JobSeekerProfile.UserId instead of creating
        // an unwanted shadow JobSeekerProfileId property.
        builder.HasOne<JobSeekerProfile>()
            .WithMany(p => p.Applications)
            .HasForeignKey(a => a.CandidateId)
            .HasPrincipalKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // VacancyId represents the Job.Id.
        builder.HasOne<Job>()
            .WithMany()
            .HasForeignKey(a => a.VacancyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.CandidateId);
        builder.HasIndex(a => a.VacancyId);

        // Database safety net against simultaneous duplicate applications.
        builder.HasIndex(a => new { a.CandidateId, a.VacancyId })
            .IsUnique();
    }
}