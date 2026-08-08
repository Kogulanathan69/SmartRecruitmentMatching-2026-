using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(j => j.Description)
            .IsRequired()
            .HasMaxLength(8000);

        builder.Property(j => j.Responsibilities)
            .HasMaxLength(4000);

        builder.Property(j => j.EducationRequirement)
            .HasMaxLength(500);

        builder.Property(j => j.EmploymentType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(j => j.LocationCity)
            .HasMaxLength(100);

        builder.Property(j => j.LocationCountry)
            .HasMaxLength(100);

        builder.Property(j => j.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(j => j.SalaryMin)
            .HasPrecision(18, 2);

        builder.Property(j => j.SalaryMax)
            .HasPrecision(18, 2);

        builder.Property(j => j.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(j => new { j.CompanyId, j.Status });

        builder.HasIndex(j => j.Title);

        builder.HasIndex(j => j.ClosingDate);

        builder.HasMany(j => j.RequiredSkills)
            .WithOne(rs => rs.Job)
            .HasForeignKey(rs => rs.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(j => j.PreferredSkills)
            .WithOne(ps => ps.Job)
            .HasForeignKey(ps => ps.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}