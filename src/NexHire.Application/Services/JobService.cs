using NexHire.Application.Common.Exceptions;
using NexHire.Application.DTOs.Job;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;

namespace NexHire.Application.Services;

public sealed class JobService : IJobService
{
    private readonly IJobRepository _jobs;

    public JobService(IJobRepository jobs)
    {
        _jobs = jobs;
    }

    public async Task<IReadOnlyList<JobResponseDto>> SearchAsync(
        JobSearchDto search,
        CancellationToken cancellationToken = default)
    {
        search ??= new JobSearchDto();

        var jobs =
            await _jobs.SearchAsync(
                search,
                cancellationToken);

        return jobs
            .Select(Map)
            .ToList();
    }

    public async Task<JobResponseDto> GetByIdAsync(
        Guid jobId,
        Guid userId,
        bool isEmployer,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var job =
            await _jobs.GetByIdAsync(
                jobId,
                cancellationToken)
            ?? throw new NotFoundException(
                "Job vacancy was not found.");

        var visibleToPublicUser =
            job.Status == JobStatus.Published &&
            (!job.ClosingDate.HasValue ||
             job.ClosingDate.Value >= DateTime.UtcNow);

        var ownedByEmployer =
            isEmployer &&
            job.Company is not null &&
            job.Company.CreatedByUserId == userId;

        if (!visibleToPublicUser &&
            !ownedByEmployer &&
            !isAdmin)
        {
            throw new NotFoundException(
                "Job vacancy was not found.");
        }

        return Map(job);
    }

    public async Task<IReadOnlyList<JobResponseDto>> GetMineAsync(
        Guid employerUserId,
        CancellationToken cancellationToken = default)
    {
        var jobs =
            await _jobs.GetByEmployerAsync(
                employerUserId,
                cancellationToken);

        return jobs
            .Select(Map)
            .ToList();
    }

    public async Task<JobResponseDto> CreateAsync(
        Guid employerUserId,
        CreateJobDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        ValidateCreate(dto);

        var ownsCompany =
            await _jobs.EmployerOwnsCompanyAsync(
                dto.CompanyId,
                employerUserId,
                cancellationToken);

        if (!ownsCompany)
        {
            throw new UnauthorizedException(
                "You do not own the selected company.");
        }

        var now = DateTime.UtcNow;

        var job = new Job
        {
            Id = Guid.NewGuid(),
            CompanyId = dto.CompanyId,
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Responsibilities =
                dto.Responsibilities?.Trim()
                ?? string.Empty,
            EducationRequirement =
                dto.EducationRequirement?.Trim()
                ?? string.Empty,
            MinimumEducationLevel =
                dto.MinimumEducationLevel,
            RequiredCertifications =
                CleanOptional(dto.RequiredCertifications),
            TargetProjectCount =
                dto.TargetProjectCount,
            Status = JobStatus.Published,
            EmploymentType =
                dto.EmploymentType.Trim(),
            LocationCity =
                CleanOptional(dto.LocationCity),
            LocationCountry =
                CleanOptional(dto.LocationCountry),
            IsRemote = dto.IsRemote,
            IsHybrid = dto.IsHybrid,
            SalaryMin = dto.SalaryMin,
            SalaryMax = dto.SalaryMax,
            Currency =
                dto.Currency.Trim().ToUpperInvariant(),
            ExperienceMinYears =
                dto.ExperienceMinYears,
            ExperienceMaxYears =
                dto.ExperienceMaxYears,
            VacancyCount =
                dto.VacancyCount,
            PostedAt = now,
            ClosingDate =
                dto.ClosingDate,
            CreatedAtUtc = now
        };

        await ReplaceSkillsAsync(
            job,
            dto.RequiredSkills,
            dto.PreferredSkills,
            cancellationToken);

        await _jobs.AddAsync(
            job,
            cancellationToken);

        await _jobs.SaveChangesAsync(
            cancellationToken);

        var saved =
            await _jobs.GetByIdAsync(
                job.Id,
                cancellationToken);

        return Map(saved ?? job);
    }

