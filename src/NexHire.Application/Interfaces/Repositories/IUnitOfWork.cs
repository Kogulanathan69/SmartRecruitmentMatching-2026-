namespace NexHire.Application.Interfaces.Repositories;

public interface IUnitOfWork
{
    ICompanyRepository Companies { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}