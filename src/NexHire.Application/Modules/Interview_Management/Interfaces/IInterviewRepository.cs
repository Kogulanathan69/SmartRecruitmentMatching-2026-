using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces.Repositories;

public interface IInterviewRepository
{
    Task AddAsync(Interview interview, CancellationToken cancellationToken = default);
    Task<Interview?> GetByIdAsync(Guid interviewId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Interview> Items, int TotalCount)> GetCompanyPageAsync(
        Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Interview> Items, int TotalCount)> GetCandidatePageAsync(
        Guid candidateProfileId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> HasEvaluatorScoreAsync(
        Guid interviewId, Guid evaluatorUserId, CancellationToken cancellationToken = default);
    Task AddScoreAsync(InterviewScore score, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