    public async Task<JobResponseDto> UpdateAsync(
        Guid jobId,
        Guid employerUserId,
        UpdateJobDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var job =
            await _jobs.GetOwnedByIdAsync(
                jobId,
                employerUserId,
                cancellationToken)
            ?? throw new UnauthorizedException(
                "You do not own this job vacancy.");

        if (job.Status == JobStatus.Closed)
        {
            throw new BusinessRuleException(
                "A closed vacancy cannot be edited.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            job.Title = dto.Title.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            job.Description = dto.Description.Trim();
        }

        if (dto.Responsibilities is not null)
        {
            job.Responsibilities =
                dto.Responsibilities.Trim();
        }

        if (dto.EducationRequirement is not null)
        {
            job.EducationRequirement =
                dto.EducationRequirement.Trim();
        }

        if (dto.MinimumEducationLevel.HasValue)
        {
            job.MinimumEducationLevel =
                dto.MinimumEducationLevel.Value;
        }

        if (dto.RequiredCertifications is not null)
        {
            job.RequiredCertifications =
                CleanOptional(
                    dto.RequiredCertifications);
        }

        if (dto.TargetProjectCount.HasValue)
        {
            job.TargetProjectCount =
                dto.TargetProjectCount.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.EmploymentType))
        {
            job.EmploymentType =
                dto.EmploymentType.Trim();
        }

        if (dto.LocationCity is not null)
        {
            job.LocationCity =
                CleanOptional(dto.LocationCity);
        }

        if (dto.LocationCountry is not null)
        {
            job.LocationCountry =
                CleanOptional(dto.LocationCountry);
        }

        if (dto.IsRemote.HasValue)
        {
            job.IsRemote =
                dto.IsRemote.Value;
        }

        if (dto.IsHybrid.HasValue)
        {
            job.IsHybrid =
                dto.IsHybrid.Value;
        }

        if (dto.SalaryMin.HasValue)
        {
            job.SalaryMin =
                dto.SalaryMin.Value;
        }

        if (dto.SalaryMax.HasValue)
        {
            job.SalaryMax =
                dto.SalaryMax.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Currency))
        {
            job.Currency =
                dto.Currency.Trim()
                    .ToUpperInvariant();
        }

        if (dto.ExperienceMinYears.HasValue)
        {
            job.ExperienceMinYears =
                dto.ExperienceMinYears.Value;
        }

        if (dto.ExperienceMaxYears.HasValue)
        {
            job.ExperienceMaxYears =
                dto.ExperienceMaxYears.Value;
        }

        if (dto.VacancyCount.HasValue)
        {
            job.VacancyCount =
                dto.VacancyCount.Value;
        }

        if (dto.ClosingDate.HasValue)
        {
            job.ClosingDate =
                dto.ClosingDate.Value;
        }

        ValidateJobValues(
            job.Title,
            job.Description,
            job.EmploymentType,
            job.Currency,
            job.MinimumEducationLevel,
            job.TargetProjectCount,
            job.ExperienceMinYears,
            job.ExperienceMaxYears,
            job.SalaryMin,
            job.SalaryMax,
            job.VacancyCount,
            job.ClosingDate);

        if (dto.RequiredSkills is not null ||
            dto.PreferredSkills is not null)
        {
            var required =
                dto.RequiredSkills ??
                job.RequiredSkills
                    .Select(link =>
                        new JobSkillRequirementDto
                        {
                            SkillName =
                                link.Skill.Name,
                            MinimumProficiencyLevel =
                                link.MinimumProficiencyLevel
                        })
                    .ToList();

            var preferred =
                dto.PreferredSkills ??
                job.PreferredSkills
                    .Select(link =>
                        new JobSkillRequirementDto
                        {
                            SkillName =
                                link.Skill.Name,
                            MinimumProficiencyLevel =
                                link.MinimumProficiencyLevel
                        })
                    .ToList();

            await ReplaceSkillsAsync(
                job,
                required,
                preferred,
                cancellationToken);
        }

        job.UpdatedAtUtc =
            DateTime.UtcNow;

        await _jobs.SaveChangesAsync(
            cancellationToken);

        var saved =
            await _jobs.GetByIdAsync(
                job.Id,
                cancellationToken);

