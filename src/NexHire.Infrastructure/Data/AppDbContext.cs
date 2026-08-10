using Microsoft.EntityFrameworkCore;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobApplication> JobApplications { get; set; } = null!;
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistories { get; set; } = null!;
    public DbSet<ContactRequest> ContactRequests { get; set; } = null!;
    public DbSet<MatchingRule> MatchingRules => Set<MatchingRule>();
    public DbSet<MatchResult> MatchResults => Set<MatchResult>();
    public DbSet<MatchScoreDetail> MatchScoreDetails => Set<MatchScoreDetail>();

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyDocument> CompanyDocuments => Set<CompanyDocument>();
    public DbSet<CompanyVerification> CompanyVerifications => Set<CompanyVerification>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobRequiredSkill> JobRequiredSkills => Set<JobRequiredSkill>();
    public DbSet<JobPreferredSkill> JobPreferredSkills => Set<JobPreferredSkill>();
    public DbSet<JobSeekerProfile> JobSeekerProfiles => Set<JobSeekerProfile>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<PrivacyDeletionRequest> PrivacyDeletionRequests => Set<PrivacyDeletionRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly
        );
    }
}