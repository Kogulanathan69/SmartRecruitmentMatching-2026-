namespace NexHire.Application.DTOs.Dashboard;

public class RecentApplicationDto
{
    public Guid ApplicationId { get; set; }
    public Guid JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedAtUtc { get; set; }
}
