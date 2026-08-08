using Microsoft.EntityFrameworkCore;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Repositories;

public class ResumeRepository : IResumeRepository
{
    private readonly AppDbContext _context;

    public ResumeRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<ResumeTemplate?> GetActiveTemplateByIdAsync(Guid id) =>
        _context.Set<ResumeTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

    public async Task<IReadOnlyList<ResumeTemplate>> GetActiveTemplatesAsync() =>
        await _context.Set<ResumeTemplate>()
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();

    public void Remove(Resume resume) =>
        _context.Set<Resume>().Remove(resume);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
