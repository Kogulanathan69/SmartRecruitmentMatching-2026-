namespace NexHire.Application.DTOs.Job;

public sealed class JobResponseDto
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Responsibilities { get; set; } = string.Empty;

    public string EducationRequirement { get; set; } = string.Empty;

    public int MinimumEducationLevel { get; set; }

    public string? RequiredCertifications { get; set; }

    public int TargetProjectCount { get; set; }

    public string Status { get; set; } = string.Empty;

    public string EmploymentType { get; set; } = string.Empty;

    public string? LocationCity { get; set; }

    public string? LocationCountry { get; set; }

    public bool IsRemote { get; set; }

    public bool IsHybrid { get; set; }

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public string Currency { get; set; } = "LKR";

    public int ExperienceMinYears { get; set; }

    public int ExperienceMaxYears { get; set; }

    public int VacancyCount { get; set; }

    public DateTime? PostedAt { get; set; }

    public DateTime? ClosingDate { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public DateTime? ClosedAtUtc { get; set; }

    public List<JobSkillRequirementDto> RequiredSkills { get; set; }
        = new();

    public List<JobSkillRequirementDto> PreferredSkills { get; set; }
        = new();
}
