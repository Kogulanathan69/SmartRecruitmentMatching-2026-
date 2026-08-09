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
    private readonly IJobRepository _jobRepository;
    private readonly IUserRepository _userRepository;

    public ConsentContactService(
        IContactRequestRepository contactRepository,
        IJobApplicationRepository applicationRepository,
        IJobRepository jobRepository,
        IUserRepository userRepository)
    {
        _contactRepository = contactRepository;
        _applicationRepository = applicationRepository;
        _jobRepository = jobRepository;
        _userRepository = userRepository;
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

        var ownedJob =
            await _jobRepository.GetOwnedByIdAsync(
                application.VacancyId,
                employerUserId);

        if (ownedJob is null)
        {
            throw new UnauthorizedAccessException(
                "You do not own the job associated with this application.");
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
                "Contact request can only be created for a shortlisted application.");
        }

        var existing =
            await _contactRepository
                .GetByApplicationIdAsync(applicationId);

        if (existing is not null)
        {
            throw new InvalidOperationException(
                "A contact request already exists for this application.");
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

        return await MapRequestAsync(
            request,
            application,
            ownedJob);
    }

    public async Task<IReadOnlyList<ContactRequestResponseDto>>
        GetEmployerRequestsAsync(Guid employerUserId)
    {
        var requests =
            await _contactRepository
                .GetByEmployerUserIdAsync(employerUserId);

        var result =
            new List<ContactRequestResponseDto>();

        foreach (var request in requests)
        {
            result.Add(
                await MapRequestAsync(
                    request,
                    request.JobApplication));
        }

        return result;
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
            if (request.Status == ContactRequestStatus.Pending &&
                request.ExpiresAtUtc <= now)
            {
                request.Status = ContactRequestStatus.Expired;
                request.DecisionAtUtc = now;

                _contactRepository.Update(request);

                changed = true;
            }
        }

        if (changed)
        {
            await _applicationRepository.SaveChangesAsync();
        }

        var result =
            new List<ContactRequestResponseDto>();

        foreach (var request in requests)
        {
            result.Add(
                await MapRequestAsync(
                    request,
                    request.JobApplication));
        }

        return result;
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
            request.Status = ContactRequestStatus.Expired;
            request.DecisionAtUtc = now;

            _contactRepository.Update(request);

            await _applicationRepository.SaveChangesAsync();

            throw new InvalidOperationException(
                "This contact request has expired.");
        }

        var normalizedDecision =
            dto.Decision.Trim().ToLowerInvariant() switch
            {
                "accept" or "accepted" =>
                    ContactRequestStatus.Accepted,

                "decline" or "declined" =>
                    ContactRequestStatus.Declined,

                _ => throw new ArgumentException(
                    "Decision must be Accept or Decline.")
            };

        request.Status = normalizedDecision;
        request.DecisionAtUtc = now;

        _contactRepository.Update(request);

        await _applicationRepository.SaveChangesAsync();

        return await MapRequestAsync(
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

        if (request.Status != ContactRequestStatus.Accepted)
        {
            throw new InvalidOperationException(
                "Candidate contact details are available only after the request is accepted.");
        }

        var candidate =
            await _userRepository.GetByIdAsync(
                request.CandidateUserId);

        if (candidate is null)
        {
            throw new KeyNotFoundException(
                "Candidate user was not found.");
        }

        return new CandidateContactDetailsDto
        {
            ContactRequestId = request.Id,
            ApplicationId = request.JobApplicationId,
            CandidateName = candidate.FullName,
            Email = candidate.Email,
            PhoneNumber = candidate.PhoneNumber
        };
    }

    private async Task<ContactRequestResponseDto> MapRequestAsync(
        ContactRequest request,
        JobApplication? application,
        Job? knownJob = null)
    {
        Job? job = knownJob;

        if (job is null && application is not null)
        {
            job =
                await _jobRepository.GetByIdAsync(
                    application.VacancyId);
        }

        var candidate =
            await _userRepository.GetByIdAsync(
                request.CandidateUserId);

        return new ContactRequestResponseDto
        {
            Id = request.Id,
            ApplicationId = request.JobApplicationId,
            JobId = application?.VacancyId ?? Guid.Empty,

            JobTitle =
                job?.Title ??
                string.Empty,

            CompanyName =
                job?.Company?.Name ??
                string.Empty,

            CandidateName =
                candidate?.FullName ??
                string.Empty,

            Message = request.Message,
            Status = request.Status.ToString(),
            CreatedAtUtc = request.CreatedAtUtc,
            ExpiresAtUtc = request.ExpiresAtUtc,
            DecisionAtUtc = request.DecisionAtUtc
        };
    }
}
