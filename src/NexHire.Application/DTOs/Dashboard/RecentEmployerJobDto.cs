namespace NexHire.Application.DTOs.Dashboard;

public class RecentEmployerJobDto
{
    public Guid JobId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int VacancyCount { get; set; }
    public int ApplicationCount { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? ClosingDate { get; set; }
}
