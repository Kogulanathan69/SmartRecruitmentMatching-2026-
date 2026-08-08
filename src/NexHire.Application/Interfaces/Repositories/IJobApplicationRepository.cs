using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces.Repositories;

public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(Guid applicationId);

    void Update(JobApplication application);

    Task<int> SaveChangesAsync();
}