using Microsoft.EntityFrameworkCore;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Repositories;

public class ApplicationStatusHistoryRepository
    : IApplicationStatusHistoryRepository
{
    private readonly AppDbContext _context;

    public ApplicationStatusHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ApplicationStatusHistory history)
    {
        await _context.ApplicationStatusHistories.AddAsync(history);
    }

    public async Task<IReadOnlyList<ApplicationStatusHistory>>
        GetByApplicationIdAsync(Guid applicationId)
    {
        return await _context.ApplicationStatusHistories
            .AsNoTracking()
            .Where(x => x.JobApplicationId == applicationId)
            .OrderBy(x => x.ChangedAtUtc)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }
}