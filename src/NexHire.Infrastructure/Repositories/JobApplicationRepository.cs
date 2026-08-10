using Microsoft.EntityFrameworkCore;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Repositories;
public class JobApplicationRepository : IJobApplicationRepository
{
    private readonly AppDbContext _context;
    public JobApplicationRepository(AppDbContext context)=>_context=context;
    public Task<JobApplication?> GetByIdAsync(Guid applicationId)=>_context.JobApplications
        .Include(x=>x.Candidate).ThenInclude(x=>x.User)
        .Include(x=>x.Vacancy).ThenInclude(x=>x.Company)
        .Include(x=>x.StatusHistory)
        .FirstOrDefaultAsync(x=>x.JobApplicationId==applicationId);
    public void Update(JobApplication application)=>_context.JobApplications.Update(application);
    public Task<int> SaveChangesAsync()=>_context.SaveChangesAsync();
}
