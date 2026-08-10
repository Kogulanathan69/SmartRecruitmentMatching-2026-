using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;
namespace NexHire.Infrastructure.Data.Configurations;
public class PrivacyDeletionRequestConfiguration:IEntityTypeConfiguration<PrivacyDeletionRequest>{public void Configure(EntityTypeBuilder<PrivacyDeletionRequest>b){b.ToTable("PrivacyDeletionRequests");b.HasKey(x=>x.Id);b.Property(x=>x.Status).HasMaxLength(40).IsRequired();b.Property(x=>x.Reason).HasMaxLength(1000);b.HasIndex(x=>new{x.UserId,x.Status});b.HasOne(x=>x.User).WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Restrict);}}
