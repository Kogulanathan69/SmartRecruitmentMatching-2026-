namespace NexHire.Domain.Entities;

public class Resume
{
    public Guid Id { get; set; }
    public Guid JobSeekerProfileId { get; set; }
    public JobSeekerProfile JobSeekerProfile { get; set; } = null!;

    public Guid? ResumeTemplateId { get; set; }
    public ResumeTemplate? ResumeTemplate { get; set; }

    public string ResumeName { get; set; } = string.Empty;
    public string? CareerObjective { get; set; }
    public string? Languages { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }

    public int CompletenessScore { get; set; }
    public string QualityRating { get; set; } = "Incomplete";
    public string? MissingSections { get; set; }
    public string? GeneratedHtml { get; set; }
    public bool IsGenerated { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
