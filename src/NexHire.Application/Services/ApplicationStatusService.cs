using NexHire.Application.DTOs.ApplicationStatus;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;

namespace NexHire.Application.Services;

public class ApplicationStatusService : IApplicationStatusService
{
    private readonly IJobApplicationRepository _applicationRepository;
    private readonly IApplicationStatusHistoryRepository _historyRepository;
    private readonly IJobRepository _jobRepository;
    private readonly INotificationService _notificationService;

    public ApplicationStatusService(
        IJobApplicationRepository applicationRepository,
        IApplicationStatusHistoryRepository historyRepository,
        IJobRepository jobRepository,
        INotificationService notificationService)
    {
        _applicationRepository = applicationRepository;
        _historyRepository = historyRepository;
        _jobRepository = jobRepository;
        _notificationService = notificationService;
    }

    public async Task<ApplicationStatusResultDto> UpdateByEmployerAsync(
        Guid applicationId,
        Guid employerUserId,
        UpdateApplicationStatusRequestDto dto)
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

        if (ownedJob.Status is JobStatus.Closed
            or JobStatus.Expired
            or JobStatus.Suspended)
        {
            throw new InvalidOperationException(
                $"Application status cannot be changed while the job is {ownedJob.Status}.");
        }

        if (!Enum.TryParse<ApplicationStatus>(
                application.Status,
                true,
                out var currentStatus))
        {
            throw new InvalidOperationException(
                $"Invalid current application status: {application.Status}");
        }

        if (!Enum.TryParse<ApplicationStatus>(
                dto.Status,
                true,
                out var newStatus))
        {
            throw new ArgumentException(
                $"Invalid application status: {dto.Status}");
        }

        if (currentStatus == newStatus)
        {
            return new ApplicationStatusResultDto
            {
                ApplicationId = application.JobApplicationId,
                Status = application.Status,
                StatusUpdatedAtUtc = application.UpdatedAtUtc,
                Changed = false
            };
        }

        if (!IsEmployerTransitionAllowed(
                currentStatus,
                newStatus))
        {
            throw new InvalidOperationException(
                $"Application status cannot change from {currentStatus} to {newStatus}.");
        }

        var changedAt = DateTime.UtcNow;

