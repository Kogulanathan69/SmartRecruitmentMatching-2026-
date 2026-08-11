using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class JobPreferredSkillConfiguration
    : IEntityTypeConfiguration<JobPreferredSkill>
{
    public void Configure(EntityTypeBuilder<JobPreferredSkill> builder)
    {
        builder.ToTable(
            "JobPreferredSkills",
            table => table.HasCheckConstraint(
                "CK_JobPreferredSkills_MinimumProficiencyLevel",
                "[MinimumProficiencyLevel] BETWEEN 1 AND 5"));

        builder.HasKey(ps => ps.Id);

        // Same preferred skill cannot be added twice to one job.
        builder.HasIndex(ps => new { ps.JobId, ps.SkillId })
            .IsUnique();

        builder.Property(ps => ps.MinimumProficiencyLevel)
            .IsRequired();

        builder.HasOne(ps => ps.Skill)
            .WithMany()
            .HasForeignKey(ps => ps.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}