using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;
namespace NexHire.Infrastructure.Data.Configurations;
public class OutboxMessageConfiguration:IEntityTypeConfiguration<OutboxMessage>{public void Configure(EntityTypeBuilder<OutboxMessage>b){b.ToTable("OutboxMessages");b.HasKey(x=>x.Id);b.Property(x=>x.Type).HasMaxLength(150).IsRequired();b.Property(x=>x.Payload).IsRequired();b.HasIndex(x=>new{x.ProcessedAtUtc,x.OccurredAtUtc});}}
