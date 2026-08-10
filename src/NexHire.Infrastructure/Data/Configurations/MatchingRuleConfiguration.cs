using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class MatchingRuleConfiguration
    : IEntityTypeConfiguration<MatchingRule>
{
    public void Configure(EntityTypeBuilder<MatchingRule> builder)
    {
        builder.ToTable(
            "MatchingRules",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_MatchingRules_SkillsWeight",
                    "[SkillsWeight] BETWEEN 0 AND 100");

                table.HasCheckConstraint(
                    "CK_MatchingRules_ExperienceWeight",
                    "[ExperienceWeight] BETWEEN 0 AND 100");

                table.HasCheckConstraint(
                    "CK_MatchingRules_EducationWeight",
                    "[EducationWeight] BETWEEN 0 AND 100");

                table.HasCheckConstraint(
                    "CK_MatchingRules_CertificationWeight",
                    "[CertificationWeight] BETWEEN 0 AND 100");

                table.HasCheckConstraint(
                    "CK_MatchingRules_LocationWeight",
                    "[LocationWeight] BETWEEN 0 AND 100");

                table.HasCheckConstraint(
                    "CK_MatchingRules_ProjectsWeight",
                    "[ProjectsWeight] BETWEEN 0 AND 100");

                table.HasCheckConstraint(
                    "CK_MatchingRules_ProfileCompletionWeight",
                    "[ProfileCompletionWeight] BETWEEN 0 AND 100");

                table.HasCheckConstraint(
                    "CK_MatchingRules_TotalWeight",
                    "[SkillsWeight] + [ExperienceWeight] + [EducationWeight] + " +
                    "[CertificationWeight] + [LocationWeight] + [ProjectsWeight] + " +
                    "[ProfileCompletionWeight] = 100");
            });

        builder.HasKey(r => r.Id);

        builder.Property(r => r.SkillsWeight).HasPrecision(5, 2);
        builder.Property(r => r.ExperienceWeight).HasPrecision(5, 2);
        builder.Property(r => r.EducationWeight).HasPrecision(5, 2);
        builder.Property(r => r.CertificationWeight).HasPrecision(5, 2);
        builder.Property(r => r.LocationWeight).HasPrecision(5, 2);
        builder.Property(r => r.ProjectsWeight).HasPrecision(5, 2);
        builder.Property(r => r.ProfileCompletionWeight).HasPrecision(5, 2);

        // Only one matching rule can be active at a time.
        builder.HasIndex(r => r.IsActive)
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        builder.Ignore(r => r.TotalWeight);
    }
}