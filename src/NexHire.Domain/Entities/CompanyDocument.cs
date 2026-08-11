namespace NexHire.Domain.Entities;

public class CompanyDocument
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public bool IsReviewed { get; set; }
    public bool IsAccepted { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
