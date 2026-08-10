using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;
namespace NexHire.Infrastructure.Data.Configurations;
public class MatchResultConfiguration:IEntityTypeConfiguration<MatchResult>
{
 public void Configure(EntityTypeBuilder<MatchResult>b){b.ToTable("MatchResults");b.HasKey(x=>x.Id);b.Property(x=>x.TotalScore).HasPrecision(5,2);b.Property(x=>x.Recommendation).HasMaxLength(100);b.Property(x=>x.Summary).HasMaxLength(2000);b.HasIndex(x=>new{x.JobSeekerProfileId,x.JobId,x.CalculatedAtUtc});}
}
