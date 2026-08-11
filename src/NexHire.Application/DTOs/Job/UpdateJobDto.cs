namespace NexHire.Application.DTOs.Job;

public sealed class UpdateJobDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Responsibilities { get; set; }

    public string? EducationRequirement { get; set; }

    public int? MinimumEducationLevel { get; set; }

    public string? RequiredCertifications { get; set; }

    public int? TargetProjectCount { get; set; }

    public string? EmploymentType { get; set; }

    public string? LocationCity { get; set; }

    public string? LocationCountry { get; set; }

    public bool? IsRemote { get; set; }

    public bool? IsHybrid { get; set; }

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public string? Currency { get; set; }

    public int? ExperienceMinYears { get; set; }

    public int? ExperienceMaxYears { get; set; }

    public int? VacancyCount { get; set; }

    public DateTime? ClosingDate { get; set; }

    // Null means keep the current list.
    // Empty list means clear the current list.
    public List<JobSkillRequirementDto>? RequiredSkills { get; set; }

    public List<JobSkillRequirementDto>? PreferredSkills { get; set; }
}