        var history = new ApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            JobApplicationId = application.JobApplicationId,
            FromStatus = currentStatus,
            ToStatus = newStatus,
            ChangedByUserId = employerUserId,
            ChangedByRole = "Employer",
            Reason = dto.Reason,
            ChangedAtUtc = changedAt
        };

        application.Status = newStatus.ToString();
        application.UpdatedAtUtc = changedAt;

        await _historyRepository.AddAsync(history);

        _applicationRepository.Update(application);

        await _applicationRepository.SaveChangesAsync();

        await _notificationService.CreateAsync(
            application.CandidateId,
            "ApplicationStatusChanged",
            "Application status updated",
            $"Your application status is now {newStatus}.",
            application.JobApplicationId);

        return new ApplicationStatusResultDto
        {
            ApplicationId = application.JobApplicationId,
            Status = application.Status,
            StatusUpdatedAtUtc = application.UpdatedAtUtc,
            Changed = true
        };
    }

    public async Task<ApplicationStatusResultDto> WithdrawByCandidateAsync(
        Guid applicationId,
        Guid candidateUserId,
        WithdrawApplicationRequestDto dto)
    {
        var application =
            await _applicationRepository.GetByIdAsync(applicationId);

        if (application is null)
        {
            throw new KeyNotFoundException(
                "Job application was not found.");
        }

        if (application.CandidateId != candidateUserId)
        {
            throw new UnauthorizedAccessException(
                "You do not own this application.");
        }

        if (!Enum.TryParse<ApplicationStatus>(
                application.Status,
                true,
                out var currentStatus))
        {
            throw new InvalidOperationException(
                $"Invalid current application status: {application.Status}");
        }

        if (!CanCandidateWithdraw(currentStatus))
        {
            throw new InvalidOperationException(
                $"Application cannot be withdrawn from {currentStatus}.");
        }

        var changedAt = DateTime.UtcNow;

        var history = new ApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            JobApplicationId = application.JobApplicationId,
            FromStatus = currentStatus,
            ToStatus = ApplicationStatus.Withdrawn,
            ChangedByUserId = candidateUserId,
            ChangedByRole = "JobSeeker",
            Reason = dto.Reason,
            ChangedAtUtc = changedAt
        };

        application.Status =
            ApplicationStatus.Withdrawn.ToString();

        application.UpdatedAtUtc = changedAt;

        await _historyRepository.AddAsync(history);

        _applicationRepository.Update(application);

        await _applicationRepository.SaveChangesAsync();

        var job =
            await _jobRepository.GetByIdAsync(
                application.VacancyId);

        if (job is not null)
        {
            await _notificationService.CreateAsync(
                job.Company.CreatedByUserId,
                "ApplicationWithdrawn",
                "Candidate withdrew application",
                $"A candidate withdrew from {job.Title}.",
                application.JobApplicationId);
        }

        return new ApplicationStatusResultDto
        {
            ApplicationId = application.JobApplicationId,
            Status = application.Status,
            StatusUpdatedAtUtc = application.UpdatedAtUtc,
            Changed = true
        };
    }

    public async Task<IReadOnlyList<ApplicationStatusHistoryDto>>
        GetCandidateHistoryAsync(
            Guid applicationId,
            Guid candidateUserId)
    {
        var application =
            await _applicationRepository.GetByIdAsync(applicationId);

        if (application is null)
        {
            throw new KeyNotFoundException(
                "Job application was not found.");
        }

        if (application.CandidateId != candidateUserId)
        {
            throw new UnauthorizedAccessException(
                "You do not own this application.");
        }

        var history =
            await _historyRepository
                .GetByApplicationIdAsync(applicationId);

        return history
            .Select(MapHistory)
            .ToList();
    }

    public async Task<IReadOnlyList<ApplicationStatusHistoryDto>>
        GetEmployerHistoryAsync(
            Guid applicationId,
            Guid employerUserId)
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

        var history =
            await _historyRepository
                .GetByApplicationIdAsync(applicationId);

        return history
            .Select(MapHistory)
            .ToList();
    }

    private static ApplicationStatusHistoryDto MapHistory(
        ApplicationStatusHistory history)
    {
        return new ApplicationStatusHistoryDto
        {
            Id = history.Id,
            ApplicationId = history.JobApplicationId,
            FromStatus = history.FromStatus?.ToString(),
            ToStatus = history.ToStatus.ToString(),
            ChangedByUserId = history.ChangedByUserId,
            ChangedByRole = history.ChangedByRole,
            Reason = history.Reason,
            ChangedAtUtc = history.ChangedAtUtc
        };
    }

    private static bool IsEmployerTransitionAllowed(
        ApplicationStatus current,
        ApplicationStatus next)
    {
        return current switch
        {
            ApplicationStatus.Submitted =>
                next == ApplicationStatus.UnderReview,

            ApplicationStatus.UnderReview =>
                next == ApplicationStatus.Shortlisted ||
                next == ApplicationStatus.WaitingList ||
                next == ApplicationStatus.Rejected,

            ApplicationStatus.Shortlisted =>
                next == ApplicationStatus.WaitingList ||
                next == ApplicationStatus.Rejected,

            ApplicationStatus.WaitingList =>
                next == ApplicationStatus.Shortlisted ||
                next == ApplicationStatus.Rejected,

            _ => false
        };
    }

    private static bool CanCandidateWithdraw(
        ApplicationStatus current)
    {
        return current == ApplicationStatus.Submitted ||
               current == ApplicationStatus.UnderReview ||
               current == ApplicationStatus.Shortlisted ||
               current == ApplicationStatus.WaitingList;
    }
}
