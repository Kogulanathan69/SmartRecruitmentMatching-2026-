using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data.Configurations;

public class MatchScoreDetailConfiguration
    : IEntityTypeConfiguration<MatchScoreDetail>
{
    public void Configure(
        EntityTypeBuilder<MatchScoreDetail> builder)
    {
        builder.ToTable("MatchScoreDetails");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.RawScore)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(d => d.Weight)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(d => d.WeightedPoints)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(d => d.MaximumWeightedPoints)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(d => d.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.Explanation)
            .HasMaxLength(2000);

        // One match result must contain only one
        // score detail for each matching category.
        builder.HasIndex(d => new
        {
            d.MatchResultId,
            d.Category
        })
            .IsUnique();
    }
}