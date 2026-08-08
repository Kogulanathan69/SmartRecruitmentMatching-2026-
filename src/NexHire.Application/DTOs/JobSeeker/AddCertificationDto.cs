namespace NexHire.Application.DTOs.JobSeeker;

public class AddCertificationDto
{
    public string Name { get; set; } = string.Empty;
    public string? IssuingOrganization { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? CredentialUrl { get; set; }
}