        return Map(saved ?? job);
    }

    public async Task<JobResponseDto> CloseAsync(
        Guid jobId,
        Guid employerUserId,
        CancellationToken cancellationToken = default)
    {
        var job =
            await _jobs.GetOwnedByIdAsync(
                jobId,
                employerUserId,
                cancellationToken)
            ?? throw new UnauthorizedException(
                "You do not own this job vacancy.");

        if (job.Status != JobStatus.Closed)
        {
            var now = DateTime.UtcNow;

            job.Status =
                JobStatus.Closed;

            job.ClosedAtUtc =
                now;

            job.UpdatedAtUtc =
                now;

            _jobs.Update(job);

            await _jobs.SaveChangesAsync(
                cancellationToken);
        }

        var saved =
            await _jobs.GetByIdAsync(
                job.Id,
                cancellationToken);

        return Map(saved ?? job);
    }

    private async Task ReplaceSkillsAsync(
        Job job,
        IReadOnlyCollection<JobSkillRequirementDto> required,
        IReadOnlyCollection<JobSkillRequirementDto> preferred,
        CancellationToken cancellationToken)
    {
        ValidateSkills(
            required,
            preferred);

        job.RequiredSkills.Clear();
        job.PreferredSkills.Clear();

        foreach (var item in required)
        {
            var skill =
                await GetOrCreateSkillAsync(
                    item.SkillName,
                    cancellationToken);

            job.RequiredSkills.Add(
                new JobRequiredSkill
                {
                    Id = Guid.Empty,
                    JobId = job.Id,
                    SkillId = skill.Id,
                    Skill = skill,
                    MinimumProficiencyLevel =
                        item.MinimumProficiencyLevel
                });
        }

        foreach (var item in preferred)
        {
            var skill =
                await GetOrCreateSkillAsync(
                    item.SkillName,
                    cancellationToken);

            job.PreferredSkills.Add(
                new JobPreferredSkill
                {
                    Id = Guid.Empty,
                    JobId = job.Id,
                    SkillId = skill.Id,
                    Skill = skill,
                    MinimumProficiencyLevel =
                        item.MinimumProficiencyLevel
                });
        }
    }

    private async Task<Skill> GetOrCreateSkillAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var cleaned = name.Trim();

        var existing =
            await _jobs.GetSkillByNameAsync(
                cleaned,
                cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = cleaned
        };

        await _jobs.AddSkillAsync(
            skill,
            cancellationToken);

        return skill;
    }

    private static void ValidateCreate(
        CreateJobDto dto)
    {
        if (dto.CompanyId == Guid.Empty)
        {
            throw new BusinessRuleException(
                "CompanyId is required.");
        }

        ValidateJobValues(
            dto.Title,
            dto.Description,
            dto.EmploymentType,
            dto.Currency,
            dto.MinimumEducationLevel,
            dto.TargetProjectCount,
            dto.ExperienceMinYears,
            dto.ExperienceMaxYears,
            dto.SalaryMin,
            dto.SalaryMax,
            dto.VacancyCount,
            dto.ClosingDate);

        ValidateSkills(
            dto.RequiredSkills,
            dto.PreferredSkills);
    }

    private static void ValidateJobValues(
        string title,
        string description,
        string employmentType,
        string currency,
        int minimumEducationLevel,
        int targetProjectCount,
        int experienceMinYears,
        int experienceMaxYears,
        decimal? salaryMin,
        decimal? salaryMax,
        int vacancyCount,
        DateTime? closingDate)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new BusinessRuleException(
                "Job title is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new BusinessRuleException(
                "Job description is required.");
        }

        if (string.IsNullOrWhiteSpace(employmentType))
        {
            throw new BusinessRuleException(
                "Employment type is required.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new BusinessRuleException(
                "Currency is required.");
        }

        if (minimumEducationLevel < 0)
        {
            throw new BusinessRuleException(
                "Minimum education level cannot be negative.");
        }

        if (targetProjectCount < 0)
        {
            throw new BusinessRuleException(
                "Target project count cannot be negative.");
        }

        if (experienceMinYears < 0 ||
            experienceMaxYears < experienceMinYears)
        {
            throw new BusinessRuleException(
                "Experience range is invalid.");
        }

        if (salaryMin.HasValue &&
            salaryMin.Value < 0)
        {
            throw new BusinessRuleException(
                "Minimum salary cannot be negative.");
        }

        if (salaryMax.HasValue &&
            salaryMax.Value < 0)
        {
            throw new BusinessRuleException(
                "Maximum salary cannot be negative.");
        }

        if (salaryMin.HasValue &&
            salaryMax.HasValue &&
            salaryMax.Value < salaryMin.Value)
        {
            throw new BusinessRuleException(
                "Maximum salary cannot be lower than minimum salary.");
        }

        if (vacancyCount <= 0)
        {
            throw new BusinessRuleException(
                "Vacancy count must be at least 1.");
        }

        if (closingDate.HasValue &&
            closingDate.Value <= DateTime.UtcNow)
        {
            throw new BusinessRuleException(
                "Closing date must be in the future.");
        }
    }

    private static void ValidateSkills(
        IReadOnlyCollection<JobSkillRequirementDto> required,
        IReadOnlyCollection<JobSkillRequirementDto> preferred)
    {
        var requiredNames =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var item in required)
        {
            ValidateSkill(item);

            if (!requiredNames.Add(
                    item.SkillName.Trim()))
            {
                throw new BusinessRuleException(
                    $"Required skill '{item.SkillName}' is duplicated.");
            }
        }

        var preferredNames =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var item in preferred)
        {
            ValidateSkill(item);

            var cleaned =
                item.SkillName.Trim();

            if (requiredNames.Contains(cleaned))
            {
                throw new BusinessRuleException(
                    $"Skill '{cleaned}' cannot be both required and preferred.");
            }

            if (!preferredNames.Add(cleaned))
            {
                throw new BusinessRuleException(
                    $"Preferred skill '{cleaned}' is duplicated.");
            }
        }
    }

    private static void ValidateSkill(
        JobSkillRequirementDto item)
    {
        if (string.IsNullOrWhiteSpace(
                item.SkillName))
        {
            throw new BusinessRuleException(
                "Skill name is required.");
        }

        if (item.MinimumProficiencyLevel
            is < 1 or > 5)
        {
            throw new BusinessRuleException(
                "Skill proficiency must be between 1 and 5.");
        }
    }

    private static string? CleanOptional(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static JobResponseDto Map(
        Job job)
    {
        return new JobResponseDto
        {
            Id = job.Id,
            CompanyId = job.CompanyId,
            CompanyName =
                job.Company?.Name ??
                string.Empty,
            Title = job.Title,
            Description = job.Description,
            Responsibilities =
                job.Responsibilities,
            EducationRequirement =
                job.EducationRequirement,
            MinimumEducationLevel =
                job.MinimumEducationLevel,
            RequiredCertifications =
                job.RequiredCertifications,
            TargetProjectCount =
                job.TargetProjectCount,
            Status =
                job.Status.ToString(),
            EmploymentType =
                job.EmploymentType,
            LocationCity =
                job.LocationCity,
            LocationCountry =
                job.LocationCountry,
            IsRemote =
                job.IsRemote,
            IsHybrid =
                job.IsHybrid,
            SalaryMin =
                job.SalaryMin,
            SalaryMax =
                job.SalaryMax,
            Currency =
                job.Currency,
            ExperienceMinYears =
                job.ExperienceMinYears,
            ExperienceMaxYears =
                job.ExperienceMaxYears,
            VacancyCount =
                job.VacancyCount,
            PostedAt =
                job.PostedAt,
            ClosingDate =
                job.ClosingDate,
            CreatedAtUtc =
                job.CreatedAtUtc,
            UpdatedAtUtc =
                job.UpdatedAtUtc,
            ClosedAtUtc =
                job.ClosedAtUtc,
            RequiredSkills =
                job.RequiredSkills
                    .Select(link =>
                        new JobSkillRequirementDto
                        {
                            SkillName =
                                link.Skill.Name,
                            MinimumProficiencyLevel =
                                link.MinimumProficiencyLevel
                        })
                    .ToList(),
            PreferredSkills =
                job.PreferredSkills
                    .Select(link =>
                        new JobSkillRequirementDto
                        {
                            SkillName =
                                link.Skill.Name,
                            MinimumProficiencyLevel =
                                link.MinimumProficiencyLevel
                        })
                    .ToList()
        };
    }
}
