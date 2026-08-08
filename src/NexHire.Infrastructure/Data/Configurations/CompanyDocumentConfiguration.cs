using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class CompanyDocumentConfiguration : IEntityTypeConfiguration<CompanyDocument>
{
    public void Configure(EntityTypeBuilder<CompanyDocument> builder)
    {
        builder.ToTable("CompanyDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentType).IsRequired().HasMaxLength(80);
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(260);
        builder.Property(x => x.FileUrl).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.ContentType).HasMaxLength(100);
        builder.Property(x => x.ReviewNotes).HasMaxLength(1000);
        builder.HasIndex(x => new { x.CompanyId, x.DocumentType }).IsUnique();
    }
}
