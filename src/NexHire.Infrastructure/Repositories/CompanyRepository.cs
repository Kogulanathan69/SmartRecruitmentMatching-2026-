using Microsoft.EntityFrameworkCore;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly AppDbContext _context;

    public CompanyRepository(AppDbContext context) => _context = context;

    public Task<Company?> GetByIdAsync(Guid id) =>
        _context.Companies.FirstOrDefaultAsync(c => c.Id == id);

    public Task<Company?> GetByIdWithDetailsAsync(Guid id) =>
        _context.Companies
            .Include(c => c.Documents)
            .Include(c => c.Verification)
            .Include(c => c.Jobs)
            .FirstOrDefaultAsync(c => c.Id == id);

    public Task<Company?> GetByRegistrationNumberAsync(string registrationNumber) =>
        _context.Companies.FirstOrDefaultAsync(c => c.RegistrationNumber == registrationNumber);

    public Task<Company?> GetByOfficialEmailAsync(string officialEmail) =>
        _context.Companies.FirstOrDefaultAsync(c => c.OfficialEmail == officialEmail);

    public async Task<IReadOnlyList<Company>> GetAllAsync() =>
        await _context.Companies.AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<Company>> GetByOwnerUserIdAsync(Guid userId) =>
        await _context.Companies
            .Where(c => c.CreatedByUserId == userId)
            .Include(c => c.Documents)
            .Include(c => c.Verification)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IReadOnlyList<Company>> GetByStatusAsync(CompanyStatus status) =>
        await _context.Companies
            .Where(c => c.Status == status)
            .Include(c => c.Documents)
            .Include(c => c.Verification)
            .AsNoTracking()
            .ToListAsync();

    public Task AddAsync(Company company) => _context.Companies.AddAsync(company).AsTask();
    public void Update(Company company) => _context.Companies.Update(company);
    public void Delete(Company company) => _context.Companies.Remove(company);
}
