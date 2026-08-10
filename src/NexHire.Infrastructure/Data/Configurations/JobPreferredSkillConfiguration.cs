using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexHire.Domain.Entities;
namespace NexHire.Infrastructure.Data.Configurations;
public class JobPreferredSkillConfiguration : IEntityTypeConfiguration<JobPreferredSkill>
{
 public void Configure(EntityTypeBuilder<JobPreferredSkill> b){ b.ToTable("JobPreferredSkills"); b.HasKey(x=>x.Id); b.HasIndex(x=>new{x.JobId,x.SkillId}).IsUnique(); b.HasOne(x=>x.Job).WithMany(x=>x.PreferredSkills).HasForeignKey(x=>x.JobId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x=>x.Skill).WithMany().HasForeignKey(x=>x.SkillId).OnDelete(DeleteBehavior.Restrict); }
}
