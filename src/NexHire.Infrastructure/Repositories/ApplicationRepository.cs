using Microsoft.EntityFrameworkCore;
using NexHire.Application.DTOs.Application;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Repositories;

public sealed class ApplicationRepository
    : IApplicationRepository
{
    private readonly AppDbContext _context;

    public ApplicationRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(
        Guid candidateUserId,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        return _context.JobApplications
            .AsNoTracking()
            .AnyAsync(
                application =>
                    application.CandidateId ==
                        candidateUserId &&
                    application.VacancyId ==
                        jobId,
                cancellationToken);
    }

    public Task<JobApplication?> GetEntityByIdAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        return _context.JobApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(
                application =>
                    application.JobApplicationId ==
                        applicationId,
                cancellationToken);
    }

    public Task<ApplicationResponseDto?> GetResponseByIdAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        return QueryResponses()
            .SingleOrDefaultAsync(
                application =>
                    application.ApplicationId ==
                        applicationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationResponseDto>>
        GetByCandidateAsync(
            Guid candidateUserId,
            CancellationToken cancellationToken = default)
    {
        return await QueryResponses()
            .Where(application =>
                application.CandidateId ==
                    candidateUserId)
            .OrderByDescending(application =>
                application.AppliedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationResponseDto>>
        GetByJobAsync(
            Guid jobId,
            CancellationToken cancellationToken = default)
    {
        return await QueryResponses()
            .Where(application =>
                application.JobId == jobId)
            .OrderByDescending(application =>
                application.AppliedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(
        JobApplication application,
        CancellationToken cancellationToken = default)
    {
        return _context.JobApplications
            .AddAsync(
                application,
                cancellationToken)
            .AsTask();
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(
            cancellationToken);
    }

    private IQueryable<ApplicationResponseDto>
        QueryResponses()
    {
        return
            from application
                in _context.JobApplications
                    .AsNoTracking()

            join job
                in _context.Jobs.AsNoTracking()
                on application.VacancyId
                equals job.Id

            join company
                in _context.Companies.AsNoTracking()
                on job.CompanyId
                equals company.Id

            join candidate
                in _context.Users.AsNoTracking()
                on application.CandidateId
                equals candidate.Id

            select new ApplicationResponseDto
            {
                ApplicationId =
                    application.JobApplicationId,

                CandidateId =
                    application.CandidateId,

                CandidateName =
                    candidate.FirstName +
                    " " +
                    candidate.LastName,

                JobId =
                    application.VacancyId,

                JobTitle =
                    job.Title,

                CompanyId =
                    company.Id,

                CompanyName =
                    company.Name,

                Status =
                    application.Status,

                AppliedAtUtc =
                    application.AppliedAtUtc,

                UpdatedAtUtc =
                    application.UpdatedAtUtc
            };
    }
}
