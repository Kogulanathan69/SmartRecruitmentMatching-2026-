using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class MatchResultConfiguration
    : IEntityTypeConfiguration<MatchResult>
{
    public void Configure(EntityTypeBuilder<MatchResult> builder)
    {
        builder.ToTable("MatchResults");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.TotalScore)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(m => m.Recommendation)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Summary)
            .HasMaxLength(2000);

        builder.HasIndex(m => new
        {
            m.JobId,
            m.JobSeekerProfileId,
            m.CalculatedAtUtc
        });

        builder.HasOne<Job>()
            .WithMany()
            .HasForeignKey(m => m.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<JobSeekerProfile>()
            .WithMany()
            .HasForeignKey(m => m.JobSeekerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.ScoreDetails)
            .WithOne()
            .HasForeignKey(d => d.MatchResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}