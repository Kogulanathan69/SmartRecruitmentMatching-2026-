using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces.Repositories;

public interface IResumeRepository
{
    Task<ResumeTemplate?> GetActiveTemplateByIdAsync(Guid id);
    Task<IReadOnlyList<ResumeTemplate>> GetActiveTemplatesAsync();
    Task AddAsync(Resume resume);
    void Remove(Resume resume);
    Task<int> SaveChangesAsync();
}
