namespace NexHire.Application.DTOs.Resume;

public class ResumeResponseDto
{
    public Guid Id { get; set; }
    public string ResumeName { get; set; } = string.Empty;
    public Guid? TemplateId { get; set; }
    public string? TemplateName { get; set; }
    public string? CareerObjective { get; set; }
    public List<string> Languages { get; set; } = new();
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsGenerated { get; set; }
    public int CompletenessScore { get; set; }
    public string QualityRating { get; set; } = string.Empty;
    public List<string> MissingSections { get; set; } = new();
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
