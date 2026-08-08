using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class JobRequiredSkillConfiguration
    : IEntityTypeConfiguration<JobRequiredSkill>
{
    public void Configure(EntityTypeBuilder<JobRequiredSkill> builder)
    {
        builder.ToTable(
            "JobRequiredSkills",
            table => table.HasCheckConstraint(
                "CK_JobRequiredSkills_MinimumProficiencyLevel",
                "[MinimumProficiencyLevel] BETWEEN 1 AND 5"));

        builder.HasKey(rs => rs.Id);

        // Same mandatory skill cannot be added twice to one job.
        builder.HasIndex(rs => new { rs.JobId, rs.SkillId })
            .IsUnique();

        builder.Property(rs => rs.MinimumProficiencyLevel)
            .IsRequired();

        builder.HasOne(rs => rs.Skill)
            .WithMany()
            .HasForeignKey(rs => rs.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}