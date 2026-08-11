using NexHire.Domain.Entities;

namespace NexHire.Application.DTOs.Company;

public class CompanyResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? OfficialEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public string? RegisteredAddress { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? CompanySize { get; set; }
    public string? LogoUrl { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TrustScore { get; set; }
    public string TrustLevel { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsDomainMatched { get; set; }
    public bool IsAddressVerified { get; set; }
    public string VerificationStatus { get; set; } = "NotSubmitted";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IReadOnlyList<CompanyDocumentResponseDto> Documents { get; set; } = Array.Empty<CompanyDocumentResponseDto>();
}

public class CompanyDocumentResponseDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public bool IsReviewed { get; set; }
    public bool IsAccepted { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime UploadedAt { get; set; }
}
