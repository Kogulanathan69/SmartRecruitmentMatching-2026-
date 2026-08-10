using NexHire.Application.DTOs.Job;
using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces.Repositories;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task<Job?> GetOwnedByIdAsync(
        Guid jobId,
        Guid employerUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> SearchAsync(
        JobSearchDto search,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByEmployerAsync(
        Guid employerUserId,
        CancellationToken cancellationToken = default);

    Task<bool> EmployerOwnsCompanyAsync(
        Guid companyId,
        Guid employerUserId,
        CancellationToken cancellationToken = default);

    Task<Skill?> GetSkillByNameAsync(
        string skillName,
        CancellationToken cancellationToken = default);

    Task AddSkillAsync(
        Skill skill,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Job job,
        CancellationToken cancellationToken = default);

    void Update(Job job);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
