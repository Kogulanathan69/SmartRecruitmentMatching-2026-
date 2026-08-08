namespace NexHire.Application.DTOs.Resume;

public class ResumeTemplateResponseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PreviewImageUrl { get; set; }
    public bool IsAtsFriendly { get; set; }
}
