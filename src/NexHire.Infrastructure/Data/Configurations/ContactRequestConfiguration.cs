using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class ContactRequestConfiguration : IEntityTypeConfiguration<ContactRequest>
{
    public void Configure(EntityTypeBuilder<ContactRequest> builder)
    {
        builder.ToTable("ContactRequests");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.JobApplicationId).IsUnique();
        builder.HasIndex(x => new { x.Status, x.ExpiresAtUtc });
        builder.HasIndex(x => x.EmployerUserId);
        builder.HasIndex(x => x.CandidateUserId);

        builder.HasOne(x => x.JobApplication)
            .WithOne(x => x.ContactRequest)
            .HasForeignKey<ContactRequest>(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
