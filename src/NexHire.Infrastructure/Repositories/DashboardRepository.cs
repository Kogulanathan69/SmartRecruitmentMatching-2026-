using Microsoft.EntityFrameworkCore;
using NexHire.Application.DTOs.Dashboard;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Domain.Enums;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<JobSeekerDashboardDto> GetJobSeekerAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var applications =
            _context.JobApplications
                .AsNoTracking()
                .Where(x => x.CandidateId == userId);

        var recentApplications =
            await (
                from application in applications
                join job in _context.Jobs.AsNoTracking()
                    on application.VacancyId equals job.Id
                join company in _context.Companies.AsNoTracking()
                    on job.CompanyId equals company.Id
                orderby application.AppliedAtUtc descending
                select new RecentApplicationDto
                {
                    ApplicationId =
                        application.JobApplicationId,

                    JobId =
                        job.Id,

                    JobTitle =
                        job.Title,

                    CompanyName =
                        company.Name,

                    Status =
                        application.Status,

                    AppliedAtUtc =
                        application.AppliedAtUtc
                })
                .Take(5)
                .ToListAsync(cancellationToken);

        return new JobSeekerDashboardDto
        {
            TotalApplications =
                await applications.CountAsync(
                    cancellationToken),

            SubmittedApplications =
                await applications.CountAsync(
                    x => x.Status == "Submitted",
                    cancellationToken),

            UnderReviewApplications =
                await applications.CountAsync(
                    x => x.Status == "UnderReview",
                    cancellationToken),

            ShortlistedApplications =
                await applications.CountAsync(
                    x => x.Status == "Shortlisted",
                    cancellationToken),

            WaitingListApplications =
                await applications.CountAsync(
                    x => x.Status == "WaitingList",
                    cancellationToken),

            RejectedApplications =
                await applications.CountAsync(
                    x => x.Status == "Rejected",
                    cancellationToken),

            WithdrawnApplications =
                await applications.CountAsync(
                    x => x.Status == "Withdrawn",
                    cancellationToken),

            PendingContactRequests =
                await _context.ContactRequests
                    .AsNoTracking()
                    .CountAsync(
                        x =>
                            x.CandidateUserId == userId &&
                            x.Status ==
                                ContactRequestStatus.Pending,
                        cancellationToken),

            UnreadNotifications =
                await _context.Notifications
                    .AsNoTracking()
                    .CountAsync(
                        x =>
                            x.UserId == userId &&
                            !x.IsRead,
                        cancellationToken),

            RecentApplications =
                recentApplications
        };
    }

    public async Task<EmployerDashboardDto> GetEmployerAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var jobs =
            _context.Jobs
                .AsNoTracking()
                .Where(
                    x =>
                        x.Company.CreatedByUserId ==
                        userId);

        var applications =
            from application in
                _context.JobApplications.AsNoTracking()
            join job in jobs
                on application.VacancyId equals job.Id
            select application;

        var recentJobs =
            await jobs
                .OrderByDescending(
                    x => x.CreatedAtUtc)
                .Select(
                    job => new RecentEmployerJobDto
                    {
                        JobId =
                            job.Id,

                        Title =
                            job.Title,

                        Status =
                            job.Status.ToString(),

                        VacancyCount =
                            job.VacancyCount,

                        ApplicationCount =
                            _context.JobApplications.Count(
                                application =>
                                    application.VacancyId ==
                                    job.Id),

                        PostedAt =
                            job.PostedAt,

                        ClosingDate =
                            job.ClosingDate
                    })
                .Take(5)
                .ToListAsync(cancellationToken);

        return new EmployerDashboardDto
        {
            TotalJobs =
                await jobs.CountAsync(
                    cancellationToken),

            PublishedJobs =
                await jobs.CountAsync(
                    x => x.Status == JobStatus.Published,
                    cancellationToken),

            ClosedJobs =
                await jobs.CountAsync(
                    x => x.Status == JobStatus.Closed,
                    cancellationToken),

            TotalApplications =
                await applications.CountAsync(
                    cancellationToken),

            SubmittedApplications =
                await applications.CountAsync(
                    x => x.Status == "Submitted",
                    cancellationToken),

            UnderReviewApplications =
                await applications.CountAsync(
                    x => x.Status == "UnderReview",
                    cancellationToken),

            ShortlistedApplications =
                await applications.CountAsync(
                    x => x.Status == "Shortlisted",
                    cancellationToken),

            WaitingListApplications =
                await applications.CountAsync(
                    x => x.Status == "WaitingList",
                    cancellationToken),

            PendingContactRequests =
                await _context.ContactRequests
                    .AsNoTracking()
                    .CountAsync(
                        x =>
                            x.EmployerUserId == userId &&
                            x.Status ==
                                ContactRequestStatus.Pending,
                        cancellationToken),

            UnreadNotifications =
                await _context.Notifications
                    .AsNoTracking()
                    .CountAsync(
                        x =>
                            x.UserId == userId &&
                            !x.IsRead,
                        cancellationToken),

            RecentJobs =
                recentJobs
        };
    }

    public async Task<AdminDashboardDto> GetAdminAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return new AdminDashboardDto
        {
            TotalUsers =
                await _context.Users
                    .AsNoTracking()
                    .CountAsync(cancellationToken),

            TotalJobSeekers =
                await _context.Users
                    .AsNoTracking()
                    .CountAsync(
                        x => x.Role == UserRole.JobSeeker,
                        cancellationToken),

            TotalEmployers =
                await _context.Users
                    .AsNoTracking()
                    .CountAsync(
                        x => x.Role == UserRole.Employer,
                        cancellationToken),

            TotalAdmins =
                await _context.Users
                    .AsNoTracking()
                    .CountAsync(
                        x => x.Role == UserRole.Admin,
                        cancellationToken),

            TotalCompanies =
                await _context.Companies
                    .AsNoTracking()
                    .CountAsync(cancellationToken),

            TotalJobs =
                await _context.Jobs
                    .AsNoTracking()
                    .CountAsync(cancellationToken),

            PublishedJobs =
                await _context.Jobs
                    .AsNoTracking()
                    .CountAsync(
                        x => x.Status == JobStatus.Published,
                        cancellationToken),

            ClosedJobs =
                await _context.Jobs
                    .AsNoTracking()
                    .CountAsync(
                        x => x.Status == JobStatus.Closed,
                        cancellationToken),

            TotalApplications =
                await _context.JobApplications
                    .AsNoTracking()
                    .CountAsync(cancellationToken),

            PendingContactRequests =
                await _context.ContactRequests
                    .AsNoTracking()
                    .CountAsync(
                        x =>
                            x.Status ==
                                ContactRequestStatus.Pending,
                        cancellationToken),

            TotalNotifications =
                await _context.Notifications
                    .AsNoTracking()
                    .CountAsync(cancellationToken),

            UnreadNotifications =
                await _context.Notifications
                    .AsNoTracking()
                    .CountAsync(
                        x => !x.IsRead,
                        cancellationToken)
        };
    }
}
