namespace NexHire.Application.DTOs.ApplicationStatus;

public class ApplicationStatusResultDto
{
    public Guid ApplicationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StatusUpdatedAtUtc { get; set; }
    public bool Changed { get; set; }
}
