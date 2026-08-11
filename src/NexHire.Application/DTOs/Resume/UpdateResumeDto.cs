namespace NexHire.Application.DTOs.Resume;

public class UpdateResumeDto
{
    public string ResumeName { get; set; } = string.Empty;
    public Guid? TemplateId { get; set; }
    public string? CareerObjective { get; set; }
    public List<string> Languages { get; set; } = new();
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public bool IsPrimary { get; set; }
}
