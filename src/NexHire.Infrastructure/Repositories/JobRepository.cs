using Microsoft.EntityFrameworkCore;
using NexHire.Application.DTOs.Job;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Repositories;

public sealed class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;

    public JobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Job?> GetByIdAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .AsNoTracking()
            .Include(job => job.Company)
            .Include(job => job.RequiredSkills)
                .ThenInclude(link => link.Skill)
            .Include(job => job.PreferredSkills)
                .ThenInclude(link => link.Skill)
            .AsSplitQuery()
            .SingleOrDefaultAsync(
                job => job.Id == jobId,
                cancellationToken);
    }

    public async Task<Job?> GetOwnedByIdAsync(
        Guid jobId,
        Guid employerUserId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Include(job => job.Company)
            .Include(job => job.RequiredSkills)
                .ThenInclude(link => link.Skill)
            .Include(job => job.PreferredSkills)
                .ThenInclude(link => link.Skill)
            .AsSplitQuery()
            .SingleOrDefaultAsync(
                job =>
                    job.Id == jobId &&
                    job.Company.CreatedByUserId == employerUserId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Job>> SearchAsync(
        JobSearchDto search,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var query = _context.Jobs
            .AsNoTracking()
            .Include(job => job.Company)
            .Include(job => job.RequiredSkills)
                .ThenInclude(link => link.Skill)
            .Include(job => job.PreferredSkills)
                .ThenInclude(link => link.Skill)
            .AsSplitQuery()
            .Where(job =>
                job.Status == JobStatus.Published &&
                (!job.ClosingDate.HasValue ||
                 job.ClosingDate.Value >= now));

        if (!string.IsNullOrWhiteSpace(search.Query))
        {
            var text = search.Query.Trim();

            query = query.Where(job =>
                job.Title.Contains(text) ||
                job.Description.Contains(text) ||
                job.Company.Name.Contains(text));
        }

        if (!string.IsNullOrWhiteSpace(search.Location))
        {
            var location = search.Location.Trim();

            query = query.Where(job =>
                job.LocationCity != null &&
                job.LocationCity.Contains(location));
        }

        if (!string.IsNullOrWhiteSpace(search.Country))
        {
            var country = search.Country.Trim();

            query = query.Where(job =>
                job.LocationCountry != null &&
                job.LocationCountry.Contains(country));
        }

        if (!string.IsNullOrWhiteSpace(search.EmploymentType))
        {
            var employmentType = search.EmploymentType.Trim();

            query = query.Where(job =>
                job.EmploymentType == employmentType);
        }

        if (!string.IsNullOrWhiteSpace(search.Skill))
        {
            var skill = search.Skill.Trim();

            query = query.Where(job =>
                job.RequiredSkills.Any(link =>
                    link.Skill.Name.Contains(skill)) ||
                job.PreferredSkills.Any(link =>
                    link.Skill.Name.Contains(skill)));
        }

        if (search.IsRemote.HasValue)
        {
            query = query.Where(job =>
                job.IsRemote == search.IsRemote.Value);
        }

        if (search.ExperienceYears.HasValue)
        {
            var years = search.ExperienceYears.Value;

            query = query.Where(job =>
                job.ExperienceMinYears <= years);
        }

        var page = Math.Max(1, search.Page);
        var pageSize = Math.Clamp(search.PageSize, 1, 50);

        return await query
            .OrderByDescending(job =>
                job.PostedAt ?? job.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Job>> GetByEmployerAsync(
        Guid employerUserId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .AsNoTracking()
            .Include(job => job.Company)
            .Include(job => job.RequiredSkills)
                .ThenInclude(link => link.Skill)
            .Include(job => job.PreferredSkills)
                .ThenInclude(link => link.Skill)
            .AsSplitQuery()
            .Where(job =>
                job.Company.CreatedByUserId == employerUserId)
            .OrderByDescending(job => job.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> EmployerOwnsCompanyAsync(
        Guid companyId,
        Guid employerUserId,
        CancellationToken cancellationToken = default)
    {
        return _context.Companies
            .AsNoTracking()
            .AnyAsync(
                company =>
                    company.Id == companyId &&
                    company.CreatedByUserId == employerUserId,
                cancellationToken);
    }

    public Task<Skill?> GetSkillByNameAsync(
        string skillName,
        CancellationToken cancellationToken = default)
    {
        var cleaned = skillName.Trim();

        return _context.Skills
            .FirstOrDefaultAsync(
                skill => skill.Name == cleaned,
                cancellationToken);
    }

    public Task AddSkillAsync(
        Skill skill,
        CancellationToken cancellationToken = default)
    {
        return _context.Skills
            .AddAsync(skill, cancellationToken)
            .AsTask();
    }

    public Task AddAsync(
        Job job,
        CancellationToken cancellationToken = default)
    {
        return _context.Jobs
            .AddAsync(job, cancellationToken)
            .AsTask();
    }

    public void Update(Job job)
    {
        _context.Jobs.Update(job);
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}