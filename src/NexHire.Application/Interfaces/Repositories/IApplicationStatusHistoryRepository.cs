using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces.Repositories;

public interface IApplicationStatusHistoryRepository
{
    Task AddAsync(ApplicationStatusHistory history);
    Task<IReadOnlyList<ApplicationStatusHistory>> GetByApplicationIdAsync(Guid applicationId);
}
