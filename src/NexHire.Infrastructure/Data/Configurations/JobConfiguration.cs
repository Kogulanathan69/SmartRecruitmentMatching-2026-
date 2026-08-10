using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(x => x.Responsibilities)
            .HasMaxLength(5000);

        builder.Property(x => x.EducationRequirement)
            .HasMaxLength(500);

        builder.Property(x => x.EmploymentType)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.LocationCity)
            .HasMaxLength(100);

        builder.Property(x => x.LocationCountry)
            .HasMaxLength(100);

        builder.Property(x => x.Currency)
            .HasMaxLength(10);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.SalaryMin)
            .HasPrecision(18, 2);

        builder.Property(x => x.SalaryMax)
            .HasPrecision(18, 2);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.CompanyId);
    }
}