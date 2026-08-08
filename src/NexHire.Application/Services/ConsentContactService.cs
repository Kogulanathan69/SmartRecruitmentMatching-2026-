using NexHire.Application.DTOs.ConsentContact;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;

namespace NexHire.Application.Services;

public class ConsentContactService : IConsentContactService
{
    private readonly IContactRequestRepository _contactRepository;
    private readonly IJobApplicationRepository _applicationRepository;

    public ConsentContactService(
        IContactRequestRepository contactRepository,
        IJobApplicationRepository applicationRepository)
    {
        _contactRepository = contactRepository;
        _applicationRepository = applicationRepository;
    }

    public async Task<ContactRequestResponseDto> CreateRequestAsync(
        Guid applicationId,
        Guid employerUserId,
        CreateContactRequestDto dto)
    {
        var application =
            await _applicationRepository.GetByIdAsync(applicationId);

        if (application is null)
        {
            throw new KeyNotFoundException(
                "Job application was not found.");
        }

        if (!Enum.TryParse<ApplicationStatus>(
                application.Status,
                true,
                out var applicationStatus))
        {
            throw new InvalidOperationException(
                "Application contains an invalid status.");
        }

        if (applicationStatus != ApplicationStatus.Shortlisted)
        {
            throw new InvalidOperationException(
                "Contact request can only be created " +
                "for a shortlisted application.");
        }

        var existing =
            await _contactRepository
                .GetByApplicationIdAsync(applicationId);

        if (existing is not null)
        {
            throw new InvalidOperationException(
                "A contact request already exists " +
                "for this application.");
        }

        var now = DateTime.UtcNow;

        var request = new ContactRequest
        {
            Id = Guid.NewGuid(),
            JobApplicationId = applicationId,
            EmployerUserId = employerUserId,
            CandidateUserId = application.CandidateId,
            Message = dto.Message,
            Status = ContactRequestStatus.Pending,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(7)
        };

        await _contactRepository.AddAsync(request);

        await _applicationRepository.SaveChangesAsync();

        return MapRequest(request, application);
    }

    public async Task<IReadOnlyList<ContactRequestResponseDto>>
        GetEmployerRequestsAsync(Guid employerUserId)
    {
        var requests =
            await _contactRepository
                .GetByEmployerUserIdAsync(employerUserId);

        return requests
            .Select(x =>
                MapRequest(x, x.JobApplication))
            .ToList();
    }

    public async Task<IReadOnlyList<ContactRequestResponseDto>>
        GetCandidateRequestsAsync(Guid candidateUserId)
    {
        var requests =
            await _contactRepository
                .GetByCandidateUserIdAsync(candidateUserId);

        var now = DateTime.UtcNow;

        var changed = false;

        foreach (var request in requests)
        {
            if (request.Status ==
                    ContactRequestStatus.Pending &&
                request.ExpiresAtUtc <= now)
            {
                request.Status =
                    ContactRequestStatus.Expired;

                request.DecisionAtUtc = now;

                _contactRepository.Update(request);

                changed = true;
            }
        }

        if (changed)
        {
            await _applicationRepository.SaveChangesAsync();
        }

        return requests
            .Select(x =>
                MapRequest(x, x.JobApplication))
            .ToList();
    }

    public async Task<ContactRequestResponseDto> DecideAsync(
        Guid requestId,
        Guid candidateUserId,
        ContactDecisionRequestDto dto)
    {
        var request =
            await _contactRepository
                .GetByIdWithDetailsAsync(requestId);

        if (request is null)
        {
            throw new KeyNotFoundException(
                "Contact request was not found.");
        }

        if (request.CandidateUserId != candidateUserId)
        {
            throw new UnauthorizedAccessException(
                "You are not allowed to decide this request.");
        }

        if (request.Status != ContactRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                "This contact request has already been finalized.");
        }

        var now = DateTime.UtcNow;

        if (request.ExpiresAtUtc <= now)
        {
            request.Status =
                ContactRequestStatus.Expired;

            request.DecisionAtUtc = now;

            _contactRepository.Update(request);

            await _applicationRepository.SaveChangesAsync();

            throw new InvalidOperationException(
                "This contact request has expired.");
        }

        if (!Enum.TryParse<ContactRequestStatus>(
                dto.Decision,
                true,
                out var decision))
        {
            throw new ArgumentException(
                "Decision must be Accepted or Declined.");
        }

        if (decision != ContactRequestStatus.Accepted &&
            decision != ContactRequestStatus.Declined)
        {
            throw new ArgumentException(
                "Decision must be Accepted or Declined.");
        }

        request.Status = decision;
        request.DecisionAtUtc = now;

        _contactRepository.Update(request);

        await _applicationRepository.SaveChangesAsync();

        return MapRequest(
            request,
            request.JobApplication);
    }

    public async Task<CandidateContactDetailsDto>
        GetAcceptedDetailsAsync(
            Guid requestId,
            Guid employerUserId)
    {
        var request =
            await _contactRepository
                .GetByIdWithDetailsAsync(requestId);

        if (request is null)
        {
            throw new KeyNotFoundException(
                "Contact request was not found.");
        }

        if (request.EmployerUserId != employerUserId)
        {
            throw new UnauthorizedAccessException(
                "You do not own this contact request.");
        }

        if (request.Status !=
            ContactRequestStatus.Accepted)
        {
            throw new InvalidOperationException(
                "Candidate contact details are available " +
                "only after the request is accepted.");
        }

        /*
         * Current project structure has no User/Profile
         * contact source connected to JobApplication.
         *
         * Returning fake candidate name/email/phone would
         * be incorrect.
         */

        throw new InvalidOperationException(
            "Candidate profile/contact data source has not " +
            "yet been connected to ConsentContactService.");
    }

    private static ContactRequestResponseDto MapRequest(
        ContactRequest request,
        JobApplication? application)
    {
        return new ContactRequestResponseDto
        {
            Id = request.Id,
            ApplicationId = request.JobApplicationId,

            // Current JobApplication has VacancyId.
            // DTO calls this JobId.
            JobId = application?.VacancyId ?? Guid.Empty,

            JobTitle = string.Empty,
            CompanyName = string.Empty,
            CandidateName = string.Empty,

            Message = request.Message,
            Status = request.Status.ToString(),
            CreatedAtUtc = request.CreatedAtUtc,
            ExpiresAtUtc = request.ExpiresAtUtc,
            DecisionAtUtc = request.DecisionAtUtc
        };
    }
}