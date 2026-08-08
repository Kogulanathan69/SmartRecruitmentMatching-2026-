using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class ApplicationStatusHistoryConfiguration : IEntityTypeConfiguration<ApplicationStatusHistory>
{
    public void Configure(EntityTypeBuilder<ApplicationStatusHistory> builder)
    {
        builder.ToTable("ApplicationStatusHistory");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FromStatus)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.ToStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ChangedByRole)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.JobApplicationId, x.ChangedAtUtc, x.Id });

        builder.HasOne(x => x.JobApplication)
            .WithMany(x => x.StatusHistory)
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
