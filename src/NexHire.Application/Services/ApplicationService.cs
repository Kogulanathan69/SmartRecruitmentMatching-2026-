using NexHire.Application.Common.Exceptions;
using NexHire.Application.DTOs.Application;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;

namespace NexHire.Application.Services;

public sealed class ApplicationService
    : IApplicationService
{
    private readonly IApplicationRepository _applications;
    private readonly IJobRepository _jobs;
    private readonly IJobSeekerRepository _jobSeekers;

    public ApplicationService(
        IApplicationRepository applications,
        IJobRepository jobs,
        IJobSeekerRepository jobSeekers)
    {
        _applications = applications;
        _jobs = jobs;
        _jobSeekers = jobSeekers;
    }

    public async Task<ApplicationResponseDto> ApplyAsync(
        Guid candidateUserId,
        ApplyJobDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.JobId == Guid.Empty)
        {
            throw new BusinessRuleException(
                "JobId is required.");
        }

        var profile =
            await _jobSeekers
                .GetByUserIdAsync(
                    candidateUserId);

        if (profile is null)
        {
            throw new BusinessRuleException(
                "Create your job seeker profile before applying.");
        }

        var job =
            await _jobs.GetByIdAsync(
                dto.JobId,
                cancellationToken)
            ?? throw new NotFoundException(
                "Job vacancy was not found.");

        if (job.Status != JobStatus.Published)
        {
            throw new BusinessRuleException(
                "This vacancy is not open for applications.");
        }

        if (job.ClosingDate.HasValue &&
            job.ClosingDate.Value <
                DateTime.UtcNow)
        {
            throw new BusinessRuleException(
                "This vacancy has already closed.");
        }

        var duplicate =
            await _applications.ExistsAsync(
                candidateUserId,
                dto.JobId,
                cancellationToken);

        if (duplicate)
        {
            throw new BusinessRuleException(
                "You have already applied to this vacancy.");
        }

        var application =
            new JobApplication
            {
                JobApplicationId =
                    Guid.NewGuid(),

                CandidateId =
                    candidateUserId,

                VacancyId =
                    dto.JobId,

                Status =
                    ApplicationStatus.Submitted
                        .ToString(),

                AppliedAtUtc =
                    DateTime.UtcNow
            };

        await _applications.AddAsync(
            application,
            cancellationToken);

        await _applications.SaveChangesAsync(
            cancellationToken);

        return await _applications
            .GetResponseByIdAsync(
                application.JobApplicationId,
                cancellationToken)
            ?? throw new InvalidOperationException(
                "Application was saved but could not be reloaded.");
    }

    public Task<IReadOnlyList<ApplicationResponseDto>>
        GetMineAsync(
            Guid candidateUserId,
            CancellationToken cancellationToken = default)
    {
        return _applications
            .GetByCandidateAsync(
                candidateUserId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationResponseDto>>
        GetForJobAsync(
            Guid jobId,
            Guid requestingUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default)
    {
        if (!isAdmin)
        {
            var owned =
                await _jobs.GetOwnedByIdAsync(
                    jobId,
                    requestingUserId,
                    cancellationToken);

            if (owned is null)
            {
                throw new UnauthorizedException(
                    "You do not own this job vacancy.");
            }
        }

        return await _applications
            .GetByJobAsync(
                jobId,
                cancellationToken);
    }

    public async Task<ApplicationResponseDto> GetByIdAsync(
        Guid applicationId,
        Guid requestingUserId,
        bool isEmployer,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var entity =
            await _applications
                .GetEntityByIdAsync(
                    applicationId,
                    cancellationToken)
            ?? throw new NotFoundException(
                "Application was not found.");

        var allowed =
            isAdmin ||
            entity.CandidateId ==
                requestingUserId;

        if (!allowed && isEmployer)
        {
            var ownedJob =
                await _jobs.GetOwnedByIdAsync(
                    entity.VacancyId,
                    requestingUserId,
                    cancellationToken);

            allowed =
                ownedJob is not null;
        }

        if (!allowed)
        {
            throw new UnauthorizedException(
                "You do not have permission to view this application.");
        }

        return await _applications
            .GetResponseByIdAsync(
                applicationId,
                cancellationToken)
            ?? throw new NotFoundException(
                "Application was not found.");
    }
}
