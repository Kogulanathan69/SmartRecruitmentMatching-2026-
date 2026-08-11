namespace NexHire.Application.DTOs.Job;

public sealed class CreateJobDto
{
    public Guid CompanyId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Responsibilities { get; set; } = string.Empty;

    public string EducationRequirement { get; set; } = string.Empty;

    public int MinimumEducationLevel { get; set; }

    public string? RequiredCertifications { get; set; }

    public int TargetProjectCount { get; set; }

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

    public int VacancyCount { get; set; } = 1;

    public DateTime? ClosingDate { get; set; }

    public List<JobSkillRequirementDto> RequiredSkills { get; set; }
        = new();

    public List<JobSkillRequirementDto> PreferredSkills { get; set; }
        = new();
}
