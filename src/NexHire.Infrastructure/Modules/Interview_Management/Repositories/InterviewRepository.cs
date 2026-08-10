using Microsoft.EntityFrameworkCore;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Repositories;

public sealed class InterviewRepository : IInterviewRepository
{
    private readonly Member5DbContext _dbContext;

    public InterviewRepository(Member5DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Interview interview, CancellationToken cancellationToken = default) =>
        await _dbContext.Interviews.AddAsync(interview, cancellationToken);

    public Task<Interview?> GetByIdAsync(Guid interviewId, CancellationToken cancellationToken = default) =>
        _dbContext.Interviews.SingleOrDefaultAsync(item => item.InterviewId == interviewId, cancellationToken);

    public async Task<(IReadOnlyList<Interview> Items, int TotalCount)> GetCompanyPageAsync(
        Guid companyId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Interviews.AsNoTracking().Where(item => item.CompanyId == companyId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(item => item.ScheduledAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<(IReadOnlyList<Interview> Items, int TotalCount)> GetCandidatePageAsync(
        Guid candidateProfileId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Interviews.AsNoTracking().Where(item => item.CandidateProfileId == candidateProfileId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(item => item.ScheduledAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task<bool> HasEvaluatorScoreAsync(
        Guid interviewId,
        Guid evaluatorUserId,
        CancellationToken cancellationToken = default) =>
        _dbContext.InterviewScores.AnyAsync(
            item => item.InterviewId == interviewId && item.EvaluatorUserId == evaluatorUserId,
            cancellationToken);

    public async Task AddScoreAsync(InterviewScore score, CancellationToken cancellationToken = default) =>
        await _dbContext.InterviewScores.AddAsync(score, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
