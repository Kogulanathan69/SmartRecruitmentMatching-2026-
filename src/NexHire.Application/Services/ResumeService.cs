using System.Net;
using NexHire.Application.Common.Exceptions;
using NexHire.Application.DTOs.Resume;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Validators.Resume;
using NexHire.Domain.Entities;

namespace NexHire.Application.Services;

public class ResumeService : IResumeService
{
    private readonly IJobSeekerRepository _jobSeekerRepository;
    private readonly IResumeRepository _resumeRepository;
    private readonly IResumeFileStorage _fileStorage;

    public ResumeService(
        IJobSeekerRepository jobSeekerRepository,
        IResumeRepository resumeRepository,
        IResumeFileStorage fileStorage)
    {
        _jobSeekerRepository = jobSeekerRepository;
        _resumeRepository = resumeRepository;
        _fileStorage = fileStorage;
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

    public async Task<ResumeResponseDto> UploadAsync(
        Guid userId,
        string originalFileName,
        byte[] content,
        string? resumeName,
        bool isPrimary,
        CancellationToken cancellationToken = default)
    {
        var profile = await GetDetailedProfileAsync(userId);
        var extension = ResumeFileValidator.ValidateAndGetExtension(originalFileName, content);

        var defaultName = Path.GetFileNameWithoutExtension(Path.GetFileName(originalFileName));
        var cleanName = string.IsNullOrWhiteSpace(resumeName) ? defaultName.Trim() : resumeName.Trim();

        if (string.IsNullOrWhiteSpace(cleanName))
            throw new ValidationException("Resume name is required.");

        if (cleanName.Length > 120)
            throw new ValidationException("Resume name cannot exceed 120 characters.");

        if (profile.Resumes.Any(r => r.ResumeName.Equals(cleanName, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessRuleException("A resume with this name already exists.");

        if (isPrimary)
        {
            foreach (var existing in profile.Resumes)
                existing.IsPrimary = false;
        }

        var resume = new Resume
        {
            Id = Guid.NewGuid(),
            JobSeekerProfileId = profile.Id,
            ResumeName = cleanName,
            IsPrimary = isPrimary || profile.Resumes.Count == 0,
            IsGenerated = false,
            CreatedAt = DateTime.UtcNow,
            UploadedAt = DateTime.UtcNow
        };

        string? storageKey = null;

        try
        {
            storageKey = await _fileStorage.SaveAsync(userId, extension, content, cancellationToken);
            resume.FileName = storageKey;
            resume.FileUrl = $"/api/resumes/{resume.Id}/file";

            await _resumeRepository.AddAsync(resume);
            ApplyCompleteness(resume, profile);
            await _resumeRepository.SaveChangesAsync();
            return Map(resume);
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(storageKey))
                await _fileStorage.DeleteAsync(storageKey, cancellationToken);

            throw;
        }
    }

    public async Task<ResumeFileDownloadDto> DownloadUploadedAsync(
        Guid userId,
        Guid resumeId,
        CancellationToken cancellationToken = default)
    {
        var resume = await GetOwnedResumeAsync(userId, resumeId);

        if (!IsUploadedFile(resume))
            throw new BusinessRuleException("This resume does not contain an uploaded CV file.");

        byte[] content;
        try
        {
            content = await _fileStorage.ReadAsync(resume.FileName, cancellationToken);
        }
        catch (FileNotFoundException)
        {
            throw new NotFoundException("Uploaded CV file was not found.");
        }

        var extension = Path.GetExtension(resume.FileName);

        return new ResumeFileDownloadDto
        {
            Content = content,
            ContentType = ResumeFileValidator.GetContentType(extension),
            DownloadName = $"{SafeFileName(resume.ResumeName)}{extension}"
        };
    }

    public async Task<ResumeResponseDto> CreateAsync(Guid userId, CreateResumeDto dto)
    {
        ValidateDto(new CreateResumeValidator(), dto);
        var profile = await GetDetailedProfileAsync(userId);

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

        await _resumeRepository.AddAsync(resume);
        ApplyCompleteness(resume, profile);
        await _resumeRepository.SaveChangesAsync();
        return Map(resume);
    }

    public async Task<ResumeResponseDto> UpdateAsync(Guid userId, Guid resumeId, UpdateResumeDto dto)
    {
        ValidateDto(new UpdateResumeValidator(), dto);
        var profile = await GetDetailedProfileAsync(userId);
        var resume = profile.Resumes.FirstOrDefault(r => r.Id == resumeId)
            ?? throw new NotFoundException("Resume not found.");

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

        if (!IsUploadedFile(resume))
        {
            resume.IsGenerated = false;
            resume.GeneratedHtml = null;
            resume.FileName = string.Empty;
            resume.FileUrl = string.Empty;
        }

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

        var uploadedStorageKey = IsUploadedFile(resume) ? resume.FileName : null;

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

        if (!string.IsNullOrWhiteSpace(uploadedStorageKey))
        {
            try
            {
                await _fileStorage.DeleteAsync(uploadedStorageKey);
            }
            catch (IOException)
            {
                // Database deletion remains successful; stale local file can be cleaned later.
            }
        }
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

        if (IsUploadedFile(resume))
            throw new BusinessRuleException("Uploaded CVs cannot be regenerated by the CV Builder. Create a builder CV instead.");

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
        var templateCode = NormalizeTemplateCode(resume.ResumeTemplate?.Code);
        var templateCss = TemplateCss(templateCode);
        var skills = Items(profile.CandidateSkills, x => $"<span class='tag'>{E(x.Skill?.Name)}</span>");
        var education = Items(profile.Educations.OrderByDescending(x => x.StartDate), x =>
            $"<div class='item'><b>{E(x.Degree)}</b> - {E(x.Institution)}<br><small>{x.StartDate:yyyy} - {(x.EndDate.HasValue ? x.EndDate.Value.ToString("yyyy") : "Present")}</small></div>");
        var experience = Items(profile.Experiences.OrderByDescending(x => x.StartDate), x =>
            $"<div class='item'><b>{E(x.JobTitle)}</b> - {E(x.CompanyName)}<br><small>{x.StartDate:MMM yyyy} - {(x.IsCurrent ? "Present" : x.EndDate?.ToString("MMM yyyy"))}</small><p>{E(x.Description)}</p></div>");
        var projects = Items(profile.Projects, x =>
            $"<div class='item'><b>{E(x.Title)}</b><p>{E(x.Description)}</p><small>{E(x.TechStack)}</small></div>");
        var certifications = Items(profile.Certifications, x => $"<li>{E(x.Name)} - {E(x.IssuingOrganization)}</li>");

        var location = string.Join(", ", new[] { profile.City, profile.Country }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => E(value)));
        var contact = string.Join(" &middot; ", new[] { E(user?.Email), E(user?.PhoneNumber), location }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        var links = Items(new[] { resume.LinkedInUrl, resume.GitHubUrl, resume.PortfolioUrl }
                .Where(value => !string.IsNullOrWhiteSpace(value)),
            value => $"<div>{E(value)}</div>");

        return $@"<!doctype html>
<html lang='en'>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width,initial-scale=1'>
<title>{E(resume.ResumeName)}</title>
<style>
*{{box-sizing:border-box}}
body{{max-width:900px;margin:0 auto;padding:42px;font-family:Arial,sans-serif;color:#172033;line-height:1.5;background:#fff}}
.resume-header{{padding-bottom:20px;border-bottom:3px solid var(--accent)}}
h1{{margin:0;color:var(--heading);font-size:34px;line-height:1.1}}
.headline{{margin-top:7px;color:var(--accent);font-size:16px;font-weight:700}}
h2{{margin:26px 0 11px;padding-bottom:6px;border-bottom:1px solid var(--line);color:var(--heading);font-size:17px}}
.muted{{margin-top:7px;color:#667085;font-size:13px}}
.tag{{display:inline-block;margin:3px;padding:5px 9px;border-radius:14px;background:var(--soft);color:var(--heading);font-size:13px}}
.item{{margin:12px 0}}
.item p{{margin:5px 0}}
.links div{{word-break:break-all}}
{templateCss}
@media(max-width:640px){{body{{padding:24px}}h1{{font-size:28px}}}}
@media print{{body{{max-width:none;margin:0;padding:14mm}}}}
</style>
</head>
<body data-template='{E(templateCode)}'>
<header class='resume-header'>
<h1>{E(user?.FirstName)} {E(user?.LastName)}</h1>
<div class='headline'>{E(profile.Headline)}</div>
<div class='muted'>{contact}</div>
</header>
<h2>Career Objective</h2><p>{E(resume.CareerObjective)}</p>
<h2>Skills</h2><div>{skills}</div>
<h2>Education</h2>{education}
<h2>Experience</h2>{experience}
<h2>Projects</h2>{projects}
<h2>Certifications</h2><ul>{certifications}</ul>
<h2>Languages</h2><p>{E(resume.Languages)}</p>
<h2>Links</h2><div class='links'>{links}</div>
</body>
</html>";
    }

    private static string NormalizeTemplateCode(string? code) =>
        code?.Trim().ToLowerInvariant() switch
        {
            "modern" => "modern",
            "minimal" => "minimal",
            "executive" => "executive",
            "technical" => "technical",
            _ => "classic"
        };

    private static string TemplateCss(string code) => code switch
    {
        "modern" => ":root{--accent:#146c4b;--heading:#10291f;--line:#b9d4c8;--soft:#e7f2ed}.resume-header{padding-left:18px;border-left:7px solid var(--accent);border-bottom-width:1px}",
        "minimal" => ":root{--accent:#222;--heading:#111;--line:#d8d8d8;--soft:#f2f2f2}body{font-family:Helvetica,Arial,sans-serif}.resume-header{border-bottom-width:1px}h1{font-weight:500;letter-spacing:-.03em}h2{text-transform:uppercase;letter-spacing:.08em;font-size:13px}",
        "executive" => ":root{--accent:#8a6a20;--heading:#1f2937;--line:#d9cfb7;--soft:#f4efe2}body{font-family:Georgia,'Times New Roman',serif}.resume-header{text-align:center}h1{font-size:38px}.headline{color:#6f551a}h2{letter-spacing:.03em}",
        "technical" => ":root{--accent:#275d8c;--heading:#16344f;--line:#b8cad9;--soft:#e8f0f7}.resume-header{border-bottom-style:dashed}h1,h2{font-family:'Segoe UI',Arial,sans-serif}.tag{border:1px solid var(--line);border-radius:4px;font-family:Consolas,monospace}",
        _ => ":root{--accent:#284b7a;--heading:#172b4d;--line:#cbd5e1;--soft:#eef3f8}body{font-family:Georgia,'Times New Roman',serif}.muted,.tag,.item small{font-family:Arial,sans-serif}"
    };

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

    private static void ValidateDto<T>(FluentValidation.IValidator<T> validator, T dto)
    {
        var result = validator.Validate(dto);
        if (result.IsValid)
            return;

        var message = string.Join(
            " ",
            result.Errors
                .Select(error => error.ErrorMessage)
                .Distinct(StringComparer.Ordinal));
        throw new ValidationException(message);
    }

    private static bool IsUploadedFile(Resume resume) =>
        !string.IsNullOrWhiteSpace(resume.FileName) &&
        !string.IsNullOrWhiteSpace(resume.FileUrl) &&
        resume.FileUrl.EndsWith("/file", StringComparison.OrdinalIgnoreCase);

    private static string SafeFileName(string value) =>
        string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
