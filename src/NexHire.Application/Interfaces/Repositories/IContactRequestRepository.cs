using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces.Repositories;

public interface IContactRequestRepository
{
    Task<ContactRequest?> GetByIdWithDetailsAsync(Guid id);
    Task<ContactRequest?> GetByApplicationIdAsync(Guid applicationId);
    Task<IReadOnlyList<ContactRequest>> GetByEmployerUserIdAsync(Guid employerUserId);
    Task<IReadOnlyList<ContactRequest>> GetByCandidateUserIdAsync(Guid candidateUserId);
    Task AddAsync(ContactRequest request);
    void Update(ContactRequest request);
}
