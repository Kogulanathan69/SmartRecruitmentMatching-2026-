namespace NexHire.Application.DTOs.Application;

public sealed class ApplicationResponseDto
{
    public Guid ApplicationId { get; set; }

    public Guid CandidateId { get; set; }

    public string CandidateName { get; set; } = string.Empty;

    public Guid JobId { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public Guid CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime AppliedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
