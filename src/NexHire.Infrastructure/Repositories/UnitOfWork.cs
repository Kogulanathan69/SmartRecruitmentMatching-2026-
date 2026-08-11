using NexHire.Application.Interfaces.Repositories;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(
        AppDbContext context,
        ICompanyRepository companies)
    {
        _context = context;
        Companies = companies;
    }

    public ICompanyRepository Companies { get; }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}