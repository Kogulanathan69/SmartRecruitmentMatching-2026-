using NexHire.Domain.Enums;

namespace NexHire.Domain.Entities;

public class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Responsibilities { get; set; } = string.Empty;

    public string EducationRequirement { get; set; } = string.Empty;

    // Ordered education level used by the deterministic matching engine.
    // 0 means the job has no minimum education requirement.
    public int MinimumEducationLevel { get; set; }

    // Optional certification names required/preferred for this vacancy.
    // Stored as a simple delimited value for the current university-project scope.
    public string? RequiredCertifications { get; set; }

    // Number of relevant projects expected for a full Projects category score.
    // 0 means the vacancy has no project-experience target.
    public int TargetProjectCount { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Draft;

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

    public DateTime? PostedAt { get; set; }

    public DateTime? ClosingDate { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public DateTime? ClosedAtUtc { get; set; }

    public ICollection<JobRequiredSkill> RequiredSkills { get; set; }
        = new List<JobRequiredSkill>();

    public ICollection<JobPreferredSkill> PreferredSkills { get; set; }
        = new List<JobPreferredSkill>();
}