using System.Net;
using NexHire.Application.Common.Exceptions;
using NexHire.Application.DTOs.Resume;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Domain.Entities;

namespace NexHire.Application.Services;

public class ResumeService : IResumeService
{
    private readonly IJobSeekerRepository _jobSeekerRepository;
    private readonly IResumeRepository _resumeRepository;

    public ResumeService(IJobSeekerRepository jobSeekerRepository, IResumeRepository resumeRepository)
    {
        _jobSeekerRepository = jobSeekerRepository;
        _resumeRepository = resumeRepository;
    }

    public async Task<IReadOnlyList<ResumeResponseDto>> GetMyResumesAsync(Guid userId)
    {
        var profile = await GetDetailedProfileAsync(userId);
        return profile.Resumes
            .OrderByDescending(r => r.IsPrimary)
            .ThenByDescending(r => r.CreatedAt)
            .Select(Map)
            .ToList();
    }

    public async Task<ResumeResponseDto> GetByIdAsync(Guid userId, Guid resumeId) =>
        Map(await GetOwnedResumeAsync(userId, resumeId));

    public async Task<ResumeResponseDto> CreateAsync(Guid userId, CreateResumeDto dto)
    {
        var profile = await GetDetailedProfileAsync(userId);
        ValidateInput(dto.ResumeName, dto.CareerObjective, dto.Languages);

        var cleanName = dto.ResumeName.Trim();
        if (profile.Resumes.Any(r => r.ResumeName.Equals(cleanName, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessRuleException("A resume with this name already exists.");

        var template = await ValidateTemplateAsync(dto.TemplateId);

        if (dto.IsPrimary)
        {
            foreach (var existing in profile.Resumes)
                existing.IsPrimary = false;
        }

        var resume = new Resume
        {
            Id = Guid.NewGuid(),
            JobSeekerProfileId = profile.Id,
            ResumeTemplateId = dto.TemplateId,
            ResumeTemplate = template,
            ResumeName = cleanName,
            CareerObjective = Clean(dto.CareerObjective),
            Languages = Join(dto.Languages),
            LinkedInUrl = Clean(dto.LinkedInUrl),
            GitHubUrl = Clean(dto.GitHubUrl),
            PortfolioUrl = Clean(dto.PortfolioUrl),
            IsPrimary = dto.IsPrimary || profile.Resumes.Count == 0,
            CreatedAt = DateTime.UtcNow,
            UploadedAt = DateTime.UtcNow
        };

        profile.Resumes.Add(resume);
        ApplyCompleteness(resume, profile);
        await _resumeRepository.SaveChangesAsync();
        return Map(resume);
    }

    public async Task<ResumeResponseDto> UpdateAsync(Guid userId, Guid resumeId, UpdateResumeDto dto)
    {
        var profile = await GetDetailedProfileAsync(userId);
        var resume = profile.Resumes.FirstOrDefault(r => r.Id == resumeId)
            ?? throw new NotFoundException("Resume not found.");

        ValidateInput(dto.ResumeName, dto.CareerObjective, dto.Languages);
        var cleanName = dto.ResumeName.Trim();
        if (profile.Resumes.Any(r => r.Id != resumeId && r.ResumeName.Equals(cleanName, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessRuleException("A resume with this name already exists.");

        var template = await ValidateTemplateAsync(dto.TemplateId);

        if (dto.IsPrimary)
        {
            foreach (var existing in profile.Resumes)
                existing.IsPrimary = existing.Id == resumeId;
        }

        resume.ResumeName = cleanName;
        resume.ResumeTemplateId = dto.TemplateId;
        resume.ResumeTemplate = template;
        resume.CareerObjective = Clean(dto.CareerObjective);
        resume.Languages = Join(dto.Languages);
        resume.LinkedInUrl = Clean(dto.LinkedInUrl);
        resume.GitHubUrl = Clean(dto.GitHubUrl);
        resume.PortfolioUrl = Clean(dto.PortfolioUrl);
        resume.IsPrimary = dto.IsPrimary || resume.IsPrimary;
        resume.IsGenerated = false;
        resume.GeneratedHtml = null;
        resume.FileName = string.Empty;
        resume.FileUrl = string.Empty;
        resume.UpdatedAt = DateTime.UtcNow;

        ApplyCompleteness(resume, profile);
        await _resumeRepository.SaveChangesAsync();
        return Map(resume);
    }

    public async Task DeleteAsync(Guid userId, Guid resumeId)
    {
        var profile = await GetDetailedProfileAsync(userId);
        var resume = profile.Resumes.FirstOrDefault(r => r.Id == resumeId)
            ?? throw new NotFoundException("Resume not found.");

        _resumeRepository.Remove(resume);

        if (resume.IsPrimary)
        {
            var replacement = profile.Resumes
                .Where(r => r.Id != resumeId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();
            if (replacement is not null)
                replacement.IsPrimary = true;
        }

        await _resumeRepository.SaveChangesAsync();
    }

    public async Task<ResumeCompletenessDto> GetCompletenessAsync(Guid userId, Guid? resumeId = null)
    {
        var profile = await GetDetailedProfileAsync(userId);
        var resume = resumeId.HasValue
            ? profile.Resumes.FirstOrDefault(r => r.Id == resumeId.Value)
                ?? throw new NotFoundException("Resume not found.")
            : profile.Resumes.FirstOrDefault(r => r.IsPrimary)
                ?? profile.Resumes.FirstOrDefault()
                ?? new Resume();

        return CalculateCompleteness(resume, profile);
    }

    public async Task<string> GenerateHtmlAsync(Guid userId, Guid resumeId)
    {
        var profile = await GetDetailedProfileAsync(userId);
        var resume = profile.Resumes.FirstOrDefault(r => r.Id == resumeId)
            ?? throw new NotFoundException("Resume not found.");

        var completeness = CalculateCompleteness(resume, profile);

        if (!profile.Educations.Any())
            throw new BusinessRuleException("At least one education record is required before generating a CV.");
        if (profile.CandidateSkills.Count < 3)
            throw new BusinessRuleException("At least three skills are required before generating a CV.");
        if (string.IsNullOrWhiteSpace(resume.CareerObjective) || resume.CareerObjective.Trim().Length < 50)
            throw new BusinessRuleException("Career objective must contain at least 50 characters before generating a CV.");
        if (profile.YearsOfExperience > 0 && !profile.Experiences.Any())
            throw new BusinessRuleException("Experience details are required for an experienced candidate.");

        resume.GeneratedHtml = BuildHtml(profile, resume);
        resume.IsGenerated = true;
        resume.CompletenessScore = completeness.Score;
        resume.QualityRating = completeness.Rating;
        resume.MissingSections = string.Join(",", completeness.MissingSections);
        resume.FileName = $"{SafeFileName(resume.ResumeName)}.html";
        resume.FileUrl = $"/api/resumes/{resume.Id}/download";
        resume.UpdatedAt = DateTime.UtcNow;

        await _resumeRepository.SaveChangesAsync();
        return resume.GeneratedHtml;
    }

    public async Task<string> GetPreviewHtmlAsync(Guid userId, Guid resumeId)
    {
        var resume = await GetOwnedResumeAsync(userId, resumeId);
        return resume.GeneratedHtml ?? await GenerateHtmlAsync(userId, resumeId);
    }

    public async Task<IReadOnlyList<ResumeTemplateResponseDto>> GetTemplatesAsync()
    {
        var templates = await _resumeRepository.GetActiveTemplatesAsync();
        return templates.Select(t => new ResumeTemplateResponseDto
        {
            Id = t.Id,
            Code = t.Code,
            Name = t.Name,
            Description = t.Description,
            PreviewImageUrl = t.PreviewImageUrl,
            IsAtsFriendly = t.IsAtsFriendly
        }).ToList();
    }

    private async Task<JobSeekerProfile> GetDetailedProfileAsync(Guid userId) =>
        await _jobSeekerRepository.GetByUserIdWithDetailsAsync(userId)
        ?? throw new NotFoundException("Job seeker profile not found. Create the profile first.");

    private async Task<Resume> GetOwnedResumeAsync(Guid userId, Guid resumeId)
    {
        var profile = await GetDetailedProfileAsync(userId);
        return profile.Resumes.FirstOrDefault(r => r.Id == resumeId)
            ?? throw new NotFoundException("Resume not found.");
    }

    private async Task<ResumeTemplate?> ValidateTemplateAsync(Guid? templateId)
    {
        if (!templateId.HasValue)
            return null;

        return await _resumeRepository.GetActiveTemplateByIdAsync(templateId.Value)
            ?? throw new BusinessRuleException("Selected resume template is unavailable.");
    }

    private static void ValidateInput(string name, string? objective, IEnumerable<string> languages)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleException("Resume name is required.");
        if (name.Trim().Length > 120)
            throw new BusinessRuleException("Resume name cannot exceed 120 characters.");
        if (!string.IsNullOrWhiteSpace(objective) && objective.Trim().Length > 1200)
            throw new BusinessRuleException("Career objective cannot exceed 1200 characters.");
        if (languages.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 10)
            throw new BusinessRuleException("A maximum of 10 languages is allowed.");
    }

    private static ResumeCompletenessDto CalculateCompleteness(Resume resume, JobSeekerProfile profile)
    {
        var completed = new List<string>();
        var missing = new List<string>();
        var recommendations = new List<string>();
        var score = 0;

        void Add(bool ok, string section, int points, string recommendation)
        {
            if (ok)
            {
                score += points;
                completed.Add(section);
            }
            else
            {
                missing.Add(section);
                recommendations.Add(recommendation);
            }
        }

        Add(profile.User is not null && !string.IsNullOrWhiteSpace(profile.User.FirstName)
            && !string.IsNullOrWhiteSpace(profile.User.Email) && !string.IsNullOrWhiteSpace(profile.Headline),
            "Personal Details", 10, "Complete name, email and professional headline.");
        Add(!string.IsNullOrWhiteSpace(resume.CareerObjective) && resume.CareerObjective.Trim().Length >= 50,
            "Career Objective", 10, "Add a career objective with at least 50 characters.");
        Add(profile.Educations.Any(), "Education", 15, "Add at least one education record.");

        var skillPoints = Math.Min(20, profile.CandidateSkills.Count * 7);
        score += skillPoints;
        if (profile.CandidateSkills.Count >= 3)
            completed.Add("Skills");
        else
        {
            missing.Add("Skills");
            recommendations.Add("Add at least three skills.");
        }

        Add(profile.YearsOfExperience == 0 || profile.Experiences.Any(), "Experience", 15, "Add work experience details.");
        Add(profile.Projects.Any(), "Projects", 15, "Add at least one relevant project.");
        Add(profile.Certifications.Any(), "Certifications", 5, "Add certifications if available.");
        Add(Split(resume.Languages).Any(), "Languages", 5, "Add at least one language.");
        Add(!string.IsNullOrWhiteSpace(resume.LinkedInUrl) || !string.IsNullOrWhiteSpace(resume.GitHubUrl) || !string.IsNullOrWhiteSpace(resume.PortfolioUrl),
            "Contact Links", 5, "Add LinkedIn, GitHub or portfolio link.");

        var finalScore = Math.Min(score, 100);
        return new ResumeCompletenessDto
        {
            Score = finalScore,
            Rating = Rating(finalScore),
            CompletedSections = completed,
            MissingSections = missing,
            Recommendations = recommendations
        };
    }

    private static void ApplyCompleteness(Resume resume, JobSeekerProfile profile)
    {
        var result = CalculateCompleteness(resume, profile);
        resume.CompletenessScore = result.Score;
        resume.QualityRating = result.Rating;
        resume.MissingSections = string.Join(",", result.MissingSections);
    }

    private static string BuildHtml(JobSeekerProfile profile, Resume resume)
    {
        static string E(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
        static string Items<T>(IEnumerable<T> values, Func<T, string> item) => string.Join(string.Empty, values.Select(item));

        var user = profile.User;
        var skills = Items(profile.CandidateSkills, x => $"<span class='tag'>{E(x.Skill?.Name)}</span>");
        var education = Items(profile.Educations.OrderByDescending(x => x.StartDate), x =>
            $"<div class='item'><b>{E(x.Degree)}</b> - {E(x.Institution)}<br><small>{x.StartDate:yyyy} - {(x.EndDate.HasValue ? x.EndDate.Value.ToString("yyyy") : "Present")}</small></div>");
        var experience = Items(profile.Experiences.OrderByDescending(x => x.StartDate), x =>
            $"<div class='item'><b>{E(x.JobTitle)}</b> - {E(x.CompanyName)}<br><small>{x.StartDate:MMM yyyy} - {(x.IsCurrent ? "Present" : x.EndDate?.ToString("MMM yyyy"))}</small><p>{E(x.Description)}</p></div>");
        var projects = Items(profile.Projects, x =>
            $"<div class='item'><b>{E(x.Title)}</b><p>{E(x.Description)}</p><small>{E(x.TechStack)}</small></div>");
        var certifications = Items(profile.Certifications, x => $"<li>{E(x.Name)} - {E(x.IssuingOrganization)}</li>");

        return $@"<!doctype html>
<html>
<head>
<meta charset='utf-8'>
<title>{E(resume.ResumeName)}</title>
<style>
body{{font-family:Arial,sans-serif;color:#172033;margin:40px;line-height:1.45}}
h1{{color:#0b1f3a;margin-bottom:2px}}
h2{{font-size:17px;color:#2c5ff6;border-bottom:1px solid #dfe5ee;padding-bottom:6px;margin-top:24px}}
.muted{{color:#667085}}
.tag{{display:inline-block;background:#eef3fb;padding:5px 9px;border-radius:14px;margin:3px}}
.item{{margin:10px 0}}
@media print{{body{{margin:18mm}}}}
</style>
</head>
<body>
<h1>{E(user?.FirstName)} {E(user?.LastName)}</h1>
<div class='muted'>{E(profile.Headline)} &middot; {E(user?.Email)} &middot; {E(user?.PhoneNumber)} &middot; {E(profile.City)}, {E(profile.Country)}</div>
<h2>Career Objective</h2><p>{E(resume.CareerObjective)}</p>
<h2>Skills</h2><div>{skills}</div>
<h2>Education</h2>{education}
<h2>Experience</h2>{experience}
<h2>Projects</h2>{projects}
<h2>Certifications</h2><ul>{certifications}</ul>
<h2>Languages</h2><p>{E(resume.Languages)}</p>
<h2>Links</h2><p>{E(resume.LinkedInUrl)} {E(resume.GitHubUrl)} {E(resume.PortfolioUrl)}</p>
</body>
</html>";
    }

    private static ResumeResponseDto Map(Resume resume) => new()
    {
        Id = resume.Id,
        ResumeName = resume.ResumeName,
        TemplateId = resume.ResumeTemplateId,
        TemplateName = resume.ResumeTemplate?.Name,
        CareerObjective = resume.CareerObjective,
        Languages = Split(resume.Languages),
        LinkedInUrl = resume.LinkedInUrl,
        GitHubUrl = resume.GitHubUrl,
        PortfolioUrl = resume.PortfolioUrl,
        IsPrimary = resume.IsPrimary,
        IsGenerated = resume.IsGenerated,
        CompletenessScore = resume.CompletenessScore,
        QualityRating = resume.QualityRating,
        MissingSections = Split(resume.MissingSections),
        FileUrl = string.IsNullOrWhiteSpace(resume.FileUrl) ? null : resume.FileUrl,
        CreatedAt = resume.CreatedAt,
        UpdatedAt = resume.UpdatedAt
    };

    private static string Rating(int score) => score switch
    {
        >= 90 => "Excellent",
        >= 75 => "Very Good",
        >= 60 => "Good",
        >= 40 => "Average",
        _ => "Needs Improvement"
    };

    private static string Join(IEnumerable<string> values) =>
        string.Join(",", values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase));

    private static List<string> Split(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? new List<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string SafeFileName(string value) =>
        string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
