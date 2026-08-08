namespace NexHire.Application.DTOs.JobSeeker;

public class AddProjectDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TechStack { get; set; }
    public string? ProjectUrl { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
