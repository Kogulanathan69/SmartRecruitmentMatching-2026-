namespace NexHire.Application.DTOs.Application;

public sealed class UpdateApplicationStatusDto
{
    public string Status { get; set; } = string.Empty;

    public string? Reason { get; set; }
}
