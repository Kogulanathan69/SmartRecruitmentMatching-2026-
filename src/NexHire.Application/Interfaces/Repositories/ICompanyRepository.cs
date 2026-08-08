using NexHire.Domain.Entities;
using NexHire.Domain.Enums;

namespace NexHire.Application.Interfaces.Repositories;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id);
    Task<Company?> GetByIdWithDetailsAsync(Guid id);
    Task<Company?> GetByRegistrationNumberAsync(string registrationNumber);
    Task<Company?> GetByOfficialEmailAsync(string officialEmail);
    Task<IReadOnlyList<Company>> GetAllAsync();
    Task<IReadOnlyList<Company>> GetByOwnerUserIdAsync(Guid userId);
    Task<IReadOnlyList<Company>> GetByStatusAsync(CompanyStatus status);
    Task AddAsync(Company company);
    void Update(Company company);
    void Delete(Company company);
}
