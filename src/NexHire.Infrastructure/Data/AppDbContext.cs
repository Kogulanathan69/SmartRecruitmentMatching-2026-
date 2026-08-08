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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly
        );
    }
    // Company
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyDocument> CompanyDocuments => Set<CompanyDocument>();
    public DbSet<CompanyVerification> CompanyVerifications => Set<CompanyVerification>();
}