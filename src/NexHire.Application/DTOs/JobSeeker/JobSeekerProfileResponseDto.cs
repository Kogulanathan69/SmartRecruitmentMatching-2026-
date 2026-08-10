namespace NexHire.Application.DTOs.JobSeeker;

public class JobSeekerProfileResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal? ExpectedSalaryMin { get; set; }
    public decimal? ExpectedSalaryMax { get; set; }
    public bool IsProfilePublic { get; set; }
    public bool IsOpenToWork { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<EducationResponseDto> Educations { get; set; } = new();
    public List<ExperienceResponseDto> Experiences { get; set; } = new();
    public List<CandidateSkillResponseDto> Skills { get; set; } = new();
    public List<ProjectResponseDto> Projects { get; set; } = new();
    public List<CertificationResponseDto> Certifications { get; set; } = new();
}

public class PublicJobSeekerProfileResponseDto
{
    public Guid Id { get; set; }
    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public int YearsOfExperience { get; set; }
    public bool IsOpenToWork { get; set; }
    public List<EducationResponseDto> Educations { get; set; } = new();
    public List<ExperienceResponseDto> Experiences { get; set; } = new();
    public List<CandidateSkillResponseDto> Skills { get; set; } = new();
    public List<ProjectResponseDto> Projects { get; set; } = new();
    public List<CertificationResponseDto> Certifications { get; set; } = new();
}

public class EducationResponseDto
{
    public Guid Id { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? GradeOrGpa { get; set; }
}

public class ExperienceResponseDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }
}

public class CandidateSkillResponseDto
{
    public Guid Id { get; set; }
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int ProficiencyLevel { get; set; }
    public int YearsOfExperience { get; set; }
}

public class ProjectResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TechStack { get; set; }
    public string? ProjectUrl { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CertificationResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IssuingOrganization { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? CredentialUrl { get; set; }
}
