using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class JobApplicationConfiguration
    : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(
        EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("JobApplications");

        builder.HasKey(x => x.JobApplicationId);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(x => x.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.MatchRuleVersion)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.MatchScoreSnapshot)
            .HasPrecision(5, 2);

        builder.HasIndex(x => new
        {
            x.CandidateId,
            x.VacancyId
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.CandidateId,
            x.IdempotencyKey
        })
        .IsUnique();

        builder.HasOne(x => x.Candidate)
            .WithMany()
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Vacancy)
            .WithMany()
            .HasForeignKey(x => x.VacancyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}