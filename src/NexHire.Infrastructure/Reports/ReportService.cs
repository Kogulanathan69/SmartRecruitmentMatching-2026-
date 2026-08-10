using Microsoft.EntityFrameworkCore;
using NexHire.Application.Common.Exceptions;
using NexHire.Application.DTOs.Reports;
using NexHire.Application.Interfaces.Services;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Reports;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PlatformSummaryDto> GetPlatformSummaryAsync()
    {
        return new PlatformSummaryDto
        {
            Users = await _db.Set<User>().CountAsync(),

            Companies = await _db.Set<Company>().CountAsync(),

            ActiveJobs = await _db.Set<Job>()
                .CountAsync(x => x.Status == JobStatus.Published),

            Applications = await _db.Set<JobApplication>().CountAsync(),

            PendingVerifications = await _db.Set<CompanyVerification>()
                .CountAsync(x => x.Status == VerificationStatus.Pending)
        };
    }

    public async Task<CompanyReportDto> GetCompanyReportAsync(
        Guid userId,
        Guid companyId)
    {
        var company = await _db.Set<Company>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == companyId);

        if (company is null)
        {
            throw new NotFoundException("Company not found.");
        }

        if (company.CreatedByUserId != userId)
        {
            throw new UnauthorizedException(
                "You do not own this company.");
        }

        var jobs = _db.Set<Job>()
            .Where(x => x.CompanyId == companyId);

        var applications = _db.Set<JobApplication>()
            .Where(x => x.Vacancy.CompanyId == companyId);

        return new CompanyReportDto
        {
            CompanyId = companyId,

            Jobs = await jobs.CountAsync(),

            ActiveJobs = await jobs
                .CountAsync(x => x.Status == JobStatus.Published),

            Applications = await applications.CountAsync(),

            Shortlisted = await applications
                .CountAsync(x => x.Status == "Shortlisted"),

            Rejected = await applications
                .CountAsync(x => x.Status == "Rejected")
        };
    }
}