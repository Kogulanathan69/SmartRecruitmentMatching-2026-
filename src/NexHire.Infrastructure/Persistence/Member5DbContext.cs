using Microsoft.EntityFrameworkCore;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data;

public sealed class Member5DbContext : DbContext
{
    public Member5DbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<InterviewScore> InterviewScores => Set<InterviewScore>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Interview>(entity =>
        {
            entity.ToTable("Interviews");
            entity.HasKey(item => item.InterviewId);

            entity.Property(item => item.Mode)
                .HasConversion<int>();

            entity.Property(item => item.Status)
                .HasConversion<int>();

            entity.Property(item => item.MeetingLink)
                .HasMaxLength(1000);

            entity.Property(item => item.Location)
                .HasMaxLength(500);

            entity.Property(item => item.ContactPhone)
                .HasMaxLength(50);

            entity.Property(item => item.Notes)
                .HasMaxLength(4000);

            entity.Property(item => item.CancellationReason)
                .HasMaxLength(1000);

            entity.Property(item => item.RowVersion)
                .IsRowVersion();

            entity.HasIndex(item => item.ApplicationId);

            entity.HasIndex(item => new
            {
                item.CompanyId,
                item.ScheduledAtUtc
            });

            entity.HasIndex(item => new
            {
                item.CandidateProfileId,
                item.ScheduledAtUtc
            });
        });

        modelBuilder.Entity<InterviewScore>(entity =>
        {
            entity.ToTable("InterviewScores");
            entity.HasKey(item => item.InterviewScoreId);

            entity.Property(item => item.Feedback)
                .HasMaxLength(4000)
                .IsRequired();

            entity.HasIndex(item => new
            {
                item.InterviewId,
                item.EvaluatorUserId
            })
            .IsUnique();

            entity.HasOne<Interview>()
                .WithMany()
                .HasForeignKey(item => item.InterviewId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}