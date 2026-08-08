using NexHire.Application.DTOs.ConsentContact;

namespace NexHire.Application.Interfaces.Services;

public interface IConsentContactService
{
    Task<ContactRequestResponseDto> CreateRequestAsync(
        Guid applicationId,
        Guid employerUserId,
        CreateContactRequestDto dto);

    Task<IReadOnlyList<ContactRequestResponseDto>>
        GetEmployerRequestsAsync(Guid employerUserId);

    Task<IReadOnlyList<ContactRequestResponseDto>>
        GetCandidateRequestsAsync(Guid candidateUserId);

    Task<ContactRequestResponseDto> DecideAsync(
        Guid requestId,
        Guid candidateUserId,
        ContactDecisionRequestDto dto);

    Task<CandidateContactDetailsDto> GetAcceptedDetailsAsync(
        Guid requestId,
        Guid employerUserId);
}