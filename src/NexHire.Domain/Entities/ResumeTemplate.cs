namespace NexHire.Domain.Entities;

public class ResumeTemplate
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TemplateUrl { get; set; } = string.Empty;
    public string? PreviewImageUrl { get; set; }
    public bool IsAtsFriendly { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
}
