namespace NexHire.Application.DTOs.ApplicationStatus;

public class UpdateApplicationStatusRequestDto
{
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
