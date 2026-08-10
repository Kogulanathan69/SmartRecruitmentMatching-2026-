using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;
namespace NexHire.Infrastructure.Data.Configurations;
public class AuditLogConfiguration:IEntityTypeConfiguration<AuditLog>{public void Configure(EntityTypeBuilder<AuditLog>b){b.ToTable("AuditLogs");b.HasKey(x=>x.Id);b.Property(x=>x.Action).HasMaxLength(150).IsRequired();b.Property(x=>x.EntityType).HasMaxLength(100).IsRequired();b.Property(x=>x.Details).HasMaxLength(4000);b.HasIndex(x=>x.CreatedAtUtc);}}
